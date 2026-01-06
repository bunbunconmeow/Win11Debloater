using SecVerseLHE.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SecVerseLHE.Core
{
    internal class FileSuspicionResult
    {
        public bool IsSuspicious;
        public bool LooksPackedOrEncrypted;
        public bool LooksLikeDropper;
        public double Entropy;
        public int Score;
        public string[] Reasons;
    }

    internal class SuspiciousFileScanner
    {
        // public entry point, wrapped in try/catch
        public static void StartTry(string filePath, TrayManager tray)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                    return;

                var result = Analyze(filePath);
                if (!result.IsSuspicious)
                    return;

                try
                {
                    tray?.ShowAlert(
                        "Suspicious file detected",
                        "We found a suspicious file: " + Path.GetFileName(filePath));
                }
                catch
                {
                    // tray errors are not critical
                }

                ShowPopup(filePath, result, tray);
            }
            catch
            {
                // swallow everything here, we don't want to crash protection
            }
        }

        private static FileSuspicionResult Analyze(string filePath)
        {
            var result = new FileSuspicionResult();
            var reasons = new List<string>();

            byte[] data;

            try
            {
                data = ReadSample(filePath, 2 * 1024 * 1024); // max 2 MB
            }
            catch
            {
                // if we can't read the file, we just return not suspicious
                result.IsSuspicious = false;
                result.Score = 0;
                result.Reasons = Array.Empty<string>();
                return result;
            }

            if (data == null || data.Length == 0)
            {
                result.IsSuspicious = false;
                result.Score = 0;
                result.Reasons = Array.Empty<string>();
                return result;
            }

            // entropy
            result.Entropy = CalculateEntropy(data);
            if (result.Entropy > 7.2)
            {
                result.LooksPackedOrEncrypted = true;
                result.Score += 2;
                reasons.Add("High entropy (looks packed/encrypted/obfuscated).");
            }

            // string stats (how many readable strings etc.)
            var stringStats = GetStringStats(data);
            if (stringStats.TotalStrings < 20 && result.Entropy > 7.0)
            {
                result.Score += 2;
                reasons.Add("Very few readable strings + high entropy (heavily obfuscated).");
            }
            else if (stringStats.TotalStrings < 5 && result.Entropy > 6.5)
            {
                result.Score += 1;
                reasons.Add("Almost no readable strings.");
            }

            if (stringStats.AvgLength > 20 && stringStats.TotalStrings > 0)
            {
                result.Score += 1;
                reasons.Add("Unusual string pattern (few but long strings).");
            }

            // dropper indicators (API names, temp path, etc.)
            var dropperHits = CountDropperIndicators(data, out var dropperStrings);
            if (dropperHits > 0)
            {
                result.Score += 1;
                reasons.Add("Found dropper-related strings: " + string.Join(", ", dropperStrings));
            }
            if (dropperHits >= 3)
            {
                result.LooksLikeDropper = true;
                result.Score += 2;
                reasons.Add("Multiple dropper indicators present.");
            }

            // decrypt / crypto / obfuscation indicators
            var decryptHits = CountDecryptIndicators(data, out var decryptStrings);
            if (decryptHits > 0)
            {
                result.Score += 1;
                reasons.Add("Found decryption/encoding related code: " + string.Join(", ", decryptStrings));
            }
            if (decryptHits >= 3 && result.Entropy > 7.0)
            {
                result.Score += 2;
                reasons.Add("High entropy + multiple decryption patterns (likely encrypted payload).");
            }

            // some coarse decision
            if (result.Score >= 3 || result.LooksPackedOrEncrypted || result.LooksLikeDropper)
            {
                result.IsSuspicious = true;
            }
            else
            {
                result.IsSuspicious = false;
            }

            result.Reasons = reasons.ToArray();
            return result;
        }

        private static void ShowPopup(string filePath, FileSuspicionResult result, TrayManager tray)
        {
            var sb = new StringBuilder();

            sb.AppendLine("We found a suspicious file:");
            sb.AppendLine(filePath);
            sb.AppendLine();
            sb.AppendLine("It looks like it may be encrypted, obfuscated or act as a dropper.");
            sb.AppendLine();
            sb.AppendLine("Entropy: " + result.Entropy.ToString("F2") + " (0 = very structured, 8 = very random)");
            sb.AppendLine("Score: " + result.Score);

            if (result.Reasons != null && result.Reasons.Length > 0)
            {
                sb.AppendLine();
                sb.AppendLine("Heuristic reasons:");
                foreach (var r in result.Reasons)
                {
                    sb.AppendLine(" - " + r);
                }
            }

            sb.AppendLine();
            sb.AppendLine("This is only a heuristic check and does NOT replace a real antivirus scan.");
            sb.AppendLine();
            sb.AppendLine("Actions:");
            sb.AppendLine("Yes    = Open file location");
            sb.AppendLine("No     = Delete file");
            sb.AppendLine("Cancel = Ignore");

            var dlgResult = MessageBox.Show(
                sb.ToString(),
                "Suspicious file detected",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Warning);

            try
            {
                if (dlgResult == DialogResult.Yes)
                {
                    // open file location in explorer
                    try
                    {
                        System.Diagnostics.Process.Start("explorer.exe", "/select,\"" + filePath + "\"");
                    }
                    catch { }
                }
                else if (dlgResult == DialogResult.No)
                {
                    // delete file (best-effort)
                    try
                    {
                        File.Delete(filePath);
                        tray?.ShowAlert("File deleted", "Suspicious file was deleted:\n" + Path.GetFileName(filePath));
                    }
                    catch
                    {
                        tray?.ShowAlert("Delete failed", "Could not delete suspicious file.");
                    }
                }
                else
                {
                    // ignore
                    tray?.ShowAlert("Ignored", "Suspicious file was ignored by user.");
                }
            }
            catch
            {
                // don't let UI actions kill the app
            }
        }

        private static byte[] ReadSample(string filePath, int maxBytes)
        {
            var fi = new FileInfo(filePath);
            int toRead = (int)Math.Min(fi.Length, maxBytes);

            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                var buffer = new byte[toRead];
                int read = fs.Read(buffer, 0, toRead);
                if (read < toRead)
                {
                    Array.Resize(ref buffer, read);
                }
                return buffer;
            }
        }

        private static double CalculateEntropy(byte[] data)
        {
            if (data == null || data.Length == 0)
                return 0.0;

            double[] counts = new double[256];
            double len = data.Length;

            for (int i = 0; i < data.Length; i++)
            {
                counts[data[i]]++;
            }

            double entropy = 0.0;
            for (int i = 0; i < 256; i++)
            {
                if (counts[i] == 0) continue;
                double p = counts[i] / len;
                entropy -= p * Math.Log(p, 2);
            }

            return entropy;
        }

        private struct StringStats
        {
            public int TotalStrings;
            public double AvgLength;
        }

        private static StringStats GetStringStats(byte[] data, int minLen = 4)
        {
            int count = 0;
            int totalLen = 0;

            int i = 0;
            int len = data.Length;

            while (i < len)
            {
                // printable ASCII only
                if (data[i] >= 32 && data[i] <= 126)
                {
                    int start = i;
                    while (i < len && data[i] >= 32 && data[i] <= 126)
                        i++;

                    int sLen = i - start;
                    if (sLen >= minLen)
                    {
                        count++;
                        totalLen += sLen;
                    }
                }
                else
                {
                    i++;
                }
            }

            var stats = new StringStats();
            stats.TotalStrings = count;
            stats.AvgLength = count == 0 ? 0.0 : (double)totalLen / count;
            return stats;
        }

        private static int CountDropperIndicators(byte[] data, out List<string> found)
        {
            string content;
            try
            {
                content = Encoding.ASCII.GetString(data);
            }
            catch
            {
                content = string.Empty;
            }

            string[] indicators =
            {
                // file / temp / startup
                "CreateFileA","CreateFileW","WriteFile",
                "WriteAllBytes","File.WriteAllBytes","FileStream",
                "GetTempPath","Path.GetTempPath","%TEMP%","%APPDATA%",
                "Startup","StartUp","Run\\","RunOnce",
                "Software\\Microsoft\\Windows\\CurrentVersion\\Run",

                // process / execution
                "CreateProcessA","CreateProcessW",
                "ShellExecuteA","ShellExecuteW",
                "WinExec","Process.Start","CreateRemoteThread",

                // network / download
                "URLDownloadToFile","InternetOpen","InternetReadFile",
                "WinHttpOpen","WinHttpSendRequest",
                "WebClient","HttpClient","DownloadData","DownloadFile"
            };

            found = new List<string>();
            int hits = 0;

            if (string.IsNullOrEmpty(content))
                return 0;

            foreach (var s in indicators)
            {
                if (content.IndexOf(s, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    hits++;
                    found.Add(s);
                }
            }

            return hits;
        }

        private static int CountDecryptIndicators(byte[] data, out List<string> found)
        {
            string content;
            try
            {
                content = Encoding.ASCII.GetString(data);
            }
            catch
            {
                content = string.Empty;
            }

            string[] indicators =
            {
                // base64 / encoding
                "Convert.FromBase64String",
                "Encoding.UTF8.GetString",
                "Encoding.ASCII.GetString",
                "Encoding.Unicode.GetString",

                // crypto
                "AesManaged","RijndaelManaged","ICryptoTransform","CryptoStream",
                "AesCryptoServiceProvider","DESCryptoServiceProvider",
                "CreateEncryptor","CreateDecryptor",

                // XOR 
                " xor ",              
                "XorDecrypt",
                "XorDecode",
                "XorEncode",
                "xorKey",
                "decryptXor",
                "xor_decrypt",
                "xor_decode",

                // typische C#: array[i] ^= key / 0x??
                "bytes[i] ^=",
                "buffer[i] ^=",
                "data[i] ^=",
                "array[i] ^=",
                "text[i] ^=",

                // mit Hex-Key
                "^= 0x",
                " ^ 0x",
                " ^ 0xFF",
                " ^ 0xAA",
                " ^ 0xCC",
                " ^ 0x13",
                " ^ 0x37",
                " ^ 0x42",
                "(byte)(bytes[i] ^",
                "(byte)(buffer[i] ^",
                "(byte)(data[i] ^",
                "(byte)(array[i] ^",
                "for (int i = 0; i < bytes.Length; i++)",
                "for (int i = 0; i < buffer.Length; i++)",
                "for (int i = 0; i < data.Length; i++)",
                "for (int i = 0; i < array.Length; i++)"
            };

            found = new List<string>();
            int hits = 0;

            if (string.IsNullOrEmpty(content))
                return 0;

            foreach (var s in indicators)
            {
                if (content.IndexOf(s, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    hits++;
                    found.Add(s);
                }
            }

            return hits;
        }
    }
}
