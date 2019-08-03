namespace Nanoka.Core.Models.Query
{
    public interface ISearchQuery
    {
        QueryStrictness Strictness { get; }
        QueryOperator Operator { get; }

        bool IsSpecified();
    }
}
