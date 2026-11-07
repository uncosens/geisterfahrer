# Geisterfahrer — Game Design (final v0.5)

"Geisterfahrer" is German for a wrong-way driver ("ghost driver"). This draft describes the chosen concept and lists open questions. The developer implements from it, so numbers are concrete; they are first values to tune in playtests.

Status: final v0.4. Decided by the user (and by the director while the user was away, see `docs/decisions.md`):
- Concept A, one map (German Autobahn), realistic 3D, chase camera, Unity, Android, **landscape only**.
- **Personal use only** (the user plays it alone; no store release, no monetization, no online). So the player car is modeled directly on the **Porsche 911**, in **red**, and real vehicle designs may be used. (Personal use told by the user directly to the planner.)
- **No hard shoulder.** (Told by the user directly to the planner.)
- **4 lanes.**
- **Endless procedural road** with curves, hills, tunnels, viaducts and overpasses. **In curves the player must steer** against outward drift from v²/R.
- **Oncoming vehicles have a side offset inside their lane**, so the player can thread between them.
- Two-thumb controls: left half steers with a floating origin, right half is the accelerator.
- **Impulse-based collisions** with vehicles and guardrails.
- **Real car force model** for speed, no top-speed cap, **no minimum speed**, **start at 0 km/h**.
- **Records = top speed and max distance** (no points, no near miss, no combo).

Changes from v0.4 (user playtest, applied by the developer, `docs/decisions.md` 32–37):
- Steering: max side speed 7 → **9.8 m/s** and side-speed limit 0.25 × v → **0.35 × v** (+40 %); return to straight after the finger lifts: time constant 0.15 → **0.30 s** (2.2).
- Acceleration: power 260 → **140 kW**, new drive traction limit **μ_drive 0.60** (grip μ 0.85 stays for scrub and curve drift). 0–100 km/h **5.7 s**, 100–200 km/h **18.6 s**, flat ceiling **247 km/h** (240 km/h only after about a minute of clean driving). All speed tables in 2.2 are redone for this model.
- Traffic density **+40 %** in every phase (2.4). Two more vehicle types and 17 paint colours (4.1).
- Tunnels from the first minute: every straight of 500 m or more after the first 500 m gets one with 90 %, the first always (6.4, 6.5).
- Camera and road shake only while the accelerator is held above **150 km/h** (5.1).
- Overpass and tunnel concrete mid-grey `#7A7C80`, matte; steel rails and fields toned down; sun 1.35.

Changes from v0.3:
- 4 lanes instead of 3; cross-section, lane bias, density and spawn rules updated (3.1, 4.1, 2.4).
- New section 6 "Road geometry": curves with clothoids, hills, tunnels, viaducts, overpasses, chaining, frequency per phase.
- Curve drift model and its link to the side-speed limit and the rail impulse rule (2.2, 6.3).
- Grade force added to the force model; downhill raises the ceiling (2.2).
- Steering costs speed: tyre scrub force `F_steer ∝ s²` (2.2, user decision).
- Sight-distance rule per section type (5.4).
- Side offsets and slow wander inside lanes; the fair rule now uses a gap in meters ("safe corridor") instead of free lanes (4.2, 4.3).
- Start at 0 km/h; first-seconds check (2.2, 2.4).
- Late power boost proposal dropped: downhill sections now move the top-speed record (2.2).

All planner proposals of v0.4 were accepted by the director (`docs/decisions.md`). Values marked "RAA" are German motorway design guideline values (RAA 2008) as the planner recalls them; they could not be checked against the guideline text, so treat them as a guide. The game values used are on the safe side of them.

---

## 1. Concept

### Wrong Way (endless Autobahn dodge) — accepted

- **Pitch:** Drive a red 911 against traffic on the Autobahn. How fast can you go, and how far can you get?
- **Core loop:** Pull away → push your speed → dodge oncoming vehicles, slow for tight curves → crash → see top speed and distance → retry at once.
- **Controls:** Left thumb steers, right thumb holds the accelerator.
- **Session length:** 30 s – 3 min per run.
- **Platform:** Android, landscape, Unity, realistic 3D. Personal use.

---

## 2. Wrong Way

### 2.1 Core rules

- 3D chase view: the camera follows behind and above the player car (see 5). The player drives forward along an endless procedural Autobahn (see 6).
- The player is on the wrong carriageway, so **all traffic is oncoming**.
- **4 lanes, no shoulder** (see 3.1). The player steers freely across the road between the two guardrails.
- The run starts at standstill. The player sets their own speed with the accelerator; a car force model decides how fast it changes (see 2.2).
- In curves the car drifts outward unless the player steers against it (see 6.3).
- Traffic gets denser over time, no matter how fast the player drives (see 2.4).
- Every collision is judged by impulse: a hard hit ends the run, a light one only slows the car (see 2.5).

### 2.2 Controls and driving model

**Layout (landscape, multi-touch, both thumbs at once):**

| Screen part | Action |
|---|---|
| Left half | Steering with a floating origin |
| Right half | Accelerator (hold) |

No visible buttons for steering or gas. Optional feedback: a faint ring at the steering origin while the left thumb is down (no text).

**Steering (left half):**

- The first touch point is the center (origin). Drag left of it to steer left, right of it to steer right.
- **Dead zone:** 1.5 mm (about 10 dp) around the origin.
- **Max offset:** 12 mm (about 75 dp) from the origin = full steering. The origin does not follow the finger.
- Steering input `s` goes from -1 to +1, linear between the dead zone and the max offset.
- Lifting the finger = `s = 0`. Touching again sets a new origin at the new spot.
- If the touch starts on the left half and slides into the right half, it stays the steering finger until lifted.

