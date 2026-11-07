using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Geisterfahrer.Game
{
    // Per-car presentation: soft blob shadow, wheel spin (blur above a speed), front-wheel steering, scratch decals.
    // Sits on the car root made by Placeholder. No model changes: wheels are found by node name.
    public class VehicleFx : MonoBehaviour
    {
        const float BlurKmh = 70; // above this the rims swap to a flat disc, spokes would alias at 60 fps
        const int MaxScratches = 6;

        static Material blobMat, scratchMat, blurMat;
        static Texture2D scratchTex;

        class Wheel
        {
            public Transform t; public Renderer[] rs; public Material[][] mats;
            public Vector3 baseP, centerP, axisP, upP; public Quaternion baseR;
            public float radius; public bool front;
        }
        readonly List<Wheel> wheels = new List<Wheel>();
        bool blurred;
        float angle;
        readonly List<Transform> scratches = new List<Transform>();
        int nextScratch;
        float halfW, midY;

        // spark = any material whose shader works for transparent particles.
        public static void Init(Material spark)
        {
            if (blobMat) return;
            blobMat = Dark(Blob());
            scratchTex = Scratch();
            scratchMat = Alpha(spark, scratchTex);
            blurMat = Placeholder.Mat(Placeholder.Hex(0x2E2F33), 0, 0.2f);
        }

        // Transparent URP Lit (the particle material blended additively, so black showed nothing).
        static Material Dark(Texture2D tex)
        {
            var m = new Material(Placeholder.BaseMat) { mainTexture = tex, color = Color.white, renderQueue = (int)RenderQueue.Transparent };
            m.SetFloat("_Surface", 1); m.SetFloat("_Blend", 0);
            m.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha); m.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha); m.SetFloat("_ZWrite", 0);
            m.SetFloat("_Smoothness", 0); m.SetFloat("_SpecularHighlights", 0); m.SetFloat("_EnvironmentReflections", 0);
            m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT"); m.EnableKeyword("_SPECULARHIGHLIGHTS_OFF"); m.EnableKeyword("_ENVIRONMENTREFLECTIONS_OFF");
            m.SetOverrideTag("RenderType", "Transparent");
            return m;
        }

        static Material Alpha(Material spark, Texture2D tex)
        {
            var m = new Material(spark) { mainTexture = tex, color = Color.white, renderQueue = (int)RenderQueue.Transparent };
            m.SetFloat("_Blend", 0);
            m.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha); m.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha); m.SetFloat("_ZWrite", 0);
            m.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            return m;
        }

        // Soft rounded rectangle, dark in the middle, no hard edge.
        static Texture2D Blob()
        {
            const int N = 64;
            var tex = new Texture2D(N, N, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
            for (int y = 0; y < N; y++)
                for (int x = 0; x < N; x++)
                {
                    float u = (x + 0.5f) / N * 2 - 1, v = (y + 0.5f) / N * 2 - 1;
                    float r = Mathf.Pow(Mathf.Pow(Mathf.Abs(u), 4) + Mathf.Pow(Mathf.Abs(v), 4), 0.25f);
                    float a = 1 - Mathf.SmoothStep(0, 1, Mathf.InverseLerp(0.45f, 1, r));
                    tex.SetPixel(x, y, new Color(0, 0, 0, a * 0.8f));
                }
            tex.Apply();
            return tex;
        }

        // A few thin pale diagonal lines.
        static Texture2D Scratch()
        {
            const int W = 64, H = 32;
            var tex = new Texture2D(W, H, TextureFormat.RGBA32, true) { wrapMode = TextureWrapMode.Clamp };
            var px = new Color[W * H];
            for (int i = 0; i < px.Length; i++) px[i] = new Color(0.92f, 0.92f, 0.9f, 0);
            for (int k = 0; k < 3; k++)
                for (int x = 6; x < W - 6; x++)
                {
                    int y = 8 + k * 7 + (x - 6) * 6 / (W - 12) - 3 + (k == 1 ? 2 : 0);
                    float fade = Mathf.Sin((x - 6f) / (W - 12) * Mathf.PI);
                    px[y * W + x] = new Color(0.92f, 0.92f, 0.9f, 0.85f * fade);
                }
            tex.SetPixels(px); tex.Apply();
            return tex;
        }

        // Call once after Placeholder.Fit / Vehicle. soft = blob offset sign (+1 player, -1 oncoming: root turned 180).
        public static VehicleFx Attach(GameObject car, bool player)
        {
            var fx = car.AddComponent<VehicleFx>();
            var rs = car.GetComponentsInChildren<Renderer>();
            var b = rs[0].bounds;
            foreach (var r in rs)
            {
                r.shadowCastingMode = ShadowCastingMode.Off; // no real-time car shadows: they flickered and had hard edges
                r.receiveShadows = false;
                b.Encapsulate(r.bounds);
            }
            fx.halfW = b.extents.x; fx.midY = b.size.y * 0.4f;

            var blob = GameObject.CreatePrimitive(PrimitiveType.Quad);
            Destroy(blob.GetComponent<Collider>());
            blob.name = "Blob";
            blob.transform.SetParent(car.transform, false);
            float s = player ? 1 : -1;
            blob.transform.localPosition = new Vector3(0.2f * s, 0.06f, 0.35f * s); // pushed along the sun direction
            blob.transform.localRotation = Quaternion.Euler(90, 0, 0);
            blob.transform.localScale = new Vector3(b.size.x * 1.7f, b.size.z * 1.4f, 1);
            var br = blob.GetComponent<Renderer>();
            br.sharedMaterial = blobMat; br.shadowCastingMode = ShadowCastingMode.Off; br.receiveShadows = false;

            fx.FindWheels(car.transform, b.center.z, player);
            return fx;
        }

        void FindWheels(Transform root, float midZ, bool player)
        {
            foreach (var t in root.GetComponentsInChildren<Transform>())
            {
                string n = t.name.ToLowerInvariant();
                bool hit = n.Contains("wheel") || n.StartsWith("disk") || n.StartsWith("tire") || (player && n.StartsWith("cylinder."));
                if (!hit || t.GetComponentsInChildren<Renderer>().Length == 0 || wheels.Exists(w => t.IsChildOf(w.t))) continue;
                var rs = t.GetComponentsInChildren<Renderer>();
                var b = rs[0].bounds;
                foreach (var r in rs) b.Encapsulate(r.bounds);
                if (b.size.y > b.size.z * 1.6f || b.size.y < 0.15f || b.size.y > 1.5f) continue; // not a wheel-shaped part
                var p = t.parent;
                var w = new Wheel
                {
                    t = t, rs = rs, mats = new Material[rs.Length][], baseP = t.localPosition, baseR = t.localRotation,
                    centerP = p.InverseTransformPoint(b.center), axisP = p.InverseTransformDirection(root.right).normalized,
                    upP = p.InverseTransformDirection(root.up).normalized, radius = b.size.y / 2, front = b.center.z > midZ,
                };
                for (int i = 0; i < rs.Length; i++) w.mats[i] = rs[i].sharedMaterials;
                wheels.Add(w);
            }
        }

        // speed in m/s along the car's own forward; steerDeg only turns the front wheels.
        public void Drive(float speed, float dt, float steerDeg = 0)
        {
            if (wheels.Count == 0) return;
            angle += speed * dt / Mathf.Max(wheels[0].radius, 0.1f) * Mathf.Rad2Deg;
            bool blur = speed * 3.6f > BlurKmh;
            foreach (var w in wheels)
            {
                var q = Quaternion.AngleAxis(angle, w.axisP);
                if (w.front && steerDeg != 0) q = Quaternion.AngleAxis(steerDeg, w.upP) * q;
                w.t.localRotation = q * w.baseR;
                w.t.localPosition = w.centerP + q * (w.baseP - w.centerP);
                if (blur != blurred) Swap(w, blur);
            }
            blurred = blur;
        }

        static void Swap(Wheel w, bool blur)
        {
            for (int i = 0; i < w.rs.Length; i++)
            {
                if (!blur) { w.rs[i].sharedMaterials = w.mats[i]; continue; }
                var ms = new Material[w.mats[i].Length];
                for (int k = 0; k < ms.Length; k++) ms[k] = blurMat;
                w.rs[i].sharedMaterials = ms;
            }
        }

        // Light hit: a scratch on the side, z in the car's own frame. side -1 left, +1 right.
        public void Scratch(float side, float z)
        {
            if (side == 0) return;
            if (scratches.Count < MaxScratches)
            {
                var q = GameObject.CreatePrimitive(PrimitiveType.Quad);
                Destroy(q.GetComponent<Collider>());
                q.name = "Scratch"; q.transform.SetParent(transform, false);
                var r = q.GetComponent<Renderer>();
                r.sharedMaterial = scratchMat; r.shadowCastingMode = ShadowCastingMode.Off; r.receiveShadows = false;
                scratches.Add(q.transform);
            }
            var t = scratches[nextScratch++ % MaxScratches];
            t.gameObject.SetActive(true);
            t.localPosition = new Vector3(side * (halfW + 0.01f), midY, z);
            t.localRotation = Quaternion.Euler(0, -side * 90, Random.Range(-15f, 15f)); // quad faces outward
            t.localScale = new Vector3(0.7f, 0.35f, 1);
        }

        public void ClearScratches()
        {
            foreach (var t in scratches) t.gameObject.SetActive(false);
            nextScratch = 0;
        }
    }
}
