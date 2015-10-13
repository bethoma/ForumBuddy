using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.IO;
using System.Xml.Linq;

namespace ForumBuddy
{

    class InputParamaters
    {
        public string ForumUrl;
        public string WebhookUrl;
    }
    class Program
    {
        private const string ConfigFileName = "ForumBuddy.config";

        public static List<IThreadInfoSource> s_ThreadInfoSources;
        public static List<IThreadInfoSink> s_ThreadInfoSinks;
        static void Main(string[] args)
        {
            var command = Args.Configuration.Configure<InputParamaters>().CreateAndBind(args);

            // Welcome text
            Console.WriteLine();
            Console.WriteLine("====");
            Console.WriteLine("Forum Buddy");
            Console.WriteLine("====");
            Console.WriteLine();

            // Get config
            if (!File.Exists(ConfigFileName))
            {
                Console.WriteLine("Cannot find conifg file:" + ConfigFileName + "\n Exiting.");                
                return;
            }

            var xmlConfig = XElement.Load(ConfigFileName);
            var sourcesList = xmlConfig.Elements("Sources").FirstOrDefault();
            var sourceConfigFragments = sourcesList.Elements("Source");

            Console.WriteLine("Initializing Sources");
            s_ThreadInfoSources = new List<IThreadInfoSource>();
            var startupThreads = new List<ThreadInfo>();

            foreach (var sourceConfigFragment in sourceConfigFragments)
            {
                var sourceTypeAttribute = sourceConfigFragment.Attributes().Where(a => a.Name == "type").FirstOrDefault();

                if (null != sourceTypeAttribute)
                {
                    var type = Type.GetType(sourceTypeAttribute.Value);
                    IThreadInfoSource source = Activator.CreateInstance(type) as IThreadInfoSource;
                    if (null != source)
                    {
                        startupThreads.AddRange(source.Initialize(sourceConfigFragment.Element("SourceConfig")));
                        source.OnNewThread += OnNewThread;
                        source.StartThreadListener();
                        s_ThreadInfoSources.Add(source);
                    }
                }
            }

            Console.WriteLine("\nInitializing Sinks");
            s_ThreadInfoSinks = new List<IThreadInfoSink>();
            var sinksConfig = xmlConfig.Elements("Sinks").FirstOrDefault();
            var sinksConfigFragments = sinksConfig.Elements("Sink");

            foreach (var sinksConfigFragment in sinksConfigFragments)
            {
                var sinkTypeAttribute = sinksConfigFragment.Attributes().Where(a => a.Name == "type").FirstOrDefault();

                if (null != sinkTypeAttribute)
                {
                    var type = Type.GetType(sinkTypeAttribute.Value);
                    IThreadInfoSink sink = Activator.CreateInstance(type) as IThreadInfoSink;
                    if (null != sink)
                    {
                        sink.Initialize(sinksConfigFragment.Element("SinkConfig"));
                        s_ThreadInfoSinks.Add(sink);
                    }
                }
            }

            foreach (var thread in startupThreads)
            {
                OnNewThread(null, thread);
            }

            Console.WriteLine("Waiting for Press any key to exit");
            Console.ReadKey(false);
        }

        private static void OnNewThread(object sender, ThreadInfo thread)
        {
            foreach (var sink in s_ThreadInfoSinks)
            {
                Console.WriteLine();
                Console.WriteLine(String.Format("Thread Info\n\nID: {0}\nTitle: {1}\nSummary: {2}\nLink: {3}", thread.Id, thread.Title, thread.Summary, thread.Link));
                Console.WriteLine();
                sink.PostThreadInfo(thread);
            }
        }
    }
}
