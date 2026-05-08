using System.Diagnostics;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Driver;
using MongoDB.Driver.Core.Events;

namespace K1.Atlas.MongoDB;

public class MongoDbConnection
{
    public IMongoDatabase Database { get; set; }

    public MongoDbConnection(IOptions<DbConnectionConfig> config)
    {
        var url = $"mongodb://{config.Value?.Host}:{config.Value?.Port.ToString() ?? "27017"}?waitQueueMultiple=1000&maxPoolSize=1000";
        var mongoConnectionUrl = new MongoUrl(url);
        var mongoSettings = MongoClientSettings.FromUrl(mongoConnectionUrl);
        mongoSettings.ClusterConfigurator = cb =>
        {
            cb.Subscribe<CommandStartedEvent>(e =>
            {
                var comando = e.Command.ToJson(new JsonWriterSettings { Indent = true });
                if (!comando.ToLower().Contains(": \"history"))
                    Activity.Current?.AddEvent(new ActivityEvent("MongoDB", DateTimeOffset.Now, new ActivityTagsCollection
                {
                    new KeyValuePair<string, object?>($"Cmd", comando),
                    new KeyValuePair<string, object?>($"Con", e.ConnectionId),
                    new KeyValuePair<string, object?>($"DB", e.DatabaseNamespace)
                }));
            });
        };

        var client = new MongoClient(mongoSettings);
        Database = client.GetDatabase(config.Value?.Database);
    }

    protected MongoDbConnection()
    {
        Database = default!;
    }
}
