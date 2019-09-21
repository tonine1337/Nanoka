using System;
using System.Collections.Generic;

namespace Nanoka.Models
{
    public class QuerySortingBuilder<TSort>
        where TSort : Enum
    {
        static readonly Func<TSort, TSort> _invert;

        static QuerySortingBuilder()
        {
            var type = Enum.GetUnderlyingType(typeof(TSort));

            if (type == typeof(short))
                _invert = x => (TSort) (object) ~(byte) (object) x;

            else if (type == typeof(int))
                _invert = x => (TSort) (object) ~(int) (object) x;

            else if (type == typeof(long))
                _invert = x => (TSort) (object) ~(long) (object) x;

            else
                throw new NotSupportedException($"Underlying enum type '{type}' not supported for conversion.");
        }

        readonly List<TSort> _list = new List<TSort>();
        readonly HashSet<TSort> _set = new HashSet<TSort>();

        public TSort[] Items => _list.ToArray();

        public QuerySortingBuilder<TSort> Ascending(TSort sort)
        {
            if (!_set.Add(sort))
                throw new InvalidOperationException($"May not sort by '{sort}' more than once.");

            _list.Add(sort);
            return this;
        }

        public QuerySortingBuilder<TSort> Descending(TSort sort)
        {
            if (!_set.Add(sort))
                throw new InvalidOperationException($"May not sort by '{sort}' more than once.");

            _list.Add(_invert(sort));
            return this;
        }
    }
}