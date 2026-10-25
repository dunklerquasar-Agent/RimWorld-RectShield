#!/usr/bin/env python3
"""Prueft die erwartete Mod-Ordnerstruktur und Pflicht-Dateien."""
from __future__ import annotations

import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]

required = [
    "About/About.xml",
    "About/Preview.png",
    "About/ModIcon.png",
    "LoadFolders.xml",
    "LICENSE",
    "README.md",
    "CHANGELOG.md",
    "Source/RectShield.1.6.csproj",
    "Source/RectShield.1.7.csproj",
    "Source/Core/CompRectShield.cs",
    "Source/Core/RectShieldShape.cs",
    "Source/Core/RectShieldMapComponent.cs",
    "Source/Patches/HarmonyInit.cs",
    "Common/Defs/ThingDefs_Buildings/Buildings_RectShield.xml",
    "Common/Defs/ResearchProjectDefs/Research_RectShield.xml",
    "Common/Defs/ResearchTabDefs/ResearchTabs_RectShield.xml",
    "Common/Languages/English/Keyed/RectShield_Keys.xml",
    "Common/Languages/German/Keyed/RectShield_Keys.xml",
    "Common/Languages/German/DefInjected/ResearchTabDef/RectShield_ResearchTabs.xml",
    "1.6/Assemblies/.gitkeep",
    "1.7/Assemblies/.gitkeep",
    "docs/API_NOTES.md",
    "docs/GRAPHICS.md",
    "docs/PLAN.md",
    "docs/BALANCE.md",
    "Tests/validate_xml.py",
    "Tests/parse_csharp.py",
    ".github/workflows/validate.yml",
]

# Alle Ingame-Grafiken
graphics = [
    "Common/Textures/Things/Building/RectShield/RectShield_Mk1_S.png",
    "Common/Textures/Things/Building/RectShield/RectShield_Mk1_M.png",
    "Common/Textures/Things/Building/RectShield/RectShield_Mk1_L.png",
    "Common/Textures/Things/Building/RectShield/RectShield_Mk2.png",
    "Common/Textures/UI/Icons/Buildings/RectShield_Mk1_S.png",
    "Common/Textures/UI/Icons/Buildings/RectShield_Mk1_M.png",
    "Common/Textures/UI/Icons/Buildings/RectShield_Mk1_L.png",
    "Common/Textures/UI/Icons/Buildings/RectShield_Mk2.png",
    "Common/Textures/UI/Icons/Research/RectShield_Mk1_Research.png",
    "Common/Textures/UI/Icons/Research/RectShield_Mk2_Research.png",
]
required.extend(graphics)

missing = [p for p in required if not (ROOT / p).exists()]

# Jede C#-Datei muss im Namespace RectShield liegen.
cs_bad = []
for path in (ROOT / "Source").rglob("*.cs"):
    if "namespace RectShield" not in path.read_text(encoding="utf-8"):
        cs_bad.append(str(path.relative_to(ROOT)))

# Jedes Gebaeude braucht texPath UND uiIconPath.
import xml.etree.ElementTree as ET
buildings = ET.parse(ROOT / "Common/Defs/ThingDefs_Buildings/Buildings_RectShield.xml").getroot()
ui_issues = []
for thing in buildings.findall("ThingDef"):
    if thing.get("Abstract") == "True":
        continue
    name = thing.findtext("defName")
    if thing.find("uiIconPath") is None or thing.find("graphicData/texPath") is None:
        ui_issues.append(name or "?")

print(f"Geprueft: {len(required)} Pflichtpfade, {len(graphics)} Grafiken, "
      f"{len(list((ROOT / 'Source').rglob('*.cs')))} C#-Dateien.")

if missing or cs_bad or ui_issues:
    for p in missing:
        print(f"  FEHLT: {p}", file=sys.stderr)
    for p in cs_bad:
        print(f"  FALSCHER NAMESPACE: {p}", file=sys.stderr)
    for name in ui_issues:
        print(f"  FEHLENDE GRAFIK-REFERENZ: {name}", file=sys.stderr)
    sys.exit(1)
print("OK: Mod-Struktur und Grafik-Referenzen vollstaendig.")
