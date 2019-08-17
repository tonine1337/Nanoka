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

        public string IpfsEndpoint { get; set; } = "http://localhost:5001";
        public string IpfsIdentity { get; set; }
        public string IpfsSwarmKey { get; set; }

        public string RecaptchaSite { get; set; }
        public string RecaptchaSecret { get; set; }

        public UserPermissions DefaultUserPermissions { get; set; }

        public int MaxResultCount { get; set; } = 30;
    }
}