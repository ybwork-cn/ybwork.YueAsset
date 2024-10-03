namespace ybwork.Assets
{
    internal class CompoletedAsyncHandler : IAsyncHandlerDownload
    {
        AsyncEvent IAsyncHandler.OnComplete { get; } = new();
        public bool Completed { get; } = true;
        float IAsyncHandlerDownload.Progress => 1;
    }
}
