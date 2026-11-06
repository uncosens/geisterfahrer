# Look Spec — Unity checklist

A concrete checklist for the art pass. Everything is free (CC0, or CC BY with credit). It respects `docs/dev/unity-plan.md` §1 (Forward, HDR off, MSAA 2x, 1 shadow cascade 60 m) and §6 (budget). Where this spec proposes a change to that plan, it says **[change]**.

World axes as in the dev plan: the player car sits at z = 0 facing **+z**, and the world moves toward −z. The camera is behind the car, looking along +z.

Values marked *(tune)* are starting points; check them on the Galaxy Tab S7 and one phone.

---

## 1. Sun (Directional Light)

| Setting | Value |
|---|---|
| Rotation | **X 40, Y 30, Z 0** (sun 40° high, light comes from behind-left of the camera, shadows fall forward-right) |
| Colour | `#FFF1DE` (about 5500 K) |
| Intensity | **1.6** *(tune)* |
| Shadow type | Hard (dev plan) |
| Shadow strength | 0.85 |
| Depth bias / normal bias | 1.0 / 0.6 |
| Shadow distance, cascades, atlas | 60 m, 1, 1024 (dev plan) |

## 2. Sky, ambient, reflections

**Skybox HDRI (CC0):** [Dreifaltigkeitsberg](https://polyhaven.com/a/dreifaltigkeitsberg) by Poly Haven.
- Southern Germany (48.08° N, 8.76° E), clear midday, field and trees.
- Download HDR **2K**.
- Fallback: [Hausdorf Clear Sky](https://polyhaven.com/a/hausdorf_clear_sky) (clear, afternoon, fields and forest, 5400 K).

| Setting | Value |
|---|---|
| Texture import | Texture Shape 2D, Max Size 2048, Format **RGB9e5** on Android (ASTC HDR if the device supports it), mipmaps off |
| Skybox material | `Skybox/Panoramic`, Mapping Latitude-Longitude, Exposure **1.0**, Rotation: turn it until the sun in the HDRI sits behind-left of the camera, matching §1 *(tune)* |
| Lighting → Environment | Skybox material above; Sun Source = the Directional Light |
| Environment lighting | Source Skybox, Intensity Multiplier **1.0** |
| Environment reflections | Source Skybox, Resolution **128**, Compression on, Intensity 1.0 |
| Realtime GI / baked GI | off / off (no lightmaps in the MVP; the road is a moving loop) |

## 3. Fog (haze)

| Setting | Value |
|---|---|
| Mode | Linear |
| Colour | **`#C9D6E2`**. Then pick the HDRI's horizon colour in the Game view and match it within ±5 per channel, so the fog meets the sky without a seam. |
| Start | **300 m** (light haze only) |
| End | **1,600 m**. GDD 5.3 draws road and terrain to about 1.5 km, so the road fades out just past it. If the developer cuts the draw distance to 1 km for frame rate, use start 200 m and end 1,100 m. |
| Camera far clip | **1,650 m** (or 1,150 m with the 1 km cut) |
| Vehicles | do **not** use fog to appear. They fade in over 30 m at the spawn distance (GDD 5.3) with a dithered alpha (screen-door) on their material, so there is no transparent sorting. |
| Tunnels | fog off inside (GDD 5.4: no haze in tunnels); see §10.3 |

## 4. Post-processing (decided: on)

Decision (docs/decisions.md): turn on **one cheap global Volume**, with HDR still off (LDR grading, one LUT pass). The tunnel exposure (§10.3), the crash grey-out (§9.2) and the speed vignette (§11.3) all use it. If the Tab S7 drops under 60 fps, turn off the Vignette first; keep the Color Adjustments.

| Override | Setting |
|---|---|
| Tonemapping | None (HDR is off) |
| Color Adjustments | Post Exposure 0, Contrast **+8**, Saturation **−6** |
| White Balance | Temperature **+5**, Tint 0 |
| Vignette | Intensity **0.18**, Smoothness 0.40, Rounded off, Colour `#000000` |
| Bloom, SSAO, DOF, Motion Blur, Film Grain | off |
| URP asset → Grading Mode / LUT size | Low Dynamic Range / 32 |

## 5. Camera

Keep the dev plan values: **6.5 m behind, 2.7 m high**. The height never drops below 2.7 m, because the GDD crest sight rule (5.4) assumes a camera at about 2.75 m.

| Setting | Value |
|---|---|
| Look-at point | 14 m ahead of the car, 0.8 m above the road |
| FOV | set as **horizontal FOV** so every aspect ratio meets GDD 5.1 (at least 75°), see §11.1 |
| Roll | camera roll = **50 %** of the road bank angle (max 2°), so curves feel banked without tilting the horizon much |
| Hills | look-at point follows the road surface 14 m ahead (not a fixed height), so the camera pitches with crests and dips; pitch smoothing 0.3 s |
| x follow | critically damped, about 0.15 s |

## 6. Road

### 6.0 Cross-section (4 lanes, user decision)

Seen from the player (oncoming traffic drives toward the camera):

```
outer guardrail | edge strip 0.75 | lane 4 | lane 3 | lane 2 | lane 1 | edge strip 0.75 | median guardrail
                  0.75 m            3.75     3.75     3.75     3.75     0.75 m
```

- Rail to rail: **16.5 m** (4 × 3.75 + 2 × 0.75).
- Lane lines: 3 dashed lines (0.15 m).
- Edge lines: 2 solid lines (0.30 m), on the lane side of each edge strip.
- Lane 4 is the slow lane (trucks, buses), next to the outer rail. Lane 1 is the fast lane, next to the median.
- Keep the code parametric (`laneCount`, `laneWidth`, `edgeStrip`). The planner's GDD v0.4 is the source of truth if it differs.

### 6.1 Asphalt (generated, user decision)

The photo asphalt (Poly Haven Asphalt 01, 2.08 m tile) is **dropped**. Its fine grain and short repeat strained the user's eyes while driving. The road is now a calm generated surface (`RoadView.RoadTex`):

| Setting | Value |
|---|---|
| Shader | URP/Lit, Opaque, Metallic 0, Smoothness **0.25**, no normal map |
| Base colour | **`#55575C`** (dark asphalt grey) × the generated shade texture |
| Shade texture | 128 × 256, made at start-up, Trilinear, aniso 8. It spans the full road width once (no repeat across) and repeats every **60 m** along the road. |
| Tyre tracks | two soft darker bands per lane, ±0.8 m from the lane centre, Gaussian width 0.35 m, **−7 %** |
| Slow variation | two sine waves along the road (3 and 5 per 60 m), **±3.5 %** in total, periodic so the repeat has no seam |
| No | photo grain, high-frequency noise, cracks, patches or a normal map. Anything finer than about 1 m would flicker at speed. |

### 6.2 Lane marks

| Item | Size | Material |
|---|---|---|
| Lane lines (between lanes) | **0.15 m wide, 6 m dash, 12 m gap** (18 m period) | URP/Lit, Base `#E9E9E4`, Smoothness 0.35, Metallic 0 |
| Edge lines (both sides) | **0.30 m wide**, solid (width not verified, see `art-direction-3d.md` sources) | same |
| Height | 0.004 m above the asphalt (no z-fight; no depth offset needed) | |
| Wear | Base Map: 256 × 64 white texture with 10 % random dark speckle, tiled 1 per 1 m | |

**Dash phase:** place the dashes by **road distance `d`** (GDD 6.1 road coordinates): a dash covers `d mod 18 < 6`. Then they never jump at a segment seam, on straights or curves (decision in docs/decisions.md).

### 6.3 Guardrail (Schutzplanke)

Build it in code as an extruded W-profile along both rails (cheaper and more exact than a download).

| Part | Size | Note |
|---|---|---|
| W-beam | 0.31 m tall, 0.085 m deep, centre at **0.55 m** above the road, top at 0.75 m | Profile: two waves; 10-vertex cross-section |
| Posts | 0.10 × 0.05 × 0.75 m, every **2.0 m**, behind the beam | GPU instanced |
| Distance from edge line | at the outer edge of the edge strip (0.5–0.75 m) | |

Sizes are typical German A-profile values from memory, not verified.

Material:
- URP/Lit, Base `#A9AFB5`, Metallic **0.85**, Smoothness **0.55**.
- Vertex colour darkens the bottom 0.1 m ×0.7 (road grime).
- Normal map: none (a flat beam reads fine at speed).

### 6.4 Delineators (Leitpfosten)

| Item | Value |
|---|---|
| Spacing | every **50 m** on both sides, 0.5 m outside the rail line (GDD: one pair per segment) |
| Post | 1.0 m tall, trapezoid cross-section 0.12 × 0.10 m, `#F2F2F0`, Smoothness 0.3 |
| Black band | 0.25 m tall, 0.15 m below the top, `#1A1A1A` |
| Reflector | outer side (the traffic's right): one rectangle 0.04 × 0.18 m; median side: two discs Ø 0.06 m. Colour `#FFFFFF`, URP/Unlit (reads as lit up at all times) |

Reflector shapes follow the German convention as remembered; not verified.

### 6.5 Kilometre posts (GDD: every 500 m, number = run distance)

| Item | Value |
|---|---|
| Panel | 0.50 × 0.35 m, white `#F4F4F2`, black border 0.015 m, on a 1.2 m grey post `#8B9097` |
| Text | run km with one decimal (`4.5`), **Barlow Semi Condensed SemiBold**, cap height 0.15 m, `#111111`, TMP world-space (one shared SDF asset) |
| Place | outer side, 1.0 m outside the rail |
| Faces | **double-sided**: the same panel on both faces. The player drives against the traffic, so a one-sided post would only show its grey back, and the GDD wants the km number readable (decision in docs/decisions.md). |

**Signs in general are seen from behind** (GDD 3.2). Every one-sided sign (chevron boards, later the blue gantries) gets a designed back: plain galvanised grey `#8B9097`, Metallic 0.6, Smoothness 0.4, with a 0.03 m white `#F4F4F2` edge frame, so its shape reads against grass and sky.

### 6.6 Overpass, tunnel, viaduct, curves, hills

See §10.

## 7. Verge, fields, tree lines

| Layer | Distance from rail | Source / build | Settings |
|---|---|---|---|
| Grass verge and median | 0–15 m | [ambientCG Grass004](https://ambientcg.com/view?id=Grass004) (CC0), 1K, real tile 1.4 m | Tiling 1 per **3 m** (less repetition at speed), Base tint `#D8DCC8`, Smoothness 0.1 |
| Fields | 15–400 m, flat | Unlit-cheap: URP/Simple Lit, no texture, colour per strip | Strips 40–120 m long (z) and 30–80 m wide (x), random from `#6F8A3E` (green crop), `#C8B46A` (wheat), `#8A7556` (ploughed), `#7E9A4A` (meadow) |
| Tree lines | 60–250 m, two layers | Billboard quads 40 m wide × 14–18 m tall, alpha clip 0.5. Texture: render a 1024 × 256 silhouette strip in Blender from Poly Haven [Island Tree 02](https://polyhaven.com/a/island_tree_02) and [Fir Tree 01](https://polyhaven.com/a/fir_tree_01) (both CC0), dark side toward the camera | Colour multiply near layer `#3E5233`, far layer `#55684A` (atmosphere), cast shadows off |
| Horizon | at fog end | sky + fog only | |

The real trees from Poly Haven (0.3–17 M tris) are only a source for the billboard render. Never place them in the scene.

## 8. Vehicle materials

All vehicles use **one shared set of URP/Lit materials**. On import, map each model's original materials onto these by part. Textures go up to 1024² (dev plan §6).

| Material | Base colour | Metallic | Smoothness | Notes |
|---|---|---|---|---|
| `Paint_Player` (911) | **`#C8102E`** | 0.55 | 0.85 | red, close to Porsche Guards Red (approximation, not an official value; tune by eye against reference photos). The only red body on the road. |
| `Paint_White` | `#E9E9E7` | 0.10 | 0.75 | |
| `Paint_Silver` | `#B9BDC2` | 0.80 | 0.70 | |
| `Paint_Graphite` | `#3A3D42` | 0.60 | 0.78 | |
| `Paint_Black` | `#1C1D20` | 0.40 | 0.85 | |
| `Paint_Beige` | `#C8B79A` | 0.20 | 0.70 | |
| `Paint_Yellow` (vans) | `#E3B53A` | 0.10 | 0.70 | |
| `Paint_DarkBlue` | `#24324A` | 0.60 | 0.80 | |
| `Truck_Trailer` | `#EDEDEB` | 0.00 | 0.40 | flat panels |
| `Glass` | `#1A1F26` | 0.00 | 0.95 | opaque (no transparency, no overdraw) |
| `Tyre` | `#1B1B1B` | 0.00 | 0.25 | |
| `Trim_Chrome` | `#C9CCD0` | 1.00 | 0.85 | |
| `Trim_Plastic` | `#2A2A2A` | 0.00 | 0.35 | grilles, mirrors |
| `Light_Head` | `#FFF6E0` | 0 | 0.9 | Emission on, colour `#FFF6E0` ×1 (HDR off) |
| `Light_Tail` | `#8E1C1F` | 0 | 0.9 | Emission `#B3262B`; small and dark, so it never competes with the red player car |

Rules:
- **No red traffic paint, and no orange either.** Red is the player car only, so the player finds their car at a glance. Orange (`#E0601A`-like) would be read as red at speed. Blue and green traffic paint are fine now; the UI accent blue lives only in the UI.
- Traffic colour weights: white 20 %, dark blue 10 %, silver 25 %, graphite 15 %, black 15 %, beige 10 %, yellow 5 % (vans only).
- Bus: `Paint_White` body with a `Paint_Graphite` window band. Semi: cab from the paint list, trailer `Truck_Trailer`.
- Per model from `vehicle-assets.md`: merge all body meshes to one submesh for paint, one for glass, one for the rest (tyres, trim, lights in a small atlas). That gives at most 3 materials and 3 draw calls per vehicle.

---

## 9. Collision feedback (parameters)

Impulse `J` in t·m/s from GDD 2.5. **Light hit: J < 6.0** (run goes on). **Crash: J ≥ 6.0** (run ends). Let `k = J / 6` (0–1 for light hits).

### 9.1 Light hit (and scraping)

| Channel | Parameters |
|---|---|
| **Sparks** (Particle System at the contact point, world space) | Burst `12 + 4·J` (max 36). Lifetime 0.25–0.45 s. Start speed 6–14 m/s. Shape: cone 25° around the contact normal, tilted backward (−z) 30°. Inherit velocity 0 (the world already moves). Start size 0.03–0.06 m. Renderer: Stretched Billboard, Speed Scale 0.04, Length Scale 4. Gravity 1.0. Colour over lifetime `#FFF4D0` → `#FFB347` (at 40 %) → `#FF7A1A` α 0. Material: URP Particles/Unlit, Additive, 32 × 32 soft dot. Max particles 64. Collision off. |
| **Scraping** (contact lasts > 0.1 s, e.g. along the rail) | The same system emits **60 / s** while touching, plus scrape sound (below). Speed loss 15 km/h per second (GDD). |
| **Camera shake** | Perlin noise, position amplitude `0.02 + 0.012·J` m, rotation amplitude (pitch/yaw only, no roll) `0.2° + 0.15°·J`, frequency 25 Hz, duration **0.18 s**, ease-out quadratic. |
| **Edge flash** (`ui-spec.md` §7) | Hit side only, **amber `#FFB020`**, α `0.15 + 0.03·J` (max 0.33). In 0.06 s, out 0.20 s. Never red: red is the player car (`ui-spec.md` §3). |
| **Scratch decal** | URP Decal Projector, 0.6 × 0.3 m, on the hit side of the player car at the contact height, opacity 0.6. Texture: 256 × 256 white scratch lines on transparent. Pool of **6** on the player; the oldest is reused. Traffic gets one 0.4 × 0.2 m decal at opacity 0.5. |
| **Haptics** (Android `VibrationEffect.createOneShot`) | Duration `20 + 5·J` ms, amplitude `80 + 20·J` (max 255). |
| **Sound** | One-shot `metal_hit`: volume `0.5 + 0.08·J`, pitch random 0.9–1.1. Loop `metal_scrape` while touching: volume 0.6, pitch `0.9 + 0.4·speed/TopSpeed`, fade out 0.15 s. |
| **Speed read-out** | The number just drops (`−8 km/h × J`, GDD). No colour change. |

### 9.2 Crash (J ≥ 6)

| Channel | Parameters |
|---|---|
| **Slow motion** | `Time.timeScale = 0.25` for **0.8 s real time**, then 0 (frozen). `fixedDeltaTime` scaled the same way. Game over shows at **1.0 s** real time. |
| **Sparks** | One burst of 80, lifetime 0.4–0.8 s, start speed 8–18 m/s, sphere-ish cone 70°, same look as 9.1. |
| **Debris** | Mesh particles, 12 pieces: 8 glass shards (0.03–0.08 m, `#CFE3EC` α 0.8, Smoothness 0.95) and 4 plastic bits (0.05–0.12 m, `#2A2A2A`). Start speed 6–16 m/s, gravity 1.0, collision with a world plane at y 0, bounce 0.3, lifetime 1.2 s. |
| **Smoke puff** | 6 particles, size 1.5–3.0 m, `#8C8C8C` α 0.5 → 0, lifetime 1.5 s, speed 1 m/s up. Soft texture 64 × 64, alpha blended. **Off** by default (docs/decisions.md #22). |
| **Camera shake** | Position amplitude 0.25 m, rotation 2.5° (pitch/yaw), 18 Hz, exponential decay over 0.5 s (real time). |
| **FOV punch** | −5° in 0.08 s, back over 0.6 s (real time). |
| **Crash flash** | All four edges, **white `#FFFFFF`**, α 0.65, plus a full-screen white Image α 0.35. In 0.05 s, hold 0.10 s, out 0.5 s. |
| **Grey-out** | Over the same 0.8 s, Color Adjustments **Saturation 0 → −100** and Post Exposure 0 → −0.3. Without the post Volume, use a full-screen Image `#7A7A7A` α 0 → 0.45 with blend Multiply (UI shader) instead. The red car turns grey with the world, so nothing red stays on screen at the crash. Then the Dim fades to 0.60 over 0.25 s under the game over panel; saturation resets on Retry. |
| **Car** | Dent on the player: push vertices within **0.6 m** of the contact point inward by up to **0.12 m** (falloff smoothstep), once. Glass material swaps to `Glass_Cracked` (the same material plus a 512² crack texture). Physics spin is up to the developer (`art-direction-3d.md` §7 suggests letting it spin or stop). |
| **Haptics** | 180 ms, amplitude 255. |
| **Sound** | `crash_impact` volume 1.0; `glass_break` volume 0.7 at +0.05 s; engine loop fades out over 0.1 s; `tyre_skid` volume 0.6, fades out over 0.6 s. Pitch of all crash sounds ×0.8 during slow motion. |

### 9.3 Sound sources (free)

- **Freesound** with the licence filter **Creative Commons 0**. Search for `metal scrape`, `metal hit car`, `car crash`, `glass break`, `tyre skid`.
- Save the chosen file IDs in `CREDITS.md`, even for CC0.
- Format: Vorbis, mono, 44.1 kHz. Loops are imported with "Load in background" off and Decompress on Load (they are short).

---

## 10. Road features: curves, hills, tunnels, viaducts, overpasses

Geometry rules (radii, grades, lengths, how often they appear, fairness) are the planner's, in GDD v0.4. This section only says how each feature looks. All textures are CC0.

| Texture | Source | Real tile | Use |
|---|---|---|---|
| Concrete slab | [Poly Haven Concrete Slab Wall](https://polyhaven.com/a/concrete_slab_wall) | 2.3 m | overpass and viaduct decks, piers, tunnel portals |
| Painted concrete | [Poly Haven Concrete Wall 003](https://polyhaven.com/a/concrete_wall_003) | 3.0 m | tunnel walls (lower band) |
| Rough concrete | [Poly Haven Rough Concrete](https://polyhaven.com/a/rough_concrete) | 1.23 m | tunnel ceiling, bridge undersides |
| Rock face | [Poly Haven Rock Face 03](https://polyhaven.com/a/rock_face_03) | 2.7 m | rock cuts around tunnel portals and hill cuttings |
| Concrete (fallback) | [ambientCG Concrete034](https://ambientcg.com/view?id=Concrete034) | 1.1 m | any of the above |
| Grass, fields, trees | as §7 | | slopes, valley floor |

Download each at 1K (Diffuse, Normal GL, Roughness) and pack as in §6.1.

### 10.1 Curves

- **Bank:** the road surface tilts into the curve. The planner gives the bank (German motorways use up to a few percent). The lane marks, rails and delineators follow the same surface.
- **Rails and marks:** follow the spline exactly. Dashes keep the 6/12 m rhythm along the arc length.
- **Delineators:** keep 50 m spacing.
- **Chevron boards (GDD 3.2: curves with R < 1000 m):** on the outer side of the curve, 1.0 m outside the rail, **every 20 m** through the arc and the last 60 m of the entry clothoid.
  - Board 0.5 m wide × 0.75 m tall, bottom at 0.9 m on one 1.0 m grey post.
  - Front (toward the real traffic): black arrow chevron `#111111` on white `#F4F4F2`, 128² texture.
  - **Back (what the player sees):** the grey sign back with the white edge frame from §6.5. The row of tall pale rectangles is the cue: a line of boards = tight curve ahead.
  - Spot-check: at 1.5 km draw distance a 0.75 m board is about 2 px tall on a phone, so the row reads from about 400 m on, which is enough to start coasting (GDD 6.3: 210 m needed from the ceiling).
- **Inside of the curve:** no trees, walls or cuttings higher than 1 m within **15 m of the inner rail** in curves with R < 3000 m (GDD 5.4).
- **Camera:** roll 50 % of the bank (§5), no extra yaw lead.

### 10.2 Hills

- **Terrain:** a strip of terrain mesh 400 m wide on each side follows the road's height. Vertex colours use the field colours of §7. Cut slopes (road below the ground) use Rock Face 03 tinted `#B8B0A0` on the steep part and grass on the rest. Fill slopes (road above the ground) use grass.
- **Crest (road goes over a hill top):** the road disappears at the crest with sky behind it. Place 2–3 tree billboards and a far tree line beyond the crest, so the player sees there is more road. Oncoming vehicles fade in at the spawn distance as before.
- **Horizon:** three layers of distant hill silhouettes (flat meshes, no texture, unlit, fog on):

| Layer | Distance | Height | Colour |
|---|---|---|---|
| Near hills | 1.8–2.5 km | 60–120 m | `#6E8262` |
| Mid hills | 3–4 km | 100–200 m | `#8A9B88` |
| Far hills | 5–7 km | 150–300 m | `#A6B5B2` (close to the fog colour) |

  These layers sit beyond the road draw distance and the fog end (1.6 km), so give them their own material: **Fog off, colour already mixed toward `#C9D6E2`** as listed. They move with the camera in x and z (like a skybox), so they never get closer.

### 10.3 Tunnels

**Shape (per tube):** tunnels are straight (GDD 6.4). The walls stand **on the rail lines** (they replace the rails as the collision line), so the inside is 16.5 m wide: 4 lanes + edge strips. A 0.15 m painted kerb `#E4E6E2` runs at the wall base, and there is no walkway inside the collision line. The walls are vertical to 4.0 m, then curve into a flat-topped arch, with **4.8 m clear height** above the lanes.

| Surface | Material |
|---|---|
| Lower wall 0–3.5 m | Concrete Wall 003, tint `#E4E6E2` (light painted band, like real German tunnels), Smoothness 0.55 |
| Upper wall and ceiling | Rough Concrete, tint `#7E7F7B`, Smoothness 0.2 |
| Road | same asphalt as outside, vertex colour ×0.95 |
| Portal | a concrete frame (Concrete Slab, tint `#C9C8C2`) 1.5 m thick around the opening, set into a Rock Face 03 or grass slope |

**Lights (no real-time lights; additional lights stay off):**
- **Luminaires:** emissive boxes 1.2 × 0.15 × 0.1 m, colour `#FFF4E0` (about 4000 K LED), in two rows on the upper walls at 4.5 m height.
- **Spacing:** every **4 m in the entry and exit zones** (first and last **100 m**, GDD 6.4) and every **12 m in the interior**. This copies the real reinforced entry lighting.
- **Light pools:** under each luminaire, an additive quad on the road and the lower wall, `#FFF4E0` α 0.12, 3 m long. GPU instanced.
- **Inside lighting:** the main light (sun) must not light the inside. Either:
  - a trigger lerps the sun intensity 1.6 → 0, and the ambient from Skybox ×1.0 to a flat colour `#8A8578` ×0.9, over the first **25 m**; or
  - the tunnel shell casts shadows (60 m shadow distance covers the entry, but not the deep inside). Prefer the trigger.
- **Fog inside:** **off** (GDD 5.4: no haze in tunnels; sight = full tunnel length). Disable `RenderSettings.fog` while the camera is inside, and re-enable it 10 m after the exit portal.
- **Readability:** the ambient level (`#8A8578` ×0.9) plus the luminaire rows keep traffic paint readable; white and silver cars must stay clearly brighter than the walls' upper part `#7E7F7B`.

**Exposure (eye adaptation, Color Adjustments Post Exposure).** GDD 5.4 caps the adaptation at **0.5 s** at each portal, so vehicles stay readable:

| Moment | Post Exposure | Time |
|---|---|---|
| Outside, approaching | 0 | the opening looks dark, as in reality |
| Entering the portal | 0 → **−0.3** | 0.15 s |
| Adapted inside | −0.3 → **+0.3** | 0.35 s (total 0.5 s) |
| Approaching the exit | +0.3 | the exit opening looks overexposed |
| Leaving the portal | +0.3 → 0 | 0.5 s |

Without the post Volume, use the same curve on a full-screen Image: black α 0.2 for −0.3, white α 0.08 for +0.3.

**Exit glow:** with fog off inside, the outside seen through the exit is the real scene. Add an **unlit quad** in the exit opening anyway, colour `#FFFFFF` → `#DDE6EE` (top to bottom), alpha 0 → **0.5** as the exit gets closer than 300 m, so the opening glows. Turn it off once the car passes. Keep it at 0.5 or less, so oncoming vehicles in the opening stay visible.

**Entry from outside:** GDD 5.4 puts 200 m of straight road before each portal. The opening shows the shell's dark interior, with the dense entry luminaires as two dotted lines, and the traffic inside is lit by them. No extra trick is needed.

### 10.4 Viaducts (the player drives on a high bridge)

**Deck cross-section:** the road as §6.0, then on each side:
- **Concrete parapet with a rail = the collision line** (GDD 6.4), on the rail line.
  - A concrete wall 0.9 m tall and 0.4 m thick, Concrete Slab tint `#BDBCB6`.
  - The W-beam of §6.3 is mounted on its road face at 0.55 m.
  - From the 2.7 m camera height the player still looks over it into the valley.
- **Concrete cap (Kappe):** 1.5 m wide behind the parapet, raised 0.15 m, same tint. It is a service walkway, not reachable.
- **Outer railing:** 1.1 m tall, at the outer edge of the cap.
  - Steel posts 0.08 m wide every 2.0 m, plus a top rail 0.08 m and two mid tubes, `#7D858C`, Metallic 0.8, Smoothness 0.5.
  - The infill bars are one **alpha-clip quad** strip with a 256 × 128 bar texture (vertical bars every 0.12 m), so it costs 2 triangles per 2 m.
- **Deck edge:** a 0.6 m fascia below the cap, Concrete Slab tint `#A9A8A2`, with a 0.1 m darker drip line `#6E6D69` (water stain).

**What is below and around:**
- **Height:** 20–80 m above the valley floor (GDD 6.4). The deck sits over a sag, so the approach dips into the valley and climbs out.
- **Valley floor:** the terrain strip of §10.2 at the low height, with:
  - fields as in §7;
  - a river 15–30 m wide, colour `#4F6E78`, Smoothness 0.9, a slow scrolling normal (0.02 m/s);
  - a few low-poly house clusters, white walls `#EDEAE2` and roofs `#8C3B2B` / `#5A5A5A`. Roof red is far away and small, so it does not compete with the car.
- **Piers:** concrete columns 3 × 6 m (along × across the road), Concrete Slab tint `#B5B4AE`, every **60 m**, from the deck down to the ground. Two per pier line, under each carriageway.
- **Bridge shadow on the valley:** a baked dark quad on the terrain, `#000000` α 0.30, the deck's shape projected along the sun direction of §1. Not a real-time shadow (it is far beyond the 60 m shadow distance).
- **No roadside props on the deck** (GDD 6.4): no windsock, no signs, no delineators (the parapet replaces them). Crosswind gusts are a later idea.
- **Camera:** the railings sit low in the frame, so the player sees past them into the valley on both sides. This is the visual reward of the viaduct. Keep the rails thin.
- **Fog over the valley:** the same fog. The far valley sides fade into the hill layers of §10.2.

### 10.5 Overpasses (bridges crossing above the road)

Build them from boxes. The parts are simple, and a download would need more cleanup than building.

| Part | Size | Material |
|---|---|---|
| Deck | **8–15 m** along our road (GDD 6.4; random per overpass), spans the full width plus 5 m past each rail; 1.2 m thick; underside **4.7 m** above our road (clearance, GDD 6.4) | Concrete Slab tint `#C9C8C2`; underside Rough Concrete `#9A9994` |
| Parapet on top | 1.0 m concrete wall each side, 0.3 m thick, with a 1.8 m mesh fence above it on half the overpasses (alpha-clip mesh texture) | Concrete Slab; fence `#6F7780` |
| Middle pier | in the median **behind the median rail** (no collision, GDD 6.4): one wall pier 1.0 m thick × 8 m long | Concrete Slab `#B5B4AE` |
| Abutments | sloped concrete walls behind the outer rails, 35° slope, with Rock Face 03 or grass on the embankment | Concrete Slab |
| Traffic on top | optional: 1–2 cars from the traffic pool crossing at 50–70 km/h at right angles, every few overpasses. LOD1 meshes, no shadows. | |

**Variants (to avoid repetition):** two deck colours (`#C9C8C2`, `#B1B0AA`), with or without a fence, and one variant with a steel-blue railing `#4C6A86`.

**Shadow on our road:** GDD 5.4 wants it **light**, so vehicles stay readable.
- The deck and parapets have **Cast Shadows: Off**. The real-time sun shadow (strength 0.85) would be too dark.
- Instead, a baked shadow quad lies on the road: `#000000` α **0.20**, the deck's footprint pushed 4 m forward-right (sun direction of §1). It is part of the overpass prefab, so there is no hand-over at 60 m.
- **Light under the deck:** none extra.

### 10.6 Budget for the new features

- Tunnel segment (50 m): about 1.5k tris shell + about 40 luminaires (instanced) + light pools.
- Viaduct segment (50 m): about 2k tris (cap, railing quads, fascia) + piers + the valley strip.
- Overpass: about 1k tris.
- Hill layers: about 600 tris total.
- All of it stays inside the dev plan's draw-call budget if each feature uses at most **3 materials** and repeated parts are GPU instanced.

## 11. Driving feel (speed and realism)

Target: 60 fps on the Galaxy Tab S7 and a stable 60 (min 45) on a mid phone.
- **GPU cost:** low = under 0.2 ms, med = 0.2–1 ms, high = over 1 ms on the Tab S7. These are estimates; measure with the Unity Profiler.
- **On?** is the recommended default.
- `s = speed / TopSpeed` (0–1, TopSpeed 306.2 km/h). `v` = speed in km/h.

### 11.1 Camera

| Item | Parameters | GPU | On? |
|---|---|---|---|
| **Speed FOV** | Set as **horizontal** FOV so every ratio meets GDD 5.1 (≥ 75°): `hFOV = 80 + 12 · s^1.5` degrees (80° standing, 82° at 100 km/h, 92° at 306). Unity's vertical FOV = `max(2·atan(tan(hFOV/2) / aspect), 50 + 6 · s^1.5)`. That gives 50.5° on 16:9, 50° on 20:9 (so horizontal 92° there, as in GDD 5.1) and 64° on 4:3 when standing. Smoothed with a 0.5 s low-pass. | low | **on** |
| **Throttle kick** | +1.5° horizontal FOV while the throttle is held and the acceleration is over 1 m/s². In 0.4 s, out 0.6 s. | low | **on** |
| **Camera distance by speed** | Behind 6.5 → **7.2 m** as `s` goes 0 → 1, smoothed 0.8 s. **Height stays 2.7 m** (GDD crest sight rule). The car shrinks a little and the road stretches. | low | **on** |
| **Steering lag** | Camera x follows the car with a critically damped spring, ω **6 rad/s** (about 0.3 s). Camera yaw follows the car's yaw with ω 8 rad/s. The car drifts up to **0.35 m** off-centre in the frame during a lane change. | low | **on** |
| **Camera roll on steering** | roll = `−0.25° × sideSpeed (m/s)`, max **1.8°**, smoothed 0.2 s. Added to the curve-bank roll of §5. | low | **on** |
| **Speed shake** | Perlin noise. Position amplitude `0.004 m × (v/100)²`, max 0.03 m. Rotation (pitch/yaw) `0.05° × (v/100)²`, max 0.35°. Frequency `12 + v/10` Hz. Below 60 km/h it is zero. | low | **on** |
| **Surface multiplier on shake** | asphalt 1.0, tunnel 0.8, viaduct deck 1.2, plus a one-off bump at each viaduct expansion joint (0.02 m, 0.08 s; ends of each viaduct). | low | **on** |
| **Edge-line rumble** | When a tyre is on the 0.30 m edge line: shake 30 Hz, 0.006 m, plus haptic pulses of 10 ms every 60 ms, plus the rumble sound (§11.4). | low | **on** |
| **Hit and crash shake** | as §9 (adds on top) | low | on |

### 11.2 Car body and wheels (visual only, on the body mesh, not the physics)

| Item | Parameters | GPU | On? |
|---|---|---|---|
| **Pitch on acceleration** | Nose up `0.12° per m/s²` of acceleration, max 1.0°. Nose down when coasting, `0.08° per m/s²` of deceleration. Spring ω 7 rad/s, damping ratio 0.7. | low | **on** |
| **Body roll** | `0.35° per m/s` of side speed, max **2.2°**, leaning out of the turn. Spring ω 9 rad/s, damping ratio 0.6 (one small overshoot). A light hit adds a 1.5° × k roll kick. | low | **on** |
| **Wheel spin** | Angular speed = `speed / 0.345 m` (911 rear tyre radius). Above about 60 km/h the spokes alias, so cross-fade the rim to a **radially blurred rim material** between **40 and 80 km/h**. The blurred texture is 256², made once in Blender from the model's rim. | low | **on** |
| **Steering visible** | Front wheel yaw = `(sideSpeed / maxSideSpeed) × 12° × clamp(60 / v, 0.2, 1)`: up to 12° at low speed, 2.4° at 300 km/h. Smoothed 0.1 s. | low | **on** |
| **Suspension bob** | Body y offset by speed shake × 0.5 (body moves less than the camera). | low | on |

### 11.3 Speed effects

| Item | Parameters | GPU | On? |
|---|---|---|---|
| **Speed streaks (cheap motion blur)** | A particle system parented to the camera. 40 max particles, spawned on a cone (radius 6–14 m, 25–60 m ahead), moving toward the camera at the car speed. Stretched Billboard, length scale 0.06 × speed (m/s). Colour `#FFFFFF`, alpha `0.18 × clamp((v − 160) / 146, 0, 1)` (invisible below 160 km/h, full at 306). Additive, 16 × 64 soft streak texture. Kept out of the centre 30 % of the screen (cone inner radius) so the road ahead stays clear. | low–med (overdraw only) | **on** |
| **Vignette by speed** | Needs the post Volume (§4): intensity 0.18 → **0.30** as `s` goes 0.5 → 1. | low (already in the post pass) | **on** if §4 is on |
| **Radial blur (post)** | Full-screen radial blur from the vanishing point, needs the opaque texture plus a custom pass. | high | off |
| **URP Motion Blur** | Camera motion blur. | high | off |
| **Chromatic aberration at speed** | Post, 0 → 0.15. | med | off |
| **Asphalt anisotropic filtering** | Aniso level **4** on asphalt and lane marks, so the road keeps its grain at speed instead of smearing. | low–med | **on** |
| **Heat haze** | Distortion over the road in the distance; needs the opaque texture. | high | off |
| **Lens dirt** | A UI-layer texture 1024 × 512, additive, alpha `0.06 × sunInView` (dot of the camera forward and the sun direction). | low | off (it clutters the HUD; revisit after the art pass) |
| **Tunnel exposure** | as §10.3 | low | on |

### 11.4 Sound layers by speed

One Audio Mixer with groups **Engine, Road, Wind, SFX** and a **Reverb** send. All loops are mono Vorbis. The sources are Freesound CC0 (§9.3). Volumes are linear 0–1 on the AudioSource.

| Layer | Volume | Pitch / filter | CPU | On? |
|---|---|---|---|---|
| **Engine** (3 loops: low / mid / high rpm, cross-faded by rpm) | Throttle on 0.9, off 0.6 (fade 0.15 s) | Sound-only gears (no gameplay effect): 1st to 70, 2nd to 110, 3rd to 150, 4th to 195, 5th to 240, 6th to 285, 7th above, all in km/h. rpm runs 2,500 → 7,500 inside each gear and drops to about 5,000 on a shift (0.12 s volume dip to 0.5). Loop crossfade: low < 3,500 < mid < 5,500 < high. Pitch = `rpm / loopBaseRpm`. | low | **on** |
| **Tyre roar** | `0.6 × clamp((v − 30) / 250, 0, 1)^1.2` | Pitch `0.8 + 0.4·s`. Viaduct deck: volume ×1.2, pitch ×1.05. | low | **on** |
| **Wind** (pink-noise loop) | `0.8 × s²` | Low-pass cutoff `800 + 4,000·s` Hz; pitch `0.9 + 0.3·s` | low | **on** |
| **Passing whoosh** | One-shot when a vehicle passes within 6 m sideways. Volume `clamp(closingSpeed / 400 km/h, 0.2, 1)`. Doppler level 1 (GDD). Trucks and buses use a deeper sample. | random pitch 0.95–1.05 | low | **on** |
| **Edge-line rumble** | 0.7 while on the line | loop, pitch `0.8 + 0.4·s` | low | **on** |
| **Expansion joint** | One-shot "da-dum", 0.6, at each viaduct end | — | low | on |
| **Tunnel echo** | Reverb send 0 → **−3 dB** over 0.4 s on entry (SFX Reverb: room −600 mB, decay 1.6 s, reflections −200 mB). Inside: wind ×0.5, tyre ×1.3, engine ×1.3. Reverse on exit over 0.6 s. Or an Audio Reverb Zone preset *StoneCorridor* on the tunnel segments. | — | low | **on** |

### 11.5 Recommended set and budget

**On:**
- speed FOV, throttle kick, distance by speed, steering lag, roll, speed shake with surfaces and rumble;
- body pitch and roll, wheel spin with blur swap, visible steering;
- speed streaks, asphalt aniso 4×, tunnel exposure;
- all sound layers.

Estimated extra cost on the Tab S7: **about 0.5 ms GPU** (streak overdraw and aniso) and **under 0.3 ms CPU** (springs, audio).

**Off:** radial blur, URP motion blur, chromatic aberration, heat haze, lens dirt. Turn them on one at a time only if the profiler shows over 3 ms of spare GPU.

## Sources

- Poly Haven HDRIs [Dreifaltigkeitsberg](https://polyhaven.com/a/dreifaltigkeitsberg) and [Hausdorf Clear Sky](https://polyhaven.com/a/hausdorf_clear_sky)
- Poly Haven textures [Asphalt 01](https://polyhaven.com/a/asphalt_01) and [Asphalt 06](https://polyhaven.com/a/asphalt_06): no longer used (road is generated, §6.1)
- Poly Haven models [Island Tree 02](https://polyhaven.com/a/island_tree_02) and [Fir Tree 01](https://polyhaven.com/a/fir_tree_01)
- ambientCG [Grass004](https://ambientcg.com/view?id=Grass004) and [Concrete034](https://ambientcg.com/view?id=Concrete034)
- Poly Haven textures [Concrete Slab Wall](https://polyhaven.com/a/concrete_slab_wall), [Concrete Wall 003](https://polyhaven.com/a/concrete_wall_003), [Rough Concrete](https://polyhaven.com/a/rough_concrete) and [Rock Face 03](https://polyhaven.com/a/rock_face_03)
- All of the above are CC0. IDs, sizes and locations were checked with the Poly Haven and ambientCG APIs on 2026-11-05 and 2026-11-06.
- Tunnel heights, entry-lighting zones, viaduct cap and railing sizes, and overpass clearance are typical values from memory, not verified. Check them against GDD v0.4 when it lands.
- Lane line 6 m / 12 m / 0.15 m: see `art-direction-3d.md` sources.
- Guardrail, delineator and km-post sizes: typical values from memory, not verified.
