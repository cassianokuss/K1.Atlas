using K1.Atlas.PubSub.Rabbit.Serializers;
using K1.Atlas.PubSub.Rabbit;

namespace K1.Atlas.UnitTest.RabbitMQ;

public class MessagePackSerializerTest
{
    private readonly MessagePackSerializer _serializer;

    public MessagePackSerializerTest()
    {
        _serializer = new MessagePackSerializer();
    }

    [Fact]
    public void MimeType_Should_Return_MessagePack_MimeType()
    {
        // Assert
        Assert.Equal("application/msgpack", _serializer.MimeType);
    }

    [Fact]
    public void Serialize_Should_Serialize_Object()
    {
        // Arrange
        var obj = new TestData { Id = 1, Name = "Test" };

        // Act
        var result = _serializer.Serialize(obj);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public void Deserialize_Should_Deserialize_Bytes()
    {
        // Arrange
        var original = new TestData { Id = 42, Name = "Test Object" };
        var serialized = _serializer.Serialize(original);

        // Act
        var result = _serializer.Deserialize<TestData>(serialized);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(original.Id, result.Id);
        Assert.Equal(original.Name, result.Name);
    }

    [Fact]
    public void Serialize_Deserialize_RoundTrip_Should_Work()
    {
        // Arrange
        var data = new TestData { Id = 123, Name = "Round Trip Test" };

        // Act
        var serialized = _serializer.Serialize(data);
        var deserialized = _serializer.Deserialize<TestData>(serialized);

        // Assert
        Assert.Equal(data.Id, deserialized.Id);
        Assert.Equal(data.Name, deserialized.Name);
    }

    [Fact]
    public void Serialize_Should_Handle_Complex_Objects()
    {
        // Arrange
        var complex = new ComplexData
        {
            Id = 1,
            Items = new List<string> { "item1", "item2", "item3" },
            Nested = new TestData { Id = 99, Name = "Nested" }
        };

        // Act
        var serialized = _serializer.Serialize(complex);
        var deserialized = _serializer.Deserialize<ComplexData>(serialized);

        // Assert
        Assert.Equal(complex.Id, deserialized.Id);
        Assert.Equal(3, deserialized.Items.Count);
        Assert.Equal("item1", deserialized.Items[0]);
        Assert.Equal(99, deserialized.Nested.Id);
        Assert.Equal("Nested", deserialized.Nested.Name);
    }

    [MessagePack.MessagePackObject]
    public class TestData
    {
        [MessagePack.Key(0)]
        public int Id { get; set; }

        [MessagePack.Key(1)]
        public string Name { get; set; } = string.Empty;
    }

    [MessagePack.MessagePackObject]
    public class ComplexData
    {
        [MessagePack.Key(0)]
        public int Id { get; set; }

        [MessagePack.Key(1)]
        public List<string> Items { get; set; } = new();

        [MessagePack.Key(2)]
        public TestData Nested { get; set; } = null!;
    }
}

public class SerializationManagerTest
{
    [Fact]
    public void Constructor_Should_Throw_When_No_Serializers_Provided()
    {
        // Arrange
        var serializers = Enumerable.Empty<ISerializer>();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new SerializationManagerImpl(serializers));
    }

    [Fact]
    public void DefaultMimeType_Should_Return_First_Serializer_MimeType()
    {
        // Arrange
        var serializer1 = new MessagePackSerializer();
        var serializers = new[] { serializer1 };

        // Act
        var manager = new SerializationManagerImpl(serializers);

        // Assert
        Assert.Equal("application/msgpack", manager.DefaultMimeType);
    }

    [Fact]
    public void Serialize_Should_Use_Specified_MimeType()
    {
        // Arrange
        var serializer = new MessagePackSerializer();
        var manager = new SerializationManagerImpl(new[] { serializer });
        var obj = new { Name = "Test" };

        // Act
        var result = manager.Serialize(obj, "application/msgpack");

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public void Deserialize_Should_Use_Specified_MimeType()
    {
        // Arrange
        var serializer = new MessagePackSerializer();
        var manager = new SerializationManagerImpl(new[] { serializer });
        var original = new TestDto { Value = "Test" };
        var serialized = manager.Serialize(original, "application/msgpack");

        // Act
        var result = manager.Deserialize<TestDto>(serialized, "application/msgpack");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(original.Value, result.Value);
    }

    [Fact]
    public void SerializeBatch_Should_Serialize_All_Items()
    {
        // Arrange
        var serializer = new MessagePackSerializer();
        var manager = new SerializationManagerImpl(new[] { serializer });
        var items = new[]
        {
            new TestDto { Value = "Item1" },
            new TestDto { Value = "Item2" },
            new TestDto { Value = "Item3" }
        };

        // Act
        var result = manager.SerializeBatch(items);

        // Assert
        Assert.Equal(3, result.Count);
        Assert.All(result, bytes => Assert.NotEmpty(bytes));
    }

    [Fact]
    public void Serialize_Should_Use_Default_When_MimeType_Not_Specified()
    {
        // Arrange
        var serializer = new MessagePackSerializer();
        var manager = new SerializationManagerImpl(new[] { serializer });
        var obj = new TestDto { Value = "Test" };

        // Act
        var result = manager.Serialize(obj);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [MessagePack.MessagePackObject]
    public class TestDto
    {
        [MessagePack.Key(0)]
        public string Value { get; set; } = string.Empty;
    }

    // Internal class wrapper for testing
    private class SerializationManagerImpl : ISerializationManager
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
            return serializer.Deserialize<T>(body);
        }

        public byte[] Serialize<T>(T obj, string? mimeType = null)
        {
            var serializer = GetSerializer(mimeType);
            return serializer.Serialize(obj);
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
            if (mimeType == null)
                return _default;

            if (_dic.TryGetValue(mimeType, out var serializer))
                return serializer;

            throw new NotSupportedException($"MimeType {mimeType} is not supported");
        }
    }
}
