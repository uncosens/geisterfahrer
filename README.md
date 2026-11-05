# Geisterfahrer

Endless wrong-way driving on the German Autobahn. Unity 6.3 (URP), Android, landscape. Personal use.

- Game rules: [docs/plan/gdd-draft.md](docs/plan/gdd-draft.md) (v0.3)
- Tech plan: [docs/dev/unity-plan.md](docs/dev/unity-plan.md)
- Art direction: [docs/design/](docs/design/)

## Layout

| Folder | Content |
|---|---|
| `Assets/Scripts/Core` | Pure C# rules, no Unity references: car force model, contacts and impulse, traffic spawning |
| `Assets/Scripts/Game` | Unity side: `GameLoop` (views, camera, HUD), `RoadView`, `DriveInput`, placeholder models and sounds |
| `Assets/Scripts/Game/Editor` | `BuildScript` (scene setup, URP settings, Android build) |
| `Assets/Tests/EditMode` | Tests for Core |

All tuning numbers are in `Core/Tuning.cs` (editable on the `GameLoop` component). Vehicle sizes, masses and spawn weights are in `Core/Road.cs`.

Vehicles and the player car are primitive placeholders. To use real models, drop prefabs into `GameLoop.vehiclePrefabs` (index = Compact, Sedan, Van, Bus, Truck) and `playerPrefab`. Rules: 1 unit = 1 m, root at the footprint center on the ground, +Z = front.

## Build

Unity 6000.3.27f1 with the Android module. From the project folder:

```
"C:\Program Files\Unity\Hub\Editor\6000.3.27f1\Editor\Unity.exe" -batchmode -runTests -projectPath . -testPlatform EditMode -testResults test-results/editmode.xml
"C:\Program Files\Unity\Hub\Editor\6000.3.27f1\Editor\Unity.exe" -batchmode -quit -projectPath . -buildTarget Android -executeMethod Geisterfahrer.Editor.BuildScript.BuildAndroid -logFile Logs/build-android.log
adb -s <serial> install -r Builds/Android/geisterfahrer-dev.apk
```

Add `-release` to the build line for a non-development APK (`Builds/Android/geisterfahrer.apk`).
The build creates `Assets/Scenes/Game.unity` if it is missing (menu: Geisterfahrer > Create Game Scene).

Controls: left half = steer (drag from where you touch), right half = accelerate. Editor: A/D or arrows, W or Space, Esc/P pause, Enter play/retry.

## Checked (2026-11-05)

| What | Where | Result |
|---|---|---|
| 27 EditMode tests (speed curve, 11 GDD collision examples, scrape, no tunneling, fair-lane rule) | batch mode | all pass |
| Dev APK build (IL2CPP, ARM64) | this PC | 47 MB dev APK |
| Start screen, Play, accelerate, head-on crash, game over with "New best" | emulator `rc4k` (ARM translation, `-force-gles`) | works |
| Landscape, HUD (km/h, km, pause), steering origin ring, rail hit and scrape along the rail, crash, Retry, best records kept | Galaxy Tab S7 (Vulkan) | works |

Screenshots: `docs/dev/shots/`.

Not verified:
- Two thumbs at once (steer + accelerate): `adb input` can send only one finger. Needs a hand test on the tablet.
- Frame rate and Profiler numbers; sound by ear.
- On the emulator the default Vulkan path crashes inside the emulator's Vulkan driver (`vulkan.ranchu.so`, under ARM translation). Start it with `adb shell am start -n com.uncosens.geisterfahrer/com.unity3d.player.UnityPlayerGameActivity -e unity -force-gles`. The tablet uses Vulkan without problems.

## Not done yet

- Real models, UI design (HUD is IMGUI), real sounds (engine, scrape and crash are generated tones; no Doppler on passing vehicles).
- Truck columns in lane 3 and the "tall vehicle hides the only free lane" rule (GDD 4.2, 5.3).
- Fade-in at the haze, km numbers on the km posts, crash slow motion, haptics.
- Android x86_64: Unity 6.3 no longer builds it, so the emulator runs the ARM64 APK through translation.
