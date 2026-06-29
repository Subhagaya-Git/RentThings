namespace RentThings.Api.Configuration;

public class AzureBlobSettings
{
    public const string Section = "Azure:BlobStorage";
    public string ConnectionString { get; set; } = string.Empty;
    public string AccountName { get; set; } = "rentthings";
    public string ListingsContainer { get; set; } = "listings";
    public string ProfilesContainer { get; set; } = "profiles";
}

public class AzureAiSettings
{
    public const string Section = "Azure:Ai";
    public string OpenAiEndpoint { get; set; } = string.Empty;
    public string OpenAiApiKey { get; set; } = string.Empty;
    public string OpenAiDeploymentName { get; set; } = "gpt-4o";
    public string VisionEndpoint { get; set; } = string.Empty;
    public string VisionApiKey { get; set; } = string.Empty;
}

public class EntraIdSettings
{
    public const string Section = "Azure:EntraId";
    public string TenantId { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string Instance { get; set; } = "https://login.microsoftonline.com/";
    public string Audience { get; set; } = string.Empty;
}

public class CommunicationSettings
{
    public const string Section = "Azure:Communication";
    public string ConnectionString { get; set; } = string.Empty;
    public string SenderEmail { get; set; } = "noreply@rentthings.com";
    public string SenderPhone { get; set; } = string.Empty;
}

public class DatabaseSettings
{
    public const string Section = "ConnectionStrings";
    public string DefaultConnection { get; set; } = string.Empty;
}
