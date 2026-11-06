# Director decisions (made while the user was away)

The user asked the director to decide open points alone and report at the end. Each line can be changed later.

| # | Topic | Decision | Why |
|---|---|---|---|
| 1 | Run start (GDD 2.4) | Accepted: clock, traffic and road phase start on the first accelerator press | Player is never hit before starting |
| 2 | Draw distance (GDD 5.3) | Accepted: road and terrain to about 1.5 km, vehicles fade in at spawn distance. Developer may cut to 1 km if the Tab S7 drops below 60 fps | Curves and crests must be seen early to slow down |
| 3 | Curve chevron boards (GDD 3.2) | Accepted on curves under 1000 m | Real Autobahn has them; cheap early warning |
| 4 | Crest radius (GDD 5.4) | Accepted: 15 000 m | Keeps the 3 s rule downhill |
| 5 | UI language | Icons only where possible; any leftover text in English | User prefers no text; English matches the German-themed map better than mixed text |
| 6 | Build order (GDD 2.7) | Accepted: straights + curves, then hills, overpasses, viaducts, tunnels | Each step playable before the next |
| 7 | Lane choice per vehicle (GDD 4.1) | Accepted as written | Matches the German keep-right rule |
| 8 | Downhill + tight curve (GDD 6.3) | Accepted: no R < 1000 m on grades steeper than −2 % | Avoids unfair speed into tight curves |
| 9 | Player paint (designer) | `#C8102E`, Guards Red-like, metallic 0.55, smoothness 0.85 | User asked for a red 911; tune by eye against photos |
| 10 | Danger feedback (designer) | Light hit = amber `#FFB020` edge; crash = white flash + grey-out; no red in UI or feedback | Red now means "my car", so danger must use another hue and shape |
| 11 | Speedometer (designer) | Round 911-style dial top-right, 0–330 km/h, white needle, digital km/h inside; pause moves top-left | User request; white needle because red is the car; 330 covers the 327 km/h downhill ceiling |
| 12 | Post-processing (designer) | One LDR Volume on: Color Adjustments, White Balance, Vignette; no bloom/HDR | Tunnel exposure and crash grey-out need it; about one LUT pass |
| 13 | Skybox (designer) | Poly Haven HDRI Dreifaltigkeitsberg | German clear-day sky |
| 14 | Lane dash phase (designer) | Dashes placed by road distance `d mod 18 < 6`, not per segment | No jumps at segment seams on straights or curves |
| 15 | Fog vs draw distance (designer) | Fog 300–1,600 m, far clip 1,650 m; vehicles fade in on their own at the spawn distance | Matches GDD 5.3 (road to 1.5 km); fog no longer hides spawning |
| 16 | Camera FOV and height (designer) | Horizontal FOV 80° → 92° with speed (min 75°), height fixed 2.7 m, distance 6.5 → 7.2 m | Meets GDD 5.1 on every ratio; keeps the crest sight rule |
| 17 | Signs seen from behind (designer) | Grey backs with a white edge frame; km posts double-sided | The player drives against traffic; km number must stay readable |
| 18 | Chevron boards (designer) | 0.5 × 0.75 m boards every 20 m on the outer side of curves R < 1000 m | Row of pale backs reads from about 400 m, enough to coast |
| 19 | Tunnel look (designer) | Walls on the rail lines, no walkway, fog off inside, entry/exit lights every 4 m for 100 m, exposure adapts in 0.5 s | Follows GDD 5.4 and 6.4; vehicles stay readable |
| 20 | Viaduct edge (designer) | 0.9 m concrete parapet with W-beam on the rail line, then cap and thin railing; no props on the deck | GDD 6.4 collision line; low parapet keeps the valley view |
| 21 | Overpass shadow (designer) | Deck casts no real-time shadow; a baked shadow quad at alpha 0.20 instead | GDD 5.4 wants a light shadow so vehicles stay readable |
| 22 | Driving-feel set (designer) | On: speed FOV, throttle kick, camera lag/roll, speed shake, body pitch/roll, wheel blur swap, visible steering, speed streaks from 160 km/h, aniso 4×, all sound layers. Off: radial blur, motion blur, chromatic aberration, heat haze, lens dirt, crash smoke | About +0.5 ms GPU on the Tab S7; the off items cost more than they add |
| 23 | Free models (designer) | Use the first pick per slot in `vehicle-assets.md`; switch the 911 to the Carrera 4S fallback only if the first pick looks too simple on screen | Least cleanup; stays inside the polygon budget |
| 24 | Traffic paint (designer) | No red or orange traffic; add dark blue `#24324A` to the mix | Keeps the red car unique; more colour variety |
| 25 | Road surface (user) | Photo asphalt dropped; generated calm surface: #55575C, soft tyre tracks, ±3.5 % slow variation, 60 m repeat, no normal map | The photo grain and 2 m repeat strained the eyes while driving |
