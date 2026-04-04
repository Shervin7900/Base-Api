using BaseApi.Features.Health;
using FastEndpoints;
using FluentAssertions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Moq;
using Microsoft.AspNetCore.Http;

namespace BaseApi.UnitTests.Features.Health;

public class HealthCheckEndpointTests
{
    private readonly Mock<HealthCheckService> _healthCheckServiceMock;

    public HealthCheckEndpointTests()
    {
        _healthCheckServiceMock = new Mock<HealthCheckService>();
    }

    [Fact]
    public async Task HandleAsync_ReturnsHealthy_WhenServiceIsHealthy()
    {
        // Arrange
        var entries = new Dictionary<string, HealthReportEntry>();
        var report = new HealthReport(entries, HealthStatus.Healthy, TimeSpan.FromMilliseconds(100));
        _healthCheckServiceMock.Setup(s => s.CheckHealthAsync(It.IsAny<Func<HealthCheckRegistration, bool>>(), It.IsAny<CancellationToken>())).ReturnsAsync(report);

        var endpoint = Factory.Create<HealthCheckEndpoint>(_healthCheckServiceMock.Object);

        // Act
        await endpoint.HandleAsync(default);
        var response = endpoint.Response;

        // Assert
        response.Should().NotBeNull();
        response.Status.Should().Be("Healthy");
        endpoint.HttpContext.Response.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task HandleAsync_ReturnsUnhealthy_WhenServiceIsUnhealthy()
    {
        // Arrange
        var entries = new Dictionary<string, HealthReportEntry>();
        var report = new HealthReport(entries, HealthStatus.Unhealthy, TimeSpan.FromMilliseconds(100));
        _healthCheckServiceMock.Setup(s => s.CheckHealthAsync(It.IsAny<Func<HealthCheckRegistration, bool>>(), It.IsAny<CancellationToken>())).ReturnsAsync(report);

        var endpoint = Factory.Create<HealthCheckEndpoint>(_healthCheckServiceMock.Object);

        // Act
        await endpoint.HandleAsync(default);
        var response = endpoint.Response;

        // Assert
        response.Should().NotBeNull();
        response.Status.Should().Be("Unhealthy");
        endpoint.HttpContext.Response.StatusCode.Should().Be(503);
    }
}
