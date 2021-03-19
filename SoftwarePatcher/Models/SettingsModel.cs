using System;

namespace SoftwarePatcher.Models
{
    [Serializable]
    public class SettingsModel:IModel
    {
        public int DownloadChunkSize { get; }
        public string DirectoryTmp { get; }
        public string DirectoryData { get; }
        public string RemoteDirectoryBuilds { get; set; }

        public string DirectoryDownload { get; set; }

        public SettingsModel()
        {
            DirectoryData = "data/";
            DirectoryTmp = "temp/data/";
            DirectoryDownload = "temp/download/";

            RemoteDirectoryBuilds = "builds";

            DownloadChunkSize = 1024 * 1000 / 2;
        }
    }
}