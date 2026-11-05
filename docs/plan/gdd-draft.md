# Geisterfahrer — Game Design Draft (v0.3)

"Geisterfahrer" is German for a wrong-way driver ("ghost driver"). This draft describes the chosen concept and lists open questions.

Status: v0.3. Decided by the user:
- Concept A, one map (German Autobahn), realistic 3D, chase camera, Unity, Android, **landscape only**.
- **Personal use only** (the user plays it alone; no store release, no monetization, no online). So the player car is modeled directly on the **Porsche 911**, and real vehicle designs may be used. (Told by the user directly to the planner.)
- **No hard shoulder.** (Told by the user directly to the planner.)
- Two-thumb controls: left half steers with a floating origin, right half is the accelerator.
- **Impulse-based collisions** with vehicles and guardrails.
- **Real car force model** for speed, no top-speed cap.
- **Records = top speed and max distance** (no points, no near miss, no combo).

Changes from v0.2:
- Collisions: every hit is judged by impulse. Above a threshold = game over, below = slow down and get pushed off (2.5).
- Scoring: points, speed factor, near miss and combo removed. Two records per run: top speed and distance (2.3).
- Speed: force model (power, drag, rolling resistance, traction) instead of fixed acceleration steps; drag gives a natural ceiling of about 306 km/h (2.2).
- Hard shoulder removed; 3 lanes between the guardrails (3.1).
- No minimum speed: the car can coast to a stop (2.2, user decision). View distance grows with speed (5.2). Vehicle masses added (4.1).
- Open questions on brands, tone, monetization, online and goal closed by "personal use only".

Items marked **[Proposal]** are planner suggestions the user has not confirmed yet.

---

## 1. Concept

### Wrong Way (endless Autobahn dodge) — accepted

- **Pitch:** Drive a 911 against traffic on the Autobahn. How fast can you go, and how far can you get?
- **Core loop:** Drive → push your speed → dodge oncoming vehicles → crash → see top speed and distance → retry at once.
- **Controls:** Left thumb steers, right thumb holds the accelerator.
- **Session length:** 30 s – 3 min per run.
- **Platform:** Android, landscape, Unity, realistic 3D. Personal use.

Dropped earlier: "Ghost Run" (phasing car) and "Escape the Police" (city chase).

---

## 2. Wrong Way

### 2.1 Core rules

- 3D chase view: the camera follows behind and above the player car (see 5). The player car drives forward along an endless straight Autobahn.
- The player is on the wrong carriageway, so **all traffic is oncoming**.
- **3 lanes, no shoulder** (see 3.1). The player steers freely across the road between the two guardrails.
- Traffic drives in lanes at its own speed. The player sets their own speed with the accelerator; a simple car force model decides how fast it changes (see 2.2).
- Traffic gets denser over time, no matter how fast the player drives (see 2.4).
- Every collision is judged by impulse: a hard hit ends the run, a light one only slows the car (see 2.5).

### 2.2 Controls and driving model

Controls decided by the user. Numbers are first values to tune in playtests.

**Layout (landscape, multi-touch, both thumbs at once):**

| Screen part | Action |
|---|---|
| Left half | Steering with a floating origin |
| Right half | Accelerator (hold) |

No visible buttons for steering or gas. Optional feedback: a faint ring at the steering origin while the left thumb is down (no text).

**Steering (left half):**

- The first touch point is the center (origin). Drag left of it to steer left, right of it to steer right.
- **Dead zone:** 1.5 mm (about 10 dp) around the origin. No steering inside it, so a resting thumb does not drift.
- **Max offset:** 12 mm (about 75 dp) from the origin = full steering. Further drag stays at full steering. The origin does not follow the finger.
- Steering input `s` goes from -1 to +1, linear between the dead zone and the max offset.
- Lifting the finger = straight driving. Touching again sets a new origin at the new spot.
- If the touch starts on the left half and slides into the right half, it stays the steering finger until lifted.

