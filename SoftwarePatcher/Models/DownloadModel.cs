using System.Collections.Generic;
using Google.Apis.Drive.v2.Data;

namespace SoftwarePatcher.Models
{
    public class DownloadModel
    {
        public Dictionary<string, File> MapFiles { get; set; }
        public Dictionary<string, File> MapBuilds { get; set; }

        public DownloadModel()
        {
            MapFiles = new Dictionary<string, File>();
            MapBuilds = new Dictionary<string, File>();
        }
    }
}