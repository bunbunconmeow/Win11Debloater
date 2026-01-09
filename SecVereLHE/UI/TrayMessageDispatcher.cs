using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SecVerseLHE.UI
{
    internal class TrayMessageDispatcher : IDisposable
    {
        private readonly TrayManager _trayManager;
        private readonly ConcurrentQueue<TrayMessage> _queue;
        private readonly Timer _timer;
        private bool _disposed;

        private readonly struct TrayMessage
        {
            public TrayMessage(string title, string text)
            {
                Title = title;
                Text = text;
            }

            public string Title { get; }
            public string Text { get; }
        }

        public TrayMessageDispatcher(TrayManager trayManager, int intervalMs = 1000)
        {
            _trayManager = trayManager ?? throw new ArgumentNullException(nameof(trayManager));
            _queue = new ConcurrentQueue<TrayMessage>();

            _timer = new Timer
            {
                Interval = intervalMs
            };

            _timer.Tick += OnTimerTick;
            _timer.Start();
        }

        public void Enqueue(string title, string message)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(TrayMessageDispatcher));

            if (string.IsNullOrWhiteSpace(message))
                return;

            if (string.IsNullOrWhiteSpace(title))
                title = "SecVerse LHE";

            _queue.Enqueue(new TrayMessage(title, message));
        }

        private void OnTimerTick(object sender, EventArgs e)
        {
            if (_disposed)
                return;

            while (_queue.TryDequeue(out var msg))
            {
                _trayManager.ShowAlert(msg.Title, msg.Text);
            }
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            _timer.Stop();
            _timer.Tick -= OnTimerTick;
            _timer.Dispose();
        }
    }
}
