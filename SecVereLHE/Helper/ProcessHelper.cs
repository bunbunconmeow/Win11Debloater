using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SecVerseLHE.Helper
{
    internal class ProcessHelper
    {
        [DllImport("ntdll.dll", SetLastError = true)]
        private static extern uint NtSuspendProcess(IntPtr processHandle);
        [DllImport("ntdll.dll", SetLastError = true)]
        private static extern uint NtResumeProcess(IntPtr processHandle);

        public static Process GetProcessByPath(string filePath)
        {
            foreach (Process process in Process.GetProcesses())
            {
                try
                {
                    string processPath = process.MainModule?.FileName;
                    if (!string.IsNullOrEmpty(processPath) && processPath.Equals(filePath, StringComparison.OrdinalIgnoreCase))
                    {
                        return process;
                    }
                }
                catch (Exception)
                {

                }
            }
            return null;
        }

        public static bool SuspendProcess(Process process)
        {
            if (process == null || process.HasExited)
                return false;

            try
            {
                IntPtr handle = process.Handle;
                uint result = NtSuspendProcess(handle);
                return result == 0;
            }
            catch
            {
                return false;
            }
        }

        public static bool KillProcess(Process process)
        {
            if (process == null || process.HasExited)
                return false;

            try
            {
                process.Kill();
                process.WaitForExit(2000);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool ResumeProcess(Process process)
        {
            if (process == null || process.HasExited)
                return false;
            try
            {
                IntPtr handle = process.Handle;
                uint result = NtResumeProcess(handle);
                return result == 0;
            }
            catch
            {
                return false;
            }
        }
    }
}
