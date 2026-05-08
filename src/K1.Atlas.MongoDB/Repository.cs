using System.Linq.Expressions;
using K1.Atlas.Domain.Repositories;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using SortDirection = K1.Atlas.Domain.Repositories.SortDirection;

namespace K1.Atlas.MongoDB;

public class Repository<T>(IMongoCollection<T> collection) : IRepository<T>
{
    protected readonly IQueryable<T> Queryable = collection.AsQueryable();

    public Task<bool> AnyAsync<TRes>(Func<IQueryable<T>, IQueryable<TRes>> builder,
        CancellationToken cancellationToken = default)
    {
        var query = builder(Queryable);
        return query.AnyAsync(cancellationToken);
    }

    public Task<bool> AnyAsync(Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return Queryable.AnyAsync(predicate, cancellationToken);
    }

    public Task<int> CountAsync<TRes>(Func<IQueryable<T>, IQueryable<TRes>> builder,
        CancellationToken cancellationToken = default)
    {
        var options = new AggregateOptions { AllowDiskUse = true };
        var query = builder(collection.AsQueryable(options));
        return query.CountAsync(cancellationToken);
    }

    public Task<int> CountAsync(Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        var options = new AggregateOptions { AllowDiskUse = true };
        return collection.AsQueryable(options).CountAsync(predicate, cancellationToken);
    }

    public Task DeleteAsync(Expression<Func<T, bool>> filter, CancellationToken cancellationToken = default)
    {
        return collection.DeleteOneAsync(filter, cancellationToken);
    }

    public Task<TRes> FirstOrDefaultAsync<TRes>(Func<IQueryable<T>, IQueryable<TRes>> builder,
        CancellationToken cancellationToken = default)
    {
        var query = builder(Queryable);
        return query.FirstOrDefaultAsync(cancellationToken);
    }


    public Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return Queryable.FirstOrDefaultAsync(predicate, cancellationToken);
    }

    public async Task SaveOrUpdateAsync(T obj, Expression<Func<T, bool>>? filtro = default,
        CancellationToken cancellationToken = default)
    {
        var idProp = obj!.GetType().GetProperty("Id");
        if (idProp is not null)
        {
            var value = idProp.GetValue(obj);

            if (value == null)
            {
                await collection.InsertOneAsync(obj, new InsertOneOptions(), cancellationToken);
                return;
            }

            var idFilter = Builders<T>.Filter.Eq("_id", value);
            await collection.ReplaceOneAsync(idFilter, obj, new ReplaceOptions { IsUpsert = true }, cancellationToken);
            return;
        }

        await collection.ReplaceOneAsync<T>(filtro, obj, new ReplaceOptions { IsUpsert = true }, cancellationToken);
    }

    public async Task<decimal> SumAsync(Func<IQueryable<T>, IQueryable<decimal>> builder,
        CancellationToken cancellationToken = default)
    {
        var options = new AggregateOptions { AllowDiskUse = true };
        var query = builder(collection.AsQueryable(options));
        var valor = await query.SumAsync(cancellationToken);

        return valor;
    }

    public async Task<decimal> SumAsync(Expression<Func<T, bool>> predicate, Expression<Func<T, decimal>> selector,
        CancellationToken cancellationToken = default)
    {
        var options = new AggregateOptions { AllowDiskUse = true };
        var valor = await collection.AsQueryable(options).Where(predicate).SumAsync(selector, cancellationToken);
        return valor;
    }

    public Task<List<TRes>> ToListAsync<TRes>(Func<IQueryable<T>, IQueryable<TRes>> builder,
        CancellationToken cancellationToken = default)
    {
        var options = new AggregateOptions { AllowDiskUse = true };
        var query = builder(collection.AsQueryable(options));
        return query.ToListAsync(cancellationToken);
    }

    public Task<List<T>> ToListAsync(Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        var options = new AggregateOptions { AllowDiskUse = true };
        return collection.AsQueryable(options).Where(predicate).ToListAsync(cancellationToken);
    }

    public async Task<(int totalPages, IReadOnlyList<T> readOnlyList)> ToListByPageAsync(ISortListByPage<T> definition)
    {
        var countFacet = CreateCountFacet();
        var dataFacet = CreateDataFacet(definition);

        var aggregation = await ExecuteAggregationAsync(definition, countFacet, dataFacet);

        var count = ExtractCountFromAggregation(aggregation);
        var totalPages = CalculateTotalPages(count, definition.PageSize);
        var data = ExtractDataFromAggregation(aggregation);

        return (totalPages, data);
    }

    public static AggregateFacet<T> CreateCountFacet()
    {
        return AggregateFacet.Create("count",
            PipelineDefinition<T, AggregateCountResult>.Create(new[]
            {
                PipelineStageDefinitionBuilder.Count<T>()
            }));
    }

    public AggregateFacet<T> CreateDataFacet(ISortListByPage<T> definition)
    {
        var sortDefinition = BuildSortDefinition(definition.SortConfigurations);

        return AggregateFacet.Create("data",
            PipelineDefinition<T, T>.Create(new[]
            {
                PipelineStageDefinitionBuilder.Sort(sortDefinition),
                PipelineStageDefinitionBuilder.Skip<T>((definition.Page - 1) * definition.PageSize),
                PipelineStageDefinitionBuilder.Limit<T>(definition.PageSize),
            }));
    }

    public static SortDefinition<T> BuildSortDefinition(IEnumerable<SortConfiguration<T>> sortConfigurations)
    {
        var sortDefinitions = sortConfigurations.Select(config =>
            config.Direction == SortDirection.Ascending
                ? Builders<T>.Sort.Ascending(config.SortExpression)
                : Builders<T>.Sort.Descending(config.SortExpression));

        return Builders<T>.Sort.Combine(sortDefinitions);
    }

    public async Task<List<AggregateFacetResults>> ExecuteAggregationAsync(
        ISortListByPage<T> definition,
        AggregateFacet<T> countFacet,
        AggregateFacet<T> dataFacet)
    {
        return await collection.Aggregate()
            .Match(definition.FilterExpression ?? Builders<T>.Filter.Empty)
            .Facet(countFacet, dataFacet)
            .ToListAsync();
    }

    public static long ExtractCountFromAggregation(List<AggregateFacetResults> aggregation)
    {
        return aggregation[0]
            .Facets.First(x => x.Name == "count")
            .Output<AggregateCountResult>()
            ?.FirstOrDefault()
            ?.Count ?? 0;
    }

    public static int CalculateTotalPages(long count, int pageSize)
    {
        return (int)count / pageSize;
    }

    public static IReadOnlyList<T> ExtractDataFromAggregation(List<AggregateFacetResults> aggregation)
    {
        return aggregation[0]
            .Facets.First(x => x.Name == "data")
            .Output<T>();
    }
}
