using System;

namespace Segmentation.Algorithms.Helpers
{
    public class UpdatableItem<T> : GenerationWaiter
    {
        private readonly Func<T, T> _translater;
        private T _item;
        public T Item => _translater(_item);

        public UpdatableItem(Func<T, T> translater = null)
        {
            if (translater == null)
                translater = (t) => t;
            _translater = translater;
        }

        public void Update(T item)
        {
            _item = item;
            Update();
        }

        public bool WaitNew(out T item, int timeout = int.MaxValue)
        {
            if (!WaitNew(timeout))
            {
                item = default(T);
                return false;
            }

            item = Item;
            return true;
        }
    }
}
