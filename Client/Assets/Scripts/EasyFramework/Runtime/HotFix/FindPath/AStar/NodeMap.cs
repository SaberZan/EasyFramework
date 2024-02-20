namespace Easy
{

    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class NodeMap
    {
        /**
     * 地图上最大的X
     */
        public int maxX;

        /**
     * 地图上最大的X
     */
        public int maxY;

        /**
     * 地图点集合
     */
        public Dictionary<int, MapNode> mapNodes;

        /**
     * 是否8方向
     */
        public bool eightDir;

        /**
     * 间隔，生成MapNode 的 key用
     */
        private int _interval;


        public NodeMap(int maxX, int maxY, int defautMapNodeType, bool eightDir = true)
        {
            this.maxX = maxX;
            this.maxY = maxY;
            this.eightDir = eightDir;
            this._interval = 1 << System.Math.Max(this.maxX, this.maxY);
            this.mapNodes = new Dictionary<int, MapNode>();

            for (int y = 0; y < this.maxY; ++y)
            {
                for (int x = 0; x < this.maxX; ++x)
                {
                    MapNode mapNode = new MapNode(x, y, defautMapNodeType, x * this._interval + y);
                    this.mapNodes[mapNode.key] = mapNode;
                }
            }
        }

        public void Refresh()
        {
            foreach (var kv in this.mapNodes)
            {
                kv.Value.Clear();
            }
        }

        public void ChangeMapNodeType(int x, int y, int mapNodeType)
        {
            if (IsInMap(x, y))
            {
                int key = x * this._interval + y;
                if (this.mapNodes.ContainsKey(key))
                {
                    this.mapNodes[key] = new MapNode(x, y, mapNodeType, key);
                }
                else
                {
                    this.mapNodes[key].mapNodeType = mapNodeType;
                }
            }
        }

        public bool IsInMap(int x, int y)
        {
            return x >= 0 && x <= this.maxX && y >= 0 && y <= this.maxY;
        }

        public bool IsReachable(int x, int y, int mapNodeTypes)
        {
            if (this.IsInMap(x, y))
            {
                int key = x * this._interval + y;
                return this.mapNodes[key].IsReachable(mapNodeTypes);
            }

            return false;
        }

        public MapPoint GetMaxSize()
        {
            return new MapPoint(this.maxX, this.maxY);
        }

        public MapPoint GetCenterPoint()
        {
            return new MapPoint(this.maxX / 2, this.maxY / 2);
        }

        public MapNode GetMapNode(int x, int y)
        {
            int key = x * this._interval + y;
            if (this.IsInMap(x, y) && this.mapNodes.ContainsKey(key))
            {
                return this.mapNodes[key];
            }

            return null;
        }

        /// <summary>
        /// 获取相邻的链接节点信息对象
        /// </summary>
        /// <param name="point">目标点</param>
        /// <returns>节点信息对象</returns>
        public List<MapNode> GetLinkPoints(int x, int y, int mapNodeTypes, int step = 1)
        {
            List<MapNode> links = new List<MapNode>();
            for (int i = -1 * step; i <= 1 * step; i++)
            {
                for (int j = -1 * step; j <= step; j++)
                {
                    if (this.eightDir)
                    {
                        if ((i != 0 || j != 0))
                        {
                            MapNode mapNode = this.GetMapNode(x + i, y + j);
                            if (mapNode != null && mapNode.IsReachable(mapNodeTypes))
                            {
                                links.Add(mapNode);
                            }
                        }
                    }
                    else
                    {
                        if ((i != 0 && j == 0) || (i == 0 && j != 0))
                        {
                            MapNode mapNode = this.GetMapNode(x + i, y + j);
                            if (mapNode != null && mapNode.IsReachable(mapNodeTypes))
                            {
                                links.Add(mapNode);
                            }
                        }
                    }
                }
            }

            return links;
        }

    }

}
