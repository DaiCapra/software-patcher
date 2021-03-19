using SoftwarePatcher.Services;

namespace SoftwarePatcher.Providers
{
    public class Provider<T> where T : new()
    {
        public T Current { get; set; }
        public string Path { get; set; }
        private readonly FileService _fileService;

        public Provider(FileService fileService)
        {
            _fileService = fileService;
            Current = new T();
        }

        public void Save()
        {
            _fileService.TrySaveToJson(Path, Current);
        }

        public void Load()
        {
            if (_fileService.TryLoadFromJson(Path, out T t))
            {
                Current = t;
            }
            else
            {
                // Ensure default file
                Save();
            }
        }
    }
}