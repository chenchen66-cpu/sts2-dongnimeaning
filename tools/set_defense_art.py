"""Builds the Dongni Defense card art from the supplied meme image.

Source: art_source/dongni_defense.jpg (1080x1145, pure black background).
The meme is fitted by height and the sides are padded with the same black, so nothing gets cropped
off (the "16" sword at the top and the creature's legs at the bottom both stay visible).

Outputs:
  images/card_portraits/dongni_defense.png        250x190
  images/card_portraits/big/dongni_defense.png   1000x760
"""

import os
from PIL import Image

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
SOURCE = os.path.join(ROOT, "art_source", "dongni_defense.jpg")
IMAGES = os.path.join(ROOT, "DongniDefense", "images")

BACKDROP = (0, 0, 0)


def build(width, height, out_path):
    img = Image.open(SOURCE).convert("RGB")
    scale = height / img.height
    scaled = img.resize((int(round(img.width * scale)), height), Image.LANCZOS)

    canvas = Image.new("RGB", (width, height), BACKDROP)
    if scaled.width > width:
        x = (scaled.width - width) // 2
        scaled = scaled.crop((x, 0, x + width, height))

    canvas.paste(scaled, ((width - scaled.width) // 2, 0))
    os.makedirs(os.path.dirname(out_path), exist_ok=True)
    canvas.save(out_path)
    print("wrote", os.path.relpath(out_path, ROOT), canvas.size)


build(250, 190, os.path.join(IMAGES, "card_portraits", "dongni_defense.png"))
build(1000, 760, os.path.join(IMAGES, "card_portraits", "big", "dongni_defense.png"))
