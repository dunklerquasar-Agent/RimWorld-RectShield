# RectShield – Rechteckige Schilde für RimWorld

Ein eigenständiger Mod, der **rechteckige Schutzfelder** statt der üblichen kreisförmigen Kuppeln erzeugt. **Drei Ausbaustufen**, jede mit eigener Grafik:

| | **Stufe 1 – Mk I** (Industriell) | **Stufe 2 – Mk II** (Militärisch) | **Stufe 3 – Gravship Shield** (High-Tech) |
|---|---|---|---|
| Größen | 3 feste **symmetrische** Varianten: **11×11**, **17×17**, **23×23** | frei einstellbar **5…41** pro Achse | frei einstellbar **5…51** pro Achse |
| Blockt | **nur direkte Schüsse** | Direktfeuer **+ Mörser + Drop Pods, Meteoriten** | wie Mk II, mit mehr Reserve |
| Versatz / Randmodus | nein | ja | ja |
| Strombedarf | fix (150 / 250 / 400 W) | **300 W + 3 W/Kachel** | **500 W + 2 W/Kachel** (effizienter ab ~200 Kacheln) |
| Energie / Regen | 600 / 900 / 1200 · 0,5–0,7 | 1400 · 0,7 | **2400 · 1,2** |
| Forschung | 900 | 1600 (nach Mk I) | **2600 (nach Mk II)** |

Stufe 3 folgt **automatisch der exakten Form aller Gravschiff-Bodenplatten** (Odyssey-Substructure) – eine **L-Form bleibt eine L-Form**, die Aussparung wird weder geschützt noch berechnet. Der Strom richtet sich nach der echten Zellenzahl. Der Auto-Fit-Modus lässt sich per Gizmo abschalten.

Schüsse **aus dem Schild heraus** bleiben immer möglich – geblockt wird nur beim **Eintritt** in das Rechteck (`!Contains(from) && Contains(to)`).

Zielversionen: **RimWorld 1.6** (stabil) und **1.7 Waylith Beta**. Sprache: Deutsch + Englisch.

---

## Inhalt des Repositories

```
About/                     Mod-Metadaten, Preview.png
Common/Defs/               ThingDefs, Forschungsprojekte
Common/Languages/          EN + DE Keyed-Strings und DefInjected
Common/Textures/           3 Stufen-Sprites + Legacy-Platzhalter
1.6/Assemblies/            hierhin kompiliert die net472-DLL
1.7/Assemblies/            hierhin kompiliert die net10.0-DLL
Source/                    C#-Quellcode + zwei csproj + Stubs
Tests/                     XML-Validierung, C#-Syntaxcheck, Strukturcheck
docs/                      Plan, API-Notizen, Balance
.github/workflows/         GitHub Actions
```

> ✅ **Fertig zum Installieren:** Beide `RectShield.dll`-Dateien (1.6 / 1.7) sind im Repo und wurden in GitHub Actions gegen öffentliche Referenz-Assemblies gebaut. Es werden **keine** Spiel- oder Harmony-DLLs mitgeliefert – Harmony muss separat installiert sein.

---

## Installation (für Spieler)

