# Changelog

## [0.1.3] - 2026-10-25 - Kompilierung in GitHub Actions

- **Neuer Workflow `.github/workflows/build.yml`** kompiliert `RectShield.dll` für
  **RimWorld 1.6 (net472)** und **1.7 Waylith (net10.0)** komplett in GitHub Actions –
  ohne lokale Spielinstallation. Referenzen aus NuGet:
  `Krafs.Rimworld.Ref` 1.6.4871 / 1.7.4985-unstable,
  `Lib.Harmony.Ref` 2.4.3 / 3.0.0-rc.3. Versionen sind per `workflow_dispatch`
  übersteuerbar.
- Neue CI-Projekte `Source/RectShield.NuGet.1.6.csproj` und `.1.7.csproj` teilen sich
  den Quellcode mit den lokalen Projekten. `Tests/verify_build.py` prüft PE-Header,
  Dateigröße und dass keine Spiel-/Harmony-DLL im Output liegt.
- **Beide Assemblys sind jetzt im Repo** (`1.6/Assemblies/`, `1.7/Assemblies/`) –
  die Mod ist ohne lokalen Build installierbar.
- Acht API-Signaturen gegen die echten 1.6-Referenz-Assemblies korrigiert
  (`CS0115`, `CS1503`, `CS0103`, `CS0122`): `PostDeSpawn(Map, DestroyMode)`,
  `PostDrawExtraSelectionOverlays()`, `GetDamageAmount(Thing)`,
  `RimWorld.VersionControl`, `Gizmo.Disabled`, `GenDraw.DrawFieldEdges(...)`,
  `PlaceWorker.AllowsPlacing` (6 Parameter), `Projectile.Tick`/`Skyfaller.Tick`.
  Details in `docs/API_NOTES.md`.

## [0.1.2] - 2026-10-25 - Waylith-Buildpfade korrigiert

### Fix (P0-Vorbereitung)
- **`RectShield.1.7.csproj`**: `ManagedPath` → **`GameDir`**. RimWorld 1.7 (Waylith) hat keinen `RimWorldWin64_Data/Managed`-Ordner mehr; `Assembly-CSharp.dll`, `UnityStubs.dll`, `Waylith.dll` und optional `Waylith.UnityCompat.dll` liegen im Spiel-Root. Unity-DLL-Referenzen entfernt, Harmony-Default zeigt auf `1.7/Assemblies`.
- **`RectShield.1.6.csproj`**: `PackageReference Microsoft.NETFramework.ReferenceAssemblies 1.0.3` ergänzt (verhindert `MSB3644` ohne installiertes .NET-4.7.2-Targeting-Pack); `UnityEngine.dll`-Referenz nur noch bei vorhandener Datei; Harmony-Default auf `1.6/Assemblies`.
- `Source/build.sh`: `RW17_MANAGED` → `RW17_DIR` (`-p:GameDir=...`), Harmony-Pfade `HARMONY_2`/`HARMONY_3` ergänzt.
- `README.md`, `docs/API_NOTES.md`, `.github/workflows/validate.yml`: Build-Anleitung und CI an die 1.7-Pfade angepasst (`RW17_DIR`, `HARMONY_3`).
- Zusätzliches Windows-Paket `windows-p0/` (außerhalb des Repos): `P0-Windows.md`, `Build-RectShield.ps1`, `Check-API.ps1`, `Apply-WindowsFix.ps1`, `Install-DevMod.ps1`.

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
