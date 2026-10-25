#!/usr/bin/env python3
"""Prueft die von GitHub Actions erzeugten RectShield-Assemblies.

Stellt sicher, dass beide DLLs existieren, plausibel gross sind und dass
weder Harmony- noch Spiel-DLLs in den Assemblies-Ordnern liegen (diese
duerfen niemals mit einer Mod ausgeliefert werden).
"""
from __future__ import annotations

import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
FORBIDDEN = {
    "0Harmony.dll", "Assembly-CSharp.dll", "UnityStubs.dll", "Waylith.dll",
    "UnityEngine.dll", "UnityEngine.CoreModule.dll", "Waylith.UnityCompat.dll",
}

errors: list[str] = []
for version in ("1.6", "1.7"):
    folder = ROOT / version / "Assemblies"
    dll = folder / "RectShield.dll"
    if not folder.exists():
        errors.append(f"{version}/Assemblies/ fehlt")
        continue
    if not dll.exists():
        errors.append(f"{dll} fehlt - Build fehlgeschlagen?")
        continue

    size = dll.stat().st_size
    if size < 10_000 or size > 2_000_000:
        errors.append(f"{dll}: unplausible Groesse {size} Bytes")
    else:
        print(f"  OK {version}: RectShield.dll ({size:,} Bytes)")

    with open(dll, "rb") as handle:
        if handle.read(2) != b"MZ":
            errors.append(f"{dll}: kein PE/CLR-Header (kein gueltiges .NET-Assembly)")
        else:
            print(f"       PE/CLR-Header vorhanden")

    stray = sorted(
        p.name for p in folder.iterdir()
        if p.is_file() and (p.name in FORBIDDEN or p.suffix in {".pdb", ".deps.json"})
    )
    if stray:
        errors.append(f"{version}/Assemblies/ enthaelt unerwuenschte Dateien: {stray}")

    sub = folder / ("net10.0" if version == "1.7" else "net472")
    if sub.exists():
        errors.append(f"{version}/Assemblies/{sub.name}/ existiert - AppendTargetFrameworkToOutputPath pruefen")

print(f"Geprueft: 1.6/Assemblies und 1.7/Assemblies")
if errors:
    print("\nFEHLER:", file=sys.stderr)
    for err in errors:
        print(f"  - {err}", file=sys.stderr)
    sys.exit(1)
print("OK: beide Assemblies valide und sauber.")
