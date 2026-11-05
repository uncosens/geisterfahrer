# UI Spec — Start, HUD, Pause, Game Over

Implementation spec for uGUI. Landscape only. Mockup of every screen at 16:9, 20:9 and 4:3: `ui/mockup.html`. Icons: `ui/icons/*.svg`.

Words on screen: only the title, numbers and the units `km/h` and `km`. Everything else is an icon (GDD 2.6, user taste).

## 1. Canvas setup

| Setting | Value |
|---|---|
| Canvas | Screen Space – Overlay, Pixel Perfect off |
| Canvas Scaler | Scale With Screen Size, reference **1920 × 1080**, Screen Match Mode *Match Width Or Height*, **Match 0.5**, Reference Pixels Per Unit 100 |
| Text | TextMeshPro (`TextMeshProUGUI`) |
| Raycasts | Only buttons have Raycast Target on. All other Images and Texts are off. |

Resulting canvas size (canvas units) on the three test ratios:

| Screen | Canvas size |
|---|---|
| 16:9 (1920 × 1080) | 1920 × 1080 |
| 20:9 (2400 × 1080) | 2147 × 966 |
| 4:3 (2048 × 1536) | 1663 × 1247 |

All positions below are in canvas units on that reference, and they work on all three. `pos` = `anchoredPosition` (x right, y up), `size` = `sizeDelta`.

### Hierarchy

```
Canvas
  Dim            (stretch full canvas, ignores safe area)      – one Image, alpha set per screen
  EdgeFlash      (stretch full canvas, ignores safe area)      – hit / crash red edges
  SafeArea       (stretch; script sets anchors to Screen.safeArea on start and on change)
    Panel_HUD
    Panel_Touch
    Panel_Start
    Panel_Pause
    Panel_GameOver
```

- **SafeArea:** `anchorMin = safeArea.min / screen size`, `anchorMax = safeArea.max / screen size`, offsets 0. All buttons and numbers live inside it.
- **Dim and EdgeFlash** cover the full screen, including cutouts.

## 2. Fonts

