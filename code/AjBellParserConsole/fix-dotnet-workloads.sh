#!/usr/bin/env bash
set -euo pipefail

WORKLOADSETS=/usr/share/dotnet/sdk-manifests/10.0.100/workloadsets/10.0.110.1

sudo mkdir -p "$WORKLOADSETS"
sudo tee "$WORKLOADSETS/microsoft.net.workloads.workloadset.json" > /dev/null << 'EOF'
{
  "Microsoft.NET.Workload.Emscripten.Current": "10.0.110/10.0.100",
  "Microsoft.NET.Workload.Emscripten.net6": "10.0.110/10.0.100",
  "Microsoft.NET.Workload.Emscripten.net7": "10.0.110/10.0.100",
  "Microsoft.NET.Workload.Emscripten.net8": "10.0.110/10.0.100",
  "Microsoft.NET.Workload.Emscripten.net9": "10.0.110/10.0.100",
  "Microsoft.NET.Sdk.Android": "36.1.2/10.0.100",
  "Microsoft.NET.Sdk.iOS": "26.0.11017/10.0.100",
  "Microsoft.NET.Sdk.MacCatalyst": "26.0.11017/10.0.100",
  "Microsoft.NET.Sdk.macOS": "26.0.11017/10.0.100",
  "Microsoft.NET.Sdk.Maui": "10.0.0/10.0.100",
  "Microsoft.NET.Sdk.tvOS": "26.0.11017/10.0.100",
  "Microsoft.NET.Workload.Mono.ToolChain.Current": "10.0.110/10.0.100",
  "Microsoft.NET.Workload.Mono.ToolChain.net6": "10.0.110/10.0.100",
  "Microsoft.NET.Workload.Mono.ToolChain.net7": "10.0.110/10.0.100",
  "Microsoft.NET.Workload.Mono.ToolChain.net8": "10.0.110/10.0.100",
  "Microsoft.NET.Workload.Mono.ToolChain.net9": "10.0.110/10.0.100"
}
EOF
