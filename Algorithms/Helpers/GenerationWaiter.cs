using System.Threading;

namespace Segmentation.Algorithms.Helpers
{
    public class GenerationWaiter
    {
        private readonly ManualResetEvent _newGeneration = new ManualResetEvent(false);

        public void Update()
        {
            _newGeneration.Set();
        }

        public bool WaitNew(int timeout = int.MaxValue)
        {
            const int generationCountForActual = 2;

            for (var i = 0; i < generationCountForActual; i++)
            {
                if (!_newGeneration.WaitOne(timeout / generationCountForActual))
                    return false;

                _newGeneration.Reset();
            }

            return true;
        }
    }
}
