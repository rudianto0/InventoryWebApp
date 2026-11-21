using System.Net.Http.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Inventory.IntegrationTests;

internal sealed record ThirdPartyApprovalRequest(Guid DocumentId, string Decision, string? Reason);

internal sealed class ThirdPartyApprovalStub : IAsyncDisposable
{
    private readonly WebApplication _app;
    private readonly StubState _state;

    private ThirdPartyApprovalStub(WebApplication app, string baseUrl, StubState state)
    {
        _app = app;
        BaseUrl = baseUrl;
        _state = state;
    }

    public string BaseUrl { get; }
    public ThirdPartyApprovalRequest? LastRequest => _state.LastRequest;

    public static async Task<ThirdPartyApprovalStub> StartAsync(
        Func<ThirdPartyApprovalRequest, Task>? onDecision = null,
        int? failWithStatus = null)
    {
        var builder = WebApplication.CreateBuilder();
        builder.Logging.ClearProviders();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        var app = builder.Build();
        var state = new StubState();

        app.MapPost("/api/approvals", async context =>
        {
            if (context.Request.Headers["X-Api-Key"].ToString() != "test-key")
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }

            var request = await context.Request.ReadFromJsonAsync<ThirdPartyApprovalRequest>();
            if (request is null)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                return;
            }

            state.LastRequest = request;

            if (failWithStatus is int status)
            {
                context.Response.StatusCode = status;
                return;
            }

            if (onDecision is not null)
            {
                try
                {
                    await onDecision(request);
                }
                catch (Exception ex)
                {
                    context.Response.StatusCode = StatusCodes.Status409Conflict;
                    await context.Response.WriteAsJsonAsync(new { error = ex.Message });
                    return;
                }
            }

            await context.Response.WriteAsJsonAsync(new { approved = request.Decision == "approve" });
        });

        await app.StartAsync();
        var url = app.Services.GetRequiredService<IServer>()
            .Features.Get<IServerAddressesFeature>()!
            .Addresses.First();
        return new ThirdPartyApprovalStub(app, url, state);
    }

    public async ValueTask DisposeAsync()
    {
        await _app.StopAsync();
        await _app.DisposeAsync();
    }

    private sealed class StubState
    {
        public ThirdPartyApprovalRequest? LastRequest { get; set; }
    }
}

internal sealed class ThirdPartyApprovalClient(HttpClient http)
{
    public Task<HttpResponseMessage> SendAsync(
        ThirdPartyApprovalRequest request,
        string apiKey = "test-key",
        CancellationToken ct = default)
    {
        var message = new HttpRequestMessage(HttpMethod.Post, "/api/approvals")
        {
            Content = JsonContent.Create(request)
        };
        message.Headers.Add("X-Api-Key", apiKey);
        return http.SendAsync(message, ct);
    }
}
