#!/usr/bin/env python3
"""Erzeugt die Legacy-Platzhalter, Forschungs-Icons und das Vorschaubild.

    python3 Tools/make_assets.py

WICHTIG: Die drei eigentlichen Stufen-Sprites werden hier NICHT erzeugt.
Sie sind KI-generierte, freigestellte Grafiken und liegen bereits im Repo:

  RectShield_Gen_Industrial.png  -> Stufe 1 / Mk I  (alle drei Groessen)
  RectShield_Gen_Military.png    -> Stufe 2 / Mk II
  RectShield_Gen_HighTech.png    -> Stufe 3 / Mk III

Quelle/Pipeline: /home/user/generator_ansichten/prepare_sprites.py
(Nicht loeschen oder ueberschreiben - dieses Skript erzeugt nur die alten
Platzhalter, die 3 Forschungs-Icons, ModIcon und Preview neu.)
"""
from __future__ import annotations

from pathlib import Path

from PIL import Image, ImageDraw, ImageFont

ROOT = Path(__file__).resolve().parents[1]
TEX = ROOT / "Common/Textures"
BUILDINGS = TEX / "Things/Building/RectShield"
MENU_ICONS = TEX / "UI/Icons/Buildings"
RESEARCH_ICONS = TEX / "UI/Icons/Research"
GIZMOS = TEX / "UI/Gizmos"
for folder in (BUILDINGS, MENU_ICONS, RESEARCH_ICONS, GIZMOS, ROOT / "About"):
    folder.mkdir(parents=True, exist_ok=True)

BLUE = (70, 155, 255)
CYAN = (70, 240, 205)
AMBER = (245, 170, 55)     # Stufe 1 - Industriell
CRIMSON = (235, 85, 75)    # Stufe 2 - Militaerisch
ICE = (105, 225, 255)      # Stufe 3 - High-Tech
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


def gizmo_icon(path: Path, kind: str, size: int = 64) -> None:
    """Gizmo-Symbolgrafik: 'plus', 'minus' oder 'fit' (Rumpf-Anpassung)."""
    accent = {
        "plus": (110, 225, 255, 255),
        "minus": (255, 145, 95, 255),
        "fit": (120, 255, 200, 255),
    }[kind]

    img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    d.rounded_rectangle((3, 3, size - 4, size - 4), radius=10,
                        fill=(18, 24, 32, 240), outline=accent, width=2)
    d.rounded_rectangle((8, 8, size - 9, size - 9), radius=7,
                        outline=(accent[0], accent[1], accent[2], 90), width=1)

    c = size // 2
    if kind in ("plus", "minus"):
        r = 15
        d.line((c - r, c, c + r, c), fill=accent, width=6)
        if kind == "plus":
            d.line((c, c - r, c, c + r), fill=accent, width=6)
        # Kleine Achs-Markierung unten (W = Breite, H = Hoehe ist am Label zu sehen)
        d.ellipse((c - 2, size - 14, c + 2, size - 10), fill=accent)
    else:
        # Stilisiertes Schiff in einem groesseren Feld-Rahmen.
        d.rectangle((14, 14, size - 15, size - 15), outline=accent, width=3)
        d.rectangle((24, 24, size - 25, size - 25),
                    fill=(accent[0], accent[1], accent[2], 90),
                    outline=(accent[0], accent[1], accent[2], 200), width=2)
        for x, y in ((14, 14), (size - 15, 14), (14, size - 15), (size - 15, size - 15)):
            d.ellipse((x - 3, y - 3, x + 3, y + 3), fill=accent)

    img.save(path)
    print("  gizmo-icon:", path.relative_to(ROOT))


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