(1 dp = 1/160 inch, about 0.16 mm. Writing it in dp makes it the same physical size on every phone and tablet.)

**How steering moves the car (arcade side motion, not tire physics):**

- Steering sets a sideways speed: `target side speed = s × max side speed`.
- **Max side speed:** 7 m/s, but never more than 0.25 × forward speed (so the car cannot slide sideways when slow; at 50 km/h it is 3.5 m/s). From 100 km/h up this is the full 7 m/s, so one lane (3.75 m) takes about 0.55 s.
- At the speed ceiling (306 km/h = 85 m/s), 7 m/s is a path angle of only 4.7°, so the limit still looks natural at the top. It stays fixed at high speed on purpose: the warning time gets shorter as speed rises, and dodging must stay possible.
- **Steering response:** the side speed moves toward the target with a time constant of 0.12 s.
- **Re-center:** when the finger lifts, the side speed returns to zero with a time constant of 0.15 s. The car keeps its side position. There is **no** automatic pull to the lane center.
- **Look:** the car body turns (yaw) by about twice the real angle of its path, max 6°, and leans (roll) up to 3° away from the turn. Wheels turn with `s`.
- Note: real tire grip (about 0.85 g) would make a lane change take about 1.3 s. That is too slow for this game, so side motion stays arcade on purpose while forward speed is realistic.

**Forward speed: car force model (user decision)**

No gears in the MVP. Each physics step:

```
a = (F_drive − F_drag − F_roll − F_brake) / m

F_drive = min(P / v, μ·m·g)      while the accelerator is held, else 0
F_drag  = 0.5 · ρ · CdA · v²
F_roll  = Crr · m · g
F_brake = 400 N                   engine braking, only while the accelerator is up
```

| Value | Number | Note |
|---|---|---|
| m | 1500 kg | 911-like |
| P | 260 kW at the wheels | 330 kW (450 hp) engine minus about 20 % drivetrain and gear-shift losses |
| μ | 0.85 | traction limit; power-limited above about 75 km/h |
| CdA | 0.65 m² | |
| ρ | 1.225 kg/m³ | air |
| Crr | 0.012 | |
| Engine braking | 400 N | gives a gentle slowdown at low speed |
| Start speed | 100 km/h | the run starts already moving |

Why 260 kW and μ 0.85, not 330 kW and 1.0: with the full 330 kW at the wheels and μ 1.0, the car did 0–100 km/h in 3.0 s and 100–200 km/h in 6.0 s, quicker than a real 911 Carrera S (about 3.5 s and 8–9 s). With 260 kW and 0.85 the numbers fit:

- 0–100 km/h: **3.6 s**
- 100–200 km/h: **8.1 s**
- Ceiling (drive force = drag + rolling): **306 km/h** (a real 911 Carrera S tops out at about 308 km/h)

**Holding the accelerator from the start of a run (100 km/h):**

| Run time | Speed | Acceleration then |
|---|---|---|
| 0 s | 100 km/h | 20 km/h/s |
| 3.0 s | 150 km/h | 13 km/h/s |
| 8.0 s | 200 km/h | 8 km/h/s |
| 16.9 s | 250 km/h | 4 km/h/s |
| 27.8 s | 280 km/h | 2 km/h/s |
| 34.8 s | 290 km/h | 1.4 km/h/s |
| 48.6 s | 300 km/h | 0.6 km/h/s |
| 58.2 s | 303 km/h | 0.25 km/h/s |
| 72.4 s | 305 km/h | 0.1 km/h/s |
| 2 min + | 306.2 km/h | about 0 |

Where the curve flattens: below 150 km/h the car pulls hard; around 250 km/h acceleration drops under 4 km/h/s; above 290 km/h each extra km/h takes several seconds; the last 1–2 km/h take minutes.

**Coasting (accelerator up) from near the ceiling (303 km/h):**

