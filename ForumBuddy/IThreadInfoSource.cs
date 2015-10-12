using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForumBuddy
{
    interface IThreadInfoSource
    {
        List<ThreadInfo> Initialize();
        void StartThreadListener();
        void StopThreadListener();

        event EventHandler OnNewThread;
    }
}
