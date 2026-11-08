# Urban kit: look spec

Buildings, street furniture and structures along the Autobahn, so that speed is felt. Everything is generated in code: no downloads, no licences. Companion: [urban-preview.html](urban-preview.html) (six mock frames from the chase view).

**Source of truth.** Layout rules (zones, setbacks, interchanges, fairness F1–F9, speed feel, performance caps C1–C10) are in GDD 6.6–6.10. This file only adds the *look*: shapes, colours, textures, how to build them cheaply. Where a number here repeats a GDD number it is copied; if they differ, the GDD wins. Sun, sky and fog are exactly `look-spec.md` §1–3 (sun X 40 / Y 30, `#FFF1DE`, fog `#C9D6E2` linear 300–1600 m). Daytime only.

**Frame of reference.** Lateral distances are measured from the nearest rail. The outer side (buildings, noise walls) is the player's **left**; the median and the other carriageway are on the right (look-spec §6.0).

**Three rules that everything below follows**
1. Red and orange never appear in scenery (no hue 340°–35° with saturation above 12 %). The only red object is the player car.
2. Facades are mid to dark and calm (GDD F8): every colour channel at most `#BE`, no flat white or light grey surface wider than 30 m.
3. No realtime shadows, no transparency except tree billboards and bridge glass (GDD C6). Daylight depth comes from vertex-colour shading and from the facade texture itself.

---

## 1. Building kit

### 1.1 Types

Floor pitch is **3.2 m** (GDD 6.9). Height = floors × 3.2 m. A mixed-use block gets a 4.5 m ground floor in place of the first 3.2 m.

| Type | Along road | Deep | Floors / height | Roof | Used in |
|---|---|---|---|---|---|
| Residential block (Zeilenbau) | 25–60 m | 12–16 m | 5–9 (16–29 m) | flat + parapet | city, suburb (4–5 floors) |
| Residential tower | 22–30 m | 22–30 m | 12–22 (38–70 m) | flat + plant box | city only |
| Office tower | 30–50 m | 25–40 m | 10–25 (32–80 m) near; 25–35 (80–112 m) beyond 300 m only | flat + plant box + mast | city |
| Mixed-use block | 25–45 m | 15–25 m | 5–9 (16–29 m) | flat | city, suburb centre |
| Terraced / detached house | 10–20 m | 8–12 m | 2 (6.4 m eaves) + roof | gable, ridge along the road | suburb |
| Industrial shed | 40–120 m | 30–60 m | 8–15 m (no floors) | low pitch 6° | industrial |
| Silo | Ø 6–12 m | – | 20–45 m | flat cap | industrial |
| Tank | Ø 12–25 m | – | 8–14 m | flat cap | industrial |
| Chimney | Ø 3–5 m | – | 60–100 m, at most 1 per 600 m, beyond 300 m only | – | industrial |
| Farm | 15–30 m | 10–20 m | 6–10 m | gable | countryside, 120–400 m back |

Counts per 100 m chunk (both sides, without trees and lamps) follow GDD C2: city ≤ 14, suburb ≤ 24, industrial ≤ 10, countryside ≤ 4. Heights vary at least ±30 % along a row, and a gap shows sky at least every 150 m (GDD 6.7 gaps).

**Mesh shape.** A building is a box with a roof. Window geometry never exists: windows are in the texture (GDD C5). Full LOD ≤ **120 triangles** (box 10, roof parapet 16, plant box 10–20, optional 4 balcony slabs of 12, base plinth 8), box LOD ≤ **24**.

### 1.2 Facade styles (the 8 tile slices)

Tiles repeat in **world metres**, so one tile fits any building size. Window grid is the GDD grid: floor pitch 3.2 m, bay pitch 3.6 m (curtain walls use a 1.8 m mullion pitch, which is half a bay).

