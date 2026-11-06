using System;
using TMPro;
using UnityEngine;

namespace Geisterfahrer.Game
{
    // Asset references set by BuildScript.Setup. Anything missing (no ThirdParty download yet) falls back to flat colours.
    [Serializable]
    public class Art
    {
        public Material baseLit, asphalt, grass, dash, spark;
        public TMP_FontAsset bold, semi, black, sign;
        public Material boldShadow, semiShadow;
        [Tooltip("Models by vehicle type index (Compact, Sedan, Van, Bus, Truck). Empty = placeholder.")]
        public GameObject[] vehicles = new GameObject[0];
        public GameObject player;
    }
}
