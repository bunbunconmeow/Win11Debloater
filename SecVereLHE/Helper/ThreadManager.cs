using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SecVerseLHE.Helper
{
    internal class ThreadManager : IDisposable
    {
        private sealed class WorkerEntry
        {
            public Thread Thread;
            public CancellationTokenSource TokenSource;
            public IBackgroundWorker Worker;
        }

        private readonly object _lock = new object();
        private readonly Dictionary<string, WorkerEntry> _workers = new Dictionary<string, WorkerEntry>(StringComparer.OrdinalIgnoreCase);
        private bool _disposed;

        public void StartWorker(IBackgroundWorker worker)
        {
            if (worker == null)
                throw new ArgumentNullException(nameof(worker));

            lock (_lock)
            {
                if (_disposed)
                    throw new ObjectDisposedException(nameof(ThreadManager));

                if (_workers.ContainsKey(worker.Name))
                    throw new InvalidOperationException($"Worker with name '{worker.Name}' is already running.");

                var cts = new CancellationTokenSource();
                var entry = new WorkerEntry
                {
                    TokenSource = cts,
                    Worker = worker
                };

                var thread = new Thread(() => RunWorker(entry))
                {
                    IsBackground = true,
                    Name = worker.Name
                };

                entry.Thread = thread;
                _workers.Add(worker.Name, entry);

                thread.Start();
            }
        }

        public void StopWorker(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return;

            WorkerEntry entry;

            lock (_lock)
            {
                if (_disposed)
                    return;

                if (!_workers.TryGetValue(name, out entry))
                    return;

                _workers.Remove(name);
            }

            try
            {
                entry.TokenSource.Cancel();
            }
            catch { }

            try
            {
                if (entry.Thread.IsAlive)
                    entry.Thread.Join(TimeSpan.FromSeconds(5));
            }
            catch { }

            try
            {
                entry.Worker.Dispose();
            }
            catch { }

            try
            {
                entry.TokenSource.Dispose();
            }
            catch { }
        }

        public bool IsRunning(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            lock (_lock)
            {
                if (_disposed)
                    return false;

                return _workers.ContainsKey(name);
            }
        }

        private void RunWorker(WorkerEntry entry)
        {
            try
            {
                entry.Worker.Run(entry.TokenSource.Token);
            }
            catch (OperationCanceledException)
            {
            }
            catch
            {
            }
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            List<WorkerEntry> snapshot;

            lock (_lock)
            {
                if (_disposed)
                    return;

                _disposed = true;
                snapshot = new List<WorkerEntry>(_workers.Values);
                _workers.Clear();
            }

            foreach (var entry in snapshot)
            {
                try { entry.TokenSource.Cancel(); } catch { }

                try
                {
                    if (entry.Thread.IsAlive)
                        entry.Thread.Join(TimeSpan.FromSeconds(5));
                }
                catch { }

                try { entry.Worker.Dispose(); } catch { }
                try { entry.TokenSource.Dispose(); } catch { }
            }
        }
    }
}
