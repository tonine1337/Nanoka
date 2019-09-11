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
    }
}