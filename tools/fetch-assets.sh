#!/usr/bin/env bash
# Downloads the free (CC0 / OFL) assets into the git-ignored Assets/ThirdParty folder.
# Vehicle models need a Sketchfab login, so they are downloaded by hand (see CREDITS.md).
set -euo pipefail
cd "$(dirname "$0")/.."
T=Assets/ThirdParty
mkdir -p "$T/Sky" "$T/Road" "$T/Fonts" "$T/Vehicles"
get() { [ -s "$2" ] || curl -fsSL "$1" -o "$2"; }

PH=https://dl.polyhaven.org/file/ph-assets
get "$PH/HDRIs/hdr/2k/dreifaltigkeitsberg_2k.hdr" "$T/Sky/dreifaltigkeitsberg_2k.hdr"
get "$PH/Textures/jpg/1k/asphalt_01/asphalt_01_diff_1k.jpg" "$T/Road/asphalt_01_diff_1k.jpg"
get "$PH/Textures/jpg/1k/asphalt_01/asphalt_01_nor_gl_1k.jpg" "$T/Road/asphalt_01_nor_gl_1k.jpg"

tmp=$(mktemp -d)
if [ ! -s "$T/Road/Grass004_1K-JPG_Color.jpg" ]; then
  curl -fsSL "https://ambientcg.com/get?file=Grass004_1K-JPG.zip" -o "$tmp/grass.zip"
  unzip -o -j -q "$tmp/grass.zip" "Grass004_1K-JPG_Color.jpg" -d "$T/Road"
fi
if [ ! -s "$T/Fonts/Inter-Bold.ttf" ]; then
  curl -fsSL "https://github.com/rsms/inter/releases/download/v4.1/Inter-4.1.zip" -o "$tmp/inter.zip"
  unzip -o -j -q "$tmp/inter.zip" "extras/ttf/Inter-Bold.ttf" "extras/ttf/Inter-SemiBold.ttf" "extras/ttf/Inter-Black.ttf" -d "$T/Fonts"
fi
get "https://github.com/google/fonts/raw/main/ofl/barlowsemicondensed/BarlowSemiCondensed-SemiBold.ttf" "$T/Fonts/BarlowSemiCondensed-SemiBold.ttf"
rm -rf "$tmp"
ls -R "$T"
