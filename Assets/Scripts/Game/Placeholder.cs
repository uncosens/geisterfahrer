using System.Collections.Generic;
using UnityEngine;

namespace Geisterfahrer.Game
{
    // Primitive stand-in models, shared materials (look-spec 8) and fitting of downloaded models.
    public static class Placeholder
    {
        static readonly Dictionary<(Color, float, float), Material> mats = new Dictionary<(Color, float, float), Material>();
        // A URP Lit material asset referenced by the scene, so the shader and its fog variants ship in the build.
        public static Material BaseMat;

        public static Color Hex(int rgb) => new Color((rgb >> 16 & 255) / 255f, (rgb >> 8 & 255) / 255f, (rgb & 255) / 255f);

        public static Material Mat(Color c, float metallic = 0, float smoothness = 0.5f)
        {
            if (mats.TryGetValue((c, metallic, smoothness), out var m)) return m;
            m = new Material(BaseMat) { color = c };
            m.SetFloat("_Metallic", metallic);
            m.SetFloat("_Smoothness", smoothness);
            mats[(c, metallic, smoothness)] = m;
            return m;
        }

        // Calm generated asphalt (look-spec 6.1): no fine grain or short repeat, which strained the eyes.
        // The road mesh has world UVs of metres / uvTile with u = 0 on the road centre; the texture spans
        // the road width once and repeats every 60 m along it.
        public static Material CalmAsphalt(float width, float uvTile)
        {
            const int W = 128, H = 256;
            const float Repeat = 60;
            var tex = new Texture2D(W, H, TextureFormat.RGBA32, true) { filterMode = FilterMode.Trilinear, anisoLevel = 8, wrapMode = TextureWrapMode.Repeat };
            var px = new Color[W * H];
            for (int y = 0; y < H; y++)
                for (int x = 0; x < W; x++)
                {
                    float m = (x + 0.5f) / W * width - width / 2, v = 1;
                    // Darker tyre tracks, 0.8 m each side of every lane centre.
                    for (int k = 0; k < Core.Road.LaneCount; k++)
                        for (int side = -1; side <= 1; side += 2)
                        {
                            float d = (m - (float)Core.Road.LaneX(k) - side * 0.8f) / 0.35f;
                            v -= 0.07f * Mathf.Exp(-d * d);
                        }
                    // Slow light/dark waves along the road, periodic so the repeat has no seam.
                    float a = 2 * Mathf.PI * y / H;
                    v += 0.02f * Mathf.Sin(3 * a + m * 0.4f) + 0.015f * Mathf.Sin(5 * a + 1.3f - m * 0.25f);
                    px[y * W + x] = new Color(v, v, v);
                }
            tex.SetPixels(px);
            tex.Apply();
            var mat = new Material(BaseMat) { color = Hex(0x55575C), mainTexture = tex };
            mat.SetFloat("_Metallic", 0);
            mat.SetFloat("_Smoothness", 0.25f);
            mat.mainTextureScale = new Vector2(uvTile / width, uvTile / Repeat);
            mat.mainTextureOffset = new Vector2(0.5f, 0);
            return mat;
        }

        static Material GlassMat => Mat(Hex(0x1A1F26), 0, 0.95f);
        static Material Tyre => Mat(Hex(0x1B1B1B), 0, 0.25f);
        static Material HeadLight => Mat(Hex(0xFFF6E0), 0, 0.9f);
        public static Material PlayerPaint => Mat(Hex(0xB5121B), 0.7f, 0.82f); // red 911 (user choice); darker than the danger red #FF4D4F
        public static Material Trailer => Mat(Hex(0xEDEDEB), 0, 0.4f);

        // Traffic paint: white 30, silver 25, graphite 15, black 15, beige 10, yellow 5 (vans only).
        public static Material TrafficPaint(bool van)
        {
            float r = Random.value * (van ? 100 : 95);
            return r < 30 ? Mat(Hex(0xE9E9E7), 0.1f, 0.75f)
                : r < 55 ? Mat(Hex(0xB9BDC2), 0.8f, 0.7f)
                : r < 70 ? Mat(Hex(0x3A3D42), 0.6f, 0.78f)
                : r < 85 ? Mat(Hex(0x1C1D20), 0.4f, 0.85f)
                : r < 95 ? Mat(Hex(0xC8B79A), 0.2f, 0.7f)
                : Mat(Hex(0xE3B53A), 0.1f, 0.7f);
        }

