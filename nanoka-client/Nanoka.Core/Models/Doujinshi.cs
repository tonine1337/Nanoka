using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Nanoka.Core.Models
{
    public class Doujinshi : DoujinshiBase
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("upload")]
        public DateTime UploadTime { get; set; }

        [JsonProperty("update")]
        public DateTime UpdateTime { get; set; }

        [JsonProperty("score")]
        public int Score { get; set; }

        [JsonProperty("variations")]
        public DoujinshiVariant[] Variations { get; set; }
    }

    public class DoujinshiBase : IValidatableObject
    {
        [JsonProperty("name_original")]
        public string OriginalName { get; set; }

        [JsonProperty("name_romanized")]
        public string RomanizedName { get; set; }

        [JsonProperty("name_english")]
        public string EnglishName { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (OriginalName == null &&
                RomanizedName == null &&
                EnglishName == null)
                yield return new ValidationResult("Must specify at least one name of a doujinshi.");
        }
    }
}