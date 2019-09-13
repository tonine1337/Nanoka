using System;
using Nanoka.Models;
using Nest;
using Newtonsoft.Json;

namespace Nanoka.Database
{
    [ElasticsearchType(RelationName = "DeleteFile")]
    public class DbDeleteFile : IHasId
    {
        [Keyword(Name = "id", Index = false), JsonProperty("id")]
        public string Id { get; set; }

        [Date(Name = "d"), JsonProperty("d")]
        public DateTime SoftDeleteTime { get; set; }
    }
}