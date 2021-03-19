using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v2;
using Google.Apis.Services;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using SoftwarePatcher.Diagnostics;
using SoftwarePatcher.Models;
using SoftwarePatcher.Providers;
using SoftwarePatcher.Services;
using File = System.IO.File;

namespace SoftwarePatcher.Downloaders
{
    public class Downloader : IDownloader
    {
        private const string ApplicationName = "Launcher";
        private static readonly string[] Scopes = {DriveService.Scope.DriveReadonly};

        private readonly AccessProvider _accessProvider;
        private readonly DownloadProvider _downloadProvider;
        private readonly Logger _logger;
        private readonly SettingsProvider _settingsProvider;
        private readonly FileService _fileService;

        private DriveService _driveService;

        public Downloader(Logger logger, AccessProvider accessProvider, DownloadProvider downloadProvider,
            SettingsProvider settingsProvider, FileService fileService)
        {
            _logger = logger;
            _accessProvider = accessProvider;
            _downloadProvider = downloadProvider;
            _settingsProvider = settingsProvider;
            _fileService = fileService;

            _driveService = new DriveService();
        }

        public static bool TryGetCredentials(AccessModel model, out ServiceAccountCredential credential)
        {
            credential = null;
            if (model == null)
            {
                return false;
            }

            var pathToken = model.Token;
            if (!File.Exists(pathToken))
            {
                return false;
            }

            var certificate = new X509Certificate2(pathToken, model.Password, X509KeyStorageFlags.Exportable);
            credential = new ServiceAccountCredential(new ServiceAccountCredential.Initializer(model.Email)
            {
                Scopes = Scopes
            }.FromCertificate(certificate));

            return true;
        }

        public bool RemoteAuthenticate()
        {
            var model = _accessProvider.Current;

            if (model == null)
            {
                return false;
            }

            if (!TryGetCredentials(model, out var credential))
            {
                return false;
            }

            try
            {
                _driveService = new DriveService(new BaseClientService.Initializer
                {
                    HttpClientInitializer = credential,
                    ApplicationName = ApplicationName,
                });
                return true;
            }
            catch (Exception e)
            {
                _logger.Error(e);
            }

            return false;
        }

        public async Task<DownloadResponse> RemoteDownloadLatest(Action<DownloadInfo> callbackProgress = null)
        {
            var response = new DownloadResponse();
            if (!RemoteAuthenticate())
            {
                return response;
            }

            bool success = await RemoteUpdateFiles();
            if (!success)
            {
                return response;
            }

            // get last modifier build id
            var file = GetLatestBuild();

            if (file == null)
            {
                return response;
            }

            var request = _driveService.Files.Get(file.Id);
            request.MediaDownloader.ChunkSize = _settingsProvider.Current.DownloadChunkSize;
            await using var streamMemory = new MemoryStream();

            // progress callback
            if (callbackProgress != null)
            {
                request.MediaDownloader.ProgressChanged += progress =>
                {
                    var info = new DownloadInfo()
                    {
                        FileSize = (long) file.FileSize,
                        Progress = progress,
                    };
                    callbackProgress.Invoke(info);
                };
            }

            await request.MediaDownloader.DownloadAsync(file.DownloadUrl, streamMemory);

            // copy used part of the buffer
            var buffer = streamMemory.GetBuffer();
            var bytes = new byte[streamMemory.Length];
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = buffer[i];
            }

            response.Success = true;
            response.Bytes = bytes;
            response.File = file;
            return response;
        }

        // request remote file data and copy it to local model
        public async Task<bool> RemoteUpdateFiles()
        {
            var model = _downloadProvider.Current;
            if (model == null)
            {
                return false;
            }

            try
            {
                var requestFiles = _driveService.Files.List();
                var files = await requestFiles.ExecuteAsync();

                model.MapFiles.Clear();
                model.MapBuilds.Clear();

                // Update all file info
                foreach (var file in files.Items)
                {
                    model.MapFiles.TryAdd(file.Id, file);
                }

                if (!TryGetDirectoryId(_settingsProvider.Current.RemoteDirectoryBuilds, out var idBuilds))
                {
                    return false;
                }

                // request build directory content
                var requestBuilds = _driveService.Children.List(idBuilds);
                var builds = await requestBuilds.ExecuteAsync();
                foreach (var child in builds.Items)
                {
                    var id = child.Id;
                    if (model.MapFiles.TryGetValue(id, out var build))
                    {
                        model.MapBuilds.TryAdd(id, build);
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                _logger.Error(e);
            }

            return false;
        }

        public bool TryGetDirectoryId(string directory, out string id)
        {
            id = string.Empty;
            var model = _downloadProvider.Current;
            foreach (var (fileId, file) in model.MapFiles)
            {
                if (file.Title.Equals(directory, StringComparison.OrdinalIgnoreCase))
                {
                    id = fileId;
                    return true;
                }
            }

            return false;
        }

        private Google.Apis.Drive.v2.Data.File GetLatestBuild()
        {
            var file = _downloadProvider.Current.MapBuilds
                .Select(t => t.Value)
                .OrderByDescending(t => t.ModifiedDate)
                .FirstOrDefault();

            return file;
        }

        public bool HasUpdate(string idLatest)
        {
            var file = GetLatestBuild();
            return !file.Id.Equals(idLatest);
        }

        public async Task<DownloadResponse> Download(Action<DownloadInfo> callbackProgress = null)
        {
            DownloadResponse response = new DownloadResponse();

            response = await RemoteDownloadLatest(callbackProgress);
            if (response.Success && response.Bytes.Length > 0)
            {
                var dir = $"{_settingsProvider.Current.DirectoryDownload}/";
                var path = $"{dir}/{response.File.Title}";
                if (_fileService.TryCreateDirectory(dir))
                {
                    _fileService.TrySave(path, response.Bytes);
                    response.PathDownload = path;
                }
            }

            return response;
        }

        public async Task<bool> Connect()
        {
            RemoteAuthenticate();
            var success = await RemoteUpdateFiles();
            return success;
        }
    }
}