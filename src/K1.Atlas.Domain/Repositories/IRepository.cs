using System.Linq.Expressions;

namespace K1.Atlas.Domain.Repositories;

public interface IRepository<T>
{
    Task<bool> AnyAsync<TRes>(Func<IQueryable<T>, IQueryable<TRes>> builder, CancellationToken cancellationToken = default);
    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    Task<int> CountAsync<TRes>(Func<IQueryable<T>, IQueryable<TRes>> builder, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    Task DeleteAsync(Expression<Func<T, bool>> filter, CancellationToken cancellationToken = default);
    Task<TRes> FirstOrDefaultAsync<TRes>(Func<IQueryable<T>, IQueryable<TRes>> builder, CancellationToken cancellationToken = default);
    Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    Task SaveOrUpdateAsync(T obj, Expression<Func<T, bool>>? filtro = default, CancellationToken cancellationToken = default);
    Task<decimal> SumAsync(Func<IQueryable<T>, IQueryable<decimal>> builder, CancellationToken cancellationToken = default);
    Task<decimal> SumAsync(Expression<Func<T, bool>> predicate, Expression<Func<T, decimal>> selector, CancellationToken cancellationToken = default);
    Task<List<TRes>> ToListAsync<TRes>(Func<IQueryable<T>, IQueryable<TRes>> builder, CancellationToken cancellationToken = default);
    Task<List<T>> ToListAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    Task<(int totalPages, IReadOnlyList<T> readOnlyList)> ToListByPageAsync(ISortListByPage<T> definition);
}