        public static GameObject Box(Transform parent, Vector3 pos, Vector3 size, Color c, PrimitiveType type = PrimitiveType.Cube)
            => Box(parent, pos, size, Mat(c), type);

        public static GameObject Box(Transform parent, Vector3 pos, Vector3 size, Material m, PrimitiveType type = PrimitiveType.Cube)
        {
            var go = GameObject.CreatePrimitive(type);
            Object.Destroy(go.GetComponent<Collider>());
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localScale = size;
            go.GetComponent<Renderer>().sharedMaterial = m;
            return go;
        }

        // Root at the footprint center on the ground, +Z = front. The model sits in a child named "Model".
        public static GameObject Vehicle(string name, float l, float w, float h, Material body, Material rear = null)
        {
            var root = new GameObject(name).transform;
            var model = new GameObject("Model").transform;
            model.SetParent(root, false);
            bool big = h > 3;
            float wheel = big ? 0.5f : 0.33f;
            float bodyH = big ? h - wheel : h * 0.5f;
            if (rear != null) // semi: short cab in front, trailer behind
            {
                Box(model, new Vector3(0, wheel + bodyH / 2, l / 2 - 1.2f), new Vector3(w, bodyH * 0.8f, 2.4f), body);
                Box(model, new Vector3(0, wheel + bodyH / 2 + 0.1f, -1.4f), new Vector3(w, bodyH, l - 2.8f), rear);
                Box(model, new Vector3(0, wheel + bodyH * 0.55f, l / 2 + 0.01f), new Vector3(w * 0.9f, bodyH * 0.3f, 0.02f), GlassMat);
            }
            else
            {
                Box(model, new Vector3(0, wheel + bodyH / 2, 0), new Vector3(w, bodyH, l), body);
                if (!big)
                    Box(model, new Vector3(0, wheel + bodyH + h * 0.15f - 0.05f, -l * 0.05f), new Vector3(w * 0.85f, h * 0.3f, l * 0.5f), GlassMat);
                else // bus window band
                    Box(model, new Vector3(0, h * 0.65f, 0), new Vector3(w + 0.02f, h * 0.25f, l - 0.6f), GlassMat);
            }
            foreach (float x in new[] { -1f, 1f })
            {
                foreach (float z in new[] { -0.33f, 0.33f })
                {
                    var wh = Box(model, new Vector3(x * (w / 2 - 0.05f), wheel, z * l), new Vector3(wheel * 2, 0.12f, wheel * 2), Tyre, PrimitiveType.Cylinder);
                    wh.transform.localRotation = Quaternion.Euler(0, 0, 90);
                }
                Box(model, new Vector3(x * (w / 2 - 0.3f), wheel + bodyH * 0.35f, l / 2 + 0.01f), new Vector3(0.3f, 0.12f, 0.04f), HeadLight);
            }
            return root.gameObject;
        }

        // Wraps a downloaded model so it follows the prefab rules: long axis on z, scaled to the real length,
        // footprint centred on the root, wheels on y = 0.
        // ponytail: front vs back can't be told from bounds; if a model drives backwards, rotate it 180 in its file.
        public static GameObject Fit(GameObject prefab, float length, string name)
        {
            var root = new GameObject(name).transform;
            var model = Object.Instantiate(prefab, root).transform;
            model.name = "Model";
            var b = Bounds(model);
            if (b.size.x > b.size.z) { model.localRotation = Quaternion.Euler(0, 90, 0) * model.localRotation; b = Bounds(model); }
            model.localScale *= length / b.size.z;
            b = Bounds(model);
            model.localPosition -= new Vector3(b.center.x, b.min.y, b.center.z);
            return root.gameObject;
        }

        static Bounds Bounds(Transform t)
        {
            var rs = t.GetComponentsInChildren<Renderer>();
            var b = rs[0].bounds;
            foreach (var r in rs) b.Encapsulate(r.bounds);
            return b;
        }
    }
}
