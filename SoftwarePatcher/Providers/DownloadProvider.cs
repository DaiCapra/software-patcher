using SoftwarePatcher.Models;
using SoftwarePatcher.Services;

namespace SoftwarePatcher.Providers
{
    public class DownloadProvider : Provider<DownloadModel>
    {
        public DownloadProvider(FileService fileService) : base(fileService)
        {
        }
    }
}