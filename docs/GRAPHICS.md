# Ingame-Grafiken & Forschungsbaum

Seit v0.2.0 hat jede **Ausbaustufe** ein eigenes, KI-generiertes Top-Down-Sprite. Die älteren Pillow-Platzhalter bleiben als Legacy-Dateien im Repo, werden aber von keinem Def mehr referenziert.

## Die drei Stufen-Designs

| Stufe | Def(s) | Sprite (192×192 RGBA) | Icon (64×64) | Eindruck |
|---|---|---|---|---|
| **1 – Industriell** | `RectShield_Mk1_S/M/L` | `RectShield_Gen_Industrial.png` | `UI/Icons/Buildings/RectShield_Gen_Industrial.png` | Genieteter Stahl, vier Emitterspulen, orangener Warnstreifen, glühender Bernstein-Kern |
| **2 – Militärisch** | `RectShield_MkII` | `RectShield_Gen_Military.png` | `UI/Icons/Buildings/RectShield_Gen_Military.png` | Kantige Panzerung mit Eckbastionen, rote Energieleitungen, rubinroter Diamant-Kern |
| **3 – Gravship Shield** | `RectShield_MkIII` | `RectShield_Gen_HighTech.png` | `UI/Icons/Buildings/RectShield_Gen_HighTech.png` | Weiße Keramikpanzerung, cyan Leiterbahnen, türkiser Hexagonal-Kern, Satelliten-Emitter |

Alle drei Mk-I-Größen teilen sich bewusst dasselbe Industriell-Sprite: die Stufe ist der visuelle Anker, die Feldgröße unterscheidet sich über das Overlay und den Inspect-String. Seit v0.3.0 sind die drei festen Felder **quadratisch** (11×11 / 17×17 / 23×23), damit sie symmetrisch um den Generator liegen. Die Größen-Gizmos zeigen jetzt echte +/−-Grafiken, und Stufe 3 heißt „Gravship Shield". Seit v0.3.1 liegt dessen Feld **symmetrisch bündig** an den Rumpfplatten; der Rand ist per Gizmo auf 0–4 Kacheln einstellbar. Seit v0.4.0 zeichnet `DrawFieldEdges` die **echte Außenkontur beliebiger Formen** (L, U, T, Ringe, getrennte Plateaus) statt einer Box.

Seit v0.5.0 kommen zwei Effekte dazu:

| Effekt | Datei | Beschreibung |
|---|---|---|
| Diagonale Ecken | `UI/RectShieldDraw.cs` | Wo sich zwei Schildteile nur ueber eine Ecke beruehren, wird eine 45°-Linie gezogen (Wand-Link-Prinzip). Paare werden in `RectShieldShape.DiagonalLinks` gecacht. |
| Treffer-Flash | `UI/RectShieldDraw.cs` | Bei jedem geblockten Projektil/Skyfaller wird das **ganze Feld** 18 Ticks lang leicht durchscheinend ueberzogen (`DrawMeshInstanced`, Batches zu 512). Farbe = Stufenfarbe, Deckkraft faellt weich ab. |
| Mod-Einstellung | `Mod/RectShieldMod.cs` | „Bei Treffer das ganze Schildfeld kurz aufleuchten lassen" (Standard an). |

Beide Effekte funktionieren **auch ohne Auswahl** des Gebäudes und sind in
try/catch gekapselt. Parameter: `flashTicks`, `flashAlpha`, `diagonalLinks`. Wer will, kann S/M/L durch drei Varianten desselben Designs ersetzen.

**Quelle der Grafiken:** Im Workspace unter `/home/user/generator_ansichten/` (Rohbilder 1024×1024, `prepare_sprites.py` für Freistellung/Zuschnitt, `hd/` in 512×512, `vorschau.html` als Übersicht). Die Dateien sind freigestellt (transparenter Hintergrund) und als `Graphic_Single` mit `drawSize (2,2)` eingebunden.

## Weitere Grafiken

