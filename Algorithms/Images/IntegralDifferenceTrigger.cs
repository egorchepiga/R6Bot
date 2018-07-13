using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Segmentation.Algorithms.Images
{
    public class IntegralDifferenceTrigger
    {
        private readonly Queue<BImage> _charImages = new Queue<BImage>();
        private readonly Queue<int> _difference = new Queue<int>();

        public int QueueLength { get; set; }
        public int Devider { get; set; }

        public int Difference { get; private set; }
        public bool Triggered { get; private set; }

        public Func<int, bool> Trigger { get; set; }

        public IntegralDifferenceTrigger(Func<int, bool> trigger, int queueLen = 10, int devidor = 1000)
        {
            Trigger = trigger;

            QueueLength = queueLen;
            Devider = devidor;
        }

        public void Update(BImage img)
        {
            if (_charImages.Count == 0)
            {
                _charImages.Enqueue(img);
                return;
            }

            while (_charImages.Count >= QueueLength)
            {
                _charImages.Dequeue();
                _difference.Dequeue();
            }

            var last = _charImages.ToArray()[0];
            var diff = (int)(img.Dist2(last) / (img.H * img.W));

            _charImages.Enqueue(img);
            _difference.Enqueue(diff);

            Update();
        }

        private void Update()
        {
            var result = 0L;
            foreach (var i in _difference)
                result += i;

            Difference = (int)(result / Devider);
            Triggered = Trigger(Difference);
        }
    }
}
