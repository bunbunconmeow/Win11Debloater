using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;


namespace SecVerseLHE.Helper
{
    internal class TrustHelper
    {
        private static readonly string[] TrustedPublishers =
        {
            // --- Betriebssystem / Plattform / große OEMs ---
            "Microsoft Corporation",
            "Microsoft Windows",
            "Microsoft Windows Hardware Compatibility Publisher",
            "Apple Inc.",
            "Apple Computer, Inc.",
            "Intel Corporation",
            "NVIDIA Corporation",
            "Advanced Micro Devices, Inc.",
            "AMD Inc.",
            "Realtek Semiconductor Corp.",
            "Qualcomm Atheros, Inc.",
            "Broadcom Inc.",
            "HP Inc.",
            "Hewlett-Packard Company",
            "Dell Inc.",
            "Lenovo",
            "ASUSTeK Computer Inc.",
            "ASUS",
            "Acer Incorporated",
            "Toshiba Corporation",
            "Samsung Electronics Co., Ltd.",
            "LG Electronics Inc.",
            "Fujitsu Technology Solutions",
            "SecVers",

            // --- Browser & WebView ---
            "Google LLC",
            "Google Inc.",
            "Mozilla Corporation",
            "Opera Software",
            "Opera Software AS",
            "Vivaldi Technologies AS",
            "Brave Software, Inc",
            "Brave Software Inc",
            "Yandex LLC",

            // --- Chat / VoIP / Collaboration ---
            "Discord Inc.",
            "Slack Technologies, LLC",
            "Zoom Video Communications, Inc.",
            "Skype Communications S.A.",
            "Telegram FZ-LLC",
            "WhatsApp LLC",
            "Signal Messenger LLC",
            "TeamSpeak Systems GmbH",
            "Mumble VoIP",
            "Cisco Systems, Inc.",
            "Cisco Webex LLC",
            "TeamViewer GmbH",
            "AnyDesk Software GmbH",

            // --- Office / Productivity ---
            "Adobe Inc.",
            "Adobe Systems Incorporated",
            "Adobe Systems, Inc.",
            "The Document Foundation",   // LibreOffice
            "Foxit Software Incorporated",
            "SAP SE",

            // --- Game-Launcher / Gaming ---
            "Valve Corporation",                // Steam
            "Epic Games, Inc.",
            "Electronic Arts Inc.",
            "EA Swiss Sarl",
            "Blizzard Entertainment, Inc.",
            "Activision Blizzard, Inc.",
            "Riot Games, Inc.",
            "Ubisoft Entertainment",
            "Ubisoft Entertainment SA",
            "Rockstar Games, Inc.",
            "Take-Two Interactive Software, Inc.",
            "CD PROJEKT S.A.",
            "GOG sp. z o.o.",
            "Wargaming Group Limited",
            "Hi-Rez Studios, Inc.",
            "Garena Online Pte Ltd",

            // --- Dev-Tools / IDEs / Plattformen ---
            "JetBrains s.r.o.",
            "JetBrains s.r.o",
            "The Qt Company Ltd",
            "Docker Inc.",
            "GitHub, Inc.",
            "Atlassian Pty Ltd",
            "Node.js Foundation",
            "Python Software Foundation",
            "Eclipse Foundation, Inc.",

            // --- Virtualisierung / Cloud / DB ---
            "VMware, Inc.",
            "Oracle Corporation",
            "Oracle America, Inc.",
            "Citrix Systems, Inc.",

            // --- Security / Backup / Utilities (große Player) ---
            "Cisco Systems, Inc.",
            "Kaspersky Lab",
            "Kaspersky Lab ZAO",
            "Bitdefender SRL",
            "Avast Software s.r.o.",
            "AVG Technologies CZ, s.r.o.",
            "ESET, spol. s r.o.",
            "Sophos Ltd",
            "McAfee, LLC",
            "F-Secure Corporation",
            "Acronis International GmbH",
            "Piriform Ltd",        // CCleaner (gehört zu Avast)
            "WinRAR GmbH",
            "7-Zip",
            "Irfan Skiljan",       // IrfanView
            "VideoLAN",            // VLC

            // --- Launcher / Stores (Non-Gaming) ---
            "Amazon.com Services LLC",
            "Amazon Web Services, Inc.",
            "Dropbox, Inc.",
            "Box, Inc.",
            "Google LLC",          // (bewusst doppelt, stört nicht)
            "Mega Limited",

            // --- Sonstiges ---
            "Logitech Inc.",
            "Logitech Europe S.A.",
            "Razer Inc.",
            "Corsair Memory, Inc.",
            "Elgato Systems",
        };


        public static bool IsTrustedSignedFile(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                    return false;

                var cert = new X509Certificate2(X509Certificate.CreateFromSignedFile(filePath));
                string publisher = cert.GetNameInfo(X509NameType.SimpleName, false) ?? string.Empty;

                foreach (var trusted in TrustedPublishers)
                {
                    if (publisher.IndexOf(trusted, StringComparison.OrdinalIgnoreCase) >= 0)
                        return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}
