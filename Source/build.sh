#!/usr/bin/env bash
# Baut RectShield fuer beide RimWorld-Versionen.
#
# Usage:
#   RW16_MANAGED=/pfad/zu/1.6/RimWorldWin64_Data/Managed \
#   RW17_MANAGED=/pfad/zu/1.7/RimWorldWin64_Data/Managed \
#   HARMONY_2=/pfad/zu/Harmony-2.4.3 \
#   HARMONY_3=/pfad/zu/Harmony-3.0 \
#   ./Source/build.sh
#
# Ohne gesetzte Pfade werden die Windows-Standardpfade aus den csproj-Dateien
# verwendet. Fehlende DLLs fuehren zu einer klaren Fehlermeldung von MSBuild.
set -euo pipefail

cd "$(dirname "$0")/.."

echo "==> RimWorld 1.6 (net472 / Harmony 2.4.3)"
dotnet build Source/RectShield.1.6.csproj -c Release \
  ${RW16_MANAGED:+-p:ManagedPath="$RW16_MANAGED"} \
  ${HARMONY_2:+-p:HarmonyPath="$HARMONY_2"}

echo "==> RimWorld 1.7 Waylith (net10.0 / Harmony 3)"
dotnet build Source/RectShield.1.7.csproj -c Release \
  ${RW17_MANAGED:+-p:ManagedPath="$RW17_MANAGED"} \
  ${HARMONY_3:+-p:HarmonyPath="$HARMONY_3"}

echo "==> Fertig. Assemblies liegen in 1.6/Assemblies und 1.7/Assemblies."
