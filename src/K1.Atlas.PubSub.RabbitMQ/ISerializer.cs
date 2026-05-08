namespace K1.Atlas.PubSub.Rabbit
{
    public interface ISerializer
    {
        string MimeType { get; }
        T Deserialize<T>(byte[] body);
        byte[] Serialize<T>(T obj);
    }
}
