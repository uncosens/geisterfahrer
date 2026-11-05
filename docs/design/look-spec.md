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
| Start | `0.45 × spawnDistance` (180 m at 400 m) |
| End | `spawnDistance` (400–470 m, GDD 5.2), updated every frame with the spawner |
| Camera far clip | `spawnDistance + 30 m` |

## 4. Post-processing [change]

The dev plan has post off in the MVP. Proposal: turn on **one cheap global Volume**, with HDR still off (LDR grading, one LUT pass). Measure it on the Tab S7 and keep it only if the frame time stays under 16.6 ms.

| Override | Setting |
|---|---|
| Tonemapping | None (HDR is off) |
| Color Adjustments | Post Exposure 0, Contrast **+8**, Saturation **−6** |
| White Balance | Temperature **+5**, Tint 0 |
| Vignette | Intensity **0.18**, Smoothness 0.40, Rounded off, Colour `#000000` |
| Bloom, SSAO, DOF, Motion Blur, Film Grain | off |
| URP asset → Grading Mode / LUT size | Low Dynamic Range / 32 |

## 5. Camera

Keep the dev plan values: **6.5 m behind, 2.7 m high**, and FOV widens with `speed / TopSpeed`.

| Setting | Value |
|---|---|
| Look-at point | 14 m ahead of the car, 0.8 m above the road |
| FOV | vertical **58°** at 100 km/h → **66°** at 306 km/h (linear on `speed / TopSpeed`); this keeps all 3 lanes and both rails in view on 4:3 |
| x follow | critically damped, about 0.15 s |

## 6. Road

### 6.1 Asphalt (CC0)

