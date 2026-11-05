# Unity plan

Decision (from the user via the director): realistic 3D, chase camera, one Autobahn map, Unity 6.3 URP, Android, landscape only, personal use (no store release).
All game numbers come from `docs/plan/gdd-draft.md` v0.3. Docs only; no project exists yet.

## 1. Version and render settings

- Unity **6000.3.27f1** (6.3 LTS). Already installed with the Android module, same as `tower-bomber`.
- Template: **Universal 3D** (URP 17.3). Color space Linear.
- Packages: URP, Input System, uGUI, Test Framework. Remove unused template packages (Visual Scripting, Timeline, Multiplayer Center, Collab).

URP mobile asset (one quality level to start):

| Setting | Value | Why |
|---|---|---|
| Renderer | Forward (not Forward+ / Deferred) | one main light, cheapest on mobile |
| HDR | off | saves bandwidth; no bloom needed in MVP |
| MSAA | 2x | thin guardrails and lane marks shimmer without it |
| Render scale | 1.0 (drop to 0.85 if GPU bound) | |
| Main light shadows | on, 1024 atlas, 1 cascade, distance 60 m, hard | car and truck shadows sell "realistic"; far shadows are wasted |
| Additional lights | off (headlights are emissive quads) | |
| Post-processing | off in MVP | |
| SRP Batcher | on | main draw-call saver |
| Depth / opaque texture | off | not used |
| Fog | linear haze; far end moves with the spawn distance (GDD 5.2) | hides spawn and segment pop-in |

Player settings: app name `Geisterfahrer`, package ID `com.uncosens.geisterfahrer`, IL2CPP, ARM64, Vulkan first then OpenGLES3, `Application.targetFrameRate = 60`, **landscape only** (Left + Right auto-rotate), min API 29, target API 36.

## 2. Folder layout

```
Geisterfahrer/
  Assets/
    Scenes/Game.unity
    Scripts/
      Core/            # Geisterfahrer.Core.asmdef, noEngineReferences: pure C# rules
      Game/            # Geisterfahrer.Game.asmdef: MonoBehaviours
      Game/Editor/     # BuildScript.cs
    Data/              # VehicleDefinition and GameTuning assets
    Prefabs/Vehicles/  # one prefab per vehicle type + player
    Prefabs/Road/      # road segment, km post
    Art/               # models, materials, textures from the designer
    Audio/
    Settings/          # URP assets, input actions
    Tests/EditMode/    # tests for Core
  Packages/ ProjectSettings/
  docs/plan/ docs/design/ docs/dev/
  README.md
```

## 3. Scene

One scene, `Game.unity`. Three uGUI panels on one Canvas (landscape, scales with screen height):
- **Start:** title, Play, best top speed and best distance.
- **HUD:** km/h (large), distance in km, pause.
- **Game over:** this run's top speed and distance, the best of each, a "New best" mark per value, Retry.

Retry resets state and pools; no scene reload, so it restarts in under 1 s.

## 4. Core scripts

The player car stays at z = 0 and the world moves toward it (-z). No floating-point drift in endless runs.

Road (GDD 3.1), player's left to right: `outer rail | edge strip | lane 3 | lane 2 | lane 1 | edge strip | median rail`. Lanes 3.75 m, edge strips 0.5-0.75 m, about 12.5 m rail to rail. No hard shoulder.

| Script | Where | Job |
|---|---|---|
| `RunState` | Core | player x, forward speed, side speed, yaw; vehicle list (lane, z, length, width, mass, speed); run time; `Step(dt, steer, throttle)` |
| `CarModel` | Core | forward force model + arcade side motion (4a) |
| `Contacts` | Core | 2D oriented-box contacts, impulse, crash or push-off (4c) |
| `Spawner` | Core | density by run time (GDD 2.4), type weights by phase (GDD 4.1), fair-lane rule and hidden-vehicle gap (GDD 4.2, 5.3). Spawns at `max(400 m, closing × 3.5 s)` ahead (GDD 5.2). Works by time and closing speed, so traffic keeps coming when the player is stopped |
| `RunStats` | Core | this run's top speed and distance |
| `GameTuning` (ScriptableObject) | Game | all tuning numbers in one place (section 4d) |
| `GameLoop` | Game | owns `RunState`, calls `Step`, syncs views, switches panels |
| `PlayerInput` | Game | touch halves + keyboard, gives `steer` (-1..1) and `throttle` (0/1), section 4b |
| `PlayerCar` | Game | places the player model at x; look yaw = 2 × real yaw (max 6°), roll up to 3° |
| `RoadStreamer` | Game | 10 road segments of 50 m in a ring (500 m, covers the 470 m spawn distance); a segment behind the camera jumps to the front. One delineator pair per segment. Km post every 10th segment |
| `TrafficPool` | Game | one pool per vehicle type; spawn = take from pool (fade in at the haze), despawn behind camera = return |
| `ChaseCamera` | Game | about 6.5 m behind, 2.7 m high, follows x with a small delay, FOV widens with `speed / TopSpeed` in a fixed range |
| `Hud` | Game | km/h, km, crash and scrape feedback (shake scales with J) |
| `BestStats` | Game | best top speed and best distance in `PlayerPrefs` (two keys) |

