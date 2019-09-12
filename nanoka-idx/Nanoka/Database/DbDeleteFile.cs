using System;
using Nest;
using Newtonsoft.Json;

namespace Nanoka.Database
{
    [ElasticsearchType(IdProperty = nameof(Id), RelationName = "DeleteFile")]
    public class DbDeleteFile
    {
        [Keyword(Name = "id", Index = false), JsonProperty("id")]
        public string Id { get; set; }

        [Date(Name = "d"), JsonProperty("d")]
        public DateTime SoftDeleteTime { get; set; }
    }
}