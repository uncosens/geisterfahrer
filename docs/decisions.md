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
| 26 | Corridor planning (developer) | Meet times use the current player speed; no re-plan after a 20 % speed change, no crest/curve limits on the corridor yet | Simplest version that keeps the 2.85 m gap at steady speed (tested); revisit if unfair gaps show up while accelerating |
| 27 | Road build (developer) | One generator for the whole run (curves, hills, tunnels, viaducts, overpasses all on from the start, by phase) instead of one build per feature | The view and Core share one road model, so splitting the build order would only add throwaway code |
| 28 | Road view (developer) | 100 m mesh chunks with one submesh per material, drawn 1.5 km ahead, ground follows the road height (no cuttings or embankments yet) | About 10 draw calls per chunk; terrain shaping can come later |
| 29 | Vehicle fade-in (developer) | Not done: vehicles appear at the spawn distance (400-495 m), where they are a few pixels wide in the haze | Fading needs transparent materials on every vehicle; check on the tablet first |
| 30 | Sounds (developer) | Engine, tyre, wind, whoosh, scrape, rumble and crash stay generated tones/noise; tunnel echo is an Audio Reverb Filter (StoneCorridor) | Freesound needs a login; the layers and their volume/pitch rules are in place for real clips |
| 31 | Body/scratch extras (developer) | Wheel spin blur, visible steering, scratch decals, dents and debris are skipped while vehicles are placeholders | They need the real models to look right |
| 32 | Slower acceleration (user, developer) | Power 260 → 140 kW, new drive traction limit μ_drive 0.60 (grip μ 0.85 kept for scrub and curve drift); drag, rolling and engine braking unchanged. Gives 0–100 in 5.7 s, 100–200 in 18.6 s, flat ceiling 247 km/h, 240 km/h after about 60 s | Two knobs found by simulation to hit the user's 6 s / 18 s targets; separate μ so curves do not get worse |
| 33 | Steering (user, developer) | Max side speed 7 → 9.8 m/s and limit 0.25 × v → 0.35 × v (both +40 %); return time constant 0.15 → 0.30 s; dead zone and curve drift rules unchanged. Sun 1.6 → 1.35 | Both limits scale so slow speeds also steer more; the drift is a share of side speed so a full-steer curve hold still balances |
| 34 | Traffic variety (user, developer) | Types Hatchback (Compact model at its natural 4.15 m) and Long van (Van model at 5.9 m); several files per type (`Compact2.glb` …) picked at random; 17 paint colours without red/orange, never the same as the last one of the type, repainted on pool reuse; bus and truck keep their livery | The fallback models need a Sketchfab login, so variety comes from the code path plus colours now and from extra files whenever the user adds them |
| 35 | Traffic density (user, developer) | `DensityScale` 1.4 on every phase (11 / 18 / 28 per km, cap 45) | One constant; the corridor rule still holds in the test, 60 fps checked on the tablet |
| 36 | Shake (user, developer) | Camera shake and edge-line vibration only with throttle held above 150 km/h, eased 0.3 s, scaled by speed and surface; hit and crash shake kept | Exactly the user's rule; impacts are not "road shake" |
| 37 | Tunnels (developer) | Hosted on straights ≥ 500 m after the first 500 m, 90 % each (50 % gave only one per 7 km), first one forced; no crest inside instead of constant grade; the first straight is 1500–2500 m | Cause of "no tunnels": the road is generated 2.5 km ahead in phase 1 and the tunnel chance was 0 in phases 1–2 |
| 38 | Colours (user, developer) | Overpass, tunnel and viaduct concrete `#7A7C80` matte; steel rails matte `#959BA1`; fields ×0.85 | The bright concrete and mirror-like metal hurt the eyes |
| 39 | Shake rule (developer2, replaces 36) | Continuous camera shake and road vibration only while throttle is held and speed ≥ 200 km/h, growing to 400 km/h, max scaled by `Shake.ShakeScale = 0.4`; edge-line rumble and its haptics use the same gate (rumble sound stays); game over zeroes all shake. Hit/crash shakes unchanged | User request; the pure rule lives in `Core/Shake.cs` and is unit tested |
| 40 | Vehicle shadows (developer2) | Real-time car shadows removed (casting and receiving off); soft blob quad under every car, stretched along the sun, plus tiny offset | Real shadows flickered with hard blocky edges; the blob is one transparent quad per car, no extra shadow pass cost |
| 41 | Vehicle fade-in (developer2) | Scale-in over 0.4 s (smoothstep) at spawn, blob scales with the car | Replaces decision 29's "not done": no transparent car materials, no extra draw cost |
| 42 | Wheels, steering, scratches (developer2) | Wheels found by node name and spun about their own centre; above 70 km/h rims swap to a flat dark disc (blur); player front wheels turn up to 25° with steering; light hit adds a scratch quad on the hit side (max 6) | No model change; the 911 has no separate steering wheel node, so the front wheels show the steering. Compact and Truck are one mesh, so they get no wheel spin |
