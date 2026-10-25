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
| `PostDeSpawn(Map map)` | `CompRectShield.cs` | **PRÜFEN** – in manchen Versionen `PostDeSpawn()` ohne Parameter |
| `PostExposeData()` | `CompRectShield.cs` | stabil |
| `CompTick()` | `CompRectShield.cs` | benötigt `tickerType = Normal` im ThingDef |
| `CompGetGizmosExtra()` → `IEnumerable<Gizmo>` | `CompRectShield.cs` | stabil |
| `DrawExtraSelectionOverlays()` | `CompRectShield.cs` | stabil |
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
| `Verse.ProjectileProperties` | `flyOverhead`, `GetDamageAmount(float)` | Mk1/Mk2-Unterscheidung, Energiekosten |
| `Verse.VersionControl` | `CurrentVersionString` | nur Debug-Ausgabe, in `VersionInfo` abgesichert |
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
