# API-Notizen – vor dem ersten Build prüfen

RimWorld hat keine offizielle API-Dokumentation. **Jede** der folgenden Signaturen muss gegen die eigene `Assembly-CSharp.dll` (und für 1.7 zusätzlich `UnityStubs.dll` / `Waylith.dll`) mit **ILSpy** geprüft werden. Die Checkliste gilt für beide Zielversionen getrennt.

## 0. Werkzeug

```
ILSpy öffnen → Assembly-CSharp.dll laden → Suche nach Klasse/Methode
Pfad: RimWorld/RimWorldWin64_Data/Managed/Assembly-CSharp.dll
```

Bei Abweichungen: die betroffene Zeile im Quellcode anpassen und hier dokumentieren. Ein fehlender Patch erzeugt beim Spielstart eine **`[RectShield] Methode nicht gefunden`**-Warnung im Log – das ist absichtlich so, damit es nicht still scheitert.

## 1. Harmony-Ziele (kritisch)

| Klasse | Methode | Verwendet in | Erwartung |
|---|---|---|---|
| `Verse.Projectile` | `Tick()` | `Patches/Patch_Projectile_Tick.cs` (Postfix) | muss existieren; `Projectile` liegt in `Verse`, nicht `RimWorld` |
| `RimWorld.Skyfaller` | `Tick()` | `Patches/Patch_Skyfaller_Tick.cs` (Postfix) | Drop Pods / Meteoriten / Trümmer |
| `Verse.Projectile` | `CheckForFreeInterceptBetween(Vector3, Vector3)` | *nicht verwendet* | bewusst ausgelassen: `__result`-Typ ist versionsabhängig, Harmony 3 prüft ihn streng |

**Fallback, falls `Projectile.Tick()` nicht patchbar ist (z. B. Inlining):**
`AccessTools.Method(typeof(Thing), "Tick")` patchen und im Postfix per `if (__instance is Projectile p)` filtern. Das ist langsamer (alle Things), daher nur als Notlösung.

**Für Combat Extended:** eigene Klasse `CombatExtended.ProjectileCE` (Namespace/Pfad per ILSpy prüfen), Methode `Tick()` analog patchen. Nie hart referenzieren – otherwise lädt die Mod ohne CE nicht. Stattdessen:

```csharp
Type ceType = AccessTools.TypeByName("CombatExtended.ProjectileCE");
if (ceType != null) { /* AccessTools.Method(ceType, "Tick") patchen */ }
```

## 2. ThingComp-Hooks

| Signatur | Datei | Risiko |
|---|---|---|
| `PostSpawnSetup(bool respawningAfterLoad)` | `CompRectShield.cs` | stabil |
| `PostDeSpawn(Map map, DestroyMode mode = DestroyMode.Vanish)` | `CompRectShield.cs` | **GEPRÜFT 1.6.4871** – zwei Parameter; die Ein-Parameter-Version erzeugte `CS0115` |
| `PostExposeData()` | `CompRectShield.cs` | stabil |
| `CompTick()` | `CompRectShield.cs` | benötigt `tickerType = Normal` im ThingDef |
| `CompGetGizmosExtra()` → `IEnumerable<Gizmo>` | `CompRectShield.cs` | stabil |
| `PostDrawExtraSelectionOverlays()` | `CompRectShield.cs` | **GEPRÜFT 1.6.4871** – nicht `DrawExtraSelectionOverlays()` |
| `CompInspectStringExtra()` → `string` | `CompRectShield.cs` | stabil |
| `MapComponentTick()` / `MapComponentDraw()` | `RectShieldMapComponent.cs` | stabil |
| `PlaceWorker.AllowsPlacing(BuildableDef, IntVec3, Rot4, Map, Thing, Thing)` | `PlaceWorker_RectShield.cs` | **PRÜFEN** – Parameterzahl variiert zwischen 1.4/1.5/1.6 |

## 3. Verwendete Spieltypen / Member

