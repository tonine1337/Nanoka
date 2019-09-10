namespace Nanoka
{
    public class ElasticOptions
    {
        public string Endpoint { get; set; } = "http://localhost:9200";
        public string IndexPrefix { get; set; } = "nanoka-";
        public int ShardCount { get; set; } = 5;
        public int ReplicaCount { get; set; } = 2;
    }
}