using Geisterfahrer.Core;
using TMPro;
using UnityEngine;

namespace Geisterfahrer.Game
{
    // The road around the player (look-spec 6, 7). Long parts stay still and scroll their texture;
    // posts and field strips wrap around rings. Everything is placed through Place(),
    // the one point that maps road space (z ahead, d across) to the world, so curves can bend it later.
    public class RoadView : MonoBehaviour
    {
        const float Ring = 600, Behind = 40, Len = 700;
        Material asphalt, grass, dashMat;
        Transform[] posts, delineators, strips;
        float[] stripBase, stripRing;
        Transform kmPost;
        TMP_Text kmText;

        // Straight road for now. Curves, hills: change this only.
        public static Vector3 Place(float zAhead, float d, float y = 0) => new Vector3(d, y, zAhead);

        public void Init(Art art)
        {
            var t = transform;
            float rail = (float)Road.RailX, mid = Len / 2 - 60;

            // Asphalt: 1 texture tile per 2.08 m, scrolled by run distance.
            asphalt = art.asphalt ? new Material(art.asphalt) : Placeholder.Mat(Placeholder.Hex(0x4A4C50), 0, 0.3f);
            var road = Placeholder.Box(t, Place(mid, 0, -0.05f), new Vector3(rail * 2 + 0.4f, 0.1f, Len), asphalt);
            asphalt.mainTextureScale = new Vector2((rail * 2 + 0.4f) / 2.08f, Len / 2.08f);
            if (art.asphalt) asphalt.color = Placeholder.Hex(0x9C9EA2); // darker, cooler grey: less glare than the raw warm texture
            road.GetComponent<Renderer>().receiveShadows = true;

            // Grass verge and median (0-15 m past the rails), 1 tile per 3 m.
            grass = art.grass ? new Material(art.grass) : Placeholder.Mat(Placeholder.Hex(0x6F8A3E), 0, 0.1f);
            grass.color = art.grass ? Placeholder.Hex(0xD8DCC8) : grass.color;
            grass.mainTextureScale = new Vector2(15 / 3f, Len / 3f);
            foreach (float s in new[] { -1f, 1f })
                Placeholder.Box(t, Place(mid, s * (rail + 7.6f), -0.06f), new Vector3(15, 0.1f, Len), grass);
            Placeholder.Box(t, Place(mid, 0, -0.2f), new Vector3(1200, 0.1f, Len), Placeholder.Hex(0x6F8A3E)); // far ground

            // Lines: solid 0.30 m edge lines, dashed 0.15 m lane lines (6 m line, 12 m gap) by texture offset.
            var paint = Placeholder.Mat(Placeholder.Hex(0xE9E9E4), 0, 0.35f);
            foreach (float s in new[] { -1f, 1f })
                Placeholder.Box(t, Place(mid, s * (float)(Road.LaneCount * Road.LaneWidth / 2 + 0.15), 0.004f), new Vector3(0.3f, 0.01f, Len), paint);
            // 1 texel per 0.25 m with trilinear + aniso, so far dashes fade smoothly instead of sparkling.
            var tex = new Texture2D(4, 72) { filterMode = FilterMode.Trilinear, anisoLevel = 8, wrapMode = TextureWrapMode.Repeat };
            for (int y = 0; y < 72; y++)
                for (int x = 0; x < 4; x++) tex.SetPixel(x, y, y < 24 ? Color.white : new Color(1, 1, 1, 0));
            tex.Apply();
            // art.dash is an alpha-clipped Lit asset, so the clip shader variant ships in the build.
            dashMat = new Material(art.dash ? art.dash : paint) { mainTexture = tex, mainTextureScale = new Vector2(1, Len / 18), color = paint.color };
            for (int i = 1; i < Road.LaneCount; i++)
            {
                var q = Placeholder.Box(t, Place(mid, (float)(Road.LaneX(i) - Road.LaneWidth / 2), 0.005f), new Vector3(0.15f, Len, 1), dashMat, PrimitiveType.Quad);
                q.transform.localRotation = Quaternion.Euler(90, 0, 0);
            }

            // Guardrail beams (0.31 m tall, centre 0.55 m) and posts every 2 m over the near 140 m.
            var steel = Placeholder.Mat(Placeholder.Hex(0xA9AFB5), 0.85f, 0.55f);
            foreach (float s in new[] { -1f, 1f })
                Placeholder.Box(t, Place(mid, s * (rail + 0.04f), 0.55f), new Vector3(0.085f, 0.31f, Len), steel);
            posts = new Transform[140];
            for (int i = 0; i < posts.Length; i++)
                posts[i] = Placeholder.Box(t, Vector3.zero, new Vector3(0.1f, 0.75f, 0.05f), steel).transform;

            // Delineators every 50 m, 0.5 m outside the rail: white post with a black band.
            var white = Placeholder.Mat(Placeholder.Hex(0xF2F2F0), 0, 0.3f);
            delineators = new Transform[(int)(Ring / 50) * 2];
            for (int i = 0; i < delineators.Length; i++)
            {
                var p = new GameObject("Delineator").transform;
                p.SetParent(t, false);
                Placeholder.Box(p, new Vector3(0, 0.5f, 0), new Vector3(0.12f, 1f, 0.1f), white);
                Placeholder.Box(p, new Vector3(0, 0.725f, 0), new Vector3(0.125f, 0.25f, 0.105f), Placeholder.Hex(0x1A1A1A));
                delineators[i] = p;
            }

            // Km post every 500 m on the outer (left) side; number = run km.
            kmPost = new GameObject("KmPost").transform;
            kmPost.SetParent(t, false);
            Placeholder.Box(kmPost, new Vector3(0, 0.6f, 0), new Vector3(0.08f, 1.2f, 0.08f), Placeholder.Hex(0x8B9097));
            Placeholder.Box(kmPost, new Vector3(0, 1.35f, 0.01f), new Vector3(0.53f, 0.38f, 0.03f), Placeholder.Hex(0x111111));
            Placeholder.Box(kmPost, new Vector3(0, 1.35f, 0), new Vector3(0.5f, 0.35f, 0.04f), Placeholder.Hex(0xF4F4F2));
            kmText = new GameObject("Km").AddComponent<TextMeshPro>();
            kmText.transform.SetParent(kmPost, false);
            kmText.transform.localPosition = new Vector3(0, 1.35f, -0.03f);
            kmText.rectTransform.sizeDelta = new Vector2(0.5f, 0.35f);
            if (art.sign) kmText.font = art.sign;
            kmText.fontSize = 2.2f; kmText.color = Placeholder.Hex(0x111111);
            kmText.alignment = TextAlignmentOptions.Center;

            // Fields: coloured strips in 3 columns per side from 15 m past the rail, wrapping around a ring.
            var colors = new[] { 0x6F8A3E, 0xC8B46A, 0x8A7556, 0x7E9A4A };
            var rnd = new System.Random(7);
            var list = new System.Collections.Generic.List<Transform>();
            var bases = new System.Collections.Generic.List<float>();
            var rings = new System.Collections.Generic.List<float>();
            foreach (float s in new[] { -1f, 1f })
            {
                float x = rail + 15;
                for (int c = 0; c < 3; c++)
                {
                    float w = 30 + (float)rnd.NextDouble() * 50, z = 0;
                    int first = list.Count;
                    while (z < Ring)
                    {
                        float l = 40 + (float)rnd.NextDouble() * 80;
                        list.Add(Placeholder.Box(t, Vector3.zero, new Vector3(w, 0.1f, l), Placeholder.Mat(Placeholder.Hex(colors[rnd.Next(4)]), 0, 0.1f)).transform);
                        list[list.Count - 1].localPosition = new Vector3(s * (x + w / 2), -0.08f, 0);
                        bases.Add(z + l / 2);
                        z += l;
                    }
                    for (int i = first; i < list.Count; i++) rings.Add(z);
                    x += w;
                }
            }
            strips = list.ToArray(); stripBase = bases.ToArray(); stripRing = rings.ToArray();
            foreach (var s in strips) s.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        public void Scroll(double distance)
        {
            asphalt.mainTextureOffset = new Vector2(0, (float)(distance / 2.08 % 1));
            grass.mainTextureOffset = new Vector2(0, (float)(distance / 3 % 1));
            dashMat.mainTextureOffset = new Vector2(0, (float)(distance / 18 % 1));
            float rail = (float)Road.RailX;
            for (int i = 0; i < posts.Length; i++)
                posts[i].localPosition = Place(Wrap(i / 2 * 2f - distance, 140, 10), (i % 2 == 0 ? -1 : 1) * (rail + 0.12f), 0.375f);
            for (int i = 0; i < delineators.Length; i++)
                delineators[i].localPosition = Place(Wrap(i / 2 * 50f - distance, Ring, Behind), (i % 2 == 0 ? -1 : 1) * (rail + 0.5f));
            float kz = Wrap(-distance, 500, Behind);
            kmPost.localPosition = Place(kz, -(rail + 1f));
            kmText.text = ((distance + kz) / 1000).ToString("F1");
            for (int i = 0; i < strips.Length; i++)
            {
                var p = strips[i].localPosition;
                p.z = Wrap(stripBase[i] - distance, stripRing[i], 120);
                strips[i].localPosition = p;
            }
        }

        static float Wrap(double z, float ring, float behind) => (float)(((z + behind) % ring + ring) % ring - behind);
    }
}
