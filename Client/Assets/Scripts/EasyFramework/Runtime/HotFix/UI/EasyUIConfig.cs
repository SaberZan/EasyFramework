using System;
using UnityEngine;

namespace Easy
{

    [Serializable]
    public class EasyUIConfig : EasyConfig
    {
        [Label("UI宽高")]
        [SerializeField]
        public Vector2Int size = new Vector2Int(1080, 1920);
    }
}