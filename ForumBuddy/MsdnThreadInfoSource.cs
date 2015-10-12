using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ForumBuddy
{
    public class MsdnThreadInfoSource : IThreadInfoSource
    {
        public Uri forumAddress;
        public MsdnThreadInfoSource(Uri forumAddress)
        {
            this.forumAddress = forumAddress;
        }

        public event EventHandler OnNewThread;
        public List<ThreadInfo> Initialize()
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

            HtmlNode threadNode = node.SelectNodes(".//li[@class='threadblock']").FirstOrDefault();

            var info = ThreadInfo.FromHtmlNode(threadNode);

            Guid latestThreadId = Guid.Empty;
            string latestThreadIdString = string.Empty;

            try
            {
                latestThreadIdString = Properties.Settings.Default["LatestThread"].ToString();
            }
            catch (SettingsPropertyNotFoundException)
            {
                // ignore
            }

            if (string.Empty != latestThreadIdString && Guid.TryParse(latestThreadIdString, out latestThreadId))
            {
                if (Guid.Empty != latestThreadId || latestThreadId.Equals(info.Id))
                {
                    Console.WriteLine("Latest thread has not changed since last query");
                }
                else
                {
                    Properties.Settings.Default["LatestThread"] = info.Id.ToString();
                    Properties.Settings.Default.Save();
                    threads.Add(info);
                }
            }
            else
            {
                Properties.Settings.Default["LatestThread"] = info.Id.ToString();
                Properties.Settings.Default.Save();
                threads.Add(info);
            }

            return threads;
        }

        public void StartThreadListener()
        {
            Console.WriteLine("MsdnThreadInfoSource listening for threads on forum:");
            Console.WriteLine(this.forumAddress);
        }

        public void StopThreadListener()
        {
            throw new NotImplementedException();
        }
    }
}
