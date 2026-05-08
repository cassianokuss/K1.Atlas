using K1.Atlas.Domain.Repositories;

namespace K1.Atlas.UnitTest.Domain;

public class ListByPageDefinitionTest
{
    [Fact]
    public void Constructor_Should_Set_Default_Values()
    {
        // Arrange & Act
        var definition = new ListByPageDefinition<TestEntity>();

        // Assert
        Assert.Equal(1, definition.Page);
        Assert.Equal(50, definition.PageSize);
        Assert.Null(definition.FilterExpression);
        Assert.Empty(definition.SortConfigurations);
    }

    [Fact]
    public void Constructor_Should_Set_Custom_Values()
    {
        // Arrange & Act
        var definition = new ListByPageDefinition<TestEntity>(page: 3, pageSize: 100);

        // Assert
        Assert.Equal(3, definition.Page);
        Assert.Equal(100, definition.PageSize);
    }

    [Fact]
    public void Create_Should_Return_New_Instance()
    {
        // Arrange & Act
        var definition = ListByPageDefinition<TestEntity>.Create(2, 25);

        // Assert
        Assert.NotNull(definition);
        Assert.Equal(2, (definition as ListByPageDefinition<TestEntity>)?.Page);
        Assert.Equal(25, (definition as ListByPageDefinition<TestEntity>)?.PageSize);
    }

    [Fact]
    public void Filter_Should_Set_FilterExpression()
    {
        // Arrange
        var definition = new ListByPageDefinition<TestEntity>() as ISortListByPage<TestEntity>;

        // Act
        var result = definition.Filter(x => x.Id > 10);

        // Assert
        Assert.NotNull(result.FilterExpression);
        Assert.Same(definition, result);
    }

    [Fact]
    public void SortAscending_Should_Add_Sort_Configuration()
    {
        // Arrange
        var definition = new ListByPageDefinition<TestEntity>() as IListByPage<TestEntity>;

        // Act
        var result = definition.SortAscending(x => x.Name);

        // Assert
        Assert.Single((result as ListByPageDefinition<TestEntity>)!.SortConfigurations);
        var sortConfig = (result as ListByPageDefinition<TestEntity>)!.SortConfigurations.First();
        Assert.Equal(SortDirection.Ascending, sortConfig.Direction);
    }

    [Fact]
    public void SortDescending_Should_Add_Sort_Configuration()
    {
        // Arrange
        var definition = new ListByPageDefinition<TestEntity>() as IListByPage<TestEntity>;

        // Act
        var result = definition.SortDescending(x => x.Name);

        // Assert
        Assert.Single((result as ListByPageDefinition<TestEntity>)!.SortConfigurations);
        var sortConfig = (result as ListByPageDefinition<TestEntity>)!.SortConfigurations.First();
        Assert.Equal(SortDirection.Descending, sortConfig.Direction);
    }

    [Fact]
    public void Chaining_Methods_Should_Work()
    {
        // Arrange
        var definition = ListByPageDefinition<TestEntity>.Create(1, 10) as ISortListByPage<TestEntity>;

        // Act
        var result = definition
            .Filter(x => x.Id > 5)
            .SortAscending(x => x.Name)
            .SortDescending(x => x.Id);

        // Assert
        Assert.NotNull(result.FilterExpression);
        Assert.Equal(2, result.SortConfigurations.Count);
    }

    [Fact]
    public void SortConfiguration_Should_Store_Expression_And_Direction()
    {
        // Arrange & Act
        var config = new SortConfiguration<TestEntity>(x => x.Name, SortDirection.Descending);

        // Assert
        Assert.NotNull(config.SortExpression);
        Assert.Equal(SortDirection.Descending, config.Direction);
    }

    [Fact]
    public void SortConfiguration_Should_Default_To_Ascending()
    {
        // Arrange & Act
        var config = new SortConfiguration<TestEntity>(x => x.Name);

        // Assert
        Assert.Equal(SortDirection.Ascending, config.Direction);
    }
}

public class TestEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