(1 dp = 1/160 inch, about 0.16 mm.)

**Side motion (arcade, in road coordinates):**

The player's position is kept in road coordinates: `d` = distance along the road center line, `x` = side offset from the road center (positive = toward the player's right).

```
v_side_max = min(9.8 m/s, 0.35 × v)               v = forward speed in m/s
target     = v_side_max × s  −  drift             drift from curves, see 6.3
target     = clamp(target, −9.8, +9.8) m/s
side speed → target with time constant 0.12 s     (0.30 s when the finger is up)
```

- The full 9.8 m/s applies from 100 km/h (at 100 km/h it is 9.7 m/s, at 50 km/h 4.9 m/s; a stopped car cannot move sideways). One lane (3.75 m) takes about 0.45 s.
- No automatic pull to the lane center.
- **Look:** the car body turns (yaw) relative to the road tangent by about twice the real path angle, max 6°, and leans (roll) up to 3° away from the turn. Wheels turn with `s`.
- Real tire grip would make a lane change take about 1.3 s, too slow for this game. So side motion is arcade, while forward speed and curve drift use real values.

**Forward speed: car force model**

No gears. Each physics step:

```
a = (F_drive − F_drag − F_roll − F_grade − F_brake − F_steer) / m

F_drive = min(P / v, μ_drive·m·g) while the accelerator is held, else 0
F_drag  = 0.5 · ρ · CdA · v²
F_roll  = Crr · m · g            only while v > 0
F_grade = m · g · grade          grade = rise / run of the road at the car, + uphill
F_brake = 400 N                  engine braking, only while the accelerator is up and v > 0
F_steer = k_s · m · a_grip · s² · min(1, v / 27.8 m/s)    tyre scrub, see below
v       = max(v, 0)              no reverse, no rolling backward
```

| Value | Number | Note |
|---|---|---|
| m | 1500 kg | 911-like |
| P | **140 kW** at the wheels | was 260 kW; reduced after playtest so top speed is not easy |
| μ | 0.85 | tyre grip: scrub and the curve grip in 6.3 |
| μ_drive | **0.60** | traction limit of the drive force (power-limited above about 53 km/h) |
| CdA | 0.65 m² | |
| ρ | 1.225 kg/m³ | |
| Crr | 0.012 | |
| Engine braking | 400 N | |
| Start speed | **0 km/h** | user decision |

**Steering costs speed: tyre scrub (user decision)**

A tyre that makes side force runs at a slip angle, and part of that force points backward. Real drag ≈ side force × slip angle, and slip angle grows with side force. That gives a force that grows with the **square** of the steering input:

```
F_steer = k_s · m · a_grip · s² · min(1, v / 27.8 m/s)

k_s    = 0.15        (about 0.15 rad = 8.6° slip angle at full grip)
a_grip = μ · g = 8.34 m/s²
s      = the steering input actually applied (−1..+1, 0 inside the dead zone)
```

- Full steering: `0.15 × 1500 × 8.34 = 1876 N`.
- The same `s` counts for lane changes and for steering against curve drift (6.3), since both are the same thumb. So holding a tight curve costs speed all the way through it.
- **Small corrections are cheap** because of `s²`: `s = 0.3` costs only 9 % of full scrub (about 0.4 km/h per second). The dead zone (1.5 mm) costs nothing. No extra exemption is needed.
- `min(1, v / 27.8 m/s)` fades the cost out below 100 km/h, in line with the side-speed limit `0.25 × v`, so steering at low speed does not stall the car.
- Letting the car drift without steering costs nothing, but then the drift pushes it toward the rail.

**Speed change during 1 s of steering** (flat road; first number = no steering, for comparison):

| Start speed | Accelerator | s = 0 | s = 0.3 | s = 0.5 | s = 1.0 (full) |
|---|---|---|---|---|---|
| 100 km/h | held | +10.3 | +9.9 | +9.2 | +6.0 km/h |
| 100 km/h | up | −2.1 | −2.5 | −3.2 | −6.4 km/h |
| 200 km/h | held | +2.6 | +2.2 | +1.5 | −1.8 km/h |
| 200 km/h | up | −4.3 | −4.7 | −5.4 | −8.7 km/h |
| 300 km/h | held | −2.9 | −3.3 | −4.0 | −7.3 km/h |
| 300 km/h | up | −7.8 | −8.2 | −8.9 | −12.3 km/h |

- Full steering costs about **4 km/h per second** at any speed above 100 km/h. With the weaker engine that turns the gain above 200 km/h into a loss, so weaving at high speed clearly slows the car; at 100 km/h it is a small dent in a strong pull. (300 km/h is only reached downhill.)
- **Ceiling while steering steadily** (accelerator held, flat): `s = 0.3` → 239 km/h, `s = 0.5` → 227 km/h, `s = 1.0` → 171 km/h. A long curve that needs `s_hold` close to 1 pulls the car down toward a holdable speed by itself.

Checks: 0–100 km/h **5.7 s**, 100–200 km/h **18.6 s**, ceiling on flat road **247 km/h**. This is deliberately slower than a real 911 Carrera S (3.5 s, 8–9 s, 308 km/h): the user wants the high speeds to be earned.

**Holding the accelerator from the start (0 km/h, flat road):**

