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

        public string ElasticEndpoint { get; set; } = "http://localhost:9200";
        public string ElasticIndexPrefix { get; set; } = "nanoka-";
        public int ElasticShardCount { get; set; } = 5;
        public int ElasticReplicaCount { get; set; } = 2;

        public string IpfsEndpoint { get; set; } = "http://localhost:5001";
        public string IpfsIdentity { get; set; }
        public string IpfsSwarmKey { get; set; }

        public string RecaptchaSite { get; set; }
        public string RecaptchaSecret { get; set; }

        public UserPermissions DefaultUserPermissions { get; set; }

        public int MaxResultCount { get; set; } = 30;

        public string B2AccountId { get; set; }
        public string B2ApplicationKey { get; set; }
        public string B2BucketName { get; set; }
    }
}
