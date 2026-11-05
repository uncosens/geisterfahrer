# Art Direction — Realistic 3D (chosen)

Decision: the user chose a realistic version of candidate b. The game uses a 3D chase camera and is built in Unity 6.3 URP for Android phones and tablets. One map: a German Autobahn by day. The game is for the user's **personal use only** (no store release), so real vehicle designs are allowed: the player car is a Porsche 911 (992). Vehicles: see `vehicle-assets.md`.

Goal: it should look like a real motorway on a clear day. Realism comes from good light, believable materials and correct proportions. It does not come from heavy effects that mobile GPUs can't afford.

---

## 1. Camera

| Setting | Value | Note |
|---|---|---|
| Position | 5.5 m behind, 1.9 m above the car's centre | Car fills about the lower 1/4 of the screen |
| Look-at | 12 m ahead of the car, 0.8 m high | Horizon at about 40 % from the top |
| Horizontal FOV | locked at **75°** | Unity uses vertical FOV; compute it from the aspect ratio each frame |
| Speed FOV | +6° at top speed, eased | Gives a sense of speed cheaply |
| Follow | position damped (~0.15 s), yaw damped more (~0.3 s), no roll | Steering feels responsive without nausea |
| Near / far clip | 0.3 m / 600 m | Fog hides the far end |
| Shake | small on a light hit, strong on a crash (see section 7) | Taste setting; scaled by percent |

**Orientation: landscape only** (user decision). Both landscape rotations are allowed; portrait is not supported.
- The controls split the screen into left (steer) and right (throttle) halves. That is the natural two-thumb landscape grip.
- A chase camera needs width to show all 3 lanes and both guardrails (about 12.5 m).

**Ratios.** Locking the horizontal FOV keeps all lanes visible on every device:
- 20:9 phones: same width of road, less sky.
- 16:10 and 4:3 tablets: same road, more sky and more road ahead.
- The HUD is anchored to the safe area. In landscape, a notch or punch-hole can sit on the left or right edge.

## 2. Lighting

- **Time:** mid-morning, clear sky, a few high clouds.
- **Sun:** about 40° elevation. Azimuth about 30° to the side of the driving direction, slightly behind the camera. Cars then get a lit side, a shaded side and a diagonal shadow, and the camera never looks straight into the sun.
- **Sun colour and intensity:** warm white, about 5500 K. Ambient comes from the sky.
- **Static scenery** (road, guardrails, bridges, signs, trees): baked lightmaps (GPU lightmapper) plus baked light probes.
- **Moving vehicles:** one realtime directional light.
  - Shadows: 2 cascades, shadow distance 60 m on high tier and 40 m on mid. Hard shadows on mid and low.
  - Low tier: blob or decal shadows under the cars instead of realtime shadows.
- **Reflections:**
  - One baked reflection probe, or the sky cubemap, for car paint and glass.
  - A small set of box probes under overpasses so cars darken there.
  - No realtime reflection probes.
- **Sky:** HDRI skybox of a clear day (Poly Haven, CC0). The same HDRI drives ambient light.
- **Fog:** linear distance fog, light blue-grey, starting at about 250 m. It hides the far clip and adds air.

## 3. Post-processing (mobile budget)

| Effect | High | Mid | Low |
|---|---|---|---|
| Tonemapping (Neutral or ACES) | on | on | on |
| Colour adjustments (slight contrast and saturation) | on | on | on |
| Bloom (low intensity, for sun glints and lamps) | on, low res | off | off |
| Vignette (very subtle) | on | on | off |
| Anti-aliasing | MSAA 4x | MSAA 2x | FXAA |
| Render scale | 1.0 | 0.85 | 0.7 |
| SSAO, depth of field, motion blur, SSR | off | off | off |

Target: a stable 60 fps on high, and 30 fps minimum on low. The speed feel comes from FOV, camera shake, wind particles and road texture flow, not motion blur.

## 4. Road and scenery

The cross-section follows GDD v0.3: **no hard shoulder**, 3 lanes, about 12.5 m from rail to rail. It stays parametric so the lane count can change.

Seen from the player (oncoming traffic drives toward the camera):

```
outer guardrail | edge strip 0.5–0.75 m | lane 3 | lane 2 | lane 1 | edge strip 0.5–0.75 m | median guardrail
```

