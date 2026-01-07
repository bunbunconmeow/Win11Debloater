using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace SecVers_Debloat.Helper
{
    internal class SecVersCertificateInstaller
    {
        private static readonly Uri CertUri = new Uri("https://api.secvers.org/v1/downloads/helpers/cert");

        public static async Task<bool> DownloadAndInstallAsync(bool installForAllUsers = false)
        {
            byte[] certBytes;
            using (var http = new HttpClient())
            {
                certBytes = await http.GetByteArrayAsync(CertUri)
                                      .ConfigureAwait(false);
            }

            var cert = new X509Certificate2(certBytes);
            var location = installForAllUsers ? StoreLocation.LocalMachine : StoreLocation.CurrentUser;
            InstallIntoStore(cert, StoreName.Root, location);
            InstallIntoStore(cert, StoreName.TrustedPublisher, location);

            return true;
        }

        private static void InstallIntoStore(X509Certificate2 cert, StoreName storeName, StoreLocation storeLocation)
        {
            using (var store = new X509Store(storeName, storeLocation))
            {
                store.Open(OpenFlags.ReadWrite);
                var existing = store.Certificates.Find(
                    X509FindType.FindByThumbprint,
                    cert.Thumbprint,
                    validOnly: false);

                if (existing == null || existing.Count == 0)
                {
                    store.Add(cert);
                }

                store.Close();
            }
        }
    }
}