| Datei | Größe | Verwendung |
|---|---|---|
| `UI/Icons/Research/RectShield_Mk1_Research.png` | 128×128 | Forschungs-Icon Stufe 1 (Rahmen + Industriell-Sprite + „I") |
| `UI/Icons/Research/RectShield_Mk2_Research.png` | 128×128 | Forschungs-Icon Stufe 2 („II") |
| `UI/Icons/Research/RectShield_Mk3_Research.png` | 128×128 | Forschungs-Icon Stufe 3 („III") |
| `UI/Gizmos/RectShield_Plus.png` | 64×64 | **+**-Symbol für die Größen-Gizmos |
| `UI/Gizmos/RectShield_Minus.png` | 64×64 | **−**-Symbol für die Größen-Gizmos |
| `UI/Gizmos/RectShield_FitHull.png` | 64×64 | Rumpf-Anpassung (Gravship Shield) |
| `About/ModIcon.png` | 64×64 | Ladebildschirm / Mod-Einstellungen |
| `About/Preview.png` | 512×512 | Workshop-Vorschau: Kreis vs. Rechteck + alle drei Stufen |
| `RectShield_Mk1_S/M/L.png`, `RectShield_Mk2.png` | 192×192 | **Legacy-Platzhalter**, nicht mehr in den Defs referenziert |

> Für den Steam-Workshop wird zusätzlich eine **1280×720**-Vorschau benötigt. Sie liegt vorbereitet unter `/home/user/workshop-release/Preview_1280x720.png` und muss vor dem Upload nach `About/Preview.png` kopiert (oder als Workshop-Preview gesetzt) werden.

## Einbindung

- **Welt-Sprites** → `<texPath>` in `graphicData` des jeweiligen `ThingDef` (`Graphic_Single`, `drawSize (2,2)`), Pfad **ohne** `.png`.
- **Baumenü-Icons** → `<uiIconPath>` direkt im `ThingDef`. Seit RimWorld 1.5/1.6 werden diese Icons **auch im Forschungsbaum** angezeigt, weil das Research-Fenster die Icons der freigeschalteten Defs (`UnlockedDefs`) verwendet.
- **Mod-Icon** → `About/ModIcon.png` wird automatisch erkannt.
- **Workshop-Bild** → `About/Preview.png`.

## Forschungsbaum

Die **drei** Forschungen liegen in einem eigenen Tab:

```xml
<!-- Common/Defs/ResearchTabDefs/ResearchTabs_RectShield.xml -->
<ResearchTabDef>
  <defName>RectShield_ResearchTab</defName>
  <label>RectShield</label>
  <generalTitle>Rectangular shielding</generalTitle>
  <visibleByDefault>true</visibleByDefault>
</ResearchTabDef>
```

| Projekt | `researchViewX` | `researchViewY` | Voraussetzung |
|---|---|---|---|
| Mk I | 1 | 1.4 | `Electricity` |
| Mk II | 2 | 1.4 | Mk I |
| Gravship Shield | 3 | 1.4 | Mk II |

Die Y-Achse läuft in **0,7er-Schritten** (nicht 1er); deshalb der eigene Tab, um Kollisionen mit Vanilla-Koordinaten zu vermeiden. Quelle: [RimWorld Wiki – Research Projects](https://rimworldwiki.com/wiki/Modding_Tutorials/Research_Projects).

**Warum kein `<iconPath>` in den Forschungs-Defs?** Im 1.6-Dekompilat (`Verse/ResearchProjectDef.cs`, vgl. [RimworldDecompile](https://github.com/Dyyrlysh/RimworldDecompile)) existiert dieses Feld **nicht** – die öffentlichen Felder sind u. a. `tab`, `researchViewX`, `researchViewY`, `generalRules`. Ein solches XML-Feld würde beim Laden einen Fehler erzeugen. Deshalb gehen wir über `uiIconPath`, was in 1.6 und 1.7 funktioniert. Die drei `UI/Icons/Research/*.png` sind größere Alternativ-Icons für Icon-Override-Mods oder können per `<uiIconPath>` direkt eingebunden werden.

## Anpassen

```bash
python3 Tools/make_assets.py     # Legacy-Platzhalter, Research-/Gizmo-Icons, ModIcon, Preview
python3 Tests/validate_xml.py    # prueft alle texPath/uiIconPath gegen vorhandene PNGs
```

`make_assets.py` **überschreibt die drei Stufen-Sprites nicht** – es bricht mit einer Fehlermeldung ab, wenn sie fehlen. Eigene Sprites einbauen: einfach die PNG-Dateien unter gleichem Namen ersetzen. Für 3D-/Multi-Frame-Grafiken (`Graphic_Multi`, `_north`/`_east`/`_south`) muss zusätzlich `graphicClass` in den Defs geändert werden.

Falls der eigene Tab mit einem anderen Mod kollidiert: die drei `<tab>`-Zeilen in `Research_RectShield.xml` entfernen und `researchViewX/Y` auf freie Koordinaten im Haupt-Tab setzen.
