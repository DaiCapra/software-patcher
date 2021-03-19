using System.IO;
using SoftwarePatcher.Diagnostics;
using SoftwarePatcher.Services;
using NUnit.Framework;
using SimpleInjector;

namespace TestGameLauncher
{
    public class TestFiles
    {
        private FileService _service;

        [SetUp]
        public void Setup()
        {
            var container = new Container();
            container.Register<Logger>(Lifestyle.Singleton);
            container.Register<FileService>(Lifestyle.Singleton);
            container.Options.ResolveUnregisteredConcreteTypes = true;
            _service = container.GetInstance<FileService>();
        }

        [Test]
        public void TestPathTemp()
        {
            var s1 = "data/foo.json";
            var s2 = "data/foo-temp.json";
            var temp = _service.GetFilenameTemp(s1, "-temp");
            Assert.AreEqual(s2, temp);
        }

        [Test]
        public void TestReplace()
        {
            var s1 = "temp/replace/src/foo.dat";
            var s2 = "temp/replace/dst/foo.dat";

            _service.TryCreateDirectory(s1);
            _service.TryCreateDirectory(s2);

            _service.TryDeleteFile(s1);
            _service.TryDeleteFile(s2);

            var stream1 = File.Create(s1);
            stream1.Write(new[] {byte.MaxValue}, 0, 1);
            var stream2 = File.Create(s2);
            stream1.Dispose();
            stream2.Dispose();

            _service.TryGetMd5Checksum(s1, out var h1);
            _service.TryGetMd5Checksum(s2, out var h2);
            Assert.AreNotEqual(h1, h2);

            _service.TryReplace(s1, s2);

            _service.TryGetMd5Checksum(s1, out h1);
            _service.TryGetMd5Checksum(s2, out h2);
            Assert.AreEqual(h1, h2);
        }

        
    }
}