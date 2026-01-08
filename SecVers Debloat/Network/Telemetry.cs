using Hardware.Info;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SecVers_Debloat.Network
{
    internal class Telemetry
    {
        private const string BaseUrl = "https://api.secvers.org";
  
        
        private const string PluginName = "debloater";

        private string CurrentVersion => Assembly.GetExecutingAssembly().GetName().Version.ToString();
        private const string UserAgent = "Mozilla/5.0 (Linux; Android 10; K) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/143.0.0.0 Mobile Safari/537.36";

        private readonly RestClient _client;
        private static string _cachedHwid = null;
        public Telemetry()
        {
            var options = new RestClientOptions(BaseUrl)
            {
                UserAgent = UserAgent,
                Timeout = TimeSpan.FromSeconds(10)
            };
            _client = new RestClient(options);
        }

        public async Task<bool> SendTelemetryAsync()
        {
            try
            {
                var request = new RestRequest($"{BaseUrl}/v1/telemetry/{PluginName}", Method.Post);
                var body = new
                {
                    hwid = GetHardwareId(),
                    pluginVersion = CurrentVersion,
                    serverName = "Windows Software",
                    timestamp = DateTime.UtcNow.ToString("o"),
                    osVersion = Environment.OSVersion.ToString(),
                    userName = Environment.UserName,
                    processorCount = Environment.ProcessorCount,
                    is64Bit = Environment.Is64BitOperatingSystem
                };

                request.AddJsonBody(body);

                var response = await _client.ExecuteAsync(request);
                return response.IsSuccessful;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public async Task<(bool updateAvailable, string remoteVersion)> CheckForUpdateAsync()
        {
            try
            {
                var request = new RestRequest($"{BaseUrl}/v1/plugin/{PluginName}", Method.Get);

                var response = await _client.ExecuteAsync(request);

                if (response.IsSuccessful && !string.IsNullOrEmpty(response.Content))
                {
                    dynamic json = JsonConvert.DeserializeObject(response.Content);
                    string remoteVersion = json.version;

                    if (string.IsNullOrEmpty(remoteVersion))
                        return (false, null);
                    Version local = Version.Parse(CurrentVersion);
                    Version remote = Version.Parse(remoteVersion);

                    return (remote > local, remoteVersion);
                }
            }
            catch(Exception ex)
            {
                throw ex;
            }

            return (false, null);
        }

        private static string GetHardwareId()
        {
            if (!string.IsNullOrEmpty(_cachedHwid)) return _cachedHwid;

            try
            {
                string cpuInfo = string.Empty;
                string mbInfo = string.Empty;
                using (var searcher = new ManagementObjectSearcher("SELECT ProcessorId FROM Win32_Processor"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        cpuInfo = obj["ProcessorId"]?.ToString();
                        break;
                    }
                }
                using (var searcher = new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_BaseBoard"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        mbInfo = obj["SerialNumber"]?.ToString();
                        break;
                    }
                }
                if (string.IsNullOrEmpty(cpuInfo)) cpuInfo = "UnknownCPU";
                if (string.IsNullOrEmpty(mbInfo)) mbInfo = "UnknownMB";
                string rawId = $"{cpuInfo}-{mbInfo}";
                using (var sha = SHA256.Create())
                {
                    byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(rawId));
                    StringBuilder builder = new StringBuilder();
                    for (int i = 0; i < bytes.Length && i < 8; i++)
                    {
                        builder.Append(bytes[i].ToString("x2"));
                    }
                    _cachedHwid = builder.ToString().ToUpper();
                }
            }
            catch
            {
                _cachedHwid = "FALLBACK-HWID-" + Environment.MachineName;
            }

            return _cachedHwid;
        }
    }
}
