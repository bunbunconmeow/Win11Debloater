using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace SecVerseLHE.Core
{
    internal class Heuristics
    {
        private readonly string _downloadPath;
        private readonly string _tempPath;
        private readonly string _appDataPath;

        private readonly HashSet<string> _whitelist;

        public Heuristics()
        {
            _downloadPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads").ToLower();
            _tempPath = Path.GetTempPath().ToLower();
            _appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).ToLower();

            _whitelist = new HashSet<string>();
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

            bool suspiciousLocation =
                lowerPath.StartsWith(_downloadPath) ||
                lowerPath.Contains(_tempPath) ||
                lowerPath.Contains("\\appdata\\local\\temp") ||
                IsRootSuspicious(lowerPath);

            if (!suspiciousLocation) return false;

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

        private bool HasValidSignature(string path)
        {
            try
            {
                using (var cert = X509Certificate2.CreateFromSignedFile(path))
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
