# Vehicle fetch list

Companion to `vehicle-assets.md` (section "Traffic variants"). Two parts: the files the script downloads (no login), and the Sketchfab models the user downloads by hand (free account).

Everything here is third-party data. The script downloads **only** 3D model and texture files (`.glb .gltf .fbx .obj .mtl .png`), from only these hosts: `static.poly.pizza`, `opengameart.org`, `raw.githubusercontent.com`. It never runs or unpacks anything else.

## Run

```powershell
powershell -ExecutionPolicy Bypass -File tools\fetch-vehicles.ps1 -DryRun    # prints the plan, downloads nothing
powershell -ExecutionPolicy Bypass -File tools\fetch-vehicles.ps1            # downloads what is missing
powershell -ExecutionPolicy Bypass -File tools\fetch-vehicles.ps1 -Force     # downloads again
powershell -ExecutionPolicy Bypass -File tools\fetch-vehicles.ps1 -Only D04,D05
```

What it does for each item:

1. Skips it if every target file already exists and passes the header check (unless `-Force`).
2. Downloads with `Invoke-WebRequest -OutFile` into a temp file (never a shell redirect).
3. Checks the size against the expected size (within 30 %, never over 60 MB) and the file header: `glTF` for GLB, `PK` for zip, `Kaydara FBX Binary` or `; FBX` for FBX, plain text without NUL bytes for OBJ and MTL, PNG magic for PNG.
4. For a zip, extracts **only the listed entries**, only with an allowed extension, only below `Assets/ThirdParty/Vehicles`, and checks each extracted file's header again. A zip is downloaded once even if several items use it.
5. Writes `CREDITS.vehicles.draft.md` in the repo root with the author and licence line of every item that is on disk (direct and manual). Merge it into `CREDITS.md` by hand.
6. Prints the manual list and which of those files are still missing.

It exits with a non-zero code if any download or check failed. `Assets/ThirdParty/` is git-ignored, so nothing here goes into git.

## Folder layout

`Assets/ThirdParty/Vehicles/<class>/<name>.<ext>` with the class folders `compact hatchback sedan suv sports mpv van longvan motorbike motorhome caravan rigid semi longcombo bus articulated`. The six models already in `Assets/ThirdParty/Vehicles/` (`Player.glb`, `Compact.glb`, `Sedan.glb`, `Van.glb`, `Bus.glb`, `Truck.glb`) stay where they are. R03, R17 and R33b are the same models as `Compact.glb`, `Van.glb` and `Bus.glb`: no need to download them again, only to copy them to the class folder if you want one place.

FBX and OBJ sources keep their own extension (Unity imports both). Only the Kenney, poly.pizza and Sketchfab files are GLB.

## Direct downloads (no login)

