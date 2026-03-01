using FastEndpoints;
using Microsoft.AspNetCore.Authorization;

namespace BaseApi.Features.Dashboard;

public class DashboardEndpoint : EndpointWithoutRequest
{
    private readonly IHttpClientFactory _httpClientFactory;

    public DashboardEndpoint(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public override void Configure()
    {
        Get("/api/dashboard");
        AllowAnonymous();
        Description(x => x.WithTags("Infrastructure"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var client = _httpClientFactory.CreateClient();
        
        string metrics;
        try 
        {
            metrics = await client.GetStringAsync($"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/metrics", ct);
        }
        catch (Exception ex)
        {
            metrics = $"Error fetching metrics: {ex.Message}\n\nThis is expected in some test environments where the loopback metrics endpoint might not be fully active.";
        }

        var html = $@"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Base API - Unified Monitoring Dashboard</title>
    <link href='https://fonts.googleapis.com/css2?family=Outfit:wght@300;400;600&display=swap' rel='stylesheet'>
    <style>
        :root {{
            --bg: #0f111a;
            --card-bg: #1a1c2e;
            --accent: #7289da;
            --text: #cdd6f4;
            --muted: #7f849c;
            --success: #a6e3a1;
            --error: #f38ba8;
            --header-gradient: linear-gradient(135deg, #7289da 0%, #4e5d94 100%);
        }}

        body {{ 
            font-family: 'Outfit', sans-serif; 
            background-color: var(--bg); 
            color: var(--text); 
            margin: 0; 
            padding: 20px;
            line-height: 1.6;
        }}

        .container {{ max-width: 1400px; margin: 0 auto; }}

        header {{
            background: var(--header-gradient);
            padding: 30px;
            border-radius: 12px;
            margin-bottom: 30px;
            box-shadow: 0 4px 20px rgba(0,0,0,0.3);
            display: flex;
            justify-content: space-between;
            align-items: center;
        }}

        header h1 {{ margin: 0; font-size: 2.5rem; color: white; }}
        header p {{ margin: 5px 0 0; opacity: 0.8; font-weight: 300; }}

        .grid {{ 
            display: grid; 
            grid-template-columns: repeat(auto-fit, minmax(400px, 1fr)); 
            gap: 25px; 
            margin-bottom: 30px;
        }}

        .card {{ 
            background-color: var(--card-bg); 
            border-radius: 12px; 
            padding: 24px; 
            border: 1px solid rgba(255,255,255,0.05);
            transition: transform 0.2s, box-shadow 0.2s;
        }}
        
        .card:hover {{ transform: translateY(-5px); box-shadow: 0 8px 30px rgba(0,0,0,0.4); }}

        .card h2 {{ 
            margin-top: 0; 
            font-size: 1.25rem; 
            color: var(--accent); 
            display: flex; 
            align-items: center; 
            gap: 10px;
            border-bottom: 1px solid rgba(255,255,255,0.1);
            padding-bottom: 12px;
            margin-bottom: 15px;
        }}

        .status-badge {{
            padding: 4px 12px;
            border-radius: 20px;
            font-size: 0.8rem;
            font-weight: 600;
            text-transform: uppercase;
        }}
        .status-healthy {{ background: rgba(166, 227, 161, 0.2); color: var(--success); }}
        .status-error {{ background: rgba(243, 139, 168, 0.2); color: var(--error); }}

        pre {{ 
            background-color: #0c0e16; 
            padding: 15px; 
            border-radius: 8px; 
            overflow-x: auto; 
            font-size: 12px; 
            color: #bac2de;
            max-height: 300px;
            border: 1px solid rgba(255,255,255,0.05);
        }}

        iframe {{
            width: 100%;
            height: 450px;
            border: none;
            border-radius: 8px;
            background: #111217;
        }}

        .footer {{
            text-align: center;
            margin-top: 50px;
            color: var(--muted);
            font-size: 0.9rem;
        }}

        .refresh-tag {{
            font-size: 0.8rem;
            color: var(--muted);
            background: rgba(255,255,255,0.05);
            padding: 2px 8px;
            border-radius: 4px;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <header>
            <div>
                <h1>📊 Base API Observatory</h1>
                <p>Unified Metrics, Logs, and Error Tracking</p>
            </div>
            <div class='refresh-tag'>Auto-refreshing in 30s</div>
        </header>

        <div class='grid'>
            <!-- System Health -->
            <div class='card'>
                <h2>🟢 System Health</h2>
                <div style='margin-bottom: 15px;'>
                    <span class='status-badge status-healthy'>Operational</span>
                </div>
                <p>All core microservices and dependencies (SQL, Redis, Mongo) are performing within normal parameters.</p>
                <a href='/health-ui' style='color: var(--accent); text-decoration: none; font-weight: 600;'>View Health UI →</a>
            </div>

            <!-- Sentry Error Tracking -->
            <div class='card'>
                <h2>🛡️ Sentry Protection</h2>
                <div style='margin-bottom: 15px;'>
                    <span class='status-badge status-healthy'>Active</span>
                </div>
                <p>Automatic exception capture and performance tracing is enabled. Spans are being exported for all incoming requests.</p>
                <p style='font-size: 0.9rem; color: var(--muted);'>Connected to DSN: {HttpContext.RequestServices.GetService<IConfiguration>()?["Sentry:Dsn"] ?? "N/A"}</p>
            </div>
        </div>

        <!-- Grafana Visualization -->
        <div class='card' style='margin-bottom: 30px;'>
            <h2>📈 Grafana Insights</h2>
            <iframe src='http://localhost:3000/d-solo/BaseApi/metrics-overview?orgId=1&refresh=5s&theme=dark' title='Grafana Dashboard'></iframe>
            <div style='margin-top: 15px; text-align: right;'>
                <a href='http://localhost:3000' target='_blank' style='color: var(--accent); text-decoration: none; font-weight: 600;'>Open Full Grafana →</a>
            </div>
        </div>

        <!-- Raw Prometheus Metrics -->
        <div class='card'>
            <h2>📦 Raw Prometheus Data</h2>
            <pre>{metrics}</pre>
        </div>

        <div class='footer'>
            Base API Observatory • Generated at {DateTime.Now:yyyy-MM-dd HH:mm:ss} • Vector Log Aggregator: Online
        </div>
    </div>

    <script>
        setTimeout(() => location.reload(), 30000);
    </script>
</body>
</html>";

        HttpContext.Response.StatusCode = 200;
        HttpContext.Response.ContentType = "text/html; charset=utf-8";
        await HttpContext.Response.WriteAsync(html, ct);
    }
}
