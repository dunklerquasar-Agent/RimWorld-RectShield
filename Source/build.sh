#!/usr/bin/env bash
# Baut RectShield fuer beide RimWorld-Versionen.
#
# WICHTIG: 1.7 (Waylith) hat keinen "RimWorldWin64_Data/Managed"-Ordner mehr!
# Assembly-CSharp.dll, UnityStubs.dll und Waylith.dll liegen im SPIEL-ROOT.
# Deshalb bekommt das 1.7-Projekt -p:GameDir (nicht ManagedPath).
#
# Usage (Linux/macOS):
#   RW16_MANAGED=/pfad/zu/1.6/RimWorldWin64_Data/Managed \
#   RW17_DIR=/pfad/zu/1.7/Spiel-Root \
#   HARMONY_2=/pfad/zu/Harmony/1.6/Assemblies \
#   HARMONY_3=/pfad/zu/Harmony/1.7/Assemblies \
#   ./Source/build.sh
#
# Ohne gesetzte Pfade gelten die Windows-Standardpfade aus den csproj-Dateien.
# Auf Windows stattdessen windows-p0/Build-RectShield.ps1 verwenden.
set -euo pipefail

cd "$(dirname "$0")/.."

echo "==> RimWorld 1.6 (net472 / Harmony 2.4.3)"
dotnet build Source/RectShield.1.6.csproj -c Release \
  ${RW16_MANAGED:+-p:ManagedPath="$RW16_MANAGED"} \
  ${HARMONY_2:+-p:HarmonyPath="$HARMONY_2"}

echo "==> RimWorld 1.7 Waylith (net10.0 / Harmony 3)"
dotnet build Source/RectShield.1.7.csproj -c Release \
  ${RW17_DIR:+-p:GameDir="$RW17_DIR"} \
  ${HARMONY_3:+-p:HarmonyPath="$HARMONY_3"}

echo "==> Fertig. Assemblies liegen in 1.6/Assemblies und 1.7/Assemblies."
