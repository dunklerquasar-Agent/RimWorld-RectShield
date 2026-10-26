# Projektplan: Rechteckiger Schildgenerator für RimWorld

**Arbeitstitel:** `RectShield – Rectangular Shield Generator`
**Stand:** 25.10.2026
**Zielgruppe:** RimWorld 1.6 (Main Branch), optional 1.7 (Waylith Beta)
**Dokumenttyp:** Umsetzungsplan / Spezifikation
**Status:** Phase M0–M2 umgesetzt (siehe Kapitel 0)

---

## 0. Finale Entscheidungen (25.10.2026)

Der ursprüngliche Plan unten beschreibt die allgemeine Architektur. Folgende Festlegungen haben Vorrang und sind im Repository umgesetzt:

| Punkt | Entscheidung | Umsetzung |
|---|---|---|
|Architektur|**Eigenständiger Mod** (Variante A/C), kein Addon auf eine fremde Schild-Mod|Eigener `CompRectShield`, eigene ThingDefs, eigener Namespace `RectShield`. Kein kopierter Fremdcode.|
|Versionen|**1.6 (stabil) und 1.7 Waylith Beta**|`LoadFolders.xml` mit `Common/` + `1.6/` bzw. `1.7/`; zwei csproj: `net472`/Harmony 2.4.3 und `net10.0`/Harmony 3.|
|Mk I|**Feste Größen, blockt nur Schüsse**|Drei Defs: 9×7, 15×11, 23×15. `blocksIndirectFire = false`, `blocksSkyfallers = false`, `fixedSize = true`, keine Größen-Gizmos.|
|Mk II|**Anpassbar, blockt auch Drop Pods, Energiebedarf steigt mit der Größe**|5…41 Kacheln pro Achse, Versatz, Randmodus, Presets. `blocksIndirectFire = true`, `blocksSkyfallers = true`. Strom = `300 W + 3 W × Fläche`.|
|Verteilung|GitHub-Repository|Vollständiges Git-Repo, CI-Checks, ZIP/Bundle für den Upload. Die DLL muss lokal gegen die eigene Spielinstallation gebaut werden (Lizenzgründe).|

**Abweichungen vom ursprünglichen Plan (bewusst):**

- Statt eines Harmony-Prefix auf `CompProjectileInterceptor.CheckIntercept` wird ein **Postfix auf `Projectile.Tick()`** verwendet. Grund: Harmony 3 prüft `__result`-Typen streng; `CheckIntercept` ist zwischen 1.6/1.7 zu instabil. Details in `docs/API_NOTES.md`.
- `CellRect` wird nicht direkt genutzt. `RectShieldShape` implementiert die Geometrie mit eigenen Integer-Grenzen, um Clipping und 90°-Rotation versionsunabhängig zu kontrollieren. `GenDraw.DrawFieldEdges` bleibt für die Visualisierung.
- Mk1/Mk2 sind **zwei Ausprägungen desselben Comps**, gesteuert über `CompProperties_RectShield.tier` und `fixedSize` – kein separater Codepfad.
- Explosions-Patches und `PlaceWorker.DrawGhost` sind auf v0.2 verschoben.

Die Meilensteine M0–M2 sind damit abgeschlossen (API-Checkliste, Projekt-Setup, Geometrie + Abfanglogik + Vorschau). M3–M6 (Gizmos/Energie sind bereits weitgehend enthalten, Deckel/CE-Patches, QA/Release) bleiben offen.

### Ergänzung 26.10.2026 – dritte Ausbaustufe (v0.2.0)

Der Wunsch nach drei eigenen Generator-Ansichten wurde als **echte dritte Stufe** umgesetzt, nicht nur als Grafiktausch:

| Stufe | Def | Grafik | Neuerung gegenüber Vorgänger |
|---|---|---|---|
| 1 | `RectShield_Mk1_S/M/L` | Industriell | feste Größen, nur Direktfeuer |
| 2 | `RectShield_Mk2` | Militärisch | anpassbar, Deckel gegen Skyfaller, 3 W/Kachel |
| 3 | `RectShield_Mk3` | High-Tech | bis 51×51, 2400 Energie, 1,2 Regen, **2 W/Kachel**, Reboot 900 |

Damit ist `RectShieldTier` dreistufig (`Mk1/Mk2/Mk3`), `CanResize` gilt für alle nicht-fixen Defs, und die Forschung hat einen dritten Knoten bei (3 / 1.4). Die Balance-Begründung (Mk III ist erst ab ~200 Kacheln effizienter als Mk II) steht in `docs/BALANCE.md`. Offen bleibt unverändert: Ingame-Smoke-Test, Workshop-Upload, CE-Kompatibilität.

---

## 1. Zielstellung

Ein baulicher Schildgenerator, der **kein kreis-/kugelförmiges Wirkungsgebiet** (Radius-Prüfung) erzeugt, sondern ein **frei konfigurierbares Rechteck** (Breite × Höhe in Kacheln) auf der Karte absichert.

**Kernunterschied zu bestehenden Mods:**

| Bestehende Schilde | Diese Mod |
|---|---|
| `(pos - center).LengthHorizontal <= radius` | `rect.Contains(pos)` |
| Kreis/Kuppel, immer zentriert auf dem Generator | Rechteck, Größe, Versatz und Ausrichtung einstellbar |
| Radius als einziger Parameter | Breite, Höhe, Anker/Offset, 90°-Rotation, Randverhalten |

