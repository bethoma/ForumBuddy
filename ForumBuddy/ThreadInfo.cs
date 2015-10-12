using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForumBuddy
{
    public class ThreadInfo
    {
        public Guid Id;
        public string Title;
        public string Summary;
        public Uri Link;

        public static ThreadInfo FromHtmlNode(HtmlNode node)
        {
            ThreadInfo threadInfo = null;

            Guid threadId;

            if (Guid.TryParse(node.Attributes["data-threadId"].Value, out threadId))
            {
                var titleIdString = "threadTitle_" + threadId.ToString();
                HtmlNode titleNode = node.SelectNodes(".//a[@id='"+ titleIdString + "']").FirstOrDefault();
                HtmlNode summaryNode = node.SelectNodes(".//div[@class='threadSummary']").FirstOrDefault();

                threadInfo = new ThreadInfo()
                {
                    Id = threadId,
                    Title = titleNode.InnerText,
                    Summary = summaryNode.InnerText,
                    Link = new Uri(titleNode.Attributes["href"].Value)
                };
            }

                return threadInfo;
        }
    }
}
