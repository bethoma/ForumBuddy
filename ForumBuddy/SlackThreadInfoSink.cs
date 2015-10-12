using System;
using System.IO;
using System.Net;

namespace ForumBuddy
{
    public class SlackThreadInfoSink : IThreadInfoSink
    {
        private Uri webhookUrl;

        public SlackThreadInfoSink(Uri webhookUrl)
        {
            this.webhookUrl = webhookUrl;
        }
        public void PostThreadInfo(ThreadInfo threadInfo)
        {
            var webRequest = (HttpWebRequest)WebRequest.Create(this.webhookUrl);
            webRequest.ContentType = "text/json";
            webRequest.Method = "POST";

            using (var streamWriter = new StreamWriter(webRequest.GetRequestStream()))
            {
                string json = "{\"text\":\"\\n*New Forum Post*\\n\\n*Title:* " + threadInfo.Title + "\\n*Id:* "
                                                                        + threadInfo.Id + "\\n*Summary:* "
                                                                            + threadInfo.Summary + "\\n*Link:* <"
                                                                            + threadInfo.Link.ToString() + ">\"," +
                              "\"username\":\"Forum Buddy\"," +
                              "\"channel\":\"#msdnforum\"," +
                              "\"icon_url\":\"http://i.imgur.com/sqzQ1el.jpg\"}";

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
