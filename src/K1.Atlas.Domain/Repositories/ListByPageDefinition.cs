using System.Linq.Expressions;

namespace K1.Atlas.Domain.Repositories;

public class SortConfiguration<T>(Expression<Func<T, object>> sortExpression, SortDirection direction = SortDirection.Ascending)
{
    public Expression<Func<T, object>> SortExpression { get; } = sortExpression;
    public SortDirection Direction { get; } = direction;
}

public enum SortDirection
{
    Ascending,
    Descending
}

public class ListByPageDefinition<T> : ISortListByPage<T>
{
    public int Page { get; }
    public int PageSize { get; }
    public Expression<Func<T, bool>>? FilterExpression { get; private set; }
    public IList<SortConfiguration<T>> SortConfigurations { get; }

    public ListByPageDefinition(int page = 1, int pageSize = 50)
    {
        Page = page;
        PageSize = pageSize;
        SortConfigurations = new List<SortConfiguration<T>>();
    }

    public static IListByPage<T> Create(int page = 1, int pageSize = 50)
    {
        return new ListByPageDefinition<T>(page, pageSize);
    }

    ISortListByPage<T> ISortListByPage<T>.Filter(Expression<Func<T, bool>> filter)
    {
        FilterExpression = filter;
        return this;
    }

    ISortListByPage<T> IListByPage<T>.SortAscending(Expression<Func<T, object>> sort)
    {
        SortConfigurations.Add(new SortConfiguration<T>(sort));
        return this;
    }

    ISortListByPage<T> IListByPage<T>.SortDescending(Expression<Func<T, object>> sort)
    {
        SortConfigurations.Add(new SortConfiguration<T>(sort, SortDirection.Descending));
        return this;
    }
}

public interface IListByPage<T>
{
    public ISortListByPage<T> SortAscending(Expression<Func<T, object>> sort);
    public ISortListByPage<T> SortDescending(Expression<Func<T, object>> sort);
}

public interface ISortListByPage<T> : IListByPage<T>
{
    public int Page { get; }
    public int PageSize { get; }
    public Expression<Func<T, bool>>? FilterExpression { get; }
    public ISortListByPage<T> Filter(Expression<Func<T, bool>> filter);
    public IList<SortConfiguration<T>> SortConfigurations { get; }
}
