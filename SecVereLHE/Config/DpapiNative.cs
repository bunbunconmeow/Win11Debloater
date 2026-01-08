using System;
using System.Runtime.InteropServices;
using System.Text;

namespace SecVerseLHE.Config
{
    internal static class DpapiNative
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct DATA_BLOB
        {
            public int cbData;
            public IntPtr pbData;
        }

        [DllImport("crypt32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool CryptProtectData(
            ref DATA_BLOB pDataIn,
            string szDataDescr,
            IntPtr pOptionalEntropy,
            IntPtr pvReserved,
            IntPtr pPromptStruct,
            int dwFlags,
            out DATA_BLOB pDataOut);

        [DllImport("crypt32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool CryptUnprotectData(
            ref DATA_BLOB pDataIn,
            StringBuilder ppszDataDescr,
            IntPtr pOptionalEntropy,
            IntPtr pvReserved,
            IntPtr pPromptStruct,
            int dwFlags,
            out DATA_BLOB pDataOut);

        public static byte[] Protect(byte[] data, byte[] entropy)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            DATA_BLOB inBlob = new DATA_BLOB { cbData = data.Length, pbData = Marshal.AllocHGlobal(data.Length) };
            Marshal.Copy(data, 0, inBlob.pbData, data.Length);

            IntPtr entropyPtr = IntPtr.Zero;
            if (entropy != null && entropy.Length > 0)
            {
                entropyPtr = Marshal.AllocHGlobal(entropy.Length);
                Marshal.Copy(entropy, 0, entropyPtr, entropy.Length);
            }

            try
            {
                DATA_BLOB outBlob;
                bool ok = CryptProtectData(ref inBlob, null, entropyPtr, IntPtr.Zero, IntPtr.Zero, 0, out outBlob);
                if (!ok) throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());

                byte[] result = new byte[outBlob.cbData];
                Marshal.Copy(outBlob.pbData, result, 0, outBlob.cbData);
                Marshal.FreeHGlobal(outBlob.pbData);
                return result;
            }
            finally
            {
                Marshal.FreeHGlobal(inBlob.pbData);
                if (entropyPtr != IntPtr.Zero) Marshal.FreeHGlobal(entropyPtr);
            }
        }

        public static byte[] Unprotect(byte[] data, byte[] entropy)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            DATA_BLOB inBlob = new DATA_BLOB { cbData = data.Length, pbData = Marshal.AllocHGlobal(data.Length) };
            Marshal.Copy(data, 0, inBlob.pbData, data.Length);

            IntPtr entropyPtr = IntPtr.Zero;
            if (entropy != null && entropy.Length > 0)
            {
                entropyPtr = Marshal.AllocHGlobal(entropy.Length);
                Marshal.Copy(entropy, 0, entropyPtr, entropy.Length);
            }

            try
            {
                DATA_BLOB outBlob;
                bool ok = CryptUnprotectData(ref inBlob, null, entropyPtr, IntPtr.Zero, IntPtr.Zero, 0, out outBlob);
                if (!ok) throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());

                byte[] result = new byte[outBlob.cbData];
                Marshal.Copy(outBlob.pbData, result, 0, outBlob.cbData);
                Marshal.FreeHGlobal(outBlob.pbData);
                return result;
            }
            finally
            {
                Marshal.FreeHGlobal(inBlob.pbData);
                if (entropyPtr != IntPtr.Zero) Marshal.FreeHGlobal(entropyPtr);
            }
        }
    }
}