# RectShield – Rechteckige Schilde für RimWorld

Ein eigenständiger Mod, der **rechteckige Schutzfelder** statt der üblichen kreisförmigen Kuppeln erzeugt. Zwei Geräteklassen:

| | **Mk I** | **Mk II** |
|---|---|---|
| Größen | 3 feste Varianten: **9×7**, **15×11**, **23×15** | frei einstellbar **5…41** pro Achse |
| Blockt | **nur direkte Schüsse** | direkte Schüsse **+ Mörser/indirektes Feuer + Drop Pods, Meteoriten, Trümmer** |
| Versatz / Randmodus | nein | ja |
| Strombedarf | fix (150 / 250 / 400 W) | **300 W + 3 W pro geschützter Kachel** – steigt mit der Fläche |
| Energie | 600 / 900 / 1200 | 1400 |

Schüsse **aus dem Schild heraus** bleiben immer möglich – geblockt wird nur beim **Eintritt** in das Rechteck (`!Contains(from) && Contains(to)`).

Zielversionen: **RimWorld 1.6** (stabil) und **1.7 Waylith Beta**. Sprache: Deutsch + Englisch.

---

## Inhalt des Repositories

```
About/                     Mod-Metadaten, Preview.png
Common/Defs/               ThingDefs, Forschungsprojekte
Common/Languages/          EN + DE Keyed-Strings und DefInjected
Common/Textures/           Platzhalter-Sprites (austauschbar)
1.6/Assemblies/            hierhin kompiliert die net472-DLL
1.7/Assemblies/            hierhin kompiliert die net10.0-DLL
Source/                    C#-Quellcode + zwei csproj + Stubs
Tests/                     XML-Validierung, C#-Syntaxcheck, Strukturcheck
docs/                      Plan, API-Notizen, Balance
.github/workflows/         GitHub Actions
```

> ⚠️ **Wichtig:** Die kompilierten `RectShield.dll`-Dateien sind **nicht** im Repository. RimWorlds `Assembly-CSharp.dll` darf aus Lizenzgründen nicht mitgeliefert werden. Du musst die Mod einmal lokal gegen dein Spiel bauen (siehe unten). Alles andere – Defs, Grafiken, Texte, CI – ist fertig.

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

**Mk II** – Gizmos am Gebäude:

| Gizmo | Funktion |
|---|---|
| `− / Breite N / − / Höhe N` | Feld in 2-Kachel-Schritten vergrößern/verkleinern |
| `Größen-Vorlagen` | 9×7, 13×9, 17×13, 23×15, 31×21, 41×31 |
| `Feld verschieben` | Versatz in 4 Himmelsrichtungen |
| `Modus wechseln` | **Volumen** (ganzes Rechteck) ↔ **Rand** (nur die Wandzellen) |
| `Feld zurücksetzen` | Standard 13×9, Versatz 0, Volumen |
| `Anzeige umschalten` | Umrandung ein/aus |

**Mod-Einstellungen:** dauerhafte, dezente Umrandung aller Schilde; eigene/neutrale Drop Pods ebenfalls blocken; Strom- und Energiekosten-Multiplikatoren; Debug-Log.

Bei Überlastung (`Energie ≤ 0`) schaltet das Feld ab und startet nach 900–1200 Ticks mit 25 % Energie neu. EMP-Treffer entziehen überproportional Energie.

---

## Balance

Alle Werte stehen in [`docs/BALANCE.md`](docs/BALANCE.md) und lassen sich in `Common/Defs/ThingDefs_Buildings/Buildings_RectShield.xml` anpassen. Die Kernformel für Mk II:

```
Strom (W) = 300 + 3 × Breite × Höhe × Einstellungs-Faktor
```

Beispiele: 13×9 → **651 W**, 23×15 → **1335 W**, 41×31 → **4113 W**. Die maximale Schildenergie bleibt konstant – große Felder sind damit deutlich verwundbarer.

---

## Grafiken & Forschungsbaum

Jede Generator-Variante hat ein **eigenes Ingame-Sprite** (Mk I klein/mittel/groß mit 1/2/3 Emitter-Spulen, Mk II mit türkisfarbenem Kern und goldenen Verstell-Armen) sowie ein eigenes Icon für das Baumenü. Dazu kommen `About/ModIcon.png` für Ladebildschirm/Mod-Einstellungen und `About/Preview.png` für den Workshop.

Die Grafiken hängen über `<texPath>` und `<uiIconPath>` an den `ThingDef`s. **`uiIconPath` ist zugleich das Icon im Forschungsbaum**: RimWorld 1.6/1.7 haben kein `iconPath`-Feld an `ResearchProjectDef`, sondern zeigen die Icons der freigeschalteten Gebäude (`UnlockedDefs`). Deshalb war das der korrekte Einbindungsweg.

Beide Forschungen liegen in einem **eigenen Forschungs-Tab „RectShield"** (`ResearchTabDef`), Mk I bei Position (1 / 1.4), Mk II bei (2 / 1.4). Das vermeidet Kollisionen mit Vanilla-Koordinaten; die Y-Achse läuft in 0,7er-Schritten. Wer alles im Haupt-Tab möchte, entfernt die beiden `<tab>`-Zeilen. Alle Details, Pfade und Größen: [`docs/GRAPHICS.md`](docs/GRAPHICS.md).

Neue Grafiken erzeugen oder ersetzen: `python3 Tools/make_assets.py` – danach `python3 Tests/validate_xml.py`, das alle `texPath`/`uiIconPath`-Referenzen gegen die vorhandenen PNGs prüft.

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

`.github/workflows/validate.yml` führt diese Checks bei jedem Push aus. Der optionale `game-build`-Job kompiliert gegen echte Spiel-DLLs, sobald das Repository-Secret `RW17_MANAGED` gesetzt ist.

Bekannte Limitierungen und die Roadmap stehen in [`CHANGELOG.md`](CHANGELOG.md) und [`docs/PLAN.md`](docs/PLAN.md).

---

## Lizenz & Danksagung

Eigener Code, XML-Defs und Grafiken: **MIT** (siehe `LICENSE`). Es wurde **kein** Quellcode anderer Mods kopiert. Inspiration und API-Recherche: [Ludeon](https://ludeon.com/blog/2026/10/announcing-the-waylith-engine-and-update-1-7/), [RimWorld Wiki](https://rimworldwiki.com/wiki/Modding_Tutorials/RimWorld_1.7_Mod_Updates), [HarmonyRimWorld](https://github.com/pardeike/HarmonyRimWorld), [Frontier Developments Shields](https://github.com/rimworld-frontierdevelopment/Shields), [ED-Shields](https://github.com/jaxxa/ED-Shields). RimWorld ist ein Produkt von Ludeon Studios.
