using System;
using System.Diagnostics;

namespace Segmentation.Algorithms.Helpers
{
    public class TimeTracker : IDisposable
    {
        private readonly Stopwatch _watch = new Stopwatch();
        public TimeTracker()
        {
            _watch.Start();
        }

        public void Dispose()
        {
            _watch.Stop();
        }

        public float ElapsedSec { get { return _watch.ElapsedMilliseconds / 1000.0f; } }
        public int ElapsedMilliSec { get { return (int)_watch.ElapsedMilliseconds; } }
    }
}
