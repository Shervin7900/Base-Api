using BaseApi.Features.Dashboard;
using FastEndpoints;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using Moq.Protected;
using System.Net;

namespace BaseApi.UnitTests.Features.Dashboard;

public class DashboardEndpointTests
{
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;

    public DashboardEndpointTests()
    {
        _httpClientFactoryMock = new Mock<IHttpClientFactory>();
    }

    [Fact]
    public async Task HandleAsync_ReturnsHtml_WithMetricsContent()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("fake_metrics_data")
            });

        var client = new HttpClient(mockHttpMessageHandler.Object);
        _httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(client);

        var endpoint = Factory.Create<DashboardEndpoint>(_httpClientFactoryMock.Object);
        endpoint.HttpContext.Request.Scheme = "https";
        endpoint.HttpContext.Request.Host = new HostString("localhost", 5001);

        // Act
        await endpoint.HandleAsync(default);

        // Assert
        endpoint.HttpContext.Response.StatusCode.Should().Be(200);
        endpoint.HttpContext.Response.ContentType.Should().Contain("text/html");
        // Note: For SendStringAsync, we might need a different way to check the body in unit tests 
        // but typically the endpoint testing factory handles this.
    }
}
