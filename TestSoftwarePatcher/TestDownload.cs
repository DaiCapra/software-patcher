using System.Threading.Tasks;
using SoftwarePatcher.Diagnostics;
using SoftwarePatcher.Downloaders;
using SoftwarePatcher.Models;
using SoftwarePatcher.Providers;
using SoftwarePatcher.Services;
using NUnit.Framework;
using SimpleInjector;

namespace TestGameLauncher
{
    public class TestDownload
    {
        private Downloader _downloader;
        private AccessModel _accessModel;
        private DownloadProvider _downloadProvider;

        [SetUp]
        public void Setup()
        {
            var container = new Container();
            container.Options.ResolveUnregisteredConcreteTypes = true;
            container.Register<Logger>(Lifestyle.Singleton);
            container.Register<FileService>(Lifestyle.Singleton);
            container.Register<Downloader>();
            container.Register<AccessProvider>(Lifestyle.Singleton);
            container.Register<DownloadProvider>(Lifestyle.Singleton);
            container.Register<SettingsProvider>(Lifestyle.Singleton);

            container.GetInstance<SettingsProvider>().Current.RemoteDirectoryBuilds = "test";
            _accessModel = container.GetInstance<AccessModel>();
            _downloader = container.GetInstance<Downloader>();
            _downloadProvider = container.GetInstance<DownloadProvider>();
        }


        [Test]
        public async Task TestLatest()
        {
            var response = await _downloader.RemoteDownloadLatest();
            Assert.True(response.Success);
            Assert.Greater(response.Bytes.Length, 0);
        }


        [Test]
        public void TestCredentials()
        {
            var b = Downloader.TryGetCredentials(_accessModel, out var credential);
            Assert.True(b);
        }

        [Test]
        public async Task TestFiles()
        {
            _downloader.RemoteAuthenticate();
            await _downloader.RemoteUpdateFiles();
            Assert.Greater(_downloadProvider.Current.MapFiles.Count, 0);
        }
    }
}