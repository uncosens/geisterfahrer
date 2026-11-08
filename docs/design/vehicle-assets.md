# Vehicle Assets — Realistic 3D (free models only)

This is a list of options only. Nothing was downloaded or bought. The developer imports after the director's review.

Data was checked on 2026-11-05 through the Sketchfab API. Triangle counts are Sketchfab's "faceCount" (triangles). Sketchfab does not show texture sizes before download, so **texture size = check on download** for every model. Visual quality was not checked: the Sketchfab 3D viewer doesn't render in our preview browser. **Look at each page's preview before importing.**

## Rules

- **Free only.** The user does not want to pay for models.
- **Personal use only.** The game runs on the user's own devices with no release. So real makes and models are fine, and so are these licences:
  - CC0 (no credit)
  - CC BY (credit required)
  - CC BY-SA (credit; our edited copy keeps the same licence)
  - CC BY-NC / BY-NC-SA (non-commercial; fine for personal use)
  - Sketchfab "Free Standard" (personal and commercial, no credit)
- **Credits:** every CC BY / BY-SA / BY-NC model gets a line in `Assets/Art/CREDITS.md` in this form: `"Title" by Author (link), licensed CC BY 4.0, modified (decimated)`. The developer adds the line when importing.
- ⚠️ If the game is ever released, every real-brand model and every NC model must be replaced.
- **Avoid ripped game assets** even when the uploader set a CC licence. The uploader doesn't own them. Seen in search:
  - "Traffic Racer NPC vehicles pack" by *ModelzRipper*
  - Models titled "Bus Simulator: Original" by *Villanueva-Jonatan-32621*, incl. his "Setra S 431 DT (Bus8)" and "MAN Lion's Coach"
- **Format:** every downloadable Sketchfab model offers **glTF/GLB** (auto-converted), plus the original upload if the author added one. Import GLB with the glTFast package, or convert to FBX in Blender, whichever `docs/dev/unity-plan.md` uses.

## Budget (from `docs/dev/unity-plan.md` §6)

| Slot | Triangles | Textures |
|---|---|---|
| Player car | < 20k | max 1024² per vehicle (ASTC 6×6) |
| Traffic car / van | 3–10k, LOD1 at about half beyond 80 m | max 1024² |
| Truck (and coach, treated the same) | up to 15k | max 1024² |

The "Cut" column below is the triangle reduction needed to reach budget (Blender Decimate or Unity LOD tools).

Every model also needs:
- Its materials merged to 1–2 (body + glass).
- Textures downscaled to 1024².
- Scale checked against the real length.
- Wheels separated if they aren't already.

---

## Player car — Porsche 911

