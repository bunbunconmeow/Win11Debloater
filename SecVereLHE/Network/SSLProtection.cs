using SecVerseLHE.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecVerseLHE.Network
{
    // @ToDo: SSL Pinning for MITM Prevention
    internal class SSLProtection : IDisposable
    {
        private TrayManager _tray;
        private string _certProvider = "Cloudflare";
        private readonly string _testURL = "https://www.cloudflare.com/cdn-cgi/trace"; 

        internal SSLProtection(TrayManager tray) { 
        
        }


        internal bool ValidateCertificate()
        {
        }

        public void Dispose()
        {
            this._tray = null;
            _certProvider = null;
            GC.SuppressFinalize(this);
        }
    }
}
