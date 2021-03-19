using Google.Apis.Drive.v2.Data;

namespace SoftwarePatcher.Downloaders
{
    public class DownloadResponse
    {
        public bool Success;
        public byte[] Bytes;
        public File File;
        public string PathDownload;
    }
}