using System.Collections.Generic;
using UnityEngine;

namespace Geisterfahrer.Game
{
    // The material instances of one vehicle that carry its body colour, so a pooled vehicle can be repainted on reuse.
    public class Paintable : MonoBehaviour
    {
        readonly List<Material> mats = new List<Material>();

        public void Add(Material m) => mats.Add(m);

        // Takes over another vehicle's paint materials (parts of a combination).
        public void Merge(Paintable other) { mats.AddRange(other.mats); other.mats.Clear(); }

        public void Paint(Placeholder.PaintColor p)
        {
            foreach (var m in mats)
            {
                m.color = p.Color;
                m.SetFloat("_Metallic", p.Metallic);
                m.SetFloat("_Smoothness", p.Smoothness);
            }
        }
    }
}
