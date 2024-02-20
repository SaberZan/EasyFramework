namespace Easy
{

    using System.Collections.Generic;

    public class PassPoint
    {
        public string ownAStarKey;

        public UnityEngine.Vector3 worldPos;

        public List<PassPoint> links = new List<PassPoint>(); //同一张地图可联通的传送点 和pass组件相连的点为link点

        public List<PathCache> paths = new List<PathCache>(); //包含 PassComponent 和 PathInOneMap， 可直接取出使用

        public PathInOneMap startPath = null; //缓存的起点到此Pass点的寻路路径

        public PathInOneMap endPath = null; //缓存此Pass点到终点到的寻路路径

        //以下寻路算法用
        public PassPoint parent;

        public float g = 0;

        public float h = 0;

        public float f => this.g + this.h;

        PassPoint(string ownAStarKey, UnityEngine.Vector3 worldPos)
        {
            this.ownAStarKey = ownAStarKey;
            this.worldPos = worldPos;
        }

        public void Clear()
        {
            this.g = 0;
            this.h = 0;
            this.parent = null;
        }

    }
}