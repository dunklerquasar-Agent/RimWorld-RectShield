# Changelog

## [0.4.0] - 2026-10-26 - Gravship-Schild folgt der exakten Rumpfform

### Problem
Der Auto-Fit nahm nur die **Bounding Box** aller Bodenplatten. Bei einer
**L-Form** des Schiffs wurde daraus ein ausgefuelltes Quadrat/Rechteck – die
leere Aussparung wurde mitgeschtuetzt, das Feld "wurde nur groesser".

### Umsetzung
- `RectShieldSubstructure.Scan()` liefert jetzt zusaetzlich ein
  `RectShieldHull`-Objekt mit der **kompletten Zellmenge** (`HashSet<IntVec3>`),
  nicht mehr nur `RectShieldBounds`.
- `RectShieldSubstructure.Dilate(cells, pad)` erweitert die Form um N Kacheln
  in alle Richtungen (Chebyshev/8er-Nachbarschaft). Die Form bleibt erhalten:
  eine L-Form wird zu einer groesseren L-Form, nicht zu einem Quadrat.
- `RectShieldShape.BuildFromCells(...)` baut das Schild aus dieser Zellmenge.
  `RectShieldShape` unterstuetzt jetzt zwei Modi: klassisches Rechteck
  (`Build`) **oder** exakte Zellmenge (`HasCustomCells`). `Contains`, `IsEdge`,
  `Area`, `AllCells`, `EdgeCells` und `BlocksEntry` arbeiten in beiden Modi.
  Randzellen werden beim Aufbau einmalig gecacht (kein O(n)-Scan pro Frame).
- `GenDraw.DrawFieldEdges` bekommt im Rumpf-Modus `AllCells()` und zeichnet
  dadurch automatisch die **Aussenkontur der L-Form** (nicht die Box-Kanten).
- `Area` = echte Zellenzahl → **Strombedarf folgt jetzt der echten Form**, nicht
  mehr der aufgeblaehten Bounding Box. Eine L-Form ist damit guenstiger.
- Inspect-String zeigt beides: „Rumpf: N Platten -> M Feldzellen, Rand P".

### Verifikation
Geometrie-Test mit einer 6×6-L-Form (27 Platten, 9 Zellen Aussparung, Box = 36):
- Rand 0 → exakt 27 Feldzellen, **0/9** Aussparungszellen geschuetzt.
- `Contains`/`BlocksEntry` pruefen die echte Form: Schuss in die Luecke geht
  durch, Schuss auf einen Plattenarm wird gestoppt.
- Rand 1/2/3 → 40/55/72 Feldzellen; die Innenecken der L runden sich durch die
  Dilatation naturgemaess ab (bei Rand 3 ist die kleine Luecke gefuellt). Wer die
  exakte Form will, laesst den Rand auf **0** (Standard).

### Hinweis
C# geaendert -> DLLs fuer 1.6/1.7 in GitHub Actions neu gebaut.
## [0.3.1] - 2026-10-26 - Fix: Gravship-Feld lag asymmetrisch am Rumpf

### Fehlerbild (aus einem echten Spielstand gemeldet)
Das Feld schloss im **Osten** buendig mit den Bodenplatten ab, lag aber im
**Sueden/Norden 1 Kachel** und im **Westen 2 Kacheln** weiter aussen. Im Osten
stand eine Holzwand eines anderen Gebaeudes direkt an den Platten.

### Ursache
`RebuildShape()` berechnete das Feld ueber einen Integer-Mittelpunkt:

```csharp
int cx = (lastHull.MinX + lastHull.MaxX) / 2;   // bei gerader Breite 0,5 zu weit westlich
int halfW = fitW / 2;
minX = cx - halfW;
maxX = cx + (fitW - 1 - halfW);
```

Bei geraden Plattenbreiten rutscht `cx` um eine halbe Zelle nach Westen.
Zusammen mit der Integer-Division bei `halfW/halfH` und dem Padding ergab das
eine Verschiebung von **1–2 Kacheln** nach Westen bzw. Norden/Sueden.
Simulation (gerade x gerade): Raender **W=3, O=1, S=3, N=1**.

### Fix
- Feldgrenzen werden **direkt** aus den Plattengrenzen gebildet:
  `minX = MinX - pad`, `maxX = MaxX + pad` (analog Z). Kein Mittelpunkt,
  keine halben Breiten mehr.
- `minWidth`/`maxWidth`-Clamp arbeitet jetzt **symmetrisch von beiden Seiten**
  (`excess / 2` links, Rest rechts), damit keine Seite staerker beschnitten wird.
- Standard-`substructurePadding` von **2 auf 0** gesenkt: Das Feld liegt damit
  buendig auf den aeussersten Rumpfplatten – genau wie im Osten beobachtet.
  Es ragt nicht mehr in angrenzende fremde Gebaeude (Holzwand).