| Coast time | Speed |
|---|---|
| 0 s | 303 km/h |
| 2 s | 288 km/h |
| 4 s | 273 km/h |
| 6 s | 260 km/h |
| 10 s | 236 km/h |
| 15 s | 211 km/h |
| 20 s | 189 km/h |
| 30 s | 154 km/h |
| 40 s | 126 km/h |
| 60 s | 82 km/h |

Slowdown rate by speed: about 8 km/h/s at 300, 6 at 250, 4.3 at 200, 3 at 150, 2.1 at 100, 1.7 at 60. So letting go is a real brake at high speed and only a gentle roll at low speed. Getting back from 250 to 300 km/h takes about 30 s, so every coast and every hit (2.5) costs top-speed progress.

**Minimum speed: none (user decision)**

- Coasting and hits can slow the car all the way to a stop. Speed is clamped at 0: no reverse, no brake in the MVP.
- Engine braking and rolling resistance only act while the car moves (no backward push at 0).
- Coasting to a stop takes about 41 s from 60 km/h, 62 s from 100 km/h, and 113 s from 303 km/h.
- Holding the accelerator again pulls 0–100 km/h in 3.6 s.

**Knock-on rules at very low speed:**

- **Steering:** the side-speed limit `0.25 × forward speed` stays. A stopped car cannot move sideways, and a slow car moves sideways slowly (2 m/s at 30 km/h). That is how real cars work, and it is the natural penalty for going slow. No change needed.
- **Stopped car hit by traffic:** the impulse rule still decides. Any front hit on a stopped player is game over (truck at 80 km/h: J = 32, compact at 110 km/h: J = 19.4; see 2.5).
- **No safe parking spot:** the player car (1.85 m wide) does not fit next to any vehicle in any lane or edge strip. The widest free gap is about 1.35 m between a truck and the outer rail (0.6 m left in the lane + 0.75 m edge strip), and about 1.8 m for a compact. So a car standing still is always in the path of traffic sooner or later. Checked with the sizes in 3.1 and 4.1.
- **The fair-lane rule (4.2) protects only a moving player.** It promises a free lane that can be reached, and a stopped car cannot reach anything. That is fine: stopping is the player's choice.
- **Records:** a crawling or stopped player adds almost no distance and no top speed, and traffic density keeps rising with time. So crawling gains nothing and needs no extra rule.

**Late gain for records: not in the MVP [Proposal]**

With the ceiling at 306.2 km/h, a player who survives about 2 minutes at full throttle with no hits reaches it, and the top-speed record stops moving. For now that is fine: getting within 1 km/h of the ceiling needs over a minute of clean driving in dense traffic, and every hit or coast pushes the car far down. If playtests show the record gets stuck at the ceiling, add a small "engine warm-up" boost: +2 % power per minute of survival (no cap). Since the ceiling grows with the cube root of power, that adds only about +2 km/h per minute.

**Keyboard (Unity editor testing only):**

| Action | Keys |
|---|---|
| Steer left / right | `A` / `D` or `←` / `→` (full steering while held) |
| Accelerate | `W` or `Space` |
| Pause | `Esc` / `P` |
| Retry after crash | `Enter` |

No brake in the MVP.

### 2.3 Records

Decided by the user. There are no points.

Each run records two values:

- **Top speed:** the highest speed reached in the run (km/h, one decimal, e.g. 297.4 km/h).
- **Distance:** meters driven in the run, shown in km with two decimals (e.g. 4.37 km).

Each has its **own best**, saved on the device. A run can set a new best in one, both, or neither.

Both records need speed: distance grows faster at high speed, and traffic density depends on time, not distance. Top speed also needs a long clean run, since the last km/h come slowly.

### 2.4 Difficulty curve

Difficulty grows with **time**, not distance, so slow driving does not delay it. Traffic is set as **density** (vehicles per km of road, all 3 lanes). The player meets vehicles at a rate of density × closing speed, so driving faster means more vehicles per second.

All numbers are first guesses. Spawn mix per phase is in 4.1.

