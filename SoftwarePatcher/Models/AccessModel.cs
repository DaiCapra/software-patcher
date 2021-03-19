using System;

namespace SoftwarePatcher.Models
{
    [Serializable]
    public class AccessModel
    {
        public string Password { get; set; }

        public string Email { get; set; }
        public string Token { get; set; }

        public AccessModel()
        {
            Email = "launcher@launcher-303503.iam.gserviceaccount.com";
            Password = "notasecret";
            Token = "access/token.p12";
        }
    }
}