1. **Harmony** installieren und in der Modliste ganz oben laden: [Workshop](https://steamcommunity.com/sharedfiles/filedetails/?id=2009463077).
2. Den Ordner `RimWorld-RectShield` (mit `About/`, `Common/`, `1.6/`, `1.7/`, `LoadFolders.xml`) nach
   `.../RimWorld/Mods/` kopieren – **nachdem** die DLLs gebaut wurden.
3. Mod aktivieren. Load order: `Core → Harmony → RectShield`.

---

## Kompilieren (einmalig nötig)

### Voraussetzungen

| Ziel | SDK | Harmony | Referenzen aus dem Spiel |
|---|---|---|---|
| 1.6 | .NET Framework 4.7.2 (bzw. .NET SDK mit `Microsoft.NETFramework.ReferenceAssemblies`) | **2.4.3** | `Assembly-CSharp.dll`, `UnityEngine.CoreModule.dll` |
| 1.7 | **.NET 10 SDK** (`net10.0`), VS2026 / Rider 2026 | **3.0** | `Assembly-CSharp.dll`, `UnityStubs.dll`, `Waylith.dll` |

**Wichtiger Unterschied:** Bei 1.6 liegen die Referenzen unter `RimWorld/RimWorldWin64_Data/Managed/`. Bei **1.7 (Waylith) existiert dieser Ordner nicht mehr** – `Assembly-CSharp.dll`, `UnityStubs.dll` und `Waylith.dll` liegen direkt im **Spiel-Installationsverzeichnis**. `$RW17` zeigt deshalb auf das Spiel-Root, nicht auf `Managed`. Für 1.7 die Dateien aus der **unstable**-Installation nehmen.

### Windows (PowerShell)

```powershell
$RW16 = "C:\Program Files (x86)\Steam\steamapps\common\RimWorld\RimWorldWin64_Data\Managed"
$RW17 = "C:\Games\RimWorld_1_7_Beta"   # Spiel-Root, KEIN Managed-Ordner
$HRM  = "C:\Program Files (x86)\Steam\steamapps\workshop\content\294100\2009463077"

dotnet build Source\RectShield.1.6.csproj -c Release -p:ManagedPath="$RW16" -p:HarmonyPath="$HRM\1.6\Assemblies"
dotnet build Source\RectShield.1.7.csproj -c Release -p:GameDir="$RW17" -p:HarmonyPath="$HRM\1.7\Assemblies"
```

Für Windows liegt zusätzlich das Paket `windows-p0/` bereit (`P0-Windows.md` mit `Build-RectShield.ps1`, `Check-API.ps1`, `Install-DevMod.ps1`).

### Automatisch in GitHub Actions (empfohlen)

Der Workflow [`.github/workflows/build.yml`](.github/workflows/build.yml) kompiliert **beide Versionen ohne lokale Spielinstallation**. Die RimWorld-Referenz-Assemblies kommen aus den öffentlichen NuGet-Paketen `Krafs.Rimworld.Ref` (1.6.4871 / 1.7.4985-unstable) und `Lib.Harmony.Ref` (2.4.3 / 3.0.0-rc.3) – es werden keine Spiel-DLLs hochgeladen.

- Start: bei jedem Push auf `main`, bei Pull Requests oder manuell über **Actions → Build RectShield → Run workflow** (dort lassen sich die vier Paketversionen übersteuern).
- Ergebnis: drei Artefakte (`RectShield-1.6-Assembly`, `RectShield-1.7-Assembly`, `RectShield-Release-Assemblies`), 90 Tage Aufbewahrung.
- Vor dem Build laufen `validate_xml.py`, `parse_csharp.py`, `check_structure.py`; danach prüft `verify_build.py`, dass beide DLLs PE-Header haben und keine `0Harmony.dll`/Spiel-DLL im Output liegt.
- Die heruntergeladenen DLLs gehören nach `1.6/Assemblies/` bzw. `1.7/Assemblies/`.

Die lokalen Projekte (`RectShield.1.6.csproj` / `RectShield.1.7.csproj`) bleiben für den Build gegen die echte Spielinstallation; die `*.NuGet.*`-Projekte sind die CI-Varianten. Beide teilen sich denselben Quellcode.

### Linux / macOS (bash)

```bash
RW16_MANAGED=~/Games/RimWorld/RimWorldWin64_Data/Managed \
RW17_DIR=~/Games/RimWorld_1_7_Beta \
HARMONY_2=~/Games/Harmony/1.6/Assemblies \
HARMONY_3=~/Games/Harmony/1.7/Assemblies \
./Source/build.sh
```

Danach liegen `1.6/Assemblies/RectShield.dll` und `1.7/Assemblies/RectShield.dll` bereit. `LoadFolders.xml` wählt automatisch die passende Version.

### Vor dem ersten Build: API-Recon (Pflicht)

RimWorld hat **keine offizielle Modding-API-Dokumentation**. Alle im Code verwendeten Spielmethoden sind in [`docs/API_NOTES.md`](docs/API_NOTES.md) als Checkliste aufgeführt. Öffne `Assembly-CSharp.dll` mit **ILSpy** und prüfe die Signaturen für deine genaue Spielversion. Besonders wichtig:

- `Projectile.Tick()` und `Skyfaller.Tick()` (unsere Harmony-Ziele),
- `ThingComp.PostDeSpawn(Map)` – in manchen Versionen parameterlos,
- `PlaceWorker.AllowsPlacing(...)` – Parameterzahl schwankt,
- `CompPowerTrader.PowerOutput` (setzbar?),
- `ProjectileProperties.flyOverhead` und `GetDamageAmount(float)`.

Wenn du nur **1.6** brauchst, lösche `1.7/` und entferne `<li>1.7</li>` aus `About.xml` und `LoadFolders.xml`.

---

## Bedienung

**Mk I:** Bauen, Strom anschließen, fertig. Das Gizmo zeigt die feste Größe an; das Feld wird bei Auswahl blau umrandet.

**Mk II und Gravship Shield** – identische Gizmos am Gebäude. Die vier Größen-Buttons haben jetzt eigene **+/−-Symbolgrafiken** (`UI/Gizmos/`), der Gravship Shield zusätzlich einen **Rumpf-Anpassungs-Schalter**:

| Gizmo | Funktion |
|---|---|
| `− / Breite N / − / Höhe N` | Feld in 2-Kachel-Schritten vergrößern/verkleinern |
| `Größen-Vorlagen` | quadratisch 9×9 … 51×51 (Vorlagen über dem Generator-Maximum werden ausgeblendet) |
| `Rumpf-Anpassung: an/aus` | nur Gravship Shield: Feld folgt automatisch allen Gravschiff-Bodenplatten (alle 3 s neu geprüft) |
| `Rand: N Kacheln` | Abstand des Feldes zu den äußersten Rumpfplatten, 0–4 (Standard **0** = bündig, symmetrisch in alle vier Richtungen) |
| `Feld verschieben` | Versatz in 4 Himmelsrichtungen |
| `Modus wechseln` | **Volumen** (ganzes Rechteck) ↔ **Rand** (nur die Wandzellen) |
| `Feld zurücksetzen` | Standard 13×9, Versatz 0, Volumen |
| `Anzeige umschalten` | Umrandung ein/aus |

**Mod-Einstellungen:** dauerhafte, dezente Umrandung aller Schilde; eigene/neutrale Drop Pods ebenfalls blocken; Strom- und Energiekosten-Multiplikatoren; Debug-Log.

Bei Überlastung (`Energie ≤ 0`) schaltet das Feld ab und startet nach 900–1200 Ticks mit 25 % Energie neu (Gravship Shield: 900 Ticks). EMP-Treffer entziehen überproportional Energie. Standardfeld Gravship Shield: 21×21, Reboot 900 Ticks, Baukosten 450 Stahl / 220 Plasteel / 24 Komponenten / 60 Gold, Baufähigkeit 12.

---

## Balance

Alle Werte stehen in [`docs/BALANCE.md`](docs/BALANCE.md) und lassen sich in `Common/Defs/ThingDefs_Buildings/Buildings_RectShield.xml` anpassen. Die Kernformeln:

```
Mk II :         Strom (W) = 300 + 3 × Breite × Höhe × Einstellungs-Faktor
Gravship:      Strom (W) = 500 + 2 × Breite × Höhe × Einstellungs-Faktor
```

Beispiele Mk II: 15×15 → **975 W**, 23×23 → **1887 W**, 41×41 → **5343 W**. Der Gravship Shield ist ab ~200 Kacheln günstiger (23×23 → **1558 W**) und hat mit 2400 statt 1400 Energie deutlich mehr Reserve.

---

## Grafiken & Forschungsbaum

**Drei Stufen = drei eigene Ingame-Sprites** (je 192×192 RGBA, freigestellt): Stufe 1 **Industriell** (genieteter Stahl, Bernstein-Kern) für alle drei Mk-I-Größen, Stufe 2 **Militärisch** (Panzerung, roter Diamant-Kern) für Mk II, Stufe 3 **High-Tech** (weiße Keramik, türkiser Hexagonal-Kern) für den Gravship Shield. Dazu passende 64×64-Baumenü-Icons und drei 128×128-Forschungs-Icons.

Die Grafiken hängen über `<texPath>` und `<uiIconPath>` an den `ThingDef`s. **`uiIconPath` ist zugleich das Icon im Forschungsbaum**: RimWorld 1.6/1.7 haben kein `iconPath`-Feld an `ResearchProjectDef`, sondern zeigen die Icons der freigeschalteten Gebäude (`UnlockedDefs`).

Alle **drei Forschungen** liegen in einem **eigenen Forschungs-Tab „RectShield"** (`ResearchTabDef`): Mk I (1 / 1.4), Mk II (2 / 1.4), Gravship Shield (3 / 1.4). Wer alles im Haupt-Tab möchte, entfernt die drei `<tab>`-Zeilen. Details: [`docs/GRAPHICS.md`](docs/GRAPHICS.md).

`python3 Tools/make_assets.py` erzeugt die Legacy-Platzhalter, Forschungs- und **Gizmo-Icons**, `ModIcon.png` und `Preview.png` neu – die drei Stufen-Sprites bleiben dabei unangetastet (Quelle: `generator_ansichten/prepare_sprites.py` im Workspace).

---

## Kompatibilität

- **Harmony** – Pflicht (`brrainz.harmony`), 2.4.3 für 1.6 / 3.0 für 1.7.
- **Combat Extended** – noch nicht getestet. CE nutzt eigene `ProjectileCE`-Klassen; dafür ist ein optionaler Zusatz-Patch nötig (siehe `docs/API_NOTES.md`).
- **Vanilla Expanded / VFE-Security / Frontier Developments Shields** – keine Def-Überschneidungen, eigene `defName`s und eigener Comp. Koexistenz sollte funktionieren, ist aber ungetestet.
- **Multiplayer / RimThreaded** – kein ungeseedetes `Rand`, kein geteilter statischer Zustand. Nicht getestet.
- **1.7 Waylith** – 99 % der 1.6-Mods laufen laut Ludeon unverändert; `supportedVersions` erst nach eigenem Testlauf bestätigen.

---

## Auf GitHub hochladen

Das Repository ist bereits als lokales Git-Repo vorbereitet (Branch `main`, initialer Commit). Drei Wege:

**A) Push auf ein bestehendes leeres Repo** (empfohlen):

