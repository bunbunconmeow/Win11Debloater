using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SecVerseLHE.Core
{
    internal class BsodProtection
    {
        [DllImport("ntdll.dll", SetLastError = true)]
        private static extern void RtlSetProcessIsCritical(bool bNew, bool[] pbOld, bool bNeedScsb);
        public static void SetCritical(bool enable)
        {
            try
            {
                Process.EnterDebugMode();
                RtlSetProcessIsCritical(enable, null, false);
            }
            catch { }
        }
    }
}
