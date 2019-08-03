using System;
using System.Linq.Expressions;
using Nanoka.Core.Models.Query;
using Nest;

namespace Nanoka.Web.Database
{
    public static class ElasticHelper
    {
        public sealed class QueryWrapper<T> where T : class
        {
            public readonly QueryContainerDescriptor<T> Descriptor;
            public readonly QueryStrictness Strictness;

            public QueryContainer Container;

            public QueryWrapper(QueryContainerDescriptor<T> descriptor, QueryStrictness strictness)
            {
                Descriptor = descriptor;
                Strictness = strictness;
                Container  = descriptor;
            }
        }

        public static SearchDescriptor<T> MultiQuery<T>(this SearchDescriptor<T> searchDesc,
                                                        Func<QueryWrapper<T>, QueryWrapper<T>> query)
            where T : class
            => searchDesc.Query(searchQuery => searchQuery.Bool(boolQuery =>
            {
                boolQuery.Must(q => query(new QueryWrapper<T>(q, QueryStrictness.Must)).Container);
                boolQuery.Should(q => query(new QueryWrapper<T>(q, QueryStrictness.Should)).Container);
                boolQuery.Filter(q => query(new QueryWrapper<T>(q, QueryStrictness.Filter)).Container);

                return boolQuery;
            }));

        static QueryWrapper<T> Query<T>(this QueryWrapper<T> wrapper,
                                        ISearchQuery query,
                                        Func<QueryContainerDescriptor<T>, QueryContainer> createContainer)
            where T : class
        {
            if (query.Strictness != wrapper.Strictness || !query.IsSpecified())
                return wrapper;

            var container = createContainer(wrapper.Descriptor);

            switch (query.Strictness)
            {
                case QueryStrictness.Must:
                case QueryStrictness.Filter:
                    wrapper.Container &= container;
                    break;

                case QueryStrictness.Should:
                    wrapper.Container |= container;
                    break;

                default: throw new NotSupportedException();
            }

            return wrapper;
        }

        public static QueryWrapper<T> Query<T>(this QueryWrapper<T> wrapper,
                                               TextQuery query,
                                               params Expression<Func<T, object>>[] paths)
            where T : class
            => wrapper.Query(
                query,
                descriptor =>
                {
                    var container = null as QueryContainer;

                    foreach (var value in query.Values)
                    {
                        QueryContainer c;

                        switch (query.Mode)
                        {
                            case TextQueryMode.Simple:
                                if (paths.Length == 1)
                                    c = descriptor.SimpleQueryString(q => q.Fields(f => f.Field(paths[0]))
                                                                           .Query(value));
                                else
                                    c = descriptor.SimpleQueryString(q => q.Fields(paths)
                                                                           .Query(value));

                                break;

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

                            default: throw new NotSupportedException();
                        }

                        if (container == null)
                            container = c;
                        else
                            switch (query.Operator)
                            {
                                case QueryOperator.All:
                                    container &= c;
                                    break;
                                case QueryOperator.Any:
                                case QueryOperator.None:
                                    container |= c;
                                    break;

                                default: throw new NotSupportedException();
                            }
                    }

                    if (query.Operator == QueryOperator.None)
                        container = !container;

                    return container;
                });

        public static QueryWrapper<T> Query<T, TField>(this QueryWrapper<T> wrapper,
                                                       FilterQuery<TField> query,
                                                       Expression<Func<T, object>> path)
            where T : class
            where TField : struct
            => wrapper.Query(
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
                            switch (query.Operator)
                            {
                                case QueryOperator.All:
                                    container &= c;
                                    break;
                                case QueryOperator.Any:
                                case QueryOperator.None:
                                    container |= c;
                                    break;

                                default: throw new NotSupportedException();
                            }
                    }

                    if (query.Operator == QueryOperator.None)
                        container = !container;

                    return container;
                });

        public static QueryWrapper<T> Query<T, TField>(this QueryWrapper<T> wrapper,
                                                       RangeQuery<TField> query,
                                                       Expression<Func<T, object>> path)
            where T : class
            where TField : struct
            => wrapper.Query(
                query,
                descriptor =>
                {
                    var container = null as QueryContainer;

                    foreach (var range in query.Values)
                    {
                        QueryContainer c;

                        switch (range)
                        {
                            case RangeQueryItem<DateTime> dateTimeRange:
                                c = descriptor.DateRange(q =>
                                {
                                    q = q.Field(path);

                                    if (dateTimeRange.Min != null)
                                        q = dateTimeRange.Exclusive
                                            ? q.GreaterThan(dateTimeRange.Min.Value)
                                            : q.GreaterThanOrEquals(dateTimeRange.Min.Value);

                                    if (dateTimeRange.Max != null)
                                        q = dateTimeRange.Exclusive
                                            ? q.LessThan(dateTimeRange.Max.Value)
                                            : q.LessThanOrEquals(dateTimeRange.Max.Value);

                                    return q;
                                });

                                break;

                            //todo: how to DRY
                            case RangeQueryItem<int> intRange:
                                c = descriptor.Range(q =>
                                {
                                    q = q.Field(path);

                                    if (intRange.Min != null)
                                        q = intRange.Exclusive
                                            ? q.GreaterThan(intRange.Min.Value)
                                            : q.GreaterThanOrEquals(intRange.Min.Value);

                                    if (intRange.Max != null)
                                        q = intRange.Exclusive
                                            ? q.LessThan(intRange.Max.Value)
                                            : q.LessThanOrEquals(intRange.Max.Value);

                                    return q;
                                });

                                break;

                            case RangeQueryItem<double> doubleRange:
                                c = descriptor.Range(q =>
                                {
                                    q = q.Field(path);

                                    if (doubleRange.Min != null)
                                        q = doubleRange.Exclusive
                                            ? q.GreaterThan(doubleRange.Min.Value)
                                            : q.GreaterThanOrEquals(doubleRange.Min.Value);

                                    if (doubleRange.Max != null)
                                        q = doubleRange.Exclusive
                                            ? q.LessThan(doubleRange.Max.Value)
                                            : q.LessThanOrEquals(doubleRange.Max.Value);

                                    return q;
                                });

                                break;

                            default: throw new NotSupportedException();
                        }

                        if (container == null)
                            container = c;
                        else
                            switch (query.Operator)
                            {
                                case QueryOperator.All:
                                    container &= c;
                                    break;
                                case QueryOperator.Any:
                                case QueryOperator.None:
                                    container |= c;
                                    break;

                                default: throw new NotSupportedException();
                            }
                    }

                    if (query.Operator == QueryOperator.None)
                        container = !container;

                    return container;
                });

        public delegate Expression<Func<T, object>> AttributePathDelegate<T, in TAttribute>(TAttribute attribute);

        public static SearchDescriptor<T> MultiSort<T, TAttribute>(this SearchDescriptor<T> searchDesc,
                                                                   TAttribute[] attributes,
                                                                   AttributePathDelegate<T, TAttribute> path)
            where T : class
            where TAttribute : struct
        {
            if (attributes == null || attributes.Length == 0)
                return searchDesc;

            return searchDesc.Sort(s =>
            {
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

                    s = s.Field(f =>
                    {
                        // use 0 for relevance sorting
                        f = attrValue == 0
                            ? f.Field("_score")
                            : f.Field(path(attr));

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