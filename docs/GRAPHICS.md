# Ingame-Grafiken & Forschungsbaum

Alle Grafiken werden von `Tools/make_assets.py` erzeugt (nur Pillow nötig) und liegen als RGBA-PNGs mit transparentem Hintergrund in `Common/Textures/` bzw. `About/`.

## Übersicht

| Datei | Größe | Verwendung |
|---|---|---|
| `Things/Building/RectShield/RectShield_Mk1_S.png` | 192×192 | Welt-Sprite Mk I klein (9×7) |
| `Things/Building/RectShield/RectShield_Mk1_M.png` | 192×192 | Welt-Sprite Mk I mittel (15×11) |
| `Things/Building/RectShield/RectShield_Mk1_L.png` | 192×192 | Welt-Sprite Mk I groß (23×15) |
| `Things/Building/RectShield/RectShield_Mk2.png` | 192×192 | Welt-Sprite Mk II (verstellbar) |
| `UI/Icons/Buildings/RectShield_Mk1_S.png` | 64×64 | Baumenü + **Forschungsbaum** |
| `UI/Icons/Buildings/RectShield_Mk1_M.png` | 64×64 | Baumenü + **Forschungsbaum** |
| `UI/Icons/Buildings/RectShield_Mk1_L.png` | 64×64 | Baumenü + **Forschungsbaum** |
| `UI/Icons/Buildings/RectShield_Mk2.png` | 64×64 | Baumenü + **Forschungsbaum** |
| `UI/Icons/Research/RectShield_Mk1_Research.png` | 128×128 | optionales Groß-Icon für Icon-Overrides |
| `UI/Icons/Research/RectShield_Mk2_Research.png` | 128×128 | optionales Groß-Icon für Icon-Overrides |
| `About/ModIcon.png` | 64×64 | Ladebildschirm / Mod-Einstellungen |
| `About/Preview.png` | 512×512 | Steam-Workshop-Vorschau |

Die vier Gebäude-Sprites sind bewusst unterschiedlich gestaltet: Mk I klein hat **einen** Emitter, mittel **zwei**, groß **drei Spulen** plus Warnstreifen; Mk II hat einen türkisfarbenen Kern, vier goldene Verstell-Arme und ein kleines Display. So sind die Größenklassen im Spiel auf einen Blick unterscheidbar.

## Einbindung

- **Welt-Sprites** → `<texPath>` in `graphicData` des jeweiligen `ThingDef` (`Graphic_Single`, `drawSize (2,2)`).
- **Baumenü-Icons** → `<uiIconPath>` direkt im `ThingDef`. Seit RimWorld 1.5/1.6 werden diese Icons **auch im Forschungsbaum** angezeigt, weil das Research-Fenster die Icons der freigeschalteten Defs (`UnlockedDefs`) verwendet.
- **Mod-Icon** → `About/ModIcon.png` wird automatisch erkannt (alternativ `<modIconPath>` in `About.xml`).
- **Workshop-Bild** → `About/Preview.png`.

## Forschungsbaum

Die beiden Forschungen liegen in einem **eigenen Tab**:

```xml
<!-- Common/Defs/ResearchTabDefs/ResearchTabs_RectShield.xml -->
<ResearchTabDef>
  <defName>RectShield_ResearchTab</defName>
  <label>RectShield</label>
  <generalTitle>Rectangular shielding</generalTitle>
  <generalDescription>...</generalDescription>
  <visibleByDefault>true</visibleByDefault>
</ResearchTabDef>
```

Beide `ResearchProjectDef`s verweisen per `<tab>RectShield_ResearchTab</tab>` darauf. Positionen im Tab: Mk I bei `researchViewX=1 / researchViewY=1.4`, Mk II bei `2 / 1.4`. Die Y-Achse läuft in **0,7er-Schritten** (nicht 1er), und zwei Projekte auf derselben Koordinate werden versetzt dargestellt – deshalb der eigene Tab. Quelle: [RimWorld Wiki – Research Projects](https://rimworldwiki.com/wiki/Modding_Tutorials/Research_Projects).

**Warum kein `<iconPath>` in den Forschungs-Defs?** Im 1.6-Dekompilat (`Verse/ResearchProjectDef.cs`, vgl. [RimworldDecompile](https://github.com/Dyyrlysh/RimworldDecompile)) existiert dieses Feld **nicht** – die öffentlichen Felder sind u. a. `tab`, `researchViewX`, `researchViewY`, `generalRules`, aber kein `iconPath`. Ein solches XML-Feld würde beim Laden einen Fehler erzeugen. Ältere Modding-Anleitungen und Icon-Override-Mods (z. B. *Node Research*) nutzen eigene Mechanismen. Deshalb gehen wir über `uiIconPath`, was in 1.6 und 1.7 funktioniert.

Die beiden `UI/Icons/Research/*.png` sind größere Alternativ-Icons (Gebäude-Sprite in einem Schild-Rahmen, mit römischer Ziffer). Sie liegen bewusst zusätzlich bereit:

- Für Icon-Override-Mods, die Research-Icons patchen.
- Für Versionen/Mods, in denen `ResearchProjectDef.iconPath` doch verfügbar ist.
- Oder einfach, indem du `<uiIconPath>` eines Gebäudes auf `UI/Icons/Research/RectShield_Mk1_Research` umstellst.

## Anpassen

```bash
python3 Tools/make_assets.py     # erzeugt alle PNGs neu
python3 Tests/validate_xml.py    # prueft, dass alle texPath/uiIconPath existieren
```

Eigene Sprites einbauen: einfach die PNG-Dateien unter gleichem Namen ersetzen. Für 3D-/Multi-Frame-Grafiken (`Graphic_Multi`, `_north`/`_east`/`_south`) muss zusätzlich `graphicClass` in den Defs geändert werden.

Falls der eigene Tab mit einem anderen Mod kollidiert oder du alles im Haupt-Tab haben willst: die beiden `<tab>`-Zeilen in `Research_RectShield.xml` entfernen und `researchViewX/Y` auf freie Koordinaten im Haupt-Tab setzen. Den `ResearchTabDef` kannst du dann löschen (oder stehen lassen – er schadet nicht).
