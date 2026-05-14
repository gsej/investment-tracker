
# Investment Tracker

A personal investment tracking application for UK AjBell accounts. CSV exports from AjBell are parsed into JSON, loaded into SQL Server, and served through an ASP.NET Core API to an Angular frontend that displays portfolio and historical value charts.

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org/) (for the Angular frontend)
- [Docker](https://www.docker.com/) (for SQL Server and Seq)

## Quick Start

### 1. Start infrastructure

```bash
cd code
./sql.sh   # SQL Server 2022 on port 1433
./seq.sh   # Seq log viewer on port 5341
```

### 2. Parse AjBell CSV exports

Place AjBell CSV downloads in `SampleData/AccountStatements/<AccountCode>/`, then run:

```bash
dotnet run --project code/AjBellParserConsole/AjBellParserConsole.csproj
```

This writes `transactions.json` and `cashstatement_items.json` into each account folder.

### 3. Load data into SQL Server

```bash
dotnet run --project code/LoaderConsole/LoaderConsole.csproj
```

This drops and recreates the database, then loads accounts, stocks, transactions, prices, and exchange rates from `SampleData/` (or a configured alternative folder).

### 4. Pre-calculate historical values

```bash
dotnet run --project code/HistoryCalculatorConsole/HistoryCalculatorConsole.csproj
```

### 5. Run the API

```bash
dotnet run --project code/Api/Api.csproj
```

### 6. Run the frontend

```bash
cd code/web-angular
npm install
npm start   # dev server at http://localhost:4200
```

## Using your own data

By default all apps look for data in `SampleData/`. To use a different location, set `dataFolder` (and optionally `priceFolder`) in the relevant `appsettings.json` files. See [SampleData/README.md](SampleData/README.md) for the expected folder structure.

## Project Structure

```
code/
  AjBellParserConsole/   CSV → JSON parser
  LoaderConsole/         JSON → SQL Server loader
  HistoryCalculatorConsole/  Pre-calculates account value history
  Api/                   ASP.NET Core 8 REST API
  web-angular/           Angular 18 frontend (Chart.js + Tailwind CSS)
  Database/              EF Core DbContext, entities, and migrations
  DataLoaders/           Writes parsed data to the database
  FileReaders/           Typed readers for each JSON/CSV format
  Common/                Shared enums, helpers, OpenTelemetry setup
  UnitTests/             xUnit + FluentAssertions + NSubstitute
SampleData/              Sample input files (accounts, stocks, prices, statements)
```

## Running Tests

```bash
# C# unit tests
dotnet test code/UnitTests/UnitTests.csproj

# Angular tests
cd code/web-angular && npm test
```

See [code/UnitTests/README.md](code/UnitTests/README.md) for generating coverage reports.

## Data Quality

Account statement data (transactions and cash events) is accurate. Calculated account values depend on collected stock prices, which come from various sources and can contain duplicates or gaps. The application highlights discrepancies between calculated values and any manually recorded reference values, and flags large day-to-day swings.

## API Endpoints

| Method | Path | Description |
|--------|------|-------------|
| GET | `/accounts` | List all accounts |
| POST | `/account/portfolio` | Current portfolio for an account |
| POST | `/account/history` | Historical account value |
| POST | `/account/precalculated-history` | Historical account value (alternate implementation) |

## Notes

- 2024-04-01 AJBell transaction fees changed to £5.
- 2026-04-01 Regular investment fees reduced from £1.50 to £0.