| Phase | Time | Density | Vehicles met per second at 100 / 250 km/h | Traffic |
|---|---|---|---|---|
| 1 | 0 – 20 s | 6 per km | about 0.4 / 0.6 | sparse, single vehicles, mostly cars |
| 2 | 20 – 60 s | 10 per km | about 0.6 / 1.0 | pairs; first buses and trucks |
| 3 | 60 – 120 s | 15 per km | about 0.9 / 1.5 | rows with only 1 free lane; truck columns in lane 3 |
| 4 | 120 s + | rises slowly to a cap of 25 per km | up to about 1.5 / 2.6 | many trucks and buses, tight gaps, fast cars in lane 1 |

- Density rises smoothly, not in jumps.
- After the cap, difficulty grows only through more long vehicles and fewer free rows.

### 2.5 Collisions

Decided by the user: every collision, with a vehicle or a guardrail, is judged by its impulse.

**Impulse:**

- `v_n` = closing speed along the contact normal (the part of the relative velocity that pushes the two bodies into each other), in m/s.
- `m_red = m_player × m_other / (m_player + m_other)`, in tonnes. Guardrail = infinite mass, so `m_red = m_player = 1.5 t`.
- `J = m_red × v_n`, in t·m/s (= kN·s). No bounce term (restitution 0), to keep it simple.
- The contact normal comes from our own C# collision code, not the Unity physics engine: 2D oriented boxes tested with SAT, the player box rotated by its real yaw, guardrails as lines with infinite mass. A front-to-front hit gives a normal along the road, a side touch gives a normal across the road. This is what makes angle matter.
- Traffic vehicles are not knocked off course in the MVP (they keep their lane and speed).

**Rule:**

| J | Result |
|---|---|
| `J ≥ 6.0 t·m/s` | **Game over.** Short slow motion, camera shake, game over screen. |
| `J < 6.0 t·m/s` | **Survive.** The car loses speed and is pushed off. |

- The threshold equals a speed change of 4 m/s (about 14 km/h) for the player car (`J / 1.5 t`). That is below the speed change where real airbags usually fire (roughly 20–25 km/h), so a survived hit stays believable as a scrape.

**Survived hit:**

- **Speed loss:** `−8 km/h × J` (with J in t·m/s), applied once at contact. Not below 0.
- **Push-off:** the player's side speed is set to 1.5 m/s away from the contact. Steering works again at once.
- **Scraping:** while the car keeps touching (for example sliding along the rail), it loses another 15 km/h per second. Sparks and scrape sound.
- Camera shake and haptics scale with J.

**Worked examples:**

| Case | v_n | m_red | J | Result |
|---|---|---|---|---|
| Head-on with a sedan, player at 60 km/h, sedan at 130 km/h (closing 190 km/h) | 52.8 m/s | 0.77 t | 40.9 | Game over (7× threshold) |
| Same, if only 60 km/h closing speed (lower bound) | 16.7 m/s | 0.77 t | 12.9 | Game over (2×) |
| Head-on, slowest possible case: player stopped vs truck at 80 km/h | 22.2 m/s | 1.44 t | 32.0 | Game over |
| Head-on, lightest vehicle: player stopped vs compact at 110 km/h | 30.6 m/s | 0.63 t | 19.4 | Game over |
| Side-swipe of a truck, 1 m/s side speed | 1.0 m/s | 1.44 t | 1.4 | Survive, −12 km/h |
| Side-swipe of a truck, 2 m/s side speed | 2.0 m/s | 1.44 t | 2.9 | Survive, −23 km/h |
| Side hit on a truck at full steering (7 m/s) | 7.0 m/s | 1.44 t | 10.1 | Game over |
| Side hit on a sedan at full steering (7 m/s) | 7.0 m/s | 0.77 t | 5.4 | Survive, −43 km/h |
| Rail, flat touch: 1 m/s side speed (0.3° at 200 km/h) | 1.0 m/s | 1.5 t | 1.5 | Survive, −12 km/h |
| Rail, medium: 3 m/s side speed (3.4° at 180 km/h) | 3.0 m/s | 1.5 t | 4.5 | Survive, −36 km/h |
| Rail, steep: 7 m/s side speed (14° at 100 km/h) | 7.0 m/s | 1.5 t | 10.5 | Game over |