| Element | Spec | Material |
|---|---|---|
| Lanes | 3 × 3.75 m | Asphalt PBR, tiling 2K. Darker worn tire tracks in each lane centre. Faint patch repairs. |
| Edge strips | 0.5–0.75 m between edge line and guardrail, on both sides | Same asphalt, slightly lighter, less worn, a little grit near the rail |
| Lane lines | white, 0.15 m wide, **6 m dash / 12 m gap** | Separate thin mesh strips, slightly raised, merged into one mesh per road segment |
| Edge lines | white solid, 0.30 m (not verified, see sources) | As above |
| Guardrails | German W-profile steel, galvanised, posts every 2 m, on both outer edges and both sides of the median | Metallic, mid roughness, light grime at the bottom |
| Reflector posts | white posts with a black band, every 50 m on the verge | Unlit-ish, small reflector |
| Median | ~3 m grass between two guardrails, low bushes | Grass texture, billboard bushes |
| Verge | grass slope, then trees or fields; optional noise walls near towns | Tree impostors beyond 80 m |
| Blue signs | RAL 5017-like blue `#0E518D`, white border. On gantries and on posts on the verge. | Personal-use game, so real German town names are fine. Use an OFL DIN-style face (D-DIN). |
| Overpasses | concrete bridges every 1–2 km, with piers in the median and the verge | Concrete PBR, baked shadow on the road below |
| Road segments | 120 m straight or gently curved pieces, recycled (endless road) | One material atlas for all scenery props |

**Texture sources:** CC0 asphalt, concrete and grass from Poly Haven or ambientCG. Use ASTC compression: 6×6 for colour and 4×4 for normal maps.

## 5. HUD over 3D

> Exact layout, sizes and colours now live in `ui-spec.md` (it wins over this section). Speedometer style **a** is chosen.

User decision: no points, no near miss, no combo. The HUD shows only **speed (km/h)**, **distance (km)** and pause. No hint text, no tutorial lines, no visible joystick.

Fixed meaning colours:

| Meaning | Colour | Where |
|---|---|---|
| Player | blue `#3D7BFF` | Player car paint base (metallic, so it shifts with light) and primary buttons |
| Crash / danger | red `#FF4D4F` | Light-hit edge flicker and crash flash (section 7), game over accent |
| Good / new best | green `#2FD27A` | Proposal: with near miss gone, green marks a **new best** on the game-over screen |

**Layout (landscape):**

```
+--------------------------------------------------------------+
| [speedo]               12.4 km                        ( || ) |
|                                                              |
|                         (3D scene)                           |
|                                                              |
|   left half: steering           |   right half: throttle     |
|   (no HUD here)                 |   (no HUD here)            |
+--------------------------------------------------------------+
```

- **Distance:** top centre, white, bold, tabular figures, one decimal (`12.4 km`), with a soft dark shadow so it reads over sky and road.
- **Speed:** top left (styles a/b below).
- **Pause:** top right, round, 44 dp touch target, semi-transparent dark fill.
- **Both lower thumb areas stay free of buttons.** Touches anywhere in the lower part go to the controls.
- **Steering touch feedback:** a faint white ring (about 15 % opacity, 64 dp) at the first touch point, shown only while touching. A small dot inside it follows the finger horizontally.
- **Throttle:** no visual. The speedometer shows the effect.
- **Game over:**
  - The scene freezes on the crash and dims to about 70 %. There is no blur, because blur costs too much on mobile.
  - Centred, two equal columns with icons instead of words:

    ```
        ⟶  distance           ⏲  top speed
         12.4 km               231 km/h
        🏆 18.9 km            🏆 254 km/h
              [ ↻ ]   [ ⌂ ]
    ```

  - Top row: this run, big numbers. Second row: best of each, smaller, with a trophy icon. If this run set a new best, that number is green and its best row shows the same value.
  - Buttons: retry `↻` (filled blue) and home `⌂` (outline), equal square 64 dp.
  - Icons: a road/arrow icon for distance and a gauge icon for top speed. There are no text labels besides the units.
- **Font:** Inter (OFL) for all HUD numbers.

### Speedometer: two styles (pick one)

Both styles sit **top-left**, mirroring the pause button. In landscape the lower corners belong to the thumbs, so the speedometer goes at the top. The unit `km/h` is the only label. On a light hit the number dips visibly, which is part of the hit feedback.

