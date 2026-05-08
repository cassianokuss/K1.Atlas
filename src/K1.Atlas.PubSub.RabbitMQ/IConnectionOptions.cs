namespace K1.Atlas.PubSub.Rabbit
{
    public interface IConnectionOptions
    {
        string Host { get; }
        int? Port { get; }
        string? User { get; }
        string? Password { get; }
        string? VirtualHost { get; }
    }
}
