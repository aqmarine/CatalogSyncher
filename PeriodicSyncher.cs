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
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public PeriodicSyncher(TimeSpan interval, SyncManager syncher)
        {
            _syncher = syncher;
            _timer = new Timer((e) => Run(), null, TimeSpan.Zero, interval);
        }

        private void Run()
        {
            if (Monitor.TryEnter(_lockObj)) 
            {
                try 
                {
                    _syncher.Sync();
                }
                finally 
                {
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
            ((IDisposable)_timer).Dispose();
        }
    }
}