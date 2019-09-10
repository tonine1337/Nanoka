using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Nanoka.Models
{
    public class Book : BookBase
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("upload")]
        public DateTime UploadTime { get; set; }

        [JsonProperty("update")]
        public DateTime UpdateTime { get; set; }

        [JsonProperty("score")]
        public int Score { get; set; }

        [JsonProperty("variants")]
        public List<BookVariant> Variants { get; set; }
    }

    public class BookBase
    {
        [JsonProperty("category"), EnumDataType(typeof(BookCategory))]
        public BookCategory Category { get; set; }

        [JsonProperty("metas"), Required]
        public Dictionary<BookMeta, string[]> Metas { get; set; }
    }
}