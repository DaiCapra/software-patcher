using Google.Apis.Download;

namespace SoftwarePatcher.Downloaders
{
    public struct DownloadInfo
    {
        public long FileSize;
        public IDownloadProgress Progress;
    }
}