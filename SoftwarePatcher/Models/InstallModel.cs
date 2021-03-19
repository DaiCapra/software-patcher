using System;

namespace SoftwarePatcher.Models
{
    [Serializable]
    public class InstallModel
    {
        public string IdLatestBuild;
        public string NameLatestBuild;

        public InstallModel()
        {
            IdLatestBuild = "";
            NameLatestBuild = "";
        }
    }
}