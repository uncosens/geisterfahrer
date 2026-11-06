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
On each page: Download 3D Model → **glTF Binary (.glb)**. Save the file under the given name, then run
`BuildScript.Setup` (menu Geisterfahrer → Setup Scene and Assets). Until a file is there, the game uses a placeholder.

| Save as `Assets/ThirdParty/Vehicles/…` | Model | Attribution (CC BY 4.0) |
|---|---|---|
| `Player.glb` | https://sketchfab.com/3d-models/porsche-911-s-992-low-poly-16881132aa764a18ad465e2a1d27851b | "Porsche 911 S 992 Low Poly" by Pavel6678 (https://sketchfab.com/Pavel6678), licensed under CC BY 4.0 (https://creativecommons.org/licenses/by/4.0/), modified (scaled) |
| `Compact.glb` | https://sketchfab.com/3d-models/1998-volkswagen-golf-mk4-ec7b2f2fd567419a965db5b07b777c0c | "1998 Volkswagen Golf Mk4" by ImperialBlue3D (https://sketchfab.com/ImperialBlue3D), licensed under CC BY 4.0, modified (scaled) |
| `Sedan.glb` | https://sketchfab.com/3d-models/low-poly-bmw-m5-f90-87102e4f8f2b42c087ff1a4423b5cccd | "Low Poly BMW M5 F90" by kulonee (https://sketchfab.com/ynesolefru), licensed under CC BY 4.0, modified (scaled) |
| `Van.glb` | https://sketchfab.com/3d-models/mercedes-sprinter-passenger-d3029b62317c4956bd0a28140ffef83d | "Mercedes Sprinter Passenger" by kuwalol93 (https://sketchfab.com/kuwalol93), licensed under CC BY 4.0, modified (scaled) |
| `Bus.glb` | https://sketchfab.com/3d-models/coach-bus-89866d7d4a314f1f81b024280b1cf416 | "Coach bus" by own.guest (https://sketchfab.com/own.guest), licensed under CC BY 4.0, modified (scaled) |
| `Truck.glb` | https://sketchfab.com/3d-models/mercedes-actros-com-semi-reboque-prancha-carrega-8ee4c1329219413e879760a25208ee15 | "Mercedes Actros com Semi Reboque Prancha Carrega" by LuizZed36 (https://sketchfab.com/LuizZed36), licensed under CC BY 4.0, modified (scaled) |

Fallback picks and more detail: `docs/design/vehicle-assets.md`.
Vehicle names and designs belong to their respective owners; this project is non-commercial and not affiliated with them.

## In git

- `Assets/TextMesh Pro/` — TextMesh Pro essential resources from Unity's `com.unity.ugui` package (Unity Companion License).
- `Assets/UI/Resources/Icons/` — our own icons (`docs/design/ui/icons/`), rendered to PNG.
- Sounds are generated in code (no sound files yet).