| Use | Font | File | Download |
|---|---|---|---|
| All numbers, units, title | **Inter** (OFL 1.1) | `Inter-Bold.ttf`, `Inter-SemiBold.ttf`, `Inter-Black.ttf` (static folder) | [Google Fonts: Inter](https://fonts.google.com/specimen/Inter) → "Get font" → "Download all". Or [rsms/inter releases](https://github.com/rsms/inter/releases). |
| Road signs and km posts (world space, see `look-spec.md`) | **Barlow Semi Condensed** (OFL 1.1) | `BarlowSemiCondensed-SemiBold.ttf` | [Google Fonts: Barlow Semi Condensed](https://fonts.google.com/specimen/Barlow+Semi+Condensed) |

TMP font assets (Font Asset Creator):

| Asset | Source | Sampling size | Padding | Atlas | Render mode | Characters |
|---|---|---|---|---|---|---|
| `Inter-Bold SDF` | Inter-Bold | 96 | 10 | 1024 × 1024 | SDFAA | `0123456789.,- —kmh/` |
| `Inter-SemiBold SDF` | Inter-SemiBold | 72 | 8 | 512 × 512 | SDFAA | same |
| `Inter-Black SDF` | Inter-Black | 96 | 10 | 1024 × 1024 | SDFAA | `GEISTERFAHR` |

**Material preset `HUD-Shadow`** (on top of the Bold and SemiBold assets) is used for every text that sits over the 3D scene:
- Underlay on, colour `#000000` alpha 0.55.
- Offset X 0, Y −0.6. Dilate 0. Softness 0.6.

**Fixed-width digits:** wrap changing numbers in `<mspace=0.6em>…</mspace>`, so the text does not jitter while counting.

## 3. Colours

| Token | Hex | Alpha | Use |
|---|---|---|---|
| `text` | `#FFFFFF` | 1.0 | numbers |
| `text-dim` | `#FFFFFF` | 0.75 | units, best values |
| `icon-dim` | `#FFFFFF` | 0.70 | label icons |
| `player` / primary | `#3D7BFF` | 1.0 | primary buttons |
| primary pressed | `#2F66DB` | 1.0 | |
| `danger` | `#FF4D4F` | per effect | hit and crash edges |
| `good` / new best | `#2FD27A` | 1.0 | new-best values and the star |
| `dim` | `#05070A` | Start 0.40 · Pause 0.55 · Game over 0.60 | Dim image |
| `scrim` (round icon bg) | `#000000` | 0.35 | pause button background |
| secondary fill | `#FFFFFF` | 0.10 | secondary buttons |
| secondary outline | `#FFFFFF` | 0.55 | 4 px outline |

## 4. Sprites to make (from `ui/icons/` and simple shapes)

| Sprite | Size | Note |
|---|---|---|
| `pause`, `play`, `retry`, `home`, `trophy`, `speed`, `distance`, `new-best` | export each SVG to PNG at **128 × 128** (Inkscape or `rsvg-convert -w 128`) | White on transparent, so they can be tinted by Image colour. Import as Sprite (2D/UI), no mipmaps, ASTC 6×6. |
| `circle` | 128 × 128 white disc | pause button background |
| `ring` | 256 × 256, 6 px white stroke | steering touch ring |
| `rounded-44` | 128 × 128 white, corner radius 44, 9-slice border 48 | button fill |
| `rounded-44-outline` | 128 × 128, 4 px white stroke, radius 44, 9-slice 48 | secondary button outline |
| `bar` | 16 × 8 white, radius 3, 9-slice 4 | speed bar |
| `edge` | 256 × 4, white, alpha 1 → 0 left to right (smooth) | edge flashes (rotate or flip for each side) |

## 5. Buttons (shared)

| Style | Size | Visual | Icon | Pressed |
|---|---|---|---|---|
| **Primary** | 176 × 176 (Start Play: 208 × 208) | `rounded-44`, `#3D7BFF` | 80 × 80 (Play: 96), `#FFFFFF`. Play icon offset x +6 so it looks centred. | colour `#2F66DB`, scale 0.96, 0.06 s |
| **Secondary** | 176 × 176 | `rounded-44` `#FFFFFF` α 0.10 + child `rounded-44-outline` `#FFFFFF` α 0.55 | 72 × 72, `#FFFFFF` α 0.90 | fill α 0.18, scale 0.96 |

- The button's hit area is the full rect.
- Buttons on a newly shown panel ignore taps for **0.6 s** (Start excepted), so a thumb still on the screen after a crash cannot press Retry by accident.

## 6. Screens

All pivots are (0.5, 0.5) unless stated. Anchor shorthand: TL = top-left (0,1), TC = top-centre (0.5,1), TR = top-right (1,1), C = centre (0.5,0.5).

### 6.1 Start (`Panel_Start`)

Scene behind: the road with the player car, camera at its normal chase position, traffic paused. Dim α 0.40.

| Element | Anchor | Pivot | pos | size | Content / style |
|---|---|---|---|---|---|
| Title | C | C | (0, 190) | 1400 × 160 | `GEISTERFAHRER`, Inter-Black SDF 128, character spacing 6, `#FFFFFF`, HUD-Shadow, centre |
| Best speed (group) | C | C | (−260, 10) | 460 × 72 | `trophy` 40 × 40 `icon-dim`, then 12 gap, then `297.4` Inter-SemiBold 52 `#FFFFFF` α 0.9 + `<size=36><alpha=#B3> km/h`. Horizontal Layout, middle centre. |
| Best distance (group) | C | C | (260, 10) | 460 × 72 | same, `18.92 km` |
| Play | C | C | (0, −230) | 208 × 208 | Primary, `play` icon 96 |

- With no best yet, the two best groups are empty, but their rects stay so nothing moves.
- Tapping Play starts the run at 100 km/h (GDD 2.2). The HUD fades in over 0.2 s.

### 6.2 HUD (`Panel_HUD`)

| Element | Anchor | Pivot | pos | size | Content / style |
|---|---|---|---|---|---|
| Speed (group) | TL | (0, 1) | (56, −36) | 520 × 152 | container |
| └ SpeedValue | TL | (0, 1) | (0, 0) | 330 × 128 | `<mspace=0.6em>297</mspace>`, Inter-Bold SDF **120**, `#FFFFFF`, HUD-Shadow, alignment **Right / Bottom**. Integer km/h, rounded down. |
| └ SpeedUnit | TL | (0, 1) | (342, −64) | 180 × 64 | `km/h`, Inter-SemiBold SDF 44, `#FFFFFF` α 0.75, HUD-Shadow, alignment **Left / Bottom**. Then move it up until its baseline matches SpeedValue (about +14 for Inter). |
| └ SpeedBar track | TL | (0, 1) | (4, −144) | 400 × 6 | `bar`, `#FFFFFF` α 0.20 |
| └ SpeedBar fill | stretch in track | (0, 0.5) | 0 | 0 | `bar`, `#FFFFFF` α 0.90, Image Type Filled, Horizontal, Origin Left, `fillAmount = speed / TopSpeed` (306.2 km/h) |
| Distance | TC | (0.5, 1) | (0, −48) | 480 × 96 | `<mspace=0.6em>4.37</mspace><size=44><alpha=#BF> km`, Inter-Bold SDF **80**, `#FFFFFF`, HUD-Shadow, centre. Two decimals (GDD 2.3). |
| Pause button | TR | (1, 1) | (−40, −28) | 128 × 128 | transparent hit Image; child `circle` 96 × 96 `#000000` α 0.35; child `pause` 44 × 44 `#FFFFFF` |

**Speedometer: style a (digital number + thin bar) is chosen.**
- The GDD shows speed as an exact number (`297 km/h`), and top speed is a record with one decimal. A digital read-out matches what the player compares.
- The game has no hard top speed (306.2 km/h is a drag ceiling, not a cap). An arc gauge needs a clear maximum. The thin bar uses the ceiling only as a soft scale.
- It takes the least space at the top, and it matches the distance number, so the HUD reads as one system.

**Lower part of the screen:** no HUD element below the speed bar on the left (y −180 from the top) or below the pause button on the right (y −156). Everything under that belongs to the thumbs.

### 6.3 Touch feedback (`Panel_Touch`, no raycasts)

| Element | size | Style | Behaviour |
|---|---|---|---|
| Ring | 168 × 168 | `ring`, `#FFFFFF` α 0.16 | Appears at the first touch point of the **left half**. Fade in 0.08 s, fade out 0.15 s on release. |
| Dot | 32 × 32 | `circle`, `#FFFFFF` α 0.40 | Follows the finger on x only, clamped to ±56 from the ring centre (so it reads as steering amount) |

The right half (throttle) shows nothing; the speed number is the feedback.

### 6.4 Pause (`Panel_Pause`)

The scene freezes. Dim α 0.55. Speed and distance stay visible under the dim. The pause button hides.

| Element | Anchor | pos | size | Content |
|---|---|---|---|---|
| Home | C | (−128, 0) | 176 × 176 | Secondary, `home` |
| Resume | C | (128, 0) | 176 × 176 | Primary, `play` (offset x +6) |

Primary is on the right, under the right thumb. On Resume, the run continues after a 0.3 s fade, with no countdown.

### 6.5 Game over (`Panel_GameOver`)

Shown **1.0 s real time** after the crash, after the slow-motion beat (`look-spec.md` §8). Dim α 0.60, fade 0.25 s. The HUD hides.

Two columns, each a container (anchor C):

| Column | pos | size |
|---|---|---|
| Top speed | (−320, 120) | 560 × 360 |
| Distance | (320, 120) | 560 × 360 |

Inside each column (anchor C of the column):

| Element | pos | size | Content / style |
|---|---|---|---|
| Label icon | (0, 140) | 56 × 56 | `speed` or `distance`, `icon-dim` |
| New-best star | (52, 160) | 36 × 36 | `new-best`, `#2FD27A`. Visible only when this run beat the best; the rect always exists. |
| Run value | (0, 40) | 560 × 140 | `<mspace=0.6em>297.4</mspace><size=48><alpha=#BF> km/h` (or `4.37 km`), Inter-Bold SDF **120**, centre, HUD-Shadow. Colour `#FFFFFF`, or **`#2FD27A`** when it is a new best. |
| Best row | (0, −80) | 560 × 64 | Horizontal Layout, middle centre, spacing 14: `trophy` 36 × 36 `icon-dim` + `312.0 km/h` Inter-SemiBold SDF 52, `#FFFFFF` α 0.75. On a new best it shows the new value in `#2FD27A` α 1. |

Buttons:

| Element | Anchor | pos | size | Content |
|---|---|---|---|---|
| Home | C | (−128, −300) | 176 × 176 | Secondary, `home` |
| Retry | C | (128, −300) | 176 × 176 | Primary, `retry` |

Number formats (GDD 2.3):
- Top speed has one decimal (`297.4`).
- Distance has two decimals (`4.37`).
- No thousands separator.

## 7. Edge flash (`EdgeFlash`)

Four Images using the `edge` sprite: Left and Right are 220 wide and stretch vertically, Top and Bottom are 160 tall and stretch horizontally. Colour `#FF4D4F`. The alpha animation is in `look-spec.md` §8:
- Light hit: one side only.
- Crash: all four sides.

## 8. Fit check

Checked in `ui/mockup.html` with the canvas sizes from §1:
- **20:9** (canvas 966 tall): game over spans y +288 to −388, inside ±483. A simulated 90-unit camera cutout on the left keeps the speed group clear.
- **4:3** (canvas 1663 wide):
  - The speed group ends at x 576 and the distance box starts at 591, so they don't overlap.
  - The game-over columns span ±600, inside ±831.
- **16:9:** reference, no issues.
