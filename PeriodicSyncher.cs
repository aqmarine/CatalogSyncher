using System;
using System.Threading;
using NLog;

namespace CatalogSyncher
{
    public class PeriodicSyncher: IDisposable
    {
        private readonly Object _lockObj = new Object();
        private readonly Timer _timer;
        private readonly SyncManager _syncher;
        private bool _synchronizationStarted;
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public bool SynchronizationRunning { get => _synchronizationStarted; }

        public PeriodicSyncher(TimeSpan interval, SyncManager syncher)
        {
            _syncher = syncher;
            _timer = new Timer((e) => Run(), null, TimeSpan.Zero, interval);
        }

        private void Run()
        {
            if (Monitor.TryEnter(_lockObj)) 
            {
                _synchronizationStarted = true;
                _logger.Info("Start sync");
                try 
                {
                   _syncher.Sync();
                }
                finally 
                {
                    _synchronizationStarted = false;
                    _logger.Info("Sync completed.");
                    Monitor.Exit(_lockObj);
                }
            }
            else
            {
                _logger.Trace("The synchronization operation is not finished yet.");
            }
        }

        public void Dispose()
        {
            _timer.Dispose();
        }
    }
}