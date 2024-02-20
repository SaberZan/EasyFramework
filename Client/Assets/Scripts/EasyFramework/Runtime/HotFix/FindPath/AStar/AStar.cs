namespace Easy
{

    using System;
    using System.Collections.Generic;
    using System.Linq;

    /**
 * A*寻路
 */

    public class AStar
    {
        /**
     * 最大寻找次数 
     */
        private int _MAX_FIND_NUM = 5000;

        /**
     * 直线花费
     */
        private int _STRAIGHT_COST = 10;

        /**
     * 斜线花费
     */
        private int _OBLIQUE_COST = 14;

        private Dictionary<int, MapNode> _openDic = new Dictionary<int, MapNode>();

        private Dictionary<int, MapNode> _closeDic = new Dictionary<int, MapNode>();

        private MapNode _startNode;

        private MapNode _endNode;

        private List<MapNode> _endNodes = new List<MapNode>();

        private int _maxF;

        private NodeMap _map;

        public AStar()
        {

        }

        public List<MapNode> FindPath(NodeMap nodeMap, MapNode start, MapNode end, int mapNodeTypes,
            bool endLinks = true)
        {
            this._map = nodeMap;
            this._map.Refresh();
            if (!this._map.IsInMap(start.x, start.y) || !this._map.IsInMap(end.x, end.y))
            {
                // DebugerMgr.Instance().Log("AStar-- 在地图外");
                return new List<MapNode>();
            }

            if (start == end)
            {
                return new List<MapNode>() {start};
            }

            this._openDic.Clear();
            this._closeDic.Clear();
            this._endNodes.Clear();
            this._startNode = this._map.GetMapNode(start.x, start.y);
            if (this._startNode == null || this._startNode.IsObstacle(mapNodeTypes))
            {
                // DebugerMgr.Instance().Log("AStar-- 没有起点或者起点不可走" + (this.startNode == null));
                return new List<MapNode>();
            }

            this._endNode = this._map.GetMapNode(end.x, end.y);
            if (this._endNode == null)
            {
                // DebugerMgr.Instance().Log("AStar-- 没有终点");
                return new List<MapNode>();
            }

            if (this._endNode.IsObstacle(mapNodeTypes))
            {
                if (endLinks)
                {
                    this._endNodes = _map.GetLinkPoints(this._endNode.x, this._endNode.y, mapNodeTypes);
                }
            }
            else
            {
                this._endNodes.Add(this._endNode);
            }

            if (this._endNodes.Count == 0)
            {
                // DebugerMgr.Instance().Log("AStar-- 终点不可到达");
                return new List<MapNode>();
            }

            this._maxF = this.GetManhattanCost(this._map.GetMapNode(0, 0),
                this._map.GetMapNode(this._map.maxX - 1, this._map.maxY - 1));

            int findNum = 0;
            MapNode currentNode = this._startNode;
            this.AddOpenDic(currentNode);

            while (true)
            {
                ++findNum;
                if (findNum >= this._MAX_FIND_NUM)
                {
                    // DebugerMgr.Instance().Log("AStar-- 找太多次了");
                    return new List<MapNode>();
                }

                currentNode = this.SelectNodeInOpenDic();
                if (currentNode == null)
                {
                    // DebugerMgr.Instance().Log("AStar-- 没用路");
                    return new List<MapNode>();
                }

                if (this.IsEndNode(currentNode))
                {
                    return this.BuildPath(currentNode);
                }

                this.RemoveFromOpenDic(currentNode);
                this.AddCloseDic(currentNode);

                List<MapNode> links = this._map.GetLinkPoints(currentNode.x, currentNode.y, mapNodeTypes);
                for (int i = 0; i < links.Count(); ++i)
                {
                    MapNode link = links[i];
                    if (!this.IsInCloseDic(link))
                    {
                        int newG = currentNode.g;
                        if (link.x == currentNode.x || link.y == currentNode.y)
                        {
                            newG += this._STRAIGHT_COST * link.mapNodeType;
                        }
                        else
                        {
                            newG += this._OBLIQUE_COST * link.mapNodeType;
                        }

                        if (this.IsInOpenDic(link))
                        {
                            if (newG < link.g)
                            {
                                link.g = newG;
                                link.h = this.GetManhattanCost(link, this._endNode);
                                link.parent = currentNode;
                            }
                        }
                        else
                        {
                            link.g = newG;
                            link.h = this.GetManhattanCost(link, this._endNode);
                            link.parent = currentNode;
                            this.AddOpenDic(link);
                        }
                    }
                }
            }
        }

        public List<MapNode> BuildPath(MapNode endNode)
        {
            List<MapNode> path = new List<MapNode>();
            MapNode tmpNode = endNode;
            while (tmpNode.parent != null || tmpNode.parent != null)
            {
                path.Add(tmpNode);
                tmpNode = tmpNode.parent;
            }

            path.Add(tmpNode);
            path.Reverse();
            return path;
        }

        public MapNode SelectNodeInOpenDic()
        {
            MapNode miniFNode = null;
            foreach (var kv in this._openDic)
            {
                MapNode node = kv.Value;
                // DebugerMgr.Instance(DebugerMgr).Log("node == " + (node == null) + " " + key);
                if (node == null)
                    continue;
                if (miniFNode == null)
                {
                    miniFNode = node;
                }
                else if (node.f < miniFNode.f)
                {
                    miniFNode = node;
                }
            }

            return miniFNode;
        }

        public int GetManhattanCost(MapNode node1, MapNode node2)
        {
            int dx = System.Math.Abs(node1.x - node2.x);
            int dy = System.Math.Abs(node1.y - node2.y);
            return this._STRAIGHT_COST * (dx + dy);
        }

        private bool IsStartNode(MapNode node)
        {
            return this._startNode.Equals(node);
        }

        private bool IsEndNode(MapNode node)
        {
            return this._endNodes.Contains(node);
        }

        private bool IsInOpenDic(MapNode node)
        {
            return this._openDic.ContainsKey(node.key);
        }

        private void AddOpenDic(MapNode node)
        {
            this._openDic[node.key] = node;
        }

        private bool RemoveFromOpenDic(MapNode node)
        {
            if (this.IsInOpenDic(node))
            {
                this._openDic.Remove(node.key);
                return true;
            }

            return false;
        }

        private bool IsInCloseDic(MapNode node)
        {
            return this._closeDic.ContainsKey(node.key);
        }

        private void AddCloseDic(MapNode node)
        {
            this._closeDic[node.key] = node;
        }

        private void RemoveFromCloseDic(MapNode node)
        {
            this._closeDic.Remove(node.key);
        }
    }

}
