namespace RentThings.Api.Services.Azure;

public class MockCommunicationService(ILogger<MockCommunicationService> logger) : ICommunicationService
{
    public Task SendEmailAsync(string to, string subject, string body, CancellationToken ct = default)
    {
        logger.LogInformation("[Mock ACS Email] To: {To}, Subject: {Subject}", to, subject);
        return Task.CompletedTask;
    }

    public Task SendSmsAsync(string phoneNumber, string message, CancellationToken ct = default)
    {
        logger.LogInformation("[Mock ACS SMS] To: {Phone}, Message: {Message}", phoneNumber, message);
        return Task.CompletedTask;
    }

    public Task SendBookingConfirmationAsync(string email, string listingTitle, DateOnly start, DateOnly end, CancellationToken ct = default)
        => SendEmailAsync(email, "Booking Confirmed - RentThings",
            $"Your rental of '{listingTitle}' from {start} to {end} has been confirmed.", ct);

    public Task SendBookingApprovedSmsAsync(string phone, string listingTitle, DateOnly start, DateOnly end, CancellationToken ct = default)
        => SendSmsAsync(phone, $"RentThings: Booking approved for \"{listingTitle}\" ({start}–{end})", ct);

    public Task SendBookingRejectedSmsAsync(string phone, string listingTitle, CancellationToken ct = default)
        => SendSmsAsync(phone, $"RentThings: Booking for \"{listingTitle}\" was declined.", ct);

    public Task SendReturnReminderSmsAsync(string phone, string listingTitle, DateOnly returnDate, CancellationToken ct = default)
        => SendSmsAsync(phone, $"RentThings: Return \"{listingTitle}\" by {returnDate}.", ct);

    public Task SendRentalReminderAsync(string email, string listingTitle, DateOnly returnDate, CancellationToken ct = default)
        => SendEmailAsync(email, "Return Reminder - RentThings",
            $"Reminder: Please return '{listingTitle}' by {returnDate}.", ct);
}
