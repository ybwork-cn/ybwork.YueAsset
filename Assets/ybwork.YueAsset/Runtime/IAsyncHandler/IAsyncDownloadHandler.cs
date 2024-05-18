namespace ybwork.Assets
{
    public interface IAsyncDownloadHandler : IAsyncHandler
    {
        float IAsyncHandler.Progress => Length == 0 ? 0 : (float)DownloadedBytes / Length;
        long DownloadedBytes { get; }
        long Length { get; }
    }
}
