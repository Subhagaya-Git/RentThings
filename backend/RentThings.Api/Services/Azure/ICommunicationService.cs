namespace RentThings.Api.Services.Azure;

public interface ICommunicationService
{
    Task SendEmailAsync(string to, string subject, string body, CancellationToken ct = default);
    Task SendSmsAsync(string phoneNumber, string message, CancellationToken ct = default);
    Task SendBookingConfirmationAsync(string email, string listingTitle, DateOnly start, DateOnly end, CancellationToken ct = default);
    Task SendBookingApprovedSmsAsync(string phone, string listingTitle, DateOnly start, DateOnly end, CancellationToken ct = default);
    Task SendBookingRejectedSmsAsync(string phone, string listingTitle, CancellationToken ct = default);
    Task SendReturnReminderSmsAsync(string phone, string listingTitle, DateOnly returnDate, CancellationToken ct = default);
    Task SendRentalReminderAsync(string email, string listingTitle, DateOnly returnDate, CancellationToken ct = default);
}
