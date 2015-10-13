using System;
using System.Xml.Linq;

namespace ForumBuddy
{
    public enum ThreadPriority
    {
        Normal,
        High
    }
    public class ThreadInfo
    {
        public Guid Id;
        public string Title;
        public string Summary;
        public Uri Link;
        public ThreadPriority Priority;

        public ThreadInfo()
        {
            this.Link = new Uri("http://null.Uri");
        }

        public static ThreadInfo FromXml(XElement xml)
        {
            var threadInfo = new ThreadInfo();

            threadInfo.Id = Guid.Parse(xml.Element("Id").Value);
            threadInfo.Title = xml.Element("Title").Value;
            threadInfo.Summary = xml.Element("Summary").Value;
            threadInfo.Link = new Uri(xml.Element("Link").Value);
            threadInfo.Priority = (ThreadPriority)Enum.Parse(typeof(ThreadPriority), xml.Element("Priority").Value);

            return threadInfo;
        }

        public XElement ToXml()
        {
            var xml = new XElement("ThreadInfo");

            xml.Add(new XElement("Id", this.Id.ToString()));
            xml.Add(new XElement("Title", this.Title));
            xml.Add(new XElement("Summary", this.Summary));
            xml.Add(new XElement("Link", this.Link.ToString()));
            xml.Add(new XElement("Priority", this.Priority.ToString()));

            return xml;
        }
    }
}
