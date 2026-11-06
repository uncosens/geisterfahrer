# UI Spec — Start, HUD, Pause, Game Over

Implementation spec for uGUI. Landscape only. Mockup of every screen at 16:9, 20:9 and 4:3: `ui/mockup.html`. Icons: `ui/icons/*.svg`.

Words on screen: only the title (the game name), numbers and the units `km/h` and `km`. Everything else is an icon (GDD 2.6, user taste). If any text is ever needed, it is English (docs/decisions.md #5).

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
  EdgeFlash      (stretch full canvas, ignores safe area)      – hit (amber) / crash (white) edges
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
| `accent` / primary | `#3D7BFF` | 1.0 | primary buttons (UI only; no longer the car colour) |
| primary pressed | `#2F66DB` | 1.0 | |
| `hit` | `#FFB020` (amber) | per effect | light-hit edge flash |
| `crash` | `#FFFFFF` | per effect | crash edge flash, followed by the grey-out (`look-spec.md` §9.2) |
| `good` / new best | `#2FD27A` | 1.0 | new-best values and the star |
| `dim` | `#05070A` | Start 0.40 · Pause 0.55 · Game over 0.60 | Dim image |
| `scrim` (round icon bg) | `#000000` | 0.35 | pause button background |
| secondary fill | `#FFFFFF` | 0.10 | secondary buttons |
| secondary outline | `#FFFFFF` | 0.55 | 4 px outline |

### Why danger is no longer red

The player car is now a **red 911** (user decision). The car is always at the bottom centre of the screen. A red edge flash would echo the car's own colour and read as "that's me", not "that hurt". So:

- **No red anywhere in the UI or in feedback.** Red belongs to the player car only (and to real tail lights, which are small and dark).
- **Light hit = amber `#FFB020`.** Amber is a different hue from the car (orange-yellow against red), it matches the sparks, and it reads as a warning.
- **Crash = white flash, then the whole scene drains to grey** (saturation −100, `look-spec.md` §9.2). A crash is told by shape and motion (full-screen flash, slow motion, colour loss), not by a hue, so it cannot be mixed up with the car.
- **The UI accent stays blue `#3D7BFF`,** which is the opposite hue to the car. New best stays green `#2FD27A`.

## 4. Sprites to make (from `ui/icons/` and simple shapes)

| Sprite | Size | Note |
|---|---|---|
| `pause`, `play`, `retry`, `home`, `trophy`, `speed`, `distance`, `new-best` | export each SVG to PNG at **128 × 128** (Inkscape or `rsvg-convert -w 128`) | White on transparent, so they can be tinted by Image colour. Import as Sprite (2D/UI), no mipmaps, ASTC 6×6. |
| `circle` | 128 × 128 white disc | pause button background |
| `ring` | 256 × 256, 6 px white stroke | steering touch ring |
| `rounded-44` | 128 × 128 white, corner radius 44, 9-slice border 48 | button fill |
| `rounded-44-outline` | 128 × 128, 4 px white stroke, radius 44, 9-slice 48 | secondary button outline |
| `gauge-face` | `ui/icons/gauge-face.svg` → PNG **512 × 512** | dial face with ticks (full colour, keep alpha) |
| `gauge-needle` | `ui/icons/gauge-needle.svg` → PNG 24 × 324 | needle; set the sprite pivot to (0.5, 0.136) |
| `gauge-hub` | `ui/icons/gauge-hub.svg` → PNG 64 × 64 | needle hub |
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
- Tapping Play hides this panel and shows the HUD (fade 0.2 s) with the car at **standstill, 0 km/h** (GDD 2.2). The run clock, traffic and road phase start on the **first accelerator press** (GDD 2.4); nothing on screen announces it.

### 6.2 HUD (`Panel_HUD`)

| Element | Anchor | Pivot | pos | size | Content / style |
|---|---|---|---|---|---|
User decision: the speedometer is a **round dial in the top-right corner**, styled like a 911 instrument. It replaces the earlier digital style a. The pause button moves to the top-left.

| Element | Anchor | Pivot | pos | size | Content / style |
|---|---|---|---|---|---|
| Pause button | TL | (0, 1) | (40, −28) | 128 × 128 | transparent hit Image; child `circle` 96 × 96 `#000000` α 0.35; child `pause` 44 × 44 `#FFFFFF` |
| Distance | TC | (0.5, 1) | (0, −48) | 480 × 96 | `<mspace=0.6em>4.37</mspace><size=44><alpha=#BF> km`, Inter-Bold SDF **80**, `#FFFFFF`, HUD-Shadow, centre. Two decimals (GDD 2.3). |
| **Gauge** (root) | TR | (1, 1) | (−36, −24) | 320 × 320 | container. `localScale = clamp(canvasHeight × 0.30 / 320, 0.875, 1.0)`: 0.906 on 20:9 (290 units), 1.0 on 16:9 and 4:3. Children below are relative to the gauge centre (anchor C, pivot C). |
| └ Face | C | C | (0, 0) | 320 × 320 | sprite `gauge-face` (dark face α 0.72, chrome bezel, ticks). Not tinted. |
| └ Numerals ×6 | C | C | see table below | 48 × 28 each | `0 60 120 180 240 300`, Inter-SemiBold SDF **26**, `#F4F4F2` α 0.9, centre, no shadow |
| └ Digital value | C | C | (0, −96) | 100 × 56 | `<mspace=0.6em>297</mspace>`, Inter-Bold SDF **56**, `#FFFFFF`, centre. Integer km/h, rounded down. |
| └ Unit | C | C | (0, −128) | 100 × 24 | `km/h`, Inter-SemiBold SDF 20, `#FFFFFF` α 0.65, centre |
| └ Needle | C | (0.5, 0.136) | (0, 0) | 12 × 162 | sprite `gauge-needle`, `#F4F4F2`. The pivot sits on the hub (22 of 162 from the bottom). `rotation.z = −(−135 + 270 × v / 330)` degrees (Unity z rotation is counter-clockwise). |
| └ Hub | C | C | (0, 0) | 32 × 32 | sprite `gauge-hub`, drawn above the needle |

**Scale:** 0 to 330 km/h over 270°. 0 sits at the lower left (−135° from straight up) and 330 at the lower right (+135°). The 90° gap at the bottom holds the digital value. Ticks are every 10 km/h, with medium ticks every 30 and long ticks every 60 plus 330. The ceiling of 306 km/h sits at +115°, so a top-speed run puts the needle near the end, as in a real car.

| Numeral | pos (x, y) |
|---|---|
| 0 | (−76.4, −76.4) |
| 60 | (−107.7, 7.7) |
| 120 | (−64.7, 86.5) |
| 180 | (23.0, 105.5) |
| 240 | (94.8, 51.8) |
| 300 | (101.2, −37.7) |

These are radius 108 from the centre. The digital box (±50) clears the "0" numeral (x ≤ −52).

**Needle smoothing:**
- A critically damped spring on the displayed speed: `ω = 14 rad/s` (about 0.15 s to settle).
- Max needle speed 600°/s, so a hit's sudden drop still sweeps visibly instead of jumping.
- Idle wobble: none.
- **Digital value:** updated at **10 Hz** from the *real* speed (not the smoothed one), so the number does not flicker but stays exact.

**Colours:** the needle is white `#F4F4F2`, not red. Real 911 needles are often red-orange, but red is the player car (§3). There is no red zone on the dial.

**Thumb areas:** the gauge's bottom edge is at y −344 (−314 on 20:9). Throttle touches on the right half still work through it (Raycast Target off on every gauge Image). Nothing interactive sits below the pause button on the left (y −156) or below the gauge on the right.

### 6.3 Touch feedback (`Panel_Touch`, no raycasts)

| Element | size | Style | Behaviour |
|---|---|---|---|
| Ring | 168 × 168 | `ring`, `#FFFFFF` α 0.16 | Appears at the first touch point of the **left half**. Fade in 0.08 s, fade out 0.15 s on release. |
| Dot | 32 × 32 | `circle`, `#FFFFFF` α 0.40 | Follows the finger on x only, clamped to ±56 from the ring centre (so it reads as steering amount) |

The right half (throttle) shows nothing; the gauge needle is the feedback.

### 6.4 Pause (`Panel_Pause`)

The scene freezes. Dim α 0.55. Speed and distance stay visible under the dim. The pause button hides.

| Element | Anchor | pos | size | Content |
|---|---|---|---|---|
| Home | C | (−128, 0) | 176 × 176 | Secondary, `home` |
| Resume | C | (128, 0) | 176 × 176 | Primary, `play` (offset x +6) |

Primary is on the right, under the right thumb. On Resume, the run continues after a 0.3 s fade, with no countdown.

### 6.5 Game over (`Panel_GameOver`)

Shown **1.0 s real time** after the crash, after the slow-motion beat (`look-spec.md` §9.2). Dim α 0.60, fade 0.25 s. The HUD hides.

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

Retry puts the car back at **standstill** in under 1 s (GDD 2.6) and shows the HUD, as after Play. Home goes back to the Start panel.

Number formats (GDD 2.3):
- Top speed has one decimal (`297.4`).
- Distance has two decimals (`4.37`).
- No thousands separator.

## 7. Edge flash (`EdgeFlash`)

Four Images using the `edge` sprite: Left and Right are 220 wide and stretch vertically, Top and Bottom are 160 tall and stretch horizontally. Colour **`#FFB020` for a light hit, `#FFFFFF` for a crash**. Never red, because red is the player car. The alpha animation is in `look-spec.md` §9:
- Light hit: one side only.
- Crash: all four sides.

## 8. Fit check

Checked in `ui/mockup.html` with the canvas sizes from §1:
- **20:9** (canvas 2147 × 966):
  - Game over spans y +288 to −388, inside ±483.
  - A simulated 90-unit camera cutout on the left keeps the pause button clear (it sits in the safe area).
  - The gauge is scaled to 0.906 (290 units), x 1821–2111, y −24 to −314.
- **4:3** (canvas 1663 × 1247):
  - The distance box spans x 591–1071 and the gauge starts at x 1307, so they are 236 apart. The pause button ends at x 168.
  - The game-over columns span ±600, inside ±831.
- If the cutout is on the right (the phone is rotated the other way), the safe area moves the gauge left by the inset. The distance box still has room on every ratio above.
- **16:9:** reference, no issues.
