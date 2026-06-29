# RentThings Azure Functions

Background jobs for the RentThings platform. Deploy to Azure Functions when ready.

## Functions

| Function | Schedule | Description |
|----------|----------|-------------|
| `RentalReminderFunction` | Daily | Sends rental start reminders via Azure Communication Services |
| `ReturnReminderFunction` | Daily | Sends return deadline reminders |
| `TrustScoreRecalculationFunction` | Weekly | Recalculates user trust scores |
| `ListingExpirationFunction` | Daily | Marks expired listings as inactive |

## Local development

These are placeholder implementations with clear interfaces. Install [Azure Functions Core Tools](https://learn.microsoft.com/azure/azure-functions/functions-run-local) and run:

```bash
cd functions/RentThings.Functions
func start
```

## Configuration

Set in `local.settings.json` (not committed):

- `AzureWebJobsStorage`
- `RentThingsApi__BaseUrl`
- `Azure__Communication__ConnectionString`
