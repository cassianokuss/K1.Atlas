using K1.Atlas.Telemetry;
using K1.Atlas.Telemetry.Logging;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace K1.Atlas.UnitTest.Telemetry;

public class LoggingDecoratorTest
{
    private readonly Mock<IRequestHandler<TestRequest>> _innerHandlerMock;
    private readonly Mock<IRequestHandler<TestRequestWithResponse, TestResponse>> _innerHandlerWithResponseMock;
    private readonly Mock<ILogger<LoggingDecorator.RequestHandler<TestRequest>>> _loggerMock;
    private readonly Mock<ILogger<LoggingDecorator.RequestHandler<TestRequestWithResponse, TestResponse>>> _loggerWithResponseMock;

    public LoggingDecoratorTest()
    {
        _innerHandlerMock = new Mock<IRequestHandler<TestRequest>>();
        _innerHandlerWithResponseMock = new Mock<IRequestHandler<TestRequestWithResponse, TestResponse>>();
        _loggerMock = new Mock<ILogger<LoggingDecorator.RequestHandler<TestRequest>>>();
        _loggerWithResponseMock = new Mock<ILogger<LoggingDecorator.RequestHandler<TestRequestWithResponse, TestResponse>>>();
    }

    [Fact]
    public async Task RequestHandler_DeveLogarInicioEConclusao()
    {
        // Arrange
        var handler = new LoggingDecorator.RequestHandler<TestRequest>(
            _innerHandlerMock.Object,
            _loggerMock.Object);

        var request = new TestRequest { Name = "Test" };

        // Act
        await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        _innerHandlerMock.Verify(h => h.HandleAsync(request, It.IsAny<CancellationToken>()), Times.Once);
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Processing")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Completed")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task RequestHandler_QuandoOcorreExcecao_DeveLogarErro()
    {
        // Arrange
        var exception = new InvalidOperationException("Test error");
        _innerHandlerMock.Setup(h => h.HandleAsync(It.IsAny<TestRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        var handler = new LoggingDecorator.RequestHandler<TestRequest>(
            _innerHandlerMock.Object,
            _loggerMock.Object);

        var request = new TestRequest { Name = "Test" };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.HandleAsync(request, CancellationToken.None));

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task RequestHandlerWithResponse_DeveRetornarResposta()
    {
        // Arrange
        var expectedResponse = new TestResponse { Result = "Success" };
        _innerHandlerWithResponseMock.Setup(h => h.HandleAsync(It.IsAny<TestRequestWithResponse>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var handler = new LoggingDecorator.RequestHandler<TestRequestWithResponse, TestResponse>(
            _innerHandlerWithResponseMock.Object,
            _loggerWithResponseMock.Object);

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
        var exception = new InvalidOperationException("Test error");
        _innerHandlerWithResponseMock.Setup(h => h.HandleAsync(It.IsAny<TestRequestWithResponse>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        var handler = new LoggingDecorator.RequestHandler<TestRequestWithResponse, TestResponse>(
            _innerHandlerWithResponseMock.Object,
            _loggerWithResponseMock.Object);

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
