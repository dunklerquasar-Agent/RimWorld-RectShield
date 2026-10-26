# Balance-Werte (v0.3.1)

Alle Werte stehen in `Common/Defs/ThingDefs_Buildings/Buildings_RectShield.xml` und können dort ohne Neukompilieren geändert werden.

## Übersicht der drei Stufen

| | **Stufe 1 – Mk I** | **Stufe 2 – Mk II** | **Stufe 3 – Gravship Shield** |
|---|---|---|---|
| Grafik | Industriell | Militärisch | High-Tech (Gravship Shield) |
| Feld | 3 feste **quadratische** Größen | 5…41 × 5…41 | **5…51 × 5…51** |
| Standard | – | **15×15** | **21×21** |
| Blockt | nur direkte Schüsse | + indirektes Feuer, Drop Pods | wie Mk II, Auto-Fit um Rumpfplatten |
| Energie | 600 / 900 / 1200 | 1400 | **2400** |
| Regen/Tick | 0,5 / 0,6 / 0,7 | 0,7 | **1,2** |
| Grundlast | 150 / 250 / 400 W | 300 W | **500 W** |
| Flächenlast | 0 W/Kachel | 3 W/Kachel | **2 W/Kachel** |
| Drop-Pod-Kosten | – | 180 | **120** |
| Reboot | 900 Ticks | 1200 Ticks | **900 Ticks** |
| Forschung | 900 | 1600 | **2600** |

## Mk I – feste Größen

| Def | Feld | Fläche | Energie | Regen/Tick | Strom | Kosten |
|---|---|---|---|---|---|---|
| `RectShield_Mk1_S` | **11×11** | 121 | 600 | 0,5 | 150 W | 140 Stahl, 30 Plasteel, 6 Komponenten |
| `RectShield_Mk1_M` | **17×17** | 289 | 900 | 0,6 | 250 W | 220 Stahl, 55 Plasteel, 9 Komponenten |
| `RectShield_Mk1_L` | **23×23** | 529 | 1200 | 0,7 | 400 W | 320 Stahl, 85 Plasteel, 12 Komponenten |

Alle drei Felder sind jetzt **quadratisch (symmetrisch)** und in gleichmäßigen 6-Kachel-Schritten abgestuft (11 / 17 / 23). Gegenüber v0.2.x sind die Kanten rund 20–30 % länger; Strom und Energiepuffer blieben absichtlich unverändert.

Gemeinsam: `blocksShots = true`, `blocksIndirectFire = false`, `blocksSkyfallers = false`, Reboot nach 900 Ticks. Mörsergranaten, Drop Pods und Meteoriten **kommen durch** – das ist der gewollte Nachteil gegenüber Stufe 2 und 3. Alle drei Varianten teilen sich das **Industriell**-Sprite.

## Mk II – anpassbar (Stufe 2)

| Parameter | Wert |
|---|---|
| Feld | 5…41 × 5…41 Kacheln, Standard 13×9 |
| Energie | 1400 (konstant) |
| Regen | 0,7 / Tick |
| Grundlast | 300 W |
| Flächenlast | **3 W pro Kachel** |
| Drop-Pod-Kosten | 180 Energie pro Pod |
| Reboot | 1200 Ticks, danach 25 % Energie |
| Kosten | 380 Stahl, 140 Plasteel, 16 Komponenten, 20 Gold |

## Gravship Shield – Stufe 3

| Parameter | Wert |
|---|---|
| Feld | **5…51 × 5…51** Kacheln, Standard 21×21 |
| Energie | **2400** (konstant) |
| Regen | **1,2 / Tick** |
| Grundlast | **500 W** |
| Flächenlast | **2 W pro Kachel** |
| Drop-Pod-Kosten | **120** Energie pro Pod |
| Reboot | **900 Ticks** |
| `MaxHitPoints` / `WorkToBuild` | 800 / 7600 |
| Kosten | **450 Stahl, 220 Plasteel, 24 Komponenten, 60 Gold** |
| Baufähigkeit | **12** |