What this means in play:

- **Hitting the front of any vehicle is always game over.** Even with the player stopped, the lowest closing speed is 80 km/h (a truck), which gives over 3× the threshold. Only a corner clip where the contact normal points almost fully sideways can survive, and that is fine.
- **Rail:** the rail is parallel to the road, so only side speed counts. Survive below 4 m/s side speed, game over above. At the same forward speed, a steeper angle means more side speed: a glancing touch slows, a hard steer into the rail ends the run.
- **Heavy vehicles are less forgiving from the side:** a truck or bus side hit ends the run above about 4.2 m/s side speed, a compact or sedan only above about 7.8 m/s (more than the max side speed). Physically right: a heavy truck does not give way.

### 2.6 HUD and game over screen

**During the run (landscape, no text labels beyond units):**

- Speed, large: `297 km/h`.
- Distance: `4.37 km`.
- Pause icon.
- Nothing else. The kilometer posts on the road match the distance read-out.

**Game over screen:**

- This run: top speed (one decimal) and distance.
- Best top speed and best distance, each with a "New best" mark when this run beat it.
- A big Retry button. Retry restarts in under 1 s.

### 2.7 MVP feature list (smallest playable)

1. One map: straight Autobahn carriageway, 3 lanes, guardrails on both sides, no shoulder, endless.
2. MVP map props from 3.2: lane marks, guardrails, median, delineator posts, kilometer posts, simple roadside.
3. Chase camera behind the player car, haze at the view distance, view distance grows with speed (see 5).
4. Player car modeled on the Porsche 911.
5. Two-thumb touch controls: floating-origin steering (left), accelerator (right), multi-touch, landscape only. Keyboard for the editor.
6. Car force model for forward speed (2.2): power, traction, drag, rolling resistance, engine braking. No cap, no gears.
7. 5 oncoming vehicle types: compact, sedan, van, bus, truck (see 4.1), real-world sizes, speeds and masses.
8. Spawn by density and phase weights, trucks and buses mostly in lane 3, fair rules from 4.2 and 5.3.
9. Impulse collisions with vehicles and guardrails: game over above the threshold, slow down and push off below it, scraping.
10. Records: top speed and distance per run, each best saved locally.
11. HUD and game over screen as in 2.6.
12. Basic sound: engine (pitch follows speed), passing vehicles (Doppler), scrape, crash.

Not in MVP: menus beyond a start screen, settings, unlocks, brake, gears, damage model.

### 2.8 Later list

- Brake (for example, a second zone on the right half).
- Gears and an engine sound with shifts.
- "Engine warm-up" power boost, only if top-speed records get stuck (2.2).
- More vehicles from 4.1 (motorbike, sports car, caravan, car transporter, police).
- Vehicles that change lanes, flash headlights or honk at the player.
- Traffic vehicles that react to hits (swerve, spin).
- Map props marked "later" in 3.2 (gantry signs, overpasses, construction zone, tunnel).
- Gentle curves and hills (they cut the view distance, so they need their own fair rules).
- Day / night / rain on the same map (night: headlights are the main warning).
- Settings: steering sensitivity (max offset), left-handed layout (swap halves), sound, haptics.
- Other 911 colors or variants.

---

## 3. Map: Autobahn

### 3.1 Layout

Germany drives on the right. The player drives the wrong way on the traffic's carriageway, so the traffic's right side is the player's **left**. From the player's view, left to right:

`outer guardrail | edge strip | lane 3 (slow lane) | lane 2 | lane 1 (fast lane) | edge strip | median guardrail`

- **No hard shoulder** (user decision). Some real Autobahn sections have none either (for example where the shoulder was turned into a lane).
- Edge strips are narrow (about 0.5–0.75 m) between the solid edge line and the guardrail.
- Trucks and buses stay mostly in lane 3 (German keep-right rule; trucks may not overtake on many sections). Fast cars use lane 1 next to the median.
- Lane width 3.75 m. Road width from guardrail to guardrail is about 12.5 m.

