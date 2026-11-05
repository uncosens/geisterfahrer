# Art Direction — Candidates

> **Decision (2026-11-05):** the user chose a **realistic** version of b (3D chase camera, Unity 6.3 URP, Android). See `art-direction-3d.md`. This file and `preview.html` stay as history.

Status: proposal. The user picks one (a, b or c). Mockups: `preview.html` (open in a browser).

Shared rules for all three:

- The player drives against the traffic. Oncoming cars come toward the player.
- Fixed meaning colors, same in every candidate:
  - Player car: rocket blue `#3D7BFF` (house accent).
  - Crash / danger: red `#FF4D4F`.
  - Near miss / bonus: green `#2FD27A`.
- HUD has only three things: score, pause icon, game over screen. No hint lines, no tutorial text, no dialog titles.
- Game over: big score, best score with a small trophy icon, two equal square icon buttons: retry `↻` (filled, primary) and home `⌂` (outline). Tapping outside does nothing (no accidental exit).
- Pause: icon button top-right, 44 dp touch target. Pause screen reuses the game over layout with resume `▶` in place of retry.
- HUD respects safe areas (notch, rounded corners, gesture bar). Score sits below the top inset.
- Number font uses tabular figures so the score does not jitter while counting.
- Fonts below are all SIL Open Font License, bundled in the app (offline).

---

## a — Flat Minimal, top-down  (recommended)

**Camera.** Straight top-down, 2D. The road scrolls down the screen; the player moves left/right between lanes at the bottom third. Portrait.

**Palette.**

| Role | Hex |
|---|---|
| Verge / background | `#24262A` |
| Road | `#313338` |
| Lane marks, edge lines | `#E8E6E1` |
| Traffic car (light) | `#E8E6E1` |
| Traffic car (yellow) | `#F2C94C` |
| Traffic car (grey) | `#9AA0A6` |
| Headlights | `#FFF8E1` |
| HUD text | `#F2F2F2` |
| Player / danger / near miss | `#3D7BFF` / `#FF4D4F` / `#2FD27A` |

Dark only. Low contrast road, high contrast cars, so the cars are what the eye reads.

**Car / road / traffic.** Cars are rounded rectangles with a dark windshield and two headlight dots. No outlines, no shading, no textures. Road: 4 lanes, dashed lane lines, solid edge lines. Traffic types differ by size only (car, van, truck), not detail.

**HUD.** Score top-center, large and bold. Pause top-right. Game over: screen dims to 85 %, thin red edge glow, score in the middle, best below, retry + home buttons in one row.

**Font.** Inter (Bold for score, tabular figures).

**Scaling.** The world is defined in lanes. Fit the road width (4 lanes + small verge) to the short side, capped so the road never takes more than ~520 dp on tablets; extra width shows more verge. Taller screens show more road ahead (more reaction time, so cap the visible length to keep difficulty fair: e.g. visible length = 2.2 × road width, letterbox with verge color beyond that). Landscape tablets: same road centered, verges wide.

**Art cost: low.** All shapes can be drawn in code (rects, rounded rects). No image assets needed. One person can finish the full set in a day. Easiest to keep pixel-sharp at any resolution.

**Risk.** Can look plain. Polish must come from motion: smooth lane changes, screen shake on crash, a short green flash on near misses, speed lines.

---

## b — Low-poly 3D, chase camera

**Camera.** 3D, behind and above the player car (chase cam). Oncoming cars grow from the horizon. Portrait or landscape (portrait suggested for one-hand play).

**Palette (dusk).**

| Role | Hex |
|---|---|
| Sky top → horizon | `#2B2A5A` → `#F28C5B` |
| Far hills | `#4B3B6E`, `#5E4A80` |
| Ground facets | `#2E3A44`, `#34424C` |
| Road | `#3B3D4A` |
| Lane marks | `#E9E4D8` |
| Traffic cars | `#E9E4D8`, `#F2B33D`, `#7FB2A8` |
| Headlights / taillights | `#FFE8A3` / `#FF4D4F` |
| HUD text | `#FFFFFF` on 30 % dark scrim |

