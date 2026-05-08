using System.Linq.Expressions;
using System.Reflection;
using MediatR;

namespace K1.Atlas.Telemetry;

public interface ITraceMap<TClass> where TClass : IBaseRequest
{
    IList<PropertyInfo> Properties { get; }
    ITraceMap<TClass> AddProperty<TField>(Expression<Func<TClass, TField>> property);
    ITraceMap<TClass> AutoMap();
}

public class TraceMap<TClass> : ITraceMap<TClass> where TClass : IBaseRequest
{
    public IList<PropertyInfo> Properties { get; } = default!;

    public TraceMap()
    {
        Properties = new List<PropertyInfo>();
    }

    public ITraceMap<TClass> AddProperty<TField>(Expression<Func<TClass, TField>> property)
    {
        var member = property.Body as MemberExpression;
        var propInfo = member!.Member as PropertyInfo;

        if (propInfo?.Name is null) throw new ArgumentNullException(nameof(propInfo.Name));
        Properties.Add(propInfo);
        return this;

    }

    public ITraceMap<TClass> AutoMap()
    {
        foreach (var property in typeof(TClass).GetProperties())
        {
            Properties.Add(property);
        }

        return this;
    }
}