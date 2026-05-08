namespace K1.Atlas.PubSub.Rabbit
{
    public interface ISerializationManager
    {
        string DefaultMimeType { get; }
        T Deserialize<T>(byte[] body, string? mimeType = null);
        byte[] Serialize<T>(T obj, string? mimeType = null);
        List<byte[]> SerializeBatch<T>(IEnumerable<T> enumerable, string? mimeType = null);
    }

    internal class SerializationManagerImpl : ISerializationManager
    {
        private readonly Dictionary<string, ISerializer> _dic;
        private readonly ISerializer _default;

        public SerializationManagerImpl(IEnumerable<ISerializer> serializers)
        {
            if (!serializers.Any())
                throw new ArgumentException("It's necessary to register at least one serializer", nameof(serializers));

            _dic = serializers.ToDictionary(key => key.MimeType);
            _default = serializers.First();
        }

        public string DefaultMimeType => _default.MimeType;

        public T Deserialize<T>(byte[] body, string? mimeType = null)
        {
            var serializer = GetSerializer(mimeType);
            var obj = serializer.Deserialize<T>(body);

            return obj;
        }

        public byte[] Serialize<T>(T obj, string? mimeType = null)
        {
            var serializer = GetSerializer(mimeType);
            var result = serializer.Serialize(obj);

            return result;
        }

        public List<byte[]> SerializeBatch<T>(IEnumerable<T> enumerable, string? mimeType = null)
        {
            var serializer = GetSerializer(mimeType);
            var list = new List<byte[]>();
            foreach (var item in enumerable)
            {
                list.Add(serializer.Serialize(item));
            }

            return list;
        }

        private ISerializer GetSerializer(string? mimeType)
        {
            if (mimeType is null)
                return _default;

            return _dic[mimeType];
        }
    }
}
