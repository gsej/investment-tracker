# UnitTests

## Test coverage

The project uses `coverlet.collector`.

### One-time setup

Install ReportGenerator as a global tool:

```bash
dotnet tool install --global dotnet-reportgenerator-globaltool
```

### Generating a coverage report

Run `run-tests.sh` to run the tests and print a coverage summary to the console:

```bash
./run-tests.sh
```

Or manually, from this directory:

```bash
dotnet test --collect:"XPlat Code Coverage"
reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"coveragereport" -reporttypes:TextSummary
cat coveragereport/Summary.txt
```

For an HTML report to browse in detail:

```bash
reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"coveragereport" -reporttypes:Html
```

The `TestResults/` and `coveragereport/` directories are in `.gitignore`.
