using System;
using System.Threading.Tasks;

namespace SoftwarePatcher.Downloaders
{
    public interface IDownloader
    {
        bool HasUpdate(string idLatest);
        Task<DownloadResponse> Download(Action<DownloadInfo> callbackProgress = null);
        Task<bool> Connect();
    }
}