def preview(path: Path, tier_sources: list[Path]) -> None:
    """Vorschaubild: Kreis vs. Rechteck + die drei Stufen-Designs."""
    size = 512
    img = Image.new("RGB", (size, size), (18, 22, 28))
    d = ImageDraw.Draw(img)
    for i in range(0, size, 32):
        d.line((i, 0, i, size), fill=(28, 34, 42), width=1)
        d.line((0, i, size, i), fill=(28, 34, 42), width=1)

    f_big, f_small, f_label, f_tier = font(40), font(18), font(14), font(17)

    d.text((256, 52), "RectShield", font=f_big, fill=(235, 245, 255), anchor="mm")
    d.text((256, 92), "Rectangular shields for RimWorld", font=f_small,
           fill=(140, 165, 185), anchor="mm")

    # Alte Kuppel (durchgestrichen)
    d.ellipse((28, 140, 148, 260), outline=(105, 116, 130), width=4)
    d.ellipse((52, 164, 124, 236), outline=(55, 63, 73), width=2)
    d.line((36, 248, 140, 152), fill=(215, 70, 70), width=6)
    d.line((36, 152, 140, 248), fill=(215, 70, 70), width=6)
    d.text((88, 282), "circle / dome", font=f_label,
           fill=(150, 160, 172), anchor="mm")

    # Neues Rechteck
    d.rectangle((188, 148, 330, 252), outline=(70, 220, 255), width=4)
    d.rectangle((198, 158, 320, 242), outline=(40, 90, 110), width=2)
    d.text((259, 282), "rectangle", font=f_label,
           fill=(70, 220, 255), anchor="mm")

    # Die drei Stufen-Sprites
    tiers = [
        ("STAGE 1", "Mk I  -  Industrial", AMBER),
        ("STAGE 2", "Mk II  -  Military", CRIMSON),
        ("STAGE 3", "Mk III  -  High-Tech", ICE),
    ]
    for index, source in enumerate(tier_sources):
        sprite = Image.open(source).convert("RGBA")
        sprite.thumbnail((112, 112), Image.Resampling.LANCZOS)
        cx = 85 + index * 171
        x = cx - sprite.width // 2
        img.paste(sprite, (x, 306), sprite)
        d.rectangle((cx - 70, 302, cx + 70, 414), outline=(48, 58, 70), width=1)
        d.text((cx, 432), tiers[index][0], font=f_tier,
               fill=tiers[index][2], anchor="mm")
        d.text((cx, 454), tiers[index][1], font=f_label,
               fill=(180, 198, 215), anchor="mm")

    d.text((256, 482), "RimWorld 1.6  +  1.7 Waylith   |   three upgrade tiers",
           font=f_label, fill=(120, 140, 158), anchor="mm")
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

    # Die drei Stufen-Sprites sind KI-generiert und werden hier NICHT ueberschrieben.
    tier_paths = [
        BUILDINGS / "RectShield_Gen_Industrial.png",
        BUILDINGS / "RectShield_Gen_Military.png",
        BUILDINGS / "RectShield_Gen_HighTech.png",
    ]
    for p in tier_paths:
        if not p.exists():
            raise SystemExit(
                f"Fehlt: {p}\n"
                "  -> sprites/ aus /home/user/generator_ansichten/ nach "
                "Common/Textures/Things/Building/RectShield/ kopieren."
            )

    research_icon(RESEARCH_ICONS / "RectShield_Mk1_Research.png", tier_paths[0], AMBER, "I")
    research_icon(RESEARCH_ICONS / "RectShield_Mk2_Research.png", tier_paths[1], CRIMSON, "II")
    research_icon(RESEARCH_ICONS / "RectShield_Mk3_Research.png", tier_paths[2], ICE, "III")

    # + / - Symbole fuer die Groessen-Gizmos und Rumpf-Anpassung.
    gizmo_icon(GIZMOS / "RectShield_Plus.png", "plus")
    gizmo_icon(GIZMOS / "RectShield_Minus.png", "minus")
    gizmo_icon(GIZMOS / "RectShield_FitHull.png", "fit")

    mod_icon(ROOT / "About/ModIcon.png")
    # About/Preview.png ist die 1280x720-Workshop-Vorschau. Sie wird von
    # workshop-release/make_workshop_preview.py erzeugt und darf hier NICHT
    # durch die alte 512x512-Grafik ueberschrieben werden.
    print("  Hinweis: About/Preview.png (1280x720) kommt aus make_workshop_preview.py")

    legacy = BUILDINGS / "RectShield_Mk1.png"
    if legacy.exists():
        legacy.unlink()
        print("  entfernt (alter Platzhalter):", legacy.relative_to(ROOT))

    print("Fertig: alle Ingame-Grafiken erzeugt.")


if __name__ == "__main__":
    main()