| Run time | Speed | Distance driven |
|---|---|---|
| 1 s | 21 km/h | 3 m |
| 2 s | 41 km/h | 12 m |
| 3 s | 62 km/h | 26 m |
| 4 s | 78 km/h | 45 m |
| 6 s | 103 km/h | 96 m |
| 8 s | 122 km/h | 159 m |
| 10 s | 137 km/h | 231 m |
| 12 s | 150 km/h | 311 m |
| 15 s | 166 km/h | 443 m |
| 20 s | 187 km/h | 688 m |
| 30 s | 213 km/h | 1.25 km |
| 40 s | 227 km/h | 1.86 km |
| 60 s | 240 km/h | 3.17 km |
| 90 s | 245 km/h | 5.2 km |
| 2 min + | 247 km/h | |

Where it flattens: 21 km/h/s at the start, 11 at 100 km/h, 6 at 150, under 3 km/h/s from 200; above 230 km/h each extra km/h takes several seconds.

**Ceiling by grade** (where drive force = drag + rolling + grade):

| Grade | −4 % | −2 % | 0 % | +2 % | +4 % |
|---|---|---|---|---|---|
| Ceiling | 272 km/h | 259 km/h | 247 km/h | 234 km/h | 222 km/h |

- Downhill sections let the car go above the flat ceiling. So the top-speed record keeps moving with skill: catching a long downhill at full speed with a clean line. This replaces the v0.3 "late power boost" idea.
- Uphill, coasting slows the car faster; on a 4 % downhill a coasting car settles at about 20 km/h instead of stopping (grade force is a bit more than rolling + engine braking).

**Coasting (accelerator up) on flat road from 303 km/h:**

| Coast time | 0 s | 2 s | 4 s | 6 s | 10 s | 15 s | 20 s | 30 s | 40 s | 60 s |
|---|---|---|---|---|---|---|---|---|---|---|
| Speed (km/h) | 303 | 288 | 273 | 260 | 236 | 211 | 189 | 154 | 126 | 82 |

Slowdown by speed: about 8 km/h/s at 300, 6 at 250, 4.3 at 200, 3 at 150, 2.1 at 100, 1.7 at 60. Coasting to a stop takes about 41 s from 60 km/h, 62 s from 100 km/h. No brake in the MVP, so coasting is the only way to slow before a tight curve (see 6.3 for the distances).

**Minimum speed: none (user decision).** Knock-on rules:

- A stopped or slow car can hardly move sideways (side-speed limit `0.25 × v`). That is the natural penalty for going slow.
- A stopped car hit head-on is always game over (see 2.5).
- No permanent safe spot exists: with side offsets (4.2) a gap next to a vehicle can open for a moment, but the next vehicle in that lane may be centered or lean the other way.
- The safe corridor (4.3) protects only a moving player; stopping is the player's choice.
- Crawling adds almost no distance and no top speed, and density keeps rising with time, so it needs no extra rule.

**Keyboard (Unity editor testing only):**

| Action | Keys |
|---|---|
| Steer left / right | `A` / `D` or `←` / `→` (full steering while held) |
| Accelerate | `W` or `Space` |
| Pause | `Esc` / `P` |
| Retry after crash | `Enter` |

### 2.3 Records

Each run records two values, each with its **own best** saved on the device:

- **Top speed:** the highest speed reached in the run (km/h, one decimal, e.g. 297.4 km/h).
- **Distance:** meters driven in the run, shown in km with two decimals (e.g. 4.37 km).

A run can set a new best in one, both, or neither.

### 2.4 Difficulty curve and run start

**Run start:** the car waits at standstill on the road with no traffic within 400 m. The run clock, the traffic spawner and the road phase all start on the **first accelerator press**. So the player is never hit before they start.

First seconds after that press (holding the accelerator):
- The first vehicles spawn at 400 m (view distance at low speed, see 5.3).
- A sedan at 130 km/h spawned at 400 m meets the player after about 6.6 s; by then the player is at about 150 km/h. A truck at 80 km/h meets them after about 8 s.
- So the player gets 6–8 s to get going before the first pass. Phase 1 traffic is sparse (below), and the corridor (4.3) starts at the player's own position.

Difficulty grows with **time**, not distance. The densities below include the user's +40 % (`DensityScale` 1.4). Traffic is set as **density** (vehicles per km of road, all 4 lanes). The player meets vehicles at density × closing speed, so driving faster means more vehicles per second.

| Phase | Time | Density (4 lanes) | Vehicles met per second at 100 / 250 km/h | Traffic | Road (see 6.5) |
|---|---|---|---|---|---|
| 1 | 0 – 20 s | 11 per km | about 0.7 / 1.1 | single vehicles, mostly cars | gentle |
| 2 | 20 – 60 s | 18 per km | about 1.1 / 2.0 | pairs; first buses and trucks | medium |
| 3 | 60 – 120 s | 28 per km | about 1.8 / 2.9 | side-by-side rows with one corridor; truck columns in lane 4 | hard |
| 4 | 120 s + | rises slowly to a cap of 45 per km | up to about 2.8 / 4.8 | many trucks and buses, tight gaps, fast cars in lane 1 | hardest |

- Per lane this is the same as v0.3 (density × 4/3).
- Density rises smoothly, not in jumps.
- Density is lower in tunnels and after blind spots, see 5.4.

### 2.5 Collisions

Every collision, with a vehicle, a guardrail, a tunnel wall or a bridge parapet, is judged by its impulse.

**Impulse:**

