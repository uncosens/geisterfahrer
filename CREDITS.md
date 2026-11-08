# Credits and third-party assets

None of these files are in git. `Assets/ThirdParty/` is git-ignored; get them as described below.

## Downloaded by `tools/fetch-assets.sh` (free, direct links)

| Asset | Source | Licence |
|---|---|---|
| HDRI sky "Dreifaltigkeitsberg" 2K | https://polyhaven.com/a/dreifaltigkeitsberg | CC0 |
| Texture "Asphalt 01" 1K (diffuse, normal GL), fetched but no longer shown (the road is generated) | https://polyhaven.com/a/asphalt_01 | CC0 |
| Texture "Grass004" 1K (colour) | https://ambientcg.com/view?id=Grass004 | CC0 |
| Font Inter 4.1 (Bold, SemiBold, Black) by Rasmus Andersson | https://github.com/rsms/inter | SIL OFL 1.1 |
| Font Barlow Semi Condensed SemiBold by Jeremy Tribby | https://fonts.google.com/specimen/Barlow+Semi+Condensed | SIL OFL 1.1 |

Textures and HDRIs from Poly Haven (polyhaven.com) and ambientCG (ambientcg.com), CC0.

## Vehicle models (download by hand)

Sketchfab needs a free account to download, so these are not fetched automatically.
On each page: Download 3D Model → **glTF Binary (.glb)**. Save the file under the given name in `Assets/ThirdParty/Vehicles/`, then run
`BuildScript.Setup` (menu Geisterfahrer → Setup Scene and Assets). A variant whose files are missing uses a primitive placeholder.
Which files make which variant: `Assets/Scripts/Game/VehicleRecipes.cs`.

