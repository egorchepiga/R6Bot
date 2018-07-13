using System;
using System.Threading;
using System.Threading.Tasks;
using Segmentation.Algorithms.Helpers;
using Segmentation.Algorithms.Images;
using botCore;



namespace Segmentation.Algorithms.Screenshot
{

    public class ScreenshotSource : IDisposable
    {

        private bool _disposed;

        private readonly Screener _screener;
        private readonly UpdatableItem<BImage> _screenshotItem;

        private long _waitersCount = 0;
        private readonly ManualResetEvent _requestScreenshot = new ManualResetEvent(false);

        public float ScreenshotTime { get; private set; }

        public BImage Screenshot
        {
            get
            {
                try
                {
                    Interlocked.Increment(ref _waitersCount);
                    _requestScreenshot.Set();

                    if (!_screenshotItem.WaitNew(out var screenshot, 2000))
                    {
                        screenshot = _screenshotItem.Item;
                        if (screenshot == null)
                        {
                            return new BImage(1, 1);
                        }
                    }

                    return screenshot;
                }
                finally
                {
                    Interlocked.Decrement(ref _waitersCount);
                }
            }
        }

        public ScreenshotSource(IntPtr hWnd)
        {
            _screener = new Screener(hWnd);
            _screenshotItem = new UpdatableItem<BImage>(BImageExtentions.CopyThreadSafty);

            Task.Factory.StartNew(ScreenshotProc);
        }

        private void ScreenshotProc()
        {
            while (!_disposed)
            {
                if (_waitersCount == 0)
                {
                    _requestScreenshot.WaitOne();
                    _requestScreenshot.Reset();
                }

                using (var tracker = new TimeTracker())
                {
                    var s = _screener.Execute();
                    ScreenshotTime = tracker.ElapsedSec;

                    _screenshotItem.Update(new BImage(s));
                }
            }
        }

        public void Dispose()
        {
            _disposed = true;
        }
    }
}
