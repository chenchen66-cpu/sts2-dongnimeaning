"""Builds the Steam Workshop images for the mod.

Steam requires every preview image to be under 1 MB, so each output is generated at the largest
size in SIZE_LADDER that still fits, and the script fails loudly if one would be too big.

Outputs:
  workshop/image.png                  main workshop preview (16:9, from the event illustration)
  workshop/previews/dongni_defense.png
  workshop/previews/dongni_strike.png
  workshop/previews/dongni_curse.png   extra previews (card art)
"""

import os
from PIL import Image, ImageFilter

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
IMAGES = os.path.join(ROOT, "DongniDefense", "images")
WORKSHOP = os.path.join(ROOT, "workshop")
LIMIT = 950 * 1024  # keep a margin under Steam's 1 MB

SIZE_LADDER = [(1280, 720), (1152, 648), (1024, 576), (896, 504), (768, 432)]


def save_under_limit(img, path, ladder=SIZE_LADDER):
    os.makedirs(os.path.dirname(path), exist_ok=True)
    for size in ladder:
        candidate = img.resize(size, Image.LANCZOS)
        candidate.save(path, optimize=True)
        if os.path.getsize(path) < LIMIT:
            print(f"wrote {os.path.relpath(path, ROOT)} {size} "
                  f"{os.path.getsize(path) / 1024:.0f} KB")
            return
    raise SystemExit(f"{path} is still larger than {LIMIT} bytes at the smallest size")


def wide_16_9(src_path):
    """Crop to 16:9 (blur-extended so nothing important is cut off)."""
    img = Image.open(src_path).convert("RGB")
    target_ratio = 16 / 9
    w, h = img.size
    ratio = w / h

    if ratio > target_ratio:
        new_w = int(round(h * target_ratio))
        x = (w - new_w) // 2
        return img.crop((x, 0, x + new_w, h))

    new_h = int(round(w / target_ratio))
    backdrop = img.resize((w, new_h), Image.LANCZOS).filter(ImageFilter.GaussianBlur(30))
    y = (new_h - h) // 2
    backdrop.paste(img, (0, y))
    return backdrop


save_under_limit(wide_16_9(os.path.join(IMAGES, "events", "dongni_meaning.png")),
                 os.path.join(WORKSHOP, "image.png"))

for name in ("dongni_defense", "dongni_strike", "dongni_curse"):
    card = Image.open(os.path.join(IMAGES, "card_portraits", "big", f"{name}.png")).convert("RGB")
    save_under_limit(card, os.path.join(WORKSHOP, "previews", f"{name}.png"),
                     ladder=[(1000, 760), (750, 570), (600, 456), (500, 380)])