| File | Model | Used for | Attribution (CC BY 4.0) |
|---|---|---|---|
| `Player.glb` | https://sketchfab.com/3d-models/97785983d3a949cf87e9ee6bf49f4032 | the player's car | "Porsche 911 Carrera S (991)" by Mona x Supercars (https://sketchfab.com/Car2022), licensed under CC BY 4.0 (https://creativecommons.org/licenses/by/4.0/), modified (recoloured, scaled, wheels split into separate parts). |
| `hatchback/golf.glb` | https://sketchfab.com/3d-models/ec7b2f2fd567419a965db5b07b777c0c | City car, Golf-class hatchback | "1998 Volkswagen Golf Mk4" by ImperialBlue3D (https://sketchfab.com/ImperialBlue3D), licensed under CC BY 4.0 (https://creativecommons.org/licenses/by/4.0/), modified (scaled, body paint recoloured; interior removed; heavy meshes thinned) |
| `sedan/sedan.glb` | https://sketchfab.com/3d-models/28f0bc2f0083417a8e521150529034d6 | Sedan, Taxi, Police car | "BMW 320i E90 Low Polly" by rifdanzz (https://sketchfab.com/rifdanzz), licensed under CC BY 4.0 (https://creativecommons.org/licenses/by/4.0/), modified (scaled, body paint recoloured; interior removed; heavy meshes thinned) |
| `sedan/estate.glb` | https://sketchfab.com/3d-models/2f6aea1435bd4ad093146c019c8e0fb6 | Estate | "Audi A4 B5.5 Avant" by Tomaso (https://sketchfab.com/Tomasoo), licensed under CC BY 4.0 (https://creativecommons.org/licenses/by/4.0/), modified (scaled, body paint recoloured; interior removed; heavy meshes thinned) |
| `suv/mid-suv.glb` | https://sketchfab.com/3d-models/1a2a52b16cad4e618af347461817895c | Mid SUV, SUV with caravan | "Mercedes Benz G-class W263" by Lexyc16 (https://sketchfab.com/Lexyc16), licensed under CC BY 4.0 (https://creativecommons.org/licenses/by/4.0/), modified (scaled, body paint recoloured; interior removed; heavy meshes thinned) |
| `suv/large-suv.glb` | https://sketchfab.com/3d-models/d515af89f6c6414b93c537ee1fcfc800 | Large SUV | "BMW X5 E70" by LEON elenaisakova248 (https://sketchfab.com/elenaisakova248), licensed under CC BY 4.0 (https://creativecommons.org/licenses/by/4.0/), modified (scaled, body paint recoloured; interior removed; heavy meshes thinned) |
| `sports/coupe.glb` | https://sketchfab.com/3d-models/1d24aac71183435eb6a5ecaba5c0434d | Coupe | "BMW M4 (F82)" by Mona x Supercars (https://sketchfab.com/Car2022), licensed under CC BY 4.0 (https://creativecommons.org/licenses/by/4.0/), modified (scaled, body paint recoloured; interior removed; heavy meshes thinned) |
| `sports/convertible.glb` | https://sketchfab.com/3d-models/84c255b8a34b496bb1cb076df5ff6e70 | Convertible | "Mercedes-Benz SL 65 AMG Black Series (R230)" by GT Cars: Hyperspeed, shown on the model page as Mona x Supercars (https://sketchfab.com/Car2022), licensed under CC BY 4.0 (https://creativecommons.org/licenses/by/4.0/), modified (scaled, body paint recoloured; interior removed; heavy meshes thinned) |
| `mpv/minivan.glb` | https://sketchfab.com/3d-models/3543b8be92d44f5aa79d8fd9bd9b7835 | Minivan, Minivan with caravan | "VW Sharan" by LEON elenaisakova248 (https://sketchfab.com/elenaisakova248), licensed under CC BY 4.0 (https://creativecommons.org/licenses/by/4.0/), modified (scaled, body paint recoloured; interior removed; heavy meshes thinned) |
| `van/panel.glb` | https://sketchfab.com/3d-models/e9bc406f71dd4d888e1d598c9ad05889 | Panel van | "VW Transporter Multivan Green" by LEON elenaisakova248 (https://sketchfab.com/elenaisakova248), licensed under CC BY 4.0 (https://creativecommons.org/licenses/by/4.0/), modified (scaled, body paint recoloured; interior removed; heavy meshes thinned) |
| `van/delivery.glb` | https://sketchfab.com/3d-models/0b2f1ad95a79419f9a092420024d329c | Delivery van | "European Delivery Van" by Evan Hiltz (https://sketchfab.com/evan.hiltz), licensed under CC BY 4.0 (https://creativecommons.org/licenses/by/4.0/), modified (scaled, body paint recoloured; interior removed; heavy meshes thinned) |
| `van/camper.glb` | https://sketchfab.com/3d-models/931d18f3e94b4cd580440bdc85436734 | Camper van | "Free Low-poly Vintage Hippy Camper Van" by Libau Media (https://sketchfab.com/robinmikart), licensed under CC BY 4.0 (https://creativecommons.org/licenses/by/4.0/), modified (scaled, body paint recoloured; interior removed; heavy meshes thinned) |
| `longvan/long-panel.glb` | https://sketchfab.com/3d-models/d3029b62317c4956bd0a28140ffef83d | Long panel van, Ambulance | "Mercedes Sprinter Passenger" by kuwalol93 (https://sketchfab.com/kuwalol93), licensed under CC BY 4.0 (https://creativecommons.org/licenses/by/4.0/), modified (scaled, body paint recoloured; interior removed; heavy meshes thinned) |
| `motorbike/sport.glb` | https://sketchfab.com/3d-models/3e05cd872ef0458ca3d6b0770bb03622 | Sport bike | "Suzuki GSXR 750" by Silyboy (https://sketchfab.com/kaleemullah550), licensed under CC BY 4.0 (https://creativecommons.org/licenses/by/4.0/), modified (scaled, body paint recoloured; interior removed; heavy meshes thinned) |
| `motorbike/touring.glb` | https://sketchfab.com/3d-models/c32f7b87090d4ce4b10c76e844dc86cd | Touring bike | "Akt Tt Ds 200 repainted" by e-restrepo1114 (https://sketchfab.com/EmanuelRestrepoVelez), licensed under CC BY 4.0 (https://creativecommons.org/licenses/by/4.0/), modified (scaled, body paint recoloured; interior removed; heavy meshes thinned) |
| `motorhome/coachbuilt.glb` | https://sketchfab.com/3d-models/4574fbdfb8af498d8e61fd4d43018c9e | Coachbuilt motorhome | "Motorhome RV" by BillyJJackson, shown on the model page as Medical Modeller (https://sketchfab.com/pistonstone), licensed under CC BY 4.0 (https://creativecommons.org/licenses/by/4.0/), modified (scaled, body paint recoloured; interior removed; heavy meshes thinned) |
| `caravan/caravan.glb` | https://sketchfab.com/3d-models/bc096d0ad5c64ac1a48647200fd559c2 | the caravan of the three car-with-caravan variants | "Large Caravan" by rhcreations (https://sketchfab.com/rhcreations), licensed under CC BY 4.0 (https://creativecommons.org/licenses/by/4.0/), modified (scaled, body paint recoloured; interior removed; heavy meshes thinned) |
| `rigid/box.glb` | https://sketchfab.com/3d-models/3be03b6a43aa41898c9ca806b8787052 | Box truck | "LCT 3000 '07- Low poly model" by Daniel Zhabotinsky (https://sketchfab.com/DanielZhabotinsky), licensed under CC BY 4.0 (https://creativecommons.org/licenses/by/4.0/), modified (scaled, body paint recoloured; interior removed; heavy meshes thinned) |
| `semi/tractor-volvo-fh.glb` | https://sketchfab.com/3d-models/7e5f387b6660438aa0942cd17716d196 | tractor of the semis | "2021 Volvo FH" by mcskinssketchfab (https://sketchfab.com/mcvehiclessketchfab), licensed under CC BY 4.0 (https://creativecommons.org/licenses/by/4.0/), modified (scaled, body paint recoloured; interior removed; heavy meshes thinned) |
| `semi/trailer-box.glb` | https://sketchfab.com/3d-models/cd9a5677626f4b648a4a3b1e0d9ca5b9 | Box semi | "Truck Trailer" by cmitche1 (https://sketchfab.com/cmitche1), licensed under CC BY 4.0 (https://creativecommons.org/licenses/by/4.0/), modified (scaled, body paint recoloured; interior removed; heavy meshes thinned) |
| `semi/trailer-tank.glb` | https://sketchfab.com/3d-models/738d0e2db6584fe39d4bf9b284c7b614 | Tanker semi | "Simple tank trailer" by AndersonCardoso (https://sketchfab.com/AndersonCardoso), licensed under CC BY 4.0 (https://creativecommons.org/licenses/by/4.0/), modified (scaled, body paint recoloured; interior removed; heavy meshes thinned) |
| `semi/trailer-flatbed.glb` | https://sketchfab.com/3d-models/76998b4d82604bb7996768835dcaa276 | Flatbed semi | "Flatbed Trailer" by Arsen Ismailov (https://sketchfab.com/fdgasd7), licensed under CC BY 4.0 (https://creativecommons.org/licenses/by/4.0/), modified (scaled, body paint recoloured; interior removed; heavy meshes thinned) |
| `longcombo/car-transporter.glb` | https://sketchfab.com/3d-models/4b34a78f69974e62ba0b5b8f461f4360 | Car transporter | "Mitsubishi Fuso Car Carrier" by 3DCars, shown on the model page as ImperialBlue3D (https://sketchfab.com/3DCarss), licensed under CC BY 4.0 (https://creativecommons.org/licenses/by/4.0/), modified (scaled, body paint recoloured; interior removed; heavy meshes thinned) |
| `bus/coach.glb` | https://sketchfab.com/3d-models/b51273fe6c19486eb71e95bc9c5c7032 | Coach | "1998 Hyundai Aero Space Coach Bus" by ImperialBlue (https://sketchfab.com/efemetin), shown on the model page as ImperialBlue3D, licensed under CC BY 4.0 (https://creativecommons.org/licenses/by/4.0/), modified (scaled, body paint recoloured; interior removed; heavy meshes thinned) |
| `bus/coach-alt.glb` | https://sketchfab.com/3d-models/89866d7d4a314f1f81b024280b1cf416 | Coach B | "Coach bus" by own.guest (https://sketchfab.com/own.guest), licensed under CC BY 4.0 (https://creativecommons.org/licenses/by/4.0/), modified (scaled, body paint recoloured; interior removed; heavy meshes thinned) |
| `articulated/artic-line.glb` | https://sketchfab.com/3d-models/bacf3741cd134f639a77ea635ecd2d5a | Articulated line bus | "Cee Urbano C7 Volvo B340M - Padrão Mobi Rio" by CR3D Modelagens (https://sketchfab.com/cr3dmods), licensed under CC BY 4.0 (https://creativecommons.org/licenses/by/4.0/), modified (scaled, body paint recoloured; interior removed; heavy meshes thinned) |

