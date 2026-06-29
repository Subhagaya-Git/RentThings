# RentThings

AI-powered peer-to-peer rental marketplace. Rent cameras, tools, camping gear, event equipment, and more — or list your own items and earn.

## Tech Stack

| Layer | Technology |
|-------|------------|
| Frontend | React 19, TypeScript, Vite, Tailwind CSS, React Router, React Query, Zustand |
| Backend | ASP.NET Core Web API, Entity Framework Core, SQL Server |
| Cloud | Microsoft Azure (SQL, Blob, Entra ID, AI Services, AI Vision, Functions, ACS) |
| Hosting | Azure App Service |

## Project Structure

```
RentThings/
├── frontend/          # React SPA (Vite)
├── backend/
│   └── RentThings.Api/   # ASP.NET Core Web API
├── functions/
│   └── RentThings.Functions/  # Azure Functions (background jobs)
├── database/
│   └── schema.sql     # Azure SQL schema
├── docs/
│   └── architecture.md
└── README.md
```

## Prerequisites

- **Node.js** 20+ and npm
- **.NET SDK** 10+
- **SQL Server LocalDB** (included with Visual Studio) or Azure SQL connection string

## Quick Start

### 1. Backend API

```bash
cd backend/RentThings.Api
dotnet restore
dotnet run --launch-profile http
```

API runs at **http://localhost:5280**. On first run, EF Core creates the database and seeds demo data.

**Demo accounts** (any password works in mock auth):

| Email | Role |
|-------|------|
| renter@rentthings.com | Renter |
| owner@rentthings.com | Owner |
| admin@rentthings.com | Admin |

### 2. Frontend

```bash
cd frontend
npm install
npm run dev
```

App runs at **http://localhost:5173** with API proxy to port 5280.

### 3. Azure Functions (optional)

Requires [Azure Functions Core Tools](https://learn.microsoft.com/azure/azure-functions/functions-run-local):

```bash
cd functions/RentThings.Functions
dotnet restore
func start
```

## Key Features

- **Home page** — Hero, search, featured rentals, categories, stats, testimonials
- **Search & discovery** — Filters, grid/list views, sorting
- **Listing management** — Create listings with AI description generator and image validation
- **Rental workflow** — Request → approve → active → complete → review
- **Trust score** — 0–100 score with Bronze/Silver/Gold/Platinum levels
- **Dashboards** — Renter, Owner, and Admin with charts
- **AI assistant** — Floating chat powered by Azure AI Services (mock locally)
- **Notifications** — Booking requests, approvals, reminders

## API Endpoints

| Method | Path | Description |
|--------|------|-------------|
| POST | `/api/auth/login` | Login |
| POST | `/api/auth/register` | Register |
| GET | `/api/listings` | Search listings |
| GET | `/api/listings/featured` | Featured listings |
| POST | `/api/listings` | Create listing |
| POST | `/api/rentals` | Request rental |
| PATCH | `/api/rentals/{id}/status` | Update rental status |
| GET | `/api/notifications` | User notifications |
| GET | `/api/admin/stats` | Admin analytics |
| POST | `/api/ai/chat` | AI assistant chat |
| POST | `/api/ai/validate-image` | AI Vision validation |

## Azure Configuration

Configure in `backend/RentThings.Api/appsettings.json` under the `Azure` section:

- `Sql.ConnectionString` — Azure SQL connection
- `BlobStorage.ConnectionString` — Azure Blob Storage
- `EntraId.*` — Microsoft Entra ID app registration
- `AiServices.*` — Azure OpenAI / AI Services
- `AiVision.*` — Azure AI Vision
- `Communication.ConnectionString` — Azure Communication Services

Replace mock service registrations in `Program.cs` with production implementations when deploying.

## Implemented vs Placeholder

| Component | Status |
|-----------|--------|
| React UI (all major routes) | ✅ Implemented |
| ASP.NET API + EF Core schema | ✅ Implemented |
| Seed data & mock auth | ✅ Implemented |
| Azure SQL schema file | ✅ Implemented |
| Azure service interfaces | ✅ Implemented |
| Mock Azure services (local dev) | ✅ Implemented |
| Azure Functions job stubs | ✅ Placeholder |
| Real Entra ID / Blob / AI / ACS | 🔧 Ready for credentials |

## License

MIT