**Why 3 lanes, not 4:**
- Real Autobahn sections are mostly 2–3 lanes per direction.
- In a landscape chase view, 3 lanes fit the screen, so the road next to the player stays visible.
- 3 lanes with long vehicles is already hard: one truck column blocks a lane for a long time.

### 3.2 What the map has

| Element | Notes | MVP / Later |
|---|---|---|
| Asphalt, 3 lanes | Grey, slight texture and wear | MVP |
| Lane marks | Dashed white lines between lanes (German pattern: 6 m line, 12 m gap); solid white edge lines | MVP |
| Outer guardrail (Leitplanke) | Metal rail on the player's left; collision surface (2.5) | MVP |
| Median | Metal guardrail + grass strip on the player's right; collision surface; the other carriageway with sparse traffic in the distance is optional | MVP (simple) |
| Delineator posts (Leitpfosten) | White posts with black band, every 50 m; strong sense of speed | MVP |
| Kilometer posts | Small sign with km number every 500 m; matches the distance read-out | MVP |
| Roadside | Grass, fields, tree lines beyond the guardrail | MVP (simple) |
| Blue direction signs | Overhead gantry with blue signs, city names and exit numbers; seen from behind, since the player drives the wrong way | Later |
| Overpasses | Bridge crossing over the road, shadow on the asphalt | Later |
| Emergency phone (orange SOS post) | Small prop behind the guardrail | Later |
| Noise walls | Replace roadside in some stretches | Later |
| "End of speed limit" sign | White round sign with black stripes (seen from behind) | Later |
| Exit / on-ramps | Lane splitting off; vehicles merging in | Later |
| Construction zone (Baustelle) | Yellow lines, narrowed lanes, cones; a hard event | Later |
| Tunnel | Dark stretch, lights | Later |

Props beyond the guardrails are scenery only. Signs face the traffic, so the player sees most of them from behind. That is a nice "wrong way" detail.

---

## 4. Vehicle roster

### 4.1 Table

All vehicles fit one lane (lane 3.75 m) and drive at the lane center. Sizes and masses are real-world values; the designer builds models to this scale. Real vehicle designs may be used (personal use). Speed is the vehicle's own speed and does not depend on the player.

Spawn weights are relative per phase (higher = more often). 0 = never.

| Vehicle | Length | Width | Height | Mass | Own speed | Lane bias | P1 | P2 | P3 | P4 | MVP |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Compact car (Kleinwagen) | 3.7 m | 1.7 m | 1.5 m | 1.1 t | 110–130 km/h | any | 40 | 30 | 20 | 15 | MVP |
| Sedan (Limousine) | 4.8 m | 1.85 m | 1.45 m | 1.6 t | 130–180 km/h | lanes 1–2 | 40 | 35 | 30 | 25 | MVP |
| Van (Transporter) | 5.5 m | 2.0 m | 2.5 m | 2.5 t | 110–140 km/h | lanes 2–3 | 20 | 20 | 20 | 20 | MVP |
| Bus (Reisebus) | 12 m | 2.55 m | 3.8 m | 18 t | 90–100 km/h | lane 3, sometimes 2 | 0 | 8 | 12 | 15 | MVP |
| Truck, semi (Sattelzug) | 16.5 m | 2.55 m | 4.0 m | 35 t | 80–90 km/h | lane 3, sometimes 2 | 0 | 10 | 18 | 25 | MVP |
| Motorbike (with rider) | 2.2 m | 0.8 m | 1.4 m | 0.25 t | 120–200 km/h | any | 0 | 0 | 5 | 8 | Later |
| Sports car | 4.5 m | 1.9 m | 1.3 m | 1.5 t | 200–250 km/h | lane 1 | 0 | 0 | 5 | 10 | Later |
| Car with caravan (Wohnwagen) | 11 m | 2.3 m | 2.6 m | 3.0 t | 80–100 km/h | lane 3 | 0 | 5 | 8 | 8 | Later |
| Car transporter | 20 m | 2.55 m | 4.0 m | 30 t | 80 km/h | lane 3 | 0 | 0 | 5 | 10 | Later |
| Police car | 4.8 m | 1.85 m | 1.5 m | 1.7 t | 150–200 km/h | any, may change lane | 0 | 0 | 3 | 5 | Later |

