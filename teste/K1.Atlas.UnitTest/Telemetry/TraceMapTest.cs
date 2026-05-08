using K1.Atlas.Telemetry;
using MediatR;
using System.Reflection;

namespace K1.Atlas.UnitTest.Telemetry;

public class TraceMapTest
{
    private class TestRequest : IBaseRequest
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public string? Email { get; set; }
    }

    [Fact]
    public void Constructor_Should_Initialize_Empty_Properties_List()
    {
        // Arrange & Act
        var traceMap = new TraceMap<TestRequest>();

        // Assert
        Assert.NotNull(traceMap.Properties);
        Assert.Empty(traceMap.Properties);
    }

    [Fact]
    public void AddProperty_Should_Add_Property_To_List()
    {
        // Arrange
        var traceMap = new TraceMap<TestRequest>();

        // Act
        var result = traceMap.AddProperty(x => x.Name);

        // Assert
        Assert.Single(traceMap.Properties);
        Assert.Equal("Name", traceMap.Properties.First().Name);
        Assert.Same(traceMap, result);
    }

    [Fact]
    public void AddProperty_Should_Add_Multiple_Properties()
    {
        // Arrange
        var traceMap = new TraceMap<TestRequest>();

        // Act
        traceMap.AddProperty(x => x.Name)
                .AddProperty(x => x.Age)
                .AddProperty(x => x.Email);

        // Assert
        Assert.Equal(3, traceMap.Properties.Count);
        Assert.Contains(traceMap.Properties, p => p.Name == "Name");
        Assert.Contains(traceMap.Properties, p => p.Name == "Age");
        Assert.Contains(traceMap.Properties, p => p.Name == "Email");
    }

    [Fact]
    public void AddProperty_Should_Allow_Chaining()
    {
        // Arrange
        var traceMap = new TraceMap<TestRequest>();

        // Act
        var result = traceMap.AddProperty(x => x.Name)
                .AddProperty(x => x.Age);

        // Assert
        Assert.Equal(2, traceMap.Properties.Count);
        Assert.Same(traceMap, result);
    }

    [Fact]
    public void AutoMap_Should_Add_All_Properties()
    {
        // Arrange
        var traceMap = new TraceMap<TestRequest>();

        // Act
        var result = traceMap.AutoMap();

        // Assert
        var expectedPropertyCount = typeof(TestRequest).GetProperties().Length;
        Assert.Equal(expectedPropertyCount, traceMap.Properties.Count);
        Assert.Same(traceMap, result);
    }

    [Fact]
    public void AutoMap_Should_Include_All_Property_Names()
    {
        // Arrange
        var traceMap = new TraceMap<TestRequest>();

        // Act
        traceMap.AutoMap();

        // Assert
        Assert.Contains(traceMap.Properties, p => p.Name == "Name");
        Assert.Contains(traceMap.Properties, p => p.Name == "Age");
        Assert.Contains(traceMap.Properties, p => p.Name == "Email");
    }

    [Fact]
    public void AddProperty_And_AutoMap_Should_Work_Together()
    {
        // Arrange
        var traceMap = new TraceMap<TestRequest>();

        // Act
        traceMap.AddProperty(x => x.Name).AutoMap();

        // Assert
        // AutoMap adds all properties, including Name which was already added
        Assert.True(traceMap.Properties.Count >= 3);
    }

    [Fact]
    public void Properties_Should_Return_PropertyInfo_Objects()
    {
        // Arrange
        var traceMap = new TraceMap<TestRequest>();

        // Act
        traceMap.AddProperty(x => x.Name);

        // Assert
        Assert.All(traceMap.Properties, p => Assert.IsAssignableFrom<PropertyInfo>(p));
    }
}
