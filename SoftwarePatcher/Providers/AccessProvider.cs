using SoftwarePatcher.Models;
using SoftwarePatcher.Services;

namespace SoftwarePatcher.Providers
{
    public class AccessProvider : Provider<AccessModel>
    {

        public AccessProvider(FileService fileService) : base(fileService)
        {
            Path = GlobalPaths.PathAccess;
            Load();
        }
    }
}