| Typ | Member | Zweck |
|---|---|---|
| `Verse.IntVec3` | `x`, `z`, Konstruktor `(int,int,int)`, `+`, `==` | Geometrie |
| `Verse.Rot4` | `AsInt` (0=N,1=O,2=S,3=W) | 90°-Rotation |
| `Verse.Map` | `Size.x`, `Size.z`, `GetComponent<T>()` | Clipping, Registry |
| `Verse.Thing` | `Spawned`, `Destroyed`, `Map`, `Position`, `Rotation`, `DrawPos`, `Faction`, `GetComp<T>()`, `Destroy(DestroyMode)` | Comp-Basis |
| `Verse.GenDraw` | `DrawFieldEdges(List<IntVec3>, Color, float? = null)` | Rechteck-Overlay |
| `Verse.Scribe_Values` | `Look<T>(ref T, string, T defaultValue)` | Savegames |
| `Verse.Find` | `TickManager.TicksGame`, `WindowStack` | Ticks, FloatMenu |
| `Verse.Log` | `Error`, `Warning`, `Message` | Logging |
| `Verse.Command_Action` | `defaultLabel`, `defaultDesc`, `action`, `disabled` | Gizmos |
| `Verse.Listing_Standard` | `Begin`, `Label`, `CheckboxLabeled`, `Slider`, `Gap`, `End` | Settings |
| `Verse.Mod` / `ModSettings` | `GetSettings<T>()`, `SettingsCategory()`, `DoSettingsWindowContents(Rect)`, `ExposeData()` | Settings |
| `RimWorld.CompPowerTrader` | `PowerOutput` (setzbar), `PowerOn` | dynamischer Strombedarf |
| `RimWorld.CompFlickable` | `SwitchIsOn` | Ein/Aus |
| `RimWorld.CompBreakdownable` | `BrokenDown` | Defekt |
| `RimWorld.Faction` | `OfPlayer`, `HostileTo(Faction)` | Drop-Pod-Filter |
| `RimWorld.FleckMaker` | `ThrowMicroSparks(Vector3, Map)` | Treffer-Effekt (in try/catch) |
| `RimWorld.FloatMenu` / `FloatMenuOption` | Konstruktoren | Größen-Menü |
| `Verse.ProjectileProperties` | `flyOverhead`, `GetDamageAmount(Thing weapon, StringBuilder explanation = null)` | **GEPRÜFT**: erwartet ein `Thing`, keinen `float`. `GetDamageAmount(1f)` ergab `CS1503`. |
| `RimWorld.VersionControl` | `CurrentVersionString` | **GEPRÜFT**: liegt in `RimWorld`, nicht `Verse`. Ohne `using RimWorld;` gab es `CS0103`. |
| `Verse.Gizmo` | `Disabled` (Property), `disabledReason` | **GEPRÜFT**: das Feld `disabled` ist `protected`; `disabled = true` ergab `CS0122`, korrekt ist `Disabled = true`. |
| `Verse.GenDraw` | `DrawFieldEdges(List<IntVec3>, Color, float? = null, HashSet<IntVec3> = null, int = 2900)` | **GEPRÜFT**: 5 Parameter mit Defaults, Aufruf mit 2 Argumenten ist gültig. |
| `UnityEngine` | `Vector3`, `Color`, `Rect` | DrawPos, Overlay, Settings |

## 4. 1.7-spezifisch (Waylith)

- `net10.0`, SDK-style csproj, VS2026 / Rider 2026 (VS2022 kann kein .NET 10).
- **Kein `RimWorldWin64_Data/Managed` mehr!** `Assembly-CSharp.dll`, `UnityStubs.dll`, `Waylith.dll` (und ggf. `Waylith.UnityCompat.dll`) liegen im **Spiel-Installations-Root**. Deshalb nutzt `RectShield.1.7.csproj` die Property `GameDir` (nicht `ManagedPath`). `using UnityEngine;` bleibt dank `UnityStubs` gültig; Unity-DLL-Referenzen wurden entfernt.
- Referenzen: `Assembly-CSharp.dll`, `UnityStubs.dll`, `Waylith.dll`, optional `Waylith.UnityCompat.dll`. `0Harmony.dll` (Harmony 3) kommt aus dem Harmony-Workshop-Ordner `1.7/Assemblies/`.
- **Harmony 3** prüft `__instance`/`__result`-Typen exakt. Unsere Postfixes nutzen nur `__instance` mit dem korrekten Typ – das ist der Grund, warum wir auf `CheckForFreeInterceptBetween` verzichten.
- `LoadFolders.xml` lädt `Common/` + `1.7/`.