| ID | Direct URL | Target under `Assets/ThirdParty/Vehicles/` | Download size (bytes) | Licence |
|---|---|---|---|---|
| D01 | `https://opengameart.org/sites/default/files/vehicle_asset_v1.zip`<br>zip entry: `obj/car-micro-silver.obj`, `obj/car-micro-silver.mtl` | `compact/car-micro-silver.obj`, `compact/car-micro-silver.mtl` | 3,529,075 | CC0 |
| D02 | `https://static.poly.pizza/7dc33135-12c6-4de4-b5f9-cf445fd35994.glb` | `hatchback/supermini.glb` | 82,232 | CC0 |
| D03 | `https://opengameart.org/sites/default/files/kenney_car-kit_3.1.zip`<br>zip entry: `Models/GLB format/hatchback-sports.glb` | `hatchback/golf.glb` | 4,814,237 | CC0 |
| D04 | `https://opengameart.org/sites/default/files/kenney_car-kit_3.1.zip`<br>zip entry: `Models/GLB format/sedan.glb` | `sedan/sedan.glb` | 4,814,237 | CC0 |
| D05 | `https://static.poly.pizza/840253f1-676f-4d91-bd5c-9c6e3647ba73.glb` | `sedan/estate.glb` | 84,872 | CC0 |
| D06 | `https://opengameart.org/sites/default/files/kenney_car-kit_3.1.zip`<br>zip entry: `Models/GLB format/taxi.glb` | `sedan/taxi.glb` | 4,814,237 | CC0 |
| D07 | `https://opengameart.org/sites/default/files/kenney_car-kit_3.1.zip`<br>zip entry: `Models/GLB format/police.glb` | `sedan/police.glb` | 4,814,237 | CC0 |
| D08 | `https://opengameart.org/sites/default/files/kenney_car-kit_3.1.zip`<br>zip entry: `Models/GLB format/suv.glb` | `suv/mid-suv.glb` | 4,814,237 | CC0 |
| D09 | `https://opengameart.org/sites/default/files/kenney_car-kit_3.1.zip`<br>zip entry: `Models/GLB format/suv-luxury.glb` | `suv/large-suv.glb` | 4,814,237 | CC0 |
| D10 | `https://static.poly.pizza/ecae441a-cba8-4dd6-9795-207a72d7e88e.glb` | `suv/pickup.glb` | 192,800 | CC BY 3.0 |
| D11 | `https://static.poly.pizza/c2ae6507-7a82-4b1c-a8cd-d7bfa6378a21.glb` | `sports/coupe.glb` | 822,800 | CC BY 3.0 |
| D12 | `https://static.poly.pizza/c434a5e4-0232-4a96-a008-f1152ff6b4fa.glb` | `sports/convertible.glb` | 1,762,084 | CC BY 3.0 |
| D13 | `https://opengameart.org/sites/default/files/vehicle_asset_v1.zip`<br>zip entry: `obj/car-mpv-silver.obj`, `obj/car-mpv-silver.mtl` | `mpv/car-mpv-silver.obj`, `mpv/car-mpv-silver.mtl` | 3,529,075 | CC0 |
| D14 | `https://opengameart.org/sites/default/files/kenney_car-kit_3.1.zip`<br>zip entry: `Models/GLB format/van.glb` | `van/panel.glb` | 4,814,237 | CC0 |
| D15 | `https://opengameart.org/sites/default/files/kenney_car-kit_3.1.zip`<br>zip entry: `Models/GLB format/delivery.glb` | `van/delivery.glb` | 4,814,237 | CC0 |
| D17 | `https://opengameart.org/sites/default/files/free_low_poly_vehicles_pack_by_rgsdev.zip`<br>zip entry: `Free Low Poly Vehicles Pack by Rgsdev/Van/Van.fbx` | `longvan/long-panel.fbx` | 1,443,472 | CC0 |
| D18 | `https://opengameart.org/sites/default/files/kenney_car-kit_3.1.zip`<br>zip entry: `Models/GLB format/ambulance.glb` | `longvan/ambulance.glb` | 4,814,237 | CC0 |
| D19 | `https://raw.githubusercontent.com/KenneyNL/Starter-Kit-Racing/main/models/vehicle-motorcycle.glb` | `motorbike/sport.glb` | 97,172 | CC0 |
| D20 | `https://static.poly.pizza/f7635710-7cfa-4768-ad70-8366ab485358.glb` | `motorbike/touring.glb` | 109,608 | CC BY 3.0 |
| D21 | `https://static.poly.pizza/eb574665-ed92-403d-bcc2-2b8dde8e2494.glb` | `motorhome/coachbuilt.glb` | 181,740 | CC BY 3.0 |
| D22 | `https://static.poly.pizza/341f6a07-c987-4eb6-8ee9-5b3d817eb1b3.glb` | `caravan/caravan.glb` | 168,068 | CC BY 3.0 |
| D24 | `https://static.poly.pizza/4e798498-be05-47ac-988a-ad953cf46236.glb` | `rigid/box.glb` | 316,240 | CC BY 3.0 |
| D26 | `https://static.poly.pizza/1511f362-d635-4dc6-b9a2-a70fdb6370b2.glb` | `rigid/dump.glb` | 128,580 | CC BY 3.0 |
| D27 | `https://opengameart.org/sites/default/files/free_low_poly_vehicles_pack_by_rgsdev.zip`<br>zip entry: `Free Low Poly Vehicles Pack by Rgsdev/Truck with trailer/Truck with trailer.fbx` | `semi/box-semi.fbx` | 1,443,472 | CC0 |
| D29 | `https://static.poly.pizza/e56c8efb-940f-4150-9bf8-d468c0165116.glb` | `semi/container-load.glb` | 57,020 | CC0 |
| D30 | `https://static.poly.pizza/70483e1c-8adf-4abf-85e1-51fcfc5adf84.glb` | `semi/tanker-semi.glb` | 47,408 | CC BY 3.0 |
| D33 | `https://opengameart.org/sites/default/files/3DBus_by_ajanhallinta.zip`<br>zip entry: `FBX/BUS.fbx` | `bus/coach.fbx` | 782,148 | CC0 |
| D34 | `https://opengameart.org/sites/default/files/Public%20Transport_0.zip`<br>zip entry: `Public Transport/FBX/Bus.fbx` | `bus/line-bus.fbx` | 1,411,819 | CC0 |