[Asphalt 01](https://polyhaven.com/a/asphalt_01) by Poly Haven: a worn road surface, real size **2.08 m**. Download at 1K: Diffuse, Normal (GL), Roughness.

Fallback: [Asphalt 06](https://polyhaven.com/a/asphalt_06) (3.0 m tile, browner).

| Setting | Value |
|---|---|
| Shader | URP/Lit, Opaque |
| Base Map | diffuse 1024², tint `#FFFFFF` |
| Metallic map | packed: R 0, A = `1 − roughness`; Metallic 0, Smoothness source = Metallic Alpha, Smoothness ×**0.6** |
| Normal map | strength **0.6** |
| Tiling | world-space UVs, **1 tile per 2.08 m** in both x and z (UV = position / 2.08) |
| Tyre tracks | second Base Map tint by vertex colour: lane centre ±0.9 m multiplied by **0.88** (darker), 0.6 m wide bands |
| Edge strip (0.5–0.75 m) | same material, vertex colour ×**1.06** (lighter, less worn) |
| Compression | ASTC 6×6 (colour), ASTC 4×4 (normal) |

### 6.2 Lane marks

| Item | Size | Material |
|---|---|---|
| Lane lines (between lanes) | **0.15 m wide, 6 m dash, 12 m gap** (18 m period) | URP/Lit, Base `#E9E9E4`, Smoothness 0.35, Metallic 0 |
| Edge lines (both sides) | **0.30 m wide**, solid (width not verified, see `art-direction-3d.md` sources) | same |
| Height | 0.004 m above the asphalt (no z-fight; no depth offset needed) | |
| Wear | Base Map: 256 × 64 white texture with 10 % random dark speckle, tiled 1 per 1 m | |

⚠️ **Dash phase across segments:** the dev plan uses 50 m road segments, but the dash period is 18 m. 50 is not a multiple of 18, so the dashes would jump at every seam. Either use **54 m segments** (3 periods; 10 segments = 540 m, still more than the 470 m spawn distance), or place the dashes by absolute run distance instead of per segment. → developer to choose.

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

### 6.6 Overpass (optional, later)

- Deck: concrete [ambientCG Concrete034](https://ambientcg.com/view?id=Concrete034) (CC0), 1 tile per 1.1 m, Base tint `#D0D0CC`.
- Deck 12 m deep, its underside 5.5 m above the road.
- Under it, a baked shadow quad on the road: `#000000` α 0.35, extended 4 m past the deck on the side away from the sun.

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
| `Paint_Player` (911) | `#3D7BFF` | 0.70 | 0.82 | house blue; the only blue on the road |
| `Paint_White` | `#E9E9E7` | 0.10 | 0.75 | |
| `Paint_Silver` | `#B9BDC2` | 0.80 | 0.70 | |
| `Paint_Graphite` | `#3A3D42` | 0.60 | 0.78 | |
| `Paint_Black` | `#1C1D20` | 0.40 | 0.85 | |
| `Paint_Beige` | `#C8B79A` | 0.20 | 0.70 | |
| `Paint_Yellow` (vans) | `#E3B53A` | 0.10 | 0.70 | |
| `Truck_Trailer` | `#EDEDEB` | 0.00 | 0.40 | flat panels |
| `Glass` | `#1A1F26` | 0.00 | 0.95 | opaque (no transparency, no overdraw) |
| `Tyre` | `#1B1B1B` | 0.00 | 0.25 | |
| `Trim_Chrome` | `#C9CCD0` | 1.00 | 0.85 | |
| `Trim_Plastic` | `#2A2A2A` | 0.00 | 0.35 | grilles, mirrors |
| `Light_Head` | `#FFF6E0` | 0 | 0.9 | Emission on, colour `#FFF6E0` ×1 (HDR off) |
| `Light_Tail` | `#8E1C1F` | 0 | 0.9 | Emission `#B3262B`; muted so it never reads as the danger red `#FF4D4F` |

Rules:
- **No red, green or blue traffic paint.** Those colours are reserved for meaning.
- Traffic colour weights: white 30 %, silver 25 %, graphite 15 %, black 15 %, beige 10 %, yellow 5 % (vans only).
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
| **Edge flash** (`ui-spec.md` §7) | Hit side only, `#FF4D4F`, α `0.15 + 0.03·J` (max 0.33). In 0.06 s, out 0.20 s. |
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
| **Smoke puff** | 6 particles, size 1.5–3.0 m, `#8C8C8C` α 0.5 → 0, lifetime 1.5 s, speed 1 m/s up. Soft texture 64 × 64, alpha blended. On the Tab S7 only if frame time allows. |
| **Camera shake** | Position amplitude 0.25 m, rotation 2.5° (pitch/yaw), 18 Hz, exponential decay over 0.5 s (real time). |
| **FOV punch** | −5° in 0.08 s, back over 0.6 s (real time). |
| **Edge flash** | All four edges, `#FF4D4F`, α 0.55. In 0.05 s, hold 0.15 s, out 0.6 s, then the Dim fades to 0.60 over 0.25 s under the game over panel. |
| **Car** | Dent on the player: push vertices within **0.6 m** of the contact point inward by up to **0.12 m** (falloff smoothstep), once. Glass material swaps to `Glass_Cracked` (the same material plus a 512² crack texture). Physics spin is up to the developer (`art-direction-3d.md` §7 suggests letting it spin or stop). |
| **Haptics** | 180 ms, amplitude 255. |
| **Sound** | `crash_impact` volume 1.0; `glass_break` volume 0.7 at +0.05 s; engine loop fades out over 0.1 s; `tyre_skid` volume 0.6, fades out over 0.6 s. Pitch of all crash sounds ×0.8 during slow motion. |

### 9.3 Sound sources (free)

- **Freesound** with the licence filter **Creative Commons 0**. Search for `metal scrape`, `metal hit car`, `car crash`, `glass break`, `tyre skid`.
- Save the chosen file IDs in `CREDITS.md`, even for CC0.
- Format: Vorbis, mono, 44.1 kHz. Loops are imported with "Load in background" off and Decompress on Load (they are short).

---

## Sources

- Poly Haven HDRIs [Dreifaltigkeitsberg](https://polyhaven.com/a/dreifaltigkeitsberg) and [Hausdorf Clear Sky](https://polyhaven.com/a/hausdorf_clear_sky)
- Poly Haven textures [Asphalt 01](https://polyhaven.com/a/asphalt_01) and [Asphalt 06](https://polyhaven.com/a/asphalt_06)
- Poly Haven models [Island Tree 02](https://polyhaven.com/a/island_tree_02) and [Fir Tree 01](https://polyhaven.com/a/fir_tree_01)
- ambientCG [Grass004](https://ambientcg.com/view?id=Grass004) and [Concrete034](https://ambientcg.com/view?id=Concrete034)
- All of the above are CC0. IDs, sizes and locations were checked with the Poly Haven and ambientCG APIs on 2026-11-05.
- Lane line 6 m / 12 m / 0.15 m: see `art-direction-3d.md` sources.
- Guardrail, delineator and km-post sizes: typical values from memory, not verified.
