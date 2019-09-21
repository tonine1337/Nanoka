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

        public TThis WithRange(int start, int end) => Set(x =>
        {
            x.Offset = Math.Max(start, 0);
            x.Limit  = Math.Max(end - start, 0);
        });
    }

    public abstract class QueryBase<TThis, TSort> : QueryBase<TThis> where TThis : QueryBase<TThis, TSort>
                                                                     where TSort : Enum
    {
        [JsonProperty("_sort")]
        public TSort[] Sorting { get; set; }

        public TThis WithSorting(Func<QuerySortingBuilder<TSort>, QuerySortingBuilder<TSort>> sort)
            => Set(x => x.Sorting = sort(new QuerySortingBuilder<TSort>()).Items);
    }
}
