# Geisterfahrer

Endless wrong-way driving on the German Autobahn. Unity 6.3 (URP), Android, landscape. Personal use.

- Game rules: [docs/plan/gdd-draft.md](docs/plan/gdd-draft.md) (v0.3, plus the director's changes: 4 lanes, start at 0 km/h, red 911, lateral offsets in lanes)
- Tech plan: [docs/dev/unity-plan.md](docs/dev/unity-plan.md)
- Art direction and UI: [docs/design/](docs/design/)
- Third-party assets: [CREDITS.md](CREDITS.md)

## Layout

| Folder | Content |
|---|---|
| `Assets/Scripts/Core` | Pure C# rules, no Unity references: car force model, contacts and impulse, traffic spawning |
| `Assets/Scripts/Game` | Unity side: `GameLoop` (views, camera), `Hud` (uGUI per `docs/design/ui-spec.md`), `RoadView`, `Feedback` (sparks, shake, haptics), `DriveInput`, placeholders, sounds |
| `Assets/Scripts/Game/Editor` | `BuildScript`: `Setup` (materials, fonts, sky, post, scene) and the Android build |
| `Assets/Tests/EditMode` | Tests for Core |
| `tools/fetch-assets.sh` | Downloads the free sky, textures and fonts into the git-ignored `Assets/ThirdParty/` |

All tuning numbers are in `Core/Tuning.cs` (editable on the `GameLoop` component). Lanes, vehicle sizes, masses and spawn weights are in `Core/Road.cs`.

Core works in road space (lateral offset and distance along the road). Only `RoadView.Place` maps it to the world, so curves and hills can be added in the view later.

Vehicles are primitive placeholders until the `.glb` files are in `Assets/ThirdParty/Vehicles/` (see CREDITS.md). `Placeholder.Fit` turns each model's long axis to z, scales it to the real length and puts it on the ground.

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

- Real vehicle models (download by hand, see CREDITS.md) and real sounds (generated tones now; Freesound needs a login). A downloaded 911 keeps its own paint colour.
- Crash debris, smoke, scratch decals and dents; tree-line billboards; delineator reflectors; W-profile guardrail (a flat beam now).
- Truck columns and the "tall vehicle hides the only free lane" rule (GDD 4.2, 5.3).
- Curves, hills, tunnels, viaducts (GDD v0.4, waiting).
- Android x86_64: Unity 6.3 no longer builds it.
