using System;
using System.IO;
using System.IO.Compression;
using System.IO.Packaging;
using System.Net.Http;
using System.Threading.Tasks;

namespace SecVers_Debloat.Helper
{
    internal class ToolDownloader
    {
        public static class VolumeIdHelper
        {
            private static readonly string PackageDir = "Data/Packages/";
            private static readonly string ExePath = Path.Combine(PackageDir, "VolumeId64.exe");
            private const string DownloadUrl = "https://download.sysinternals.com/files/VolumeId.zip";

            public static async Task<string> EnsureVolumeIdExistsAsync()
            {

                if (File.Exists(ExePath))
                {
                    return ExePath;
                }

                try
                {
                    if (!Directory.Exists(PackageDir))
                    {
                        Directory.CreateDirectory(PackageDir);
                    }

                    using (HttpClient client = new HttpClient())
                    {
                        client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");

                        byte[] zipData = await client.GetByteArrayAsync(DownloadUrl);

                        using (MemoryStream ms = new MemoryStream(zipData))
                        using (ZipArchive archive = new ZipArchive(ms))
                        {
                            var entry = archive.GetEntry("Volumeid64.exe");

                            if (entry != null)
                            {

                                entry.ExtractToFile(ExePath, overwrite: true);
                            }
                            else
                            {
                                throw new FileNotFoundException("VolumeId64.exe not found.");
                            }
                        }
                    }

                    return ExePath;
                }
                catch (Exception ex)
                {
                    throw new Exception($"Download Error: {ex.Message}", ex);
                }
            }
        }
    }
}
