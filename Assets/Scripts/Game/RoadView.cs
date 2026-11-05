using Geisterfahrer.Core;
using UnityEngine;

namespace Geisterfahrer.Game
{
    // Static road strip; lane dashes scroll by texture offset, posts wrap around a 500 m ring.
    public class RoadView : MonoBehaviour
    {
        const float Ring = 500, PostStep = 50, Behind = 30;
        Material dashMat;
        Transform[] posts;
        Transform kmPost;

        void Awake()
        {
            var t = transform;
            float len = 700, mid = len / 2 - 60;
            var asphalt = new Color(0.33f, 0.34f, 0.36f);
            Placeholder.Box(t, new Vector3(0, -0.05f, mid), new Vector3((float)Road.RailX * 2 + 0.4f, 0.1f, len), asphalt);
            Placeholder.Box(t, new Vector3(0, -0.12f, mid), new Vector3(600, 0.1f, len), new Color(0.36f, 0.5f, 0.27f));

            // Solid edge lines and guardrails.
            foreach (float s in new[] { -1f, 1f })
            {
                Placeholder.Box(t, new Vector3(s * (float)(Road.LaneWidth * 1.5 + 0.1), 0.005f, mid), new Vector3(0.2f, 0.01f, len), Color.white);
                Placeholder.Box(t, new Vector3(s * (float)(Road.RailX + 0.05), 0.65f, mid), new Vector3(0.1f, 0.32f, len), new Color(0.75f, 0.77f, 0.8f));
            }

            // Dashed lane lines: 6 m line, 12 m gap (one 18 m texture period).
            var tex = new Texture2D(1, 18) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Repeat };
            for (int i = 0; i < 18; i++) tex.SetPixel(0, i, i < 6 ? Color.white : asphalt);
            tex.Apply();
            dashMat = new Material(Placeholder.Mat(Color.white)) { mainTexture = tex, mainTextureScale = new Vector2(1, len / 18) };
            foreach (float s in new[] { -0.5f, 0.5f })
            {
                var q = Placeholder.Box(t, new Vector3(s * (float)Road.LaneWidth, 0.006f, mid), new Vector3(0.15f, len, 1), Color.white, PrimitiveType.Quad);
                q.transform.localRotation = Quaternion.Euler(90, 0, 0);
                q.GetComponent<Renderer>().sharedMaterial = dashMat;
            }

            // Delineator posts (Leitpfosten) every 50 m on both sides, white with a black band.
            int n = (int)(Ring / PostStep);
            posts = new Transform[n * 2];
            for (int i = 0; i < posts.Length; i++)
            {
                var p = new GameObject("Post").transform;
                p.SetParent(t, false);
                float x = (i % 2 == 0 ? -1 : 1) * (float)(Road.RailX + 0.35);
                p.localPosition = new Vector3(x, 0, 0);
                Placeholder.Box(p, new Vector3(0, 0.5f, 0), new Vector3(0.12f, 1f, 0.12f), Color.white);
                Placeholder.Box(p, new Vector3(0, 0.85f, 0), new Vector3(0.13f, 0.15f, 0.13f), Color.black);
                posts[i] = p;
            }

            kmPost = new GameObject("KmPost").transform;
            kmPost.SetParent(t, false);
            Placeholder.Box(kmPost, new Vector3(0, 0.6f, 0), new Vector3(0.08f, 1.2f, 0.08f), Color.white);
            Placeholder.Box(kmPost, new Vector3(0, 1.3f, 0), new Vector3(0.05f, 0.45f, 0.6f), Color.white);
        }

        public void Scroll(double distance)
        {
            dashMat.mainTextureOffset = new Vector2(0, (float)(distance / 18 % 1));
            for (int i = 0; i < posts.Length; i++)
            {
                var p = posts[i].localPosition;
                p.z = Wrap((i / 2) * PostStep - distance, Ring);
                posts[i].localPosition = p;
            }
            kmPost.localPosition = new Vector3(-(float)(Road.RailX + 0.6), 0, Wrap(-distance, Ring));
        }

        static float Wrap(double z, float ring) => (float)(((z + Behind) % ring + ring) % ring - Behind);
    }
}
