using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Nanoka.Models;
using Nest;

namespace Nanoka.Database
{
    public static class ElasticHelper
    {
        public sealed class QueryWrapper<T> where T : class
        {
            public readonly QueryContainerDescriptor<T> Descriptor;

            public QueryContainer Container;

            public QueryWrapper(QueryContainerDescriptor<T> descriptor)
            {
                Descriptor = descriptor;
                Container  = descriptor;
            }
        }

        public static SearchDescriptor<T> MultiQuery<T>(this SearchDescriptor<T> searchDesc,
                                                        Func<QueryWrapper<T>, QueryWrapper<T>> query) where T : class
            => searchDesc.Query(q => q.MultiQueryInternal(query));

        public static SearchDescriptor<T> NestedMultiQuery<T, TNested>(this SearchDescriptor<T> searchDesc,
                                                                       Expression<Func<T, TNested>> nestedPath,
                                                                       Func<QueryWrapper<T>, QueryWrapper<T>> query) where T : class where TNested : class
            => searchDesc.Query(
                searchQuery => searchQuery.Nested(
                    x => x.Path(nestedPath)
                          .Query(nestedQuery => nestedQuery.MultiQueryInternal(query))));

        static QueryContainer MultiQueryInternal<T>(this QueryContainerDescriptor<T> searchQuery,
                                                    Func<QueryWrapper<T>, QueryWrapper<T>> query) where T : class
            => searchQuery.Bool(boolQuery =>
            {
                boolQuery.Must(q => query(new QueryWrapper<T>(q)).Container);
                /*boolQuery.Should(q => query(new QueryWrapper<T>(q, QueryStrictness.Should)).Container);
                boolQuery.Filter(q => query(new QueryWrapper<T>(q, QueryStrictness.Filter)).Container);*/

                return boolQuery;
            });

        static QueryWrapper<T> QueryInternal<T>(this QueryWrapper<T> wrapper,
                                                ISearchQuery query,
                                                Func<QueryContainerDescriptor<T>, QueryContainer> createContainer) where T : class
        {
            if (!query.IsSpecified)
                return wrapper;

            wrapper.Container &= createContainer(wrapper.Descriptor);

            return wrapper;
        }

        public static QueryWrapper<T> Text<T>(this QueryWrapper<T> wrapper,
                                              TextQuery query,
                                              Expression<Func<T, object>> path = null) where T : class
            => wrapper.QueryInternal(
                query,
                descriptor =>
                {
                    var container = null as QueryContainer;

                    foreach (var value in query.Values)
                    {
                        /*switch (query.Mode)
                        {
                            default:*/

                        var c = descriptor.SimpleQueryString(q =>
                        {
                            // path = null signifies all fields
                            if (path != null)
                                q.Fields(f => f.Field(path));

                            q.Query(value);

                            return q;
                        });

/*                              break;

                            case TextQueryMode.Match:
                                if (paths.Length == 1)
                                    c = descriptor.Match(q => q.Field(paths[0])
                                                               .Query(value));
                                else
                                    c = descriptor.MultiMatch(q => q.Fields(paths)
                                                                    .Query(value));

                                break;

                            case TextQueryMode.Phrase:
                                c = descriptor.MatchPhrase(q => q.Field(paths[0]) // always use first
                                                                 .Query(value));

                                break;
                        }*/

                        if (container == null)
                            container = c;
                        else
                            switch (query.Mode)
                            {
                                default:
                                    container &= c;
                                    break;
                                case QueryMatchMode.Any:
                                    container |= c;
                                    break;
                            }
                    }

                    return container;
                });

        public static QueryWrapper<T> Filter<T, TField>(this QueryWrapper<T> wrapper,
                                                        FilterQuery<TField> query,
                                                        Expression<Func<T, object>> path) where T : class
            => wrapper.QueryInternal(
                query,
                descriptor =>
                {
                    var container = null as QueryContainer;

                    foreach (var value in query.Values)
                    {
                        var c = descriptor.Term(t => t.Field(path).Value(value));

                        if (container == null)
                            container = c;
                        else
                            switch (query.Mode)
                            {
                                default:
                                    container &= c;
                                    break;
                                case QueryMatchMode.Any:
                                    container |= c;
                                    break;
                            }
                    }

                    return container;
                });

