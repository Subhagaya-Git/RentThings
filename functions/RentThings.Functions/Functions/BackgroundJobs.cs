using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace RentThings.Functions;

/// <summary>
/// Sends rental start reminders via Azure Communication Services.
/// Replace HTTP placeholder with ACS SDK in production.
/// </summary>
public class RentalReminderFunction(ILogger<RentalReminderFunction> logger)
{
    [Function("RentalReminder")]
    public async Task Run([TimerTrigger("0 0 8 * * *")] TimerInfo timer)
    {
        logger.LogInformation("[Placeholder] RentalReminderFunction executed at {Time}", DateTime.UtcNow);
        // TODO: Query API for rentals starting tomorrow, send email/SMS via ACS
        await Task.CompletedTask;
    }
}

public class ReturnReminderFunction(ILogger<ReturnReminderFunction> logger)
{
    [Function("ReturnReminder")]
    public async Task Run([TimerTrigger("0 0 9 * * *")] TimerInfo timer)
    {
        logger.LogInformation("[Placeholder] ReturnReminderFunction executed at {Time}", DateTime.UtcNow);
        await Task.CompletedTask;
    }
}

public class TrustScoreRecalculationFunction(ILogger<TrustScoreRecalculationFunction> logger)
{
    [Function("TrustScoreRecalculation")]
    public async Task Run([TimerTrigger("0 0 2 * * 0")] TimerInfo timer)
    {
        logger.LogInformation("[Placeholder] TrustScoreRecalculationFunction executed at {Time}", DateTime.UtcNow);
        await Task.CompletedTask;
    }
}

public class ListingExpirationFunction(ILogger<ListingExpirationFunction> logger)
{
    [Function("ListingExpiration")]
    public async Task Run([TimerTrigger("0 0 1 * * *")] TimerInfo timer)
    {
        logger.LogInformation("[Placeholder] ListingExpirationFunction executed at {Time}", DateTime.UtcNow);
        await Task.CompletedTask;
    }
}