**Gravschiff-Modus (Auto-Fit):** Mit `<autoFitSubstructure>true</autoFitSubstructure>`
scannt der Comp alle 180 Ticks `map.terrainGrid.FoundationAt(cell)` und prüft
`TerrainDef.HasTag("Substructure")`. Aus der Bounding Box aller gefundenen Platten
wird das Feld gebildet – **direkt aus den Plattengrenzen** (`MinX-pad` … `MaxX+pad`),
also zwingend symmetrisch. Standard ist `substructurePadding = 0`: das Feld liegt
**buendig** auf den aeussersten Platten. Der Gizmo „Rand: N Kacheln" stellt 0–4
Kacheln Abstand ein (im Savegame gespeichert). Bleibt das Odyssey-DLC aus, werden
keine Platten gefunden und das manuelle Feld greift. Per Gizmo
„Rumpf-Anpassung: an/aus" abschaltbar.

**Designabsicht:** Der Gravship Shield ist kein einfaches „Mk II + 10". Der höhere Grundbedarf (500 W) macht ihn auf kleinen Feldern teurer, aber ab ~200 geschützten Kacheln ist er **günstiger** als Mk II. Dazu kommen fast doppelte Energiereserve, ~70 % mehr Regeneration und ein kürzerer Reboot. Der Preis sind hohe Baukosten, Baufähigkeit 12 und 2600 Forschungspunkte.

### Stromformeln

```
Mk II :    Strom (W) = 300 + 3 × (Breite × Höhe) × WattsPerCellMultiplier
Gravship: Strom (W) = 500 + 2 × (Breite × Höhe) × WattsPerCellMultiplier
```

| Feld | Fläche | Mk II | Gravship |
|---|---|---|---|
| 5×5 (Minimum) | 25 | 375 W | 550 W |
| 15×15 (Mk-II-Standard) | 225 | 975 W | 950 W |
| 21×21 (Gravship-Standard) | 441 | 1623 W | **1382 W** |
| 23×23 | 529 | 1887 W | **1558 W** |
| 41×41 (Vorlage) | 1681 | 5343 W | **3862 W** |
| 51×51 (Gravship-Vorlage) | 2601 | nicht möglich | **5702 W** |

Der `WattsPerCellMultiplier` in den Mod-Einstellungen (0,25–3,0) wirkt auf beide Stufen.

## Schadensmodell

```
Energieverlust = Projektilschaden × energyPerDamage × EnergyPerDamageMultiplier
```

`energyPerDamage = 1` bedeutet: Ein 40-Schaden-Treffer kostet 40 Energie. Bei `Energie ≤ 0` → Feld offline, Reboot-Timer startet. Getroffene Projektile werden sofort entfernt; die Energie wird pro Projektil nur **einmal** abgezogen, auch bei überlappenden Schilden (`RectShieldMapComponent.TryIntercept` bricht nach dem ersten Treffer ab).

## Forschungsbaum

| Projekt | Kosten | Voraussetzung | Position |
|---|---|---|---|
| `RectShield_Mk1_Research` | 900 | `Electricity` | (1 / 1.4) |
| `RectShield_Mk2_Research` | 1600 | `RectShield_Mk1_Research` | (2 / 1.4) |
| `RectShield_Mk3_Research` | 2600 | `RectShield_Mk2_Research` | (3 / 1.4) – „Gravship shielding" |

Alle drei im eigenen Tab `RectShield_ResearchTab`.

## Bekannte Stellschrauben für v0.3

- EMP: derzeit nur über den normalen Schadenspfad. Ein eigener EMP-Multiplikator ist in `CompProperties_RectShield` vorbereitet (Feld ergänzen).
- `blocksExplosions` ist im Comp bereits vorhanden, aber noch nicht mit einem Explosion-Patch verdrahtet.
- Skyfaller-Kosten (`skyfallerEnergyCost`) gelten pauschal pro Pod, unabhängig vom Inhalt.
- `RectShieldSettings.MaxSizeCap` ist derzeit nur ein gespeicherter Wert ohne Wirkung; die Obergrenze kommt aus `maxWidth`/`maxHeight` der Defs.
