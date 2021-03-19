using System;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using Newtonsoft.Json;
using SoftwarePatcher.Diagnostics;

namespace SoftwarePatcher.Services
{
    public class FileService
    {
        private static JsonSerializerSettings _jsonSettings = new JsonSerializerSettings()
        {
            Formatting = Formatting.Indented,
        };

        private readonly Logger _logger;

        public FileService(Logger logger)
        {
            _logger = logger;
        }

        public string GetFilenameTemp(string path, string name = "-temp")
        {
            var extension = Path.GetExtension(path);
            var pathFile = path.Substring(0, path.Length - extension.Length);
            var temp = $"{pathFile}{name}{extension}";

            return temp;
        }

        public bool TryCreateDirectory(string path)
        {
            var p = Path.GetDirectoryName(path);
            if (Directory.Exists(p))
            {
                return true;
            }

            try
            {
                Directory.CreateDirectory(p);
                return true;
            }
            catch (Exception e)
            {
                _logger.Error(e);
            }

            return false;
        }

        public bool TryDeleteFile(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                    return true;
                }
            }
            catch (Exception e)
            {
                _logger.Error(e);
            }

            return false;
        }

        public bool TryDeleteDirectory(string path)
        {
            try
            {
                if (Directory.Exists(path))
                {
                    Directory.Delete(path, true);
                    return true;
                }
            }
            catch (Exception e)
            {
                _logger.Error(e);
            }

            return false;
        }

        public bool TryExtractZip(string source, string destination)
        {
            if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(destination))
            {
                return false;
            }

            if (!File.Exists(source))
            {
                return false;
            }

            try
            {
                ZipFile.ExtractToDirectory(source, destination);
                return true;
            }
            catch (Exception e)
            {
                _logger.Error(e);
            }

            return false;
        }

        public bool TryGetMd5Checksum(string path, out string hash)
        {
            hash = string.Empty;

            if (!File.Exists(path))
            {
                return false;
            }

            using var md5 = MD5.Create();
            try
            {
                using var stream = File.OpenRead(path);
                var bytes = md5.ComputeHash(stream);
                hash = BitConverter.ToString(bytes)
                    .Replace("-", "")
                    .ToLowerInvariant();
                return true;
            }
            catch (Exception e)
            {
                _logger.Error(e);
            }

            return false;
        }

        public bool TryReplace(string source, string destination)
        {
            if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(destination))
            {
                return false;
            }

            if (!File.Exists(source))
            {
                return false;
            }

            try
            {
                var temp = GetFilenameTemp(destination);
                // backup file
                if (File.Exists(destination))
                {
                    File.Copy(destination, temp, true);
                }

                File.Copy(source, destination, true);

                // ensure same file
                TryGetMd5Checksum(source, out var h1);
                TryGetMd5Checksum(destination, out var h2);

                if (h1.Equals(h2))
                {
                    // cleanup
                    if (File.Exists(temp))
                    {
                        TryDeleteFile(temp);
                    }

                    return true;
                }
                else
                {
                    _logger.Error($"Could not replace {source} with {destination}");
                    return false;
                }
            }
            catch (Exception e)
            {
                _logger.Error(e);
            }

            return false;
        }

        public bool TrySave(string path, byte[] bytes)
        {
            try
            {
                File.WriteAllBytes(path, bytes);
            }
            catch (Exception e)
            {
                _logger.Error(e);
            }

            return false;
        }

        public bool TryLoadFromJson<T>(string path, out T t)
        {
            t = default;
            try
            {
                if (File.Exists(path))
                {
                    var json = File.ReadAllText(path);
                    t = JsonConvert.DeserializeObject<T>(json,_jsonSettings);
                    return true;
                }
            }
            catch (Exception e)
            {
                _logger.Error(e);
            }

            return false;
        }

        public bool TrySaveToJson<T>(string path, T t)
        {
            try
            {
                var json = JsonConvert.SerializeObject(t,_jsonSettings);
                if (File.Exists(path))
                {
                    File.Delete(path);
                }

                TryCreateDirectory(path);

                File.WriteAllText(path, json);
                return true;
            }
            catch (Exception e)
            {
                _logger.Error(e);
            }

            return false;
        }
    }
}