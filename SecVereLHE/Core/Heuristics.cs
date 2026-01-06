using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace SecVerseLHE.Core
{
    internal class Heuristics
    {
        private readonly List<string> _riskyPaths;
        private readonly HashSet<string> _whitelist;

        public Heuristics()
        {
            _whitelist = new HashSet<string>();
            _riskyPaths = new List<string>();

            string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            _riskyPaths.Add(Path.Combine(userProfile, "Downloads").ToLower());
            _riskyPaths.Add(Path.GetTempPath().ToLower());
            _riskyPaths.Add(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).ToLower());       // AppData\Roaming
            _riskyPaths.Add(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData).ToLower());  // AppData\Local
            _riskyPaths.Add(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData).ToLower()); // C:\ProgramData
            _riskyPaths.Add(Path.Combine(userProfile, "AppData", "LocalLow").ToLower());                           // AppData\LocalLow
            _riskyPaths.Add(Environment.GetFolderPath(Environment.SpecialFolder.Startup).ToLower());               // Startup User
            _riskyPaths.Add(Environment.GetFolderPath(Environment.SpecialFolder.CommonStartup).ToLower());         // Startup Global
        }

        public bool IsTrusted(string path)
        {
            return _whitelist.Contains(path);
        }

        public void AddToWhitelist(string path)
        {
            if (!_whitelist.Contains(path)) _whitelist.Add(path);
        }

        public bool IsThreat(string path)
        {
            if (string.IsNullOrEmpty(path)) return false;

            string lowerPath = path.ToLower();

            bool isSuspiciousLocation = _riskyPaths.Any(rp => lowerPath.StartsWith(rp)) || IsRootSuspicious(lowerPath);

            if (!isSuspiciousLocation) return false;

            if (HasValidSignature(path))
            {
                AddToWhitelist(path);
                return false;
            }

            return true;
        }

        private bool IsRootSuspicious(string path)
        {
            string root = Path.GetPathRoot(path);
            if (root == null) return false;
            return Path.GetDirectoryName(path) == root.TrimEnd('\\');
        }

        private static readonly string[] JavaExeNames =
        {
    "java.exe",
    "javaw.exe",
    "javaws.exe"
};

        private bool IsJavaInterpreter(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
                return false;

            var fileName = Path.GetFileName(path)?.ToLowerInvariant();
            if (fileName == null)
                return false;

            if (!Array.Exists(JavaExeNames, n => n.Equals(fileName, StringComparison.OrdinalIgnoreCase)))
                return false;

            try
            {
                var info = FileVersionInfo.GetVersionInfo(path);
                var product = (info.ProductName ?? string.Empty).ToLowerInvariant();
                var company = (info.CompanyName ?? string.Empty).ToLowerInvariant();
                if (product.Contains("java") || company.Contains("oracle") || company.Contains("openjdk") ||
                    company.Contains("adoptopenjdk") || company.Contains("temurin"))
                {
                    return true;
                }
            }
            catch {  }

            return false;
        }

        private bool HasValidSignature(string path)
        {
            try
            {
                if (IsJavaInterpreter(path))
                    return false;

                using (var cert = new System.Security.Cryptography.X509Certificates.X509Certificate2(
                           System.Security.Cryptography.X509Certificates.X509Certificate2.CreateFromSignedFile(path)))
                {
                    return !string.IsNullOrEmpty(cert.Subject);
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