**Nicht-Ziele für v1.0:** freie Winkelrotation (z. B. 37°), mehrere Etagen/Z-Levels, komplette Neuerfindung der Kampf-KI, eigene 3D-Assets.

---

## 2. Ausgangslage & Recherche (verifiziert)

| Thema | Erkenntnis | Konsequenz für den Plan |
|---|---|---|
| Spielversion | Der Main-Branch ist **1.6** (zuletzt 1.6.4850). **1.7** mit neuer **Waylith-Engine** ist seit 12.10.2026 auf dem *unstable*-Beta-Branch. 1.6er-Saves und die meisten Mods laufen weiter. ([Ludeon](https://ludeon.com/blog/2026/10/announcing-the-waylith-engine-and-update-1-7/), [Wiki Version History](https://rimworldwiki.com/wiki/Version_history)) | Primär für **1.6 net472** entwickeln, `LoadFolders.xml` für `1.6/` + `1.7/` anlegen. |
| Toolchain 1.7 | .NET 10 (`net10.0`), SDK-style csproj, VS2026/Rider 2026, Referenzen `Assembly-CSharp.dll`, `UnityStubs.dll`, `Waylith.dll`; Unity-Stubs bleiben nutzbar. ([RimWorld Wiki – 1.7 Mod Updates](https://rimworldwiki.com/wiki/Modding_Tutorials/RimWorld_1.7_Mod_Updates), [Setting up a solution](https://rimworldwiki.com/wiki/Modding_Tutorials/Setting_up_a_solution)) | Build-System von Anfang an **dual-fähig** aufsetzen, nicht nachrüsten. |
| Harmony | 1.6 nutzt offizielles Harmony **2.4.3** (.NET Framework 4.7.2), 1.7 braucht **Harmony 3** (.NET 10). Harmony 3 prüft Typen von `__instance`/`__result` strenger. ([HarmonyRimWorld](https://github.com/pardeike/HarmonyRimWorld), [Steam Workshop Harmony](https://steamcommunity.com/sharedfiles/filedetails/?id=2009463077)) | **Keine** `0Harmony.dll` bündeln; `brrainz.harmony` als `modDependencies` + `loadAfter` deklarieren; Patch-Signaturen exakt typisieren. |
| Vanilla-Basis | `RimWorld.CompProjectileInterceptor` ist das Vanilla-System für Schilde (Mech-Cluster, Odyssey-Gravship-Schild). Es ist **radius-basiert** und wird von anderen Mods bereits per Harmony (`CheckIntercept`) gepatcht. ([Gravship shield generator](https://rimworldwiki.com/wiki/Gravship_shield_generator), [Shield Expanded](https://steamcommunity.com/sharedfiles/filedetails/?id=3575073524)) | Beste Abkürzung: Vanilla-Energie-/Abfanglogik nutzen und nur die **Formprüfung** ersetzen. |
| Bestehende Schild-Mods | VFE-Security hat `BuildingShield.cs` mit Radius-Logik ([Diskussion](https://steamcommunity.com/workshop/filedetails/discussion/1845154007/1631916406862211453/)). **Frontier Developments Shields** ist explizit erweiterbar: „New shields can be implemented from `IShield` like `Comp_ShieldRadial`" ([README](https://github.com/rimworld-frontierdevelopment/Shields/blob/master/README.md)). ED-Shields ebenfalls offen ([GitHub](https://github.com/jaxxa/ED-Shields)). | Zwei Wege möglich: **Addon** (schnell) oder **eigenständige Mod** (sauber). Siehe Kap. 3. |
| API-Realität | Es gibt **keine offizielle Modding-API-Doku**; Signaturen müssen per Decompiler (ILSpy) gegen die eigene `Assembly-CSharp.dll` geprüft werden. ([Wiki](https://rimworldwiki.com/wiki/Modding_Tutorials), [API-Referenz-Projekt](https://github.com/zyphraxns/rimworld-modding)) | **Phase 0 = API-Recon** ist ein eigener Meilenstein, nicht „nebenbei". |
| Visualisierung | `GenDraw.DrawFieldEdges(List<IntVec3>, Color, float?)` zeichnet Zellränder; `DrawExtraSelectionOverlays()` ist der Standard-Hook für Auswahl-Overlays. ([Debug Drawing](https://github.com/Dev-Jahn/rimworld-modding-reference/blob/main/08-advanced-topics/debugging.md)) | Perfekt für Rechteck-Vorschau ohne eigene Shader. |

> ⚠️ Alle im Folgenden genannten Methodennamen sind **Kandidaten** und müssen in Phase 0 per ILSpy gegen die tatsächlich installierte Spielversion verifiziert werden. Das ist der häufigste Fehler bei RimWorld-Modding.

---

## 3. Architekturentscheidung (wichtigste Weiche)

| | **Variante A – Eigenständig** | **Variante B – Addon/Patch** | **Variante C – Hybrid (Empfehlung)** |
|---|---|---|---|
| Idee | Eigener `ThingComp` + eigene Harmony-Patches auf `Projectile`/`Skyfaller`/`Explosion` | Bestehende Schild-Mod (z. B. VFE-Security) patchen: Radius-Check → `CellRect.Contains` | Eigene `CompRectShield`-Klasse, aber Vanilla `CompProjectileInterceptor` als Energie-/Schadensbasis; Harmony-Prefix auf `CheckIntercept` ersetzt nur die Geometrie |
| Aufwand | Hoch (3–5 Tage bis MVP) | Gering (0,5–1 Tag bis MVP) | Mittel (1,5–2 Tage bis MVP) |
| Kontrolle | Voll | Gering, abhängig von Fremdcode | Hoch bei Geometrie, Vanilla bei Balance |
| Update-Risiko | Niedrig | **Hoch** (jedes Update der Basis-Mod bricht den Patch) | Mittel |
| Rechtliche Lage | Sauber | Fremdcode nur zur Laufzeit per Harmony patchen, **nicht** kopieren; Lizenzen prüfen (Frontier Shields: CC BY-NC-SA 4.0, [LICENSE](https://github.com/rimworld-frontierdevelopment/Shields/blob/master/LICENSE.txt)) | Sauber |
| Workshop | Eigenständig veröffentlichbar | Nur als „Patch" mit `loadAfter` + `MayRequire` | Eigenständig veröffentlichbar |

**Empfehlung:** **Variante C** als Hauptweg, mit einem **B-MVP in Phase 1** zum schnellen Beweis des Konzepts.

Begründung: Der eigentliche Wunsch ist eine Geometrie-Änderung. Alles andere (Energie, EMP, Abfangen von Projektilen, visueller Ring) existiert bereits – entweder in Vanilla (`CompProjectileInterceptor`) oder in etablierten Mods. Ein kompletter Neubau (A) wäre für v1.0 Overengineering. Sollte sich in Phase 0 zeigen, dass `CompProjectileInterceptor` zu stark an `radius` gekoppelt ist, wird auf A eskaliert.

**Fallback-Regel:** Wenn `CheckIntercept` in der Zielversion nicht sauber patchbar ist (z. B. Inlining, Transpiler-Konflikte mit Combat Extended), wird stattdessen ein eigener `Harmony`-Prefix auf `Verse.Projectile.CheckForFreeInterceptBetween(...)` gesetzt – so wie es [Frontier Developments Shields mit `Harmony_Projectile`](https://github.com/rimworld-frontierdevelopment/Shields) macht.

---

## 4. Funktionale Spezifikation

### 4.1 Form & Geometrie

- **F1 – Grundform:** Achsenparalleles Rechteck aus `w × h` Kacheln, Standard `12 × 8`, Grenzen `3…41` pro Achse (konfigurierbar in Mod Settings).
- **F2 – Anker:** Das Rechteck ist **nicht** zwingend um den Generator zentriert. Der Spieler setzt einen Versatz (`offsetX`, `offsetZ`), z. B. „Schild beginnt 2 Kacheln vor dem Generator und geht 20 nach vorne". Damit lassen sich Wälle/Killboxen exakt abdecken.
- **F3 – Ausrichtung:** 90°-Rotation über die Gebäude-`Rot4`. Bei Ost/West werden Breite und Höhe intern getauscht; die Logik bleibt daher immer ein einfaches `CellRect`.
- **F4 – Abfangregel (Kern):**
  ```
  blocked = !rect.Contains(fromCell) && rect.Contains(toCell)
  ```
  Ein Projektil wird also beim **Eintritt** in das Rechteck gestoppt. Schüsse **von innen nach außen** bleiben erlaubt (bewährtes Verhalten aller gängigen Schild-Mods).
- **F5 – Randmodus:** Umschaltbar zwischen
  - `Volume` (jede Zelle im Rechteck ist geschützt – einfach, MVP),
  - `Perimeter` (nur die Randzellen sind „Wand"; im Inneren kann normal geschossen werden – taktisch besser für Höfe).
- **F6 – Deckel (optional, ab v1.1):** `blockSkyfallers` – Mörsergranaten, Drop Pods, Meteoriten, Trümmer und Orbitalbombardement über dem Rechteck abfangen. Als eigener Schalter, da das Balance-technisch massiv ist (VFE-Security nimmt dafür z. B.Extra-Energie).
- **F7 – Kanten-Clipping:** `rect.ClipInsideMap(map)` verhindert Out-of-Bounds an Kartenrändern; abgeschnittene Fläche reduziert anteilig die Energiekosten.

### 4.2 Energie, Balance, Betrieb

- **F8 – Energie:** `energy` (0…`maxEnergy`), Regeneration pro Tick, Verlust proportional zum Projektilschaden. EMP entlädt überproportional. Bei `energy <= 0` → Schild offline, automatischer Neustart nach `rebootTicks` (z. B. 600 Ticks).
- **F9 – Strom:** `CompPowerTrader` mit Grundlast + Flächenlast:
  `idlePowerW = baseW + (w * h) * wPerCell` (z. B. 200 W + 2 W/Zelle). Größere Schilde sind teurer, aber nicht stärker.
- **F10 – Stabilität:** `CompFlickable` (Ein/Aus), `CompBreakdownable`, `CompHeatPusher` (oder Hitze-Barometer), Forschungsprojekt `RectShieldResearch`, Baukosten (Stahl/Plasteel/Advanced Component).
- **F11 – Überlappung:** Mehrere Schilde dürfen sich überlappen. Es gewinnt der Schild mit der **nächstgelegenen Eintrittskante**; Energie wird nur **einmal** abgezogen (dediziertes `handled`-Flag pro Projektil/Tick).
- **F12 – Fraktionslogik:** Eigene Kolonie-Schilde blocken feindliche Projektile, lassen eigene austreten. Optional: Schilde blocken auch neutrale/handels-Schiffe (Mod-Einstellung).

### 4.3 UI/UX

- **F13 – Gizmos am Gebäude:**
  - `Breite − / +`, `Höhe − / +` (oder `Dialog_Slider`),
  - `Versatz` (4-Wege-Pfeil-Gizmos),
  - `Modus: Volumen / Rand`,
  - `Deckel: an/aus`,
  - `Farbe` (Farbwahl),
  - `Vorschau: an/aus`.
- **F14 – Visualisierung:**
  - Bei Auswahl: `GenDraw.DrawFieldEdges(edgeCells, color)` – zeigt exakt die geschützten Kacheln,
  - Dauerhaft: dezente Randlinie (`AltitudeLayer.MetaOverlays`), Hit-Effekt (`FleckMaker`/Mote) an der Einschlagstelle,
  - Platzierungsvorschau über `PlaceWorker` + `DrawExtraSelectionOverlays()`.
- **F15 – Inspect-String:** „Rechteck 12×8 | 96 Zellen | Energie 780/1000 | 392 W | Deckel: aus".
- **F16 – Mod Settings:** Maximalgröße, Energiewerte, Farbe, Standardmodus, Debug-Logging, „Deckel erlauben".
- **F17 – Lokalisierung:** `Languages/German/Keyed/*.xml` + `English`; alle Strings als Keys, keine Hartcodierung.

### 4.4 Nicht-funktional

- **NF1 – Performance:** O(1)-Formprüfung; keine Distanzberechnung pro Projektil; keine Allokationen im Tick-Hot-Path.
- **NF2 – Persistenz:** Alle Parameter in `ExposeData()` via `Scribe_Values`; Savegame-kompatibel, alte Saves bekommen Defaults.
- **NF3 – Kompatibilität:** Harmony, VFE/VFECore, Combat Extended (optional), Multiplayer (deterministisch, kein ungeseedetes `Rand`), RimThreaded (keine geteilten statischen Mutable-States ohne Lock).
- **NF4 – Versionen:** `About.xml` mit `<supportedVersions><li>1.6</li><li>1.7</li></supportedVersions>`, `LoadFolders.xml` für versionsspezifische Assemblies.
- **NF5 – Recht:** Kein Copy-Paste aus fremden Mods; nur eigene Logik + Harmony-Patches zur Laufzeit; Lizenzhinweise in `README.md`.

---

## 5. Technisches Design

### 5.1 Projektstruktur

```
RectShield/
├── About/
│   ├── About.xml
│   ├── Preview.png                 # 512×512
│   └── PublishedFileId.txt         # nach Workshop-Upload
├── LoadFolders.xml
├── 1.6/
│   └── Assemblies/RectShield.dll   # net472, Harmony 2.4.3
├── 1.7/
│   └── Assemblies/RectShield.dll   # net10.0, Harmony 3.0
├── Common/
│   ├── Defs/
│   │   ├── ThingDefs_Buildings/Buildings_RectShield.xml
│   │   ├── ResearchProjectDefs/Research_RectShield.xml
│   │   └── StatDefs/Stats_RectShield.xml
│   ├── Patches/                   # optionale PatchOperations für Fremd-Mods
│   ├── Languages/
│   │   ├── German/Keyed/RectShield_Keys.xml
│   │   └── English/Keyed/RectShield_Keys.xml
│   └── Textures/Things/Building/RectShield/
├── Source/
│   ├── RectShield.csproj
│   ├── RectShieldMod.cs           # ModSettings + StaticConstructorOnStartup
│   ├── RectShieldShape.cs         # Geometrie (reine Logik, unit-testbar)
│   ├── CompProperties_RectShield.cs
│   ├── CompRectShield.cs          # Zustand, Ticks, Gizmos, Scribe
│   ├── RectShieldMapComponent.cs  # Registry/Cache aller aktiven Schilde pro Map
│   ├── PlaceWorker_RectShield.cs
│   ├── Patches/
│   │   ├── Patch_ProjectileInterceptor.cs
│   │   ├── Patch_Projectile.cs
│   │   ├── Patch_Skyfaller.cs
│   │   └── Patch_Explosion.cs
│   ├── Gizmos/Command_RectShieldSize.cs
│   └── Util/DebugDraw.cs
├── Tests/RectShield.Tests/
├── LICENSE
├── CHANGELOG.md
└── README.md
```

### 5.2 Geometrie-Kern (Skizze)

```csharp
using Verse;

namespace RectShield
{
    /// Reine Geometrie – keine RimWorld-Abhängigkeiten im Konstruktor,
    /// daher leicht unit-testbar.
    public struct RectShieldShape
    {
        public int Width, Height;
        public IntVec3 Origin;      // untere linke Ecke, bereits rotiert/clipped
        public bool PerimeterOnly;

        private CellRect rect;      // gecacht
        private HashSet<IntVec3> cellSet;   // gecacht für O(1)

        public RectShieldShape(IntVec3 anchor, Rot4 rot, int width, int height,
                               IntVec3 offset, Map map)
        {
            // FacingCell = "nach vorne", RighthandCell = "nach rechts"
            var fwd   = rot.FacingCell;
            var right = rot.RighthandCell;   // Name per ILSpy prüfen, sonst eigene Rotation
            IntVec3 start = anchor + offset;
            // ... Ecken aus fwd/right * width/height berechnen, minX/minZ bilden
            rect = new CellRect(start.x, start.z, width, height);
            rect = rect.ClipInsideMap(map);  // Signature prüfen
            // cellSet neu aufbauen (nur bei Änderung!)
            this.Width = width; this.Height = height; this.Origin = start;
            this.PerimeterOnly = false;
            this.cellSet = null;
        }

        public bool Contains(IntVec3 c) => rect.Contains(c);

        /// F4: nur Eintritt blockieren, Austritt erlauben.
        public bool BlocksEntry(IntVec3 from, IntVec3 to)
            => !Contains(from) && Contains(to);

        public IEnumerable<IntVec3> EdgeCells => rect.EdgeCells; // ILSpy prüfen
        public int Area => rect.Area;
    }
}
```

**Warum `CellRect`?** Es ist die Vanilla-Datenstruktur für rechteckige Zellbereiche (u. a. bei `GenAdj.OccupiedRect`, `GenSpawn.WipeExistingThings`). Sie liefert `Contains`, `EdgeCells`, `Cells`, `InBounds`, `ClipInsideMap`, `ContractedBy` – genau das, was wir brauchen. ([GenSpawn-Dekompilat](https://github.com/josh-m/RW-Decompile/blob/master/Verse/GenSpawn.cs))

### 5.3 Zustand & Persistenz (Skizze)

```csharp
public class CompRectShield : ThingComp
{
    private int width = 12, height = 8, offsetX = 0, offsetZ = 0;
    private bool perimeterOnly = false, blockSkyfallers = false;
    private float energy = 1000f;
    private RectShieldShape shape;
    private bool shapeDirty = true;

    public bool Active =>
        parent.Spawned && PowerOn && energy > 0f && !parent.IsBrokenDown();

    public RectShieldShape Shape {
        get {
            if (shapeDirty) { RebuildShape(); shapeDirty = false; }
            return shape;
        }
    }

    public override void PostExposeData() {
        Scribe_Values.Look(ref width,  "width", 12);
        Scribe_Values.Look(ref height, "height", 8);
        Scribe_Values.Look(ref offsetX, "offsetX", 0);
        Scribe_Values.Look(ref offsetZ, "offsetZ", 0);
        Scribe_Values.Look(ref perimeterOnly, "perimeterOnly", false);
        Scribe_Values.Look(ref blockSkyfallers, "blockSkyfallers", false);
        Scribe_Values.Look(ref energy, "energy", 1000f);
        if (Scribe.mode == LoadSaveMode.PostLoadInit) shapeDirty = true;
    }

    public override void CompTick() {
        if (!Active) return;
        energy = Mathf.Min(MaxEnergy, energy + RegenPerTick);
    }

    public void DrawSurcharge(float damage) { /* Energie abziehen, Hit-Effekt, Breakdown-Check */ }

    public override IEnumerable<Gizmo> CompGetGizmosExtra() { /* F13 */ }

    public override void DrawExtraSelectionOverlays() {
        // F14: exakte Rechteck-Vorschau
        GenDraw.DrawFieldEdges(Shape.EdgeCells.ToList(), ShieldColor);
    }
}
```

### 5.4 Abfanglogik (Skizze, Variante C)

```csharp
[StaticConstructorOnStartup]
public static class HarmonyInit
{
    static HarmonyInit()
    {
        var h = new Harmony("yourname.rectshield");

        // Zielmethode NICHT per [HarmonyPatch]-Attribut raten, sondern explizit
        // auflösen – so fallen Overload-Änderungen sofort auf.
        var m = AccessTools.Method(typeof(CompProjectileInterceptor), "CheckIntercept");
        if (m == null) { Log.Error("[RectShield] CheckIntercept nicht gefunden – API-Recon nötig"); return; }

        h.Patch(m, prefix: new HarmonyMethod(typeof(Patch_ProjectileInterceptor), nameof(Patch_ProjectileInterceptor.Prefix)));
        // Fallback: Patch_Projectile auf Verse.Projectile.CheckForFreeInterceptBetween
    }
}

public static class Patch_ProjectileInterceptor
{
    // Harmony 3 prüft __instance/__result-Typen exakt → Signaturen in Phase 0 verifizieren!
    public static bool Prefix(CompProjectileInterceptor __instance, Projectile p,
                              Vector3 from, Vector3 to, ref bool __result)
    {
        var rect = __instance.parent.TryGetComp<CompRectShield>();
        if (rect == null || !rect.Active) return true;   // Vanilla-Logik belassen

        IntVec3 a = from.ToIntVec3(), b = to.ToIntVec3();
        if (!rect.Shape.BlocksEntry(a, b)) { __result = false; return false; }

        rect.DrawSurcharge(p.def.projectile.GetDamageAmount(1f));
        __result = true;
        return false;                                     // Original überspringen
    }
}
```

**Zusätzliche Patch-Ziele (alle in Phase 0 verifizieren):**

| Ziel | Zweck | Priorität |
|---|---|---|
| `RimWorld.CompProjectileInterceptor.CheckIntercept` | Direkt-/Indirektfeuer, Energie | P0 (MVP) |
| `Verse.Projectile.CheckForFreeInterceptBetween` | Fallback für Custom-Projectiles, CE | P0 |
| `RimWorld.Explosion` / `DamageWorker`-Pfad | Explosionen dringen nicht ins Rechteck | P1 |
| `RimWorld.Skyfaller` / `DropPodIncoming` / `Bombardment` | „Deckel" F6 | P1 |
| `RimWorld.CompShield.CompAllowVerbCast` | KI schießt nicht durch eigenen Schild | P2 |
| `Verse.GenDraw.DrawRadiusRing` (nur kosmetisch) | Runden Ring durch Rechteck-Rand ersetzen | P2 |

### 5.5 XML-Defs (Skizze)

```xml
<!-- Common/Defs/ThingDefs_Buildings/Buildings_RectShield.xml -->
<Defs>
  <ThingDef ParentName="BuildingBase">
    <defName>RectShieldGenerator</defName>
    <label>rechteckiger Schildgenerator</label>
    <description>Erzeugt ein rechteckiges Schutzfeld statt einer Kuppel.</description>
    <thingClass>Building</thingClass>
    <size>(2,2)</size>
    <rotatable>true</rotatable>
    <tickerType>Normal</tickerType>
    <researchPrerequisites><li>RectShieldResearch</li></researchPrerequisites>
    <costList>
      <Steel>180</Steel><Plasteel>60</Plasteel>
      <ComponentIndustrial>8</ComponentIndustrial><ComponentSpacer>2</ComponentSpacer>
    </costList>
    <comps>
      <li Class="CompProperties_Power"><compClass>CompPowerTrader</compClass>
        <basePowerConsumption>200</basePowerConsumption></li>
      <li Class="CompProperties_Flickable"><compClass>CompFlickable</compClass></li>
      <li Class="CompProperties_Breakdownable"><compClass>CompBreakdownable</compClass></li>
      <li Class="RectShield.CompProperties_RectShield">
        <maxEnergy>1000</maxEnergy>
        <regenPerTick>0.8</regenPerTick>
        <energyPerDamage>1.0</energyPerDamage>
        <minWidth>3</minWidth><maxWidth>41</maxWidth>
        <minHeight>3</minHeight><maxHeight>41</maxHeight>
        <wattsPerCell>2</wattsPerCell>
      </li>
    </comps>
    <placeWorkers><li>RectShield.PlaceWorker_RectShield</li></placeWorkers>
  </ThingDef>
</Defs>
```

```xml
<!-- About/About.xml -->
<?xml version="1.0" encoding="utf-8"?>
<ModMetaData>
  <packageId>yourname.rectshield</packageId>
  <name>RectShield – Rectangular Shield Generator</name>
  <author>yourname</author>
  <supportedVersions><li>1.6</li><li>1.7</li></supportedVersions>
  <description>Rechteckige Schilde statt Kuppel. Breite, Höhe, Versatz und Randmodus einstellbar.</description>
  <modDependencies>
    <li>
      <packageId>brrainz.harmony</packageId>
      <displayName>Harmony</displayName>
      <steamWorkshopUrl>steam://url/CommunityFilePage/2009463077</steamWorkshopUrl>
      <downloadUrl>https://github.com/pardeike/HarmonyRimWorld/releases/latest</downloadUrl>
    </li>
  </modDependencies>
  <loadAfter><li>brrainz.harmony</li><li>Ludeon.RimWorld</li></loadAfter>
</ModMetaData>
<!-- Siehe: https://rimworldwiki.com/wiki/Modding_Tutorials/About.xml -->
```

```xml
<!-- LoadFolders.xml -->
<loadFolders>
  <v1.6>
    <li>Common</li>
    <li>1.6</li>
  </v1.6>
  <v1.7>
    <li>Common</li>
    <li>1.7</li>
  </v1.7>
</loadFolders>
```

---

## 6. Performance-Konzept

1. **Cache statt Neuberechnung:** `RectShieldShape` wird nur bei `shapeDirty` neu gebaut (Größen-/Offset-/Rotationsänderung, Spawn, Load). Pro Tick/Projektil nur `Contains`.
2. **Registry pro Map:** `RectShieldMapComponent` hält eine `List<CompRectShield>` aktiver Schilde; Registration über `PostSpawnSetup`/`PostDeSpawn` (Harmony-Prefix auf `Thing.SpawnSetup`/`DeSpawn`) oder Rare-Tick-Refresh als Fallback. Kein `map.listerThings.AllThings` pro Projektil.
3. **Früh exit:** Erst `Active`, dann Grobtest mit `CellRect` (2 Integer-Vergleiche pro Achse), erst dann Energie/Schaden.
4. **Randmodus:** `HashSet<IntVec3>` der Randzellen einmalig aufbauen → O(1)-Suche statt `w*h`-Scan.
5. **Overlays:** `DrawFieldEdges` nur bei Selektion oder aktivem Vorschau-Modus; Dauer-Overlay als ein einziges Mesh/Graphic, nicht pro Zelle.
6. **Messung:** Dev-Modus + `Ctrl+F12`-Log, 60 s Belastungstest mit 200 Projektilen/Sek. auf 40×40-Feld; Ziel < 0,3 ms/Tick zusätzlich.

---

## 7. Kompatibilitäts-Matrix

| Mod / System | Risiko | Maßnahme |
|---|---|---|
| **Combat Extended** | Hoch – eigene `ProjectileCE`, eigene Intercept-Pfade | Zusätzlichen Patch auf `ProjectileCE` (optional, per `MayRequire`/Reflection), nie hart referenzieren; Testsave mit CE 7.x |
| **Vanilla Expanded Framework / VFE-Security** | Mittel – eigene Schildklassen | Keine Überschneidung: eigener `defName`, eigener Comp; optionaler `PatchOperation` für Balance |
| **Frontier Developments Shields** | Mittel – `IShield` + eigene `Harmony_Projectile` | Entweder als optionale Basis (B-Variante) oder Koexistenz-Test; **Lizenz CC BY-NC-SA beachten** |
| **Multiplayer / RimThreaded** | Mittel | Kein ungeseedetes `Rand`, kein statischer veränderlicher Zustand ohne Sync, keine UI-Aktion ohne `SyncMethod` |
| **Odyssey / Gravship-Schilde** | Gering | Beide nutzen `CompProjectileInterceptor` → Patch darf Vanilla-Instanzen ohne `CompRectShield` unverändert durchlaufen lassen (siehe `return true` in 5.4) |
| **RimWorld 1.7 (Waylith)** | Mittel – Beta, Harmony 3, .NET 10 | Separater Build; `supportedVersions` erst nach bestandenem Testlauf hochsetzen; `UnityStubs.dll` statt Unity-Referenzen |
| **Z-Levels / Multi-Floor-Mods** | Niedrig für v1.0 | bewusst out of scope, in README dokumentieren |

---

## 8. Testplan

| ID | Testfall | Erwartung |
|---|---|---|
| T01 | Schuss von außen auf Ziel im Rechteck | Projektil wird an der **Eintrittskante** gestoppt, Energie sinkt |
| T02 | Schuss von innen nach außen | Projektil fliegt **unghindert** |
| T03 | Schuss komplett außerhalb | Kein Eingriff, keine Energiekosten |
| T04 | Schuss, der das Rechteck nur tangiert (Ecke) | Wird korrekt als Eintritt erkannt |
| T05 | Breite 41 × Höhe 3 (langer Wall) | Rechteck sichtbar, Performance ok, kein Kreis |
| T06 | Rotation N/O/S/W | Breite/Höhe tauschen korrekt, Offset wandert mit |
| T07 | Versatz + Offset gegen Kartenrand | `ClipInsideMap` schneidet ab, keine Out-of-Bounds-Exception |
| T08 | Randmodus `Perimeter` | Innenräume beschießbar, Rand blockt |
| T09 | Deckel an: Mörser + Drop Pod + Meteorit | Alle werden abgefangen, Energieverbrauch spürbar |
| T10 | EMP-Treffer | Energie stark reduziert, Schild ggf. offline, Reboot nach N Ticks |
| T11 | Speichern/Laden mit geänderter Größe + Farbe | Alle Werte exakt wiederhergestellt |
| T12 | Zwei überlappende Schilde | Nur ein Energieabzug pro Projektil |
| T13 | 1.6-Save in 1.7-Beta laden | Keine Fehler im Log, Schild funktioniert |
| T14 | Mit Combat Extended laden | Kein roter Log, Projektil-Interception funktioniert |
| T15 | 200 Projektile/s über 60 s | Kein TPS-Einbruch, kein Memory-Leak |
| T16 | Generator abbrechen/entfernen | Registry wird bereinigt, keine NullReference |

**Werkzeuge:** Dev-Modus, `DebugAction`-Testaktionen (Projektil-Spawner), `Ctrl+F12`-Log (HugsLib-Log Uploader), ILSpy für Signatur-Checks, GitHub Actions für Build 1.6 + 1.7.

---

## 9. Meilensteine & Aufwand

| M# | Inhalt | Ergebnis | Aufwand* |
|---|---|---|---|
| **M0** | API-Recon: `Assembly-CSharp.dll` 1.6 + 1.7 dekompilieren, `CompProjectileInterceptor`, `Projectile`, `CellRect`, `Skyfaller`, `Explosion` dokumentieren; Test-Save anlegen | `docs/API_NOTES.md` mit echten Signaturen | 0,5 d |
| **M1** | Projekt-Setup: csproj (net472 + net10.0), `About.xml`, `LoadFolders.xml`, Harmony-Ref, leere DLL lädt fehlerfrei | „Hello RectShield" im Mod-Manager | 0,5 d |
| **M2** | MVP-Geometrie: `RectShieldShape` + `CellRect`, `ThingDef`, `CompRectShield`, `DrawFieldEdges`-Vorschau, erster `CheckIntercept`-Prefix | **Rechteckiger Schild stoppt Kugeln** (sichtbar & testbar) | 1,5 d |
| **M3** | Steuerung: Gizmos für Breite/Höhe/Offset/Rotation, `ExposeData`, Inspect-String, Mod Settings | Voll konfigurierbarer Schild | 1,5 d |
| **M4** | Energie & Balance: Schadensmodell, Strom, EMP, Overheat/Breakdown, Reboot, Forschung, Baukosten, Überlappungsregel | Spielbar-balanciert | 1,5 d |
| **M5** | Deckel & Robustheit: Skyfaller/Explosion-Patches, Randmodus, Fallback `Patch_Projectile`, CE-Pfad | Feature-complete v1.0 | 2 d |
| **M6** | QA & Release: T01–T16, 1.7-Build, DE/EN-Lokalisierung, Preview.png, README, LICENSE, Workshop-Upload | Veröffentlichbare v1.0 | 1,5 d |

**Summe:** ~9 Personentage für v1.0. **Erster spielbarer MVP (M0–M2): ~2,5 Tage.**
**Variante B (nur Patch einer fremden Mod) als Proof of Concept: ~0,5 Tag**, aber mit dem Risiko behaftet, beim nächsten Update der Basis-Mod zu brechen.

\* 1 d ≈ 6–8 h konzentrierte Arbeit. Puffer für unvorhergesehene API-Änderungen: +20 %.

---

## 10. Risiken & Mitigation

| Risiko | Wahrscheinlichkeit | Schaden | Mitigation |
|---|---|---|---|
| Falsche Methodensignatur → Patch greift nicht / roter Log | Hoch | Hoch | M0 als Pflicht-Phase; Patches mit `AccessTools` + Null-Check + `Log.Error`, nie still scheitern |
| `CompProjectileInterceptor` intern an `radius` gekoppelt (nicht nur `CheckIntercept`) | Mittel | Hoch | Fallback `Patch_Projectile`; notfalls Variante A (eigener Comp) |
| Harmony 3 / Waylith-Beta ändert sich vor Release | Mittel | Mittel | 1.7 als **optional** markieren; 1.6 bleibt Hauptziel; Harmony 3 Typen exakt deklarieren |
| Combat Extended umgeht Vanilla-Pfade | Hoch | Mittel | Reflection-basierter optionaler CE-Patch ohne harte Assembly-Abhängigkeit |
| Performance-Einbruch bei 41×41 + vielen Projektilen | Mittel | Hoch | Caches (Kap. 6), Belastungstest T15, Rare-Tick für Regen |
| Konflikt mit anderer Schild-Mod (doppelter Patch auf `CheckIntercept`) | Mittel | Mittel | Patches defensiv (`return true` für fremde Comps), Patch-Prioritäten, Kompatibilitäts-Patches |
| Urheberrecht bei Addon-Variante | Gering | Hoch | Nur zur Laufzeit patchen, keinen Code/Assets übernehmen, Lizenz in README nennen |
| Visuelles „Zacken"-Artefakt an Rechteckecken | Gering | Gering | `EdgeCells` + dediziertes Corner-Graphic; optional Opt-Out für Dauer-Overlay |

---

## 11. Release-Checkliste (Definition of Done)

- [ ] `About.xml` mit 1.6 + 1.7, Harmony-Dependency, `loadAfter`
- [ ] `LoadFolders.xml` und zwei kompilierte DLLs (net472 / net10.0)
- [ ] Keine gebündelte `0Harmony.dll`, keine fremden Assets
- [ ] T01–T16 bestanden, Log frei von roten Meldungen
- [ ] DE + EN Lokalisierung vollständig
- [ ] `README.md` mit Bedienung, Balance-Werten, bekannten Limitierungen, Danksagungen/Lizenzen
- [ ] `LICENSE` (z. B. MIT für eigenen Code), `CHANGELOG.md`
- [ ] `About/Preview.png`, Workshop-Beschreibung mit Screenshot Kreis vs. Rechteck
- [ ] GitHub-Repo mit CI-Build für beide Zielversionen
- [ ] `About/PublishedFileId.txt` nach Workshop-Upload

---

## 12. Offene Entscheidungen (Rückfragen)

1. **Welche Basis?** Eigenständige Mod (Variante C/A) oder schneller Patch für eine bestehende Schild-Mod (Variante B)? Falls B: welche Mod nutzt du aktuell – VFE-Security, Frontier Developments Shields, „Shield Generators (Continued)" oder eine andere?
2. **Version:** Nur 1.6 (stabil) oder auch die 1.7-Waylith-Beta mit testen?
3. **Schild-Verhalten:** Soll das **ganze Rechteck** geschützt sein (einfach) oder nur der **Rand als Wand** (taktisch)? Sollen Mörser/Drop Pods von oben geblockt werden?
4. **Größe:** Reicht ein Maximalbereich von ~41×41 Kacheln, oder soll das Feld größer/kleiner sein?
5. **Umfang dieses Auftrags:** Soll es beim **Plan** bleiben, oder soll ich im nächsten Schritt das komplette Mod-Grundgerüst (M0–M2, spielbarer MVP) im Workspace anlegen?

---

## Quellen

- Ludeon, *Announcing the Waylith Engine and Update 1.7* (12.10.2026) – https://ludeon.com/blog/2026/10/announcing-the-waylith-engine-and-update-1-7/
- RimWorld Wiki, *RimWorld 1.7 Mod Updates* – https://rimworldwiki.com/wiki/Modding_Tutorials/RimWorld_1.7_Mod_Updates
- RimWorld Wiki, *Setting up a solution* – https://rimworldwiki.com/wiki/Modding_Tutorials/Setting_up_a_solution
- RimWorld Wiki, *About.xml* – https://rimworldwiki.com/wiki/Modding_Tutorials/About.xml
- RimWorld Wiki, *Modding Tutorials* / *Version History* – https://rimworldwiki.com/wiki/Modding_Tutorials · https://rimworldwiki.com/wiki/Version_history
- pardeike, *HarmonyRimWorld* (Harmony 2.4.3 / 3.0) – https://github.com/pardeike/HarmonyRimWorld
- Steam Workshop, *Harmony* – https://steamcommunity.com/sharedfiles/filedetails/?id=2009463077
- RimWorld Wiki, *Gravship shield generator* – https://rimworldwiki.com/wiki/Gravship_shield_generator
- Steam Workshop, *Shield Expanded* (nutzt `CompProjectileInterceptor` als Referenz) – https://steamcommunity.com/sharedfiles/filedetails/?id=3575073524
- Frontier Developments, *Shields* (IShield-Erweiterungspunkt, CC BY-NC-SA 4.0) – https://github.com/rimworld-frontierdevelopment/Shields
- Steam Workshop, *VFE-Security* Diskussion zu `BuildingShield.cs` – https://steamcommunity.com/workshop/filedetails/discussion/1845154007/1631916406862211453/
- jaxxa, *ED-Shields* – https://github.com/jaxxa/ED-Shields
- rimworld-modding-reference, *Debugging / GenDraw* – https://github.com/Dev-Jahn/rimworld-modding-reference/blob/main/08-advanced-topics/debugging.md
- RW-Decompile, `Verse/GenSpawn.cs` (`CellRect`, `GenAdj.OccupiedRect`) – https://github.com/josh-m/RW-Decompile/blob/master/Verse/GenSpawn.cs
- zyphraxns, *rimworld-modding* (API-Referenz aus echten Assemblies) – https://github.com/zyphraxns/rimworld-modding
