#!/usr/bin/env python3
"""Erzeugt alle Ingame-Grafiken des Mods (nur Pillow benoetigt).

    python3 Tools/make_assets.py

Erzeugt:
  - 4 Gebaeude-Sprites (Mk I klein/mittel/gross, Mk II)
  - 4 Bau-Menue-Icons (uiIconPath)
  - 2 Forschungsbaum-Icons (ResearchProjectDef.iconPath)
  - About/ModIcon.png (Ladebildschirm / Mod-Einstellungen)
  - About/Preview.png (Workshop-Vorschau)

Alle Grafiken sind RGBA mit transparentem Hintergrund und koennen durch
richtige RimWorld-Sprites ersetzt werden - einfach Dateinamen beibehalten.
"""
from __future__ import annotations

from pathlib import Path

from PIL import Image, ImageDraw, ImageFont

ROOT = Path(__file__).resolve().parents[1]
TEX = ROOT / "Common/Textures"
BUILDINGS = TEX / "Things/Building/RectShield"
MENU_ICONS = TEX / "UI/Icons/Buildings"
RESEARCH_ICONS = TEX / "UI/Icons/Research"
for folder in (BUILDINGS, MENU_ICONS, RESEARCH_ICONS, ROOT / "About"):
    folder.mkdir(parents=True, exist_ok=True)

BLUE = (70, 155, 255)
CYAN = (70, 240, 205)
STEEL = (54, 63, 74)
STEEL_LIGHT = (108, 122, 138)
DARK = (22, 27, 34)
GOLD = (225, 180, 75)


def font(size: int):
    for candidate in (
        "/usr/share/fonts/truetype/dejavu/DejaVuSans-Bold.ttf",
        "/usr/share/fonts/truetype/liberation/LiberationSans-Bold.ttf",
    ):
        if Path(candidate).exists():
            return ImageFont.truetype(candidate, size)
    return ImageFont.load_default()


def bolt(d: ImageDraw.ImageDraw, x: int, y: int, r: int = 5) -> None:
    d.ellipse((x - r, y - r, x + r, y + r), fill=STEEL_LIGHT,
              outline=DARK, width=2)
    d.line((x - r, y, x + r, y), fill=DARK, width=1)


