using System.Diagnostics;
using K1.Atlas.Telemetry;
using MediatR;
using Moq;
using Xunit;

namespace K1.Atlas.UnitTest.Telemetry;

public class ActivityDecoratorTest
{
    private readonly ActivitySource _activitySource;
    private readonly Mock<IRequestHandler<TestRequest>> _innerHandlerMock;
    private readonly Mock<IRequestHandler<TestRequestWithResponse, TestResponse>> _innerHandlerWithResponseMock;

    public ActivityDecoratorTest()
    {
        _activitySource = new ActivitySource("TestActivitySource");
        _innerHandlerMock = new Mock<IRequestHandler<TestRequest>>();
        _innerHandlerWithResponseMock = new Mock<IRequestHandler<TestRequestWithResponse, TestResponse>>();
    }

    [Fact]
    public async Task RequestHandler_SemTraceMap_DeveCriarActivityComNomeCorreto()
    {
        // Arrange
        using var listener = new ActivityListener
        {
            ShouldListenTo = _ => true,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData
        };
        ActivitySource.AddActivityListener(listener);

        var handler = new ActivityDecorator.RequestHandler<TestRequest>(
            _innerHandlerMock.Object,
            _activitySource);

        var request = new TestRequest { Name = "Test" };

        // Act
        await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        _innerHandlerMock.Verify(h => h.HandleAsync(request, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RequestHandler_ComTraceMap_DeveAdicionarPropriedadesNaActivity()
    {
        // Arrange
        using var listener = new ActivityListener
        {
            ShouldListenTo = _ => true,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData
        };
        ActivitySource.AddActivityListener(listener);

        var traceMapMock = new Mock<ITraceMap<TestRequest>>();
        traceMapMock.Setup(tm => tm.Properties)
            .Returns([typeof(TestRequest).GetProperty(nameof(TestRequest.Name))!]);

        var handler = new ActivityDecorator.RequestHandler<TestRequest>(
            _innerHandlerMock.Object,
            _activitySource,
            traceMapMock.Object);

        var request = new TestRequest { Name = "Test" };

        // Act
        await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        _innerHandlerMock.Verify(h => h.HandleAsync(request, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RequestHandler_QuandoOcorreExcecao_DeveLancarExcecao()
    {
        // Arrange
        using var listener = new ActivityListener
        {
            ShouldListenTo = _ => true,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData
        };
        ActivitySource.AddActivityListener(listener);

        var exception = new InvalidOperationException("Test error");
        _innerHandlerMock.Setup(h => h.HandleAsync(It.IsAny<TestRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        var handler = new ActivityDecorator.RequestHandler<TestRequest>(
            _innerHandlerMock.Object,
            _activitySource);

        var request = new TestRequest { Name = "Test" };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.HandleAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task RequestHandlerWithResponse_DeveRetornarResposta()
    {
        // Arrange
        using var listener = new ActivityListener
        {
            ShouldListenTo = _ => true,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData
        };
        ActivitySource.AddActivityListener(listener);

        var expectedResponse = new TestResponse { Result = "Success" };
        _innerHandlerWithResponseMock.Setup(h => h.HandleAsync(It.IsAny<TestRequestWithResponse>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var handler = new ActivityDecorator.RequestHandler<TestRequestWithResponse, TestResponse>(
            _innerHandlerWithResponseMock.Object,
            _activitySource);

        var request = new TestRequestWithResponse { Name = "Test" };

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.Equal(expectedResponse, result);
        _innerHandlerWithResponseMock.Verify(h => h.HandleAsync(request, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RequestHandlerWithResponse_QuandoOcorreExcecao_DeveLancarExcecao()
    {
        // Arrange
        using var listener = new ActivityListener
        {
            ShouldListenTo = _ => true,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData
        };
        ActivitySource.AddActivityListener(listener);

        var exception = new InvalidOperationException("Test error");
        _innerHandlerWithResponseMock.Setup(h => h.HandleAsync(It.IsAny<TestRequestWithResponse>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        var handler = new ActivityDecorator.RequestHandler<TestRequestWithResponse, TestResponse>(
            _innerHandlerWithResponseMock.Object,
            _activitySource);

        var request = new TestRequestWithResponse { Name = "Test" };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.HandleAsync(request, CancellationToken.None));
    }

    public record TestRequest : IRequest
    {
        public string Name { get; init; } = string.Empty;
    }

    public record TestRequestWithResponse : IRequest<TestResponse>
    {
        public string Name { get; init; } = string.Empty;
    }

    public record TestResponse
    {
        public string Result { get; init; } = string.Empty;
    }
}
