using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecVers_Debloat.Cache
{
    /// <summary>
    /// Provides internal configuration options for enabling or disabling specific features such as community scripts,
    /// telemetry, software installation, and registry edits.
    /// </summary>
    /// <remarks>This class is intended for internal use to manage feature flags within the application. All
    /// members are static and control global settings that affect the application's behavior. Changes to these options
    /// may impact security, privacy, or system configuration depending on which features are enabled.</remarks>
    internal static class Popup
    {
        private static bool AllowCommunityScripts { get; set; } = false;
        private static bool AllowTelemetry { get; set; } = false;
        private static bool AllowSoftwareInstallation { get; set; } = false;
        private static bool AllowRegEdits { get; set; } = false;


        #region CommunityScripts
        internal static bool Set_AllowCommunityScripts(bool allow) => AllowCommunityScripts = allow;
        internal static bool Get_AllowCommunityScripts() => AllowCommunityScripts;
        #endregion

        #region Telemetry
        internal static bool Set_AllowTelemetry(bool allow) => AllowTelemetry = allow;
        internal static bool Get_AllowTelemetry() => AllowTelemetry;
        #endregion

        #region SoftwareInstallation
        internal static bool Set_AllowSoftwareInstallation(bool allow) => AllowSoftwareInstallation = allow;
        internal static bool Get_AllowSoftwareInstallation() => AllowSoftwareInstallation;
        #endregion

        #region RegEdits
        internal static bool Set_AllowRegEdits(bool allow) => AllowRegEdits = allow;
        internal static bool Get_AllowRegEdits() => AllowRegEdits;
        #endregion
    }
}