Kenney items also extract `Textures/colormap.png` into the same class folder, because the GLBs point to it by a relative path.

## By hand (Sketchfab, free account)

On each page: **Download 3D Model**, choose **glTF Binary (.glb)**, and save the file under the target name. The browser saves it as a random `.tmp` name sometimes; rename it. Then run the script again: it checks the headers and adds the credit lines.

| ID | Page | Save as (under `Assets/ThirdParty/Vehicles/`) | Licence | Credit line |
|---|---|---|---|---|
| R01 | [Mini Cooper S](https://sketchfab.com/3d-models/ca4076c918bc49d79ebe301343c5787d) | `compact/city-car.glb` | CC BY 4.0 | `"Mini Cooper S" by maregajavier (https://sketchfab.com/maregajavier), licensed under CC BY 4.0`; Mini Cooper S |
| R02 | [Low Poly Polo](https://sketchfab.com/3d-models/ad506688595341e880d06e04a5ce2670) | `hatchback/supermini.glb` | CC BY 4.0 | `"Low Poly Polo" by normal/studio (https://sketchfab.com/Urmanga), licensed under CC BY 4.0` |
| R03 | [1998 Volkswagen Golf Mk4](https://sketchfab.com/3d-models/ec7b2f2fd567419a965db5b07b777c0c) | `hatchback/golf.glb` | CC BY 4.0 | `"1998 Volkswagen Golf Mk4" by ImperialBlue3D (https://sketchfab.com/ImperialBlue3D), licensed under CC BY 4.0`; already in the game (Compact.glb) |
| R04 | [BMW 320i E90 Low Polly](https://sketchfab.com/3d-models/28f0bc2f0083417a8e521150529034d6) | `sedan/sedan.glb` | CC BY 4.0 | `"BMW 320i E90 Low Polly" by rifdanzz (https://sketchfab.com/rifdanzz), licensed under CC BY 4.0`; also the base for taxi (cream) and police (silver/blue) |
| R05 | [Audi A4 B5.5 Avant](https://sketchfab.com/3d-models/2f6aea1435bd4ad093146c019c8e0fb6) | `sedan/estate.glb` | CC BY 4.0 | `"Audi A4 B5.5 Avant" by Tomaso (https://sketchfab.com/Tomasoo), licensed under CC BY 4.0`; also taxi/police base |
| R08 | [Mercedes Benz G-class W263](https://sketchfab.com/3d-models/1a2a52b16cad4e618af347461817895c) | `suv/mid-suv.glb` | CC BY 4.0 | `"Mercedes Benz G-class W263" by Lexyc16 (https://sketchfab.com/Lexyc16), licensed under CC BY 4.0`; boxy; textures 128 px (flat colours) |
| R09 | [BMW X5 E70](https://sketchfab.com/3d-models/d515af89f6c6414b93c537ee1fcfc800) | `suv/large-suv.glb` | CC BY 4.0 | `"BMW X5 E70" by LEON elenaisakova248 (https://sketchfab.com/elenaisakova248), licensed under CC BY 4.0` |
| R10 | [2019 Ford Ranger Raptor CIVIL (Cities Skylines)](https://sketchfab.com/3d-models/c2d4113e6acd401cb1999f81bb318325) | `suv/pickup.glb` | CC BY 4.0 | `"2019 Ford Ranger Raptor CIVIL (Cities Skylines)" by TSB3DMODELS (https://sketchfab.com/TSB3DMODELS), licensed under CC BY 4.0`; use the CIVIL version, not POLICE |
| R11 | [BMW M4 (F82)](https://sketchfab.com/3d-models/1d24aac71183435eb6a5ecaba5c0434d) | `sports/coupe.glb` | CC BY 4.0 | `"BMW M4 (F82)" by Mona x Supercars (https://sketchfab.com/Car2022), licensed under CC BY 4.0` |
| R12 | [Mercedes-Benz SL 65 AMG Black Series (R230)](https://sketchfab.com/3d-models/84c255b8a34b496bb1cb076df5ff6e70) | `sports/convertible.glb` | CC BY 4.0 | `"Mercedes-Benz SL 65 AMG Black Series (R230)" by Mona x Supercars (https://sketchfab.com/Car2022), licensed under CC BY 4.0`; check that the roof is open in the preview |
| R13 | [VW Sharan](https://sketchfab.com/3d-models/3543b8be92d44f5aa79d8fd9bd9b7835) | `mpv/minivan.glb` | CC BY 4.0 | `"VW Sharan" by LEON elenaisakova248 (https://sketchfab.com/elenaisakova248), licensed under CC BY 4.0` |
| R14 | [VW Transporter Multivan Green](https://sketchfab.com/3d-models/e9bc406f71dd4d888e1d598c9ad05889) | `van/panel.glb` | CC BY 4.0 | `"VW Transporter Multivan Green" by LEON elenaisakova248 (https://sketchfab.com/elenaisakova248), licensed under CC BY 4.0` |
| R15 | [European Delivery Van](https://sketchfab.com/3d-models/0b2f1ad95a79419f9a092420024d329c) | `van/delivery.glb` | CC BY 4.0 | `"European Delivery Van" by Evan Hiltz (https://sketchfab.com/evan.hiltz), licensed under CC BY 4.0`; GLB is 36 MB: downscale 4096 px textures to 1024 first |
| R16 | [Free Low-poly Vintage Hippy Camper Van](https://sketchfab.com/3d-models/931d18f3e94b4cd580440bdc85436734) | `van/camper.glb` | CC BY 4.0 | `"Free Low-poly Vintage Hippy Camper Van" by Libau Media (https://sketchfab.com/robinmikart), licensed under CC BY 4.0`; only vintage camper found; flat colours |
| R17 | [Mercedes Sprinter Passenger](https://sketchfab.com/3d-models/d3029b62317c4956bd0a28140ffef83d) | `longvan/long-panel.glb` | CC BY 4.0 | `"Mercedes Sprinter Passenger" by kuwalol93 (https://sketchfab.com/kuwalol93), licensed under CC BY 4.0`; already in the game (Van.glb); also the ambulance base |
| R19 | [Suzuki GSXR 750](https://sketchfab.com/3d-models/3e05cd872ef0458ca3d6b0770bb03622) | `motorbike/sport.glb` | CC BY 4.0 | `"Suzuki GSXR 750" by Silyboy (https://sketchfab.com/kaleemullah550), licensed under CC BY 4.0`; no rider |
| R20 | [Akt Tt Ds 200 repainted](https://sketchfab.com/3d-models/c32f7b87090d4ce4b10c76e844dc86cd) | `motorbike/touring.glb` | CC BY 4.0 | `"Akt Tt Ds 200 repainted" by e-restrepo1114 (https://sketchfab.com/EmanuelRestrepoVelez), licensed under CC BY 4.0`; adventure bike with panniers; GLB is 31 MB, downscale textures |
| R21 | [Motorhome RV](https://sketchfab.com/3d-models/4574fbdfb8af498d8e61fd4d43018c9e) | `motorhome/coachbuilt.glb` | CC BY 4.0 | `"Motorhome RV" by Medical Modeller (https://sketchfab.com/pistonstone), licensed under CC BY 4.0` |
| R22 | [Large Caravan](https://sketchfab.com/3d-models/bc096d0ad5c64ac1a48647200fd559c2) | `caravan/caravan.glb` | CC BY 4.0 | `"Large Caravan" by rhcreations (https://sketchfab.com/rhcreations), licensed under CC BY 4.0` |
| R24 | [LCT 3000 '07- Low poly model](https://sketchfab.com/3d-models/3be03b6a43aa41898c9ca806b8787052) | `rigid/box.glb` | CC BY 4.0 | `"LCT 3000 '07- Low poly model" by Daniel Zhabotinsky (https://sketchfab.com/DanielZhabotinsky), licensed under CC BY 4.0`; GLB is 11 MB |
| R26 | [Japanese Dump Truck](https://sketchfab.com/3d-models/bdd68dcc73be430dbffff61017c95fb8) | `rigid/dump.glb` | CC BY 4.0 | `"Japanese Dump Truck" by Pavel Gorbachev (https://sketchfab.com/paul_sawyer_11), licensed under CC BY 4.0` |
| R27a | [2021 Volvo FH](https://sketchfab.com/3d-models/7e5f387b6660438aa0942cd17716d196) | `semi/tractor-volvo-fh.glb` | CC BY 4.0 | `"2021 Volvo FH" by mcskinssketchfab (https://sketchfab.com/mcvehiclessketchfab), licensed under CC BY 4.0`; tractor for the semi variants (cheap, 256 px textures) |
| R27b | [Truck Trailer](https://sketchfab.com/3d-models/cd9a5677626f4b648a4a3b1e0d9ca5b9) | `semi/trailer-box.glb` | CC BY 4.0 | `"Truck Trailer" by cmitche1 (https://sketchfab.com/cmitche1), licensed under CC BY 4.0`; trailer for box semi; for curtain semi reskin with a tarp texture; GLB is 21 MB |
| R28 | [low poly DAF xf105](https://sketchfab.com/3d-models/51f67c0bd11440dcbe0d1141f5d3508f) | `semi/tractor-daf-xf105.glb` | CC BY 4.0 | `"low poly DAF xf105" by Alvin.Woodly (https://sketchfab.com/Alvin.Woodly), licensed under CC BY 4.0`; alternative tractor (DAF XF105), no textures |
| R29a | [Flatbed Trailer](https://sketchfab.com/3d-models/76998b4d82604bb7996768835dcaa276) | `semi/trailer-flatbed.glb` | CC BY 4.0 | `"Flatbed Trailer" by Arsen Ismailov (https://sketchfab.com/fdgasd7), licensed under CC BY 4.0`; GLB is 25 MB, downscale 4096 px |
| R29b | [Truck Container Pack Low-poly ASIA](https://sketchfab.com/3d-models/000b6d08ad7b4f4198dcab2790008553) | `semi/containers.glb` | CC BY 4.0 | `"Truck Container Pack Low-poly ASIA" by knitro_vedant (https://sketchfab.com/knitro_vedant), licensed under CC BY 4.0`; the containers to put on R29a |
| R30 | [Simple tank trailer](https://sketchfab.com/3d-models/738d0e2db6584fe39d4bf9b284c7b614) | `semi/trailer-tank.glb` | CC BY 4.0 | `"Simple tank trailer" by AndersonCardoso (https://sketchfab.com/AndersonCardoso), licensed under CC BY 4.0`; no textures |
| R32 | [Mitsubishi Fuso Car Carrier](https://sketchfab.com/3d-models/4b34a78f69974e62ba0b5b8f461f4360) | `longcombo/car-transporter.glb` | CC BY 4.0 | `"Mitsubishi Fuso Car Carrier" by ImperialBlue3D (https://sketchfab.com/ImperialBlue3D), licensed under CC BY 4.0`; not a real transporter combination |
| R33 | [1998 Hyundai Aero Space Coach Bus](https://sketchfab.com/3d-models/b51273fe6c19486eb71e95bc9c5c7032) | `bus/coach.glb` | CC BY 4.0 | `"1998 Hyundai Aero Space Coach Bus" by ImperialBlue3D (https://sketchfab.com/ImperialBlue3D), licensed under CC BY 4.0`; Hyundai coach, older shape |
| R33b | [Coach bus](https://sketchfab.com/3d-models/89866d7d4a314f1f81b024280b1cf416) | `bus/coach-alt.glb` | CC BY 4.0 | `"Coach bus" by own.guest (https://sketchfab.com/own.guest), licensed under CC BY 4.0`; already in the game (Bus.glb); alternative coach |
| R34 | [City Bus - rigged / РоАЗ-5236](https://sketchfab.com/3d-models/36646fcf41da498ca24a02f4b6fb1d95) | `bus/line-bus.glb` | CC BY 4.0 | `"City Bus - rigged / РоАЗ-5236" by Yo.Ri (https://sketchfab.com/grox777), licensed under CC BY 4.0`; RoAZ-5236 low-floor city bus |
| R35 | [Cee Urbano C7 Volvo B340M - Padrão Mobi Rio](https://sketchfab.com/3d-models/bacf3741cd134f639a77ea635ecd2d5a) | `articulated/artic-line.glb` | CC BY 4.0 | `"Cee Urbano C7 Volvo B340M - Padrão Mobi Rio" by CR3D Modelagens (https://sketchfab.com/cr3dmods), licensed under CC BY 4.0` |

R03, R17 and R33b are already in the game. Taxi, police and ambulance need no download (paint on R04/R05 and R17).

## Attribution rules

- CC0: no credit needed; the draft lists them anyway.
- CC BY: the credit line is required in `CREDITS.md` and in the README of the public repo. If a model is changed (decimated, scaled, recoloured) add `, modified` at the end of its line.
- Real vehicle names and designs belong to their owners; the project is personal and non-commercial (`CREDITS.md`).


## Player car, 991 rear (by hand, Sketchfab login)

**No direct no-login URL exists.** poly.pizza and OpenGameArt have no Porsche 911, and Sketchfab downloads need a free account. So the user downloads these by hand (the script only lists them and checks the files). Target folder: `Assets/ThirdParty/Player/` (git-ignored with the rest of `Assets/ThirdParty/`). The old 992 model stays where it is until the new one is imported.

On each page: **Download 3D Model**, choose **glTF Binary (.glb)**, and save the file under the target name. If the dialog asks for anything other than the format, stop and tell the director.

| ID | Page | Save as | Size | Licence | Credit line |
|---|---|---|---|---|---|
| P1 (first choice) | [Porsche 911 Carrera S (991)](https://sketchfab.com/3d-models/97785983d3a949cf87e9ee6bf49f4032) | `Assets/ThirdParty/Player/porsche-991-carrera-s.glb` | about 4.9 MB, 78,990 tris | CC BY 4.0 | `"Porsche 911 Carrera S (991)" by Mona x Supercars (https://sketchfab.com/Car2022), licensed under CC BY 4.0 (https://creativecommons.org/licenses/by/4.0/), modified (decimated, recoloured)` |
| P2 (fallback) | [(FREE) Porsche 911 Carrera 4S](https://sketchfab.com/3d-models/d01b254483794de3819786d93e0e1ebf) | `Assets/ThirdParty/Player/porsche-991-carrera-4s.glb` | about 25.4 MB, 652,660 tris | CC BY-SA 4.0 | `"(FREE) Porsche 911 Carrera 4S" by Lionsharp Studios (https://sketchfab.com/lionsharp), licensed under CC BY-SA 4.0 (https://creativecommons.org/licenses/by-sa/4.0/), modified (decimated, recoloured)` |

**Status 2026-11-07: P1 is downloaded** (the user said yes), to `ThirdPartySources\player\porsche-991-carrera-s\porsche-991-carrera-s.glb` (4,929,080 bytes, SHA-256 in `ThirdPartySources\SOURCES.md`). It sits outside the Unity project; copy it to `Assets/ThirdParty/Player/porsche-991-carrera-s.glb` when importing. P2 was not needed.

`tools/fetch-vehicles.ps1` lists P1 and P2 in its manual part, checks that the files are GLB, and adds their credit lines to `CREDITS.vehicles.draft.md` once they exist. If the user downloads into another folder, move the files to the target names above first.

Details, the other three candidates, and what to check after import are in `vehicle-assets.md`, section "Player car, 991 rear".
