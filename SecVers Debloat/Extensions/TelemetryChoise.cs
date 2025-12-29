using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecVers_Debloat.Extensions
{
    public class TelemetryChoiceEventArgs : EventArgs
    {
        public bool TelemetryEnabled { get; }

        public TelemetryChoiceEventArgs(bool enabled)
        {
            TelemetryEnabled = enabled;
        }
    }
}
