using System;
using System.Threading;

namespace SecVerseLHE.Helper
{
    internal class IBackgroundWorker : IDisposable
    {
       public string Name { get; }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public IBackgroundWorker() { }

        public void Run(CancellationToken token) { }
    }
}
