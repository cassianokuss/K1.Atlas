namespace K1.Atlas.PubSub.Rabbit.Serializers
{
    public class MessagePackSerializer : ISerializer
    {
        public string MimeType => "application/msgpack";

        public T Deserialize<T>(byte[] body) => MessagePack.MessagePackSerializer.Deserialize<T>(body);
        public byte[] Serialize<T>(T obj) => MessagePack.MessagePackSerializer.Serialize(obj);
    }
}
