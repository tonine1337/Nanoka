using Nanoka.Core.Models;

namespace Nanoka.Web
{
    public class NanokaOptions
    {
        // development secret; should be overridden in production to something safer
        public string Secret { get; set; } = "OAcB&fhvjYfb#iHPG4E33OEAV*1X!5jFeZsXvB9zKLsZ$wZWe5$WK75x9t!R";

        public string ElasticEndpoint { get; set; } = "http://localhost:9200";
        public string ElasticIndexPrefix { get; set; } = "nanoka-";

        public string IpfsEndpoint { get; set; } = "http://localhost:5001";
        public string IpfsIdentity { get; set; }
        public string IpfsSwarmKey { get; set; }

        public string RecaptchaSite { get; set; }
        public string RecaptchaSecret { get; set; }

        public UserPermissions DefaultUserPermissions { get; set; }

        public int MaxResultCount { get; set; } = 30;
    }
}