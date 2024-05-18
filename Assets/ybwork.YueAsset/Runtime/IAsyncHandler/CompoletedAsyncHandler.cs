using System.Threading.Tasks;

namespace ybwork.Assets
{
    public class CompoletedAsyncHandler : IAsyncHandler
    {
        public float Progress => 1;

        public Task Task => Task.CompletedTask;
    }
}
