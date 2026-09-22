"""Builds the 东尼圣遗物 relic icons from the same meme used for 东尼防御's card art.

Source: art_source/dongni_defense.jpg (pure black background), fitted with black padding so the
whole picture stays visible at icon size.

Outputs:
  images/relics/dongni_sacred_relic.png          94x94   packed icon
  images/relics/dongni_sacred_relic_outline.png  94x94   outline layer (darkened copy)
  images/relics/big/dongni_sacred_relic.png     256x256  zoomed icon
"""

import os
from PIL import Image, ImageEnhance

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
SOURCE = os.path.join(ROOT, "art_source", "dongni_defense.jpg")
IMAGES = os.path.join(ROOT, "DongniDefense", "images")

BACKDROP = (0, 0, 0)


def fit_square(size):
    img = Image.open(SOURCE).convert("RGB")
    scale = size / img.height
    scaled = img.resize((int(round(img.width * scale)), size), Image.LANCZOS)

    canvas = Image.new("RGB", (size, size), BACKDROP)
    if scaled.width > size:
        x = (scaled.width - size) // 2
        scaled = scaled.crop((x, 0, x + size, size))
    canvas.paste(scaled, ((size - scaled.width) // 2, 0))
    return canvas


def save(img, folder, name):
    path = os.path.join(IMAGES, folder, name)
    os.makedirs(os.path.dirname(path), exist_ok=True)
    img.save(path)
    print("wrote", os.path.relpath(path, ROOT), img.size)


relic_94 = fit_square(94)
save(relic_94, "relics", "dongni_sacred_relic.png")
save(ImageEnhance.Brightness(relic_94).enhance(0.35), "relics", "dongni_sacred_relic_outline.png")
save(fit_square(256), os.path.join("relics", "big"), "dongni_sacred_relic.png")
