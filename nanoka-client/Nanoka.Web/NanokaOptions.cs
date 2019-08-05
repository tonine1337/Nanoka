using Nanoka.Core.Models;

namespace Nanoka.Web
{
    public class NanokaOptions
    {
        // development secret; should be overridden in production to something safer
        public string Secret { get; set; } = "OAcB&fhvjYfb#iHPG4E33OEAV*1X!5jFeZsXvB9zKLsZ$wZWe5$WK75x9t!R";

        public string ElasticEndpoint { get; set; }
        public string ElasticIndexPrefix { get; set; } = "nanoka-";

        public string RecaptchaSite { get; set; }
        public string RecaptchaSecret { get; set; }

        public UserPermissions DefaultUserPermissions { get; set; }

        public int MaxResultCount { get; set; } = 30;
    }
}