- Neuer Gizmo **„Rand: N Kacheln" / „Margin: N tiles"** zyklet 0–4 Kacheln Abstand.
  Der Wert wird in `rs_hullPadding` gespeichert und ist im Inspect-String sichtbar
  („Rumpf: N Platten, Rand M"). `ResetShape()` stellt den XML-Standard wieder her.
- Mathematisch geprueft fuer gerade/ungerade Breiten und jeden Rand 0–4:
  alle vier Raender sind exakt gleich. Nur bei Schiffen > 51 Kacheln greift der
  symmetrische Max-Clamp (Rest-Asymmetrie von maximal 1 Zelle).

### Hinweis
C# geaendert -> DLLs fuer 1.6/1.7 in GitHub Actions neu gebaut.
## [0.3.0] - 2026-10-26 - Symmetrische Schilde, +/- Gizmos, Gravship Shield

### Neu / geändert (drei Spieler-Wünsche)

1. **Mk I größer und symmetrisch.** Die drei festen Felder sind jetzt quadratisch
   und gleichmäßig abgestuft: **11×11**, **17×17**, **23×23** (vorher 9×7, 15×11,
   23×15). Kantenlängen ~20–30 % größer, ungerade Größen → Feld liegt exakt
   zentriert um den 2×2-Generator. Strom (150/250/400 W) und Energie
   (600/900/1200) blieben unverändert; DE/EN-Texte und `docs/BALANCE.md` angepasst.

2. **+/−-Grafiken für die einstellbare Größe.** Drei neue 64×64-RGBA-Icons unter
   `Common/Textures/UI/Gizmos/`: `RectShield_Plus.png`, `RectShield_Minus.png`
   und `RectShield_FitHull.png`. `RectShieldGizmos` lädt sie per
   `ContentFinder<Texture2D>.Get(path, false)` in `Command_Action.icon`
   (try/catch-Fallback: fehlt eine Textur, bleibt der Text-Button sichtbar).
   Die vier Breiten-/Höhen-Gizmos zeigen jetzt echte +/−-Symbole. Presets sind
   ebenfalls symmetrisch: 9×9, 15×15, 21×21, 31×31, 41×41, 51×51.

3. **Stufe 3 heißt jetzt „Gravship Shield" und legt sich um das Schiff.**
   - Neue Datei `Source/Core/RectShieldSubstructure.cs`: scannt alle Map-Zellen
     mit `map.terrainGrid.FoundationAt(cell)` und `Def.HasTag("Substructure")`
     und liefert die Bounding Box der Odyssey-Gravschiff-Bodenplatten.
   - `RectShieldShape.BuildFromWorldBounds(...)` baut das Feld direkt in
     Weltkoordinaten, zentriert auf das **Rumpfzentrum** statt auf den Generator.
   - `CompProperties_RectShield`: `autoFitSubstructure`, `substructurePadding = 2`,
     `substructureRefreshTicks = 180`. `CompRectShield` scannt alle 3 Sekunden,
     speichert den Schalter im Savegame und zeigt „Rumpf: N Platten" im Inspect-String.
   - Neuer Gizmo „Rumpf-Anpassung: an/aus" (DE) / „Fit hull: ON/OFF" (EN) schaltet
     den Auto-Fit ab; dann gelten wieder die manuellen +/−-Gizmos.
   - **Bewusst keine harten Odyssey-Typen** im Code (kein `Building_GravEngine`,
     `ModsConfig.OdysseyActive` oder `TerrainDef.IsSubstructure`). Ohne DLC werden
     einfach keine Platten gefunden und das manuelle Feld greift. Siehe
     `docs/API_NOTES.md`.
   - Umbenannt: Label „Gravship Shield" / „Gravschiff-Schild", Forschung
     „Gravship shielding" / „Gravschiff-Schilde", `RS_TierMk3` → „Gravship" /
     „Gravschiff". Der interne defName bleibt **`RectShield_MkIII`**, damit
     bestehende Spielstände nicht erneut brechen.

### Hinweise
- Das High-Tech-Sprite bleibt die Grafik der Stufe 3 (passt zum Gravschiff-Thema).
- C# wurde geändert → die DLLs für 1.6 und 1.7 werden in GitHub Actions neu gebaut.
- Der Auto-Fit schützt das **Volumen** der Bounding Box, nicht nur den Rand;
  `GenDraw.DrawFieldEdges` zeichnet die sichtbare Hülle um das Schiff.

## [0.2.1] - 2026-10-26 - Hotfix: ThingDef-defNames endeten auf einer Ziffer

### Fix (P0 - Spielstart)
RimWorld 1.6 meldete beim Laden für **fünf Defs** einen Config-Fehler:

```
Config error in RectShield_Mk2: ends with a numerical digit, which is not allowed on ThingDefs.
Config error in RectShield_Mk3: ends with a numerical digit, which is not allowed on ThingDefs.
Config error in Blueprint_RectShield_Mk2 / Frame_RectShield_Mk2 / ...
```

Ursache: `ThingDef.ConfigErrors()` verbietet defNames, deren letztes Zeichen eine
Ziffer ist. Die drei `Blueprint_`/`Frame_/`Blueprint_Install_`-Meldungen pro Stufe
sind **Folgefehler** derselben Ursache, weil RimWorld diese Defs automatisch aus
dem Gebäude-defName ableitet.

Geändert (nur die Gebäude-defNames und ihre direkten Referenzen):

| Alt | Neu |
|---|---|
| `RectShield_Mk2` | **`RectShield_MkII`** |
| `RectShield_Mk3` | **`RectShield_MkIII`** |

Angepasst: `Buildings_RectShield.xml`, deutsche DefInjected-Keys (`ThingDef`),
`Tests/validate_xml.py`, `Tests/check_structure.py`, `docs/PLAN.md`,
`docs/GRAPHICS.md`. **Nicht** geändert: Forschungs-defNames
(`ResearchProjectDef` hat diese Regel nicht), die Enum-Werte
`<tier>Mk2</tier>` / `<tier>Mk3</tier>`, Sprite-Dateinamen und alle Labels.
Die DLLs sind deshalb byte-identisch zu v0.2.0.

`Tests/validate_xml.py` prüft jetzt zusätzlich, dass **kein** ThingDef-defName
auf eine Ziffer endet, damit der Fehler nicht wiederkommt.

> ⚠️ **Save-Hinweis:** Wer mit v0.2.0 bereits Mk-II- oder Mk-III-Generatoren in
> einem Spielstand platziert hat, findet diese nach dem Update nicht wieder
> (defName geändert). Bei einem frischen Test-Save ist das irrelevant.

### Fund im echten Spiel-Log
Gemeldet unter RimWorld `1.6.4871 rev591` mit Harmony 2.4.3. Alle übrigen
Meldungen im Log (Workshop-Ordner fehlen, VWE-SoundDefs, VEF-StatDefs,
KCSG-Symbole, Tastenkonflikt `Z`, 105 deutsche Übersetzungsfehler) stammen von
anderen Mods und haben nichts mit RectShield zu tun.

## [0.2.0] - 2026-10-26 - Drei Ausbaustufen mit Mk III High-Tech

### Neu
- **Dritte Stufe `RectShield_MkIII`** (High-Tech-Schild) als echter neuer Gebäude-Def:
  frei einstellbar bis **51×51** Kacheln (Standard 17×13), **2400** Energie,
  **1,2** Regen/Tick, **900** Ticks Reboot, 120 Energie pro Drop Pod.
  Blockt wie Mk II direktes/indirektes Feuer und Skyfaller.
  Stromformel **500 W + 2 W pro Kachel** – effizienter als Mk II ab ~200 Kacheln.
  Baukosten 450 Stahl / 220 Plasteel / 24 Komponenten / 60 Gold, Baufähigkeit 12.
- **Neue Forschung `RectShield_Mk3_Research`** (2600 Forschungspunkte, nach Mk II)
  im eigenen Tab bei Position (3 / 1.4); deutsche DefInjected-Texte.
- **Drei KI-generierte Stufen-Sprites** (je 192×192 RGBA, freigestellt) plus passende
  64×64-Baumenü-Icons: Stufe 1 **Industriell** (Mk I, alle drei Größen),
  Stufe 2 **Militärisch** (Mk II), Stufe 3 **High-Tech** (Mk III).
  Dazu drei 128×128-Forschungs-Icons und ein überarbeitetes `About/Preview.png`.
- `RectShieldTier` um `Mk3` erweitert; eigenes helles Cyan-Overlay; Inspect-String
  zeigt „Mk III"; Preset-Liste um 51×35 erweitert (Vorlagen über dem
  Generator-Maximum werden automatisch ausgeblendet).
- `CanResize` gilt jetzt für alle nicht-fixen Stufen (`!Props.fixedSize`),
  damit Mk III dieselben Gizmos wie Mk II bekommt.
- `About.xml`: `author`/`url` auf das echte Repo gesetzt, Beschreibung auf drei Stufen.
- Tests prüfen Mk-III-Def, -Forschung, -Enum und alle neuen Grafikpfade.

### Hinweis
Die DLLs müssen wegen der C#-Änderungen neu gebaut werden (GitHub Actions).
Ein echter RimWorld-Spielstart-/Ingame-Test steht weiterhin aus.

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