| Slice | Style | Wall `#` | Window | Window cell | Notes |
|---|---|---|---|---|---|
| 0 | F1 plaster, warm | `#B8B2A3` sand, `#BDB594` cream | glass `#3E4A57`, frame `#8D9096` | bay 3.6 × floor 3.2; opening 1.7 × 1.5, sill 0.9 | residential, suburb; joint band every floor `#A39E91` |
| 1 | F2 plaster, cool | `#AEB5A0` sage, `#A3B0B9` blue-grey | same | same | variety by tint, no extra tile |
| 2 | F3 precast panel | `#8E9093`, `#85888C` | glass `#434C57` | bay 3.6 × 3.2; opening 1.7 × 1.5 | panel joints 0.03 m `#74777A` every 3.6 m and 3.2 m |
| 3 | F4 curtain wall | spandrel `#4A5058` | glass `#7898B6` (blue), `#6A9690` (green), `#6682A0` (graphite) | mullion 1.8; glass 1.7 × 1.9, sill 0.55 | glass takes 60 % of the area; top 20 % of each glass cell 18 % lighter (sky reflection) |
| 4 | F5 brick | `#A19470` yellow brick, `#5F5750` dark clinker | glass `#4A525C` | bay 3.6 × 3.2; opening 1.6 × 1.5 | mortar lines baked in |
| 5 | F6 cladding | `#7F909A`, `#9A9E9C`, `#566F5F` | none; a 0.9 m ribbon `#4B5560` at 6.2 m | one tile = 6 m × 9 m | vertical ribs every 0.6 m; a 0.8 m dark base band |
| 6 | F7 house | `#B5AE9A`, `#A5B09C` | glass `#3E4A57` | bay 3.6 × 3.2; opening 1.4 × 1.4 | shutters not drawn |
| 7 | Roof / solid | `#6E7174` flat roof, `#9A9E9C` low pitch | – | – | also the plain colour for LOD boxes and the bridge-pier grey |

Roofs: flat `#6E7174` with a 0.5 m parapet `#5E6164`; gable `#4A4D52` (anthracite) or `#5C5A57` (brown-grey); low-pitch `#9AA3A6`. **No clay-red roofs** (look-spec 10.2 allowed far ones; with GDD F8 and rule 1, none at all). Plant boxes `#9A9DA0`.

**Glass and reflectivity.** Glass is opaque (look-spec "Glass"). Smoothness lives in the texture alpha: walls 0.1, window glass 0.6, curtain glass 0.8. With the skybox reflection probe (look-spec §2) the glass picks up a soft sky tone in one fetch. Nothing else reflects. Metallic 0 everywhere except the lamp and rail metal.

**Balconies** (residential blocks and towers only, LOD0 only): slabs 1.6 m deep × 3.6 m wide, 0.15 m thick, every second bay on every second floor, colour wall × 0.85. Four per building at most (48 triangles); everything else is in the texture (balcony rail band on slice 0, 1, 2).

### 1.3 Building colour per instance

One material, no per-building texture. Per building, vertex colour = the slice's wall colour is already in the texture, so the tint is only a multiplier: value 0.88–1.04, hue shift ≤ ±4°, saturation ×0.9–1.0. Baked into the vertices: ground floor ×0.85 (contact shading), 0.5 m under the roof ×0.9, the side facing away from the sun ×0.85 (shading by normal is already done by the lit shader; this only keeps the same look in the unlit LOD boxes).

### 1.4 Building the textures cheaply

- **One `Texture2DArray`**, 8 slices, **256 × 256 px**, RGBA, **ASTC 6×6** on Android (about 0.4 MB with mips). Alpha = smoothness. Each slice is drawn once at start-up with `SetPixels32`: fill the wall colour, draw window cells (rounded corners 2 px, 1 px lighter top edge), joint lines, then a few random 1–2 px tone dots (±3 %) for grain. Mips on, aniso 2. Time: well under 100 ms in total.
- One tile covers **2 bays × 2 floors** (7.2 m × 6.4 m): about 36 px per metre. A 1.7 m opening is 60 px wide.
- **UV = world metres / tile size** (u along the wall, v up), computed when the mesh is built. Slice index goes in `uv.z` (or in vertex colour alpha). One Shader Graph material (`Sample Texture 2D Array`, Simple Lit lighting, no normal map).
- Fallback if arrays are a problem on the tablet: one 1024 × 1024 atlas (4 × 2 tiles of 256 + 4 px gutter) and `frac()` in the shader with explicit gradients to avoid seams. Same look, a little more shader work. A third fallback, 8 plain Simple Lit materials, costs 8 draw calls per chunk side and is not recommended.
- Smaller than GDD C5 on purpose (that allows 2048² and about 3 MB).