**a — Digital number**
- A big number (e.g. `187`) with a small `km/h` beside it.
- A thin 2 dp bar under the number fills with speed from 0 to max.
- White on the same soft shadow as the distance.
- Smallest and cleanest. It matches the distance number at top centre.

```
 187 km/h
 ▬▬▬▬▬▬▬▬▬▬▬▬────
```

**b — Arc gauge**
- A 240° arc ring (about 72 dp) that fills clockwise with speed. The number sits in the middle and `km/h` sits under it.
- The track is white at 20 % and the fill is white.
- Reads faster at a glance and feels more like a car, but it takes more space.

```
   ╭───╮
  ╱ 187 ╲
  ╲ km/h╱
```

Designer's pick: **a**. It stays minimal, takes little space at the top edge and keeps the HUD calm. Neither style uses red at high speed, because red is reserved for danger.

## 6. Performance targets (art side)

- Visible triangles: under about 500k on high and 250k on low.
- Draw calls: under about 150. Batch static scenery, use GPU instancing for traffic and props.
- Textures:
  - Vehicles: 1024² max per vehicle (dev plan §6), shared atlases where possible.
  - Scenery: one 2K atlas plus a tiling asphalt texture.
- LOD groups on every vehicle and prop. Traffic is culled beyond about 300 m, hidden by fog.

## 7. Collision feedback

The game grades hits by impact (the planner and developer own the thresholds). The visuals have two clearly different levels, so the player learns at once whether they survived.

| | **Light hit** (car slows, run goes on) | **Heavy hit** (steep or hard, run ends) |
|---|---|---|
| Sparks | Short burst (0.2–0.4 s) of 15–30 stretched orange-white spark particles at the contact point, trailing backwards | Large burst (60–100) plus a few flying debris chunks (glass bits, a trim piece) |
| Sound (for the dev/sound brief) | Metal scrape, length matches contact time | Loud impact, glass break, then tire skid fade |
| Camera | Small shake (≈ 0.1 s, low amplitude), no FOV change | Strong shake, then a 0.6–1.0 s slow-motion beat. The camera keeps following, then holds as the car spins or stops. |
| Car body | A **scratch decal** appears on the hit side and stays for the run. After 3+ light hits the side looks visibly battered. | A **dent** (see below), bonnet or bumper shifted. The car spins or rolls out by physics. |
| Screen | Thin red `#FF4D4F` edge flicker on the hit side only, 0.15 s, about 25 % opacity | Full red edge flash, then fade to the game-over dim |
| Speed | The speed number dips at once (the visible "cost" of the hit) | Speed drops to 0. Game over shows after the slow-motion beat. |
| Haptics | Short light vibration | One long strong vibration |
| Other car | A small scratch decal and a little wobble; it keeps driving | It is pushed or spun by physics and stops; smoke puff from the bonnet on high tier |

**Cheap damage on mobile (no damage-variant models needed):**
- **Scratches:** a few pooled decal projectors (URP Decal, Screen Space technique on mobile), or a scratch mask painted into a second UV. Light hits only.
- **Dents:** on the player car only, move vertices near the hit point inward on the CPU at the moment of impact, once per hit. A mesh under 20k triangles deforms in well under a frame. Use a small falloff radius and clamp the depth, so it never looks broken.
- **Loose parts:** if the model has separate doors, bonnet and bumpers (the Realistic Mobile Car series lists separate doors), a heavy hit can swing the bonnet open or drop a bumper using a rigidbody. This is optional and goes on high tier only.
- **Glass:** swap the window material to a cracked-glass texture on a heavy hit.
- **Traffic:** scratch decals only, no mesh deformation (cost and pooling).

## Sources

- Lane line 6 m dash / 12 m gap / 0.15 m: [Bissantz, "draw as measured"](https://www.bissantz.de/en/bella-consults/draw-as-measured/) (secondary) and Austrian [BoMaVO § 5](https://www.jusline.at/gesetz/bomavo/paragraf/5) (same values).
- Edge line 0.30 m appears only in an [Autobahn GmbH tender notice](https://auftrag.ai/notices/markierung-los-4-herstellung-von-typ-ii-endmarkierungen-ca-18500-m-15500-m-1200-m-5300-m-1200-m). Verify it against the German RMS ([umwelt-online index](https://umwelt-online.de/recht/bau/strassen/rms_in.htm)).
- Camera, lighting and budget numbers are the designer's starting values, not measured. Tune them on a real device (the Galaxy Tab S7 plus one mid-range phone).
