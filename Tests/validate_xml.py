#!/usr/bin/env python3
"""Validiert alle XML-Dateien des Mods und ein paar inhaltliche Regeln.

Nur Python-Standardbibliothek - laeuft ueberall ohne zusaetzliche Abhaengigkeiten.
"""
from __future__ import annotations

import sys
import xml.etree.ElementTree as ET
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
errors: list[str] = []
xml_files = sorted(ROOT.rglob("*.xml"))

if not xml_files:
    errors.append("Keine XML-Dateien gefunden.")

seen_defnames: dict[str, Path] = {}

for path in xml_files:
    rel = path.relative_to(ROOT)
    try:
        tree = ET.parse(path)
    except ET.ParseError as exc:
        errors.append(f"{rel}: XML ParseError: {exc}")
        continue

    root = tree.getroot()
    if root.tag not in {"Defs", "ModMetaData", "loadFolders", "LanguageData"}:
        errors.append(f"{rel}: unerwartetes Root-Element <{root.tag}>")

    for elem in root.iter("defName"):
        name = (elem.text or "").strip()
        if not name:
            errors.append(f"{rel}: leerer defName")
            continue
        if name in seen_defnames and seen_defnames[name] != path:
            errors.append(f"{rel}: doppelter defName '{name}' (auch in {seen_defnames[name]})")
        seen_defnames[name] = path

# About.xml: beide Zielversionen + Harmony-Dependency
about = ROOT / "About" / "About.xml"
if about.exists():
    versions = {e.text.strip() for e in ET.parse(about).getroot().iter("li") if e.text}
    for required in ("1.6", "1.7"):
        if required not in versions:
            errors.append(f"About.xml: supportedVersions enthaelt nicht {required}")
    text = about.read_text(encoding="utf-8")
    if "brrainz.harmony" not in text:
        errors.append("About.xml: Harmony-Dependency fehlt")
else:
    errors.append("About/About.xml fehlt")

# LoadFolders.xml: beide Versionen vorhanden
load_folders = ROOT / "LoadFolders.xml"
if load_folders.exists():
    lf = ET.parse(load_folders).getroot()
    tags = {child.tag for child in lf}
    for required in ("v1.6", "v1.7"):
        if required not in tags:
            errors.append(f"LoadFolders.xml: {required} fehlt")
else:
    errors.append("LoadFolders.xml fehlt")

# Alle in Defs referenzierten Texturen muessen existieren.
# texPath   -> Welt-Sprite, uiIconPath -> Baumenue + Forschungsbaum,
# iconPath  -> optional (in 1.6 nicht an ResearchProjectDef vorhanden).
textures_root = ROOT / "Common" / "Textures"
texture_tags = ("texPath", "uiIconPath", "iconPath", "modIconPath")
for def_file in (ROOT / "Common" / "Defs").rglob("*.xml"):
    root = ET.parse(def_file).getroot()
    for tag in texture_tags:
        for tex in root.iter(tag):
            rel_path = (tex.text or "").strip()
            if not rel_path:
                continue
            if not (textures_root / f"{rel_path}.png").exists():
                errors.append(f"{def_file.relative_to(ROOT)}: {tag} fehlt: {rel_path}.png")

# Forschungs-Tab muss von beiden Projekten referenziert werden.
research = ROOT / "Common/Defs/ResearchProjectDefs/Research_RectShield.xml"
if research.exists():
    tabs = [e.text.strip() for e in ET.parse(research).getroot().iter("tab") if e.text]
    if len(tabs) != 3 or any(t != "RectShield_ResearchTab" for t in tabs):
        errors.append(f"Research_RectShield.xml: erwartet 3 Forschungen im eigenen Tab, gefunden: {tabs}")
else:
    errors.append("Research_RectShield.xml fehlt")

# Keyed-Dateien: EN und DE muessen die gleichen Keys haben
def keys(path: Path) -> set[str]:
    if not path.exists():
        return set()
    return {child.tag for child in ET.parse(path).getroot()}

en = keys(ROOT / "Common/Languages/English/Keyed/RectShield_Keys.xml")
de = keys(ROOT / "Common/Languages/German/Keyed/RectShield_Keys.xml")
for missing in sorted(en - de):
    errors.append(f"Deutsche Uebersetzung fehlt fuer Key: {missing}")
for extra in sorted(de - en):
    errors.append(f"Deutscher Key ohne englisches Pendant: {extra}")

for required_def in ("RectShield_Mk1_S", "RectShield_Mk2", "RectShield_Mk3",
                     "RectShield_Mk1_Research", "RectShield_Mk2_Research",
                     "RectShield_Mk3_Research"):
    if required_def not in seen_defnames:
        errors.append(f"Pflicht-defName fehlt: {required_def}")

print(f"Geprueft: {len(xml_files)} XML-Dateien, {len(seen_defnames)} eindeutige defNames, "
      f"{len(en)} Sprach-Keys.")

if errors:
    print("\nFEHLER:", file=sys.stderr)
    for err in errors:
        print(f"  - {err}", file=sys.stderr)
    sys.exit(1)

print("OK: alle XML-Checks bestanden.")
