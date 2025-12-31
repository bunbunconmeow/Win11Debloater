using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;
namespace SecVers_Debloat.Patches.Hardening
{
    public class AccountSecurity
    {
        // Disable Guest Account
        public void DisableGuestAccount()
        {
            ExecuteCommand("net.exe", "user guest /active:no");
        }

        // Disable Administrator Account
        public void DisableAdministratorAccount()
        {
            ExecuteCommand("net.exe", "user administrator /active:no");
        }

        // Rename Administrator Account
        public void RenameAdministratorAccount(string newName)
        {
            ExecuteCommand("wmic.exe", $"useraccount where name='Administrator' rename {newName}");
        }

        // Enable Password Complexity Requirements
        public void EnablePasswordComplexity()
        {
            ExecuteCommand("net.exe", "accounts /minpwlen:12");
            SetSecurityPolicy("PasswordComplexity", "1");
        }

        // Set Password History
        public void SetPasswordHistory(int count)
        {
            SetSecurityPolicy("PasswordHistorySize", count.ToString());
        }

        // Set Maximum Password Age
        public void SetMaxPasswordAge(int days)
        {
            ExecuteCommand("net.exe", $"accounts /maxpwage:{days}");
        }

        // Set Minimum Password Age
        public void SetMinPasswordAge(int days)
        {
            ExecuteCommand("net.exe", $"accounts /minpwage:{days}");
        }

        // Enable Account Lockout Policy
        public void EnableAccountLockout(int threshold, int duration)
        {
            ExecuteCommand("net.exe", $"accounts /lockoutthreshold:{threshold}");
            ExecuteCommand("net.exe", $"accounts /lockoutduration:{duration}");
        }

        // Disable Anonymous SID Enumeration
        public void DisableAnonymousSIDEnumeration()
        {
            SetRegistryValue(@"SYSTEM\CurrentControlSet\Control\Lsa",
                "RestrictAnonymousSAM", 1, RegistryValueKind.DWord);
            SetRegistryValue(@"SYSTEM\CurrentControlSet\Control\Lsa",
                "RestrictAnonymous", 1, RegistryValueKind.DWord);
        }

        // Disable LAN Manager Hash Storage
        public void DisableLMHashStorage()
        {
            SetRegistryValue(@"SYSTEM\CurrentControlSet\Control\Lsa",
                "NoLMHash", 1, RegistryValueKind.DWord);
        }

        // Configure LAN Manager Authentication Level (NTLMv2 only)
        public void SetNTLMv2Only()
        {
            SetRegistryValue(@"SYSTEM\CurrentControlSet\Control\Lsa",
                "LmCompatibilityLevel", 5, RegistryValueKind.DWord);
        }

        // Disable Storage of Credentials
        public void DisableCredentialStorage()
        {
            SetRegistryValue(@"SYSTEM\CurrentControlSet\Control\Lsa",
                "DisableDomainCreds", 1, RegistryValueKind.DWord);
        }

        // Enable User Account Control (UAC) at maximum
        public void EnableUACMaximum()
        {
            SetRegistryValue(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System",
                "ConsentPromptBehaviorAdmin", 2, RegistryValueKind.DWord);
            SetRegistryValue(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System",
                "EnableLUA", 1, RegistryValueKind.DWord);
            SetRegistryValue(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System",
                "PromptOnSecureDesktop", 1, RegistryValueKind.DWord);
        }

        // Block Microsoft Accounts
        public void BlockMicrosoftAccounts()
        {
            SetRegistryValue(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System",
                "NoConnectedUser", 3, RegistryValueKind.DWord);
        }

        // Limit Blank Password Use to Console Only
        public void LimitBlankPasswordConsoleOnly()
        {
            SetRegistryValue(@"SYSTEM\CurrentControlSet\Control\Lsa",
                "LimitBlankPasswordUse", 1, RegistryValueKind.DWord);
        }

        private void SetRegistryValue(string path, string name, object value, RegistryValueKind kind)
        {
            try
            {
                using (RegistryKey key = Registry.LocalMachine.CreateSubKey(path))
                {
                    key?.SetValue(name, value, kind);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Registry error: {ex.Message}");
            }
        }

        private void SetSecurityPolicy(string setting, string value)
        {
            string tempFile = System.IO.Path.GetTempFileName();
            ExecuteCommand("secedit.exe", $"/export /cfg {tempFile}");

            string content = System.IO.File.ReadAllText(tempFile);
            content = System.Text.RegularExpressions.Regex.Replace(
                content,
                $@"{setting}\s*=\s*\d+",
                $"{setting} = {value}");

            System.IO.File.WriteAllText(tempFile, content);
            ExecuteCommand("secedit.exe", $"/configure /db secedit.sdb /cfg {tempFile}");

            System.IO.File.Delete(tempFile);
        }

        private void ExecuteCommand(string fileName, string arguments)
        {
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                UseShellExecute = false,
                CreateNoWindow = true,
                Verb = "runas"
            };

            using (Process process = Process.Start(psi))
            {
                process?.WaitForExit();
            }
        }
    }
}
