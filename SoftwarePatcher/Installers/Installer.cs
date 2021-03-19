using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using SoftwarePatcher.Diagnostics;
using SoftwarePatcher.Providers;
using SoftwarePatcher.Services;

namespace SoftwarePatcher.Installers
{
    public class Installer : IInstaller
    {
        private readonly Logger _logger;
        private readonly FileService _fileService;
        private readonly InstallProvider _installProvider;

        public Installer(FileService fileService, Logger logger, InstallProvider installProvider)
        {
            _fileService = fileService;
            _logger = logger;
            _installProvider = installProvider;
        }

        // extract zip to temp dir, move temp dir content to target.
        public bool Install(string source, string dirDestination, string dirTemp, bool deleteSource = true)
        {
            if (!File.Exists(source))
            {
                return false;
            }

            if (!_fileService.TryCreateDirectory(dirDestination))
            {
                return false;
            }

            if (!_fileService.TryCreateDirectory(dirTemp))
            {
                return false;
            }

            if (!_fileService.TryExtractZip(source, dirTemp))
            {
                return false;
            }

            // map filename-hashes in temp and target
            var dictTemp = GetFileHashDict(dirTemp);
            var dictDst = GetFileHashDict(dirDestination);

            // copy files that are new or differ
            foreach (var (fileTemp, hashTemp) in dictTemp)
            {
                bool replace = false;
                if (dictDst.ContainsKey(fileTemp))
                {
                    var hashDest = dictDst[fileTemp];
                    if (!hashTemp.Equals(hashDest))
                    {
                        replace = true;
                    }
                }
                else
                {
                    replace = true;
                }

                if (replace)
                {
                    var pathTemp = $"{dirTemp}/{fileTemp}";
                    var pathDst = $"{dirDestination}/{fileTemp}";
                    _fileService.TryCreateDirectory(pathDst);
                    
                    bool didReplace = _fileService.TryReplace(pathTemp, pathDst);
                    if (!didReplace)
                    {
                        return false;
                    }
                }
            }

            // cleanup
            var dirSource = Path.GetDirectoryName(source);

            _fileService.TryDeleteDirectory(dirTemp);
            if (deleteSource)
            {
                _fileService.TryDeleteDirectory(dirSource);
            }

            return true;
        }

        private Dictionary<string, string> GetFileHashDict(string path)
        {
            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var files = Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories);

            using var md5 = MD5.Create();
            foreach (var file in files)
            {
                try
                {
                    var pathRelative = file.Substring(path.Length, file.Length - path.Length);
                    if (_fileService.TryGetMd5Checksum(file, out var hash))
                    {
                        map.TryAdd(pathRelative, hash);
                    }
                }
                catch (Exception e)
                {
                    _logger.Error(e);
                }
            }

            return map;
        }
    }
}