```bash
./push-to-github.sh https://github.com/DEINNAME/RimWorld-RectShield.git
```

Oder von Hand:

```bash
git remote add origin https://github.com/DEINNAME/RimWorld-RectShield.git
git push -u origin main
```

**B) Per Browser:** Auf GitHub ein neues Repository anlegen, dann den Inhalt von `RimWorld-RectShield/` per Drag & Drop hochladen. Bequem: `RimWorld-RectShield.zip` im übergeordneten Workspace-Ordner.

**C) GitHub CLI:**

```bash
gh repo create RimWorld-RectShield --public --source=. --remote=origin --push
```

**D) Per API mit Token (ein Commit für alle Dateien):** Auf [github.com/new](https://github.com/new) ein **leeres** Repo anlegen, dann einen **feingranularen** Token erzeugen (*Settings → Developer settings → Personal access tokens → Fine-grained tokens*, Repository access = *Only select repositories* → dein Repo, Permission **Contents: Read and Write**):

```bash
GITHUB_TOKEN=github_pat_xxx GITHUB_REPO=DEINNAME/RimWorld-RectShield \
  python3 Tools/upload_to_github.py
```

Das Skript liest den Token nur aus der Umgebung, loggt ihn nicht und erzeugt genau einen Commit. **Token danach sofort widerrufen.** Ein `python3 Tools/upload_to_github.py --dry-run` zeigt vorher alle Dateien ohne Netzwerkzugriff.

Nach dem Push: `About/About.xml` → `packageId`, `author`, `url` auf deine Daten ändern. Für den Workshop-Upload `About/PublishedFileId.txt.example` in `PublishedFileId.txt` umbenennen und die Workshop-ID eintragen.

---

## Entwicklung / Tests

```bash
pip install tree-sitter tree-sitter-c-sharp
python3 Tests/validate_xml.py      # XML, defNames, Texturen, DE/EN-Keys
python3 Tests/parse_csharp.py      # C#-Syntax via tree-sitter
python3 Tests/check_structure.py   # Pflichtpfade
python3 Tools/make_assets.py       # Grafiken neu erzeugen (Pillow)
```

`.github/workflows/build.yml` führt diese Checks bei jedem Push aus und kompiliert danach beide DLLs in GitHub Actions.

Bekannte Limitierungen und die Roadmap stehen in [`CHANGELOG.md`](CHANGELOG.md) und [`docs/PLAN.md`](docs/PLAN.md).

---

## Lizenz & Danksagung

Eigener Code, XML-Defs und Grafiken: **MIT** (siehe `LICENSE`). Es wurde **kein** Quellcode anderer Mods kopiert. Inspiration und API-Recherche: [Ludeon](https://ludeon.com/blog/2026/10/announcing-the-waylith-engine-and-update-1-7/), [RimWorld Wiki](https://rimworldwiki.com/wiki/Modding_Tutorials/RimWorld_1.7_Mod_Updates), [HarmonyRimWorld](https://github.com/pardeike/HarmonyRimWorld), [Frontier Developments Shields](https://github.com/rimworld-frontierdevelopment/Shields), [ED-Shields](https://github.com/jaxxa/ED-Shields). RimWorld ist ein Produkt von Ludeon Studios.