No Unity physics: no Rigidbody, no colliders (see 4c). Rules stay testable in EditMode, like `tower-bomber`'s solver.

### 4a. Car model

**Forward speed from forces** (GDD 2.2), integrated per substep (semi-implicit Euler):

```
a = (F_drive - F_drag - F_roll - F_brake) / m
F_drive = min(P / max(v, 0.1), mu * m * g)   throttle held, else 0
F_drag  = 0.5 * rho * CdA * v²
F_roll  = Crr * m * g                        only while v > 0
F_brake = 400 N                              throttle up and v > 0
v = max(v, 0)                                no reverse
```

- At `v = 0` the traction limit wins, so there is no `P / v` blow-up and the car starts cleanly.
- No speed cap and no minimum speed. `TopSpeed` (306.2 km/h) is solved once from the constants (bisection on `F_drive = F_drag + F_roll`); camera FOV and spawn distance use it.
- Run starts at 100 km/h.

**Side motion (arcade, not tire physics):**
- `target = steer * min(7 m/s, 0.25 * v)`. A stopped car cannot move sideways.
- Side speed moves toward the target with time constant 0.12 s while steering, 0.15 s back to 0 when the finger lifts (`side += (target - side) * (1 - exp(-dt / tau))`). No pull to the lane center.
- `x += side * dt`.
- **Real yaw** `= atan2(side, v)` (0 when stopped, since side is 0). It rotates the player's collision box, so a steeper angle hits harder. The model shows 2 × that angle (look only).

Why not WheelCollider: tire physics at 300 km/h needs much tuning, can flip or jitter, and is hard to test. The GDD wants realistic forward speed but arcade side motion.

### 4b. Input (Input System + EnhancedTouch)

- `EnhancedTouchSupport.Enable()`; read `Touch.activeTouches` each frame.
- On `Began`, a finger gets its role by where it **started**: left half → steering, right half → throttle. It keeps the role until lifted, even across the middle. Track by `finger.index`. One finger per role; extra fingers are ignored.
- **Steering:** start point = origin. Offset in dp: `dp = px / (Screen.dpi / 160)` (fall back to 160 dpi if `Screen.dpi` is 0). Dead zone 10 dp, full steering at 75 dp, linear between, clamped to -1..1. The origin does not follow the finger. Lift → 0. New touch → new origin.
- **Throttle:** held → 1, lifted → 0.
- Keyboard (Editor): A/D or ←/→ = full steer while held, W or Space = throttle, Esc/P = pause, Enter = retry.
- Touches that start on a UI element (pause, panels) do not become steering or throttle.

### 4c. Collisions (impulse, our own SAT in `Core`)

Approved by the director; the GDD 2.5 wording matches.

Bodies on the road plane: the player box (4.5 × 1.85 m, rotated by real yaw), vehicle boxes (axis-aligned, sizes from GDD 4.1), two rails as lines with infinite mass. About 14 bodies at most, so a SAT test per pair costs nothing. Unity physics would need force-driven Rigidbodies that fight our model, and `Collision.impulse` depends on solver steps and frame timing, so the same hit would not always give the same result.

