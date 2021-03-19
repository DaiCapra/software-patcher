using SoftwarePatcher.Models;
using SoftwarePatcher.Services;

namespace SoftwarePatcher.Providers
{
    public class InstallProvider : Provider<InstallModel>
    {
        public InstallProvider(FileService fileService) : base(fileService)
        {
            Path = GlobalPaths.PathInstaller;
            Load();
        }
    }
}