using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecVerseLHE.Config
{
    internal class Config
    {
        internal static class UserData
        {
            internal static string SoftwareToken { get; set; } = string.Empty;
            internal static string UserName { get; set; } = string.Empty;
            internal static string CurrentIPAddress { get; set; } = string.Empty;
        }

        internal static class SoftwareSettings
        {
            internal static bool InterCeptorGuard { get; set; }
            internal static bool IG_Whitelist { get; set; }
            internal static bool OfficeProtection { get; set; }
            internal static bool RuntimeGuard { get; set; }
            internal static bool RiskAssessmentEngine { get; set; }
        }
    }
}
