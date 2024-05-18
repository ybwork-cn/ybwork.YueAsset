using System.Threading.Tasks;

namespace ybwork.Assets
{
    public interface IAsyncHandler
    {
        /// <summary>
        /// 返回[0,1]区间的数字，用于指示进度
        /// </summary>
        public float Progress { get; }
        /// <summary>
        /// 此异步任务的Task,该Task完成即代表此异步任务已完成
        /// </summary>
        public Task Task { get; }
    }
}
