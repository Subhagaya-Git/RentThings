using Azure;
using Azure.Communication.Sms;
using Microsoft.Extensions.Options;
using RentThings.Api.Configuration;

namespace RentThings.Api.Services.Azure;

public class AzureCommunicationService : ICommunicationService
{
    private readonly CommunicationSettings _settings;
    private readonly SmsClient _smsClient;
    private readonly ILogger<AzureCommunicationService> _logger;

    public AzureCommunicationService(IOptions<AzureSettings> options, ILogger<AzureCommunicationService> logger)
    {
        _settings = options.Value.Communication;
        _logger = logger;
        _smsClient = new SmsClient(_settings.ConnectionString);
    }

    public Task SendEmailAsync(string to, string subject, string body, CancellationToken ct = default)
    {
        // Email via ACS Email SDK can be added similarly; SMS is priority per requirements
        _logger.LogInformation("[ACS Email placeholder] To: {To}, Subject: {Subject}", to, subject);
        return Task.CompletedTask;
    }

    public async Task SendSmsAsync(string phoneNumber, string message, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber) || string.IsNullOrWhiteSpace(_settings.SenderPhone))
        {
            _logger.LogWarning("SMS skipped: missing phone or sender");
            return;
        }

        try
        {
            var response = await _smsClient.SendAsync(
                from: _settings.SenderPhone,
                to: phoneNumber,
                message: message,
                options: new SmsSendOptions(enableDeliveryReport: true),
                cancellationToken: ct);

            _logger.LogInformation("SMS sent to {Phone}, MessageId: {Id}", phoneNumber, response.Value.MessageId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send SMS to {Phone}", phoneNumber);
        }
    }

    public async Task SendBookingConfirmationAsync(string email, string listingTitle, DateOnly start, DateOnly end, CancellationToken ct = default)
    {
        await SendEmailAsync(email, "Booking Confirmed - RentThings",
            $"Your rental of '{listingTitle}' from {start} to {end} has been confirmed.", ct);
    }

    public async Task SendBookingApprovedSmsAsync(string phone, string listingTitle, DateOnly start, DateOnly end, CancellationToken ct = default)
    {
        await SendSmsAsync(phone,
            $"RentThings: Your booking for \"{listingTitle}\" ({start:MMM d}–{end:MMM d}) has been approved!", ct);
    }

    public async Task SendBookingRejectedSmsAsync(string phone, string listingTitle, CancellationToken ct = default)
    {
        await SendSmsAsync(phone,
            $"RentThings: Your booking request for \"{listingTitle}\" was declined. Browse other listings at rentthings.com", ct);
    }

    public async Task SendReturnReminderSmsAsync(string phone, string listingTitle, DateOnly returnDate, CancellationToken ct = default)
    {
        await SendSmsAsync(phone,
            $"RentThings reminder: Please return \"{listingTitle}\" by {returnDate:MMM d, yyyy}.", ct);
    }

    public async Task SendRentalReminderAsync(string email, string listingTitle, DateOnly returnDate, CancellationToken ct = default)
    {
        await SendEmailAsync(email, "Return Reminder - RentThings",
            $"Reminder: Please return '{listingTitle}' by {returnDate}.", ct);
    }
}
