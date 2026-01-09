using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecVerseLHE.Helper
{
    internal class InitHelper
    {
        internal static void Initialize()
        {
            Process process = Process.GetCurrentProcess();
            process.PriorityClass = ProcessPriorityClass.RealTime;

            foreach (ProcessThread thread in process.Threads)
            {
                try
                {
                    thread.PriorityLevel = ThreadPriorityLevel.TimeCritical;
                }
                catch
                {
                    
                }
            }
        }
    }
}