### 1.5 Distant skyline (fades into the fog)

Opaque, no alpha (GDD C6). Fog off on these meshes, colour pre-mixed toward `#C9D6E2`, same method as the hill layers of look-spec 10.2. They move with the camera in x and z.

| Layer | Distance | Height | Colour | Content |
|---|---|---|---|---|
| Chunk skyline strip (GDD C3, to 1500 m) | 900–1500 m | 25–112 m | wall colour × 0.85, then fogged by the normal fog | merged boxes, ≤ 100 triangles per chunk side, flat vertex colour, `#7E8A94` – `#8E9AA4` |
| Near skyline | 1.8–2.2 km | 40–120 m | `#9FB0C0` | one stepped mesh strip 6 km wide, about 140 triangles (random block widths 20–60 m) |
| Far skyline | 2.6–3.2 km | 60–160 m | `#B2C0CD` | same, different seed, about 140 triangles |

The skyline is chosen by zone: city = towers; industrial = low sheds + 2–3 chimneys; suburb = low roofs and one church spire; countryside = none (the hill layers of look-spec 10.2 only). It fades in and out over the 300–500 m zone transition by sliding the strip vertically (rises 0 → full height, down to −height inside the ground), not by alpha.

---

## 2. Street furniture (speed-feel objects)

Near-field rhythm is the main speed cue (GDD 6.9): a vertical object every 40–55 m within 10 m of the rail, never above 3 passes per second. At 342 km/h (95 m/s) a 40 m rhythm is 2.4 passes per second.

| Object | Where | Size / look | Colour, material | Tris |
|---|---|---|---|---|
| **Lamp post** | median, double arm; city every 40 m, suburb 80 m, industrial 60 m, none in countryside (GDD 6.7); also along interchange ramps (every 40 m on the ramp side) | pole 9 m, Ø 0.3 m, arm 2.4 m each side, head 0.7 × 0.25 m | pole `#9AA0A5`, Metallic 0.6, Smoothness 0.45; head `#D8DADC` unlit | 90 |
| **Gantry** (back side, GDD 6.9) | every 1.0–1.5 km, at zone borders | two posts 0.5 m, truss beam 0.9 m, spans the 16.5 m road; two sign panels 6 × 2.2 m on top | galvanised `#8B9097` + 0.03 m white `#F4F4F2` frame (look-spec 7, same as chevron backs); blue front `#0F4F8A` only on the other carriageway | 300 |
| **Noise wall** | outer side, 3.0 m or more behind the rail (GDD F1) | 3, 4, 5 or 6 m high, posts every 5 m (0.25 m, `#5E6469`) | **glass-look panels** `#A9B8C2` (city), **timber-look** `#7C8068` (suburb, with a 0.3 m dark cap `#5E6469`), **concrete** `#9C9FA3` (industrial). All opaque. | 12 per 25 m |
| **Embankment / berm** | suburb 2–4 m, industrial 1.5–3 m, countryside 1 : 2.5–1 : 3 | slope 1 : 2, planted | grass `#6F8A3E`, shrubs as billboard strip; cutting slopes `#B8B0A0` (look-spec 10.2) | part of the road mesh |
| **Retaining wall** (trench) | 2.5 m behind the rail, 5–8 m high | panels 4 m wide, joints 0.03 m darker, cap band 0.3 m `#6C6E72`, fence 1.1 m on top `#6B7075` | concrete `#7A7C80`, Smoothness 0.1; foot shading ×0.85 over 1 m | 6 per panel |
| **Trees, street** (city) | outer side behind the noise wall, every 6–12 m | 7–10 m, crown Ø 5 m | `#3E5233` near / `#55684A` far (look-spec 7) | 40 (full mesh to 150 m), then 8 (cross quad) |
| **Hedge** | suburb plot lines | 1.8 m high, 0.8 m thick | `#4F6A3A` | 12 per 14 m |
| **Forest edge** | countryside 25–200 m | 14–22 m, firs and broadleaf mixed | near `#3E5233`, far `#55684A` | billboards 8 per tree beyond 150 m |
| **Billboard** (advertising) | city and industrial, 1 per 400–600 m, on the left 25–45 m or the right 45–70 m | 9 × 3.4 m board at 6 m height, two posts Ø 0.5 m. Left ones face the player; the right ones are seen from behind. | back and frame `#8B9097`; front from a 512 × 256 atlas of 8 abstract ads, blue, teal, green and yellow blocks (no red, no orange, no readable text) | 60 |
| **Power pylon** | suburb, industrial, countryside; 250–400 m apart, 60–120 m from the road | lattice, 45–60 m high, 3 cross-arms | galvanised `#8F989A`, drawn as thin wedges (about 120 triangles) | 120 + 3 line strips |
| **Silo, tank** | industrial | see 1.1 | `#A9ACA8`, cap `#9A9E9C` | 48 |
| **Container stack** | industrial yards | 6 × 2.5 × 2.6 m, stacks of 2–3 | only `#3F5C86`, `#566F5F`, `#8B9097` (no red, no orange) | 12 per box |
| **Km post, delineators** | as look-spec §6 | unchanged | unchanged | – |