- `v_n` = closing speed along the contact normal, in m/s.
- `m_red = m_player × m_other / (m_player + m_other)`, in tonnes. Guardrails, tunnel walls and parapets = infinite mass, so `m_red = m_player = 1.5 t`.
- `J = m_red × v_n`, in t·m/s (= kN·s). No bounce term (restitution 0).
- The contact normal comes from our own C# collision code, not the Unity physics engine: 2D oriented boxes tested with SAT in road coordinates, the player box rotated by its real yaw, vehicle boxes placed at their side offset, guardrails and walls as lines with infinite mass. On a curve the rail line follows the road, so for a rail hit `v_n` = the player's side speed toward the rail (including drift, 6.3).
- Traffic vehicles are not knocked off course in the MVP.

**Rule:**

| J | Result |
|---|---|
| `J ≥ 6.0 t·m/s` | **Game over.** Short slow motion, camera shake, game over screen. |
| `J < 6.0 t·m/s` | **Survive.** The car loses speed and is pushed off. |

The threshold equals a speed change of 4 m/s (about 14 km/h) for the player car, below where real airbags usually fire (roughly 20–25 km/h).

**Survived hit:**

- **Speed loss:** `−8 km/h × J`, once at contact, not below 0.
- **Push-off:** side speed set to 1.5 m/s away from the contact. Steering works again at once.
- **Scraping:** while the car keeps touching (for example sliding along the outer rail in a curve), it loses another 15 km/h per second. Sparks and scrape sound.
- Camera shake and haptics scale with J.

**Worked examples:**

| Case | v_n | m_red | J | Result |
|---|---|---|---|---|
| Head-on with a sedan, player 60 km/h, sedan 130 km/h | 52.8 m/s | 0.77 t | 40.9 | Game over |
| Head-on, player stopped vs truck at 80 km/h | 22.2 m/s | 1.44 t | 32.0 | Game over |
| Head-on, player stopped vs compact at 110 km/h | 30.6 m/s | 0.63 t | 19.4 | Game over |
| Side-swipe of a truck, 2 m/s side speed | 2.0 m/s | 1.44 t | 2.9 | Survive, −23 km/h |
| Side hit on a truck at full steering (7 m/s) | 7.0 m/s | 1.44 t | 10.1 | Game over |
| Side hit on a sedan at full steering (7 m/s) | 7.0 m/s | 0.77 t | 5.4 | Survive, −43 km/h |
| Rail, flat touch, 1 m/s side speed | 1.0 m/s | 1.5 t | 1.5 | Survive, −12 km/h |
| Rail, 3 m/s side speed | 3.0 m/s | 1.5 t | 4.5 | Survive, −36 km/h |
| Rail, 7 m/s side speed | 7.0 m/s | 1.5 t | 10.5 | Game over |
| Curve R 720 m at 306 km/h (downhill only), full steering, 1.5 m/s net into the outer rail (6.3) | 1.5 m/s | 1.5 t | 2.3 | Survive, −18 km/h, then scrape until slow enough |
| Curve R 470 m at 306 km/h (downhill only), full steering, 7.8 m/s net (6.3) | 7.8 m/s | 1.5 t | 11.7 | Game over |

- Hitting the front of any vehicle is always game over (even stopped, at least 3× the threshold).
- Rail: survive below 4 m/s side speed, game over above.
- A truck or bus side hit ends the run above about 4.2 m/s side speed, a compact or sedan only above about 7.8 m/s.

### 2.6 HUD and game over screen

**During the run:** speed, large (`297 km/h`); distance (`4.37 km`); pause icon. Nothing else.

**Game over screen:** this run's top speed (one decimal) and distance; best top speed and best distance, each with a "New best" mark when beaten; a big Retry button. Retry puts the car back at standstill in under 1 s.

### 2.7 MVP feature list

1. Endless procedural Autobahn: 4 lanes, guardrails both sides, no shoulder (3.1, 6).
2. Road geometry: straights, clothoid curves, hills, overpasses, viaducts, tunnels (6). **Build order:** straights + curves → hills → overpasses → viaducts → tunnels, each playable before the next.
3. MVP map props from 3.2.
4. Chase camera, vehicle fade-in at the spawn distance, road drawn further (5).
5. Red Porsche 911 (992) player car.
6. Two-thumb touch controls, landscape only; keyboard for the editor.
7. Car force model with grade and tyre scrub (2.2). Start at 0 km/h, no cap, no minimum.
8. Curve drift (6.3).
9. 5 oncoming vehicle types (4.1) with side offsets and wander (4.2).
10. Spawning by density and phase, safe corridor (4.3), hidden-vehicle rule (5.5), sight rules per section (5.4).
11. Impulse collisions with vehicles, rails, walls, parapets (2.5).
12. Records: top speed and distance, each best saved locally.
13. HUD and game over screen (2.6).
14. Sound: engine (pitch follows speed), passing vehicles (Doppler), tunnel echo, scrape, crash.

Not in MVP: menus beyond a start screen, settings, brake, gears, damage model, night, rain.

### 2.8 Later list

- Brake (for example, a second zone on the right half).
- Gears and an engine sound with shifts.
- More vehicles from 4.1 (motorbike, sports car, caravan, car transporter, police).
- Vehicles that change lanes, flash headlights or honk.
- Trucks that lean onto the lane line in curves.
- Crosswind gusts on viaducts (a short side push).
- Traffic vehicles that react to hits.
- Construction zones, exits and on-ramps.
- Day / night / rain.
- Settings: steering sensitivity, left-handed layout, sound, haptics.
- Other 911 colors or variants.
- "Engine warm-up" power boost, only if top-speed records still get stuck despite downhills.

---

## 3. Map: Autobahn

### 3.1 Cross-section

Germany drives on the right. The player drives the wrong way, so the traffic's right side is the player's **left**. From the player's view, left to right:

