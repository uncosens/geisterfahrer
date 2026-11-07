using System.Collections.Generic;
using UnityEngine;

namespace Geisterfahrer.Game
{
    // The material instances of one vehicle that carry its body colour, so a pooled vehicle can be repainted on reuse.
    public class Paintable : MonoBehaviour
    {
        readonly List<Material> mats = new List<Material>();

        public void Add(Material m) => mats.Add(m);

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
