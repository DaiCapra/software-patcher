using SoftwarePatcher.Models;
using SoftwarePatcher.Services;

namespace SoftwarePatcher.Providers
{
    public class SettingsProvider : Provider<SettingsModel>
    {
        public SettingsProvider(FileService fileService) : base(fileService)
        {
            Path = GlobalPaths.PathSettings;
            Load();
        }
    }
}