`outer guardrail | edge strip | lane 4 (slowest) | lane 3 | lane 2 | lane 1 (fastest) | edge strip | median guardrail`

| Part | Width |
|---|---|
| Lanes 1–4 | 3.75 m each = 15.0 m |
| Edge strip, each side | 0.75 m |
| **Rail to rail** | **16.5 m** |

- Road center line = middle of the 16.5 m. Lane centers from the player's left rail: lane 4 at 2.625 m, lane 3 at 6.375 m, lane 2 at 10.125 m, lane 1 at 13.875 m.
- (The director's figure of about 16.25 m is close; 15.0 m of lanes + 2 × 0.75 m edge strips = 16.5 m. Some real 4-lane sections use 3.5 m for inner lanes; the game keeps 3.75 m for all four.)
- Trucks and buses keep to lane 4, sometimes lane 3 (German keep-right rule). Fast cars use lanes 1–2.
- In a landscape chase view the 16.5 m road fits the screen: the camera's horizontal field of view must be at least 75° (see 5.1).

### 3.2 What the map has

| Element | Notes | MVP / Later |
|---|---|---|
| Asphalt, 4 lanes | Grey, slight texture and wear | MVP |
| Lane marks | Dashed white lines between lanes (6 m line, 12 m gap); solid white edge lines | MVP |
| Outer guardrail (Leitplanke) | Metal rail on the player's left; collision line | MVP |
| Median | Metal guardrail + grass strip on the player's right; collision line | MVP |
| Delineator posts (Leitpfosten) | White posts with black band, every 50 m | MVP |
| Kilometer posts | Sign every 500 m; matches the distance read-out | MVP |
| Roadside | Grass, fields, trees, cuttings and embankments that follow the hills | MVP |
| Overpasses | Other roads crossing above (6.4) | MVP |
| Viaducts | Player's road on a high bridge (6.4) | MVP |
| Tunnels | (6.4); from the first minute | MVP |
| Curve chevron boards | Black/white arrow boards on the outer side of tight curves (R < 1000 m), seen from behind like all signs, so the shape matters more than the arrows | MVP |
| Blue direction signs | Overhead gantries with blue signs, seen from behind | Later |
| Emergency phone (orange SOS post) | Behind the rail | Later |
| Noise walls | Some stretches | Later |
| Exits / on-ramps, construction zones | | Later |

Props beyond the rails are scenery only and never block sight in curves (5.4).

---

## 4. Vehicles

### 4.1 Roster

Variety (user request): Hatchback and Long van reuse the Compact and Van models at their natural size; extra model files named `Compact2.glb`, `Sedan2.glb` and so on are picked at random; every vehicle gets one of 17 paint colours (no red or orange), never the same as the last one of its type. Buses and trucks keep their livery.

Sizes and masses are real-world values; the designer builds models to this scale. Speed is the vehicle's own speed and does not depend on the player. Spawn weights are relative per phase (0 = never).

| Vehicle | Length | Width | Height | Mass | Own speed | Lanes | P1 | P2 | P3 | P4 | MVP |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Compact car | 3.7 m | 1.7 m | 1.5 m | 1.1 t | 110–130 km/h | 2–4 | 22 | 17 | 12 | 9 | MVP |
| Hatchback (Golf size) | 4.15 m | 1.76 m | 1.45 m | 1.3 t | 115–145 km/h | 2–4, sometimes 1 | 22 | 18 | 13 | 10 | MVP |
| Sedan | 4.8 m | 1.85 m | 1.45 m | 1.6 t | 130–180 km/h | 1–3 | 28 | 24 | 20 | 17 | MVP |
| Van | 5.5 m | 2.0 m | 2.5 m | 2.5 t | 110–140 km/h | 3–4, sometimes 2 | 14 | 14 | 14 | 14 | MVP |
| Long van | 5.9 m | 2.0 m | 2.55 m | 3.0 t | 100–130 km/h | 3–4, sometimes 2 | 6 | 8 | 8 | 8 | MVP |
| Bus (coach) | 12 m | 2.55 m | 3.8 m | 18 t | 90–100 km/h | 4, sometimes 3 | 0 | 8 | 12 | 15 | MVP |
| Truck (semi) | 16.5 m | 2.55 m | 4.0 m | 35 t | 80–90 km/h | 4, sometimes 3 | 0 | 10 | 18 | 25 | MVP |
| Motorbike | 2.2 m | 0.8 m | 1.4 m | 0.25 t | 120–200 km/h | any | 0 | 0 | 5 | 8 | Later |
| Sports car | 4.5 m | 1.9 m | 1.3 m | 1.5 t | 200–250 km/h | 1 | 0 | 0 | 5 | 10 | Later |
| Car with caravan | 11 m | 2.3 m | 2.6 m | 3.0 t | 80–100 km/h | 4 | 0 | 5 | 8 | 8 | Later |
| Car transporter | 20 m | 2.55 m | 4.0 m | 30 t | 80 km/h | 4 | 0 | 0 | 5 | 10 | Later |
| Police car | 4.8 m | 1.85 m | 1.5 m | 1.7 t | 150–200 km/h | any | 0 | 0 | 3 | 5 | Later |

Lane choice inside the allowed lanes: trucks and buses 80 % lane 4 / 20 % lane 3; vans 45 % lane 4, 45 % lane 3, 10 % lane 2; compacts even over lanes 2–4; sedans 40 % lane 1, 35 % lane 2, 25 % lane 3.

Traffic does not slow in curves in the MVP: the tightest game curve (R 470 m) needs only 0.25 g at 130 km/h, which is normal driving.

**Player car:** Porsche 911 (992), red. About 4.5 m long, 1.85 m wide, 1.3 m high, 1.5 t.

### 4.2 Side offset inside the lane

Each oncoming vehicle drives at a side offset `o(t)` from its lane center (measured along the road normal, so it works in curves too).

```
o_max = (3.75 − width) / 2 − 0.10 m        body never crosses the lane line (0.10 m margin)
o_base ~ normal(0, 0.5 × o_max), clamped to ±o_max     drawn once at spawn
A ~ uniform(0, 0.3 × o_max)                 wander amplitude
T ~ uniform(8, 15) s, φ ~ uniform(0, 2π)    wander period and phase
o(t) = clamp(o_base + A · sin(2π t / T + φ), −o_max, +o_max)
```

| Vehicle | Width | o_max | Max wander speed (2πA/T) |
|---|---|---|---|
| Compact | 1.7 m | 0.93 m | 0.21 m/s |
| Sedan | 1.85 m | 0.85 m | 0.19 m/s |
| Van | 2.0 m | 0.78 m | 0.18 m/s |
| Bus / truck | 2.55 m | 0.50 m | 0.12 m/s |

- Wander is slow (under 0.25 m/s), so a gap the player sees stays readable.
- Bodies stay inside the lane lines. Touching the line is not allowed in the MVP; trucks leaning onto the line in curves is a later idea.

**Threading:** the gap between two vehicles side by side in neighbouring lanes is `3.75 + o_out1 + o_out2 − (w1 + w2) / 2` (o_out = offset away from each other). The player car is 1.85 m wide.

| Pair | Both centered | Both leaning out fully | Player fits? |
|---|---|---|---|
| Compact + compact | 2.05 m | 3.90 m | yes, easy when leaning out |
| Sedan + sedan | 1.90 m | 3.60 m | centered: 2.5 cm each side (no); leaning out: yes |
| Truck + sedan | 1.55 m | 2.90 m | only when leaning out |
| Truck + truck | 1.20 m | 2.20 m | only when both lean fully out (17 cm each side) |

So threading is possible but risky, and it is a choice: the safe corridor (4.3) never depends on it.

### 4.3 Fair rule: the safe corridor

Replaces the v0.3 free-lane rule. It uses real gaps in meters.

- The spawner keeps a planned **safe corridor**: a side position `x_c(t)` as a function of **meet time** `t` (the time until something at that place meets the player).
- **Corridor width:** `W_c = 1.85 m (player) + 2 × 0.50 m = 2.85 m`. Every vehicle is placed so that its whole body, with its full wander envelope (`o_base ± A`), stays at least `W_c / 2 = 1.425 m` from `x_c` during its meet window. The meet window is the time its body takes to pass the player, plus 0.3 s on each side.
- **Corridor stays on the road:** `x_c` is at least 1.425 m + 0.2 m from each rail.
- **Corridor moves slowly:** `|dx_c/dt| ≤ 4 m/s` on straights (57 % of the max side speed, so a player who reacts 1 s late still makes it). In curves it moves only outward or not at all when the current `s_hold > 0.3` (6.3), since the player's inward reach is smaller there.
- **At run start** the corridor begins at the player's position.
- **Meet time** for a vehicle at road distance `Δd` ahead: `Δd / (v_player + v_vehicle)`, using the current player speed. If the player speed changes by more than 20 % since a vehicle was placed, the spawner rechecks vehicles not yet within 3 s of the player and moves the corridor plan, never moving a placed vehicle.
- Vehicles in the same lane keep their speed (no overtaking). A faster vehicle never spawns right behind a slower one in the same lane.
- Trucks and buses come in columns of 2–3 in lane 4 from phase 3.
- Gaps narrower than the corridor (threading, 4.2) appear naturally and are allowed; only the corridor is guaranteed.

---

## 5. Camera and visibility

### 5.1 Chase camera

- About 6–7 m behind, 2.75 m high, looking slightly down the road. Starting values.
- Horizontal field of view at least 75° (on a 20:9 phone, a vertical FOV of 50° gives about 92° horizontal). Widens a little with speed.
- The camera follows the car's side position with a small delay, and turns with the road tangent and the car's yaw.
- **Shake (user decision):** camera shake and road vibration only while the accelerator is held and the speed is above **150 km/h**, scaled by speed and eased in and out over 0.3 s. None at low speed, at steady speed without throttle, or when coasting. Hits and crashes still shake the camera.
- The camera sees over normal cars, not over buses and trucks (5.5).

### 5.2 Reaction-time rule

- Human reaction to a visual cue is 0.7–1.5 s; one lane takes 0.55 s, four lanes (11.25 m) about 1.6 s. The corridor moves at most 4 m/s, so the player never needs more than about 1 lane of move per second.
- **Rule: every vehicle is clearly readable at least 3 s before it meets the player.** Readable = inside the camera view, not hidden by terrain, walls or a tall vehicle, and fully faded in.

### 5.3 Spawn distance and draw distance

- **Vehicles spawn** at `max(400 m, closing speed × 3.5 s)` ahead along the road, and fade in over the first 30 m. 3.5 s at spawn leaves 3 s after the fade.

| Player speed | Max closing (vs 180 km/h sedan) | Spawn distance |
|---|---|---|
| 0 – 230 km/h | up to 410 km/h | 400 m |
| 250 km/h | 430 km/h | 420 m |
| 247 km/h (flat ceiling) | 427 km/h | 415 m |
| 272 km/h (4 % downhill ceiling) | 452 km/h | 440 m |

- **Road and terrain** are drawn to about 1.5 km with light distance fog (the developer may cut it to 1 km if the Tab S7 drops below 60 fps). Only vehicles fade in at the spawn distance. So curves, crests and tunnel portals are visible long before traffic, and the player can coast early enough (6.3).
- At the start (0 km/h) the spawn distance is 400 m and the warning time is 8 s or more.
- Daytime headlights on all oncoming vehicles are the early cue at high speed. To be checked on a real device.

### 5.4 Sight distance per section type

| Section | What could cut sight | How the 3 s rule is kept |
|---|---|---|
| Straight | nothing | spawn distance (5.3) |
| Curve | inner roadside, and the camera view angle | No sight-blocking objects (trees, walls, cuttings higher than 1 m) within 15 m of the inner rail in curves with R < 3000 m. Bearing of a vehicle at distance S on a curve is about S / (2R): at R 470 m and S 470 m it is 29°, inside the 37.5° half-FOV. |
| Crest | the hill itself | Crest radius **≥ 15 000 m** (RAA minimum for the top design class is about 13 000 m). Sight over a crest with camera height 2.75 m and target height 1.0 m: `S = (√(2·2.75) + √(2·1.0)) · √R` = 460 m at R 15 000 m, which is 3.3 s at the downhill-ceiling closing speed of 507 km/h. Extra rule: no side-by-side rows (only one corridor) within 300 m after a crest top, in all phases. |
| Sag (dip) | nothing by day | no rule |
| Tunnel | walls in a curve | Tunnels are **straight** (no curve, no crest inside) and lit, so sight = full length. No haze inside. Density inside × 0.7. 200 m straight before each portal so the portal and the traffic behind it are seen head-on. Camera exposure adapts over 0.5 s at portals; tunnel lights bright enough that vehicles stay readable during it. |
| Viaduct | nothing | Parapet on both sides = rail (infinite mass). No sight rule. |
| Overpass | short shadow | Shadow kept light (vehicles stay readable in it). No other rule. |
| Behind a tall vehicle | the vehicle | 5.5 |

Combination rules: no crest inside a curve with R < 1500 m; no tunnel portal within 500 m after a crest top.

### 5.5 Hidden vehicles behind trucks and buses

- Behind a tall vehicle (van, bus, truck) in the same lane, leave at least 1.5 s closing time (at the current player speed) before the next vehicle.
- Never let a tall vehicle hide a vehicle that borders the corridor.

---

## 6. Road geometry

### 6.1 Model

The road is a chain of sections generated ahead of the player (always at least 2 km ahead, so the 1.5 km draw distance is never empty). It has three layers, all functions of road distance `d`:

1. **Horizontal:** curvature `κ(d)` (1/R, signed; + = curve to the player's right). Heading = integral of κ, position = integral of heading.
2. **Vertical:** grade `g(d)` (rise/run). Height = integral of grade.
3. **Structure tag:** open road, tunnel, viaduct; plus overpass props at points.

Everything (lanes, rails, vehicles, the player) lives in road coordinates `(d, x)`; the 3D mesh and the camera are built from them.

### 6.2 Horizontal: straights, clothoids, arcs

- **Transitions: clothoids.** Curvature changes linearly with distance, so drift (6.3) builds up smoothly. A simple arc would jump from 0 to full drift in one frame. Clothoids are also simple here: `κ(d)` is piecewise linear.
- A curve = clothoid in (κ from 0 to 1/R over length L) + arc (length R·θ_arc) + clothoid out.
- Clothoid parameter `A = R / 2`, so `L = A² / R = R / 4` (RAA: A between R/3 and R). R 470 m → L 118 m; R 1000 m → 250 m; R 3000 m → 750 m.
- Total turning angle θ: 10° to 45°, random.
- **S-curves** (phase 3+): a curve followed directly by an opposite curve, clothoid to clothoid, with no straight between.
- **Straights** between curves: length by phase (6.5).

Design values (RAA, as recalled): top design class min radius about 720 m; the next class about 470 m. The game uses **470 m as the hard minimum** and 720 m as the normal minimum.

### 6.3 Curve drift (user decision)

In a curve the car is pushed outward. The player must steer against it, and faster means more push.

```
c       = max(0, v² / R − g · q)       needed side acceleration, m/s²
          q = 0.04 superelevation (road tilted toward the inside, in all curves)
a_grip  = μ · g = 0.85 · 9.81 = 8.34 m/s²
s_hold  = c / a_grip                   steering needed just to hold the line
drift   = v_side_max × s_hold, pushing outward    (used in 2.2: target = v_side_max × s − drift)
```

- `s_hold < 1`: the player can hold the line and still dodge: inward up to `(1 − s_hold) × 7 m/s`, outward easily.
- `s_hold = 1`: full steering only holds the line; no inward dodge.
- `s_hold > 1`: even full steering drifts outward at `(s_hold − 1) × 7 m/s` until the car hits the outer rail. The rail rule (2.5) decides: below 4 m/s side speed (`s_hold < 1.57`) it is a scrape that costs 15 km/h per second, which lowers `s_hold` until the car can hold again; above that it is game over.
- The car body visually turns with the road; the drift shows as the car sliding toward the outer rail.

**Speeds per curve radius (flat curve, q = 4 %):**

| R | Comfortable (s_hold 0.5) | Hold limit (s_hold 1.0) | Rail game over above (s_hold 1.57) | At the 306 km/h ceiling |
|---|---|---|---|---|
| 470 m | 167 km/h | 231 km/h | 287 km/h | s_hold 1.80, drift 5.6 m/s → **game over** |
| 720 m | 206 km/h | 285 km/h | 355 km/h | s_hold 1.16, drift 1.1 m/s → scrape |
| 1000 m | 243 km/h | 336 km/h | 418 km/h | s_hold 0.82 → holds, little inward dodge (1.3 m/s) |
| 1500 m | 298 km/h | 412 km/h | — | s_hold 0.53 → fine |
| 2000 m | 344 km/h | 476 km/h | — | s_hold 0.39 → fine |
| 3000 m | 421 km/h | — | — | s_hold 0.24 → fine |

So:
- **The sharpest curve (470 m) is fine at 160–170 km/h**, holdable up to 231 km/h, and deadly near the ceiling.
- **The ceiling is only safe on straights and gentle curves** (R ≥ 1500 m comfortable, R ≥ 1000 m just holdable).
- **Slowing in time:** the flat ceiling is now 247 km/h, so speeds above the hold limit at R 470 (231 km/h) are reached only near it or downhill; from 247 km/h, coasting to 231 km/h takes about 3 s and 200 m. The 306 km/h columns of this table apply only to a car that was already that fast (not reachable any more). With road drawn to 1.5 km (5.3), a tight curve is visible about 17 s ahead at the ceiling. Chevron boards (3.2) mark curves under 1000 m.
- **Tyre scrub (2.2) helps in curves:** holding `s = s_hold` costs speed. In a 1000 m curve at 306 km/h (`s_hold` 0.82) the car slows by about 3 km/h per second even with the accelerator held, so it settles toward a comfortable speed. The table above is the speed at curve entry; the speed through the curve drops from there.
- **Downhill + tight curve:** no curve with R < 1000 m on a downhill steeper than −2 %, since the car would arrive faster than the flat ceiling.

### 6.4 Vertical and structures

**Hills (vertical):**
- Grade between −4 % and +4 % (RAA top class max about 4 %).
- Grade changes through vertical curves: crest radius ≥ 15 000 m (5.4), sag radius ≥ 9 000 m (RAA about 8 800 m). Vertical curve length = R × |grade change|; e.g. +3 % to −3 % over a crest of R 15 000 m is 900 m long.
- Grade segments (constant grade): 400–2000 m.
- At the ceiling, a crest of R 15 000 m gives v²/R = 0.5 m/s² (5 % of g), so the car never lifts off. No jumps.
- Grade feeds `F_grade` in the force model (2.2).

**Tunnels:**
- On straights only: any straight of 500 m or more, starting after the first 500 m of the run, gets a tunnel with 90 % chance, and the first qualifying straight always does (the road is generated 2.5 km ahead, so a phase-based chance never fired in the first minute).
- After 3 km of road without a tunnel the generator makes the next straight at least 1000 m long and puts a tunnel on it, so tunnels come about every 2.5–4 km.
- Length 300–1500 m. Straight, no crest inside (it may climb or sag). 200 m straight before each portal (5.4).
- Cross-section: a round bore. Vertical walls up to 1.0 m on the rail lines, then a half-ellipse vault of 6.5 m rise (4.6 m clearance at the lane edge). Earth mound over it, concrete portal ring at entry and exit, lamps along the crown.
- Walls replace rails: same collision line, infinite mass. Lit with a row of ceiling lights; brighter entry and exit zones (about 100 m).
- Density × 0.7 inside.

**Viaducts (Talbrücken):**
- Length 300–1500 m, deck 20–80 m above a valley. Usually over a sag, so the road dips into the valley approach and climbs out.
- Straight or curves with R ≥ 720 m.
- Concrete parapet with a rail on both sides = collision line. Valley, river or forest below; open sky, no roadside props.

**Overpasses:**
- Another road or a farm track crossing above on a bridge, 8–15 m wide, clearance 4.7 m (German standard clearance, as recalled). Abutments and piers stand outside the rails and in the median behind the median rail; no collision with them.
- Can sit on any open section, including curves. A short light shadow (5.4).

### 6.5 Chaining and frequency per phase

The generator appends one horizontal element at a time (straight or curve), then lays the vertical profile and structures over it, checking the combination rules (5.4, 6.3). The phase used is the phase at the moment the section is generated.

| | Phase 1 (0–20 s) | Phase 2 (20–60 s) | Phase 3 (60–120 s) | Phase 4 (120 s +) |
|---|---|---|---|---|
| Straight length | 800–2000 m | 500–1500 m | 300–1200 m | 200–1000 m |
| Curve radius | 2000–5000 m | 1200–3000 m | 720–2000 m | 470–1500 m (20 % of curves under 720 m) |
| S-curves | no | no | 20 % of curves | 35 % of curves |
| Max grade | ±1 % | ±2.5 % | ±4 % | ±4 % |
| Overpasses | every 1–2 km | every 1–2 km | every 1–2 km | every 1–2 km |
| Viaducts | none | 1 per 5 km | 1 per 5 km | 1 per 4 km |
| Tunnels | on 90 % of straights ≥ 500 m, the first one always | same | same | same |

- "1 per X km" = chance per km of road generated (e.g. 1/5 per km).
- In phase 1 the player covers about 0.9 km (from 0, holding the accelerator), so the first section is a straight of at least 800 m: the start is always straight and flat.
- A 3-minute run at about 230 km/h average covers about 11 km, so a typical good run sees 1–2 tunnels and 2 viaducts.

---

## 7. Open points

None in this document. Decisions taken after v0.4, and small points the planner decides later, are logged in `docs/decisions.md`.