## 5. Def-Checks (XML)

Folgende Defs/Referenzen existieren in Core (ohne DLC): `BuildingBase`, `Electricity`, `Security`, `Steel`, `Plasteel`, `ComponentIndustrial`, `Gold`, `Graphic_Single`, `CompPowerTrader`, `CompFlickable`, `CompBreakdownable`. **Vor dem Veröffentlichen prüfen**, dass `Electricity` in der eigenen Version noch der defName des Strom-Forschungsprojekts ist.

### Forschungsbaum (verifiziert am 1.6-Dekompilat)

`Verse/ResearchProjectDef.cs` hat **kein** Feld `iconPath`. Öffentliche Felder (Auszug): `baseCost`, `prerequisites`, `hiddenPrerequisites`, `techLevel`, `requiredResearchBuilding`, `tags`, `tab`, `researchViewX`, `researchViewY`, `generalRules`, `knowledgeCategory`. Properties u. a.: `UnlockedDefs`, `InfoCardHyperlinks`.

Konsequenz: Ein `<iconPath>` im ResearchProjectDef erzeugt einen XML-Ladefehler. Das Forschungsfenster nimmt stattdessen die Icons der freigeschalteten Defs → **`BuildableDef.uiIconPath`** an unseren vier ThingDefs. Siehe `docs/GRAPHICS.md`.

`RimWorld/ResearchTabDef.cs` hat die Felder `generalTitle`, `generalDescription`, `visibleByDefault`, `minMonolithLevelVisible`, `tutorTag` (plus die Standard-Def-Felder wie `label`). Unser `RectShield_ResearchTab` nutzt `label`, `generalTitle`, `generalDescription`, `visibleByDefault`.

