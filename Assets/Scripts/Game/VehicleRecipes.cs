using System.Linq;
using Geisterfahrer.Core;
using UnityEngine;

namespace Geisterfahrer.Game
{
    // Which model files (GLB under Assets/ThirdParty/Vehicles/) make each variant of Road.Variants, and how they are painted.
    // Several parts = a combination laid out nose to tail (tractor + trailer, car + caravan); the lengths add up to the class length.
    public class Part
    {
        public string File;      // "folder/name", no extension
        public float Length;     // 0 = the whole class length
        public bool Flip;        // model built facing -z (checked on the contact sheet)
        public bool Tint;        // body paint is changed; false = the model keeps its own livery
        public string PaintMat;  // lower-case name of the one material that is the body paint (when the model has no "paint" name)
        public string Drop;      // extra material-name pieces to remove (comma separated), on top of the global hidden list
        public int Reduce;       // triangle target for this part (0 = keep)

        public Part(string file, float length = 0, bool tint = true, string paintMat = null, bool flip = false, string drop = null, int reduce = 0)
        { File = file; Length = length; Tint = tint; PaintMat = paintMat; Flip = flip; Drop = drop; Reduce = reduce; }
    }

    public class Recipe
    {
        public string Variant;
        public int FixedColor;   // police, ambulance, taxi: one colour (0 = random traffic paint)
        public Part[] Parts;
    }

    public static class VehicleRecipes
    {
        // Interiors and shadow quads are never seen (glass is opaque dark, the road has its own blob shadow).
        public static readonly string[] Hidden = { "interior", "sidenie", "kreslo", "guantes", "shadow", "seat", "steering", "dashboard" };

        static Recipe R(string variant, params Part[] parts) => new Recipe { Variant = variant, Parts = parts };
        static Recipe C(string variant, int color, params Part[] parts) => new Recipe { Variant = variant, FixedColor = color, Parts = parts };

        public static readonly Recipe[] All =
        {
            R("City car", new Part("hatchback/golf")),
            R("Golf-class hatchback", new Part("hatchback/golf")),
            R("Sedan", new Part("sedan/sedan", paintMat: "material_0")),
            R("Estate", new Part("sedan/estate", tint: false, reduce: 14000)),
            C("Taxi", 0xF0E6C8, new Part("sedan/sedan", paintMat: "material_0")),
            C("Police car", 0x2C4F8C, new Part("sedan/sedan", paintMat: "material_0")),
            R("Mid SUV", new Part("suv/mid-suv", paintMat: "material.001")),
            R("Large SUV", new Part("suv/large-suv")),
            R("Coupe", new Part("sports/coupe", reduce: 14000)),
            R("Convertible", new Part("sports/convertible", reduce: 14000)),
            R("Minivan", new Part("mpv/minivan")),
            R("Panel van", new Part("van/panel", reduce: 14000)),
            R("Delivery van", new Part("van/delivery", drop: "underc", reduce: 17500)),
            R("Camper van", new Part("van/camper", paintMat: "body")),
            R("Long panel van", new Part("longvan/long-panel")),
            C("Ambulance", 0xF2F2EE, new Part("longvan/long-panel")),
            R("Sport bike", new Part("motorbike/sport", reduce: 14000)),
            R("Touring bike", new Part("motorbike/touring", reduce: 14000)),
            R("Coachbuilt motorhome", new Part("motorhome/coachbuilt")),
            R("SUV with caravan", new Part("suv/mid-suv", 4.9f), new Part("caravan/caravan", 6.1f, paintMat: "main_paint_material")),
            R("Estate with caravan", new Part("sedan/estate", 4.8f, reduce: 9000), new Part("caravan/caravan", 6.2f, paintMat: "main_paint_material")),
            R("Minivan with caravan", new Part("mpv/minivan", 4.7f), new Part("caravan/caravan", 6.3f, paintMat: "main_paint_material")),
            R("Box truck", new Part("rigid/box", tint: false, reduce: 14000)),
            R("Box semi", new Part("semi/tractor-volvo-fh", 6.2f), new Part("semi/trailer-box", 10.3f, tint: false)),
            R("Tanker semi", new Part("semi/tractor-volvo-fh", 6.2f), new Part("semi/trailer-tank", 10.3f, tint: false)),
            R("Flatbed semi", new Part("semi/tractor-volvo-fh", 6.2f), new Part("semi/trailer-flatbed", 10.3f, tint: false, reduce: 14000)),
            R("Car transporter", new Part("longcombo/car-transporter", tint: false)),
            R("Coach", new Part("bus/coach", tint: false)),
            R("Coach B", new Part("bus/coach-alt", tint: false)),
            R("Articulated line bus", new Part("articulated/artic-line", tint: false)),
        };

        public static Recipe For(int variant) => All.First(r => r.Variant == Road.Variants[variant].Name);

        // A painted vehicle for one variant, or a primitive stand-in when the model files are missing.
        public static GameObject Make(Art art, int variant, ref int lastPaint)
        {
            var vr = Road.Variants[variant];
            var t = Road.Types[vr.Class];
            var rec = For(variant);
            var models = variant < art.vehicles.Length ? art.vehicles[variant].list : null;
            GameObject go;
            if (models != null && models.Length == rec.Parts.Length && models.All(m => m))
                go = Placeholder.FitParts(vr.Name, (float)t.Length, models, rec.Parts);
            else
                go = Placeholder.PaintableVehicle(vr.Name, (float)t.Length, (float)t.Width, (float)t.Height, t.Name == "Truck" ? Placeholder.Trailer : null);
            go.GetComponent<Paintable>().Paint(rec.FixedColor != 0 ? new Placeholder.PaintColor(rec.FixedColor, 0.3f, 0.8f) : Placeholder.RandomPaint(ref lastPaint));
            return go;
        }
    }
}
