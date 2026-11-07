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
        [Tooltip("Models per vehicle type (Road.Types order); several files per type give variety. Empty = placeholder.")]
        public Models[] vehicles = new Models[0];
        public GameObject player;
    }

    [Serializable]
    public class Models { public GameObject[] list = new GameObject[0]; }
}
