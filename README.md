<picture>
  <source media="(prefers-color-scheme: dark)" srcset="docs/design/logo/logo-horizontal-dark.svg">
  <img alt="Geisterfahrer" src="docs/design/logo/logo-horizontal.svg" height="64">
</picture>

# Geisterfahrer

Endless wrong-way driving on the German Autobahn. Unity 6.3 (URP), Android, landscape. Personal use.

- Game rules: [docs/plan/gdd-draft.md](docs/plan/gdd-draft.md) (final v0.4); decisions taken while the user was away: [docs/decisions.md](docs/decisions.md)
- Tech plan: [docs/dev/unity-plan.md](docs/dev/unity-plan.md)
- Art direction and UI: [docs/design/](docs/design/)
- Third-party assets: [CREDITS.md](CREDITS.md)

## Layout

| Folder | Content |
|---|---|
| `Assets/Scripts/Core` | Pure C# rules, no Unity references: car force model (scrub, grade), curve drift, contacts and impulse, traffic with wander and the safe corridor, procedural `Track` |
| `Assets/Scripts/Game` | Unity side: `GameLoop` (views, camera, driving feel, sound layers), `Hud` (uGUI and 911 gauge per `docs/design/ui-spec.md`), `TrackView` (road mesh chunks), `Feedback` (sparks, shake, haptics), `DriveInput`, placeholders, sounds |
| `Assets/Scripts/Game/Editor` | `BuildScript`: `Setup` (materials, fonts, sky, post, scene) and the Android build; `TrackShots` renders road features to `docs/dev/shots/track-*.png` |
| `Assets/Tests/EditMode` | Tests for Core and for the input logic |
| `tools/fetch-assets.sh` | Downloads the free sky, textures and fonts into the git-ignored `Assets/ThirdParty/` |

All tuning numbers are in `Core/Tuning.cs` (editable on the `GameLoop` component). Lanes, vehicle sizes, masses and spawn weights are in `Core/Road.cs`.

Core works in road space (distance along the road, lateral offset). Only `TrackView.Place` maps it to the world along the generated centre line.

Vehicle models are the `.glb` files in `Assets/ThirdParty/Vehicles/` (hand downloads, see CREDITS.md); without them the game uses primitive placeholders. `Placeholder.Fit` turns each model's long axis to z, scales it to the real length, grounds it, recolours its paint and glass, and flips models listed in `GameLoop.Flipped`.

## Attribution

