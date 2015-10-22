namespace PlayverseMetrics.Infrastructure
{
    public interface IQueryExecutor<out TResponse>
    {
        TResponse Execute();
    }

    public interface IQueryExecutor<in TQuery, out TResponse>
    {
        TResponse Execute(TQuery query);
    }
}