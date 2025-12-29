using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecVers_Debloat.Patches.Hardening
{
    public class AttackSurfaceReduction
    {
        // Block executable content from email client and webmail
        public void BlockExecutableContentFromEmailWebmail()
        {
            SetASRRule("BE9BA2D9-53EA-4CDC-84E5-9B1EEEE46550", "1");
        }

        // Block all Office applications from creating child processes
        public void BlockOfficeChildProcesses()
        {
            SetASRRule("D4F940AB-401B-4EFC-AADC-AD5F3C50688A", "1");
        }

        // Block Office applications from creating executable content
        public void BlockOfficeExecutableContent()
        {
            SetASRRule("3B576869-A4EC-4529-8536-B80A7769E899", "1");
        }

        // Block Office applications from injecting code into other processes
        public void BlockOfficeInjection()
        {
            SetASRRule("75668C1F-73B5-4CF0-BB93-3ECF5CB7CC84", "1");
        }

        // Block JavaScript or VBScript from launching downloaded executable content
        public void BlockScriptExecutableDownload()
        {
            SetASRRule("D3E037E1-3EB8-44C8-A917-57927947596D", "1");
        }

        // Block execution of potentially obfuscated scripts
        public void BlockObfuscatedScripts()
        {
            SetASRRule("5BEB7EFE-FD9A-4556-801D-275E5FFC04CC", "1");
        }

        // Block Win32 API calls from Office macros
        public void BlockWin32APICalls()
        {
            SetASRRule("92E97FA1-2EDF-4476-BDD6-9DD0B4DDDC7B", "1");
        }

        // Block credential stealing from Windows local security authority subsystem
        public void BlockCredentialStealing()
        {
            SetASRRule("9E6C4E1F-7D60-472F-BA1A-A39EF669E4B2", "1");
        }

        // Block untrusted and unsigned processes from USB
        public void BlockUntrustedUSBProcesses()
        {
            SetASRRule("B2B3F03D-6A65-4F7B-A9C7-1C7EF74A9BA4", "1");
        }

        // Block Adobe Reader from creating child processes
        public void BlockAdobeReaderChildProcesses()
        {
            SetASRRule("7674BA52-37EB-4A4F-A9A1-F0F9A1619A2C", "1");
        }

        // Block persistence through WMI event subscription
        public void BlockWMIPersistence()
        {
            SetASRRule("E6DB77E5-3DF2-4CF1-B95A-636979351E5B", "1");
        }

        // Block process creations from PSExec and WMI commands
        public void BlockPSExecWMICommands()
        {
            SetASRRule("D1E49AAC-8F56-4280-B9BA-993A6D77406C", "1");
        }

        // Block executable files from running unless they meet prevalence, age, or trusted list criteria
        public void BlockUntrustedExecutables()
        {
            SetASRRule("01443614-CD74-433A-B99E-2ECDC07BFC25", "1");
        }

        // Use advanced protection against ransomware
        public void EnableAdvancedRansomwareProtection()
        {
            SetASRRule("C1DB55AB-C21A-4637-BB3F-A12568109D35", "1");
        }

        // Block Office communication apps from creating child processes
        public void BlockOfficeCommChildProcesses()
        {
            SetASRRule("26190899-1602-49E8-8B27-EB1D0A1CE869", "1");
        }

        // Enable ALL ASR Rules (Maximum Protection)
        public void EnableAllASRRules()
        {
            BlockExecutableContentFromEmailWebmail();
            BlockOfficeChildProcesses();
            BlockOfficeExecutableContent();
            BlockOfficeInjection();
            BlockScriptExecutableDownload();
            BlockObfuscatedScripts();
            BlockWin32APICalls();
            BlockCredentialStealing();
            BlockUntrustedUSBProcesses();
            BlockAdobeReaderChildProcesses();
            BlockWMIPersistence();
            BlockPSExecWMICommands();
            BlockUntrustedExecutables();
            EnableAdvancedRansomwareProtection();
            BlockOfficeCommChildProcesses();
        }

        // Disable all ASR Rules
        public void DisableAllASRRules()
        {
            ExecutePowerShell("Set-MpPreference -AttackSurfaceReductionRules_Ids @() -AttackSurfaceReductionRules_Actions @()");
        }

        private void SetASRRule(string ruleId, string action)
        {
            ExecutePowerShell($"Add-MpPreference -AttackSurfaceReductionRules_Ids {ruleId} -AttackSurfaceReductionRules_Actions {action}");
        }

        private void ExecutePowerShell(string command)
        {
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{command}\"",
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