The vehicle models are free Sketchfab models under **CC BY 4.0** (https://creativecommons.org/licenses/by/4.0/); the licence was read from each file's own metadata. Changes: scaled, recoloured (body paint), the player's wheels split into separate parts.

| Model | Author |
|---|---|
| Porsche 911 Carrera S (991), the player's car | Mona x Supercars (https://sketchfab.com/Car2022) |
| 1998 Volkswagen Golf Mk4 | ImperialBlue3D (https://sketchfab.com/ImperialBlue3D) |
| Low Poly BMW M5 F90 | kulonee (https://sketchfab.com/ynesolefru) |
| BMW 320i E90 Low Polly | rifdanzz (https://sketchfab.com/rifdanzz) |
| Mercedes Sprinter Passenger | kuwalol93 (https://sketchfab.com/kuwalol93) |
| VW Transporter Multivan Green | LEON elenaisakova248 (https://sketchfab.com/elenaisakova248) |
| Coach bus | own.guest (https://sketchfab.com/own.guest) |
| 1998 Hyundai Aero Space Coach Bus | ImperialBlue (https://sketchfab.com/ImperialBlue3D) |
| Mercedes Actros with lowboy trailer | LuizZed36 (https://sketchfab.com/LuizZed36) |

Sky "Dreifaltigkeitsberg" (Poly Haven) and the grass texture (ambientCG) are CC0. Fonts Inter and Barlow Semi Condensed are SIL OFL 1.1. Full links in [CREDITS.md](CREDITS.md). Vehicle names and designs belong to their owners; this project is non-commercial and not affiliated with any manufacturer.

## Build

Unity 6000.3.27f1 with the Android module. From the project folder:

```
bash tools/fetch-assets.sh
"C:\Program Files\Unity\Hub\Editor\6000.3.27f1\Editor\Unity.exe" -batchmode -quit -projectPath . -executeMethod Geisterfahrer.Editor.BuildScript.Setup -logFile Logs/setup.log
"C:\Program Files\Unity\Hub\Editor\6000.3.27f1\Editor\Unity.exe" -batchmode -runTests -projectPath . -testPlatform EditMode -testResults test-results/editmode.xml
"C:\Program Files\Unity\Hub\Editor\6000.3.27f1\Editor\Unity.exe" -batchmode -quit -projectPath . -buildTarget Android -executeMethod Geisterfahrer.Editor.BuildScript.BuildAndroid -logFile Logs/build-android.log
adb -s <serial> install -r Builds/Android/geisterfahrer-dev.apk
```

`Setup` rebuilds `Assets/Scenes/Game.unity` and the generated materials and fonts (menu: Geisterfahrer > Setup Scene and Assets). Run it again after adding models.
Add `-release` to the build line for a non-development APK (`Builds/Android/geisterfahrer.apk`).
Dev builds log `PERF fps … worst … ms` every 5 s: `adb -s <serial> logcat -s Unity | findstr PERF`.

Controls: left half = steer (drag from where you touch), right half = accelerate. Editor: A/D or arrows, W or Space, Esc/P pause, Enter play/retry.

## Checked (2026-11-07, playtest changes, GDD v0.5)

| What | Where | Result |
|---|---|---|
| 55 EditMode tests: slower power model (0-100 5.7 s, 100-200 18.6 s, ceiling 247 km/h), +40 % steering, slower return, +40 % traffic, tunnel within 4 km for 30 seeds, two-finger input logic | batch mode | all pass |
| Red 911, grey overpass and tunnel concrete, tunnel entry / inside / exit (`tab-tunnel-*.png`), traffic in many colours and 7 types (`tab-traffic.png`) | Galaxy Tab S7 | works |
| Tap on Play does not start the run; the run starts on the first gas press (`RUNSTART` log) | Galaxy Tab S7 | works |
| Shake only with gas held (decision 39: from 200 km/h, replaced my 150 km/h rule; `shake` field of the PERF log, measured before the merge with the 150 km/h rule) | Galaxy Tab S7 | 0.0 at low speed and when coasting, 1.0 above the threshold with gas |
| Autopilot runs, up to 215 km/h, tunnels, 3-7 cars on screen; stress run with 3.5 x traffic: 10-25 cars on screen | Galaxy Tab S7, dev build | 60.0 fps in every 5 s sample, worst frame 16.9 ms (blips of 33-42 ms only at a crash restart), 87-93 MB |

Not checked: two real thumbs at once on glass (adb injects one finger; the logic is covered by `InputTests`), vibration and sound by ear, the paint tint on the Van model by eye.

Dev tools: a file `autodrive` in `/sdcard/Android/data/com.uncosens.geisterfahrer/files/` makes a dev build drive itself (text `density=3.5` and `immortal` inside change the traffic and make hits harmless). Remove the file to play by hand. The flag counts for 10 minutes after the file's last write (the app deletes an older one at start; `adb shell touch` it to renew), and a TEST mark shows on screen while it is active. Tests must remove it in a `finally` / at the end and check with `adb -s <serial> shell ls /sdcard/Android/data/com.uncosens.geisterfahrer/files/`. `adb -s <serial> logcat -s Unity` shows `PERF` and `RUNSTART` lines.

Extra model files for variety: `Compact2.glb`, `Sedan2.glb`, `Van2.glb`, `Bus2.glb`, `Truck2.glb` in `Assets/ThirdParty/Vehicles/` are picked at random next to the first ones (fallback picks in `docs/design/vehicle-assets.md`; Sketchfab needs a login, so not fetched here).

## Checked (2026-11-07, GDD v0.4)

| What | Where | Result |
|---|---|---|
| 42 EditMode tests: v0.3 set plus tyre scrub table, grade ceilings, curve drift at R 470/720/1000, safe corridor, wander, run start | batch mode | all pass |
| Curve, crest, viaduct, tunnel portal and inside, overpass | `TrackShots` renders | look right (`docs/dev/shots/track-*.png`) |
| Start at standstill, red 911 model facing forward, gauge needle and digits, distance, traffic models, crash grey-out, game over | emulator `rc4k` (`-force-gles`) | works (`v04-*.png`) |

Not checked this round: the Galaxy Tab S7 was not connected, so no fps or Profiler numbers for the new road and effects, no two-thumb test, no sound check by ear.

## Checked (2026-11-06, art pass)

| What | Where | Result |
|---|---|---|
| 27 EditMode tests (4 lanes, start at 0 km/h) | batch mode | all pass |
| HDRI sky, asphalt and grass textures, fields, guardrail posts, delineators, km post with number, red player car | Galaxy Tab S7 | works |
| uGUI start, HUD (speed number and bar, distance, pause), steering ring, game over with best rows, Home and Retry | Galaxy Tab S7 | works |
| Rail scrape sparks, crash sparks, shake, slow motion, edge flash | Galaxy Tab S7 | works (vibration not checked by hand) |
| Frame rate at full throttle, up to 194 km/h | Galaxy Tab S7, dev build, `PERF` log | 60.0 fps, worst frame 16.9 ms, 77 MB managed memory |

Screenshots: `docs/dev/shots/art-*.png`.

## Checked (2026-11-05, MVP)

| What | Where | Result |
|---|---|---|
| Speed curve, 11 GDD collision examples, scrape, no tunneling, fair-lane rule | batch mode | all pass |
| Start, Play, accelerate, head-on crash, game over | emulator `rc4k` (ARM translation, `-force-gles`) | works |
| Rail hit and scrape, crash, Retry, best records kept | Galaxy Tab S7 (Vulkan) | works |

Not verified:
- Two thumbs at once (steer + accelerate): `adb input` sends one finger only. Needs a hand test.
- Sound by ear; vibration; frame rate on a mid-range phone (the Tab S7 is faster).
- Real vehicle models (not downloaded yet).
- On the emulator, Vulkan crashes inside the emulator's driver. Start it with `adb shell am start -n com.uncosens.geisterfahrer/com.unity3d.player.UnityPlayerGameActivity -e unity -force-gles`.

## Not done yet

- Real sounds (generated tones now; Freesound needs a login).
- Crash debris, smoke, scratch decals and dents; tree-line billboards; delineator reflectors; W-profile guardrail (a flat beam now).
- Truck columns and the "tall vehicle hides the only free lane" rule (GDD 4.2, 5.3).
- Corridor re-plan after big speed changes, truck columns, hidden-vehicle rule, crest row rule, vehicle fade-in (decisions 26, 29).
- Cuttings and embankments (ground just follows the road height), tree lines, overpass shadow quad.
- Android x86_64: Unity 6.3 no longer builds it.
