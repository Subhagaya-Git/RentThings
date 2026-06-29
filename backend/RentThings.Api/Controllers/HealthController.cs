using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RentThings.Api.Configuration;
using RentThings.Api.Data;
using RentThings.Api.Services.Azure;

namespace RentThings.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController(
    RentThingsDbContext db,
    IOptions<AzureSettings> azureOptions,
    IConfiguration configuration,
    IBlobStorageService blobStorage,
    IAiVisionService vision,
    ICommunicationService communication,
    IMapsService maps,
    IEntraIdService entra) : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(new { status = "healthy", timestamp = DateTime.UtcNow });

    [HttpGet("azure")]
    public async Task<IActionResult> AzureStatus(CancellationToken ct)
    {
        var azure = azureOptions.Value;
        var integration = azure.Integration;
        var useInMemory = configuration.GetValue<bool>("UseInMemoryDatabase");
        var appInsightsConfigured = !string.IsNullOrWhiteSpace(configuration["ApplicationInsights:ConnectionString"]);

        string? dbStatus = null;
        try
        {
            dbStatus = useInMemory ? "in-memory" : (await db.Database.CanConnectAsync(ct) ? "connected" : "unreachable");
        }
        catch (Exception ex)
        {
            dbStatus = $"error: {ex.Message}";
        }

        return Ok(new
        {
            database = new
            {
                provider = useInMemory ? "InMemory" : "SqlServer",
                status = dbStatus
            },
            applicationInsights = new { configured = appInsightsConfigured },
            services = new
            {
                blobStorage = ServiceInfo(integration.UseRealBlobStorage, azure.BlobStorage.ConnectionString, blobStorage.GetType().Name),
                aiVision = ServiceInfo(integration.UseRealAiVision, $"{azure.AiVision.Endpoint}|{azure.AiVision.ApiKey}", vision.GetType().Name),
                communication = ServiceInfo(integration.UseRealCommunication, azure.Communication.ConnectionString, communication.GetType().Name),
                maps = ServiceInfo(integration.UseRealMaps, azure.Maps.SubscriptionKey, maps.GetType().Name, mockWhenDisabled: true),
                signalR = ServiceInfo(integration.UseAzureSignalR, azure.SignalR.ConnectionString, integration.UseAzureSignalR && IsConfigured(azure.SignalR.ConnectionString) ? "AzureSignalR" : "InProcess"),
                translator = ServiceInfo(true, $"{azure.AzureTranslator.Endpoint}|{azure.AzureTranslator.ApiKey}|{azure.AzureTranslator.Region}", "AzureTranslator"),
                entraId = new
                {
                    implementation = entra.GetType().Name,
                    tenantConfigured = IsConfigured(azure.EntraId.TenantId),
                    clientConfigured = IsConfigured(azure.EntraId.ClientId),
                    note = "JWT auth uses ClientId/ClientSecret; Microsoft login popup requires MSAL on frontend."
                },
                functions = new
                {
                    configured = IsConfigured(azure.Functions.BaseUrl),
                    hasKey = IsConfigured(azure.Functions.FunctionKey),
                    note = "Separate Azure Functions project; not invoked by API at runtime."
                },
                aiLanguage = new { implementation = "MockAiServicesClient", note = "Configure Azure:AiServices for OpenAI." }
            }
        });
    }

    private static object ServiceInfo(bool useReal, string? configValue, string implementation, bool mockWhenDisabled = false)
    {
        var parts = configValue?.Split('|') ?? [];
        var configured = parts.Length > 0 && parts.All(IsConfigured);
        return new
        {
            useReal,
            configured,
            implementation,
            active = mockWhenDisabled && !useReal ? $"{implementation} (mock fallback)" : implementation
        };
    }

    private static bool IsConfigured(string? value) =>
        !string.IsNullOrWhiteSpace(value) &&
        !value.StartsWith("your-", StringComparison.OrdinalIgnoreCase);
}
