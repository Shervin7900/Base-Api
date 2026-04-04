using FastEndpoints;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace BaseApi.Features.Health;

public class HealthCheckResponse
{
    public string Status { get; set; } = string.Empty;
    public TimeSpan TotalDuration { get; set; }
    public IEnumerable<HealthCheckEntryResponse> Entries { get; set; } = Enumerable.Empty<HealthCheckEntryResponse>();
}

public class HealthCheckEntryResponse
{
    public string Component { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TimeSpan Duration { get; set; }
}

public class HealthCheckEndpoint : EndpointWithoutRequest<HealthCheckResponse>
{
    private readonly HealthCheckService _healthCheckService;

    public HealthCheckEndpoint(HealthCheckService healthCheckService)
    {
        _healthCheckService = healthCheckService;
    }

    public override void Configure()
    {
        Get("/api/health");
        AllowAnonymous();
        Description(x => x.WithTags("Infrastructure"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var report = await _healthCheckService.CheckHealthAsync(ct);

        var result = new HealthCheckResponse
        {
            Status = report.Status.ToString(),
            TotalDuration = report.TotalDuration,
            Entries = report.Entries.Select(e => new HealthCheckEntryResponse
            {
                Component = e.Key,
                Status = e.Value.Status.ToString(),
                Description = e.Value.Description,
                Duration = e.Value.Duration
            })
        };

        Response = result;
        if (report.Status == HealthStatus.Healthy)
        {
            await this.HttpContext.Response.SendAsync(result, 200, null, ct);
        }
        else
        {
            await this.HttpContext.Response.SendAsync(result, 503, null, ct);
        }
    }
}
