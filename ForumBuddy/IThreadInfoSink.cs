using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ForumBuddy
{
    interface IThreadInfoSink
    {
        void Initialize(XElement configFragment);
        void PostThreadInfo(ThreadInfo threadInfo);
    }
}
