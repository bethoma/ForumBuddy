using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Xml.Linq;

namespace ForumBuddy
{
    public class MsdnThreadInfoSource : IThreadInfoSource
    {
        private string sourceName;
        private Uri forumAddress;
        private int pollingIntervalSeconds;
        private int maxNewThreads;
        private int highPriorityThreshold;
        private List<ThreadPriority> priorities;
        private Timer pollingTimer;
        private ThreadInfo currentHeadThread;

        public event NewThreadHandler OnNewThread;

        public List<ThreadInfo> Initialize(XElement configFragment)
        {
            this.currentHeadThread = new ThreadInfo();
            this.sourceName = configFragment.Attribute("name").Value;
            this.forumAddress = new Uri(configFragment.Element("ForumUrl").Value);
            this.pollingIntervalSeconds = int.Parse(configFragment.Element("PollingIntervalSeconds").Value);
            this.maxNewThreads = int.Parse(configFragment.Element("MaxNewThreads").Value);
            this.highPriorityThreshold = int.Parse(configFragment.Element("HighPriorityThreshold").Value);
            var priorities = configFragment.Element("ThreadPriorities").Value;
            this.priorities = new List<ThreadPriority>();
            foreach (var priority in priorities.Split(';'))
            {
                this.priorities.Add((ThreadPriority)Enum.Parse(typeof(ThreadPriority), priority));
            }

            this.loadState();

            return new List<ThreadInfo>();
        }

        public void StartThreadListener()
        {
            if (null != this.pollingTimer)
                return;

            this.pollingTimer = new Timer(((state) => 
            {
                var threads = this.getNewThreads();
                if (threads.Any())
                {
                    this.OnNewThread(this, threads[0]);
                }

            }), null, 0, this.pollingIntervalSeconds * 1000); 

            Console.WriteLine("MsdnThreadInfoSource listening for threads on forum:");
            Console.WriteLine(this.forumAddress);
        }

        public void StopThreadListener()
        {
            if (null == this.pollingTimer)
                return;

            this.pollingTimer.Dispose();
            this.pollingTimer = null;
        }

        private List<ThreadInfo> getThreads()
        {
            var threads = new List<ThreadInfo>();
            string forumHtml = string.Empty;

            using (WebClient client = new WebClient())
            {
                forumHtml = client.DownloadString(this.forumAddress);
            }

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(forumHtml);
            HtmlNode node = doc.DocumentNode.SelectNodes("//ul[@id='threadList']").FirstOrDefault();

            foreach(var threadNode in node.SelectNodes(".//li[@class='threadblock']"))
            {
                threads.Add(fromHtmlNode(threadNode));
            }

            return threads;
        }

        private List<ThreadInfo> getNewThreads()
        {
            var threads = new List<ThreadInfo>();

            var allThreads = getThreads();

            var threadsToReturn = (allThreads.Count < this.maxNewThreads) ? allThreads.Count : this.maxNewThreads;

            for (int i = 0; i < threadsToReturn; i++)
            {
                var threadInfo = allThreads[i];

                if (threadInfo.PostedDate <= currentHeadThread.PostedDate)
                    break;

                if (this.priorities.Contains(threadInfo.Priority))
                {
                    threads.Add(threadInfo);
                }
            }

            if (threads.Any())
            {
                currentHeadThread = threads[0];
                this.saveState();
            }
            else
            {
                Console.WriteLine("Latest thread has not changed since last query");
            }

            return threads;
        }

        private ThreadInfo fromHtmlNode(HtmlNode node)
        {
            ThreadInfo threadInfo = null;

            Guid threadId;

            if (Guid.TryParse(node.Attributes["data-threadId"].Value, out threadId))
            {
                var titleIdString = "threadTitle_" + threadId.ToString();
                HtmlNode titleNode = node.SelectNodes(".//a[@id='" + titleIdString + "']").FirstOrDefault();
                HtmlNode summaryNode = node.SelectNodes(".//div[@class='threadSummary']").FirstOrDefault();
                HtmlNode lastPostNode = node.SelectNodes(".//span[@class='lastpost']").FirstOrDefault();
                HtmlNode timeNode = lastPostNode.SelectNodes(".//span[@class='timeago smallgreytext']")[1];
                DateTime postedDate = DateTime.Now;
                var timeText = timeNode.InnerText;
                if (timeText.Contains("ago"))
                {
                    var timeComponents = timeText.Split(' ');
                    var date = DateTime.Now;
                    if (timeComponents.Count() == 5)
                    {
                        int hours = int.Parse(timeComponents[0]);
                        date = date.AddHours(hours * -1);
                        int minutes = int.Parse(timeComponents[2]);
                        date = date.AddMinutes(minutes * -1);
                    }
                    else
                    {
                        int minutes = int.Parse(timeComponents[0]);
                        date = date.AddMinutes(minutes - minutes * -1);
                    }

                    postedDate = new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, 0);

                }
                else
                {
                    postedDate = DateTime.Parse(timeText);
                }

                var votesIdString = "threadVoteText_" + threadId.ToString();
                HtmlNode votesNode = node.SelectNodes(".//div[@id='" + votesIdString + "']").FirstOrDefault();
                var votes = int.Parse(votesNode.InnerText.Split(' ')[0]);

                threadInfo = new ThreadInfo()
                {
                    Id = threadId,
                    Title = titleNode.InnerText,
                    PostedDate = postedDate,
                    Summary = summaryNode.InnerText,
                    Link = new Uri(titleNode.Attributes["href"].Value),
                    Priority = (this.highPriorityThreshold != -1 && votes >= this.highPriorityThreshold) ? ThreadPriority.High : ThreadPriority.Normal
                };
            }

            return threadInfo;
        }

        private void loadState()
        {
            var sourceStateFileName = this.sourceName + ".state";
            if (!File.Exists(sourceStateFileName))
                this.saveState();

            var state = XElement.Load(sourceStateFileName);
            this.currentHeadThread = ThreadInfo.FromXml(state.Element("ThreadInfo"));
        }

        private void saveState()
        {
            var sourceStateFileName = this.sourceName + ".state";
            var stateFile = new XElement("MsdnThreadInfoSourceState");
            stateFile.Add(this.currentHeadThread.ToXml());
            stateFile.Save(sourceStateFileName);
        }
    }
}