**Car / road / traffic.** Flat-shaded low-poly models (100–400 tris each), no textures, one color per face. Headlights as small emissive quads with a soft glow sprite. Road is a long strip with lane marks as a texture or decals; scenery = faceted hills and a few low-poly trees/posts.

**HUD.** Score top-center, condensed italic for speed feel. Pause top-right. Game over: frosted dark panel over the frozen 3D scene, same buttons as shared rules.

**Font.** Barlow Condensed (SemiBold Italic for score).

**Scaling.** Keep vertical FOV fixed; wider screens see more side scenery, taller screens get more sky/road. HUD anchored to corners and center. Need a min horizontal FOV so on very tall phones the outer lanes stay visible (switch to horizontal-FOV lock below ~9:20).

**Art cost: high.** Needs a 3D engine (Unity/Godot), 4–6 car models, road pieces, scenery, lighting tuning. Several days to a few weeks of art work, plus GPU cost on low-end phones (battery, heat).

**Risk.** Most "real game" look, but the slowest and heaviest. Low-poly assets from stores can make it look generic.

---

## c — Neon Retro (synthwave), pseudo-3D

**Camera.** Pseudo-3D like old arcade racers: horizon at ~38 % of the height, road and grid converge to a vanishing point. Player car seen from behind. Can be done in 2D (scaled sprites/vector lines) without a 3D engine.

**Palette.**

| Role | Hex |
|---|---|
| Background / night | `#0B0420` |
| Ground | `#12062E` |
| Grid / oncoming cars | `#FF2BD6` (magenta) |
| Road edges | `#20E3FF` (cyan) |
| Mountains | `#7B2CBF` |
| Sun gradient | `#FFD166` → `#FF2BD6` |
| Headlights | `#FFFFFF` |
| Player / danger / near miss | `#3D7BFF` outline with cyan glow / `#FF4D4F` / `#2FD27A` |

Note: magenta and red are close. Danger must show as a full-screen red flash + shake, not just color, so it stays clear.

**Car / road / traffic.** Cars as glowing outlines (vector lines with a blur glow), dark fill. Road = cyan edge lines and a moving magenta grid. Sun with horizontal stripes on the horizon.

**HUD.** Score top-center in a monospace/techno font with glow. Pause top-right as a neon outline icon. Game over: big magenta score with glow, buttons as neon outlines.

**Font.** Orbitron (Bold) for score, or Share Tech Mono.

**Scaling.** Horizon height fixed as a percent of screen height; grid and road extend sideways on wide screens. Road width at the bottom fits ~90 % of the short side, capped on tablets.

**Art cost: medium.** No textures, but the glow needs a shader or blur pass (or pre-baked glow sprites), and the pseudo-3D projection needs some math. 2–4 days of art/tech art.

**Risk.** Strong style but a well-known trend; glow is costly on old phones and can blur small HUD text. Less "minimal" than a.

---

## Comparison

| | a Flat Minimal | b Low-poly 3D | c Neon Retro |
|---|---|---|---|
| Camera | Top-down 2D | Chase cam 3D | Pseudo-3D |
| Engine need | Any 2D / Compose Canvas | 3D engine | 2D + glow |
| Art cost | Low | High | Medium |
| Phone performance | Best | Worst | Middle |
| Fits "minimal, refined" | Best | Middle | Weak |
| Reads danger fast | Best (whole road visible) | Middle (depth) | Middle (depth) |

**Designer's pick: a.** It matches the user's minimal taste, has the lowest art cost, runs well on every phone, and a top-down view shows all lanes at once, which suits a wrong-way dodge game. If the team wants more "wow", c is the next choice; its palette can be added later as an unlockable theme on top of a's top-down layout.
