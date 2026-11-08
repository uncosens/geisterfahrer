# Geisterfahrer — Game Design (final v0.8)

"Geisterfahrer" is German for a wrong-way driver ("ghost driver"). This draft describes the chosen concept and lists open questions. The developer implements from it, so numbers are concrete; they are first values to tune in playtests.

Status: final v0.8. Decided by the user (and by the director while the user was away, see `docs/decisions.md`):
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

Changes from v0.7 (merge of the planner's v0.7 with the developer's newer rules, plus the user's newest numbers, `docs/decisions.md` 73–78):
- **Power 175 kW** (user; CdA 0.30, μ_drive 0.55, mass 1500 kg unchanged). From standstill: 100 km/h **5.5 s**, 200 km/h **17.0 s**, 250 km/h **28.1 s**, 300 km/h **49.1 s**, flat ceiling **342 km/h**, 4 % downhill ceiling 382 km/h, practical top after a 2 km downhill about **363 km/h** (2.2). Every speed-dependent table is redone: scrub (2.2), first seconds (2.4), spawn distance up to 604 m (5.3), crest sight with the lowered camera (5.4), curve speeds and drift (6.3), speed dial **0–400 km/h** (2.6).
- **Minimum curve radius 470 → 600 m** (6.2, 6.3, 6.5): at 342 km/h a 470 m curve left only 0.5 s between seeing it and the last moment to coast. At 600 m that margin is 8.8 s.
- **Speed camera pitch range +50 %** (user): 3° → **4.5°** further down at 300 km/h; height and distance unchanged (5.1).
- **Shake from 180 km/h** (was 200), full at 340 km/h, `ShakeScale` **0.52** (5.1).
- Kept from the developer: speed-based camera instead of the throttle camera, underground and mound tunnels with the random tunnel schedule, covered sections, crest radius 20 000 m, the `autodrive` test flag that expires after 10 minutes, the 991 player car (2.7, 4.1).
- Decision cross references use the renumbered rows of `docs/decisions.md` (planner rows 58–72 in the merged file).

Changes from v0.6 (user decisions on top speed and motorbikes, `docs/decisions.md` 66–71):
- **Real top speed:** CdA 0.65 → 0.30 m², μ_drive 0.60 → 0.55 and a stronger engine (power raised in steps, 175 kW in v0.8 above) so that the car reaches a real top speed in about a minute. The 18 s target for 100–200 km/h cannot be held together with 300 km/h in about a minute for a constant-power car (2.2).
- Everything that depends on top speed was redone: spawn distance (5.3), crest radius **20 000 m** (5.4, 6.4), curve speeds and drift with the 9.8 m/s side speed (6.3), a rule against tight curves right after a steep downhill (6.3), first-seconds timing (2.4), shake range (5.1), speed dial (2.6), records text (2.3). The numbers were recomputed again for 175 kW in v0.8.
- **Motorbikes are non-lethal** (replaces the rider rule of v0.6): a hit, even head-on, never ends the run; it costs 40–60 % of the speed (2.5).

Changes from v0.5 (user request: at least 30 different vehicles, `docs/decisions.md` 58–64):
- 4.1 is rewritten: **16 physical classes** and **62 variants** (model and paint on the same physics). Spawn weights per class still sum to 100 per phase and keep the heavy-vehicle share (0 / 17 / 29 / 40 %).
- Variants carry only a share inside the class; police and ambulance also carry a speed and lane override (occasional fast vehicles).
- Motorbike class: flagged `NonLethal` in v0.7 (see above).
- Side offsets for all classes (4.2); spawn distance computed per vehicle (5.3); catch-up gap rule for fast vehicles behind slow ones in the same lane (4.3); impulse examples corrected for the 9.8 m/s side speed (2.5).

Changes from v0.4 (user playtest, applied by the developer, `docs/decisions.md` 32–37):
- Steering: max side speed 7 → **9.8 m/s** and side-speed limit 0.25 × v → **0.35 × v** (+40 %); return to straight after the finger lifts: time constant 0.15 → **0.30 s** (2.2).
- Acceleration: power 260 → **140 kW**, new drive traction limit **μ_drive 0.60** (grip μ 0.85 stays for scrub and curve drift). 0–100 km/h **5.7 s**, 100–200 km/h **18.6 s**, flat ceiling **247 km/h** (240 km/h only after about a minute of clean driving). All speed tables in 2.2 are redone for this model.
- Traffic density **+40 %** in every phase (2.4). Two more vehicle types and 17 paint colours (4.1).
- Tunnels are regular but random: none in the first 1.5 km, then a 50 % chance per straight of 500 m or more, two kinds: earth-mound bore or underground cut-and-cover box (6.4, 6.5).
- The camera follows speed, not the accelerator (5.1; numbers updated in v0.8).
- Camera and road shake only while the accelerator is held, from a speed threshold (5.1; numbers updated in v0.8). Power was raised in steps (165 kW in decision 54; 175 kW in v0.8).
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
| P | **175 kW** at the wheels | v0.4: 260, v0.5: 140, then 155, 165, now 175 (user): 300 km/h after about 49 s |
| μ | 0.85 | tyre grip: scrub and the curve grip in 6.3 |
| μ_drive | **0.55** | traction limit of the drive force (power-limited above about 72 km/h) |
| CdA | **0.30 m²** | was 0.65; lower drag lets a modest engine reach 342 km/h (a real 911 is about 0.65, this is a game value) |
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
- `min(1, v / 27.8 m/s)` fades the cost out below 100 km/h, in line with the side-speed limit `0.35 × v`, so steering at low speed does not stall the car.
- Letting the car drift without steering costs nothing, but then the drift pushes it toward the rail.

**Speed change during 1 s of steering** (flat road; first number = no steering, for comparison):

| Start speed | Accelerator | s = 0 | s = 0.3 | s = 0.5 | s = 1.0 (full) |
|---|---|---|---|---|---|
| 100 km/h | held | +13.4 | +13.0 | +12.3 | +9.2 km/h |
| 100 km/h | up | −1.7 | −2.1 | −2.8 | −6.1 km/h |
| 200 km/h | held | +5.6 | +5.2 | +4.5 | +1.2 km/h |
| 200 km/h | up | −2.7 | −3.1 | −3.8 | −7.2 km/h |
| 300 km/h | held | +1.5 | +1.1 | +0.4 | −2.9 km/h |
| 300 km/h | up | −4.4 | −4.8 | −5.5 | −8.9 km/h |

- Full steering costs about **4 km/h per second** at any speed above 100 km/h. Above 250 km/h that turns a small gain into a loss, so weaving at high speed clearly slows the car; at 100 km/h it is a small dent in a strong pull.
- **Ceiling while steering steadily** (accelerator held, flat): `s = 0.3` → 331 km/h, `s = 0.5` → 312 km/h, `s = 1.0` → 227 km/h. A long curve that needs `s_hold` close to 1 pulls the car down toward a holdable speed by itself.

**Checks (v0.8 constants, chosen by simulation against the user's targets):**

| Target | Result |
|---|---|
| 0–100 km/h about 6 s | **5.5 s** |
| 100–200 km/h about 18 s | **11.5 s** (36 % under; the user raised power twice since) |
| 300 km/h after about 55 s from standstill | **49.1 s** (100 km/h → 300 km/h takes 43.7 s) |
| Flat ceiling 315–330 km/h | **342 km/h** (above the old range, user's choice of 175 kW) |

With a constant-power car, slow 100–200 km/h and fast 300 km/h pull against each other; each step up in power moved the car further toward "fast". The mass stays 1.5 t because the collision rule uses it. A power curve that rises with speed would add one more constant and is not used.

**Holding the accelerator from the start (0 km/h, flat road):**

| Run time | Speed | Distance driven | Acceleration |
|---|---|---|---|
| 1 s | 19 km/h | 3 m | 19 km/h/s |
| 2 s | 38 km/h | 11 m | 19 |
| 3 s | 57 km/h | 24 m | 19 |
| 4 s | 76 km/h | 42 m | 19 |
| 6 s | 107 km/h | 94 m | 13 |
| 8 s | 131 km/h | 160 m | 10.5 |
| 10 s | 150 km/h | 238 m | 8.9 |
| 12 s | 167 km/h | 326 m | 7.7 |
| 15 s | 188 km/h | 474 m | 6.4 |
| 20 s | 216 km/h | 756 m | 5.0 |
| 30 s | 256 km/h | 1.42 km | 3.2 |
| 40 s | 283 km/h | 2.17 km | 2.2 |
| 50 s | 301 km/h | 2.98 km | 1.5 |
| 60 s | 314 km/h | 3.84 km | 1.0 |
| 70 s | 323 km/h | 4.72 km | 0.7 |
| 90 s | 333 km/h | 6.55 km | 0.3 |
| 2 min | 339 km/h | 9.35 km | 0.1 |
| 3 min + | 342 km/h | 15 km | 0 |

Time-to-speed from standstill: **100 km/h 5.5 s, 200 km/h 17.0 s, 250 km/h 28.1 s, 300 km/h 49.1 s, 310 km/h 56.5 s, 320 km/h 66.6 s.** Time-to-speed starting at 100 km/h: 200 km/h 11.5 s, 250 km/h 22.6 s, 300 km/h 43.7 s.

Where it flattens: strong pull (19 km/h/s) to about 70 km/h, 13 km/h/s at 107, 6.4 at 188, 3.2 at 256, under 1 km/h/s from 300; the last 10 km/h below the ceiling take over a minute.

**Ceiling by grade** (where drive force = drag + rolling + grade):

| Grade | −4 % | −2 % | 0 % | +2 % | +4 % |
|---|---|---|---|---|---|
| Ceiling | 382 km/h | 362 km/h | 342 km/h | 323 km/h | 304 km/h |

- A car at the flat ceiling that drives a 2 km downhill at −4 % reaches about **363 km/h** at the bottom (355 km/h after 1 km); the 382 km/h is approached only on very long downhills. So the practical top is about 365 km/h, and the speed dial (2.6) and the sight rules (5.3, 5.4) are sized for 382 km/h.
- Downhill sections let the car go above the flat ceiling, so the top-speed record keeps moving with skill: catching a long downhill at full speed with a clean line.
- Uphill, coasting slows the car faster. On a −4 % downhill a coasting car settles at about 29 km/h; on −2 % it still comes to a stop.

**Coasting (accelerator up) on flat road from 339 km/h:**

| Coast time | 0 s | 2 s | 4 s | 6 s | 10 s | 15 s | 20 s | 30 s | 40 s | 60 s |
|---|---|---|---|---|---|---|---|---|---|---|
| Speed (km/h) | 339 | 329 | 319 | 310 | 292 | 271 | 253 | 220 | 192 | 145 |

Slowdown by speed: 5.3 km/h/s at 340, 4.4 at 300, 3.5 at 250, 2.7 at 200, 2.1 at 150, 1.7 at 100, 1.5 at 60. Coasting to a stop takes about 42 s from 60 km/h, 67 s from 100 km/h, 143 s from 300 km/h. No brake in the MVP, so coasting is the only way to slow before a tight curve. From 342 km/h, coasting takes **7.1 s and 0.64 km** to reach 307 km/h and 18 s and 1.5 km to reach 261 km/h (see 6.3).

**Minimum speed: none (user decision).** Knock-on rules:

- A stopped or slow car can hardly move sideways (side-speed limit `0.35 × v`). That is the natural penalty for going slow.
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

- **Top speed:** the highest speed reached in the run (km/h, one decimal, e.g. 341.4 km/h). 300 km/h needs about 50 s of clean driving; the flat ceiling is 342 km/h and a long downhill gives up to about 365 km/h, so the record stays open-ended.
- **Distance:** meters driven in the run, shown in km with two decimals (e.g. 4.37 km).

A run can set a new best in one, both, or neither.

### 2.4 Difficulty curve and run start

**Run start:** the car waits at standstill on the road with no traffic within 400 m. The run clock, the traffic spawner and the road phase all start on the **first accelerator press**. So the player is never hit before they start.

First seconds after that press (holding the accelerator):
- The first vehicles spawn at 400 m (view distance at low speed, see 5.3).
- A sedan at 130 km/h spawned at 400 m meets the player after about 7.3 s (player at about 123 km/h). A truck at 80 km/h meets them after about 9.0 s (141 km/h), a 180 km/h sedan after 6.1 s (108 km/h).
- So the player gets 6–9 s to get going before the first pass. Phase 1 traffic is sparse (below), and the corridor (4.3) starts at the player's own position.

Difficulty grows with **time**, not distance. The densities below include the user's +40 % (`DensityScale` 1.4). Traffic is set as **density** (vehicles per km of road, all 4 lanes). The player meets vehicles at density × closing speed, so driving faster means more vehicles per second.

| Phase | Time | Density (4 lanes) | Vehicles met per second at 100 / 300 km/h | Traffic | Road (see 6.5) |
|---|---|---|---|---|---|
| 1 | 0 – 20 s | 11 per km | about 0.7 / 1.3 | single vehicles, mostly cars | gentle |
| 2 | 20 – 60 s | 18 per km | about 1.1 / 2.1 | pairs; first buses and trucks | medium |
| 3 | 60 – 120 s | 28 per km | about 1.8 / 3.3 | side-by-side rows with one corridor; truck columns in lane 4 | hard |
| 4 | 120 s + | rises slowly to a cap of 45 per km | up to about 2.8 / 5.3 | many trucks and buses, tight gaps, fast cars in lane 1 | hardest |

- Per lane this is the same as v0.3 (density × 4/3). Phase 3 starts at 60 s, when a clean run is already above 300 km/h (300 km/h at 49 s), so the densest traffic meets the fastest player at the highest closing speeds.
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
| Side hit on a truck at full steering (9.8 m/s) | 9.8 m/s | 1.44 t | 14.1 | Game over |
| Side hit on a sedan at 5 m/s side speed | 5.0 m/s | 0.77 t | 3.9 | Survive, −31 km/h |
| Side hit on a sedan at full steering (9.8 m/s) | 9.8 m/s | 0.77 t | 7.6 | Game over |
| Head-on with a motorbike, player 100 km/h, bike 120 km/h | 61.1 m/s | 0.21 t | 13.1 | NonLethal: loss = clamp(105, 40 %, 60 %) = 60 km/h, 40 km/h left |
| Head-on with a motorbike, player 300 km/h, bike 200 km/h | 138.9 m/s | 0.21 t | 29.8 | NonLethal: loss = clamp(238, 120, 180) = 180 km/h, 120 km/h left |
| Side-swipe of a motorbike, player 200 km/h, 2 m/s side speed | 2.0 m/s | 0.21 t | 0.43 | NonLethal: loss = 40 % = 80 km/h, 120 km/h left |
| Rail, flat touch, 1 m/s side speed | 1.0 m/s | 1.5 t | 1.5 | Survive, −12 km/h |
| Rail, 3 m/s side speed | 3.0 m/s | 1.5 t | 4.5 | Survive, −36 km/h |
| Rail, 7 m/s side speed | 7.0 m/s | 1.5 t | 10.5 | Game over |
| Curve R 1000 m at 342 km/h (flat ceiling), full steering, 0.3 m/s net into the outer rail (6.3) | 0.3 m/s | 1.5 t | 0.5 | Survive, −4 km/h, then scrape until slow enough |
| Curve R 720 m at 342 km/h, full steering, 4.5 m/s net (6.3) | 4.5 m/s | 1.5 t | 6.8 | Game over |
| Curve R 600 m at 342 km/h, full steering, 7.4 m/s net (6.3) | 7.4 m/s | 1.5 t | 11.1 | Game over |

- Hitting the front of any vehicle is always game over (even stopped, at least 3× the threshold).
- Rail: survive below 4 m/s side speed, game over above.
- A truck or bus side hit ends the run above about 4.2–4.4 m/s side speed, a sedan above about 7.7 m/s, a compact above about 9.5 m/s. All classes are in 4.1.4. Motorbikes are `NonLethal` (below).

**Non-lethal vehicles: motorbikes (user decision, flag `NonLethal` in the class row)**

A contact with a motorbike, even head-on, is **never game over**. It costs a large part of the speed. Everything else (all other vehicles, rails, walls, parapets) keeps the impulse rule above.

- **Impulse** is still computed (`m_red` 0.21 t) and drives the feedback, but the game-over test is skipped for this class.
- **Slowdown:** `loss = clamp(8 km/h × J, 0.40 × v, 0.60 × v)` and `v_new = v − loss`. So the car always loses **40–60 % of its speed**: at least 40 % so it is never a free scratch, at most 60 % so the car is never stopped dead in the traffic stream (a stopped car would be killed by the next vehicle, which is game over by another way). `J` near the top of the range (hard head-on) gives 60 %, a gentle side touch 40 %. The loss is applied once at contact, never below 0.
- **The bike:** removed from traffic at once. A short debris burst (bike and rider as 4–6 simple pieces flung ahead and sideways, fading in 1.5 s, no collision, no ragdoll).
- **Push-off:** side speed 1.5 m/s away from the bike (`sign(x_player − x_bike)`; toward the road centre if the two are within 0.05 m). Steering works at once.
- **Feedback:** a strong camera shake at the level of the strongest survived hit (J = 6), 0.6 s, with a strong haptic pulse. It is a hit shake, so the 200 km/h throttle rule does not apply; game over still zeroes all shake (not triggered here).
- **No chaining:** a motorbike never spawns within **600 m** of another motorbike (road distance; 4.3 s at 500 km/h closing). Safety net: a second contact with a `NonLethal` vehicle within **4 s** of the first is game over (plowing through a group of riders).
- **Anti-exploit (farming bikes to brake in a tight curve):**
  - Bikes are 0 / 1 / 3 / 4 % of the traffic (at most about 1.8 per km, 4.1.2), 600 m apart, and ride at a random side offset of up to ±1.4 m, so the player cannot place a hit where one is needed.
  - A deliberate hit throws away 40–60 % of the speed, which takes **about 30–40 s** to rebuild (from 205 km/h to 300 km/h: about 31 s; from 137 km/h: about 40 s), against 7.1 s of coasting to slow from 342 to 307 km/h. Both records suffer: the run clock keeps density rising while the car is slow.
  - It stays what it is: a lucky escape that costs a lot, not a tactic.

### 2.6 HUD and game over screen

**During the run:** speed, large (`297 km/h`), on a dial with scale **0–400 km/h** (flat ceiling 342, practical downhill top about 365, theory 382, so the needle never pegs); distance (`4.37 km`); pause icon. Nothing else.

**Game over screen:** this run's top speed (one decimal) and distance; best top speed and best distance, each with a "New best" mark when beaten; a big Retry button. Retry puts the car back at standstill in under 1 s.

### 2.7 MVP feature list

1. Endless procedural Autobahn: 4 lanes, guardrails both sides, no shoulder (3.1, 6).
2. Road geometry: straights, clothoid curves, hills, overpasses, viaducts, tunnels (6). **Build order:** straights + curves → hills → overpasses → viaducts → tunnels, each playable before the next.
3. MVP map props from 3.2.
4. Chase camera, vehicle fade-in at the spawn distance, road drawn further (5).
5. Red Porsche 911 Carrera S (991) player car (decision 52).
6. Two-thumb touch controls, landscape only; keyboard for the editor.
7. Car force model with grade and tyre scrub (2.2). Start at 0 km/h, no cap, no minimum.
8. Curve drift (6.3).
9. 16 vehicle classes with 35 first-wave variants (4.1; the 7 old classes first, then the 9 new rows), side offsets and wander (4.2).
10. Spawning by density and phase, safe corridor (4.3), hidden-vehicle rule (5.5), sight rules per section (5.4).
11. Impulse collisions with vehicles, rails, walls, parapets; `NonLethal` motorbikes with the 40–60 % slowdown (2.5).
12. Records: top speed and distance, each best saved locally.
13. HUD and game over screen (2.6).
14. Sound: engine (pitch follows speed), passing vehicles (Doppler), tunnel echo, scrape, crash.

Test aids (decision 51): the `autodrive` flag file counts only for 10 minutes after its last write, the app deletes an older one at start, and a TEST mark shows bottom-left while it is active. Test scripts remove it when they end.

Not in MVP: menus beyond a start screen, settings, brake, gears, damage model, night, rain.

### 2.8 Later list

- Brake (for example, a second zone on the right half).
- Gears and an engine sound with shifts.
- The 27 second-wave variants of 4.1.3.
- Vehicles that change lanes, flash headlights or honk.
- Trucks that lean onto the lane line in curves.
- Crosswind gusts on viaducts (a short side push).
- Traffic vehicles that react to hits.
- Construction zones, exits and on-ramps.
- Day / night / rain.
- Settings: steering sensitivity, left-handed layout, sound, haptics.
- Slowing fast traffic in tight curves.
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

### 4.1 Classes and variants

**Two levels, to keep Core simple:**

- A **class** is what Core needs: a box (length, width), a height, a mass, an own-speed range, lane shares, a "tall" flag, spawn weights per phase and any special rule. One class = one row in the Core table.
- A **variant** is only a model and paint on top of a class. Same physics, same collision box. Core knows a variant only by its name, its class and its **share inside the class**. The only exceptions: police and ambulance variants carry their own speed range and lane shares (4.1.3), and the bike class has the `NonLethal` flag.
- Model fit: a variant's model should be within about 8 % of the class length (and within 5 % of the class width). The collision box is the class box, never the model. A model that does not fit goes to another class.
- Height is only used for the hiding rule (5.5), so a class height is the height of its tallest variant. **Tall** = class height ≥ 2.4 m.
- Speeds follow real Autobahn behaviour: trucks over 3.5 t are limited to 80 km/h (drivers go up to 90), coaches and buses 100 km/h, cars with caravan or trailer 80 km/h (100 with a permit), everything else is free. Mopeds, tractors and other slow vehicles are not allowed on the Autobahn and are not in the game.
- Paint: no red or orange traffic (decision 24); the 911 stays the only red car. Police: blue and silver. Ambulance: white with yellow-green markings.

#### 4.1.1 Class table

Lengths, widths and heights in m, mass in t. The first seven rows keep their old values (the developer already uses them), with two small changes: Bus height 3.8 → 4.0 (double-decker, used only for hiding) and Long van height 2.55 → 2.6 (ambulance).

| Class | Length | Width | Height | Mass | Own speed | Tall | Lane shares L1 / L2 / L3 / L4 (%) | Core row |
|---|---|---|---|---|---|---|---|---|
| Compact | 3.7 | 1.7 | 1.55 | 1.1 | 110–130 km/h | no | 0 / 34 / 33 / 33 | exists |
| Hatchback | 4.15 | 1.76 | 1.45 | 1.3 | 115–145 km/h | no | 10 / 30 / 30 / 30 | exists |
| Sedan | 4.8 | 1.85 | 1.5 | 1.6 | 130–180 km/h | no | 40 / 35 / 25 / 0 | exists |
| Van | 5.5 | 2.0 | 2.5 | 2.5 | 110–140 km/h | yes | 0 / 10 / 45 / 45 | exists |
| Long van | 5.9 | 2.0 | 2.6 | 3.0 | 100–130 km/h | yes | 0 / 10 / 45 / 45 | exists |
| Bus | 12.0 | 2.55 | 4.0 | 18 | 90–100 km/h | yes | 0 / 0 / 20 / 80 | exists |
| Semi truck | 16.5 | 2.55 | 4.0 | 35 | 80–90 km/h | yes | 0 / 0 / 20 / 80 | exists (called "Truck") |
| SUV | 4.9 | 1.92 | 1.8 | 2.1 | 120–160 km/h | no | 15 / 40 / 35 / 10 | **new row** |
| Sports car | 4.5 | 1.9 | 1.3 | 1.5 | 180–240 km/h | no | 90 / 10 / 0 / 0 | **new row** |
| MPV | 4.6 | 1.85 | 1.7 | 1.8 | 110–150 km/h | no | 0 / 30 / 40 / 30 | **new row** |
| Motorbike | 2.2 | 0.8 | 1.4 | 0.25 | 120–200 km/h | no | 25 / 25 / 25 / 25 | **new row** + `NonLethal` |
| Motorhome | 7.5 | 2.3 | 3.2 | 3.5 | 90–110 km/h | yes | 0 / 0 / 30 / 70 | **new row** |
| Car with caravan | 11.0 | 2.3 | 2.6 | 3.0 | 80–100 km/h | yes | 0 / 0 / 20 / 80 | **new row** |
| Rigid truck | 9.0 | 2.5 | 3.8 | 14 | 80–90 km/h | yes | 0 / 0 / 15 / 85 | **new row** |
| Long combination | 19.0 | 2.55 | 4.0 | 36 | 80–90 km/h | yes | 0 / 0 / 20 / 80 | **new row** |
| Articulated bus | 18.0 | 2.55 | 3.4 | 20 | 85–100 km/h | yes | 0 / 0 / 20 / 80 | **new row** |

Nine classes need a new Core row (SUV, Sports car, MPV, Motorbike, Motorhome, Car with caravan, Rigid truck, Long combination, Articulated bus). The seven old classes need no Core change except the two heights. Every variant in 4.1.3 is only a new model.

Mass notes: a class mass is a typical loaded value. Rigid trucks range 7.5–26 t, but `m_red` then varies only between 1.2 and 1.4 t against the player's 1.5 t, which changes no outcome in 2.5. The car with caravan uses the whole combination (1.8 t car + 1.2 t caravan); a head-on hit lands on the car, so this is a small over-estimate and fine.

#### 4.1.2 Spawn weights

Relative weight per phase. Each phase sums to **100**, so the density per km (2.4) is unchanged.

| Class | P1 | P2 | P3 | P4 |
|---|---|---|---|---|
| Compact | 14 | 10 | 6 | 4 |
| Hatchback | 22 | 17 | 12 | 8 |
| Sedan | 26 | 19 | 15 | 12 |
| SUV | 14 | 12 | 9 | 7 |
| Sports car | 2 | 2 | 4 | 6 |
| MPV | 6 | 5 | 4 | 3 |
| Van | 10 | 8 | 7 | 6 |
| Long van | 6 | 5 | 5 | 5 |
| Motorbike | 0 | 1 | 3 | 4 |
| Motorhome | 0 | 1 | 2 | 2 |
| Car with caravan | 0 | 3 | 4 | 3 |
| Rigid truck | 0 | 4 | 8 | 10 |
| Semi truck | 0 | 7 | 12 | 17 |
| Long combination | 0 | 1 | 3 | 4 |
| Bus | 0 | 4 | 5 | 7 |
| Articulated bus | 0 | 1 | 1 | 2 |
| **Sum** | **100** | **100** | **100** | **100** |
| of which heavy (rigid, semi, combination, bus, articulated) | 0 | 17 | 29 | 40 |
| of which tall | 16 | 34 | 47 | 56 |

- The heavy share is the same as before (trucks and buses only from phase 2: 0 / 17 / 29 / 40 %). Phase 1 has no heavy vehicle, motorbike, motorhome or caravan, so the first seconds stay simple.
- The tall share is higher than before because vans, motorhomes and caravans count; the hiding rule (5.5) covers all of them.
- The lane for a vehicle is picked by the lane shares in 4.1.1.

**Variant choice:** first the class by weight, then the variant by its share inside the class: **equal shares**, except police (5 % of Sedan) and ambulance (5 % of Long van). So police are about 1 % and ambulances about 0.3 % of all traffic.

#### 4.1.3 Variants (62)

★ = first wave (the designer sources these first, 35 in all). The rest come when time allows. A class works with a single variant, so the game is playable after wave one.

| Class | Variants |
|---|---|
| Compact (3.7 m) | ★ City car (500 / up! type), Microcar (Twingo / ForFour type) |
| Hatchback (4.15 m) | ★ Supermini (Polo / Corsa type), ★ Golf-class hatchback, Hot hatch |
| Sedan (4.8 m) | ★ Sedan (Passat / 3-series type), ★ Estate (wagon), Executive sedan (5-series / E-class), Liftback (Octavia / Arteon type), ★ Taxi (cream estate or sedan), ★ Police car (blue / silver; speed 150–200 km/h, lanes L1 / L2 only) |
| SUV (4.9 m) | ★ Mid SUV (X3 / GLC type), Compact SUV, ★ Large SUV (Touareg / X5 type), ★ Pickup (double cab), Pickup with hardtop |
| Sports car (4.5 m) | ★ Coupe, ★ Convertible, Roadster, GT |
| MPV (4.6 m) | ★ Minivan (Touran / Sharan type), MPV (Scenic / Zafira type) |
| Van (5.5 m) | ★ Panel van (Transporter / Vito type), ★ Delivery van, ★ Camper van (pop-top), Tradesman van with roof rack |
| Long van (5.9 m) | ★ Long panel van (Sprinter type), Minibus (9 seats), High-roof long van, ★ Ambulance (white / yellow-green; speed 130–160 km/h, lanes L1 / L2 only) |
| Motorbike (2.2 m) | ★ Sport bike, ★ Touring bike, Naked bike, Adventure bike, Cruiser |
| Motorhome (7.5 m) | ★ Coachbuilt motorhome, Integrated motorhome |
| Car with caravan (11 m) | ★ SUV with caravan, ★ Estate with caravan, Minivan with caravan |
| Rigid truck (9.0 m) | ★ Box truck (12 t), ★ Curtainsider, ★ Dump truck (3-axle), Flatbed with load, Beverage truck, Concrete mixer, Rigid tanker |
| Semi truck (16.5 m) | ★ Box semi, ★ Curtain semi, ★ Container semi, ★ Tanker semi, Flatbed semi, Reefer semi, Silo semi |
| Long combination (19 m) | ★ Swap-body combination (Wechselbrücke), ★ Car transporter, Curtain combination |
| Bus (12 m) | ★ Coach, Double-decker coach, ★ Line bus (intercity) |
| Articulated bus (18 m) | ★ Articulated line bus, Articulated coach |

Counting: 62 variants in 16 classes; 35 are first wave.

**Traffic and curves:** traffic does not slow in curves in the MVP: the tightest game curve (R 600 m) needs only 0.22 g at 130 km/h, which is normal driving. Fast vehicles (sports car at 240 km/h would need 0.75 g) are not slowed in curves in the MVP; slowing traffic in curves is on the later list.

**Player car:** Porsche 911 Carrera S (991), red (model by Mona × Supercars, CC BY 4.0). About 4.5 m long, 1.85 m wide with mirrors, 1.3 m high, 1.5 t.

**Special rules (table data, one code branch only):**

1. **`NonLethal` (Motorbike):** a contact never ends the run; it costs 40–60 % of the speed (2.5). One boolean in the class row.
2. **Speed and lane override (police, ambulance):** these two variants use their own speed range and lane shares instead of the class ones, so they come as occasional fast vehicles in the passing lanes. Two optional fields in the variant row. No siren or lights needed.
3. **Never twice in a row:** a variant (and its paint) does not follow itself in the same class (as decision 34).

#### 4.1.4 Impulse check for the new classes (2.5)

Player 1.5 t, threshold `J ≥ 6.0`. A side hit ends the run above the side speed `6.0 / m_red`. The max side speed is 9.8 m/s from about 100 km/h on.

| Class | m_red | Side hit ends the run above | At full steering (9.8 m/s) | Head-on, player stopped, slowest vehicle |
|---|---|---|---|---|
| Compact | 0.63 t | 9.5 m/s | game over (just) | J = 19.4, game over |
| Hatchback | 0.70 t | 8.6 m/s | game over | J = 22.2 |
| Sedan | 0.77 t | 7.7 m/s | game over | J = 28.0 |
| SUV | 0.88 t | 6.9 m/s | game over | J = 29.2 |
| Sports car | 0.75 t | 8.0 m/s | game over | J = 37.5 |
| MPV | 0.82 t | 7.3 m/s | game over | J = 25.0 |
| Van | 0.94 t | 6.4 m/s | game over | J = 28.6 |
| Long van | 1.00 t | 6.0 m/s | game over | J = 27.8 |
| Motorbike | 0.21 t | `NonLethal`: never game over | `NonLethal` | J = 7.1 (bike at 120 km/h), no game over: 40–60 % speed loss |
| Motorhome | 1.05 t | 5.7 m/s | game over | J = 26.2 |
| Car with caravan | 1.00 t | 6.0 m/s | game over | J = 22.2 |
| Rigid truck | 1.35 t | 4.4 m/s | game over | J = 30.1 |
| Semi truck | 1.44 t | 4.2 m/s | game over | J = 32.0 |
| Long combination | 1.44 t | 4.2 m/s | game over | J = 32.0 |
| Bus | 1.38 t | 4.3 m/s | game over | J = 34.6 |
| Articulated bus | 1.40 t | 4.3 m/s | game over | J = 32.9 |

- **Head-on is always game over, for every class except the `NonLethal` motorbike**, even with the player standing still and the slowest vehicle. Every car and truck is above J = 19 (3× the threshold). The bike has J = 7.1 at its slowest, but never ends the run.
- **Side hits:** for heavy classes a side hit ends the run above 4.2–4.4 m/s (about 45 % steering), so a truck or bus side-swipe is survivable only as a gentle touch. For light cars the limit is 6.9–9.5 m/s, which a hard lane change at speed reaches. This replaces the old line that compacts and sedans survive any side hit (written for the 7 m/s steering).
- Speed loss of a survived hit (`−8 km/h × J`, 2.5) is larger for heavy classes: a 2 m/s side-swipe of a semi loses 23 km/h, of a compact 10 km/h.

### 4.2 Side offset inside the lane

Each oncoming vehicle drives at a side offset `o(t)` from its lane center (measured along the road normal, so it works in curves too).

```
o_max = (3.75 − width) / 2 − 0.10 m        body never crosses the lane line (0.10 m margin)
o_base ~ normal(0, 0.5 × o_max), clamped to ±o_max     drawn once at spawn
A ~ uniform(0, 0.3 × o_max)                 wander amplitude
T ~ uniform(8, 15) s, φ ~ uniform(0, 2π)    wander period and phase
o(t) = clamp(o_base + A · sin(2π t / T + φ), −o_max, +o_max)
```

| Class | Width | o_max | Max wander amplitude A |
|---|---|---|---|
| Compact | 1.7 m | 0.92 m | 0.28 m |
| Hatchback | 1.76 m | 0.90 m | 0.27 m |
| Sedan, MPV | 1.85 m | 0.85 m | 0.26 m |
| SUV | 1.92 m | 0.82 m | 0.25 m |
| Sports car | 1.9 m | 0.83 m | 0.25 m |
| Van, Long van | 2.0 m | 0.78 m | 0.23 m |
| Motorhome, car with caravan | 2.3 m | 0.63 m | 0.19 m |
| Rigid truck | 2.5 m | 0.53 m | 0.16 m |
| Semi, long combination, bus, articulated bus | 2.55 m | 0.50 m | 0.15 m |
| Motorbike | 0.8 m | 1.38 m | 0.30 m (capped) |

- General cap: `A ≤ min(0.3 × o_max, 0.30 m)`. With `T ≥ 8 s` the wander speed `2πA/T` stays under 0.24 m/s for every class. Only the motorbike hits the 0.30 m cap (0.3 × o_max would be 0.41 m).
- A motorbike can ride close to a lane line, but the corridor (4.3) keeps `W_c / 2 = 1.425 m` from every vehicle body, bikes included, so no extra rule is needed.

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
- Vehicles in the same lane keep their speed (no overtaking). **Catch-up rule:** a faster vehicle may spawn behind a slower one in the same lane only if it cannot reach it before the slower one passes the player: `gap ≥ (v_fast − v_slow) × t_meet_slow + 20 m`, with `t_meet_slow` = meet time of the slower vehicle (4.3 above). Example, player at 200 km/h, spawn at 500 m: a 240 km/h sports car behind a 90 km/h truck needs a gap of about 280 m; behind a 130 km/h car about 190 m. If the gap does not fit, the spawner picks another lane or waits.
- Trucks and buses come in columns of 2–3 in lane 4 from phase 3.
- Gaps narrower than the corridor (threading, 4.2) appear naturally and are allowed; only the corridor is guaranteed.
- Motorbikes (`NonLethal`) keep at least 600 m of road distance to each other at spawn (2.5).

---

## 5. Camera and visibility

### 5.1 Chase camera

- About 6–7 m behind, 2.75 m high at standstill, looking slightly down the road. The developer's named constants are the truth.
- Horizontal field of view at least 75° (on a 20:9 phone, a vertical FOV of 50° gives about 92° horizontal). The speed FOV widens it from 80° to 92° with speed.
- **Speed camera (user decision, replaces the throttle camera of decision 45):** camera height, distance and pitch follow the **speed** through one smoothstep over 0–300 km/h with a low-pass of 0.6 s. At 300 km/h and above the camera is **0.5 m lower, 1.0 m closer and tilted 4.5° further down** (was 3°; the user wants the pitch range about 50 % larger, as an intended limit of the view at high speed). No accelerator-dependent camera code.
- At full effect the camera is 2.25 m high: still above cars (1.45 m), below trucks, so the blind spot behind trucks stays (5.5). The crest rule uses 2.25 m (5.4).
- To check on the tablet at 300 km/h: the horizon and the far road (vehicles at the spawn distance, up to 604 m, 5.3) must stay in frame with the extra pitch. If they do not, reduce the pitch range, not the view distance.
- The camera follows the car's side position with a small delay, and turns with the road tangent and the car's yaw.
- **Shake (user decision):** continuous camera shake and road vibration only while the accelerator is held and the speed is at least **180 km/h**, growing to full at **340 km/h** (about the flat ceiling) and staying full above it, scaled by `ShakeScale` **0.52**, eased in and out over 0.3 s. None at low speed, at steady speed without throttle, or when coasting. Hits and crashes still shake the camera; game over zeroes all shake.
- The camera sees over normal cars, not over buses and trucks (5.5).

### 5.2 Reaction-time rule

- Human reaction to a visual cue is 0.7–1.5 s; one lane takes 0.55 s, four lanes (11.25 m) about 1.6 s. The corridor moves at most 4 m/s, so the player never needs more than about 1 lane of move per second.
- **Rule: every vehicle is clearly readable at least 3 s before it meets the player.** Readable = inside the camera view, not hidden by terrain, walls or a tall vehicle, and fully faded in.

### 5.3 Spawn distance and draw distance

- **Vehicles spawn** at `max(400 m, closing speed × 3.5 s)` ahead along the road, **computed per vehicle** with its own speed: `closing = v_player + v_vehicle`. They fade in over the first 30 m. 3.5 s at spawn leaves 3 s after the fade.
- The fastest vehicle is the 240 km/h sports car (motorbikes and police 200 km/h). The player can reach 342 km/h on the flat, about 363 km/h after a long downhill and 382 km/h in theory. Spawn distance, with `closing = v_player + v_vehicle`:

| Player speed | vs 240 km/h sports car | Spawn distance | vs 180 km/h sedan (spawn distance) |
|---|---|---|---|
| 0 – 160 km/h | up to 400 km/h | 400 m | 400 m |
| 250 km/h | 490 km/h | 476 m | 418 m |
| 300 km/h | 540 km/h | 525 m | 467 m |
| 342 km/h (flat ceiling) | 582 km/h | 566 m | 508 m |
| 363 km/h (after a 2 km downhill) | 603 km/h | 586 m | 528 m |
| 382 km/h (theory) | 622 km/h | 604 m | 546 m |

- At 342 km/h against a 180 km/h sedan the closing speed is 522 km/h (145 m/s). The readable time after the 30 m fade-in is at least 3.0 s in every row.
- Slow vehicles (trucks at 80 km/h) spawn closer, but never under 400 m.
- The spawner must create vehicles up to about 610 m ahead, so the active zone is 610 m ahead of the player.

- **Road and terrain** are drawn to about 1.5 km with light distance fog (the developer may cut it to 1 km if the Tab S7 drops below 60 fps). Only vehicles fade in at the spawn distance. So curves, crests and tunnel portals are visible long before traffic, and the player can coast early enough (6.3).
- At the start (0 km/h) the spawn distance is 400 m and the warning time is 8 s or more. At 363 km/h the 1.5 km of drawn road is 14.9 s ahead.
- Daytime headlights on all oncoming vehicles are the early cue at high speed. To be checked on a real device.

### 5.4 Sight distance per section type

| Section | What could cut sight | How the 3 s rule is kept |
|---|---|---|
| Straight | nothing | spawn distance (5.3) |
| Curve | inner roadside, and the camera view angle | No sight-blocking objects (trees, walls, cuttings higher than 1 m) within 15 m of the inner rail in curves with R < 3000 m. Bearing of a vehicle at distance S on a curve is about S / (2R): at R 600 m and S 600 m it is 29°, inside the 37.5° half-FOV. |
| Crest | the hill itself | Crest radius **≥ 20 000 m** (the RAA minimum for the top design class is about 13 000 m). Sight over a crest with the speed camera at full effect (eye height 2.25 m) and a target height of 1.0 m: `S = (√(2·2.25) + √(2·1.0)) · √R` = **500 m** at R 20 000 m. Against the fastest vehicle that is 3.1 s at the flat ceiling (342 + 240 = 582 km/h), 3.0 s at the practical 363 km/h and 2.9 s only at the theoretical 382 km/h (rare: a crest taken at that speed needs a long downhill before it). At low speed (eye 2.75 m) it is 532 m. With R 22 000 m the margin would be 3.0 s even at 382 km/h; the developer already uses 20 000 m, so this stays unless a playtest shows blind crests. Extra rule: no side-by-side rows (only one corridor) within 300 m after a crest top, in all phases. |
| Sag (dip) | nothing by day | no rule |
| Tunnel | walls in a curve | Tunnels are **straight** (no curve, no crest inside) and lit, so sight = full length. No haze inside. Density inside × 0.7. 200 m straight before each portal so the portal and the traffic behind it are seen head-on. Camera exposure adapts over 0.5 s at portals; tunnel lights bright enough that vehicles stay readable during it. |
| Viaduct | nothing | Parapet on both sides = rail (infinite mass). No sight rule. |
| Overpass | short shadow | Shadow kept light (vehicles stay readable in it). No other rule. |
| Behind a tall vehicle | the vehicle | 5.5 |

Combination rules: no crest inside a curve with R < 1500 m; no tunnel portal within 500 m after a crest top.

### 5.5 Hidden vehicles behind tall vehicles

Every class with the **tall** flag (4.1.1: van, long van, motorhome, car with caravan, rigid truck, semi, long combination, bus, articulated bus) can hide what drives behind it in the same lane. About 16 % of phase 1 traffic and 56 % of phase 4 traffic is tall, so this rule does a lot of work.

- Behind a tall vehicle in the same lane, leave at least 1.5 s closing time (at the current player speed) before the next vehicle.
- A **light vehicle or a motorbike** right behind a tall one is the worst case (small, and fast ones close the gap): use 2.0 s for sports cars, police, ambulances and motorbikes behind a tall vehicle.
- Never let a tall vehicle hide a vehicle that borders the corridor.
- Not tall (cannot hide anything): Compact, Hatchback, Sedan, SUV, Sports car, MPV, Motorbike.

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
- Clothoid parameter `A = R / 2`, so `L = A² / R = R / 4` (RAA: A between R/3 and R). R 600 m → L 150 m; R 1000 m → 250 m; R 3000 m → 750 m.
- Total turning angle θ: 10° to 45°, random.
- **S-curves** (phase 3+): a curve followed directly by an opposite curve, clothoid to clothoid, with no straight between.
- **Straights** between curves: length by phase (6.5).

Design values (RAA, as recalled): top design class min radius about 720 m; the next class about 470 m. The game uses **600 m as the hard minimum** (it was 470 m until v0.7; at 342 km/h a 470 m curve left 0.5 s to react) and 720 m as the normal minimum.

### 6.3 Curve drift (user decision)

In a curve the car is pushed outward. The player must steer against it, and faster means more push.

```
c       = max(0, v² / R − g · q)       needed side acceleration, m/s²
          q = 0.04 superelevation (road tilted toward the inside, in all curves)
a_grip  = μ · g = 0.85 · 9.81 = 8.34 m/s²
s_hold  = c / a_grip                   steering needed just to hold the line
drift   = v_side_max × s_hold, pushing outward    (used in 2.2: target = v_side_max × s − drift)
```

- `s_hold < 1`: the player can hold the line and still dodge: inward up to `(1 − s_hold) × v_side_max` (9.8 m/s from 100 km/h on), outward easily.
- `s_hold = 1`: full steering only holds the line; no inward dodge.
- `s_hold > 1`: even full steering drifts outward at `(s_hold − 1) × 9.8 m/s` until the car hits the outer rail. The rail rule (2.5) decides: below 4 m/s side speed (`s_hold < 1.41`) it is a scrape that costs 15 km/h per second, which lowers `s_hold` until the car can hold again; above that it is game over.
- The car body visually turns with the road; the drift shows as the car sliding toward the outer rail.

**Speeds per curve radius (flat curve, q = 4 %, side speed 9.8 m/s):**

| R | Comfortable (s_hold 0.5) | Hold limit (s_hold 1.0) | Rail game over above (s_hold 1.41) | At the 342 km/h flat ceiling | At 363 km/h (after a long downhill) |
|---|---|---|---|---|---|
| 600 m | 188 km/h | 261 km/h | 307 km/h | s_hold 1.76, net 7.4 m/s → **game over** | s_hold 1.99 → **game over** |
| 720 m | 206 km/h | 285 km/h | 336 km/h | s_hold 1.46, net 4.5 m/s → **game over** | s_hold 1.65 → **game over** |
| 1000 m | 243 km/h | 336 km/h | 397 km/h | s_hold 1.04, net 0.3 m/s → light scrape | s_hold 1.17, net 1.7 m/s → scrape |
| 1500 m | 298 km/h | 412 km/h | 486 km/h | s_hold 0.67 → fine | 0.77 → fine |
| 2000 m | 344 km/h | 476 km/h | 561 km/h | 0.49 → fine | 0.56 → fine |
| 3000 m | 421 km/h | 583 km/h | 687 km/h | 0.31 → fine | 0.36 → fine |

So:
- **The sharpest curve (600 m) is fine at 188 km/h** and holdable up to 261 km/h. Above 307 km/h it ends the run unless the car slows first.
- **The ceiling is only safe on straights and gentle curves:** R ≥ 1500 m is comfortable at 342 km/h, R 1000 m just holdable, R 720 m and R 600 m are deadly.
- **Slowing in time:** from 342 km/h, coasting to 307 km/h (the survivable limit at R 600) takes 7.1 s and 0.64 km, to 336 km/h (R 720) 1.1 s and 0.11 km, to the hold limit 261 km/h 18 s and 1.5 km. The 1.5 km of drawn road is 15.8 s ahead at 342 km/h, so a curve under 1000 m (marked by chevrons, 3.2) is seen at least 8 s before the last moment to start coasting for R 600. From 363 km/h coasting to 307 km/h takes 10.8 s and 1.0 km, which still leaves 4 s.
- **Tyre scrub (2.2) helps in curves:** holding `s = s_hold` costs speed (about 3 km/h per second at `s_hold` 0.8, 4 km/h per second at full steering), so the car settles toward a holdable speed. The table above is the speed at curve entry; the speed through the curve drops from there.
- **Downhill + tight curve:** no curve with R < 1000 m on a downhill steeper than −2 %, since the car would arrive faster than the flat ceiling.
- **Curve after a downhill:** no curve with R < 1000 m starts within **1500 m after the end** of a downhill steeper than −2 %. From 363 km/h coasting to 307 km/h takes 1.0 km, so the player has the distance to slow, and the car is still above the flat ceiling at the bottom.

### 6.4 Vertical and structures

**Hills (vertical):**
- Grade between −4 % and +4 % (RAA top class max about 4 %).
- Grade changes through vertical curves: crest radius ≥ 20 000 m (5.4), sag radius ≥ 9 000 m (RAA about 8 800 m). Vertical curve length = R × |grade change|; e.g. +3 % to −3 % over a crest of R 20 000 m is 1 200 m long.
- Grade segments (constant grade): 400–2000 m.
- At 363 km/h, a crest of R 20 000 m gives v²/R = 0.51 m/s² (5 % of g), so the car never lifts off. No jumps.
- Grade feeds `F_grade` in the force model (2.2).

**Tunnels:**
- On straights only, never in the first 1.5 km of the run: each straight of 500 m or more has a 50 % chance (at least 700 m from any other structure). After 3 km of road without a tunnel the next straight is made at least 1400 m long and gets one. Spacing is random, about every 2.5–5 km.
- **Mound tunnel** (length 300–1500 m, no crest inside, 200 m straight before each portal, 5.4): a round bore, vertical walls up to 1.0 m on the rail lines, then a half-ellipse vault of 6.5 m rise (4.6 m clearance at the lane edge). The ground rises over 300 m to an 8.7 m soft hill (flat top, about 240 m wide, gone at 130 m from the road) so the mound is a long slope, not a lump. Before each portal the road runs in a cutting: concrete wing walls rising to 4.5 m over the last 40 m, grass slopes 1 : 2.5, an earth face around the concrete portal ring. Lamp poles along the last 150 m, a tunnel sign and an 80 km/h sign 170 m ahead, trees on the hill, a small control building beside some entry portals.
- **Underground tunnel** (length 800–1300 m, tried on half of the tunnel straights, about one per 10–15 km: the roofed middle lies on a straight, the open ramps may run into the curves around it; the terrain may not fall faster than 2 % on the entry ramp or rise faster than 2 % on the exit ramp): the road sinks 8 m below the terrain over 340 m (smoothstep, steepest 3.5 %, so at most 5.5 % with the terrain), then runs in a covered box (6.0 m clear height, walls 0.5 m outside the rails, roof slab) with ceiling lights, and rises again. The ramps are U-shaped trenches with concrete retaining walls and the guardrail; where the depth reaches 6.5 m a roof starts with a lintel. Fields, trees and houses continue on top; the terrain does not move. Sun, ambient light and echo follow the covered part (`Track.Covered`).
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
| Curve radius | 2000–5000 m | 1200–3000 m | 720–2000 m | 600–1500 m (20 % of curves under 720 m) |
| S-curves | no | no | 20 % of curves | 35 % of curves |
| Max grade | ±1 % | ±2.5 % | ±4 % | ±4 % |
| Overpasses | every 1–2 km | every 1–2 km | every 1–2 km | every 1–2 km |
| Viaducts | none | 1 per 5 km | 1 per 5 km | 1 per 4 km |
| Tunnels | 50 % of straights ≥ 500 m after 1.5 km, mound or underground | same | same | same |

- "1 per X km" = chance per km of road generated (e.g. 1/5 per km).
- In phase 1 the player covers about 0.9 km (from 0, holding the accelerator), so the first section is a straight of at least 800 m: the start is always straight and flat.
- A 3-minute run at about 230 km/h average covers about 11 km, so a typical good run sees 1–2 tunnels and 2 viaducts.

---

## 7. Open points

None in this document. Decisions taken after v0.4, and small points the planner decides later, are logged in `docs/decisions.md`.
