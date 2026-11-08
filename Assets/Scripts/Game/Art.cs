using System;
using TMPro;
using UnityEngine;

namespace Geisterfahrer.Game
{
    // Asset references set by BuildScript.Setup. Anything missing (no ThirdParty download yet) falls back to flat colours.
    [Serializable]
    public class Art
    {
        public Material baseLit, asphalt, grass, dash, spark, facade; // facade: Geisterfahrer/Facade shader (scenery)
        public TMP_FontAsset bold, semi, black, sign;
        public Material boldShadow, semiShadow;
        [Tooltip("Prefabs per variant (Road.Variants order), one per part of its VehicleRecipes recipe. Empty = placeholder.")]
        public Models[] vehicles = new Models[0];
        public GameObject player;
    }

    [Serializable]
    public class Models { public GameObject[] list = new GameObject[0]; }
}
