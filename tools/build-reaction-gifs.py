from __future__ import annotations

import argparse
import math
from pathlib import Path

from PIL import Image, ImageEnhance


REACTIONS = (
    ("failing-noob.gif", 0, 0, "shake"),
    ("confused-goblin.gif", 1, 0, "sway"),
    ("barely-operational.gif", 0, 1, "smug"),
    ("certified-menace.gif", 1, 1, "bounce"),
)


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Build the CertPrep reaction GIFs.")
    parser.add_argument("source", type=Path)
    parser.add_argument("output", type=Path)
    return parser.parse_args()


def crop_panel(source: Image.Image, column: int, row: int) -> Image.Image:
    width, height = source.size
    left = column * (width // 2)
    top = row * (height // 2)
    right = (column + 1) * (width // 2)
    bottom = (row + 1) * (height // 2)
    inset = max(4, width // 512)
    return source.crop((left + inset, top + inset, right - inset, bottom - inset))


def transform(panel: Image.Image, frame_index: int, motion: str) -> Image.Image:
    phase = frame_index * math.tau / 12
    if motion == "shake":
        scale = 1.035
        x = round(math.sin(phase * 3) * 5)
        y = round(abs(math.sin(phase)) * 2)
        rotation = math.sin(phase * 3) * 0.8
    elif motion == "sway":
        scale = 1.045 + (math.sin(phase) * 0.012)
        x = round(math.sin(phase) * 4)
        y = round(math.cos(phase * 2) * 2)
        rotation = math.sin(phase) * 1.2
    elif motion == "smug":
        scale = 1.025 + (math.sin(phase) * 0.008)
        x = 0
        y = round(math.sin(phase) * 2)
        rotation = math.sin(phase) * 0.35
    else:
        scale = 1.06 + (math.sin(phase) * 0.025)
        x = round(math.sin(phase) * 2)
        y = round(-abs(math.sin(phase)) * 6)
        rotation = math.sin(phase) * 1.0

    target = 256
    enlarged = panel.resize(
        (round(target * scale), round(target * scale)),
        Image.Resampling.LANCZOS,
    ).rotate(rotation, resample=Image.Resampling.BICUBIC, expand=False)
    left = max(0, (enlarged.width - target) // 2 - x)
    top = max(0, (enlarged.height - target) // 2 - y)
    frame = enlarged.crop((left, top, left + target, top + target))
    if motion == "shake" and frame_index in (1, 4, 7, 10):
        frame = ImageEnhance.Color(frame).enhance(0.82)
    return frame.convert("P", palette=Image.Palette.ADAPTIVE, colors=192)


def build_gif(panel: Image.Image, output_path: Path, motion: str) -> None:
    square = panel.resize((256, 256), Image.Resampling.LANCZOS).convert("RGB")
    frames = [transform(square, index, motion) for index in range(12)]
    frames[0].save(
        output_path,
        save_all=True,
        append_images=frames[1:],
        duration=85,
        loop=0,
        disposal=2,
        optimize=True,
    )


def main() -> None:
    args = parse_args()
    args.output.mkdir(parents=True, exist_ok=True)
    with Image.open(args.source) as source:
        source.load()
        for filename, column, row, motion in REACTIONS:
            build_gif(crop_panel(source, column, row), args.output / filename, motion)


if __name__ == "__main__":
    main()
