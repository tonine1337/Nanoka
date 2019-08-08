using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Newtonsoft.Json;

namespace Nanoka.Core.Models.Query
{
    public abstract class QueryWrapperBase<TSelf, TSort>
        where TSelf : QueryWrapperBase<TSelf, TSort>
        where TSort : Enum
    {
        [JsonProperty("offset"), Range(0, int.MaxValue)]
        public int Offset { get; set; }

        [JsonProperty("limit"), Range(1, int.MaxValue), Required]
        public int Limit { get; set; }

        /// <summary>
        /// Queries against all fields.
        /// </summary>
        [JsonProperty("all")]
        public TextQuery All { get; set; }

        [JsonProperty("sorting"), Required]
        public List<TSort> Sorting { get; set; }

        protected TSelf Set(Action<TSelf> setter)
        {
            var self = (TSelf) this;

            setter(self);

            return self;
        }

        public TSelf WithOffset(int offset) => Set(x => x.Offset = offset);
        public TSelf WithLimit(int limit) => Set(x => x.Limit = limit);

        public TSelf WithAll(TextQuery q) => Set(x => x.All = q);

        public TSelf WithSorting(Action<SortingBuilder<TSort>> build) => Set(x =>
        {
            var builder = new SortingBuilder<TSort>();
            build(builder);

            x.Sorting = builder.Items.ToList();
        });
    }
}