Per step:
1. **Substeps** so no box moves more than 0.5 m relative to another per substep (about 5 per frame at 486 km/h closing).
2. **Contact:** player vs vehicle: SAT on the 4 box axes; the smallest overlap gives normal `n` and depth. Rail: `n` = rail's inward normal, depth = deepest player corner past the rail line.
3. `v_n = (v_player - v_other) · n`, only when moving into each other. World velocities: player `(side, 0)`, vehicle `(0, -(v + own speed))`, rail `(0, 0)` with the rail moving at `-v` along itself (no effect on `n`).
4. `J = m_red * v_n` in t·m/s, `m_red = m_p * m_o / (m_p + m_o)`; rail: `m_red = m_p`. No bounce term.
5. **Result:**
   - `J >= 6.0` → game over.
   - Else, once at first contact: speed `-= 8 km/h * J` (not below 0), side speed set to 1.5 m/s away from the contact, push out by the depth.
   - While still touching (e.g. along a rail): speed `-= 15 km/h` per second (scrape), sparks and sound.
   - Traffic keeps its lane and speed.

### 4d. GameTuning constants (GDD v0.3)

| Name | Value |
|---|---|
| `PlayerMass` | 1500 kg |
| `Power` | 260 kW at the wheels |
| `Mu` | 0.85 |
| `CdA` | 0.65 m² |
| `AirDensity` | 1.225 kg/m³ |
| `Crr` | 0.012 |
| `EngineBrake` | 400 N |
| `StartSpeed` | 100 km/h |
| `MaxSideSpeed` / `SideSpeedPerSpeed` | 7 m/s / 0.25 |
| `SteerTau` / `RecenterTau` | 0.12 s / 0.15 s |
| `SteerDeadZoneDp` / `SteerMaxDp` | 10 / 75 dp |
| `LookYawFactor` / `MaxLookYaw` / `MaxRoll` | 2 / 6° / 3° |
| `CrashImpulse` | 6.0 t·m/s |
| `SpeedLossPerImpulse` | 8 km/h per t·m/s |
| `PushOffSideSpeed` | 1.5 m/s |
| `ScrapeSlowdown` | 15 km/h per s |
| `MinSpawnDistance` / `SpawnWarningTime` | 400 m / 3.5 s |
| `MaxStepMove` | 0.5 m |

Vehicle masses and sizes live in each `VehicleDefinition` (section 5).

## 5. Vehicle prefabs

`VehicleDefinition` ScriptableObject per type: id, prefab, length, width, height, mass, own speed range, lane bias, phase weights P1-P4 (all from GDD 4.1).

| MVP vehicle | L × W × H | Mass | Speed |
|---|---|---|---|
| Compact | 3.7 × 1.7 × 1.5 m | 1.1 t | 110-130 km/h |
| Sedan | 4.8 × 1.85 × 1.45 m | 1.6 t | 130-180 km/h |
| Van | 5.5 × 2.0 × 2.5 m | 2.5 t | 110-140 km/h |
| Bus | 12 × 2.55 × 3.8 m | 18 t | 90-100 km/h |
| Semi truck | 16.5 × 2.55 × 4.0 m | 35 t | 80-90 km/h |
| Player (911-like) | 4.5 × 1.85 × 1.3 m | 1.5 t | force model |

Prefab rules (so the designer can swap models later):
- 1 unit = 1 m. Root at the center of the footprint on the ground, +Z = vehicle front.
- Root has a `VehicleView` component; the model is a child called `Model`. Swap the child, keep the root.
- Game logic reads sizes and mass from the definition, not from the mesh.
- First version: placeholders from primitives (box body + box cabin + 4 cylinder wheels, one color per type), built to the sizes above.

## 6. Performance budget (60 fps)

Frame = 16.6 ms; aim CPU < 8 ms, GPU < 12 ms. Main device: Galaxy Tab S7.

| Item | Budget |
|---|---|
| Draw calls (batches) | < 100, SetPass < 30 |
| Triangles on screen | < 200k |
| Player car | < 20k tris |
| Traffic vehicle | 3-10k tris (truck up to 15k), LOD1 at about half beyond 80 m |
| Vehicles on screen | up to about 15 (25 per km at the cap, 470 m view) |
| Materials | one shared lit material per vehicle (atlas), road uses one tiled material |
| Textures | ASTC 6x6, max 1024² per vehicle, 2048² for road; total < 80 MB in memory |
| Total memory | < 400 MB |
| Battery | cap at 60 fps; no 120 fps |

Numbers are starting guesses. Measure on a device with the Profiler before calling anything fast or slow (same rule as `tower-bomber` `docs/PERF.md`).

## 7. Build size estimate

- Empty Unity 6 URP, IL2CPP, ARM64, release: about 20-25 MB.
- 6 vehicles + road + scenery with ASTC: about 10-20 MB. Audio (Vorbis): about 2 MB.
- **Total: about 35-50 MB APK**, installed directly with `adb` (no store).

