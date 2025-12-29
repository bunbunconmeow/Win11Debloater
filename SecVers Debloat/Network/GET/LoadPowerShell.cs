using SecVers_Debloat.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SecVers_Debloat.Network.GET
{
    internal class LoadPowerShell
    {
        // Download PS1 from URL and load into memory 
        // Useragende is set to SecVers-Debloat
        public static string DownloadAndLoadPS1(string url)
        {
            var webClient = new System.Net.WebClient();

            webClient.Headers.Add("user-agent", "SecVers-Debloat");
            webClient.Encoding = Encoding.UTF8;
            if(Cache.Global.CommunityRepository.useProxy && !string.IsNullOrEmpty(Cache.Global.CommunityRepository.proxyAddress))
            {
                try
                {
                    webClient.Proxy = new System.Net.WebProxy(Cache.Global.CommunityRepository.proxyAddress);
                }
                catch (Exception ex)
                {
                    throw new Exception("Error setting proxy: " + ex.Message);
                }
            }

         

            try
            {
                string ps1Content = webClient.DownloadString(url);
                return ps1Content;
            }
            catch (Exception ex)
            {
                throw new Exception("Error downloading PowerShell script: " + ex.Message);
            }

        }
    }
}
