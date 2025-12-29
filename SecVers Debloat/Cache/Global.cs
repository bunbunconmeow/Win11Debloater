using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace SecVers_Debloat.Cache
{
    internal static class Global
    {

        internal static class Patches
        {

        }

        internal static class Softwares
        {

        }

        internal static class Regestry
        {

        }

        internal static class Hardening
        {
        }

        internal static class CommunityRepository
        {
            internal static Dictionary<string, Dictionary<string, bool>> Scripts = new Dictionary<string, Dictionary<string, bool>>();
            internal static bool useProxy = false;
            internal static string proxyAddress = "";
        }
    }
}