## 8. Testing

**EditMode tests for `Core`**, batch:
`Unity.exe -batchmode -runTests -projectPath . -testPlatform EditMode -testResults test-results/editmode.xml`

Speed curve (GDD 2.2; checked with a quick simulation of these constants):
- 0-100 km/h in 3.6 s (±0.1), 100-200 km/h in 8.1 s (±0.1).
- Speed rises monotonically with throttle; approaches `TopSpeed` 306.2 km/h but never passes it.
- Coasting 303 → about 288 km/h after 2 s; coast decel larger at high speed than at low speed.
- Coasting from 100 km/h reaches 0 in about 62 s and stays at 0. No NaN at v = 0; throttle from 0 starts at the traction limit.

Side motion: stopped car does not move sideways; full steer at 100 km/h gives 7 m/s; real yaw = atan2(side, v).

Collisions, one test per row of the GDD 2.5 worked examples (J within ±0.1):

| Case | Expected J | Result |
|---|---|---|
| Head-on sedan, closing 190 km/h | 40.9 | game over |
| Head-on sedan, closing 60 km/h | 12.9 | game over |
| Stopped player vs truck at 80 km/h | 32.0 | game over |
| Stopped player vs compact at 110 km/h | 19.4 | game over |
| Truck side-swipe, 1 m/s | 1.4 | survive, -12 km/h |
| Truck side-swipe, 2 m/s | 2.9 | survive, -23 km/h |
| Truck side hit, 7 m/s | 10.1 | game over |
| Sedan side hit, 7 m/s | 5.4 | survive, -43 km/h |
| Rail, 1 m/s side speed | 1.5 | survive, -12 km/h |
| Rail, 3 m/s side speed | 4.5 | survive, -36 km/h |
| Rail, 7 m/s side speed | 10.5 | game over |

Plus: no tunneling at 486 km/h closing; push-off sets side speed to 1.5 m/s away; scrape removes 15 km/h per second while touching; spawn distance = 400 m up to 230 km/h and 470 m at the ceiling; fair-lane rule always leaves a reachable lane.

**On devices:**
- **Build** with a `BuildScript.BuildAndroid` like `tower-bomber`'s (`-batchmode -quit -executeMethod ...`), then `adb -s <serial> install -r`.
- **Emulator `rc4k`:** smoke test only (starts in landscape, UI, retry, no crash). Dev builds add `x86_64` so it runs natively. Graphics and fps on the emulator mean nothing.
- **Galaxy Tab S7 (SM-T870):** real play, two-thumb feel, Profiler attach, fps. It is faster than a typical mid phone, so its numbers are optimistic for other devices.
- Always pass `-s <serial>` to `adb`. Check 16 KB alignment of the `.so` files (`zipalign -c -P 16`).

## 9. Git ignore

Start from `tower-bomber`'s `.gitignore` and add build outputs:

```
/[Ll]ibrary/
/[Tt]emp/
/[Oo]bj/
/[Bb]uild/
/[Bb]uilds/
/[Ll]ogs/
/[Uu]ser[Ss]ettings/
/[Mm]emoryCaptures/
/[Rr]ecordings/
/test-results/
*.apk
*.aab
*.symbols.zip
.vs/
.idea/
.vscode/
*.csproj
*.sln
*.user
.utmp/
.DS_Store
Thumbs.db
```

Keep `.meta` files. Editor settings: Visible Meta Files, Force Text (Unity defaults).
Large binary art (FBX, PNG, WAV) through Git LFS (`.gitattributes`), if the user agrees.

## 10. Reuse from tower-bomber

- Same Unity version, Android module and license.
- `BuildScript` pattern (IL2CPP, ARM64, landscape, batch build, size log line).
- Pure C# `Core` assembly with `noEngineReferences` + EditMode tests.
- `.gitignore` and the `PerfRecorder` idea (fps and memory line in logcat).
- Not reused: its 2D renderer and 2D packages.

## 11. Needed from the user

Confirmed: `Geisterfahrer`, `com.uncosens.geisterfahrer`, landscape only, personal use.

1. Go-ahead to create the Unity project in this folder.
2. Git: GitHub repo `uncosens/geisterfahrer` (private?) and Git LFS for art, yes or no.
3. Unity Hub license still active on this PC (batch builds need it).
