using System.Collections.Generic;
using UnityEngine;

namespace Geisterfahrer.Game
{
    // Primitive stand-in models and shared materials. Replaced by the designer's models later.
    public static class Placeholder
    {
        static readonly Dictionary<Color, Material> mats = new Dictionary<Color, Material>();
        // A URP Lit material asset referenced by the scene, so the shader and its fog variants ship in the build.
        public static Material BaseMat;

        public static Material Mat(Color c)
        {
            if (mats.TryGetValue(c, out var m)) return m;
            m = new Material(BaseMat) { color = c };
            mats[c] = m;
            return m;
        }

        public static GameObject Box(Transform parent, Vector3 pos, Vector3 size, Color c, PrimitiveType type = PrimitiveType.Cube)
        {
            var go = GameObject.CreatePrimitive(type);
            Object.Destroy(go.GetComponent<Collider>());
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localScale = size;
            go.GetComponent<Renderer>().sharedMaterial = Mat(c);
            return go;
        }

        // Root at the footprint center on the ground, +Z = front. The model sits in a child named "Model".
        public static GameObject Vehicle(string name, float l, float w, float h, Color body)
        {
            var root = new GameObject(name).transform;
            var model = new GameObject("Model").transform;
            model.SetParent(root, false);
            bool big = h > 3;
            float wheel = big ? 0.5f : 0.33f;
            float bodyH = big ? h - wheel : h * 0.5f;
            Box(model, new Vector3(0, wheel + bodyH / 2, 0), new Vector3(w, bodyH, l), body);
            if (!big)
                Box(model, new Vector3(0, wheel + bodyH + h * 0.15f - 0.05f, -l * 0.05f), new Vector3(w * 0.85f, h * 0.3f, l * 0.5f), new Color(0.15f, 0.17f, 0.2f));
            else
                Box(model, new Vector3(0, h * 0.7f, l / 2 + 0.01f), new Vector3(w * 0.9f, h * 0.25f, 0.02f), new Color(0.15f, 0.17f, 0.2f));
            foreach (float x in new[] { -1f, 1f })
            {
                foreach (float z in new[] { -0.33f, 0.33f })
                {
                    var wh = Box(model, new Vector3(x * (w / 2 - 0.05f), wheel, z * l), new Vector3(wheel * 2, 0.12f, wheel * 2), new Color(0.08f, 0.08f, 0.08f), PrimitiveType.Cylinder);
                    wh.transform.localRotation = Quaternion.Euler(0, 0, 90);
                }
                Box(model, new Vector3(x * (w / 2 - 0.3f), wheel + bodyH * 0.6f, l / 2 + 0.01f), new Vector3(0.3f, 0.12f, 0.04f), new Color(1f, 1f, 0.9f));
            }
            return root.gameObject;
        }
    }
}
