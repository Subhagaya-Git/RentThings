namespace RentThings.Api.Configuration;

public class AzureSettings
{
    public const string SectionName = "Azure";

    public SqlSettings Sql { get; set; } = new();
    public BlobStorageSettings BlobStorage { get; set; } = new();
    public EntraIdSettings EntraId { get; set; } = new();
    public AiServicesSettings AiServices { get; set; } = new();
    public AiVisionSettings AiVision { get; set; } = new();
    public CommunicationSettings Communication { get; set; } = new();
    public FunctionsSettings Functions { get; set; } = new();
    public MapsSettings Maps { get; set; } = new();
    public SignalRSettings SignalR { get; set; } = new();
    public AzureTranslatorSettings AzureTranslator { get; set; } = new();
    public AzureIntegrationSettings Integration { get; set; } = new();
}

public class SqlSettings
{
    public string ConnectionString { get; set; } = string.Empty;
}

public class BlobStorageSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public string ContainerName { get; set; } = "listings";
    public string ProfileContainer { get; set; } = "profiles";
    public string VerificationContainer { get; set; } = "verification";
    /// <summary>Optional Azure CDN base URL, e.g. https://rentthings.azureedge.net</summary>
    public string CdnBaseUrl { get; set; } = string.Empty;
}

public class EntraIdSettings
{
    public string TenantId { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string Instance { get; set; } = "https://login.microsoftonline.com/";
    public string Domain { get; set; } = string.Empty;
}

public class AiServicesSettings
{
    public string Endpoint { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string DeploymentName { get; set; } = "gpt-4o";
}

public class AiVisionSettings
{
    public string Endpoint { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
}

public class CommunicationSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public string SenderEmail { get; set; } = "noreply@rentthings.com";
    public string SenderPhone { get; set; } = string.Empty;
}

public class FunctionsSettings
{
    public string BaseUrl { get; set; } = string.Empty;
    public string FunctionKey { get; set; } = string.Empty;
}

public class MapsSettings
{
    public string SubscriptionKey { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
}

public class SignalRSettings
{
    public string ConnectionString { get; set; } = string.Empty;
}

public class AzureTranslatorSettings
{
    public string ApiKey { get; set; } = string.Empty;
    public string Endpoint { get; set; } = "https://api.cognitive.microsofttranslator.com/";
    public string Region { get; set; } = string.Empty;
}
