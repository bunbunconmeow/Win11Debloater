using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecVers_Debloat.Extensions
{
    public static class ProcessExtensions
    {
        public static Task WaitForExitAsync(this Process process)
        {
            return Task.Run(() => process.WaitForExit());
        }
    }
}
