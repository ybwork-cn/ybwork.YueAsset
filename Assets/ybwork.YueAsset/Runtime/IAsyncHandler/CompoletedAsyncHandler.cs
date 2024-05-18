using System;
using System.Collections;

namespace ybwork.Assets
{
    public class CompoletedAsyncHandler : IAsyncHandler
    {
        public float Progress => 1;

        public IEnumerator Task => null;

        public bool Completed => true;

        public void Then(Action action)
        {
            action?.Invoke();
        }
    }
}
