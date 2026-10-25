# Changelog

## [0.1.1] - 2026-10-25 - Ingame-Grafiken & Forschungsbaum

### Neu
- **Vier eigene Gebäude-Sprites** (192×192): Mk I klein/mittel/groß mit 1/2/3 Emitter-Spulen und Warnstreifen, Mk II mit türkisfarbenem Kern, vier goldenen Verstell-Armen und Display.
- **Vier Baumenü-Icons** (64×64) via `uiIconPath` – erscheinen auch als Icons im Forschungsbaum.
- **Zwei Forschungs-Icons** (128×128) unter `UI/Icons/Research/` als optionale Groß-Icons für Icon-Overrides.
- **`About/ModIcon.png`** (64×64) für Ladebildschirm/Mod-Einstellungen; überarbeitetes `About/Preview.png` mit allen vier Sprites.
- **Eigener Forschungs-Tab** `RectShield_ResearchTab` (`ResearchTabDef`), beide Forschungen mit `<tab>` und `researchViewX/Y` eingebunden; deutsche DefInjected-Texte für Tab und Forschungen.
- `Tools/make_assets.py` erzeugt jetzt alle Grafiken; `Tests/validate_xml.py` prüft `texPath`/`uiIconPath`/`iconPath`, `Tests/check_structure.py` prüft alle Grafik-Pfade und die Icon-Referenzen.
- Neue Doku `docs/GRAPHICS.md`.

### Technischer Befund
Im 1.6-Dekompilat (`Verse/ResearchProjectDef.cs`) existiert **kein** `iconPath`-Feld. Forschungsbaum-Icons kommen aus `uiIconPath` der freigeschalteten Defs. Ein `<iconPath>` im XML würde beim Laden einen Fehler werfen – deshalb bewusst nicht verwendet.

## [0.1.0] - 2026-10-25

Erste Version des Grundgeruests.

### Neu
- **Mk I** in drei festen Groessen (9x7, 15x11, 23x15 Kacheln).
  Blockt ausschliesslich direkte Schuesse. Keine Groessenanpassung moeglich.
- **Mk II** mit frei einstellbarer Breite/Hoehe (5-41 Kacheln), Versatz,
  90-Grad-Ausrichtung und Randmodus. Blockt zusaetzlich indirektes Feuer,
  Drop Pods und Meteoriten.
- Strombedarf von Mk II skaliert linear mit der geschuetzten Flaeche
  (300 W + 3 W pro Kachel, per Mod-Einstellung multiplizierbar).
- Energie/EMP-Modell mit automatischem Reboot nach Ueberlastung.
- Rechteck-Vorschau per `GenDraw.DrawFieldEdges`, persistente Umrandung
  optional in den Mod-Einstellungen.
- Zwei Forschungsprojekte, deutsche und englische Lokalisierung.
- Build-Konfiguration fuer RimWorld 1.6 (net472 / Harmony 2.4.3) und
  1.7 Waylith Beta (net10.0 / Harmony 3).
- CI: XML-Validierung und C#-Syntaxcheck.

### Bekannte Limitierungen
- Explosionsradien werden noch nicht am Schildrand abgeschnitten (v0.2).
- Freie Winkelrotation (z.B. 37 Grad) ist nicht vorgesehen - nur 90-Grad-Schritte.
- Drop-Pod-Erkennung nutzt die Faction des Skyfallers; Mods mit eigenen
  Pod-Klassen brauchen ggf. einen zusaetzlichen Patch.
- Die C#-Assembly muss gegen die eigene RimWorld-Installation kompiliert
  werden; die Methodensignaturen stehen in `docs/API_NOTES.md`.