`researchViewX` inkrementiert in 1er-, `researchViewY` in **0,7er-Schritten**. Zwei Projekte auf derselben Koordinate werden versetzt dargestellt. Der eigene Tab verhindert Kollisionen mit Vanilla/Mods; die `<tab>`-Zeilen können ersatzlos entfernt werden, wenn alles im Haupt-Tab liegen soll (dann Koordinaten anpassen). Quelle: [RimWorld Wiki – Research Projects](https://rimworldwiki.com/wiki/Modding_Tutorials/Research_Projects).

`About/ModIcon.png` wird seit 1.5 automatisch als Mod-Icon erkannt; alternativ gibt es `<modIconPath>` in `About.xml`.

## 6. Was bewusst NICHT implementiert ist (v0.2)

- `RimWorld.Explosion` / `DamageWorker`: Explosionsradien werden noch nicht am Schildrand gekappt.
- `CompShield.CompAllowVerbCast`: KI schießt ggf. noch auf eigene Schilde.
- `GenDraw.DrawRadiusRing`: Vanilla-Radiusringe anderer Mods bleiben unverändert.
- `PlaceWorker.DrawGhost(...)`: Vorschau **beim Platzieren** (die Umrandung erscheint erst nach dem Bau).

---

## Ergänzung 26.10.2026 – ThingDef-defNames dürfen nicht auf Ziffern enden

Beim ersten echten Spielstart (RimWorld `1.6.4871 rev591`) meldete
`Verse.DefDatabase<ThingDef>.ErrorCheckAllDefs()`:

```
Config error in RectShield_Mk2: ends with a numerical digit, which is not allowed on ThingDefs.
Config error in RectShield_Mk3: ends with a numerical digit, which is not allowed on ThingDefs.
Config error in Blueprint_RectShield_Mk2 / Frame_RectShield_Mk2 / Blueprint_Install_RectShield_Mk2 ...
```

**Regel:** `ThingDef.ConfigErrors()` prüft `char.IsDigit(defName.Last())` und
lehnt Ziffern am Ende ab. RimWorld generiert aus jedem Gebäude-defName
automatisch `Blueprint_<name>`, `Frame_<name>` und `Blueprint_Install_<name>` –
deshalb erscheint der Fehler **mehrfach** pro betroffenem Def.

**Fix (v0.2.1):** `RectShield_Mk2` → `RectShield_MkII`,
`RectShield_Mk3` → `RectShield_MkIII` (römische Ziffern, enden auf Buchstaben).
`RectShield_Mk1_S/M/L` waren nie betroffen, weil sie auf `S`/`M`/`L` enden.

**Nicht betroffen:** `ResearchProjectDef`-defNames (z. B.
`RectShield_Mk2_Research`) – die Regel gilt nur für `ThingDef`. Auch die
Enum-Werte `<tier>Mk2</tier>` / `<tier>Mk3</tier>` in den Comp-Properties
bleiben unverändert; das sind C#-Enum-Namen, keine defNames.

`Tests/validate_xml.py` enthält seit v0.2.1 einen Regressionstest, der alle
`ThingDef`-defNames im `Common/Defs/`-Baum auf ein numerisches Endzeichen prüft.

---

## Ergänzung 26.10.2026 – Gravschiff-Substructure (v0.3.0)

Geprüft gegen das 1.6-Dekompilat ([Dyyrlysh/RimworldDecompile](https://github.com/Dyyrlysh/RimworldDecompile),
`Verse/TerrainGrid.cs`, `Verse/TerrainDef.cs`, `RimWorld/GravshipUtility.cs`):

| API | Signatur / Bedeutung |
|---|---|
| `Verse.TerrainGrid.FoundationAt(IntVec3 c)` | `TerrainDef` der **Foundation-Layer** (seit 1.6.4518). `TerrainAt()` liefert dagegen die oberste Schicht. |
| `Verse.TerrainGrid.FoundationAt(int ind)` | Index-Variante. |
| `Verse.TerrainDef.isFoundation` | `bool`-Feld, markiert Foundation-Terrains. |
| `Verse.TerrainDef.IsSubstructure` | Property: `ModsConfig.OdysseyActive && HasTag("Substructure")`. |
| `Verse.Def.HasTag(string)` | Stabile Basis-API; in `RectShieldSubstructure` bewusst **statt** `IsSubstructure` verwendet, um keine Odyssey-Abhängigkeit zu kompilieren. |
| `RimWorld.GravshipUtility.GetConnectedSubstructure(Building_GravEngine, HashSet<IntVec3>, int, bool)` | Vanilla-Floodfill ab dem Grav-Engine. **Nicht verwendet**, weil es einen Odyssey-Typ voraussetzt und nur *verbundene* Platten liefert. Unser Scan nimmt alle Platten auf der Map. |

**Zellmenge statt Bounding Box (v0.4.0):** `GenDraw.DrawFieldEdges` zeichnet
die Kanten zwischen uebergebenen Zellen und ihren Nachbarn ausserhalb der Liste.
Uebergibt man `Shape.AllCells()` statt einer Box, entsteht automatisch die
Aussenkontur einer beliebigen Form (L, U, T, ...). `HashSet<IntVec3>` funktioniert
direkt, weil `Verse.IntVec3` Value-Equality und `GetHashCode` implementiert.
`Shape.EdgeCells()` wird beim Aufbau einmalig gecacht, damit das persistente
Overlay nicht pro Frame O(n) Nachbarlookups macht.

**Geometrie-Falle (v0.3.1):** Ein Feld aus einem Integer-Mittelpunkt
`(MinX+MaxX)/2` plus `halfW = width/2` zu bauen, verschiebt sich bei **geraden**
Breiten um eine halbe Zelle Richtung Westen. Sichtbar wurde das als 1–2 Kacheln
zu grosser Rand im Westen/Norden. Korrekt ist immer: Grenzen direkt aus den
Quell-Grenzen ableiten (`MinX-pad`, `MaxX+pad`) und einen Max-Clamp symmetrisch
auf beide Seiten verteilen.

`Command_Action.icon` (geerbt von `Verse.Command`) ist ein öffentliches `UnityEngine.Texture2D`-Feld;
`ContentFinder<Texture2D>.Get(string path, bool reportFailure)` lädt Texturen aus `Textures/<path>.png`.
Beides wird in `RectShieldGizmos` mit einem try/catch-Fallback verwendet, damit eine fehlende
Textur nie den Spielstart blockiert.
