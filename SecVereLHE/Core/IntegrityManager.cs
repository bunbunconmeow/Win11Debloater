using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Forms;

namespace SecVerseLHE.Core
{
    internal class IntegrityManager
    {
        private const string AppName = "SecVerseLHE"; 
        private const string ExpectedPublisher = "CN=SecVers";

        public static void EnsureIntegrityAndStartup()
        {
            ManageAutostart();
            ValidateSignature();
        }

        private static void ManageAutostart()
        {
            try
            {
                string exePath = Process.GetCurrentProcess().MainModule.FileName;
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true))
                {
                    if (key.GetValue(AppName) == null)
                    {
                        key.SetValue(AppName, exePath);
                        Debug.WriteLine("Added application to Autostart.");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Autostart registration failed: {ex.Message}");
            }
        }

        private static void ValidateSignature()
        {
            string exePath = Process.GetCurrentProcess().MainModule.FileName;
            bool isSecure = false;

            try
            {
                X509Certificate cert = X509Certificate.CreateFromSignedFile(exePath);
                if (cert.Subject.Contains(ExpectedPublisher))
                {
                    isSecure = true;
                }
            }
            catch
            {
                isSecure = false;
            }

            if (!isSecure)
            {
                string message = "SECURITY ALERT:\n" +
                                 "The integrity of this application could not be verified.\n" +
                                 $"The publisher does not match '{ExpectedPublisher}' or the signature is missing.\n\n" +
                                 "Is this a custom/developer build?";

                var result = MessageBox.Show(message, "Integrity Violation", MessageBoxButtons.YesNo, MessageBoxIcon.Error);

                if (result == DialogResult.No)
                {
                   Process.GetCurrentProcess().Kill();
                }
            }
        }
    }
}
