#!/bin/bash
set -e

dotnet test --collect:"XPlat Code Coverage"
reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"coveragereport" -reporttypes:TextSummary
cat coveragereport/Summary.txt
