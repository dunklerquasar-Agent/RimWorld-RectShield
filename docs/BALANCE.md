# Balance-Werte (v0.1)

Alle Werte stehen in `Common/Defs/ThingDefs_Buildings/Buildings_RectShield.xml` und können dort ohne Neukompilieren geändert werden.

## Mk I – feste Größen

| Def | Feld | Fläche | Energie | Regen/Tick | Strom | Kosten |
|---|---|---|---|---|---|---|
| `RectShield_Mk1_S` | 9×7 | 63 | 600 | 0,5 | 150 W | 140 Stahl, 30 Plasteel, 6 Komponenten |
| `RectShield_Mk1_M` | 15×11 | 165 | 900 | 0,6 | 250 W | 220 Stahl, 55 Plasteel, 9 Komponenten |
| `RectShield_Mk1_L` | 23×15 | 345 | 1200 | 0,7 | 400 W | 320 Stahl, 85 Plasteel, 12 Komponenten |

Gemeinsam: `blocksShots = true`, `blocksIndirectFire = false`, `blocksSkyfallers = false`, Reboot nach 900 Ticks. Mörsergranaten, Drop Pods und Meteoriten **kommen durch** – das ist der gewollte Nachteil gegenüber Mk II.

## Mk II – anpassbar

| Parameter | Wert |
|---|---|
| Feld | 5…41 × 5…41 Kacheln, Standard 13×9 |
| Energie | 1400 (konstant!) |
| Regen | 0,7 / Tick |
| Grundlast | 300 W |
| Flächenlast | **3 W pro Kachel** |
| Drop-Pod-Kosten | 180 Energie pro Pod |
| Reboot | 1200 Ticks, danach 25 % Energie |
| Kosten | 380 Stahl, 140 Plasteel, 16 Komponenten, 20 Gold |

### Stromformel

```
Strom (W) = 300 + 3 × (Breite × Höhe) × WattsPerCellMultiplier
```

| Feld | Fläche | Strom |
|---|---|---|
| 5×5 (Minimum) | 25 | 375 W |
| 13×9 (Standard) | 117 | 651 W |
| 23×15 | 345 | 1335 W |
| 31×21 | 651 | 2253 W |
| 41×31 (Vorlage) | 1271 | 4113 W |

**Designabsicht:** Der Energie-*Puffer* bleibt bei 1400, egal wie groß das Feld ist. Wer das Feld vergrößert, zahlt mehr Strom, hat aber nicht mehr Reserve – große Felder sind dadurch anfälliger für konzentrierte Angriffe. Der `WattsPerCellMultiplier` in den Mod-Einstellungen (0,25–3,0) erlaubt es, das an den eigenen Modpack anzupassen.

## Schadensmodell

```
Energieverlust = Projektilschaden × energyPerDamage × EnergyPerDamageMultiplier
```

`energyPerDamage = 1` bedeutet: Ein 40-Schaden-Treffer kostet 40 Energie. Bei `Energie ≤ 0` → Feld offline, Reboot-Timer startet. Getroffene Projektile werden sofort entfernt; die Energie wird pro Projektil nur **einmal** abgezogen, auch bei überlappenden Schilden (`RectShieldMapComponent.TryIntercept` bricht nach dem ersten Treffer ab).

## Forschungsbaum

| Projekt | Kosten | Voraussetzung |
|---|---|---|
| `RectShield_Mk1_Research` | 900 | `Electricity` |
| `RectShield_Mk2_Research` | 1600 | `RectShield_Mk1_Research` |

## Bekannte Stellschrauben für v0.2

- EMP: derzeit nur über den normalen Schadenspfad. Ein eigener EMP-Multiplikator ist in `CompProperties_RectShield` vorbereitet (Feld ergänzen).
- `blocksExplosions` ist im Comp bereits vorhanden, aber noch nicht mit einem Explosion-Patch verdrahtet.
- Skyfaller-Kosten (`skyfallerEnergyCost`) gelten pauschal pro Pod, unabhängig vom Inhalt.
