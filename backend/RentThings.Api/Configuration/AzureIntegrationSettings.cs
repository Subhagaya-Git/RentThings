namespace RentThings.Api.Configuration;

/// <summary>
/// Feature flags for Azure integrations. When false, mock implementations are used (local dev).
/// Set individual flags to true in appsettings.Production.json or user secrets.
/// </summary>
public class AzureIntegrationSettings
{
    public const string SectionName = "Azure:Integration";

    public bool UseRealBlobStorage { get; set; }
    public bool UseRealAiVision { get; set; }
    public bool UseRealCommunication { get; set; }
    public bool UseRealMaps { get; set; }
    /// <summary>When true, uses Azure SignalR Service; when false, uses in-process SignalR (local dev).</summary>
    public bool UseAzureSignalR { get; set; }
}
