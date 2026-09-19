"""Turns the supplied artwork in Desktop/teste/dnys into the sizes the game expects.

Sources (2560x1696, "豆包AI生成" watermark in the bottom-right corner):
  dongnistrike.png   -> Dongni Strike card art
  dongni诅咒.png      -> Dongni's Curse card art
  eventbackground.png -> Dongni Meaning event illustration

Targets:
  images/card_portraits/<name>.png         250x190   in-hand card art
  images/card_portraits/big/<name>.png    1000x760   zoomed card art
  images/events/<name>.png             3440x1613   event illustration
"""

import os
from PIL import Image, ImageFilter, ImageEnhance

SRC = r"C:\Users\SUSV\Desktop\teste\dnys"
ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
IMAGES = os.path.join(ROOT, "DongniDefense", "images")

# card art: drop the watermarked bottom band, then crop to the 1000:760 aspect
CARD_Y_MAX = 1600
CARD_ASPECT = 1000 / 760


def save(img, folder, name):
    path = os.path.join(IMAGES, folder, name)
    os.makedirs(os.path.dirname(path), exist_ok=True)
    img.save(path)
    print("wrote", os.path.relpath(path, ROOT), img.size)


def card_art(src_name, out_name):
    img = Image.open(os.path.join(SRC, src_name)).convert("RGB")
    w, h = img.size

    crop_h = min(CARD_Y_MAX, h)
    crop_w = int(round(crop_h * CARD_ASPECT))
    if crop_w > w:
        crop_w = w
        crop_h = int(round(crop_w / CARD_ASPECT))
    x0 = (w - crop_w) // 2
    img = img.crop((x0, 0, x0 + crop_w, crop_h))

    save(img.resize((1000, 760), Image.LANCZOS), os.path.join("card_portraits", "big"), out_name)
    save(img.resize((250, 190), Image.LANCZOS), "card_portraits", out_name)


def event_art(src_name, out_name, size=(3440, 1613)):
    img = Image.open(os.path.join(SRC, src_name)).convert("RGB")
    w, h = img.size
    tw, th = size

    # blurred, slightly darkened backdrop fills the sides
    backdrop = img.resize((tw, th), Image.LANCZOS).filter(ImageFilter.GaussianBlur(42))
    backdrop = ImageEnhance.Brightness(backdrop).enhance(0.55)

    scale = th / h
    scaled = img.resize((int(round(w * scale)), th), Image.LANCZOS)
    x = (tw - scaled.width) // 2
    backdrop.paste(scaled, (x, 0))
    save(backdrop, "events", out_name)


card_art("dongnistrike.png", "dongni_strike.png")
card_art("dongni诅咒.png", "dongni_curse.png")
event_art("eventbackground.png", "dongni_meaning.png")
