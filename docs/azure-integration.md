# Azure Integration Guide — RentThings

Each Azure service has a **real implementation** and a **mock fallback**, toggled via feature flags in `appsettings.json` under `Azure:Integration`.

## Feature flags (local dev defaults)

```json
"Azure": {
  "Integration": {
    "UseRealBlobStorage": false,
    "UseRealAiVision": false,
    "UseRealCommunication": false,
    "UseRealMaps": false,
    "UseAzureSignalR": false
  }
}
```

Set individual flags to `true` and provide credentials to enable real Azure services.

---

## 1. Azure Blob Storage

### NuGet
```bash
dotnet add package Azure.Storage.Blobs
```

### Real service
`Services/Azure/AzureBlobStorageService.cs` — uploads to containers, returns CDN URLs when configured.

### DI registration (`Extensions/AzureServiceCollectionExtensions.cs`)
```csharp
if (integration.UseRealBlobStorage && IsBlobConfigured(azure.BlobStorage.ConnectionString))
    services.AddSingleton<IBlobStorageService, AzureBlobStorageService>();
else
    services.AddScoped<IBlobStorageService, MockBlobStorageService>();
```

### Configuration
```json
"Azure": {
  "Integration": { "UseRealBlobStorage": true },
  "BlobStorage": {
    "ConnectionString": "DefaultEndpointsProtocol=https;AccountName=...;AccountKey=...;EndpointSuffix=core.windows.net",
    "ContainerName": "listings",
    "ProfileContainer": "profiles",
    "VerificationContainer": "verification",
    "CdnBaseUrl": "https://rentthings.azureedge.net"
  }
}
```

**Local Azurite:** `"ConnectionString": "UseDevelopmentStorage=true"`

### Frontend
No npm package. Uploads via `listingsApi.uploadImage()` → `POST /api/listings/{id}/images`.

---

## 2. Azure AI Vision

### NuGet
```bash
dotnet add package Azure.AI.Vision.ImageAnalysis
dotnet add package Azure.AI.ContentSafety
```

### Real service
`Services/Azure/AzureAiVisionService.cs` — tags, caption, objects, category mapping, Content Safety for flagged images.

### Response shape
```json
{
  "isValid": true,
  "category": "Cameras",
  "subcategory": "camera",
  "tags": ["camera", "lens", "photography"],
  "confidence": 0.92,
  "flagged": false
}
```

### Configuration
```json
"Azure": {
  "Integration": { "UseRealAiVision": true },
  "AiVision": {
    "Endpoint": "https://YOUR-RESOURCE.cognitiveservices.azure.com/",
    "ApiKey": "YOUR-KEY"
  }
}
```

### Frontend
`CreateListingPage.tsx` — calls `aiApi.validateImage(file)`, auto-fills category from Vision results.

---

## 3. Azure Communication Services (SMS)

### NuGet
```bash
dotnet add package Azure.Communication.Sms
```

### Real service
`Services/Azure/AzureCommunicationService.cs`

### Triggers (RentalsController / RentalService)
| Event | Method |
|-------|--------|
| Booking approved | `SendBookingApprovedSmsAsync` |
| Booking rejected | `SendBookingRejectedSmsAsync` |
| Return reminder | `SendReturnReminderSmsAsync` |

### Configuration
```json
"Azure": {
  "Integration": { "UseRealCommunication": true },
  "Communication": {
    "ConnectionString": "endpoint=https://YOUR-ACS.communication.azure.com/;accesskey=YOUR-KEY",
    "SenderPhone": "+1XXXXXXXXXX"
  }
}
```

---

## 4. Azure Maps

### NuGet
None — uses REST API via `HttpClient`.

### Real service
`Services/Azure/AzureMapsService.cs` — geocoding, Haversine distance, static map URLs.

### API endpoints
| Method | Path | Description |
|--------|------|-------------|
| GET | `/api/maps/geocode?address=` | Geocode address |
| GET | `/api/maps/static?lat=&lon=` | Redirect to static map image |
| GET | `/api/maps/distance?lat1=&lon1=&lat2=&lon2=` | Distance in km |

### Configuration
```json
"Azure": {
  "Integration": { "UseRealMaps": true },
  "Maps": {
    "SubscriptionKey": "YOUR-MAPS-KEY",
    "ClientId": "YOUR-MAPS-CLIENT-ID"
  }
}
```

### Frontend
- `ListingMap.tsx` — map on listing detail page
- `SearchPage.tsx` — nearby filter (5/10/25 km) with geolocation

Listings store `Latitude` / `Longitude` (geocoded on create).

---

## 5. Azure SignalR Service

### NuGet
```bash
dotnet add package Microsoft.Azure.SignalR
```

### Backend
- Hub: `Hubs/NotificationHub.cs` at `/hubs/notifications`
- Publisher: `Services/Azure/NotificationPublisher.cs`
- Events: rental request, approve/reject, new review

### DI registration
```csharp
builder.Services.AddRentThingsSignalR(builder.Configuration);
// ...
app.MapHub<NotificationHub>("/hubs/notifications");
```

### Configuration
```json
"Azure": {
  "Integration": { "UseAzureSignalR": true },
  "SignalR": {
    "ConnectionString": "Endpoint=https://YOUR.service.signalr.net;AccessKey=...;Version=1.0;"
  }
}
```

**Local dev:** `UseAzureSignalR: false` uses in-process SignalR (no Azure resource needed).

### Frontend npm
```bash
npm install @microsoft/signalr
```

- `src/lib/signalr.ts` — hub connection
- `src/hooks/useSignalRNotifications.ts` — auto-invalidates notifications query
- Vite proxy: `/hubs` → `ws: true` for WebSockets

---

## Enabling all services (production example)

```json
{
  "Azure": {
    "Integration": {
      "UseRealBlobStorage": true,
      "UseRealAiVision": true,
      "UseRealCommunication": true,
      "UseRealMaps": true,
      "UseAzureSignalR": true
    },
    "BlobStorage": { "ConnectionString": "...", "CdnBaseUrl": "https://cdn.rentthings.com" },
    "AiVision": { "Endpoint": "...", "ApiKey": "..." },
    "Communication": { "ConnectionString": "...", "SenderPhone": "+1..." },
    "Maps": { "SubscriptionKey": "..." },
    "SignalR": { "ConnectionString": "..." }
  }
}
```

Store secrets in **Azure Key Vault** or **User Secrets** (`dotnet user-secrets set "Azure:BlobStorage:ConnectionString" "..."`).