def emitter(d: ImageDraw.ImageDraw, cx: int, cy: int, radius: int,
            accent: tuple[int, int, int], rings: int = 3) -> None:
    for i in range(rings, 0, -1):
        rr = int(radius * i / rings)
        d.ellipse((cx - rr, cy - rr, cx + rr, cy + rr),
                  outline=accent + (90 + 45 * (rings - i),), width=3)
    d.ellipse((cx - radius // 3, cy - radius // 3,
               cx + radius // 3, cy + radius // 3),
              fill=accent + (235,), outline=(235, 250, 255, 255), width=2)
    d.line((cx - radius, cy, cx + radius, cy), fill=accent + (130,), width=2)
    d.line((cx, cy - radius, cx, cy + radius), fill=accent + (130,), width=2)


def building_sprite(path: Path, accent: tuple[int, int, int], coils: int,
                    label: str, gold: bool = False, size: int = 192) -> None:
    """Top-Down-Sprite fuer ein 2x2-Gebaeude."""
    img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    s = size / 192.0

    def box(coords, **kw):
        d.rounded_rectangle(tuple(int(v * s) for v in coords), **kw)

    # Schatten
    d.ellipse((int(14 * s), int(150 * s), int(178 * s), int(186 * s)),
              fill=(0, 0, 0, 95))
    # Sockel (Achteck angedeutet)
    box((12, 12, 180, 180), radius=int(18 * s), fill=DARK + (255,),
        outline=(10, 13, 17, 255), width=max(2, int(3 * s)))
    box((18, 18, 174, 174), radius=int(14 * s), fill=STEEL + (255,),
        outline=STEEL_LIGHT + (255,), width=max(2, int(2 * s)))
    # Innenplatte
    box((28, 28, 164, 164), radius=int(9 * s), fill=(26, 33, 42, 255),
        outline=accent + (220,), width=max(2, int(2 * s)))

    # Kuehlrippen / Paneele
    for y in (36, 154):
        d.line((int(40 * s), int(y * s), int(152 * s), int(y * s)),
               fill=accent + (110,), width=max(1, int(2 * s)))
    for x in (36, 156):
        d.line((int(x * s), int(40 * s), int(x * s), int(150 * s)),
               fill=(90, 104, 120, 150), width=max(1, int(2 * s)))

    # Ecken-Bolzen
    for x, y in ((26, 26), (166, 26), (26, 166), (166, 166)):
        bolt(d, int(x * s), int(y * s), max(3, int(6 * s)))

    # Zusaetzliche Spulen bei groesseren Varianten
    cx, cy = size // 2, size // 2
    if coils >= 2:
        for ox, oy in ((-52, -52), (52, 52)):
            emitter(d, cx + int(ox * s), cy + int(oy * s),
                    max(6, int(15 * s)), accent, rings=2)
    if coils >= 3:
        for ox, oy in ((52, -52), (-52, 52)):
            emitter(d, cx + int(ox * s), cy + int(oy * s),
                    max(6, int(15 * s)), accent, rings=2)

    # Haupt-Emitter
    emitter(d, cx, cy, max(16, int(46 * s)), accent, rings=3)

    # Warnstreifen bei Mk I gross
    if coils >= 3:
        for i in range(6):
            x = int((34 + i * 12) * s)
            d.polygon([(x, int(170 * s)), (x + int(7 * s), int(170 * s)),
                       (x + int(3 * s), int(180 * s)), (x - int(4 * s), int(180 * s))],
                      fill=GOLD + (200,))

    # Mk II: verstellbare Arme + Goldakzente
    if gold:
        for ox, oy in ((-70, 0), (70, 0), (0, -70), (0, 70)):
            d.line((cx, cy, cx + int(ox * s), cy + int(oy * s)),
                   fill=GOLD + (200,), width=max(2, int(4 * s)))
            d.ellipse((cx + int(ox * s) - int(6 * s), cy + int(oy * s) - int(6 * s),
                       cx + int(ox * s) + int(6 * s), cy + int(oy * s) + int(6 * s)),
                      fill=GOLD + (235,), outline=DARK + (255,), width=2)
        box((76, 168, 116, 182), radius=int(4 * s), fill=DARK + (255,),
            outline=GOLD + (255,), width=2)
        for i in range(4):
            d.line((int((82 + i * 8) * s), int(174 * s),
                    int((82 + i * 8) * s), int(178 * s)),
                   fill=accent + (220,), width=2)

    f = font(max(9, int(15 * s)))
    d.text((cx, int(181 * s)), label, font=f,
           fill=(228, 238, 248, 255), anchor="mm")
    img.save(path)
    print("  gebaeude:", path.relative_to(ROOT))


def menu_icon(path: Path, accent: tuple[int, int, int], gold: bool = False,
              size: int = 64) -> None:
    img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    d.rounded_rectangle((3, 3, 61, 61), radius=9, fill=DARK + (255,),
                        outline=STEEL_LIGHT + (255,), width=2)
    d.rounded_rectangle((8, 8, 56, 56), radius=6, fill=(26, 33, 42, 255),
                        outline=accent + (220,), width=2)
    emitter(d, 32, 32, 18, accent, rings=3)
    if gold:
        for x, y in ((10, 10), (54, 10), (10, 54), (54, 54)):
            d.ellipse((x - 3, y - 3, x + 3, y + 3), fill=GOLD + (255,))
    img.save(path)
    print("  menu-icon:", path.relative_to(ROOT))


def research_icon(path: Path, source: Path, accent: tuple[int, int, int],
                  roman: str, size: int = 128) -> None:
    """Forschungsbaum-Icon: Gebaeude-Sprite in einem Schild-Rahmen."""
    img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)

    # Aeusserer Rahmen (Forschungs-Kachel)
    d.rounded_rectangle((4, 4, 124, 124), radius=16, fill=(16, 22, 30, 235),
                        outline=accent + (255,), width=3)
    d.rounded_rectangle((10, 10, 118, 118), radius=12,
                        outline=accent + (110,), width=2)

    # Stilisiertes Rechteck-Feld im Hintergrund
    d.rectangle((18, 24, 110, 100), outline=accent + (120,), width=2)
    for x in range(24, 110, 12):
        d.line((x, 24, x, 100), fill=accent + (35,), width=1)

    # Verkleinertes Gebaeude-Sprite in der Mitte
    sprite = Image.open(source).convert("RGBA")
    sprite.thumbnail((78, 78), Image.Resampling.LANCZOS)
    img.alpha_composite(sprite, ((size - sprite.width) // 2, (size - sprite.height) // 2 + 2))

    # Roemische Ziffer unten
    f = font(18)
    d.text((64, 112), roman, font=f, fill=(235, 245, 255, 255), anchor="mm")
    img.save(path)
    print("  research-icon:", path.relative_to(ROOT))


def mod_icon(path: Path, size: int = 64) -> None:
    img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    d.rounded_rectangle((2, 2, 62, 62), radius=10, fill=(18, 24, 32, 255),
                        outline=CYAN + (255,), width=3)
    d.rectangle((10, 16, 54, 46), outline=CYAN + (255,), width=3)
    d.rectangle((16, 22, 48, 40), outline=(40, 110, 130, 255), width=2)
    d.ellipse((27, 27, 37, 37), fill=CYAN + (240,))
    img.save(path)
    print("  mod-icon:", path.relative_to(ROOT))


def preview(path: Path, sources: list[Path]) -> None:
    size = 512
    img = Image.new("RGB", (size, size), (18, 22, 28))
    d = ImageDraw.Draw(img)
    for i in range(0, size, 32):
        d.line((i, 0, i, size), fill=(28, 34, 42), width=1)
        d.line((0, i, size, i), fill=(28, 34, 42), width=1)

    f_big, f_small, f_label = font(40), font(18), font(14)

    d.text((256, 56), "RectShield", font=f_big, fill=(235, 245, 255), anchor="mm")
    d.text((256, 96), "Rectangular shields for RimWorld", font=f_small,
           fill=(140, 165, 185), anchor="mm")

    # Alte Kuppel (durchgestrichen)
    d.ellipse((28, 150, 148, 270), outline=(105, 116, 130), width=4)
    d.ellipse((52, 174, 124, 246), outline=(55, 63, 73), width=2)
    d.line((36, 258, 140, 162), fill=(215, 70, 70), width=6)
    d.line((36, 162, 140, 258), fill=(215, 70, 70), width=6)
    d.text((88, 292), "circle / dome", font=f_label,
           fill=(150, 160, 172), anchor="mm")

    # Neues Rechteck
    d.rectangle((188, 158, 330, 262), outline=(70, 220, 255), width=4)
    d.rectangle((198, 168, 320, 252), outline=(40, 90, 110), width=2)
    d.text((259, 292), "rectangle", font=f_label,
           fill=(70, 220, 255), anchor="mm")

    # Die vier Gebaeude-Sprites
    labels = ["Mk I S", "Mk I M", "Mk I L", "Mk II"]
    for index, source in enumerate(sources):
        sprite = Image.open(source).convert("RGBA")
        sprite.thumbnail((76, 76), Image.Resampling.LANCZOS)
        x = 40 + index * 118
        img.paste(sprite, (x, 330), sprite)
        d.text((x + 38, 412), labels[index], font=f_label,
               fill=(200, 215, 230), anchor="mm")

    d.text((256, 452), "Mk I  fixed sizes  |  Mk II  adjustable", font=f_small,
           fill=(190, 205, 220), anchor="mm")
    d.text((256, 482), "RimWorld 1.6  +  1.7 Waylith", font=f_label,
           fill=(120, 140, 158), anchor="mm")
    img.save(path)
    print("  preview:", path.relative_to(ROOT))


def main() -> None:
    specs = [
        ("RectShield_Mk1_S", BLUE, 1, "MK I-S", False),
        ("RectShield_Mk1_M", BLUE, 2, "MK I-M", False),
        ("RectShield_Mk1_L", BLUE, 3, "MK I-L", False),
        ("RectShield_Mk2", CYAN, 4, "MK II", True),
    ]

    building_paths: list[Path] = []
    for name, accent, coils, label, gold in specs:
        building = BUILDINGS / f"{name}.png"
        building_sprite(building, accent, coils, label, gold=gold)
        building_paths.append(building)
        menu_icon(MENU_ICONS / f"{name}.png", accent, gold=gold)

    research_icon(RESEARCH_ICONS / "RectShield_Mk1_Research.png",
                  building_paths[1], BLUE, "I")
    research_icon(RESEARCH_ICONS / "RectShield_Mk2_Research.png",
                  building_paths[3], CYAN, "II")

    mod_icon(ROOT / "About/ModIcon.png")
    preview(ROOT / "About/Preview.png", building_paths)

    legacy = BUILDINGS / "RectShield_Mk1.png"
    if legacy.exists():
        legacy.unlink()
        print("  entfernt (alter Platzhalter):", legacy.relative_to(ROOT))

    print("Fertig: alle Ingame-Grafiken erzeugt.")


if __name__ == "__main__":
    main()
