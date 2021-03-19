using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using SimpleInjector;
using SoftwarePatcher.Diagnostics;
using SoftwarePatcher.Downloaders;
using SoftwarePatcher.Installers;
using SoftwarePatcher.Providers;
using SoftwarePatcher.Services;

namespace SoftwarePatcher
{
    class Launcher
    {
        static async Task Main(string[] args)
        {
            var l = new Launcher();
            await l.Run();
        }

        private readonly IInstaller _installer;
        private readonly IDownloader _downloader;
        private readonly SettingsProvider _settingsProvider;
        private readonly InstallProvider _installProvider;
        private readonly Logger _logger;

        public Launcher()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("en-US");

            var container = new Container();

            container.Register<Logger>(Lifestyle.Singleton);
            container.Register<FileService>(Lifestyle.Singleton);
            container.Register<AccessProvider>(Lifestyle.Singleton);
            container.Register<SettingsProvider>(Lifestyle.Singleton);
            container.Register<DownloadProvider>(Lifestyle.Singleton);
            container.Register<InstallProvider>(Lifestyle.Singleton);


            container.Register<Installer>();
            container.Register<Downloader>();

            _logger = container.GetInstance<Logger>();

            _installProvider = container.GetInstance<InstallProvider>();
            _settingsProvider = container.GetInstance<SettingsProvider>();


            _installer = container.GetInstance<Installer>();
            _downloader = container.GetInstance<Downloader>();
        }


        private async Task Run()
        {
            var isConnected = await _downloader.Connect();
            if (!isConnected)
            {
                _logger.Log("Could not connect to remote host!");
                return;
            }

            _logger.Log("Checking for update...");
            var hasUpdate = _downloader.HasUpdate(_installProvider.Current.IdLatestBuild);
            if (!hasUpdate)
            {
                _logger.Log("No new version found!");
            }
            else
            {
                _logger.Log("New version found!");
                _logger.Log($"Download starting...");
                var response = await _downloader.Download(OnProgress);
                if (response.Success && !string.IsNullOrEmpty(response.PathDownload))
                {
                    _logger.Log($"\nDownload Complete!");
                    var src = response.PathDownload;
                    var dst = _settingsProvider.Current.DirectoryData;
                    var tmp = _settingsProvider.Current.DirectoryTmp;


                    _logger.Log("Installing...");
                    if (_installer.Install(src, dst, tmp))
                    {
                        _installProvider.Current.IdLatestBuild = response.File.Id;
                        _installProvider.Current.NameLatestBuild = response.File.Title;
                        _installProvider.Save();
                        _logger.Log("Installation complete!");
                    }
                    else
                    {
                        _logger.Log("Installation failed!");
                    }
                }
                else
                {
                    _logger.Log($"\nDownload failed!");
                }
            }

            _logger.Log("Press any key to exit...");
            Console.Read();
        }


        private void OnProgress(DownloadInfo info)
        {
            var mb = Math.Pow(1024, 2);
            var p = info.Progress.BytesDownloaded / (float) info.FileSize;
            var d = Math.Floor(info.Progress.BytesDownloaded / mb);
            var t = Math.Floor(info.FileSize / mb);
            var progress = $"{Math.Floor(p * 100)}%\t{d} of {t} MB\r";
            Console.Write(progress);
        }
    }
}