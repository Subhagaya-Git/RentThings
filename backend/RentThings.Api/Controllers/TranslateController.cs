using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using RentThings.Api.Configuration;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace RentThings.Api.Controllers;

public class TranslationRequest
{
    public string Text { get; set; } = string.Empty;
    public string TargetLanguage { get; set; } = string.Empty; // expected values: 'en', 'si', 'ta'
}

[ApiController]
[Route("api/translate")]
public class TranslateController(
    IOptions<AzureSettings> azureOptions,
    IHttpClientFactory httpClientFactory) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Translate([FromBody] TranslationRequest request, CancellationToken ct)
    {
        var translator = azureOptions.Value.AzureTranslator;
        var endpoint = translator.Endpoint;
        var apiKey = translator.ApiKey;
        var region = translator.Region;

        if (string.IsNullOrEmpty(endpoint) || string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(region))
            return StatusCode(500, "Azure Translator configuration is missing.");

        if (string.IsNullOrWhiteSpace(request.Text))
            return BadRequest("Text is required.");

        var route = $"/translate?api-version=3.0&to={Uri.EscapeDataString(request.TargetLanguage)}";
        var requestUrl = endpoint.TrimEnd('/') + route;

        var body = new object[] { new { Text = request.Text } };
        var requestBody = JsonSerializer.Serialize(body);

        using var requestMessage = new HttpRequestMessage(HttpMethod.Post, requestUrl)
        {
            Content = new StringContent(requestBody, Encoding.UTF8, "application/json")
        };

        requestMessage.Headers.Add("Ocp-Apim-Subscription-Key", apiKey);
        requestMessage.Headers.Add("Ocp-Apim-Subscription-Region", region);

        var client = httpClientFactory.CreateClient();
        var response = await client.SendAsync(requestMessage, ct);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(ct);
            return StatusCode((int)response.StatusCode, errorContent);
        }

        var result = await response.Content.ReadAsStringAsync(ct);
        return Content(result, "application/json");
    }
}
