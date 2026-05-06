# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

An investment tracking application for personal UK investment accounts (AjBell). Data flows from CSV exports → JSON files → SQL Server → REST API → Angular frontend.

## Build & Run Commands

All C# commands run from `/code/`:

```bash
# Build entire solution
dotnet build investment-tracker.sln

# Run API
dotnet run --project Api/Api.csproj

# Run tests
dotnet test UnitTests/UnitTests.csproj

# Run a single test
dotnet test UnitTests/UnitTests.csproj --filter "FullyQualifiedName~TestClassName"

# Console apps (run in order for data prep)
dotnet run --project AjBellParserConsole/AjBellParserConsole.csproj
dotnet run --project LoaderConsole/LoaderConsole.csproj
dotnet run --project HistoryCalculatorConsole/HistoryCalculatorConsole.csproj
```

Angular frontend, from `/code/web-angular/`:

```bash
npm install
npm start       # dev server at http://localhost:4200
npm test        # Karma + Jasmine
npm run build   # production build
```

## Local Infrastructure

Three Docker containers are required for local development — start scripts are in `/code/`:

```bash
./sql.sh     # SQL Server 2022 on port 1433
./seq.sh     # Seq logging on port 5341
./jaeger.sh  # Jaeger tracing on ports 6831–9411
```

Connection string used by all apps: `Server=localhost;Initial Catalog=investments;User ID=sa;Password=Password123!;Encrypt=True;Trust Server Certificate=True;`

## Architecture

### Data Pipeline (one-time / periodic)

1. **AjBellParserConsole** — converts AjBell CSV exports to JSON, writing to `AccountStatements/{accountCode}/`
2. **LoaderConsole** — drops/recreates the database via EF Core migrations, then loads JSON files (accounts, stocks, transactions, prices, exchange rates) into SQL Server
3. **HistoryCalculatorConsole** — pre-calculates historical account values and stores them in `AccountHistoricalValue` for fast API responses

### Runtime

- **Api** (ASP.NET Core 8) — serves the Angular frontend with endpoints:
  - `GET /accounts`
  - `POST /account/portfolio`
  - `POST /account/history` / `POST /account/history2`
- **web-angular** (Angular 18) — chart-heavy UI using Chart.js + Tailwind CSS; communicates with the API

### Shared Libraries

- **Database** — EF Core 7 `InvestmentsDbContext`, entities, and migrations
- **Common** — enumerations, `DirectoryHelper`, OpenTelemetry tracing setup
- **FileReaders** — typed readers (`IReader<T>`) for each JSON/CSV input format
- **DataLoaders** — loads parsed data into the database; includes enrichers for stock transaction types, fees, and stamp duty

### Key Patterns

- All C# projects use `Microsoft.Extensions.DependencyInjection`
- Complex queries use the query-handler pattern (see `Api/QueryHandlers/`)
- OpenTelemetry is wired throughout; traces export to Jaeger and Azure Application Insights
- EF Core InMemory provider is used in `UnitTests`; NSubstitute handles mocks

## Testing

- C# tests: xUnit + FluentAssertions + NSubstitute, located in `/code/UnitTests/`
- Angular tests: Karma + Jasmine

## Solution File

`/code/investment-tracker.sln` — references all C# projects.
