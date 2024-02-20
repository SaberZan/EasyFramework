using System.Collections.Generic;

namespace Easy
{
    public class PathInOneMapCache : PathCache
    {
        private bool _everyTimeRefreshTag = false;

        public Dictionary<int, PathInOneMap> caches = new Dictionary<int, PathInOneMap>();

        public PathInOneMapCache(bool everyTimeRefreshTag = false)
        {
            this._everyTimeRefreshTag = everyTimeRefreshTag;
        }

        public bool IsHadPath(int mapNodeTypes)
        {
            return this.caches.ContainsKey(mapNodeTypes);
        }

        public PathInOneMap GetPath(int mapNodeTypes)
        {
            return caches[mapNodeTypes];
        }

        public void ChangePath(int mapNodeTypes, PathInOneMap pathInOneMap)
        {
            this.caches[mapNodeTypes] = pathInOneMap;
        }

        public void CheckPathOk(FindPathMap findPathMap, int mapNodeTypes)
        {
            if (this.caches.ContainsKey(mapNodeTypes))
            {
                if (this._everyTimeRefreshTag)
                {
                    this.caches.Remove(mapNodeTypes);
                    return;
                }

                bool isPathOk = true;
                PathInOneMap pathInOneMap = this.caches[mapNodeTypes];
                for (int i = 0; i < pathInOneMap.pathOnePoints.Count; ++i)
                {
                    UnityEngine.Vector3 v3 = pathInOneMap.root.position + pathInOneMap.pathOnePoints[i].locationPos;
                    MapNode mapNode = findPathMap.GetMapNodeByWorldPos(v3);
                    if (mapNode.IsObstacle(mapNodeTypes))
                    {
                        isPathOk = false;
                        break;
                    }
                }

                if (!isPathOk)
                {
                    this.caches.Remove(mapNodeTypes);
                }
            }
        }
    }

}