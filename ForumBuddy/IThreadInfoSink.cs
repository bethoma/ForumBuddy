using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForumBuddy
{
    interface IThreadInfoSink
    {
        void PostThreadInfo(ThreadInfo threadInfo);
    }
}
