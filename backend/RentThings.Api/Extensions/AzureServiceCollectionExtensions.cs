using RentThings.Api.Configuration;
using RentThings.Api.Services.Azure;

namespace RentThings.Api.Extensions;

public static class AzureServiceCollectionExtensions
{
    public static IServiceCollection AddRentThingsAzureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AzureSettings>(configuration.GetSection(AzureSettings.SectionName));
        services.PostConfigure<AzureSettings>(settings => ApplyLegacyBlobStorageFallback(configuration, settings));
        var azure = configuration.GetSection(AzureSettings.SectionName).Get<AzureSettings>() ?? new();
        ApplyLegacyBlobStorageFallback(configuration, azure);
        var integration = azure.Integration;

        // 1. Azure Blob Storage
        if (integration.UseRealBlobStorage && IsBlobConfigured(azure.BlobStorage.ConnectionString))
            services.AddSingleton<IBlobStorageService, AzureBlobStorageService>();
        else
            services.AddScoped<IBlobStorageService, MockBlobStorageService>();

        // 2. Azure AI Vision (+ Content Safety for flagged content)
        if (integration.UseRealAiVision && IsConfigured(azure.AiVision.Endpoint) && IsConfigured(azure.AiVision.ApiKey))
            services.AddScoped<IAiVisionService, AzureAiVisionService>();
        else
            services.AddScoped<IAiVisionService, MockAiVisionService>();

        // 3. Azure Communication Services (SMS)
        if (integration.UseRealCommunication && IsConfigured(azure.Communication.ConnectionString))
            services.AddScoped<ICommunicationService, AzureCommunicationService>();
        else
            services.AddScoped<ICommunicationService, MockCommunicationService>();

        // 4. Azure Maps
        if (integration.UseRealMaps && IsConfigured(azure.Maps.SubscriptionKey))
            services.AddHttpClient<IMapsService, AzureMapsService>();
        else
            services.AddScoped<IMapsService, MockMapsService>();

        // AI Language — mock until Azure OpenAI credentials are configured
        services.AddScoped<IAiServicesClient, MockAiServicesClient>();

        // Entra ID — keep mock unless configured
        if (IsConfigured(azure.EntraId.TenantId) && IsConfigured(azure.EntraId.ClientId))
            services.AddScoped<IEntraIdService, MockEntraIdService>(); // swap with EntraIdService when ready
        else
            services.AddScoped<IEntraIdService, MockEntraIdService>();

        services.AddScoped<INotificationPublisher, NotificationPublisher>();

        return services;
    }

    public static IServiceCollection AddRentThingsSignalR(this IServiceCollection services, IConfiguration configuration)
    {
        var azure = configuration.GetSection(AzureSettings.SectionName).Get<AzureSettings>() ?? new();

        var signalR = services.AddSignalR();
        if (azure.Integration.UseAzureSignalR && IsConfigured(azure.SignalR.ConnectionString))
            signalR.AddAzureSignalR(azure.SignalR.ConnectionString);

        return services;
    }

    private static bool IsConfigured(string? value) =>
        !string.IsNullOrWhiteSpace(value) &&
        !value.StartsWith("your-", StringComparison.OrdinalIgnoreCase);

    private static bool IsBlobConfigured(string? value) =>
        IsConfigured(value) || value == "UseDevelopmentStorage=true";

    /// <summary>
    /// Supports legacy root-level AzureStorage section (Azure:BlobStorage is canonical).
    /// </summary>
    private static void ApplyLegacyBlobStorageFallback(IConfiguration configuration, AzureSettings azure)
    {
        if (IsBlobConfigured(azure.BlobStorage.ConnectionString)) return;

        var legacyConnection = configuration["AzureStorage:ConnectionString"];
        if (!IsBlobConfigured(legacyConnection)) return;

        azure.BlobStorage.ConnectionString = legacyConnection!;
        var legacyContainer = configuration["AzureStorage:ContainerName"];
        if (IsConfigured(legacyContainer))
            azure.BlobStorage.ContainerName = legacyContainer!;
    }
}