All lamps, gantries, pylons and billboards are **instanced** and cast no shadow. The only shadows are the existing blob and bridge shadow quad (GDD C6, 5.4).

---

## 3. Zones and transitions

Lengths, setbacks, heights, density and noise-wall shares are in GDD 6.7. The look of each zone:

| | City | Suburb | Industrial | Countryside |
|---|---|---|---|---|
| Ground | `#8A9078` (grey-green, mostly covered) | `#86995A` lawns | `#8D9185` (gravel, tarmac yards) | field strips of look-spec 7 |
| Building line | continuous, 5–9 floors in front, towers behind | houses with 6–20 m gaps | sheds with 20–80 m gaps, silos | farms far back, forest edge near |
| Facade slices | 0–4 | 0, 1, 6 | 5 | 6 (farms) |
| Noise wall | 4 m glass-look on 60 % | 3 m timber-look on 40 % | 3 m concrete on 20 % | none |
| Greenery | street trees, small | hedges, garden trees | sparse rows, screening hedge | forest edge, tree lines, hills |
| Furniture | lamps 40 m, gantry at borders, billboards | lamps 80 m | lamps 60 m, pylons, billboards | lamps none; pylons; farm overpass |
| Sky edge | towers, 2 skyline layers | low roofs, spire | sheds, chimneys | hills only |

