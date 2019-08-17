namespace Nanoka.Models.Query
{
    public interface ISearchQuery
    {
        QueryStrictness Strictness { get; }
        QueryOperator Operator { get; }

        bool IsSpecified();
    }
}