| | Model | Licence | Tris | Cut | Notes |
|---|---|---|---|---|---|
| **1st** | [Porsche 911 S 992 Low Poly](https://sketchfab.com/3d-models/porsche-911-s-992-low-poly-16881132aa764a18ad465e2a1d27851b) by Pavel6678 | CC BY | 15.9k | **none** | 992 generation, already inside budget. 17 materials → merge to 2–3. Least cleanup of all 911s found. Published Nov 2025. |
| Fallback | [2019 Porsche 911 Carrera 4S (992)](https://sketchfab.com/3d-models/2019-porsche-911-carrera-4s-992-4e324fdfada24355aba3f34cf09a9552) by ddiaz-design | CC BY-NC-SA | 55.3k | about 65 % | Likely the more detailed and realistic one (the author makes detailed car models), but it needs decimation. Use it if the 1st pick looks too simple. |
| Other | [Low-Poly 2024 Porsche 911](https://sketchfab.com/3d-models/low-poly-2024-porsche-911-79968fc4820c4524aed2c697ad10db81) by Kyaie | CC BY | 4.2k | none | Very light. Probably too simple for "as real as possible". |
| Other | [2021 Porsche 911 GT3](https://sketchfab.com/3d-models/2021-porsche-911-gt3-931e8121d2f3495f97f6836fb99ccaac) by outpiston | CC BY-NC-SA | 46.7k | about 57 % | GT3 with a rear wing. |

Most free 992 models on Sketchfab are 116k–1M triangles (render models). They were left out.

## Compact — VW Golf

| | Model | Licence | Tris | Cut | Notes |
|---|---|---|---|---|---|
| **1st** | [1998 Volkswagen Golf Mk4](https://sketchfab.com/3d-models/1998-volkswagen-golf-mk4-ec7b2f2fd567419a965db5b07b777c0c) by ImperialBlue3D | CC BY | 5.4k | none | Fits traffic budget as-is. Still a common sight. |
| Fallback | [Volkswagen Golf (Plus) 2011](https://sketchfab.com/3d-models/volkswagen-golf-plus-2011-2dd0f5839b7f4b6287761c962ade7dc0) by Preview2SEBIT69 | CC BY | 17.7k | about 45 % | Newer and taller (Golf Plus). |

## Sedan

| | Model | Licence | Tris | Cut | Notes |
|---|---|---|---|---|---|
| **1st** | [Low Poly BMW M5 F90](https://sketchfab.com/3d-models/low-poly-bmw-m5-f90-87102e4f8f2b42c087ff1a4423b5cccd) by kulonee (@ynesolefru) | CC BY | 5.7k | none | Modern German sedan, inside budget. The same author also makes stylised cars, so check the preview for a realistic look. |
| Fallback | [Low Poly Car - BMW E46 1998](https://sketchfab.com/3d-models/low-poly-car-bmw-e46-1998-d9bfd8126e754164b48ded833c017bbf) by roh3d | CC BY | 18.4k | about 45 % | Classic 3 Series. |

## Van — Mercedes Sprinter

| | Model | Licence | Tris | Cut | Notes |
|---|---|---|---|---|---|
| **1st** | [Mercedes Sprinter Passenger](https://sketchfab.com/3d-models/mercedes-sprinter-passenger-d3029b62317c4956bd0a28140ffef83d) by kuwalol93 | CC BY | 11.4k | about 15 % (or keep; only just over) | Right vehicle. Remove the interior if it has one. |
| Fallback | [Mercades Sprinter car LOW POLY](https://sketchfab.com/3d-models/mercades-sprinter-car-low-poly-9f1459c92b2447f2a3c1476d11fbbf9a) by randomcarguy | CC BY | 4.7k | none | Lighter, simpler. |

## Coach bus

| | Model | Licence | Tris | Cut | Notes |
|---|---|---|---|---|---|
| **1st** | [Coach bus](https://sketchfab.com/3d-models/coach-bus-89866d7d4a314f1f81b024280b1cf416) by own.guest | CC BY | 11.2k | none (truck budget 15k) | Generic modern coach shape. No free German-brand coach was found from an uploader who owns the model. |
| Fallback | [1998 Hyundai Aero Space Coach Bus](https://sketchfab.com/3d-models/1998-hyundai-aero-space-coach-bus-b51273fe6c19486eb71e95bc9c5c7032) by ImperialBlue3D | CC BY | 2.3k | none | Very light. Korean coach, older look. Same author as the Golf pick, so the style matches. |

## Semi truck

| | Model | Licence | Tris | Cut | Notes |
|---|---|---|---|---|---|
| **1st** | [Mercedes Actros with flatbed semi-trailer](https://sketchfab.com/3d-models/mercedes-actros-com-semi-reboque-prancha-carrega-8ee4c1329219413e879760a25208ee15) by LuizZed36 | CC BY | 10.4k | none | European cab-over with a trailer, inside the 15k truck budget. The trailer is a flatbed; a box or tarp trailer would look more German, but this is fine for a start. |
| Fallback | [MAN TGX 2010 V8 – High-Detail Semi-Truck](https://sketchfab.com/3d-models/man-tgx-2010-v8-high-detail-semi-truck-34ce748d9a4b4de7b035cfe5cb97f5d1) by DevPoly3D | CC BY | 37.7k | about 60 % | Detailed German truck. Check whether a trailer is included; if not, pair it with the simple [Trailer](https://sketchfab.com/3d-models/trailer-05261a6f0679439b8dbce690522b9ee1) (616 tris, CC BY). |
| Last resort | [Scania 352 R440](https://sketchfab.com/3d-models/scania-352-r440-1df36a77cfb84f63a19e36325ed982c4) + [Trailer](https://sketchfab.com/3d-models/trailer-05261a6f0679439b8dbce690522b9ee1) by zairiq-123 | CC BY | 2.0k + 0.6k | none | Very simple shapes. |

## Road and roadside (optional)

The road surface, lane lines, guardrail runs and the median are built by code from road segments (see `art-direction-3d.md` §4), so only textures and a few props are needed.

| Use | Asset | Licence | Tris | Notes |
|---|---|---|---|---|
| Asphalt, concrete, grass, clear-day HDRI sky | [Poly Haven](https://polyhaven.com) textures and HDRIs, ambientCG | CC0 | – | First choice. No credit needed. |
| Guardrail piece | [Steel Road Barrier](https://sketchfab.com/3d-models/steel-road-barrier-438c36d48ab04ec4b5178c6989d3ab3b) by shockproof | CC BY | 2.4k | Check that it is a W-profile. Otherwise extrude a W-profile in code (cheapest and exact). |
| Guardrail, light | [Guardrails Avenue metallic low poly](https://sketchfab.com/3d-models/guardrails-avenue-metallic-low-poly-da05e7a16cdc4519b0aea22b36739f6a) by ahmagh2e | CC BY | 0.5k | A repeating piece for long runs. |
| Guardrail end | [GuardRail and Terminal](https://sketchfab.com/3d-models/guardrail-and-terminal-693dee3dc6a1495f90ab0ba1f8237cb4) by xplanepilot | CC BY | 2.9k | For rail starts near overpass piers. |
| Motorway light pole | [AEL AUTOBAHN ATBM FRWY streetlight](https://sketchfab.com/3d-models/ael-autobahn-atbm-frwy-streetlight-dba1bb6b1d1046a4b9ef0e41e0c3850e) by daffyxxcoolxx | CC BY | 22k | Too heavy for many copies. Use it as a reference, or decimate it hard (about 90 %) for occasional use. |

Blue signs and gantries are simple boxes plus a sign texture. It is faster to make them in Unity than to clean a download.

---

## Recommended free set

| Slot | 1st pick | Fallback | Cleanup |
|---|---|---|---|
| Player 911 | 911 S 992 Low Poly (15.9k) | Carrera 4S 992 (55k, cut 65 %) | merge materials |
| Compact | Golf Mk4 (5.4k) | Golf Plus 2011 (17.7k, cut 45 %) | small |
| Sedan | BMW M5 F90 (5.7k) | BMW E46 (18.4k, cut 45 %) | small |
| Van | Sprinter Passenger (11.4k) | Sprinter LOW POLY (4.7k) | trim about 15 % |
| Coach | Coach bus (11.2k) | Hyundai Aero Space (2.3k) | small |
| Semi | Actros + flatbed (10.4k) | MAN TGX (37.7k, cut 60 %) | small |
| **Cost** | **$0** | **$0** | |

With all 1st picks, only the 911's materials and the Sprinter's slight excess need work. Every other pick is already inside the budget.

Credits needed: all picks above are CC BY (or BY-NC-SA for the fallback 911). Six to twelve lines go in `CREDITS.md`.

## Redistribution and attribution (for the public repo README)

The repo is public, but **model files stay out of git**. Each user downloads them from the links. So we never redistribute the models, and only credit lines go in the README.

- **CC BY 4.0:** redistribution is allowed, including modified copies, with credit.
- **CC BY-NC-SA 4.0:** redistribution is allowed only non-commercially, and modified copies keep the same licence.
- **CC0:** anything goes, and no credit is needed.
- Sketchfab's own CC licences are version 4.0.

| Slot | Model | Licence | Redistribution allowed? | Attribution line for the README |
|---|---|---|---|---|
| Player (1st) | 911 S 992 Low Poly | CC BY 4.0 | Yes, with credit | "Porsche 911 S 992 Low Poly" (https://sketchfab.com/3d-models/16881132aa764a18ad465e2a1d27851b) by Pavel6678 (https://sketchfab.com/Pavel6678), licensed under CC BY 4.0 (https://creativecommons.org/licenses/by/4.0/) |
| Player (fallback) | 2019 911 Carrera 4S (992) | CC BY-NC-SA 4.0 | Non-commercial only, same licence | "2019 Porsche 911 Carrera 4S (992)" (https://sketchfab.com/3d-models/4e324fdfada24355aba3f34cf09a9552) by ddiaz-design (https://sketchfab.com/ddiaz-design), licensed under CC BY-NC-SA 4.0 (https://creativecommons.org/licenses/by-nc-sa/4.0/) |
| Compact (1st) | 1998 Volkswagen Golf Mk4 | CC BY 4.0 | Yes, with credit | "1998 Volkswagen Golf Mk4" (https://sketchfab.com/3d-models/ec7b2f2fd567419a965db5b07b777c0c) by ImperialBlue3D (https://sketchfab.com/ImperialBlue3D), licensed under CC BY 4.0 (https://creativecommons.org/licenses/by/4.0/) |
| Compact (fallback) | Volkswagen Golf (Plus) 2011 | CC BY 4.0 | Yes, with credit | "Volkswagen golf (PLUS) 2011" (https://sketchfab.com/3d-models/2dd0f5839b7f4b6287761c962ade7dc0) by Preview2SEBIT69 (https://sketchfab.com/Preview2SEBIT69), licensed under CC BY 4.0 (https://creativecommons.org/licenses/by/4.0/) |
| Sedan (1st) | Low Poly BMW M5 F90 | CC BY 4.0 | Yes, with credit | "Low Poly BMW M5 F90" (https://sketchfab.com/3d-models/87102e4f8f2b42c087ff1a4423b5cccd) by kulonee (https://sketchfab.com/ynesolefru), licensed under CC BY 4.0 (https://creativecommons.org/licenses/by/4.0/) |
| Sedan (fallback) | Low Poly Car - BMW E46 1998 | CC BY 4.0 | Yes, with credit | "Low Poly Car - BMW E46 1998" (https://sketchfab.com/3d-models/d9bfd8126e754164b48ded833c017bbf) by roh3d (https://sketchfab.com/roh3d), licensed under CC BY 4.0 (https://creativecommons.org/licenses/by/4.0/) |
| Van (1st) | Mercedes Sprinter Passenger | CC BY 4.0 | Yes, with credit | "Mercedes Sprinter Passenger" (https://sketchfab.com/3d-models/d3029b62317c4956bd0a28140ffef83d) by kuwalol93 (https://sketchfab.com/kuwalol93), licensed under CC BY 4.0 (https://creativecommons.org/licenses/by/4.0/) |
| Van (fallback) | Mercades Sprinter car LOW POLY | CC BY 4.0 | Yes, with credit | "Mercades Sprinter car LOW POLY" (https://sketchfab.com/3d-models/9f1459c92b2447f2a3c1476d11fbbf9a) by randomcarguy (https://sketchfab.com/randomcarguy), licensed under CC BY 4.0 (https://creativecommons.org/licenses/by/4.0/) |
| Coach (1st) | Coach bus | CC BY 4.0 | Yes, with credit | "Coach bus" (https://sketchfab.com/3d-models/89866d7d4a314f1f81b024280b1cf416) by own.guest (https://sketchfab.com/own.guest), licensed under CC BY 4.0 (https://creativecommons.org/licenses/by/4.0/) |
| Coach (fallback) | 1998 Hyundai Aero Space Coach Bus | CC BY 4.0 | Yes, with credit | "1998 Hyundai Aero Space Coach Bus" (https://sketchfab.com/3d-models/b51273fe6c19486eb71e95bc9c5c7032) by ImperialBlue3D (https://sketchfab.com/ImperialBlue3D), licensed under CC BY 4.0 (https://creativecommons.org/licenses/by/4.0/) |
| Semi (1st) | Mercedes Actros com Semi Reboque Prancha Carrega | CC BY 4.0 | Yes, with credit | "Mercedes Actros com Semi Reboque Prancha Carrega" (https://sketchfab.com/3d-models/8ee4c1329219413e879760a25208ee15) by LuizZed36 (https://sketchfab.com/LuizZed36), licensed under CC BY 4.0 (https://creativecommons.org/licenses/by/4.0/) |
| Semi (fallback) | MAN TGX 2010 V8 – High-Detail Semi-Truck | CC BY 4.0 | Yes, with credit | "MAN TGX 2010 V8 – High-Detail Semi-Truck" (https://sketchfab.com/3d-models/34ce748d9a4b4de7b035cfe5cb97f5d1) by DevPoly3D (https://sketchfab.com/DevPoly3D), licensed under CC BY 4.0 (https://creativecommons.org/licenses/by/4.0/) |
| Semi (last resort) | Scania 352 r440 + Trailer | CC BY 4.0 | Yes, with credit | "Scania 352 r440" (https://sketchfab.com/3d-models/1df36a77cfb84f63a19e36325ed982c4) and "Trailer" (https://sketchfab.com/3d-models/05261a6f0679439b8dbce690522b9ee1) by zairiq-123 (https://sketchfab.com/zairiq-123), licensed under CC BY 4.0 (https://creativecommons.org/licenses/by/4.0/) |
| Road | Steel Road Barrier | CC BY 4.0 | Yes, with credit | "Steel Road Barrier" (https://sketchfab.com/3d-models/438c36d48ab04ec4b5178c6989d3ab3b) by shockproof (https://sketchfab.com/shockproof), licensed under CC BY 4.0 (https://creativecommons.org/licenses/by/4.0/) |
| Road | Guardrails Avenue metallic low poly | CC BY 4.0 | Yes, with credit | "Guardrails Avenue metallic low poly" (https://sketchfab.com/3d-models/da05e7a16cdc4519b0aea22b36739f6a) by ahmagh2e (https://sketchfab.com/ahmagh2e), licensed under CC BY 4.0 (https://creativecommons.org/licenses/by/4.0/) |
| Road | GuardRail and Terminal | CC BY 4.0 | Yes, with credit | "GuardRail and Terminal" (https://sketchfab.com/3d-models/693dee3dc6a1495f90ab0ba1f8237cb4) by xplanepilot (https://sketchfab.com/xplanepilot), licensed under CC BY 4.0 (https://creativecommons.org/licenses/by/4.0/) |
| Textures, HDRI | Poly Haven, ambientCG | CC0 | Yes, no conditions | None required. Optional: "Textures and HDRIs from Poly Haven (polyhaven.com) and ambientCG (ambientcg.com), CC0." |

Notes:
- Use only the lines for the models actually used. If a model is modified (decimated, materials merged), add ", modified" at the end of its line, as CC BY asks us to say so.
- Model titles are copied as the authors wrote them, including their typos ("Mercades").
- Author profile URLs use the Sketchfab usernames from the API. Click each one once before publishing the README.
- Car makes in the titles (Porsche, VW, BMW, Mercedes, MAN, Scania, Hyundai) are trademarks of their owners. A one-line README note fits a personal, non-commercial fan project: "Vehicle names and designs belong to their respective owners; this project is non-commercial and not affiliated with them."

## Damage support

See `art-direction-3d.md` §7. None of these free models have damage variants.
- **Dents:** done by runtime vertex deformation on the player car. Enable Read/Write on its mesh.
- **Scratches:** use decals.
- **Loose parts:** if the 911 model has a separate bonnet or bumpers (check after import), a heavy hit can swing or drop them.
- **Traffic:** scratch decals only.

## Paid, not used

Dropped because the user wants free only. Kept here in case that changes:
- [911 992 Turbo S Gameready](https://cgtrader.com/3d-models/car/sport-car/porsche-911-992-turbo-s-2021-gameready) ($75)
- [VW Golf 2024](https://www.cgtrader.com/3d-models/car/car/volkswagen-golf-2024) ($25)
- [Realistic Mobile Car series](https://assetstore.unity.com/packages/3d/vehicles/land/realistic-mobile-car-01-279524) ($16 each)
- [Setra S 415 GT-HD](https://www.cgtrader.com/3d-models/vehicle/bus/setra-s-415-gt-hd)
- [Scania cargo semi truck 09](https://www.cgtrader.com/3d-models/vehicle/truck/cargo-semi-truck-3d-model-09) (about $24)
- [Traffic Cars Pack #1](https://assetstore.unity.com/packages/3d/vehicles/land/traffic-cars-pack-1-189672) ($10.99)

## Sources

- Sketchfab Data API search (`api.sketchfab.com/v3/search`, downloadable only) and the model pages linked above, checked 2026-11-05.
- Budget: `docs/dev/unity-plan.md` §6.


## Traffic variants: free sources for the 35 first-wave variants (GDD 4.1.3)

Made on 2026-11-07 from public listings and public APIs only. Nothing was downloaded and no account was used. The download script is `tools/fetch-vehicles.ps1` (see `vehicle-fetch.md`).

**Three kinds of source**

- **D = direct, no login.** A plain file URL (poly.pizza static GLB, OpenGameArt zip, GitHub raw). The script downloads these. **Mostly stylized low-poly**, because no realistic free model was found without a login.
- **R = realistic, Sketchfab, free account needed.** The user downloads by hand (Download 3D Model, then glTF Binary). The page link is in the table. These are the realistic models, so use them wherever one exists.
- **Reuse / assembly:** taxi, police and ambulance are a paint job on a downloaded model (no separate model). The semi variants are a tractor plus a trailer, two or three downloads.

**Reading the tables.** Tris = triangles (Sketchfab: the GLB archive; poly.pizza: the page; `est.` = not measured, estimated from file size). Reduce = share of triangles to remove to reach the 15k traffic budget (`docs/dev/unity-plan.md` §6); textures also go down to 1024 px. Real length is the model's real vehicle length (approximate); the class length comes from GDD 4.1.1 and the model is scaled to it. Style: S = stylized, L = low-poly but realistic proportions.

**Licences.** CC0 needs no credit. CC BY needs the line in `CREDITS.md` (the script writes a draft). No CC BY-NC, BY-SA or ND models are used. poly.pizza lists its models as CC BY 3.0 (the Google Poly archive and other authors); Sketchfab models are CC BY 4.0. Models stay out of git.

**Style mixing.** Do not put a stylized Kenney car next to a realistic Sketchfab car on the same road. Plan: use R wherever it exists; use D only for the gaps below, and for a quick first test of the pipeline.

### Coverage (35 first-wave variants)

| # | Class | Variant | D (no login) | R (login) | Status |
|---|---|---|---|---|---|
| 1 | Compact (3.7 m) | City car | D01 | R01 | D + R |
| 2 | Hatchback (4.15 m) | Supermini | D02 | R02 | D + R |
| 3 | Hatchback (4.15 m) | Golf-class hatchback | D03 | R03 | D + R |
| 4 | Sedan (4.8 m) | Sedan | D04 | R04 | D + R |
| 5 | Sedan (4.8 m) | Estate (wagon) | D05 | R05 | D + R |
| 6 | Sedan (4.8 m) | Taxi | D06 | R04/R05 + cream livery | D + R |
| 7 | Sedan (4.8 m) | Police car | D07 | R04 + blue/silver livery | D + R |
| 8 | SUV (4.9 m) | Mid SUV | D08 | R08 | D + R |
| 9 | SUV (4.9 m) | Large SUV | D09 | R09 | D + R |
| 10 | SUV (4.9 m) | Pickup (double cab) | D10 | R10 | D + R |
| 11 | Sports car (4.5 m) | Coupe | D11 | R11 | D + R |
| 12 | Sports car (4.5 m) | Convertible | D12 | R12 | D + R |
| 13 | MPV (4.6 m) | Minivan | D13 | R13 | D + R |
| 14 | Van (5.5 m) | Panel van | D14 | R14 | D + R |
| 15 | Van (5.5 m) | Delivery van | D15 | R15 | D + R |
| 16 | Van (5.5 m) | Camper van (pop-top) | none | R16 | R only |
| 17 | Long van (5.9 m) | Long panel van | D17 | R17 | D + R |
| 18 | Long van (5.9 m) | Ambulance | D18 | R17 + white/yellow-green livery | D + R |
| 19 | Motorbike (2.2 m) | Sport bike | D19 | R19 | D + R |
| 20 | Motorbike (2.2 m) | Touring bike | D20 | R20 | D + R |
| 21 | Motorhome (7.5 m) | Coachbuilt motorhome | D21 | R21 | D + R |
| 22 | Car with caravan (11 m) | SUV with caravan | D22 | R22 | D + R |
| 23 | Car with caravan (11 m) | Estate with caravan | D22 | R22 | D + R |
| 24 | Rigid truck (9 m) | Box truck | D24 | R24 | D + R |
| 25 | Rigid truck (9 m) | Curtainsider | none | none found | **none** |
| 26 | Rigid truck (9 m) | Dump truck (3-axle) | D26 | R26 | D + R |
| 27 | Semi truck (16.5 m) | Box semi | D27 | R27a + R27b (assembly) | D + R |
| 28 | Semi truck (16.5 m) | Curtain semi | none | R27a/R28 tractor + R27b trailer, reskinned (partial) | R only |
| 29 | Semi truck (16.5 m) | Container semi | D29 (container load only) | R27a + R29a + R29b (assembly) | R only |
| 30 | Semi truck (16.5 m) | Tanker semi | D30 | R27a + R30 (assembly) | D + R |
| 31 | Long combination (19 m) | Swap-body combination | none | none found | **none** |
| 32 | Long combination (19 m) | Car transporter | none | R32 (rigid carrier, wrong length) (partial) | R only |
| 33 | Bus (12 m) | Coach | D33 | R33, R33b | D + R |
| 34 | Bus (12 m) | Line bus (intercity) | D34 | R34 | D + R |
| 35 | Articulated bus (18 m) | Articulated line bus | none | R35 | R only |

### D: direct downloads, no login (28 files, 28 variants)

| ID | Variant | Model (author) | Link | Licence | Format | Tris | Texture | Style | Reduce | Real length | Login |
|---|---|---|---|---|---|---|---|---|---|---|---|
| D01 | 1 City car | Vehicles Assets pt1: car-micro (silver) (rubberduck (OpenGameArt)) | [page](https://opengameart.org/content/vehicles-assets-pt1) | CC0 | OBJ+MTL | est. ~1k (not measured) | none (colour materials) | S | none | n/a (stylized micro car) | no |
| D02 | 2 Supermini | Car Hatchback (Kay Lousberg (KayKit)) | [page](https://poly.pizza/m/BG0KAhmGDt) | CC0 | GLB | 1,194 | palette (small) | S | none | n/a (stylized) | no |
| D03 | 3 Golf-class hatchback | Kenney Car Kit: hatchback-sports (Kenney (kenney.nl), mirrored on OpenGameArt) | [page](https://opengameart.org/content/car-kit) | CC0 | GLB | est. 1-3k (not measured) | shared 1 colour map | S | none | n/a (stylized) | no |
| D04 | 4 Sedan | Kenney Car Kit: sedan (Kenney (kenney.nl), mirrored on OpenGameArt) | [page](https://opengameart.org/content/car-kit) | CC0 | GLB | est. 1-3k (not measured) | shared 1 colour map | S | none | n/a (stylized) | no |
| D05 | 5 Estate (wagon) | Stationwagon (Kay Lousberg (KayKit)) | [page](https://poly.pizza/m/vTTTjDoxhV) | CC0 | GLB | 1,234 | palette (small) | S | none | n/a (stylized) | no |
| D06 | 6 Taxi | Kenney Car Kit: taxi (Kenney (kenney.nl), mirrored on OpenGameArt) | [page](https://opengameart.org/content/car-kit) | CC0 | GLB | est. 1-3k (not measured) | shared 1 colour map | S | none | n/a (stylized) | no |
| D07 | 7 Police car | Kenney Car Kit: police (Kenney (kenney.nl), mirrored on OpenGameArt) | [page](https://opengameart.org/content/car-kit) | CC0 | GLB | est. 1-3k (not measured) | shared 1 colour map | S | none | n/a (stylized) | no |
| D08 | 8 Mid SUV | Kenney Car Kit: suv (Kenney (kenney.nl), mirrored on OpenGameArt) | [page](https://opengameart.org/content/car-kit) | CC0 | GLB | est. 1-3k (not measured) | shared 1 colour map | S | none | n/a (stylized) | no |
| D09 | 9 Large SUV | Kenney Car Kit: suv-luxury (Kenney (kenney.nl), mirrored on OpenGameArt) | [page](https://opengameart.org/content/car-kit) | CC0 | GLB | est. 1-3k (not measured) | shared 1 colour map | S | none | n/a (stylized) | no |
| D10 | 10 Pickup (double cab) | Mitsubishi L200 (Muhammad Reyhan) | [page](https://poly.pizza/m/4qjS9tFhsJg) | CC BY 3.0 | GLB | 2,899 | not checked (file 0.2 MB) | L | none | ~5.3 m (class 4.9: scale down 8 %) | no |
| D11 | 11 Coupe | 2015 Dodge Challenger (Grzybek) | [page](https://poly.pizza/m/1jB8I4t5w4) | CC BY 3.0 | GLB | 6,857 | not checked (file 0.8 MB) | L | none | ~5.0 m (class 4.5: scale down 10 %) | no |
| D12 | 12 Convertible | Convertible (Poly by Google) | [page](https://poly.pizza/m/dggOiBLYyuR) | CC BY 3.0 | GLB | 1,671 | not checked (file 1.8 MB) | S | none | n/a (stylized) | no |
| D13 | 13 Minivan | Vehicles Assets pt1: car-mpv (silver) (rubberduck (OpenGameArt)) | [page](https://opengameart.org/content/vehicles-assets-pt1) | CC0 | OBJ+MTL | est. ~1k (not measured) | none (colour materials) | S | none | n/a (stylized) | no |
| D14 | 14 Panel van | Kenney Car Kit: van (Kenney (kenney.nl), mirrored on OpenGameArt) | [page](https://opengameart.org/content/car-kit) | CC0 | GLB | est. 1-3k (not measured) | shared 1 colour map | S | none | n/a (stylized) | no |
| D15 | 15 Delivery van | Kenney Car Kit: delivery (Kenney (kenney.nl), mirrored on OpenGameArt) | [page](https://opengameart.org/content/car-kit) | CC0 | GLB | est. 1-3k (not measured) | shared 1 colour map | S | none | n/a (stylized) | no |
| D17 | 17 Long panel van | Free Low Poly Vehicles Pack: Van (Rgsdev (uploaded by mokalux, OpenGameArt)) | [page](https://opengameart.org/content/free-low-poly-vehicles-pack) | CC0 | FBX | est. ~1-2k (not measured) | none (colour materials) | S | none | n/a (stylized) | no |
| D18 | 18 Ambulance | Kenney Car Kit: ambulance (Kenney (kenney.nl), mirrored on OpenGameArt) | [page](https://opengameart.org/content/car-kit) | CC0 | GLB | est. 1-3k (not measured) | shared 1 colour map | S | none | n/a (stylized) | no |
| D19 | 19 Sport bike | Starter Kit Racing: vehicle-motorcycle (Kenney (kenney.nl)) | [page](https://github.com/KenneyNL/Starter-Kit-Racing) | CC0 | GLB | est. ~1-2k (not measured) | shared colour map (embedded?) | S | none | n/a (stylized) | no |
| D20 | 20 Touring bike | Motorcycle (Poly by Google) | [page](https://poly.pizza/m/dse64pqMKAR) | CC BY 3.0 | GLB | 6,361 | not checked (file 0.1 MB) | S | none | n/a (stylized) | no |
| D21 | 21 Coachbuilt motorhome | RV (madtrollstudio) | [page](https://poly.pizza/m/zrqfTP5xNh) | CC BY 3.0 | GLB | 2,754 | not checked (file 0.2 MB) | L | none | ~7 m (class 7.5) | no |
| D22 | 22, 23 SUV with caravan / Estate with caravan | Caravan (Poly by Google) | [page](https://poly.pizza/m/aiDmjN8uOmA) | CC BY 3.0 | GLB | 4,692 | not checked (file 0.2 MB) | S | none | ~5-6 m caravan; tow it with D08 or D05 | no |
| D24 | 24 Box truck | Cargo Truck (J-Toastie) | [page](https://poly.pizza/m/Fy3WI3uXNQ) | CC BY 3.0 | GLB | 4,287 | not checked (file 0.3 MB) | S | none | n/a (stylized) | no |
| D26 | 26 Dump truck (3-axle) | Dump truck (jeremy) | [page](https://poly.pizza/m/1BpGYg14QGD) | CC BY 3.0 | GLB | 2,916 | not checked (file 0.1 MB) | S | none | n/a (stylized) | no |
| D27 | 27 Box semi | Free Low Poly Vehicles Pack: Truck with trailer (Rgsdev (uploaded by mokalux, OpenGameArt)) | [page](https://opengameart.org/content/free-low-poly-vehicles-pack) | CC0 | FBX | est. ~2-3k (not measured) | none (colour materials) | S | none | n/a (stylized) | no |
| D29 | 29 Container semi | Shipping Container (Quaternius) | [page](https://poly.pizza/m/dQXRtm5GbO) | CC0 | GLB | 1,336 | palette (small) | S | none | 12 m container | no |
| D30 | 30 Tanker semi | Truck Tank (KolosStudios) | [page](https://poly.pizza/m/64ayx6pW3O) | CC BY 3.0 | GLB | 736 | not checked (file 0.05 MB) | S | none | n/a (stylized) | no |
| D33 | 33 Coach | 3D Bus (ajanhallinta (OpenGameArt)) | [page](https://opengameart.org/content/3d-bus) | CC0 | FBX | 7.4k (per OGA page, faces) | none (colour materials) | S | none | n/a (stylized) | no |
| D34 | 34 Line bus (intercity) | LowPoly Public Transport: Bus (Quaternius (quaternius.com), mirrored on OpenGameArt) | [page](https://opengameart.org/content/lowpoly-public-transport) | CC0 | FBX | est. ~1k (not measured) | none (colour materials) | S | none | n/a (stylized) | no |

Notes: D22 serves two variants (SUV with caravan and Estate with caravan). D29 is only a container load, not a vehicle. Kenney GLBs use one shared colour map that the script puts in each class folder (`Textures/colormap.png`). OBJ and FBX files keep their native format; Unity imports them directly. OBJ comes with its `.mtl`.

### R: realistic Sketchfab models, download by hand (32 models)

| ID | Variant | Model (author) | Link | Licence | Format | Tris | Texture | Reduce | Real length / note | Login |
|---|---|---|---|---|---|---|---|---|---|---|
| R01 | 1 City car | Mini Cooper S (maregajavier) | [page](https://sketchfab.com/3d-models/ca4076c918bc49d79ebe301343c5787d) | CC BY 4.0 | GLB | 14,287 | 512 | - | 3.7 m (fits); Mini Cooper S | yes |
| R02 | 2 Supermini | Low Poly Polo (normal/studio) | [page](https://sketchfab.com/3d-models/ad506688595341e880d06e04a5ce2670) | CC BY 4.0 | GLB | 9,269 | 2048 | - | ~3.6 m (old Polo shape; class 4.15: scale up 13 %) | yes |
| R03 | 3 Golf-class hatchback | 1998 Volkswagen Golf Mk4 (ImperialBlue3D) | [page](https://sketchfab.com/3d-models/ec7b2f2fd567419a965db5b07b777c0c) | CC BY 4.0 | GLB | 5,420 | 512 | - | 4.15 m (fits); already in the game (Compact.glb) | yes |
| R04 | 4, 6, 7 Sedan / Taxi ... | BMW 320i E90 Low Polly (rifdanzz) | [page](https://sketchfab.com/3d-models/28f0bc2f0083417a8e521150529034d6) | CC BY 4.0 | GLB | 13,441 | 2048 | - | 4.5 m (class 4.8: scale up 6 %); also the base for taxi (cream) and police (silver/blue) | yes |
| R05 | 5, 6, 7 Estate (wagon) / Taxi ... | Audi A4 B5.5 Avant (Tomaso) | [page](https://sketchfab.com/3d-models/2f6aea1435bd4ad093146c019c8e0fb6) | CC BY 4.0 | GLB | 27,083 | 2048 | -45 % | 4.5 m (class 4.8: scale up 6 %); also taxi/police base | yes |
| R08 | 8 Mid SUV | Mercedes Benz G-class W263 (Lexyc16) | [page](https://sketchfab.com/3d-models/1a2a52b16cad4e618af347461817895c) | CC BY 4.0 | GLB | 12,516 | 128 | - | 4.8 m (fits); boxy; textures 128 px (flat colours) | yes |
| R09 | 9 Large SUV | BMW X5 E70 (LEON elenaisakova248) | [page](https://sketchfab.com/3d-models/d515af89f6c6414b93c537ee1fcfc800) | CC BY 4.0 | GLB | 6,742 | 512 | - | 4.85 m (fits) | yes |
| R10 | 10 Pickup (double cab) | 2019 Ford Ranger Raptor CIVIL (Cities Skylines) (TSB3DMODELS) | [page](https://sketchfab.com/3d-models/c2d4113e6acd401cb1999f81bb318325) | CC BY 4.0 | GLB | 3,655 | 1024 | - | 5.4 m (class 4.9: scale down 10 %); use the CIVIL version, not POLICE | yes |
| R11 | 11 Coupe | BMW M4 (F82) (Mona x Supercars) | [page](https://sketchfab.com/3d-models/1d24aac71183435eb6a5ecaba5c0434d) | CC BY 4.0 | GLB | 26,768 | 1024 | -45 % | 4.7 m (fits) | yes |
| R12 | 12 Convertible | Mercedes-Benz SL 65 AMG Black Series (R230) (Mona x Supercars) | [page](https://sketchfab.com/3d-models/84c255b8a34b496bb1cb076df5ff6e70) | CC BY 4.0 | GLB | 24,025 | 1024 | -40 % | 4.6 m (fits); check that the roof is open in the preview | yes |
| R13 | 13 Minivan | VW Sharan (LEON elenaisakova248) | [page](https://sketchfab.com/3d-models/3543b8be92d44f5aa79d8fd9bd9b7835) | CC BY 4.0 | GLB | 1,892 | 512 | - | 4.85 m (fits) | yes |
| R14 | 14 Panel van | VW Transporter Multivan Green (LEON elenaisakova248) | [page](https://sketchfab.com/3d-models/e9bc406f71dd4d888e1d598c9ad05889) | CC BY 4.0 | GLB | 17,566 | 1024 | -15 % | 4.7 m (class 5.5: scale up 17 %) | yes |
| R15 | 15 Delivery van | European Delivery Van (Evan Hiltz) | [page](https://sketchfab.com/3d-models/0b2f1ad95a79419f9a092420024d329c) | CC BY 4.0 | GLB | 21,317 | 4096 | -30 % | ~5.5 m (fits); GLB is 36 MB: downscale 4096 px textures to 1024 first | yes |
| R16 | 16 Camper van (pop-top) | Free Low-poly Vintage Hippy Camper Van (Libau Media) | [page](https://sketchfab.com/3d-models/931d18f3e94b4cd580440bdc85436734) | CC BY 4.0 | GLB | 5,323 | 0 | - | ~4 m (vintage T1 style, no pop-top; class 5.5); only vintage camper found; flat colours | yes |
| R17 | 17, 18 Long panel van / Ambulance | Mercedes Sprinter Passenger (kuwalol93) | [page](https://sketchfab.com/3d-models/d3029b62317c4956bd0a28140ffef83d) | CC BY 4.0 | GLB | 11,430 | 1024 | - | 5.9 m (fits); already in the game (Van.glb); also the ambulance base | yes |
| R19 | 19 Sport bike | Suzuki GSXR 750 (Silyboy) | [page](https://sketchfab.com/3d-models/3e05cd872ef0458ca3d6b0770bb03622) | CC BY 4.0 | GLB | 27,095 | 1024 | -45 % | 2.05 m (fits); no rider | yes |
| R20 | 20 Touring bike | Akt Tt Ds 200 repainted (e-restrepo1114) | [page](https://sketchfab.com/3d-models/c32f7b87090d4ce4b10c76e844dc86cd) | CC BY 4.0 | GLB | 27,830 | 2048 | -45 % | 2.1 m (fits); adventure bike with panniers; GLB is 31 MB, downscale textures | yes |
| R21 | 21 Coachbuilt motorhome | Motorhome RV (Medical Modeller) | [page](https://sketchfab.com/3d-models/4574fbdfb8af498d8e61fd4d43018c9e) | CC BY 4.0 | GLB | 4,980 | 256 | - | ~7 m (fits) | yes |
| R22 | 22, 23 SUV with caravan / Estate with caravan | Large Caravan (rhcreations) | [page](https://sketchfab.com/3d-models/bc096d0ad5c64ac1a48647200fd559c2) | CC BY 4.0 | GLB | 10,241 | 2048 | - | ~6.5 m caravan; tow it with R05/R09 | yes |
| R24 | 24 Box truck | LCT 3000 '07- Low poly model (Daniel Zhabotinsky) | [page](https://sketchfab.com/3d-models/3be03b6a43aa41898c9ca806b8787052) | CC BY 4.0 | GLB | 15,846 | 2048 | -5 % | ~6.5 m (class 9.0: too short, scale up 38 % or use as 7.5 t truck); GLB is 11 MB | yes |
| R26 | 26 Dump truck (3-axle) | Japanese Dump Truck (Pavel Gorbachev) | [page](https://sketchfab.com/3d-models/bdd68dcc73be430dbffff61017c95fb8) | CC BY 4.0 | GLB | 18,847 | 64 | -20 % | ~8.5 m (fits) | yes |
| R27a | 27, 28, 29, 30 Box semi / Curtain semi ... | 2021 Volvo FH (mcskinssketchfab) | [page](https://sketchfab.com/3d-models/7e5f387b6660438aa0942cd17716d196) | CC BY 4.0 | GLB | 1,800 | 256 | - | ~6 m tractor unit; tractor for the semi variants (cheap, 256 px textures) | yes |
| R27b | 27, 28 Box semi / Curtain semi | Truck Trailer (cmitche1) | [page](https://sketchfab.com/3d-models/cd9a5677626f4b648a4a3b1e0d9ca5b9) | CC BY 4.0 | GLB | 7,186 | 2048 | - | ~13.6 m trailer; trailer for box semi; for curtain semi reskin with a tarp texture; GLB is 21 MB | yes |
| R28 | 28 Curtain semi | low poly DAF xf105 (Alvin.Woodly) | [page](https://sketchfab.com/3d-models/51f67c0bd11440dcbe0d1141f5d3508f) | CC BY 4.0 | GLB | 25,962 | 0 | -40 % | ~6 m tractor unit; alternative tractor (DAF XF105), no textures | yes |
| R29a | 29 Container semi | Flatbed Trailer (Arsen Ismailov) | [page](https://sketchfab.com/3d-models/76998b4d82604bb7996768835dcaa276) | CC BY 4.0 | GLB | 13,994 | 4096 | - | ~13 m flatbed / container chassis; GLB is 25 MB, downscale 4096 px | yes |
| R29b | 29 Container semi | Truck Container Pack Low-poly ASIA (knitro_vedant) | [page](https://sketchfab.com/3d-models/000b6d08ad7b4f4198dcab2790008553) | CC BY 4.0 | GLB | 5,616 | 1024 | - | containers; the containers to put on R29a | yes |
| R30 | 30 Tanker semi | Simple tank trailer (AndersonCardoso) | [page](https://sketchfab.com/3d-models/738d0e2db6584fe39d4bf9b284c7b614) | CC BY 4.0 | GLB | 4,144 | 0 | - | ~12 m tank trailer; no textures | yes |
| R32 | 32 Car transporter | Mitsubishi Fuso Car Carrier (ImperialBlue3D) | [page](https://sketchfab.com/3d-models/4b34a78f69974e62ba0b5b8f461f4360) | CC BY 4.0 | GLB | 3,820 | 1024 | - | ~8 m rigid carrier (class 19: wrong length, approximate only); not a real transporter combination | yes |
| R33 | 33 Coach | 1998 Hyundai Aero Space Coach Bus (ImperialBlue3D) | [page](https://sketchfab.com/3d-models/b51273fe6c19486eb71e95bc9c5c7032) | CC BY 4.0 | GLB | 2,250 | 512 | - | ~12 m (fits); Hyundai coach, older shape | yes |
| R33b | 33 Coach | Coach bus (own.guest) | [page](https://sketchfab.com/3d-models/89866d7d4a314f1f81b024280b1cf416) | CC BY 4.0 | GLB | 11,210 | 1024 | - | ~12 m (fits); already in the game (Bus.glb); alternative coach | yes |
| R34 | 34 Line bus (intercity) | City Bus - rigged / РоАЗ-5236 (Yo.Ri) | [page](https://sketchfab.com/3d-models/36646fcf41da498ca24a02f4b6fb1d95) | CC BY 4.0 | GLB | 5,561 | 1024 | - | ~12 m (fits); RoAZ-5236 low-floor city bus | yes |
| R35 | 35 Articulated line bus | Cee Urbano C7 Volvo B340M - Padrão Mobi Rio (CR3D Modelagens) | [page](https://sketchfab.com/3d-models/bacf3741cd134f639a77ea635ecd2d5a) | CC BY 4.0 | GLB | 11,020 | 1024 | - | ~18 m (fits) | yes |

Sketchfab shows the GLB size and texture size in its download dialog; the numbers above come from the same data. Large GLBs (R15, R20, R27b, R29a) are big because of 2048 to 4096 px textures; downscale them to 1024 px on import.

### Gaps and workarounds

- **No model at all (D or R):** Curtainsider (variant 25) and Swap-body combination (variant 31). Workarounds: curtainsider = the box truck (R24 or D24) with a tarp texture; swap-body combination = the box truck plus the generic trailer (R27b) with box bodies on both. Nothing realistic exists for the 19 m class, so the **Long combination** class is the weakest.
- **Only approximate:** Car transporter (R32 is a rigid 8 m carrier, not a 19 m combination), Curtain semi (tractor plus a reskinned box trailer), Camper van (only a vintage T1-style bus, no pop-top Transporter).
- **Direct (no-login) only stylized for these:** all classes except pickup (D10), coupe (D11) and motorhome (D21), which are low-poly with real proportions.
- **Not researched:** the 27 second-wave variants. Many are a paint job or a re-use of first-wave models.
- **Fit check:** variants where the real vehicle is more than 8 % away from the class length are marked in the Real length column (pickup, coupe, supermini, panel van, box truck, car transporter).


## Player car, 991 rear

Researched 2026-11-07. This research downloaded nothing and used no account.

**Reference.** The user's photo (`1.webp`, not in the repo) shows a 991-generation 911 from behind: two separate, curved rear lamps with LED graphics, a thin red reflector strip low on the bumper, a louvred engine-lid grille, pairs of round exhaust tips, flared hips and a fastback roof. The current 992 model has one straight full-width light bar, which the user does not want.

**Rules for a candidate.** 991 generation (2012 to 2019: Carrera, S, 4S, GTS or Turbo; no 992, 997, 996 and no GT3 wing), a free licence that allows personal use, GLB or FBX, up to about 60k triangles for LOD0, separate wheel nodes, one paint material we can recolour to the game red `#C8102E`, and rear lamps like the photo.

**How it was checked (reading only).** Candidates came from Sketchfab's public search API (downloadable models, filtered by name). The rear was checked by rotating each model in Sketchfab's own embedded viewer on a local page. The node and material lists come from the viewer API. No model file was downloaded. Third-party screenshots are not stored in the repo; each preview is the public thumbnail link.

**A no-login download does not exist for this car.** poly.pizza and OpenGameArt have no Porsche 911 at all (searched `porsche`, `911`, `carrera`, `991`). Every candidate below is on Sketchfab and needs a free account to download, so the user (or a session the user has logged in for) must download it by hand.

| # | Model (author) | Link | Licence | Variant | Format | Tris | Texture | GLB size | Login | Rear checked |
|---|---|---|---|---|---|---|---|---|---|---|
| **A** | Porsche 911 Carrera S (991) (Mona x Supercars, `Car2022`) | [page](https://sketchfab.com/3d-models/97785983d3a949cf87e9ee6bf49f4032) / [preview](https://media.sketchfab.com/models/97785983d3a949cf87e9ee6bf49f4032/thumbnails/bfc2df0720684af4b893a6049ec098af/7eaf3e2818fd4f2fa499bd702bdde974.jpeg) | CC BY 4.0 | 991.1 Carrera S (3.8 L, 400 hp per the description) | GLB | 78,990 | 1024 | 4.9 MB | yes | **yes, matches the photo** |
| **B** | (FREE) Porsche 911 Carrera 4S (Lionsharp Studios, `lionsharp`) | [page](https://sketchfab.com/3d-models/d01b254483794de3819786d93e0e1ebf) / [preview](https://media.sketchfab.com/models/d01b254483794de3819786d93e0e1ebf/thumbnails/f28bf7eb32e646019bbaf886ff705679/f0e429a46d164f78a9f4cf85ce88548b.jpeg) | CC BY-SA 4.0 | 991.1 Carrera 4S | GLB | 652,660 | 4096 | 25.4 MB | yes | **yes, matches the photo** |
| C | 2018 Porsche 911 Carrera GTS (Ddiaz Design, `ddiaz-design`) | [page](https://sketchfab.com/3d-models/5c5f8627e7d443fbbf2288429d5afad6) / [preview](https://media.sketchfab.com/models/5c5f8627e7d443fbbf2288429d5afad6/thumbnails/c90ad0236c934b46a0830a90ba1415da/737ea77029df49c4a98c4510b1e55142.jpeg) | CC BY-NC-SA 4.0 | 991.2 Carrera GTS | GLB | 71,149 | 1024 | 5.5 MB | yes | partly (991.2 style, lettering and a thin red strip seen) |
| D | 2019 Porsche 911 Carrera 4 GTS (Galaxy Car Showroom, `adrianaflak09`) | [page](https://sketchfab.com/3d-models/b3f19d82cc494a9ead7b4bbec96f3e13) / [preview](https://media.sketchfab.com/models/b3f19d82cc494a9ead7b4bbec96f3e13/thumbnails/fc2442de4f4244069ad78a5977dee0fb/48a7f18e11c24ac4b6c9c41aa1fddb65.jpeg) | CC BY 4.0 | 991.2 Carrera 4 GTS | GLB | 79,879 | 4096 | 12.6 MB | yes | no |
| E | 2015 Porsche 911 Carrera S, Mid-Poly (chiwei, `chiwei2333`; copy of a rigged original) | [page](https://sketchfab.com/3d-models/4cef7c1e22ab40fd937507eae9a62cd8) / [preview](https://media.sketchfab.com/models/4cef7c1e22ab40fd937507eae9a62cd8/thumbnails/5c473b3babd64271a10c0d588a0ec030/c44dbb0b539a4607ad089c7723759b55.jpeg) | CC BY 4.0 | 991.1 Carrera S | GLB | 242,410 | 4096 | 174.1 MB | yes | no |

**First choice: A.** **Fallback: B.** (If both fail, C.)

**Why A.**
- In the viewer its rear is the photo's rear: two separate curved red lamps, a louvred lid grille with the `PORSCHE 911 Carrera S` lettering, round twin exhaust tips, and a small red reflector under the bumper. The front shows the round 991 headlights and three-slot intakes.
- 79k triangles is the closest to the budget (reduce about 24 % to 60k), the GLB is only 4.9 MB, and textures are 1024 px.
- CC BY 4.0: credit line only, no share-alike.
- Paint: one material holds the yellow paint (`11Mtl`, base colour about `#FFB500`), so recolouring to red is one material change. The others: `T1Mtl` (tyre), `R1Mtl`, `R5Mtl`, `Ri1Mtl` (rims), `Window1Mtl` (glass), and several dark red ones for calipers, tail lamps and reflector.
- **Downloaded 2026-11-07** (the user said yes) to `ThirdPartySources\player\porsche-991-carrera-s\porsche-991-carrera-s.glb`, 4,929,080 bytes. Details are in `ThirdPartySources\SOURCES.md`. Read from the file: exactly 78,990 triangles, 12 PNG textures (max 1024 px), 29 nodes, 27 meshes, 23 materials.
- **Weak point, now confirmed:** the model is a converted OBJ. The 27 mesh nodes are named `Object_2` to `Object_28`. The paint is one mesh (`Object_7`, material `11Mtl`, 9,719 triangles): easy to recolour. **The wheels are not separate nodes**: all four tyres are in one mesh (`T1Mtl`, 2,400 triangles) and the rims in two (`Ri1Mtl`, 6,759 + 2,253 triangles), plus `R1Mtl`, `R5Mtl`, `Sw1Mtl`, `Ca1Mtl` for rim details, discs and calipers. To make the wheels spin, the developer must split those meshes by vertex position into four wheel groups (wheel centres are listed in `SOURCES.md`). This is a one-time import script, not a reason to switch to fallback B.

**Why B is the fallback.**
- Best fidelity of the five, and its rear also matches the photo: slim red lamps joined by a thin red strip across the lid base, a louvred grille, twin exhaust pairs, a plate. It was built for a WebGL configurator, so it has 112 **named** nodes (`boot.*`, `bumper_front.*`, `windshield`, `window_rear`, `underbody`, `Cylinder.*` for the wheel parts) and 14 materials with one `paint` material (plus `rubber`, `silver`, `lights`, `glass`, `window`, `plastic`, `license`, `logo`).
- Costs: 652,660 triangles (about 91 % reduction, which is real decimation work), 4096 px textures, a 25 MB GLB, and **CC BY-SA 4.0** (a modified copy must carry the same licence; fine for a personal project whose models stay out of git).

**Notes on C, D, E.**
- C is a 991.2, whose rear is not the 991.1 rear of the photo; it is NC-SA (fine for personal use). Sketchfab user `tangwang920` re-uploads ddiaz-design's models under CC BY. Do not use those copies: the original licence is NC-SA.
- D and E were not checked from behind. E is 174 MB and 242k triangles, so it is only worth it if A and B fail.
- Not checked and not chosen: ddiaz-design's `2014 Porsche 911 Turbo (991)` (NC-SA, 85.6k) and `2017 Porsche 911 Turbo S Exclusive Series` (NC-SA, 71.5k). 992, 996, 997 and GT3 wing models were ignored as asked.

**Preparing the model (after the user has downloaded it).**
- Recolour the paint material to `#C8102E`, keep the clear coat if it has one, leave the lamp and caliper materials alone.
- Remove or blank the licence plate texture. Keep the `PORSCHE` rear lettering only if you accept the brand (personal use, see `decisions.md`).
- Check the rear in the game camera at 6.5 m: the lamps must stay two separate shapes joined only low on the bumper, never a full-width bar.
- Add the credit line to `CREDITS.md` (see `vehicle-fetch.md`).


### Sketchfab models downloaded 2026-11-07

All R items were downloaded by hand with the user's approval (free models, GLB, 1k textures where a choice existed). Files are in `ThirdPartySources\<class>\<name>\` (outside git). Author, licence, URL, size and SHA-256 are in `SOURCES.md` there. Triangle counts are read from the files.

Over the 15k traffic budget (needs LOD or a cheaper stand-in): R05, R11, R12, R14, R15, R19, R20, R24, R26, R28. R29a is close (14k).

| Id | File | MB | Triangles |
|---|---|---|---|
| R03 | `hatchback/golf.glb` | 0.3 | 4,376 |
| R17 | `longvan/long-panel.glb` | 1.9 | 11,430 |
| R33b | `bus/coach-alt.glb` | 1.4 | 11,210 |
| R01 | `compact/city-car.glb` | 1.0 | 14,287 |
| R02 | `hatchback/supermini.glb` | 0.7 | 9,269 |
| R04 | `sedan/sedan.glb` | 1.3 | 13,441 |
| R05 | `sedan/estate.glb` | 4.6 | 27,083 |
| R08 | `suv/mid-suv.glb` | 0.6 | 12,516 |
| R09 | `suv/large-suv.glb` | 0.7 | 6,742 |
| R10 | `suv/pickup.glb` | 1.4 | 3,655 |
| R11 | `sports/coupe.glb` | 2.3 | 26,768 |
| R12 | `sports/convertible.glb` | 2.8 | 24,025 |
| R13 | `mpv/minivan.glb` | 0.4 | 1,892 |
| R14 | `van/panel.glb` | 1.8 | 17,566 |
| R15 | `van/delivery.glb` | 9.4 | 21,317 |
| R16 | `van/camper.glb` | 0.4 | 5,323 |
| R19 | `motorbike/sport.glb` | 2.8 | 27,095 |
| R20 | `motorbike/touring.glb` | 10.3 | 27,830 |
| R21 | `motorhome/coachbuilt.glb` | 0.4 | 4,980 |
| R22 | `caravan/caravan.glb` | 2.0 | 10,241 |
| R24 | `rigid/box.glb` | 7.4 | 15,846 |
| R26 | `rigid/dump.glb` | 1.3 | 18,847 |
| R27a | `semi/tractor-volvo-fh.glb` | 0.3 | 1,800 |
| R27b | `semi/trailer-box.glb` | 6.4 | 7,186 |
| R28 | `semi/tractor-daf-xf105.glb` | 1.9 | 25,962 |
| R29a | `semi/trailer-flatbed.glb` | 3.7 | 13,994 |
| R29b | `semi/containers.glb` | 2.9 | 5,616 |
| R30 | `semi/trailer-tank.glb` | 0.2 | 4,144 |
| R32 | `longcombo/car-transporter.glb` | 0.8 | 3,820 |
| R33 | `bus/coach.glb` | 0.4 | 2,250 |
| R34 | `bus/line-bus.glb` | 3.9 | 5,561 |
| R35 | `articulated/artic-line.glb` | 1.0 | 11,020 |
