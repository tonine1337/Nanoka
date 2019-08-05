using Nanoka.Core.Models;

namespace Nanoka.Web
{
    public class NanokaOptions
    {
        public string Secret { get; set; }

        public string ElasticEndpoint { get; set; }
        public string ElasticIndexPrefix { get; set; }

        public string RecaptchaSite { get; set; }
        public string RecaptchaSecret { get; set; }

        public UserPermissions DefaultUserPermissions { get; set; }
    }
}