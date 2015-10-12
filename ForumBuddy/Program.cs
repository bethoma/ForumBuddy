using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.IO;

namespace ForumBuddy
{
    class InputParamaters
    {
        public string ForumUrl;
        public string WebhookUrl;
    }
    class Program
    {
        public static List<IThreadInfoSource> s_ThreadInfoSources;
        public static List<IThreadInfoSink> s_ThreadInfoSinks;
        static void Main(string[] args)
        {
            var command = Args.Configuration.Configure<InputParamaters>().CreateAndBind(args);

            Console.WriteLine();
            Console.WriteLine("====");
            Console.WriteLine("Forum Buddy");
            Console.WriteLine("====");
            Console.WriteLine();


            Console.WriteLine("Initializing Sources");
            s_ThreadInfoSources = new List<IThreadInfoSource>();

            IThreadInfoSource msdnSource = new MsdnThreadInfoSource(new Uri(command.ForumUrl));
            msdnSource.OnNewThread += MsdnSource_OnNewThread;
            msdnSource.StartThreadListener();
            s_ThreadInfoSources.Add(msdnSource);

            Console.WriteLine("\nInitializing Sinks");
            s_ThreadInfoSinks = new List<IThreadInfoSink>();
            s_ThreadInfoSinks.Add(new SlackThreadInfoSink(new Uri(command.WebhookUrl)));

            var allThreads = new List<ThreadInfo>();

            foreach(var source in s_ThreadInfoSources)
            {
                allThreads.AddRange(source.Initialize());
            }

            foreach(var sink in s_ThreadInfoSinks)
            {
                foreach(var thread in allThreads)
                {
                    Console.WriteLine();
                    Console.WriteLine(String.Format("Thread Info\n\nID: {0}\nTitle: {1}\nSummary: {2}\nLink: {3}", thread.Id, thread.Title, thread.Summary, thread.Link));
                    Console.WriteLine();
                    sink.PostThreadInfo(thread);
                }
            }

            Console.WriteLine("Waiting for Press any key to exit");
            Console.ReadKey(false);
        }

        private static void MsdnSource_OnNewThread(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
