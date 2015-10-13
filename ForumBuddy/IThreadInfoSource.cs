using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ForumBuddy
{
    public delegate void NewThreadHandler(object sender, ThreadInfo thread);

    interface IThreadInfoSource
    {
        List<ThreadInfo> Initialize(XElement configFragment);
        void StartThreadListener();
        void StopThreadListener();

        event NewThreadHandler OnNewThread;
    }
}