**Transitions (300–500 m, GDD 6.7).** Over the transition: building density and the height cap ramp linearly (the next building is drawn from the new zone's rules as soon as the dice allow it); the noise-wall height changes only at a post; the skyline strip slides as in 1.5; ground colour crossfades through vertex colour over 100 m. A gateway object stands at the border: an overpass or a gantry (seen from behind). Tree and facade slices may mix only inside the transition, never in the middle of a zone.

---

## 4. Interchanges, bridges, ramps, trenches

Layout and sight rules: GDD 6.6 (modules), 6.8 F4–F7. The look:

**Concrete.** Decks, parapets, piers, abutments and walls use one muted concrete grey **`#7A7C80`**, matte (URP/Lit, Metallic 0, Smoothness **0.1**). Shading is all vertex colour: underside of any deck ×0.55 (sun cannot reach it; no GI), pier foot ×0.85, parapet cap `#6C6E72`, drip line under the deck edge `#66686C`, expansion joint every 40 m (0.1 m dark line, `#5E6064`). No texture is needed: one 128 × 128 grain texture (±2 % noise) is optional.

| Part | Look |
|---|---|
| Deck | box girder, 1.5 m thick (1.8 m on the second level and on river bridges), 0.3 m cantilever fascia with the drip line. Deck top above the road: 6.3 m for a flyover (clearance 4.7 m + 1.5 m + margin), 12.6 m for a second level. |
| Parapet | solid concrete barrier 1.1 m high, 0.35 m thick, cap `#6C6E72`; on the player's road the parapet is the collision line. Ramps and flyovers carry a 0.7 m barrier + 0.4 m handrail `#7D858C`. |
| Piers | round **Ø 1.8 m** (valley viaducts Ø 2.4 m, tapering), every **30 m** on ramps and **35–55 m** on river and valley bridges; cap beam 3.4 × 1.1 m `#707276`. Overpasses: two outside the rails and, in the median, only behind the median rail (GDD F6), 0.8 m columns. |
| Abutments | sloped paving `#8A8D86` (1 : 1.5) under both bridge ends, concrete wing walls 0.5 m. |
| Ramp (slip road) | on the player's left; width 6.5 m + 0.75 shoulders; own lane marks; guardrail `#A9AFB5` on both sides; **climbs at most 5 %** into the flyover, so 6.3 m needs 126 m or more; typical 150–250 m. Taper, gore and grass wedge as in GDD 6.6. |
| Levels | three at most in view: road 0, flyover deck top +6.3 m, second level +12.6 m (junction `b` only, seen far away). |
| Median split | the opposite carriageway swings away; the freed median is grass `#7C9A52` with a few shrubs, lamps stay on the player's side. |
| Trench walls | see section 2; concrete `#7A7C80`, 5–8 m, light level 90 % of open road (GDD F7). Street bridges every 120–250 m, flat decks 1.2 m with a pedestrian rail. |
| Tunnels | unchanged (look-spec 10.3). |
| Underside lighting | deck undersides use the tunnel-light-pool trick only if the developer has spare budget: skip by default. |

Budget (GDD C8): ≤ 3,000 triangles per bridge or overpass, ≤ 16 piers in view (instanced, one mesh), ramp and trench walls from the road mesh pipeline (≤ 1,000 triangles per 100 m).

---

## 5. Mobile budget (Galaxy Tab S7, 60 fps)

GDD 6.10 caps are the hard limits. This table breaks them down so the developer knows where the numbers go. All figures are design targets, **not measured**.

**Per 100 m chunk, both sides, all scenery together**

| Distance | Triangle cap (C1) | City chunk, typical | Suburb | Industrial | Countryside |
|---|---|---|---|---|---|
| 0–400 m | 8,000 | 5,500 | 4,500 | 3,500 | 3,000 |
| 400–900 m | 3,000 | 2,000 | 1,800 | 1,400 | 1,200 |
| 900–1,500 m | 1,000 | 700 | 600 | 500 | 300 |

Typical city chunk, 0–400 m: 12 buildings × 100 = 1,200; noise wall 100 m × 2 = 100; skyline strip 100; 3 median lamps = 270; 40 trees × 40 = 1,600 (up to 60 trees in GDD C2, billboards beyond 150 m); gantry, billboard, furniture 400; chunk ground and walls in the road mesh pipeline, not counted here. That is about 3,700; the rest is headroom for a bridge or a ramp.

**Frame totals (urban kit only):** ≤ 53,000 triangles (GDD: 32,000 + 15,000 + 6,000), **≤ 60 draw calls, ≤ 24 meshes, ≤ 6 materials**.

**Materials (six):**

| # | Material | Content | Texture |
|---|---|---|---|
| M1 | Facade | all buildings, roofs, LOD boxes | the 8-slice array, 256² (0.4 MB) |
| M2 | Props | lamps, gantries, pylons, walls, bridges, ramps, silos, containers | vertex colour only; optional 128² grain |
| M3 | Foliage | tree and hedge cross quads, alpha clip 0.5 | the 1024 × 256 strip of look-spec 7 |
| M4 | Signs | billboards, gantry panels | 512 × 256 ad atlas + 512² sign atlas |
| M5 | Far | skyline strips, fog off | none, vertex colour |
| M6 | Glass | bridge glass only, alpha (GDD C6) | none |

Texture total for the urban kit about 3 MB, far under the GDD 24 MB.

**Draw calls (typical frame):** facade 15 chunk-sides merged into 2–4 meshes by distance band (0–400, 400–900, 900–1500) × 2 sides; props instanced: lamps 1, gantry 1, pylons 1, billboards 1, noise wall 1 per band, piers 1; foliage 2 (near full mesh, far billboards); far layers 3. That is about 25–35 calls, leaving room for the road and cars.

**Batching and instancing advice**
- Build one **merged mesh per chunk side per LOD band** for buildings (all share M1), written with the Mesh API from pooled `NativeArray`s: one chunk per frame, ≤ 2 ms CPU (GDD C7). Deterministic seed per chunk, so a rebuilt chunk looks the same.
- Props with repeats (lamps, piers, billboards, trees at full LOD) go through `Graphics.DrawMeshInstanced` or `RenderMeshInstanced` with a per-instance tint, 500 instances per call at most. Turn on **SRP Batcher** for everything else.
- Pool chunk meshes with the road chunks. No allocation during a run.

**LOD and culling**
| Rule | Value |
|---|---|
| Building LOD0 (full, ≤ 120 tris, balconies, plant box) | 0–150 m |
| Building LOD0 without balconies and plant box | 150–400 m |
| Building LOD1 (box, 12–24 tris, same material) | 400–900 m |
| Chunk skyline strip (≤ 100 tris per side) | 900–1500 m |
| Second building row (behind row 1) | to 400 m only; later it is part of the skyline strip |
| Trees: full mesh | 0–150 m; cross quads 150–1000 m; ≤ 25 per chunk beyond 300 m |
| Pylons and billboards | to 900 m |
| Lamps | to 600 m (thin; at 1.6 km they are under 1 px) |
| Objects behind the camera, or beyond the far clip (1,650 m) | culled with the chunk |

**Overdraw.** Buildings are opaque and sorted front to back by Unity. Do not stack alpha. Tree strips are alpha-clipped quads, each at most 40 × 18 m, two layers only (look-spec 7).

### Switch off in this order if the Tab S7 drops under 60 fps

Cheapest visual loss first. It matches GDD C9 (trees, LOD, draw distance) and adds the extra steps. Measure after each step; the gain per step is unknown until it is tried on the tablet.

1. Balconies and roof plant boxes off (LOD0 → "no details" at 0–400 m).
2. Far foliage layer off (trees beyond 150 m). *(GDD C9, first step)*
3. Billboards (advertising) and pylon lines off; pylons stay as masts.
4. Facade mip bias +1 (blurrier, cheaper bandwidth) or ASTC 8×8 for the array.
5. Building LOD distances × 0.7: LOD0 0–105, LOD0 plain 105–280, LOD1 280–630, skyline from 630 m. *(GDD C9)*
6. Second building row removed; the skyline strip begins at 400 m.
7. Median lamps: every 80 m instead of 40 m in the city, keep the rhythm in the first 200 m only.
8. Draw distance 1.5 km → 1.0 km (fog start 200 m, end 1,100 m as in look-spec 3). *(GDD C9)*
9. Skyline layers far (2.6–3.2 km) off; near skyline stays.
10. Last resort: tree billboards in the first 150 m become cross quads too.

Never reduced (GDD C10): vehicle spawn distance and sight rules. Only scenery is cut.

---

## 6. Open points for the developer and planner

- **Lamp spacing:** the brief said 50 m; GDD 6.7 gives 40 m (city), 80 m (suburb), 60 m (industrial). I used the GDD numbers, 50 m is the average.
- **Facade colours** are darker than my first draft because of GDD F8 (mid to dark). If the city then reads gloomy under the sun, raise the plaster value by at most 8 % and keep the rule "no flat white or light grey wider than 30 m".
- **Skyline without alpha** needs the sliding mesh strips of 1.5, which look more blocky than a texture mask. Alternative with the same cost: one 2048 × 128 R8 mask with alpha clip; allowed only if the planner extends GDD C6 for it.
- **Interchange ramp climbing 5 %** is my choice (RAA ramps are often up to 6–7 %); GDD F5 keeps the ramp within 1.0 m of the road height for its first 150 m, so the 5 % climb starts after those 150 m (6.3 m then needs another 126 m).
- Nothing here is measured on the tablet.