        public static QueryWrapper<T> Range<T, TField>(this QueryWrapper<T> wrapper,
                                                       RangeQuery<TField> query,
                                                       Expression<Func<T, object>> path) where T : class where TField : struct
            => wrapper.QueryInternal(
                query,
                descriptor =>
                {
                    QueryContainer container;

                    switch (query)
                    {
                        case RangeQuery<DateTime> dateTimeRange:
                            container = descriptor.DateRange(q =>
                            {
                                q = q.Field(path);

                                if (dateTimeRange.Minimum != null)
                                    q = dateTimeRange.Exclusive
                                        ? q.GreaterThan(dateTimeRange.Minimum.Value)
                                        : q.GreaterThanOrEquals(dateTimeRange.Minimum.Value);

                                if (dateTimeRange.Maximum != null)
                                    q = dateTimeRange.Exclusive
                                        ? q.LessThan(dateTimeRange.Maximum.Value)
                                        : q.LessThanOrEquals(dateTimeRange.Maximum.Value);

                                return q;
                            });

                            break;

                        //todo: how to DRY
                        case RangeQuery<int> intRange:
                            container = descriptor.Range(q =>
                            {
                                q = q.Field(path);

                                if (intRange.Minimum != null)
                                    q = intRange.Exclusive
                                        ? q.GreaterThan(intRange.Minimum.Value)
                                        : q.GreaterThanOrEquals(intRange.Minimum.Value);

                                if (intRange.Maximum != null)
                                    q = intRange.Exclusive
                                        ? q.LessThan(intRange.Maximum.Value)
                                        : q.LessThanOrEquals(intRange.Maximum.Value);

                                return q;
                            });

                            break;

                        case RangeQuery<double> doubleRange:
                            container = descriptor.Range(q =>
                            {
                                q = q.Field(path);

                                if (doubleRange.Minimum != null)
                                    q = doubleRange.Exclusive
                                        ? q.GreaterThan(doubleRange.Minimum.Value)
                                        : q.GreaterThanOrEquals(doubleRange.Minimum.Value);

                                if (doubleRange.Maximum != null)
                                    q = doubleRange.Exclusive
                                        ? q.LessThan(doubleRange.Maximum.Value)
                                        : q.LessThanOrEquals(doubleRange.Maximum.Value);

                                return q;
                            });

                            break;

                        default: throw new NotSupportedException($"Unsupported range query type: {query.GetType()}");
                    }

                    return container;
                });

        public delegate Expression<Func<T, object>> AttributePathDelegate<T, in TAttribute>(TAttribute attribute);

        public static SearchDescriptor<T> MultiSort<T, TAttribute>(this SearchDescriptor<T> searchDesc,
                                                                   IReadOnlyList<TAttribute> attributes,
                                                                   AttributePathDelegate<T, TAttribute> path) where T : class where TAttribute : struct
        {
            if (attributes == null || attributes.Count == 0)
                return searchDesc;

            return searchDesc.Sort(s =>
            {
                var set = new HashSet<TAttribute>();

                foreach (var attr1 in attributes)
                {
                    var attr      = attr1;
                    var attrValue = (int) (object) attr;
                    var ascending = true;

                    // < 0 indicates descending
                    if (attrValue < 0)
                    {
                        attrValue = ~attrValue;
                        attr      = (TAttribute) (object) attrValue;
                        ascending = false;
                    }

                    // ensure not sorting by one field multiple times
                    if (!set.Add(attr))
                        continue;

                    var expr = path(attr);

                    if (attrValue != 0 && expr == null)
                        continue;

                    s = s.Field(f =>
                    {
                        // use 0 for relevance sorting
                        f = attrValue == 0
                            ? f.Field("_score")
                            : f.Field(expr);

                        // ordering
                        f = ascending
                            ? f.Ascending()
                            : f.Descending();

                        return f;
                    });
                }

                return s;
            });
        }
    }
}