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
    "Source/Core/RectShieldSubstructure.cs",
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
    ".github/workflows/build.yml",
    "Source/RectShield.NuGet.1.6.csproj",
    "Source/RectShield.NuGet.1.7.csproj",
    "Tests/verify_build.py",
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
    # Drei Stufen-Sprites (KI-generiert, freigestellt)
    "Common/Textures/Things/Building/RectShield/RectShield_Gen_Industrial.png",
    "Common/Textures/Things/Building/RectShield/RectShield_Gen_Military.png",
    "Common/Textures/Things/Building/RectShield/RectShield_Gen_HighTech.png",
    "Common/Textures/UI/Icons/Buildings/RectShield_Gen_Industrial.png",
    "Common/Textures/UI/Icons/Buildings/RectShield_Gen_Military.png",
    "Common/Textures/UI/Icons/Buildings/RectShield_Gen_HighTech.png",
    "Common/Textures/UI/Icons/Research/RectShield_Mk3_Research.png",
    # + / - Symbole und Rumpf-Gizmo
    "Common/Textures/UI/Gizmos/RectShield_Plus.png",
    "Common/Textures/UI/Gizmos/RectShield_Minus.png",
    "Common/Textures/UI/Gizmos/RectShield_FitHull.png",
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

# Build-Konfiguration: 1.7 braucht GameDir (Spiel-Root), 1.6 das Referenzpaket.
build_issues = []
c16 = (ROOT / "Source/RectShield.1.6.csproj").read_text(encoding="utf-8")
c17 = (ROOT / "Source/RectShield.1.7.csproj").read_text(encoding="utf-8")
if "Microsoft.NETFramework.ReferenceAssemblies" not in c16:
    build_issues.append("1.6.csproj: Microsoft.NETFramework.ReferenceAssemblies fehlt")
if "net472" not in c16 or "ManagedPath" not in c16:
    build_issues.append("1.6.csproj: net472/ManagedPath fehlt")
if "GameDir" not in c17 or "UnityStubs" not in c17 or "Waylith" not in c17:
    build_issues.append("1.7.csproj: GameDir/UnityStubs/Waylith fehlt")
if "ManagedPath" in c17:
    build_issues.append("1.7.csproj: enthaelt noch ManagedPath (1.7 hat keinen Managed-Ordner)")
if "net10.0" not in c17:
    build_issues.append("1.7.csproj: net10.0 fehlt")

# Dritte Stufe muss in Enum, Defs und Forschung vorhanden sein.
tier_file = (ROOT / "Source/Core/RectShieldTier.cs").read_text(encoding="utf-8")
buildings_text = (ROOT / "Common/Defs/ThingDefs_Buildings/Buildings_RectShield.xml").read_text(encoding="utf-8")
research_text = (ROOT / "Common/Defs/ResearchProjectDefs/Research_RectShield.xml").read_text(encoding="utf-8")
if "Mk3 = 2" not in tier_file:
    build_issues.append("RectShieldTier.cs: Mk3 fehlt")
if "<defName>RectShield_MkIII</defName>" not in buildings_text:
    build_issues.append("Buildings_RectShield.xml: Mk3-Def fehlt")
if "<defName>RectShield_Mk3_Research</defName>" not in research_text:
    build_issues.append("Research_RectShield.xml: Mk3-Forschung fehlt")

# Gravship Shield: Auto-Fit und symmetrische Mk-I-Felder muessen gesetzt sein.
if "<autoFitSubstructure>true</autoFitSubstructure>" not in buildings_text:
    build_issues.append("Buildings_RectShield.xml: Gravship Auto-Fit fehlt")
for fixed in ("<fixedWidth>11</fixedWidth>", "<fixedWidth>17</fixedWidth>",
              "<fixedWidth>23</fixedWidth>"):
    if fixed not in buildings_text:
        build_issues.append(f"Buildings_RectShield.xml: symmetrisches Feld {fixed} fehlt")

print(f"Geprueft: {len(required)} Pflichtpfade, {len(graphics)} Grafiken, "
      f"{len(list((ROOT / 'Source').rglob('*.cs')))} C#-Dateien, 2 csproj.")

if missing or cs_bad or ui_issues or build_issues:
    for p in missing:
        print(f"  FEHLT: {p}", file=sys.stderr)
    for p in cs_bad:
        print(f"  FALSCHER NAMESPACE: {p}", file=sys.stderr)
    for name in ui_issues:
        print(f"  FEHLENDE GRAFIK-REFERENZ: {name}", file=sys.stderr)
    for issue in build_issues:
        print(f"  BUILD-KONFIG: {issue}", file=sys.stderr)
    sys.exit(1)
print("OK: Mod-Struktur, Grafik-Referenzen und Build-Konfiguration vollstaendig.")
