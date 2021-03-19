using SoftwarePatcher.Diagnostics;
using SoftwarePatcher.Installers;
using SoftwarePatcher.Services;
using NUnit.Framework;
using SimpleInjector;

namespace TestGameLauncher
{
    public class TestSoftwarePatcher
    {
        private Installer _installer;
        private FileService _fileService;

        [SetUp]
        public void Setup()
        {
            var container = new Container();
            container.Options.ResolveUnregisteredConcreteTypes = true;
            container.Register<Logger>(Lifestyle.Singleton);
            container.Register<FileService>(Lifestyle.Singleton);
            container.Register<Installer>();

            _fileService = container.GetInstance<FileService>();
            _installer = container.GetInstance<Installer>();
        }

        [Test]
        public void TestInstall()
        {
            var src = "temp/install/temp/download/build.zip";
            var temp = "temp/install/temp/data/";
            var dst = "temp/install/data/";

            _fileService.TryDeleteDirectory(temp);
            _fileService.TryDeleteDirectory(dst);
            
            bool didInstall = _installer.Install(src, dst, temp, false);
            Assert.True(didInstall);
        }
    }
}