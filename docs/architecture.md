# RentThings Architecture

RentThings is an AI-powered peer-to-peer rental marketplace built on Microsoft Azure.

## System Overview

```mermaid
flowchart TB
    subgraph Client["Client Layer"]
        WEB["React SPA<br/>(Vite + TypeScript)"]
    end

    subgraph Azure["Microsoft Azure"]
        APPSVC["Azure App Service<br/>ASP.NET Core API"]
        ENTRA["Microsoft Entra ID<br/>Authentication & RBAC"]
        SQL["Azure SQL Database<br/>Users, Listings, Rentals"]
        BLOB["Azure Blob Storage<br/>Images & Documents"]
        AI["Azure AI Services<br/>Listing Generator & Chat"]
        VISION["Azure AI Vision<br/>Image Validation"]
        ACS["Azure Communication Services<br/>Email & SMS"]
        FUNC["Azure Functions<br/>Background Jobs"]
    end

    WEB -->|HTTPS / REST| APPSVC
    WEB -->|OAuth 2.0 / OIDC| ENTRA
    APPSVC --> ENTRA
    APPSVC --> SQL
    APPSVC --> BLOB
    APPSVC --> AI
    APPSVC --> VISION
    APPSVC --> ACS
    FUNC --> APPSVC
    FUNC --> ACS
```

## Component Architecture

```mermaid
flowchart LR
    subgraph Frontend
        ROUTER[React Router]
        QUERY[React Query]
        ZUSTAND[Zustand Stores]
        UI[Component Library]
    end

    subgraph Backend
        CTRL[Controllers]
        SVC[Business Services]
        AZURE[Azure Service Interfaces]
        EF[EF Core DbContext]
    end

    ROUTER --> QUERY
    QUERY --> CTRL
    CTRL --> SVC
    SVC --> AZURE
    SVC --> EF
    ZUSTAND --> QUERY
```

## Data Model

```mermaid
erDiagram
    User ||--o{ Listing : owns
    User ||--o{ Rental : rents
    User ||--o{ Review : writes
    User ||--o{ Notification : receives
    User ||--o{ Favorite : saves
    Listing ||--o{ ListingImage : has
    Listing ||--o{ Rental : generates
    Rental ||--o{ Review : produces
```

## Trust Score System

| Level | Score Range | Benefits |
|-------|-------------|----------|
| Bronze | 0–39 | Basic access |
| Silver | 40–59 | Standard visibility |
| Gold | 60–79 | Featured eligibility |
| Platinum | 80–100 | Priority support, lower deposits |

**Factors:** completed rentals (+3 each, max +30), review average, late returns (−5 each), account verification (+10).

## Rental Workflow

```mermaid
sequenceDiagram
    participant R as Renter
    participant API as API
    participant O as Owner
    participant ACS as Communication Services

    R->>API: Search & view listing
    R->>API: Request rental (dates)
    API->>O: Notification (booking request)
    O->>API: Approve / Reject
    API->>ACS: Booking confirmation email
    API->>R: Status update notification
    R->>API: Complete rental
    R->>API: Submit review
    API->>API: Recalculate trust scores
```

## Azure Integration Status

| Service | Interface | Local Dev |
|---------|-----------|-----------|
| Azure SQL | `RentThingsDbContext` | LocalDB / EnsureCreated |
| Blob Storage | `IBlobStorageService` | Mock (returns placeholder URLs) |
| Entra ID | `IEntraIdService` | Mock JWT auth |
| AI Services | `IAiServicesClient` | Mock responses |
| AI Vision | `IAiVisionService` | Mock validation |
| Communication Services | `ICommunicationService` | Logs only |
| Azure Functions | Timer triggers | Placeholder jobs |

## Deployment Topology

```mermaid
flowchart TB
    CDN["Azure CDN"] --> APPSVC["App Service (API)"]
    CDN --> STATIC["Static Web App (Frontend)"]
    APPSVC --> SQL["Azure SQL"]
    APPSVC --> BLOB["Blob Storage"]
    APPSVC --> KV["Key Vault (secrets)"]
    FUNC["Functions App"] --> APPSVC
    APPSVC --> AI["Azure AI Services"]
    APPSVC --> VISION["AI Vision"]
    APPSVC --> ACS["Communication Services"]
```

## Security

- JWT bearer authentication (Entra ID in production)
- Role-based access: Renter, Owner, Admin
- CORS restricted to frontend origins
- Image validation before listing approval
- Trust score gating for high-value rentals
