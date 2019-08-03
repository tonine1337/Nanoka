namespace Nanoka.Web
{
    public class NanokaOptions
    {
        public string Secret { get; set; }

        public string ElasticEndpoint { get; set; }
        public string ElasticIndexPrefix { get; set; }
    }
}