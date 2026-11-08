<#
.SYNOPSIS
  Downloads the free, no-login vehicle models listed in docs/design/vehicle-fetch.md into Assets/ThirdParty/Vehicles.
.DESCRIPTION
  Only 3D model and texture files, only from static.poly.pizza, opengameart.org and raw.githubusercontent.com.
  Checks the size and the file header of every download. Extracts only the listed zip entries.
  Writes CREDITS.vehicles.draft.md (author and licence lines) in the repo root.
  The Sketchfab models need a free account: they are listed at the end for the user to download by hand.
.PARAMETER DryRun  Print the plan and the manual list, download nothing.
.PARAMETER Force   Download again even when the target files exist.
.PARAMETER Only    Only these ids, for example -Only D04,D05
#>
param([switch]$DryRun, [switch]$Force, [string[]]$Only)

$ErrorActionPreference = 'Stop'
$ProgressPreference = 'SilentlyContinue'
[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
Add-Type -AssemblyName System.IO.Compression.FileSystem

$Root  = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$Dest  = Join-Path $Root 'Assets/ThirdParty/Vehicles'
$Draft = Join-Path $Root 'CREDITS.vehicles.draft.md'
$Ua    = 'Mozilla/5.0 (fetch-vehicles.ps1)'
$AllowedHosts = @('static.poly.pizza', 'opengameart.org', 'raw.githubusercontent.com')
$AllowedExt   = @('.glb', '.gltf', '.fbx', '.obj', '.mtl', '.png')
$MaxBytes     = 60MB

# ---- data (generated from docs/design/vehicle-assets.md) -------------------------------------------------
$Packs = @(
  @{ Kind='zip'; Url='https://opengameart.org/sites/default/files/vehicle_asset_v1.zip'; Bytes=3529075;
     Files=@(
       @{ Id='D01'; Entry='obj/car-micro-silver.obj'; Out='compact/car-micro-silver.obj'; Credit=$true },
       @{ Id='D01'; Entry='obj/car-micro-silver.mtl'; Out='compact/car-micro-silver.mtl'; Credit=$true },
       @{ Id='D13'; Entry='obj/car-mpv-silver.obj'; Out='mpv/car-mpv-silver.obj'; Credit=$true },
       @{ Id='D13'; Entry='obj/car-mpv-silver.mtl'; Out='mpv/car-mpv-silver.mtl'; Credit=$true }
     ) },
  @{ Kind='glb'; Url='https://static.poly.pizza/7dc33135-12c6-4de4-b5f9-cf445fd35994.glb'; Bytes=82232;
     Files=@(
       @{ Id='D02'; Entry=$null; Out='hatchback/supermini.glb'; Credit=$true }
     ) },
  @{ Kind='zip'; Url='https://opengameart.org/sites/default/files/kenney_car-kit_3.1.zip'; Bytes=4814237;
     Files=@(
       @{ Id='D03'; Entry='Models/GLB format/hatchback-sports.glb'; Out='hatchback/golf.glb'; Credit=$true },
       @{ Id='D03'; Entry='Models/GLB format/Textures/colormap.png'; Out='hatchback/Textures/colormap.png'; Credit=$false },
       @{ Id='D04'; Entry='Models/GLB format/sedan.glb'; Out='sedan/sedan.glb'; Credit=$true },
       @{ Id='D04'; Entry='Models/GLB format/Textures/colormap.png'; Out='sedan/Textures/colormap.png'; Credit=$false },
       @{ Id='D06'; Entry='Models/GLB format/taxi.glb'; Out='sedan/taxi.glb'; Credit=$true },
       @{ Id='D07'; Entry='Models/GLB format/police.glb'; Out='sedan/police.glb'; Credit=$true },
       @{ Id='D08'; Entry='Models/GLB format/suv.glb'; Out='suv/mid-suv.glb'; Credit=$true },
       @{ Id='D08'; Entry='Models/GLB format/Textures/colormap.png'; Out='suv/Textures/colormap.png'; Credit=$false },
       @{ Id='D09'; Entry='Models/GLB format/suv-luxury.glb'; Out='suv/large-suv.glb'; Credit=$true },
       @{ Id='D14'; Entry='Models/GLB format/van.glb'; Out='van/panel.glb'; Credit=$true },
       @{ Id='D14'; Entry='Models/GLB format/Textures/colormap.png'; Out='van/Textures/colormap.png'; Credit=$false },
       @{ Id='D15'; Entry='Models/GLB format/delivery.glb'; Out='van/delivery.glb'; Credit=$true },
       @{ Id='D18'; Entry='Models/GLB format/ambulance.glb'; Out='longvan/ambulance.glb'; Credit=$true },
       @{ Id='D18'; Entry='Models/GLB format/Textures/colormap.png'; Out='longvan/Textures/colormap.png'; Credit=$false }
     ) },
  @{ Kind='glb'; Url='https://static.poly.pizza/840253f1-676f-4d91-bd5c-9c6e3647ba73.glb'; Bytes=84872;
     Files=@(
       @{ Id='D05'; Entry=$null; Out='sedan/estate.glb'; Credit=$true }
     ) },
  @{ Kind='glb'; Url='https://static.poly.pizza/ecae441a-cba8-4dd6-9795-207a72d7e88e.glb'; Bytes=192800;
     Files=@(
       @{ Id='D10'; Entry=$null; Out='suv/pickup.glb'; Credit=$true }
     ) },
  @{ Kind='glb'; Url='https://static.poly.pizza/c2ae6507-7a82-4b1c-a8cd-d7bfa6378a21.glb'; Bytes=822800;
     Files=@(
       @{ Id='D11'; Entry=$null; Out='sports/coupe.glb'; Credit=$true }
     ) },
  @{ Kind='glb'; Url='https://static.poly.pizza/c434a5e4-0232-4a96-a008-f1152ff6b4fa.glb'; Bytes=1762084;
     Files=@(
       @{ Id='D12'; Entry=$null; Out='sports/convertible.glb'; Credit=$true }
     ) },
  @{ Kind='zip'; Url='https://opengameart.org/sites/default/files/free_low_poly_vehicles_pack_by_rgsdev.zip'; Bytes=1443472;
     Files=@(
       @{ Id='D17'; Entry='Free Low Poly Vehicles Pack by Rgsdev/Van/Van.fbx'; Out='longvan/long-panel.fbx'; Credit=$true },
       @{ Id='D27'; Entry='Free Low Poly Vehicles Pack by Rgsdev/Truck with trailer/Truck with trailer.fbx'; Out='semi/box-semi.fbx'; Credit=$true }
     ) },
  @{ Kind='glb'; Url='https://raw.githubusercontent.com/KenneyNL/Starter-Kit-Racing/main/models/vehicle-motorcycle.glb'; Bytes=97172;
     Files=@(
       @{ Id='D19'; Entry=$null; Out='motorbike/sport.glb'; Credit=$true }
     ) },
  @{ Kind='glb'; Url='https://static.poly.pizza/f7635710-7cfa-4768-ad70-8366ab485358.glb'; Bytes=109608;
     Files=@(
       @{ Id='D20'; Entry=$null; Out='motorbike/touring.glb'; Credit=$true }
     ) },
  @{ Kind='glb'; Url='https://static.poly.pizza/eb574665-ed92-403d-bcc2-2b8dde8e2494.glb'; Bytes=181740;
     Files=@(
       @{ Id='D21'; Entry=$null; Out='motorhome/coachbuilt.glb'; Credit=$true }
     ) },
  @{ Kind='glb'; Url='https://static.poly.pizza/341f6a07-c987-4eb6-8ee9-5b3d817eb1b3.glb'; Bytes=168068;
     Files=@(
       @{ Id='D22'; Entry=$null; Out='caravan/caravan.glb'; Credit=$true }
     ) },
  @{ Kind='glb'; Url='https://static.poly.pizza/4e798498-be05-47ac-988a-ad953cf46236.glb'; Bytes=316240;
     Files=@(
       @{ Id='D24'; Entry=$null; Out='rigid/box.glb'; Credit=$true }
     ) },
  @{ Kind='glb'; Url='https://static.poly.pizza/1511f362-d635-4dc6-b9a2-a70fdb6370b2.glb'; Bytes=128580;
     Files=@(
       @{ Id='D26'; Entry=$null; Out='rigid/dump.glb'; Credit=$true }
     ) },
  @{ Kind='glb'; Url='https://static.poly.pizza/e56c8efb-940f-4150-9bf8-d468c0165116.glb'; Bytes=57020;
     Files=@(
       @{ Id='D29'; Entry=$null; Out='semi/container-load.glb'; Credit=$true }
     ) },
  @{ Kind='glb'; Url='https://static.poly.pizza/70483e1c-8adf-4abf-85e1-51fcfc5adf84.glb'; Bytes=47408;
     Files=@(
       @{ Id='D30'; Entry=$null; Out='semi/tanker-semi.glb'; Credit=$true }
     ) },
  @{ Kind='zip'; Url='https://opengameart.org/sites/default/files/3DBus_by_ajanhallinta.zip'; Bytes=782148;
     Files=@(
       @{ Id='D33'; Entry='FBX/BUS.fbx'; Out='bus/coach.fbx'; Credit=$true }
     ) },
  @{ Kind='zip'; Url='https://opengameart.org/sites/default/files/Public%20Transport_0.zip'; Bytes=1411819;
     Files=@(
       @{ Id='D34'; Entry='Public Transport/FBX/Bus.fbx'; Out='bus/line-bus.fbx'; Credit=$true }
     ) }
)

$Credits = @(
  @{ Id='D01'; Title='Vehicles Assets pt1: car-micro (silver)'; Author='rubberduck (OpenGameArt)'; License='CC0'; Page='https://opengameart.org/content/vehicles-assets-pt1'; Out=@('compact/car-micro-silver.obj','compact/car-micro-silver.mtl') },
  @{ Id='D02'; Title='Car Hatchback'; Author='Kay Lousberg (KayKit)'; License='CC0'; Page='https://poly.pizza/m/BG0KAhmGDt'; Out=@('hatchback/supermini.glb') },
  @{ Id='D03'; Title='Kenney Car Kit: hatchback-sports'; Author='Kenney (kenney.nl), mirrored on OpenGameArt'; License='CC0'; Page='https://opengameart.org/content/car-kit'; Out=@('hatchback/golf.glb') },
  @{ Id='D04'; Title='Kenney Car Kit: sedan'; Author='Kenney (kenney.nl), mirrored on OpenGameArt'; License='CC0'; Page='https://opengameart.org/content/car-kit'; Out=@('sedan/sedan.glb') },
  @{ Id='D05'; Title='Stationwagon'; Author='Kay Lousberg (KayKit)'; License='CC0'; Page='https://poly.pizza/m/vTTTjDoxhV'; Out=@('sedan/estate.glb') },
  @{ Id='D06'; Title='Kenney Car Kit: taxi'; Author='Kenney (kenney.nl), mirrored on OpenGameArt'; License='CC0'; Page='https://opengameart.org/content/car-kit'; Out=@('sedan/taxi.glb') },
  @{ Id='D07'; Title='Kenney Car Kit: police'; Author='Kenney (kenney.nl), mirrored on OpenGameArt'; License='CC0'; Page='https://opengameart.org/content/car-kit'; Out=@('sedan/police.glb') },
  @{ Id='D08'; Title='Kenney Car Kit: suv'; Author='Kenney (kenney.nl), mirrored on OpenGameArt'; License='CC0'; Page='https://opengameart.org/content/car-kit'; Out=@('suv/mid-suv.glb') },
  @{ Id='D09'; Title='Kenney Car Kit: suv-luxury'; Author='Kenney (kenney.nl), mirrored on OpenGameArt'; License='CC0'; Page='https://opengameart.org/content/car-kit'; Out=@('suv/large-suv.glb') },
  @{ Id='D10'; Title='Mitsubishi L200'; Author='Muhammad Reyhan'; License='CC BY 3.0'; Page='https://poly.pizza/m/4qjS9tFhsJg'; Out=@('suv/pickup.glb') },
  @{ Id='D11'; Title='2015 Dodge Challenger'; Author='Grzybek'; License='CC BY 3.0'; Page='https://poly.pizza/m/1jB8I4t5w4'; Out=@('sports/coupe.glb') },
  @{ Id='D12'; Title='Convertible'; Author='Poly by Google'; License='CC BY 3.0'; Page='https://poly.pizza/m/dggOiBLYyuR'; Out=@('sports/convertible.glb') },
  @{ Id='D13'; Title='Vehicles Assets pt1: car-mpv (silver)'; Author='rubberduck (OpenGameArt)'; License='CC0'; Page='https://opengameart.org/content/vehicles-assets-pt1'; Out=@('mpv/car-mpv-silver.obj','mpv/car-mpv-silver.mtl') },
  @{ Id='D14'; Title='Kenney Car Kit: van'; Author='Kenney (kenney.nl), mirrored on OpenGameArt'; License='CC0'; Page='https://opengameart.org/content/car-kit'; Out=@('van/panel.glb') },
  @{ Id='D15'; Title='Kenney Car Kit: delivery'; Author='Kenney (kenney.nl), mirrored on OpenGameArt'; License='CC0'; Page='https://opengameart.org/content/car-kit'; Out=@('van/delivery.glb') },
  @{ Id='D17'; Title='Free Low Poly Vehicles Pack: Van'; Author='Rgsdev (uploaded by mokalux, OpenGameArt)'; License='CC0'; Page='https://opengameart.org/content/free-low-poly-vehicles-pack'; Out=@('longvan/long-panel.fbx') },
  @{ Id='D18'; Title='Kenney Car Kit: ambulance'; Author='Kenney (kenney.nl), mirrored on OpenGameArt'; License='CC0'; Page='https://opengameart.org/content/car-kit'; Out=@('longvan/ambulance.glb') },
  @{ Id='D19'; Title='Starter Kit Racing: vehicle-motorcycle'; Author='Kenney (kenney.nl)'; License='CC0'; Page='https://github.com/KenneyNL/Starter-Kit-Racing'; Out=@('motorbike/sport.glb') },
  @{ Id='D20'; Title='Motorcycle'; Author='Poly by Google'; License='CC BY 3.0'; Page='https://poly.pizza/m/dse64pqMKAR'; Out=@('motorbike/touring.glb') },
  @{ Id='D21'; Title='RV'; Author='madtrollstudio'; License='CC BY 3.0'; Page='https://poly.pizza/m/zrqfTP5xNh'; Out=@('motorhome/coachbuilt.glb') },
  @{ Id='D22'; Title='Caravan'; Author='Poly by Google'; License='CC BY 3.0'; Page='https://poly.pizza/m/aiDmjN8uOmA'; Out=@('caravan/caravan.glb') },
  @{ Id='D24'; Title='Cargo Truck'; Author='J-Toastie'; License='CC BY 3.0'; Page='https://poly.pizza/m/Fy3WI3uXNQ'; Out=@('rigid/box.glb') },
  @{ Id='D26'; Title='Dump truck'; Author='jeremy'; License='CC BY 3.0'; Page='https://poly.pizza/m/1BpGYg14QGD'; Out=@('rigid/dump.glb') },
  @{ Id='D27'; Title='Free Low Poly Vehicles Pack: Truck with trailer'; Author='Rgsdev (uploaded by mokalux, OpenGameArt)'; License='CC0'; Page='https://opengameart.org/content/free-low-poly-vehicles-pack'; Out=@('semi/box-semi.fbx') },
  @{ Id='D29'; Title='Shipping Container'; Author='Quaternius'; License='CC0'; Page='https://poly.pizza/m/dQXRtm5GbO'; Out=@('semi/container-load.glb') },
  @{ Id='D30'; Title='Truck Tank'; Author='KolosStudios'; License='CC BY 3.0'; Page='https://poly.pizza/m/64ayx6pW3O'; Out=@('semi/tanker-semi.glb') },
  @{ Id='D33'; Title='3D Bus'; Author='ajanhallinta (OpenGameArt)'; License='CC0'; Page='https://opengameart.org/content/3d-bus'; Out=@('bus/coach.fbx') },
  @{ Id='D34'; Title='LowPoly Public Transport: Bus'; Author='Quaternius (quaternius.com), mirrored on OpenGameArt'; License='CC0'; Page='https://opengameart.org/content/lowpoly-public-transport'; Out=@('bus/line-bus.fbx') }
)

$Manual = @(
  @{ Id='R01'; Title='Mini Cooper S'; Author='maregajavier'; User='maregajavier'; License='CC BY 4.0'; Page='https://sketchfab.com/3d-models/ca4076c918bc49d79ebe301343c5787d'; Out='compact/city-car.glb' },
  @{ Id='R02'; Title='Low Poly Polo'; Author='normal|studio'; User='Urmanga'; License='CC BY 4.0'; Page='https://sketchfab.com/3d-models/ad506688595341e880d06e04a5ce2670'; Out='hatchback/supermini.glb' },
  @{ Id='R03'; Title='1998 Volkswagen Golf Mk4'; Author='ImperialBlue3D'; User='ImperialBlue3D'; License='CC BY 4.0'; Page='https://sketchfab.com/3d-models/ec7b2f2fd567419a965db5b07b777c0c'; Out='hatchback/golf.glb' },
  @{ Id='R04'; Title='BMW 320i E90 Low Polly'; Author='rifdanzz'; User='rifdanzz'; License='CC BY 4.0'; Page='https://sketchfab.com/3d-models/28f0bc2f0083417a8e521150529034d6'; Out='sedan/sedan.glb' },
  @{ Id='R05'; Title='Audi A4 B5.5 Avant'; Author='Tomaso'; User='Tomasoo'; License='CC BY 4.0'; Page='https://sketchfab.com/3d-models/2f6aea1435bd4ad093146c019c8e0fb6'; Out='sedan/estate.glb' },
  @{ Id='R08'; Title='Mercedes Benz G-class W263'; Author='Lexyc16'; User='Lexyc16'; License='CC BY 4.0'; Page='https://sketchfab.com/3d-models/1a2a52b16cad4e618af347461817895c'; Out='suv/mid-suv.glb' },
  @{ Id='R09'; Title='BMW X5 E70'; Author='LEON elenaisakova248'; User='elenaisakova248'; License='CC BY 4.0'; Page='https://sketchfab.com/3d-models/d515af89f6c6414b93c537ee1fcfc800'; Out='suv/large-suv.glb' },
  @{ Id='R10'; Title='2019 Ford Ranger Raptor CIVIL (Cities Skylines)'; Author='TSB3DMODELS'; User='TSB3DMODELS'; License='CC BY 4.0'; Page='https://sketchfab.com/3d-models/c2d4113e6acd401cb1999f81bb318325'; Out='suv/pickup.glb' },
  @{ Id='R11'; Title='BMW M4 (F82)'; Author='Mona x Supercars'; User='Car2022'; License='CC BY 4.0'; Page='https://sketchfab.com/3d-models/1d24aac71183435eb6a5ecaba5c0434d'; Out='sports/coupe.glb' },
  @{ Id='R12'; Title='Mercedes-Benz SL 65 AMG Black Series (R230)'; Author='Mona x Supercars'; User='Car2022'; License='CC BY 4.0'; Page='https://sketchfab.com/3d-models/84c255b8a34b496bb1cb076df5ff6e70'; Out='sports/convertible.glb' },
  @{ Id='R13'; Title='VW Sharan'; Author='LEON elenaisakova248'; User='elenaisakova248'; License='CC BY 4.0'; Page='https://sketchfab.com/3d-models/3543b8be92d44f5aa79d8fd9bd9b7835'; Out='mpv/minivan.glb' },
  @{ Id='R14'; Title='VW Transporter Multivan Green'; Author='LEON elenaisakova248'; User='elenaisakova248'; License='CC BY 4.0'; Page='https://sketchfab.com/3d-models/e9bc406f71dd4d888e1d598c9ad05889'; Out='van/panel.glb' },
  @{ Id='R15'; Title='European Delivery Van'; Author='Evan Hiltz'; User='evan.hiltz'; License='CC BY 4.0'; Page='https://sketchfab.com/3d-models/0b2f1ad95a79419f9a092420024d329c'; Out='van/delivery.glb' },
  @{ Id='R16'; Title='Free Low-poly Vintage Hippy Camper Van'; Author='Libau Media'; User='robinmikart'; License='CC BY 4.0'; Page='https://sketchfab.com/3d-models/931d18f3e94b4cd580440bdc85436734'; Out='van/camper.glb' },
  @{ Id='R17'; Title='Mercedes Sprinter Passenger'; Author='kuwalol93'; User='kuwalol93'; License='CC BY 4.0'; Page='https://sketchfab.com/3d-models/d3029b62317c4956bd0a28140ffef83d'; Out='longvan/long-panel.glb' },
  @{ Id='R19'; Title='Suzuki GSXR 750'; Author='Silyboy'; User='kaleemullah550'; License='CC BY 4.0'; Page='https://sketchfab.com/3d-models/3e05cd872ef0458ca3d6b0770bb03622'; Out='motorbike/sport.glb' },
  @{ Id='R20'; Title='Akt Tt Ds 200 repainted'; Author='e-restrepo1114'; User='EmanuelRestrepoVelez'; License='CC BY 4.0'; Page='https://sketchfab.com/3d-models/c32f7b87090d4ce4b10c76e844dc86cd'; Out='motorbike/touring.glb' },
  @{ Id='R21'; Title='Motorhome RV'; Author='Medical Modeller'; User='pistonstone'; License='CC BY 4.0'; Page='https://sketchfab.com/3d-models/4574fbdfb8af498d8e61fd4d43018c9e'; Out='motorhome/coachbuilt.glb' },
  @{ Id='R22'; Title='Large Caravan'; Author='rhcreations'; User='rhcreations'; License='CC BY 4.0'; Page='https://sketchfab.com/3d-models/bc096d0ad5c64ac1a48647200fd559c2'; Out='caravan/caravan.glb' },
  @{ Id='R24'; Title='LCT 3000 ''07- Low poly model'; Author='Daniel Zhabotinsky'; User='DanielZhabotinsky'; License='CC BY 4.0'; Page='https://sketchfab.com/3d-models/3be03b6a43aa41898c9ca806b8787052'; Out='rigid/box.glb' },
  @{ Id='R26'; Title='Japanese Dump Truck'; Author='Pavel Gorbachev'; User='paul_sawyer_11'; License='CC BY 4.0'; Page='https://sketchfab.com/3d-models/bdd68dcc73be430dbffff61017c95fb8'; Out='rigid/dump.glb' },
  @{ Id='R27a'; Title='2021 Volvo FH'; Author='mcskinssketchfab'; User='mcvehiclessketchfab'; License='CC BY 4.0'; Page='https://sketchfab.com/3d-models/7e5f387b6660438aa0942cd17716d196'; Out='semi/tractor-volvo-fh.glb' },
  @{ Id='R27b'; Title='Truck Trailer'; Author='cmitche1'; User='cmitche1'; License='CC BY 4.0'; Page='https://sketchfab.com/3d-models/cd9a5677626f4b648a4a3b1e0d9ca5b9'; Out='semi/trailer-box.glb' },
  @{ Id='R28'; Title='low poly DAF xf105'; Author='Alvin.Woodly'; User='Alvin.Woodly'; License='CC BY 4.0'; Page='https://sketchfab.com/3d-models/51f67c0bd11440dcbe0d1141f5d3508f'; Out='semi/tractor-daf-xf105.glb' },
  @{ Id='R29a'; Title='Flatbed Trailer'; Author='Arsen Ismailov'; User='fdgasd7'; License='CC BY 4.0'; Page='https://sketchfab.com/3d-models/76998b4d82604bb7996768835dcaa276'; Out='semi/trailer-flatbed.glb' },
  @{ Id='R29b'; Title='Truck Container Pack Low-poly ASIA'; Author='knitro_vedant'; User='knitro_vedant'; License='CC BY 4.0'; Page='https://sketchfab.com/3d-models/000b6d08ad7b4f4198dcab2790008553'; Out='semi/containers.glb' },
  @{ Id='R30'; Title='Simple tank trailer'; Author='AndersonCardoso'; User='AndersonCardoso'; License='CC BY 4.0'; Page='https://sketchfab.com/3d-models/738d0e2db6584fe39d4bf9b284c7b614'; Out='semi/trailer-tank.glb' },
  @{ Id='R32'; Title='Mitsubishi Fuso Car Carrier'; Author='ImperialBlue3D'; User='ImperialBlue3D'; License='CC BY 4.0'; Page='https://sketchfab.com/3d-models/4b34a78f69974e62ba0b5b8f461f4360'; Out='longcombo/car-transporter.glb' },
  @{ Id='R33'; Title='1998 Hyundai Aero Space Coach Bus'; Author='ImperialBlue3D'; User='ImperialBlue3D'; License='CC BY 4.0'; Page='https://sketchfab.com/3d-models/b51273fe6c19486eb71e95bc9c5c7032'; Out='bus/coach.glb' },
  @{ Id='R33b'; Title='Coach bus'; Author='own.guest'; User='own.guest'; License='CC BY 4.0'; Page='https://sketchfab.com/3d-models/89866d7d4a314f1f81b024280b1cf416'; Out='bus/coach-alt.glb' },
  @{ Id='R34'; Title='City Bus - rigged / -5236'; Author='Yo.Ri'; User='grox777'; License='CC BY 4.0'; Page='https://sketchfab.com/3d-models/36646fcf41da498ca24a02f4b6fb1d95'; Out='bus/line-bus.glb' },
  @{ Id='R35'; Title='Cee Urbano C7 Volvo B340M - Padro Mobi Rio'; Author='CR3D Modelagens'; User='cr3dmods'; License='CC BY 4.0'; Page='https://sketchfab.com/3d-models/bacf3741cd134f639a77ea635ecd2d5a'; Out='articulated/artic-line.glb' }
)

# Player car (991 rear): Sketchfab, free account, downloaded by hand into Assets/ThirdParty/Player
$PlayerDir = Join-Path $Root 'Assets/ThirdParty/Player'
$ManualPlayer = @(
  @{ Id='P1'; Title='Porsche 911 Carrera S (991)'; Author='Mona x Supercars'; User='Car2022'; License='CC BY 4.0'; Page='https://sketchfab.com/3d-models/97785983d3a949cf87e9ee6bf49f4032'; Out='porsche-991-carrera-s.glb' },
  @{ Id='P2'; Title='(FREE) Porsche 911 Carrera 4S'; Author='Lionsharp Studios'; User='lionsharp'; License='CC BY-SA 4.0'; Page='https://sketchfab.com/3d-models/d01b254483794de3819786d93e0e1ebf'; Out='porsche-991-carrera-4s.glb' }
)
function Resolve-Player([string]$Name) {
  if ($Name -notmatch '^[a-z0-9-]+\.glb$') { throw "Bad player file name: $Name" }
  return (Join-Path $PlayerDir $Name)
}

$LicUrl = @{ 'CC0' = 'https://creativecommons.org/publicdomain/zero/1.0/'; 'CC BY 3.0' = 'https://creativecommons.org/licenses/by/3.0/'; 'CC BY 4.0' = 'https://creativecommons.org/licenses/by/4.0/'; 'CC BY-SA 4.0' = 'https://creativecommons.org/licenses/by-sa/4.0/' }

# ---- helpers ---------------------------------------------------------------------------------------------
function Get-Head([string]$Path, [int]$Count = 64) {
  $fs = [IO.File]::OpenRead($Path)
  try { $buf = New-Object byte[] $Count; $n = $fs.Read($buf, 0, $Count); return $buf[0..([Math]::Max($n, 1) - 1)] } finally { $fs.Close() }
}

function Test-Header([string]$Path, [string]$Ext) {
  if (-not (Test-Path -LiteralPath $Path)) { return $false }
  if ((Get-Item -LiteralPath $Path).Length -lt 64) { return $false }
  $h = Get-Head $Path
  $text = [Text.Encoding]::ASCII.GetString($h)
  switch ($Ext) {
    '.glb'  { return ($text.StartsWith('glTF')) }
    '.zip'  { return ($h[0] -eq 0x50 -and $h[1] -eq 0x4B -and $h[2] -eq 0x03 -and $h[3] -eq 0x04) }
    '.png'  { return ($h[0] -eq 0x89 -and $h[1] -eq 0x50 -and $h[2] -eq 0x4E -and $h[3] -eq 0x47) }
    '.fbx'  { return ($text.StartsWith('Kaydara FBX Binary') -or $text.StartsWith('; FBX')) }
    default { return (-not ($h -contains 0)) }   # .obj, .mtl, .gltf: text without NUL bytes
  }
}

function Test-Url([string]$Url) {
  $u = [Uri]$Url
  if ($u.Scheme -ne 'https' -or $AllowedHosts -notcontains $u.Host) { throw "Host not allowed: $Url" }
}

function Resolve-Target([string]$Rel) {
  $full = [IO.Path]::GetFullPath((Join-Path $Dest $Rel))
  if (-not $full.StartsWith([IO.Path]::GetFullPath($Dest) + [IO.Path]::DirectorySeparatorChar)) { throw "Path escapes the target folder: $Rel" }
  $ext = [IO.Path]::GetExtension($full).ToLowerInvariant()
  if ($AllowedExt -notcontains $ext) { throw "Extension not allowed: $Rel" }
  return $full
}

function Save-Download([string]$Url, [string]$Expected, [string]$Ext) {
  Test-Url $Url
  $tmp = Join-Path ([IO.Path]::GetTempPath()) ('veh_' + [Guid]::NewGuid().ToString('N') + '.dl')
  Invoke-WebRequest -Uri $Url -OutFile $tmp -UserAgent $Ua -UseBasicParsing -MaximumRedirection 3
  $len = (Get-Item -LiteralPath $tmp).Length
  if ($len -gt $MaxBytes -or $len -lt 0.7 * [double]$Expected -or $len -gt 1.3 * [double]$Expected) { Remove-Item -LiteralPath $tmp -Force; throw "Unexpected size $len (expected about $Expected): $Url" }
  if (-not (Test-Header $tmp $Ext)) { Remove-Item -LiteralPath $tmp -Force; throw "Bad file header: $Url" }
  return $tmp
}

# ---- run -------------------------------------------------------------------------------------------------
$ok = New-Object System.Collections.Generic.HashSet[string]
$failed = @()
$temps = @()

foreach ($p in $Packs) {
  $ids = @($p.Files | ForEach-Object { $_.Id } | Select-Object -Unique)
  if ($Only -and -not ($ids | Where-Object { $Only -contains $_ })) { continue }
  $todo = @($p.Files | Where-Object { $Force -or -not (Test-Header (Resolve-Target $_.Out) ([IO.Path]::GetExtension($_.Out).ToLowerInvariant())) })
  if ($todo.Count -eq 0) {
    Write-Host ("skip   {0}  (all files present)" -f ($ids -join ','))
    foreach ($i in $ids) { [void]$ok.Add($i) }
    continue
  }
  if ($DryRun) {
    Write-Host ("plan   {0}  {1}  -> {2}" -f ($ids -join ','), $p.Url, (($todo | ForEach-Object { $_.Out }) -join ', '))
    continue
  }
  try {
    $zipExt = if ($p.Kind -eq 'zip') { '.zip' } else { '.glb' }
    $tmp = Save-Download $p.Url $p.Bytes $zipExt
    $temps += $tmp
    if ($p.Kind -eq 'glb') {
      $f = $todo[0]; $out = Resolve-Target $f.Out
      New-Item -ItemType Directory -Force -Path (Split-Path $out) | Out-Null
      Copy-Item -LiteralPath $tmp -Destination $out -Force
    } else {
      $zip = [IO.Compression.ZipFile]::OpenRead($tmp)
      try {
        foreach ($f in $todo) {
          $e = $zip.Entries | Where-Object { $_.FullName -ieq $f.Entry } | Select-Object -First 1
          if (-not $e) { throw "Zip entry not found: $($f.Entry)" }
          if ($e.Length -gt $MaxBytes) { throw "Zip entry too large: $($f.Entry)" }
          $out = Resolve-Target $f.Out
          New-Item -ItemType Directory -Force -Path (Split-Path $out) | Out-Null
          [IO.Compression.ZipFileExtensions]::ExtractToFile($e, $out, $true)
        }
      } finally { $zip.Dispose() }
    }
    foreach ($f in $todo) {
      $out = Resolve-Target $f.Out
      if (-not (Test-Header $out ([IO.Path]::GetExtension($out).ToLowerInvariant()))) { throw "Bad header after extract: $($f.Out)" }
    }
    Write-Host ("done   {0}  {1}" -f ($ids -join ','), $p.Url)
    foreach ($i in $ids) { [void]$ok.Add($i) }
  } catch {
    Write-Host ("FAILED {0}  {1}  {2}" -f ($ids -join ','), $p.Url, $_.Exception.Message) -ForegroundColor Red
    $failed += $ids
  }
}
foreach ($t in $temps) { Remove-Item -LiteralPath $t -Force -ErrorAction SilentlyContinue }

# ---- credits draft ---------------------------------------------------------------------------------------
if (-not $DryRun) {
  $lines = @('# Vehicle credits (draft)', '', 'Generated by tools/fetch-vehicles.ps1. Merge into CREDITS.md. CC0 needs no credit; CC BY needs the line. Add ", modified" for changed models.', '')
  foreach ($c in $Credits) {
    $present = @($c.Out | Where-Object { Test-Path -LiteralPath (Resolve-Target $_) })
    if ($present.Count -eq 0) { continue }
    $lic = $c.License
    $url = $LicUrl[$lic]
    $lines += ('- "{0}" by {1} ({2}), licensed under {3} ({4}). Files: {5}' -f $c.Title, $c.Author, $c.Page, $lic, $url, ($present -join ', '))
  }
  foreach ($m in $Manual) {
    $out = Resolve-Target $m.Out
    if (-not (Test-Path -LiteralPath $out)) { continue }
    $lic = $m.License; $url = $LicUrl[$lic]
    $lines += ('- "{0}" by {1} (https://sketchfab.com/{2}), licensed under {3} ({4}). File: {5}' -f $m.Title, $m.Author, $m.User, $lic, $url, $m.Out)
  }
  foreach ($m in $ManualPlayer) {
    $out = Resolve-Player $m.Out
    if (-not (Test-Path -LiteralPath $out)) { continue }
    $lic = $m.License; $url = $LicUrl[$lic]
    $lines += ('- "{0}" by {1} (https://sketchfab.com/{2}), licensed under {3} ({4}). File: Player/{5}' -f $m.Title, $m.Author, $m.User, $lic, $url, $m.Out)
  }
  Set-Content -LiteralPath $Draft -Value $lines -Encoding UTF8
  Write-Host "credits draft: $Draft"
}

# ---- manual list -----------------------------------------------------------------------------------------
Write-Host ''
Write-Host 'By hand (Sketchfab, free account): Download 3D Model -> glTF Binary (.glb), save as the target name.'
$missing = 0
foreach ($m in $Manual) {
  $out = Resolve-Target $m.Out
  if (Test-Path -LiteralPath $out) {
    if (Test-Header $out '.glb') { Write-Host ("have   {0}  {1}" -f $m.Id, $m.Out) } else { Write-Host ("BAD    {0}  {1}  (not a GLB)" -f $m.Id, $m.Out) -ForegroundColor Red; $failed += $m.Id }
  } else {
    $missing++
    Write-Host ("manual {0}  {1}  -> {2}" -f $m.Id, $m.Page, $m.Out)
  }
}
foreach ($m in $ManualPlayer) {
  $out = Resolve-Player $m.Out
  if (Test-Path -LiteralPath $out) {
    if (Test-Header $out '.glb') { Write-Host ("have   {0}  Player/{1}" -f $m.Id, $m.Out) } else { Write-Host ("BAD    {0}  Player/{1}  (not a GLB)" -f $m.Id, $m.Out) -ForegroundColor Red; $failed += $m.Id }
  } else {
    $missing++
    Write-Host ("manual {0}  {1}  -> Assets/ThirdParty/Player/{2}" -f $m.Id, $m.Page, $m.Out)
  }
}
Write-Host ("{0} manual file(s) missing, {1} direct item(s) ok, {2} failed" -f $missing, $ok.Count, $failed.Count)
if ($failed.Count -gt 0) { exit 1 }
