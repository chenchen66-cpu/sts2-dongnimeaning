"""Generates placeholder art for the Dongni Defense mod.

The game expects these exact sizes:
  images/card_portraits/<name>.png          250x190   in-hand card art
  images/card_portraits/big/<name>.png     1000x760   zoomed card art
  images/powers/<name>.png                    64x64   power icon
  images/powers/big/<name>.png              256x256   zoomed power icon

This is a simple, code-drawn stand-in (a glowing shield emblem). Replace the PNGs
with hand-made or AI-generated art whenever a nicer look is wanted.
"""

import math
import os
from PIL import Image, ImageDraw, ImageFilter

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
IMAGES = os.path.join(ROOT, "DongniDefense", "images")

CARD_NAME = "dongni_defense"
POWER_NAME = "dongni_defense_power"


def lerp(a, b, t):
    return tuple(int(round(a[i] + (b[i] - a[i]) * t)) for i in range(3))


def vertical_gradient(size, top, bottom):
    w, h = size
    strip = Image.new("RGB", (1, h))
    px = strip.load()
    for y in range(h):
        px[0, y] = lerp(top, bottom, y / max(1, h - 1))
    return strip.resize((w, h), Image.BILINEAR)


def shield_points(cx, cy, w, h, power=2.2, steps=64):
    """Shield silhouette: flat top, curved sides meeting at a bottom point."""
    pts = []
    for i in range(steps + 1):
        t = i / steps
        y = -h / 2 + t * h
        x = (w / 2) * (1 - t**power)
        pts.append((cx + x, cy + y))
    for i in range(steps, -1, -1):
        t = i / steps
        y = -h / 2 + t * h
        x = (w / 2) * (1 - t**power)
        pts.append((cx - x, cy + y))
    return pts


def draw_shield_layer(size, box):
    """Shield with a brushed-metal vertical gradient, rim highlight and inner emblem."""
    layer = Image.new("RGBA", size, (0, 0, 0, 0))
    cx, cy = box[0]
    w, h = box[1]
    outline = shield_points(cx, cy, w, h, steps=160)

    mask = Image.new("L", size, 0)
    ImageDraw.Draw(mask).polygon(outline, fill=255)

    # brushed metal: light on the upper edge, deep blue at the point
    metal = vertical_gradient(size, (238, 248, 255), (86, 140, 190)).convert("RGBA")
    layer = Image.composite(metal, layer, mask)

    d = ImageDraw.Draw(layer)
    d.line(outline + [outline[0]], fill=(250, 253, 255, 255), width=max(2, int(w * 0.014)), joint="curve")
    d.line(outline + [outline[0]], fill=(24, 46, 70, 90), width=max(1, int(w * 0.028)), joint="curve")

    # rivets across the top band
    for i in range(5):
        rx = cx - w * 0.32 + w * 0.16 * i
        r = w * 0.018
        d.ellipse([rx - r, cy - h * 0.40 - r, rx + r, cy - h * 0.40 + r], fill=(40, 66, 92, 220))
        d.ellipse([rx - r * 0.5, cy - h * 0.41 - r * 0.5, rx + r * 0.5, cy - h * 0.41 + r * 0.5],
                  fill=(230, 244, 255, 200))

    # inner field + chevron emblem
    inner = shield_points(cx, cy + h * 0.03, w * 0.64, h * 0.60)
    d.polygon(inner, fill=(26, 48, 72, 240))
    d.line(inner + [inner[0]], fill=(150, 205, 245, 200), width=max(1, int(w * 0.008)), joint="curve")
    chev = [
        (cx - w * 0.21, cy - h * 0.09),
        (cx, cy + h * 0.11),
        (cx + w * 0.21, cy - h * 0.09),
        (cx + w * 0.21, cy + h * 0.03),
        (cx, cy + h * 0.23),
        (cx - w * 0.21, cy + h * 0.03),
    ]
    d.polygon(chev, fill=(168, 222, 255, 255))
    return layer


def barrier_ring(size, box, width):
    """Hexagonal energy barrier drawn behind the emblem."""
    layer = Image.new("RGBA", size, (0, 0, 0, 0))
    d = ImageDraw.Draw(layer)
    cx, cy = box[0]
    r = box[1]
    pts = []
    for i in range(6):
        a = math.pi / 6 + i * math.pi / 3
        pts.append((cx + r * math.cos(a), cy + r * math.sin(a) * 1.12))
    d.line(pts + [pts[0]], fill=(130, 200, 255, 120), width=width, joint="curve")
    return layer


def card_art(w, h):
    base = vertical_gradient((w, h), (32, 45, 60), (10, 15, 21)).convert("RGBA")

    # radial light behind the emblem
    glow = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    gd = ImageDraw.Draw(glow)
    r = int(min(w, h) * 0.45)
    gd.ellipse([w // 2 - r, h // 2 - r, w // 2 + r, h // 2 + r], fill=(90, 165, 235, 120))
    glow = glow.filter(ImageFilter.GaussianBlur(radius=max(8, int(r * 0.35))))
    base = Image.alpha_composite(base, glow)

    ring = barrier_ring((w, h), ((w / 2, h * 0.52), min(w, h) * 0.60), max(2, int(min(w, h) * 0.012)))
    ring = ring.filter(ImageFilter.GaussianBlur(radius=max(1, min(w, h) * 0.004)))
    base = Image.alpha_composite(base, ring)

    shield = draw_shield_layer((w, h), ((w / 2, h * 0.52), (min(w, h) * 0.46, min(w, h) * 0.56)))

    # outer glow of the shield
    halo = shield.filter(ImageFilter.GaussianBlur(radius=max(6, int(min(w, h) * 0.035))))
    tint = Image.new("RGBA", (w, h), (120, 200, 255, 0))
    tint.putalpha(halo.getchannel("A").point(lambda a: int(a * 0.85)))
    base = Image.alpha_composite(base, tint)

    base = Image.alpha_composite(base, shield)

    # vignette
    vig = Image.new("L", (w, h), 0)
    vd = ImageDraw.Draw(vig)
    vd.ellipse([-w * 0.15, -h * 0.25, w * 1.15, h * 1.15], fill=255)
    vig = vig.filter(ImageFilter.GaussianBlur(radius=max(10, int(min(w, h) * 0.12))))
    dark = Image.new("RGBA", (w, h), (0, 0, 0, 190))
    base = Image.composite(base, Image.alpha_composite(base, dark), vig)
    return base.convert("RGB")


def power_icon(size):
    img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    shield = draw_shield_layer((size, size), ((size / 2, size / 2), (size * 0.66, size * 0.78)))

    halo = shield.filter(ImageFilter.GaussianBlur(radius=max(2, size * 0.05)))
    tint = Image.new("RGBA", (size, size), (150, 215, 255, 0))
    tint.putalpha(halo.getchannel("A").point(lambda a: int(a * 0.9)))

    img = Image.alpha_composite(img, tint)
    img = Image.alpha_composite(img, shield)
    return img


def save(img, name, folder):
    path = os.path.join(IMAGES, folder, name)
    os.makedirs(os.path.dirname(path), exist_ok=True)
    img.save(path)
    print("wrote", os.path.relpath(path, ROOT), img.size)


save(card_art(1000, 760), f"{CARD_NAME}.png", os.path.join("card_portraits", "big"))
save(card_art(250, 190), f"{CARD_NAME}.png", "card_portraits")
save(power_icon(256), f"{POWER_NAME}.png", os.path.join("powers", "big"))
save(power_icon(256).resize((64, 64), Image.LANCZOS), f"{POWER_NAME}.png", "powers")
