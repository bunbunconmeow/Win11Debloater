using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management; // Für WMI-Abfragen (CPU, GPU, RAM, etc.)
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace SecVers_Debloat.Network
{
    internal class Telemetry
    {
        private const string TelemetryUrl = "https://dein-server.de/api/telemetry";
        private const string UserAgent = "SecVersDebloater/1.0";

        public static async Task<bool> SendTelemetryDataAsync()
        {
            try
            {
                var systemInfo = GetSystemInformation();
                var client = new RestClient(TelemetryUrl);
                var request = new RestRequest("", Method.Post);
                request.AddHeader("User-Agent", UserAgent);
                request.AddHeader("Content-Type", "application/json");
                request.AddJsonBody(systemInfo);
                var response = await client.ExecuteAsync(request);
                return response.IsSuccessful;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Telemetrie-Fehler: {ex.Message}");
                return false;
            }
        }

        private static Dictionary<string, object> GetSystemInformation()
        {
            var systemInfo = new Dictionary<string, object>
            {
                { "Timestamp", DateTime.UtcNow.ToString("o") },
                { "Username", Environment.UserName },
                { "MachineName", Environment.MachineName },
                { "OSVersion", Environment.OSVersion.ToString() },
                { "HWID", GetHardwareId() },
                { "CPU", GetCpuInfo() },
                { "TotalRAM_GB", Math.Round((double)GetTotalRamInBytes() / (1024 * 1024 * 1024), 2) },
                { "GPU", GetGpuInfo() },
                { "FreeSpace_C_GB", Math.Round((double)GetFreeSpaceOnDrive("C") / (1024 * 1024 * 1024), 2) },
            };

            return systemInfo;
        }

        private static string GetHardwareId()
        {
            try
            {
                string macAddress = NetworkInterface
                    .GetAllNetworkInterfaces()
                    .FirstOrDefault(nic => nic.OperationalStatus == OperationalStatus.Up)?
                    .GetPhysicalAddress()
                    .ToString() ?? "Unknown";
                string volumeSerial = new System.IO.DriveInfo("C")
                    .RootDirectory
                    .ToString()
                    .Substring(0, 2);
                return $"{macAddress}-{volumeSerial}";
            }
            catch
            {
                return "Unknown-HWID";
            }
        }

        private static Dictionary<string, string> GetCpuInfo()
        {
            var cpuInfo = new Dictionary<string, string>();

            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Processor"))
                {
                    foreach (var obj in searcher.Get())
                    {
                        cpuInfo["Name"] = obj["Name"]?.ToString() ?? "Unknown";
                        cpuInfo["Cores"] = obj["NumberOfCores"]?.ToString() ?? "0";
                        cpuInfo["LogicalProcessors"] = obj["NumberOfLogicalProcessors"]?.ToString() ?? "0";
                        break;
                    }
                }
            }
            catch
            {
                cpuInfo["Name"] = "Unknown";
                cpuInfo["Cores"] = "0";
                cpuInfo["LogicalProcessors"] = "0";
            }

            return cpuInfo;
        }
        private static long GetTotalRamInBytes()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT TotalPhysicalMemory FROM Win32_ComputerSystem"))
                {
                    foreach (var obj in searcher.Get())
                    {
                        return Convert.ToInt64(obj["TotalPhysicalMemory"]);
                    }
                }
            }
            catch
            {
                return 0;
            }
            return 0;
        }

        private static Dictionary<string, string> GetGpuInfo()
        {
            var gpuInfo = new Dictionary<string, string> { { "Name", "Unknown" }, { "VRAM_MB", "0" } };

            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController"))
                {
                    foreach (var obj in searcher.Get())
                    {
                        gpuInfo["Name"] = obj["Name"]?.ToString() ?? "Unknown";
                        gpuInfo["VRAM_MB"] = (Convert.ToUInt64(obj["AdapterRAM"]) / (1024 * 1024)).ToString();
                        break;
                    }
                }
            } catch {}

            return gpuInfo;
        }

        private static long GetFreeSpaceOnDrive(string driveLetter)
        {
            try
            {
                var drive = new System.IO.DriveInfo(driveLetter);
                if (drive.IsReady)
                {
                    return drive.AvailableFreeSpace;
                }
            }
            catch
            {
                return 0;
            }
            return 0;
        }
    }
}
