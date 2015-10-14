using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Xml.Linq;

namespace ForumBuddy
{
    public class SlackThreadInfoSink : IThreadInfoSink
    {
        private string sinkName;
        private Uri webhookUrl;
        private string channel;
        private string userName;
        private Uri iconUri;
        private string messageBody;
        private List<ThreadPriority> priorities;

        public void Initialize(XElement configFragment)
        {
            this.sinkName = configFragment.Attribute("name").Value;
            this.webhookUrl = new Uri(configFragment.Element("WebhookUrl").Value);
            this.channel = configFragment.Element("Channel").Value;
            this.userName = configFragment.Element("UserName").Value;
            this.iconUri = new Uri(configFragment.Element("IconUrl").Value);
            this.messageBody = configFragment.Element("MessageBody").Value.Replace("\\n", "\n");
            var priorities = configFragment.Element("ThreadPriorities").Value;
            this.priorities = new List<ThreadPriority>();
            foreach (var priority in priorities.Split(';'))
            {
                this.priorities.Add((ThreadPriority)Enum.Parse(typeof(ThreadPriority), priority));
            }
        }

        public void PostThreadInfo(ThreadInfo threadInfo)
        {
            if (!this.priorities.Contains(threadInfo.Priority))
                return;

            Console.WriteLine();
            Console.WriteLine("Posting thread to: " + this.sinkName);
            Console.WriteLine(String.Format("ID: {0}\nTitle: {1}\nSummary: {2}\nLink: {3}", 
                threadInfo.Id, 
                threadInfo.Title, 
                threadInfo.Summary, 
                threadInfo.Link));

            Console.WriteLine();

            var webRequest = (HttpWebRequest)WebRequest.Create(this.webhookUrl);
            webRequest.ContentType = "text/json";
            webRequest.Method = "POST";

            using (var streamWriter = new StreamWriter(webRequest.GetRequestStream()))
            {
                string botMessage = string.Format(this.messageBody, threadInfo.Title, threadInfo.Id, threadInfo.Summary, threadInfo.Link);
                string json = "{\"text\":\"" + botMessage + "\"," +
                              "\"username\":\"" + this.userName + "\"," +
                              "\"channel\":\"" + this.channel + "\"," +
                              "\"icon_url\":\"" + this.iconUri.ToString() + "\"}";

                streamWriter.Write(json);
                streamWriter.Flush();
                streamWriter.Close();
            }

            var response = (HttpWebResponse)webRequest.GetResponse();
            using (var streamReader = new StreamReader(response.GetResponseStream()))
            {
                if (HttpStatusCode.OK == response.StatusCode)
                {
                    Console.WriteLine("Thread info posted to Slack");
                }
            }
        }
    }
}