**Player car:** Porsche 911 (992 generation as the reference): about 4.5 m long, 1.85 m wide, 1.3 m high, **1.5 t**.

Real-world limits used as a guide: German truck speed limit 80 km/h, coach buses 100 km/h, EU max semi-trailer length 16.5 m, max width 2.55 m, max height 4.0 m, max truck mass 40 t (35 t taken as a typical loaded truck).

Note for later: with the motorbike's low mass, side hits are almost always survivable for the car under 2.5. Decide then whether that is OK.

### 4.2 Rules

- **Fair-lane rule:** at every point along the road, at least one lane is open for long enough to be reached. A lane counts as blocked for the whole length of a vehicle plus a safety gap. Reaching any lane from any other takes at most about 1.1 s (7.5 m at 7 m/s, lane 3 to lane 1), which fits inside the 3 s warning time (see 5.2).
- Trucks and buses come in columns of 2–3 in lane 3 from phase 3.
- Vehicles in the same lane keep their speed in the MVP (no overtaking). A faster vehicle never spawns right behind a slower one in the same lane, so they never overlap.

---

## 5. Camera and visibility

### 5.1 Chase camera

- Behind and above the player car: about 6–7 m behind, 2.5–3 m high, looking slightly down the road. Starting values to tune.
- The camera follows the car's side position with a small delay, so the car visibly moves inside the frame when steering.
- Field of view widens a little as speed rises (speed feel), within a fixed range.
- The camera height lets the player see over normal cars, but not over buses and trucks (see 5.3).

### 5.2 View distance and fair reaction time

- Human reaction to a visual cue is about 0.7–1.5 s; crossing one lane takes about 0.55 s. So the player needs **at least 3 s** from "vehicle clearly readable" to impact.
- At the speed ceiling the closing speed is about 306 + 180 = 486 km/h (135 m/s). A fixed 400 m would give only about 2.7 s readable time there, so the view distance grows with speed:
- **Rule:** vehicles spawn at `max(400 m, closing speed × 3.5 s)` ahead, at the edge of a light haze, and fade in. The haze distance moves with the spawn distance. The readable warning time stays at about 3 s or more at any speed.

| Player speed | Max closing (vs 180 km/h sedan) | Spawn distance |
|---|---|---|
| 0 – 230 km/h | up to 410 km/h | 400 m |
| 250 km/h | 430 km/h | 420 m |
| 280 km/h | 460 km/h | 450 m |
| 306 km/h (ceiling) | 486 km/h | 470 m |

- A sedan at 470 m is only a few pixels wide on a phone, so daytime headlights on all oncoming vehicles are the main early cue at high speed. To be checked on a real device.
- If playtests show the top speeds are unreadable, raise the view distance first; lowering power (and so the ceiling) is the fallback.

### 5.3 Hidden vehicles behind trucks and buses

In 3D, a tall vehicle (van, bus, truck) hides what drives behind it in the same lane. To keep it fair:

- Behind a tall vehicle in the same lane, leave a gap of at least 1.5 s closing time (at the current player speed) before the next vehicle. So when a truck passes, the player still has time to see what comes next in that lane.
- Never let a tall vehicle hide the next vehicle in the only free lane.

---

## 6. Open questions for the user

1. **Late gain [Proposal]:** no power boost in the MVP, add the "engine warm-up" boost only if top-speed records get stuck (see 2.2). OK?
2. **Language:** UI text is almost only numbers and units. Any words (e.g. "New best", "Retry") in English or Korean?
