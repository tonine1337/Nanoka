using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Nanoka.Models
{
    public abstract class QueryBase<TThis> where TThis : QueryBase<TThis>
    {
        [JsonProperty("offset"), Range(0, int.MaxValue)]
        public int Offset { get; set; }

        [JsonProperty("limit"), Range(0, int.MaxValue)]
        public int Limit { get; set; }

        protected TThis Set(Action<TThis> setter)
        {
            var @this = (TThis) this;

            setter(@this);

            return @this;
        }
    }

    public abstract class QueryBase<TThis, TSort> : QueryBase<TThis> where TThis : QueryBase<TThis, TSort>
    {
        [JsonProperty("_sort")]
        public TSort[] Sorting { get; set; }
    }
}