using SecVerseLHE.Core;
using System;
using System.Text;


namespace SecVerseLHE.Helper
{
    internal class PathResolver
    {
        [ThreadStatic]
        private static StringBuilder _buffer;
        public static string GetPathFromPid(int pid)
        {
            IntPtr handle = IntPtr.Zero;
            try
            {
              
                handle = NativeMethods.OpenProcess(NativeMethods.PROCESS_QUERY_LIMITED_INFORMATION, false, pid);

                if (handle == IntPtr.Zero) return null; 
                if (_buffer == null) _buffer = new StringBuilder(1024);

                int capacity = _buffer.Capacity;
                if (NativeMethods.QueryFullProcessImageName(handle, 0, _buffer, ref capacity))
                {
                    return _buffer.ToString();
                }
            }
            finally
            {
                if (handle != IntPtr.Zero) NativeMethods.CloseHandle(handle);
            }
            return null;
        }
    }
}
