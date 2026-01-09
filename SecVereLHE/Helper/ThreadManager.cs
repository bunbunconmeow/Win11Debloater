using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;

namespace SecVerseLHE.Helper
{
    internal class ThreadManager : IDisposable
    {
        private readonly ConcurrentDictionary<Guid, ManagedThread> _threads;
        private readonly object _lock = new object();
        private volatile bool _disposed;
        private readonly int _maxThreads;

        private readonly Guid _securityToken;

        public ThreadManager(int maxThreads = 10)
        {
            _maxThreads = maxThreads;
            _threads = new ConcurrentDictionary<Guid, ManagedThread>();
            _securityToken = Guid.NewGuid();
        }

        public Guid RegisterThread(IManagedThreadWorker worker)
        {
            ThrowIfDisposed();

            if (worker == null)
                throw new ArgumentNullException(nameof(worker));

            lock (_lock)
            {
                if (_threads.Count >= _maxThreads)
                    throw new InvalidOperationException($"Maximum thread count ({_maxThreads}) reached.");

                var managedThread = new ManagedThread(worker, _securityToken);

                if (!_threads.TryAdd(managedThread.Id, managedThread))
                    throw new InvalidOperationException("Failed to register thread.");

                managedThread.Start();
                return managedThread.Id;
            }
        }

        public bool StopThread(Guid threadId)
        {
            ThrowIfDisposed();

            if (_threads.TryRemove(threadId, out var thread))
            {
                thread.Stop();
                thread.Dispose();
                return true;
            }
            return false;
        }
        public void StopAll()
        {
            lock (_lock)
            {
                foreach (var kvp in _threads)
                {
                    kvp.Value.Stop();
                    kvp.Value.Dispose();
                }
                _threads.Clear();
            }
        }

        public bool IsThreadRunning(Guid threadId)
        {
            return _threads.TryGetValue(threadId, out var thread) && thread.IsRunning;
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(ThreadManager));
        }

        public void Dispose()
        {
            if (_disposed) return;

            lock (_lock)
            {
                if (_disposed) return;
                _disposed = true;

                StopAll();
            }
        }
        private sealed class ManagedThread : IDisposable
        {
            private readonly IManagedThreadWorker _worker;
            private readonly Guid _securityToken;
            private readonly Thread _thread;
            private readonly CancellationTokenSource _cts;
            private volatile bool _disposed;

            public Guid Id { get; }
            public bool IsRunning => _thread.IsAlive && !_cts.IsCancellationRequested;

            public ManagedThread(IManagedThreadWorker worker, Guid securityToken)
            {
                _worker = worker;
                _securityToken = securityToken;
                _cts = new CancellationTokenSource();
                Id = Guid.NewGuid();

                _thread = new Thread(ThreadProc)
                {
                    IsBackground = true,
                    Name = $"SecVerse_{worker.GetType().Name}_{Id:N}",
                    Priority = ThreadPriority.Highest
                };
            }

            public void Start()
            {
                if (!_disposed && !_thread.IsAlive)
                {
                    _worker.Initialize(_cts.Token);
                    _thread.Start();
                }
            }

            public void Stop()
            {
                if (!_disposed)
                {
                    _cts.Cancel();
                    if (_thread.IsAlive && !_thread.Join(3000))
                    {
                        try { _thread.Interrupt(); } catch { }
                    }
                }
            }

            private void ThreadProc()
            {
                try
                {
                    if (Thread.CurrentThread.Name?.StartsWith("SecVerse_") != true)
                    {
                        Debug.WriteLine("LHE: Thread security violation detected!");
                        return;
                    }

                    _worker.Execute();
                }
                catch (OperationCanceledException){ }
                catch (ThreadInterruptedException) { }
                catch (Exception ex)
                {
                    Debug.WriteLine($"LHE: Thread {Id} crashed: {ex.Message}");
                    _worker.OnError(ex);
                }
                finally
                {
                    _worker.Cleanup();
                }
            }

            public void Dispose()
            {
                if (_disposed) return;
                _disposed = true;

                Stop();
                _cts.Dispose();
            }
        }

        internal interface IManagedThreadWorker
        {
            void Initialize(CancellationToken token);
            void Execute();
            void Cleanup();
            void OnError(Exception ex);
        }
    }
}
