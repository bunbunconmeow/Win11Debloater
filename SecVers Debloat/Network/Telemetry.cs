using Hardware.Info;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace SecVers_Debloat.Network
{
    internal class Telemetry
    {
        private const string TelemetryUrl = "https://dein-server.de/api/telemetry";
        private const string InstallationUrl = "https://dein-server.de/api/installation";
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
            } catch { return false; }
        }


        public static async Task<bool> SendInstallationAsync()
        {
            try
            {
                var installationJSON = CreateInstallationJSON();
                var client = new RestClient(InstallationUrl);
                var request = new RestRequest("", Method.Post);
                request.AddHeader("User-Agent", UserAgent);
                request.AddHeader("Content-Type", "application/json");
                request.AddJsonBody(installationJSON);
                var response = await client.ExecuteAsync(request);
                return response.IsSuccessful;
            } catch { return false; }
            
        }

        private static Dictionary<string, object> GetSystemInformation()
        {
            var hardwareInfo = new HardwareInfo();
            hardwareInfo.RefreshAll();
            var systemInfo = new Dictionary<string, object>
            {
                { "Timestamp", DateTime.UtcNow.ToString("o") },
                { "Username", Environment.UserName },
                { "MachineName", Environment.MachineName },
                { "OSVersion", hardwareInfo.OperatingSystem.Name },
                { "HWID", GetHardwareId() },
                { "CPU", hardwareInfo.CpuList.Count },
            };

            return systemInfo;
        }

        private static Dictionary<string, object> CreateInstallationJSON()
        {
            var hardwareInfo = new HardwareInfo();
            hardwareInfo.RefreshAll();
            var systemInfo = new Dictionary<string, object>
            {
                { "HWID", GetHardwareId() },
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
    }
}
