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
