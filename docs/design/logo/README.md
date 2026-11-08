# Geisterfahrer logo

Pick: **a, Frontal**. A red 911-like car seen from the front, headlights on, on dark asphalt with faint road lines. It is the wrong-way driver coming at you. Candidates b and c are kept in `logo-preview.html` for comparison.

- b (road to the horizon): too thin at small sizes.
- c (a "G" made from the no-entry sign): rejected because it looks too much like a well-known "G" mark.

The car shape is generic 911-like (round lamps on fender humps, low wide stance). There is no Porsche crest, wordmark or any other brand mark in these files.

## Files

| File | Size | Use |
|---|---|---|
| `icon.svg`, `icon-1024.png` | 1024 × 1024, square, no rounded corners | Store icon and master. Launchers and the store apply their own mask. |
| `icon-foreground.svg`, `icon-foreground-432.png` | 108 dp canvas (432 px at xxxhdpi), transparent | Android adaptive icon, foreground. All key shapes sit inside the central 66 dp safe circle. |
| `icon-background.svg`, `icon-background-432.png` | 108 dp canvas, opaque | Android adaptive icon, background (asphalt and road lines). |
| `icon-monochrome.svg`, `icon-monochrome-432.png` | 108 dp canvas, black on transparent | Android 13 themed icon. The system tints the alpha shape. |
| `wordmark.svg`, `wordmark.png` | 1200 px wide, transparent, **white** text | Start screen and anything on a dark or 3D background. |
| `wordmark-dark.svg`, `wordmark-dark.png` | 1200 px wide, transparent, **dark** text | Light backgrounds. |
| `logo-horizontal.svg` | 864 × 200, dark text | Icon + wordmark for light backgrounds (README header in light mode). |
| `logo-horizontal-dark.svg` | 864 × 200, white text | Same for dark backgrounds (README header in dark mode). |
| `logo-preview.html` | | The three candidates side by side at 48 / 96 / 192 px on light and dark, plus Android masks. Open it in a browser. |
| `build_logo.py` | | Generates every SVG and the preview. Text is turned into outlines, so the SVGs need no font. |

The wordmark text is "GEISTERFAHRER" in Inter Black with +5 % letter spacing.

## Where to use what

- **App icon (Unity, Android):** Player Settings → Icon → Adaptive. Foreground = `icon-foreground-432.png`, background = `icon-background-432.png`. Use `icon-1024.png` for the round and legacy slots (Unity scales it). If the Unity version has a monochrome slot, use `icon-monochrome-432.png`.
- **Start screen:** `wordmark.png` in place of the text title, as a Raw Image about 1000 reference px wide (the reference canvas is 1920 × 1080). It is the only text-like element and is part of the logo.
- **Repository README header:**

  ```html
  <picture>
    <source media="(prefers-color-scheme: dark)" srcset="docs/design/logo/logo-horizontal-dark.svg">
    <img alt="Geisterfahrer" src="docs/design/logo/logo-horizontal.svg" height="64">
  </picture>
  ```

## Colours

| Role | Hex |
|---|---|
| Car red | `#C8102E` |
| Asphalt (background) | `#2B2D31` |
| Road lines on the asphalt | `#383A3F` |
| Windows and air intake | `#17181B` |
| Wheels | `#101114` |
| Headlights (warm white) | `#FFF1D0` |
| Wordmark on dark | `#F2F2F0` |
| Wordmark on light | `#2B2D31` |

The red is the same as the player car in the game (`docs/design/look-spec.md` section 8).

## Font and licence

- **Inter Black** by Rasmus Andersson, SIL Open Font License 1.1: [fonts.google.com/specimen/Inter](https://fonts.google.com/specimen/Inter). The OFL allows using the outlines in a logo.
- Candidate b only: **Barlow Semi Condensed SemiBold** by Jeremy Tribby, SIL OFL 1.1: [fonts.google.com/specimen/Barlow+Semi+Condensed](https://fonts.google.com/specimen/Barlow+Semi+Condensed).
- The artwork was drawn for this project and is part of it.

## Regenerate

Fetch the fonts first (`tools/fetch-assets.sh`, they land in `Assets/ThirdParty/Fonts`). Then:

```bash
python build_logo.py a      # a, b or c
```

The PNGs were rendered from the SVGs with headless Edge (`--screenshot`, transparent background) and their size, colours and transparency were checked pixel by pixel.
