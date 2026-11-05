# Tech stack review

Goal: a simple 2D (or light 2.5D) endless driving game on Android phones and tablets.
Checked on 2026-11-05 on this PC. No code yet.

## What is installed on this PC

| Tool | Status |
|---|---|
| JDK 21 (Temurin 21.0.12) | `C:\Users\junan\AndroidDev\jdk` |
| Android SDK (platforms 36, 37) + emulator 35.2.10, AVDs `rc4k`/`rc16k` | `C:\Users\junan\AndroidDev\sdk` |
| Gradle 9.8.1 | cached, used by rocket-chess and mindwave |
| Unity 6000.3.27f1 (6.3 LTS) + Android module (own SDK/NDK/JDK) | installed, used by `tower-bomber` |
| Godot | not installed |
| Node.js / npm | not installed |

## Options

| | A. Kotlin + Compose Canvas | B. Unity 6.3 (2D URP) | C. Godot 4 | D. Web (Canvas / Phaser) |
|---|---|---|---|---|
| Setup on this PC | none, same as rocket-chess | none, same as tower-bomber | download Godot + export templates, set SDK path | none for a plain Canvas page; Node only if we bundle; Android wrap needs Capacitor or a TWA |
| APK size | about 5-10 MB | about 25-40 MB release (tower-bomber debug APK is 67 MB) | about 25-35 MB | 2-5 MB web; wrapped APK about 5-10 MB |
| Mid phone speed | 60 fps for sprites and simple shapes; we write our own loop and collisions | 60 fps easily; strongest for many objects, particles, 3D | 60 fps for 2D | good in Chrome; WebView can stutter and has input lag |
| Time to first playable | about 1 day | about 1 day, but scene and prefab work goes through the Editor | 1-2 days (new tool to learn) | about 1 day; fastest to share by link |
| Store path | Play Store AAB, same flow as rocket-chess | Play Store AAB from Unity; also iOS later | Play Store AAB; iOS later | itch.io / web link; Play needs a wrapper |
| Fit for how we work | all code is text, unit tests with Gradle, smallest code | strong engine, but much state lives in scene/asset files | small engine, but new to us | easy demo, weakest on-device feel |

Notes:
- A top-down road with lanes, oncoming cars and a score needs no physics engine. Lane position, AABB overlap and a speed curve are enough.
- A pseudo-3D road (OutRun style: road strips scaled toward a horizon) also works in Canvas. It is a known 2D trick and needs no 3D engine.
- Real 3D (camera behind a 3D car, lighting, models) is where Unity wins clearly.

## Recommendation: A. Kotlin + Jetpack Compose Canvas

Why:
- Zero setup. Same toolchain, emulator and release flow as rocket-chess and mindwave.
- Smallest APK and fewest permissions (offline, no `INTERNET`).
- The game loop is small: `withFrameNanos` updates state, one `Canvas` draws it. Rules live in plain Kotlin and are unit tested.
- Easy to make responsive: the world is in road units and scaled to any screen size.

Switch to B (Unity) if the planner or designer picks real 3D, heavy particles or physics crashes.
Unity is ready on this PC, so the switch costs little before code exists.

### Minimal project structure

```
Geisterfahrer/
  settings.gradle.kts
  build.gradle.kts
  gradle.properties
  app/
    build.gradle.kts            # namespace com.uncosens.geisterfahrer (confirm)
    src/main/AndroidManifest.xml
    src/main/java/com/uncosens/geisterfahrer/
      MainActivity.kt           # sets content, keeps screen on
      Game.kt                   # pure state + step(dt, input): cars, spawn, collide, score
      GameScreen.kt             # frame loop, touch input, Canvas drawing, HUD
      Prefs.kt                  # best score (SharedPreferences)
    src/main/res/drawable/      # car and road sprites from the designer (or vectors)
    src/test/java/.../GameTest.kt
  docs/plan/  docs/design/  docs/dev/
  README.md
```

Four Kotlin files to start. Split only when a file gets hard to read.
Sound (if any) uses `SoundPool`, no extra library.

## Needed before coding

From the planner:
1. Core rule: are we the wrong-way driver dodging traffic, or do we chase/stop one?
2. Controls: swipe lanes, tap left/right half, hold-and-drag, or tilt.
3. Lanes: fixed lanes (how many) or free steering.
4. Fail rule: one hit ends the run, or lives/health.
5. Score: distance, time, near misses, coins?
6. Difficulty curve: what speeds up and how fast.
7. Screens for v0.1: title, play, game over only?
8. Orientation: portrait or landscape (portrait fits one-hand play).

From the designer:
1. View: top-down, behind-the-car pseudo-3D, or side view.
2. Art style: flat shapes drawn in code, or sprite images (size, format: PNG or vector).
3. Light or dark theme, and a palette.
4. Asset list for v0.1: player car, 2-3 traffic cars, road, roadside items, HUD font/icons.
5. Feedback: crash and near-miss effects, sound or none.
6. App name and icon.

From the user:
- Confirm package ID `com.uncosens.geisterfahrer` (cannot change after a Play release).
