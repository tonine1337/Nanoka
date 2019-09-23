using System;
using System.Security.Cryptography;
using Nanoka.Models;

namespace Nanoka
{
    public class NanokaOptions
    {
        public string Secret { get; set; }

        public NanokaOptions()
        {
            // generate a random secret on startup in production
            using (RandomNumberGenerator rng = new RNGCryptoServiceProvider())
            {
                var buffer = new byte[64];
                rng.GetBytes(buffer);

                Secret = Convert.ToBase64String(buffer);
            }
        }

        public UserPermissions DefaultUserPermissions { get; set; }

        public int PasswordHashVersion { get; set; }
        public int MaxImageUploadSize { get; set; } = 4000000;
        public int MaxImageUploadCount { get; set; } = 250;
        public int UploadTaskLimitPerUser { get; set; } = 3;
        public double UploadTaskExpiryMs { get; set; } = 1000 * 60 * 10; // 10 minutes
    }
}
