using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using SecVerseLHE.Helper;
using SecVerseLHE.UI;

namespace SecVerseLHE.Core
{
    internal class RansomwareDetectionWorker : IBackgroundWorker
    {
        private sealed class EventWindow
        {
            private readonly object _lock = new object();
            private readonly Queue<DateTime> _timestamps = new Queue<DateTime>();

            public void Add(DateTime timestamp)
            {
                lock (_lock)
                {
                    _timestamps.Enqueue(timestamp);
                }
            }

            public int CountSince(DateTime threshold)
            {
                lock (_lock)
                {
                    while (_timestamps.Count > 0 && _timestamps.Peek() < threshold)
                        _timestamps.Dequeue();

                    return _timestamps.Count;
                }
            }
        }


        private readonly ConcurrentDictionary<int, EventWindow> _events = new ConcurrentDictionary<int, EventWindow>();
        private readonly TimeSpan _window;
        private readonly int _thresholdCount;
        private readonly Func<int, bool> _processWhitelist;
        private bool _disposed;

        private readonly TrayManager _trayManager;

        public new string Name => "RansomwareDetection";


        public RansomwareDetectionWorker(
            TrayManager trayManager,
            string name = "RansomwareDetection",
            TimeSpan? window = null,
            int thresholdCount = 30,
            Func<int, bool> processWhitelist = null)
        {
            _trayManager = trayManager ?? throw new ArgumentNullException(nameof(trayManager));
            Name = string.IsNullOrWhiteSpace(name) ? "RansomwareDetection" : name;
            _window = window ?? TimeSpan.FromSeconds(10);
            _thresholdCount = thresholdCount;
            _processWhitelist = processWhitelist ?? (_ => false);
        }

        public void ReportEncryptedFile(int processId, string filePath)
        {
            if (_disposed)
                return;

            if (processId <= 0)
                return;

            if (_processWhitelist != null && _processWhitelist(processId)) 
                return;


            var evt = _events.GetOrAdd(processId, _ => new EventWindow());
            evt.Add(DateTime.UtcNow);
        }

        public new void Run(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    Scan(token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error in RansomwareWorker: {ex.Message}");
                }

                token.WaitHandle.WaitOne(TimeSpan.FromSeconds(1));
            }
        }

        private void Scan(CancellationToken token)
        {
            var now = DateTime.UtcNow;
            var thresholdTime = now - _window;

            foreach (var kvp in _events.ToArray())
            {
                token.ThrowIfCancellationRequested();

                var pid = kvp.Key;
                var window = kvp.Value;
                var count = window.CountSince(thresholdTime);

                if (count >= _thresholdCount)
                {
                    if (TryHandleProcess(pid, count))
                    {
                        _events.TryRemove(pid, out _);
                    }
                }
            }
        }

        private bool TryHandleProcess(int processId, int count)
        {
            Process process = null;

            try
            {
                process = Process.GetProcessById(processId);
            }
            catch
            {
                return false;
            }

            string processName = process.ProcessName;

            if (!ProcessHelper.SuspendProcess(process))
                return false;

            var result = MessageBox.Show(
                $"Process '{processName}' (PID {processId}) encrypted {count} files in {_window.TotalSeconds:0} seconds.\n\n" +
                "Is this expected?\n\n" +
                "Yes = allow and resume\n" +
                "No = terminate process",
                "Possible ransomware activity detected",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2);

            if (result == DialogResult.Yes)
            {
                ProcessHelper.ResumeProcess(process);
                return false;
            }

            try
            {
                process.Kill();
            }
            catch
            {
            }

            return true;
        }

        public void Dispose()
        {
            _disposed = true;
            _events.Clear();
        }
    }
}