### Licence check for the release APK (2026-11-08, v0.2.0)

Every vehicle model in the APK is **CC BY 4.0**, which allows redistribution with attribution. The licence, author and title were read from each GLB's own metadata (`asset.extras`) and match the table above; where the author name in the file differs from the one on the model page (convertible, motorhome, car transporter, coach), both are named. Changes in all: scaled; body paint recoloured where the model has a paintable body; interiors and shadow quads removed; models over the triangle budget thinned by vertex clustering (estate, coupe, convertible, panel van, delivery van, sport and touring bike, box truck, flatbed trailer); combinations (tractor + trailer, car + caravan) assembled from two files; the player's wheels split into separate parts.

Downloaded but not in the game or the APK (dropped after the contact-sheet check): Mini Cooper S rally car (red body with a number decal baked into its texture), Low Poly Polo (flat single-colour palette, cannot be painted per part), Ford Ranger Raptor pickup (red baked paint), Japanese dump truck (smeared cab front), City bus RoAZ-5236 (hollow body without its interior), DAF XF105 (over budget, Volvo FH serves all semis), Truck Container Pack (four loose containers, no trailer).
Vehicle names and designs belong to their respective owners; this project is non-commercial and not affiliated with them.

## In git

- `Assets/TextMesh Pro/` — TextMesh Pro essential resources from Unity's `com.unity.ugui` package (Unity Companion License).
- `Assets/UI/Resources/Icons/` — our own icons (`docs/design/ui/icons/`), rendered to PNG.
- Sounds are generated in code (no sound files yet).
