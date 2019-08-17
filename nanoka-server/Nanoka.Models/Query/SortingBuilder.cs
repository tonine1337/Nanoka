using System;
using System.Collections.Generic;

namespace Nanoka.Models.Query
{
    public class SortingBuilder<TSort>
        where TSort : Enum
    {
        static readonly Func<TSort, TSort> _negate;

        static SortingBuilder()
        {
            var type = Enum.GetUnderlyingType(typeof(TSort));

            if (type == typeof(short))
                _negate = x => (TSort) (dynamic) ~(byte) (dynamic) x;

            else if (type == typeof(int))
                _negate = x => (TSort) (dynamic) ~(int) (dynamic) x;

            else if (type == typeof(long))
                _negate = x => (TSort) (dynamic) ~(long) (dynamic) x;

            else
                throw new NotSupportedException($"Underlying enum type '{type}' not supported for conversion.");
        }

        readonly List<TSort> _list = new List<TSort>();
        readonly HashSet<TSort> _set = new HashSet<TSort>();

        public TSort[] Items => _list.ToArray();

        public SortingBuilder<TSort> Ascending(TSort sort)
        {
            if (!_set.Add(sort))
                throw new InvalidOperationException($"May not sort by '{sort}' more than once.");

            _list.Add(sort);
            return this;
        }

        public SortingBuilder<TSort> Descending(TSort sort)
        {
            if (!_set.Add(sort))
                throw new InvalidOperationException($"May not sort by '{sort}' more than once.");

            _list.Add(_negate(sort));
            return this;
        }
    }
}