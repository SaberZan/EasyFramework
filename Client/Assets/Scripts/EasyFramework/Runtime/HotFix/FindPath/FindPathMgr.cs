namespace Easy
{

    using System.Collections.Generic;
    using UnityEngine;
    
    /**
     * 多地图传送点寻路
     */

    public class FindPathMgr : Singleton<FindPathMgr>
    {

        private Dictionary<string, FindPathMap> _allMaps = new Dictionary<string, FindPathMap>();

        private List<PassPoint> _allPassPoints = new List<PassPoint>();

        private AStar _aStar = new AStar();

        public override void Init(InitCompleteCallback complete)
        {
            
        }

        public override void BeforeRestart()
        {
            
        }

        public List<PathInMap> FindPath(UnityEngine.Vector3 start, UnityEngine.Vector3 end, int mapNodeTypes)
        {

            List<PathInMap> finalPath = new List<PathInMap>();

            string startMapKey = this.GetOwnMapKey(start);
            string endMapKey = this.GetOwnMapKey(end);

            if (startMapKey == null)
            {
                //起点在地图外
                // DebugerMgr.Instance().Log("起点在地图外");
                return new List<PathInMap>();
            }

            if (endMapKey == null)
            {
                //终点在地图外
                // DebugerMgr.Instance().Log("终点在地图外");
                return new List<PathInMap>();
            }


            if (startMapKey == endMapKey)
            {
                //如果在同一张地图，直接寻路得出结果
                FindPathMap findPathMap = this._allMaps[startMapKey];
                MapNode startNode = findPathMap.GetMapNodeByWorldPos(start);
                MapNode endNode = findPathMap.GetMapNodeByWorldPos(end);

                if (startNode != null && endNode != null)
                {
                    List<MapNode> findPath =
                        this._aStar.FindPath(findPathMap.aStarMap, startNode, endNode, mapNodeTypes);
                    if (findPath.Count > 0)
                    {
                        //有路正常走
                        PathInOneMap pathInOneMap = new PathInOneMap();
                        pathInOneMap.root = findPathMap.root;
                        for (int i = 0; i < findPath.Count; ++i)
                        {
                            pathInOneMap.pathOnePoints.Add(findPathMap.GetPathOnePoint(findPath[i]));
                        }

                        finalPath.Add(pathInOneMap);
                    }
                    else
                    {
                        //没有路走传送点
                        finalPath = this.FindPathWithPassPoints(start, end, mapNodeTypes);
                    }
                }
            }
            else
            {
                //非同一张地图,通过传送点算出最优传送点路径
                finalPath = this.FindPathWithPassPoints(start, end, mapNodeTypes);
            }

            return finalPath;
        }

        private List<PathInMap> FindPathWithPassPoints(UnityEngine.Vector3 start, UnityEngine.Vector3 end,
            int mapNodeTypes)
        {
            for (int index = 0; index < this._allPassPoints.Count; ++index)
            {
                PassPoint passPoint = this._allPassPoints[index];
                passPoint.g = 0;
                passPoint.h = 0;
                passPoint.parent = null;
                passPoint.startPath = null;
                passPoint.endPath = null;
                for (int i = 0; i < passPoint.paths.Count; ++i)
                {
                    FindPathMap findPathMap = this._allMaps[passPoint.ownAStarKey];
                    PathCache passPath = passPoint.paths[i];
                    if (passPath is PathInOneMapCache pathInOneMapCache)
                    {
                        pathInOneMapCache.CheckPathOk(findPathMap, mapNodeTypes);
                    }
                }
            }

            int MaxFind = 100;
            int findNum = 0;
            List<PassPoint> open = new List<PassPoint>();
            List<PassPoint> close = new List<PassPoint>();

            MapNode startNode = null;
            MapNode endNode = null;

            string startMapKey = this.GetOwnMapKey(start);
            string endMapKey = this.GetOwnMapKey(end);

            for (int i = 0; i < this._allPassPoints.Count; ++i)
            {
                PassPoint passPoint = this._allPassPoints[i];
                if (passPoint.ownAStarKey == startMapKey)
                {
                    FindPathMap findPathMap = this._allMaps[passPoint.ownAStarKey];
                    startNode = findPathMap.GetMapNodeByWorldPos(start);
                    endNode = findPathMap.GetMapNodeByWorldPos(passPoint.worldPos);
                    List<MapNode> findPath =
                        this._aStar.FindPath(findPathMap.aStarMap, startNode, endNode, mapNodeTypes, false);
                    if (findPath.Count > 0)
                    {
                        PathInOneMap pathInOneMap = new PathInOneMap();
                        pathInOneMap.root = findPathMap.root;
                        for (int index = 0; index < findPath.Count; ++index)
                        {
                            pathInOneMap.pathOnePoints.Add(findPathMap.GetPathOnePoint(findPath[index]));
                        }

                        passPoint.startPath = pathInOneMap;
                        Vector3 v3 = pathInOneMap.GetWorldPos(pathInOneMap.pathOnePoints.Count - 1);
                        passPoint.g = this.GetDistance(start, v3);
                        passPoint.h = this.GetDistance(v3, end);
                        open.Add(passPoint);
                    }
                }
            }

            if (open.Count == 0)
            {
                //DebugerMgr.Instance().Log("起点无法到Pass点");
                return new List<PathInMap>();
            }

            List<PassPoint> endPassPoints = new List<PassPoint>();
            for (int i = 0; i < this._allPassPoints.Count; ++i)
            {
                PassPoint passPoint = this._allPassPoints[i];
                if (passPoint.ownAStarKey == endMapKey)
                {
                    FindPathMap findPathMap = this._allMaps[passPoint.ownAStarKey];
                    startNode = findPathMap.GetMapNodeByWorldPos(passPoint.worldPos);
                    endNode = findPathMap.GetMapNodeByWorldPos(end);
                    if (endNode != null)
                    {
                        List<MapNode> findPath =
                            this._aStar.FindPath(findPathMap.aStarMap, startNode, endNode, mapNodeTypes);
                        if (findPath.Count > 0)
                        {
                            PathInOneMap pathInOneMap = new PathInOneMap();
                            pathInOneMap.root = findPathMap.root;
                            for (int index = 0; index < findPath.Count; ++index)
                            {
                                pathInOneMap.pathOnePoints.Add(findPathMap.GetPathOnePoint(findPath[i]));
                            }

                            passPoint.endPath = pathInOneMap;
                            endPassPoints.Add(passPoint);
                        }
                    }
                }
            }

            if (endPassPoints.Count == 0)
            {
                //DebugerMgr.Instance().Log("终点无法到Pass点");
                return new List<PathInMap>();
            }

            PassPoint currentPassPoint = null;
            while (true)
            {
                ++findNum;
                if (findNum >= MaxFind)
                {
                    //DebugerMgr.Instance().Log("Pass点找太多次");
                    return new List<PathInMap>();
                }

                currentPassPoint = null;
                for (int i = 0; i < open.Count; ++i)
                {
                    PassPoint passPoint = open[i];
                    if (currentPassPoint == null)
                    {
                        currentPassPoint = passPoint;
                    }
                    else if (passPoint.f < currentPassPoint.f)
                    {
                        currentPassPoint = passPoint;
                    }
                }

                if (currentPassPoint == null)
                {
                    //DebugerMgr.Instance().Log("Pass点无法找到");
                    return new List<PathInMap>();
                }

                if (endPassPoints.Contains(currentPassPoint))
                {
                    List<PassPoint> path = new List<PassPoint>();
                    PassPoint tmpPassPoint = currentPassPoint;
                    while (tmpPassPoint.parent != null)
                    {
                        path.Add(tmpPassPoint);
                        tmpPassPoint = tmpPassPoint.parent;
                    }

                    path.Add(tmpPassPoint);
                    path.Reverse();
                    return this.BuildPathWithPassPoints(path, mapNodeTypes);
                }

                open.Remove(currentPassPoint);
                close.Add(currentPassPoint);

                for (int i = 0; i < this._allPassPoints.Count; ++i)
                {
                    PassPoint passPoint = this._allPassPoints[i];
                    int index = passPoint.links.IndexOf(currentPassPoint);
                    if (passPoint.links[index] == currentPassPoint)
                    {
                        if (passPoint.paths[index] is PathInOneMapCache pathInOneMapCache &&
                            !pathInOneMapCache.IsHadPath(mapNodeTypes))
                        {
                            this.RefreshPassPointPath(passPoint, index, mapNodeTypes);
                        }
                    }

                    if (index != -1 && close.Contains(passPoint))
                    {
                        float newG = currentPassPoint.g;
                        float temp = 0;
                        if (passPoint.paths[index] is PathInOneMapCache pathInOneMapCache)
                        {
                            if (!pathInOneMapCache.IsHadPath(mapNodeTypes))
                            {
                                continue;
                            }

                            temp = this.GetDistance(currentPassPoint.worldPos, passPoint.worldPos);
                        }
                        else if (passPoint.paths[index] is PathCrossTwoMapCache pathCrossTwoMapCache)
                        {
                            if (!pathCrossTwoMapCache.IsHadPath(mapNodeTypes))
                            {
                                continue;
                            }

                            temp = pathCrossTwoMapCache.caches[mapNodeTypes].GetPassDis();
                        }

                        newG += temp;

                        if (open.Contains(passPoint))
                        {
                            if (newG < passPoint.g)
                            {
                                passPoint.g = newG;
                                passPoint.h = this.GetDistance(passPoint.worldPos, end);
                                passPoint.parent = currentPassPoint;
                            }
                        }
                        else
                        {
                            passPoint.g = newG;
                            passPoint.h = this.GetDistance(passPoint.worldPos, end);
                            passPoint.parent = currentPassPoint;
                            open.Add(passPoint);
                        }
                    }

                }
            }
        }

        private List<PathInMap> BuildPathWithPassPoints(List<PassPoint> passPoints, int mapNodeTypes)
        {
            List<PathInMap> finalPath = new List<PathInMap>();
            if (passPoints.Count > 0)
            {
                //进入传送点遍历
                for (int i = 0; i < passPoints.Count; ++i)
                {
                    PassPoint nowPassPoint = passPoints[i];
                    PathInMap passPath = null;
                    PathInOneMap start = null;
                    PathInOneMap end = null;
                    if (i < passPoints.Count - 1)
                    {
                        PassPoint nextPassPoint = passPoints[i + 1];
                        int index = nowPassPoint.links.IndexOf(nextPassPoint);
                        if (nowPassPoint.paths[index] is PathInOneMapCache pathInOneMapCache)
                        {
                            passPath = pathInOneMapCache.GetPath(mapNodeTypes);
                        }
                        else if (nowPassPoint.paths[index] is PathCrossTwoMapCache pathCrossTwoMapCache)
                        {
                            passPath = pathCrossTwoMapCache.GetPath(mapNodeTypes);
                        }
                    }

                    if (i == 0)
                    {
                        //先到第一个传送点
                        start = nowPassPoint.startPath;
                    }
                    else if (i == passPoints.Count - 1)
                    {
                        //最后一个传送点算出到终点距离
                        end = nowPassPoint.endPath;
                    }

                    if (start != null)
                    {
                        finalPath.Add(start);
                    }

                    if (passPath != null)
                    {
                        finalPath.Add(passPath);
                    }

                    if (end != null)
                    {
                        finalPath.Add(end);
                    }
                }
            }

            passPoints.Clear();
            passPoints = null;
            return finalPath;
        }

        public void AddPassPoint(PassPoint passPoint1, PassPoint passPoint2, PathCrossTwoMapCache pathCrossTwoMapCache)
        {
            passPoint1.links.Add(passPoint2);
            passPoint1.paths.Add(pathCrossTwoMapCache);
            passPoint2.links.Add(passPoint1);
            passPoint2.paths.Add(pathCrossTwoMapCache);
            this.RefreshInOnMapPass(passPoint1);
            this.RefreshInOnMapPass(passPoint2);
            this._allPassPoints.Add(passPoint1);
            this._allPassPoints.Add(passPoint2);
        }

        private void RefreshInOnMapPass(PassPoint passPoint1)
        {
            for (int i = 0; i < this._allPassPoints.Count; ++i)
            {
                PassPoint passPoint = this._allPassPoints[i];
                if (passPoint.ownAStarKey == passPoint1.ownAStarKey)
                {
                    FindPathMap findPathMap = this._allMaps[passPoint.ownAStarKey];
                    MapNode startNode = findPathMap.GetMapNodeByWorldPos(passPoint1.worldPos);
                    MapNode endNode = findPathMap.GetMapNodeByWorldPos(passPoint.worldPos);
                    //如果陆地和可拆障碍物可以连接视为可连接
                    List<MapNode> findPath = this._aStar.FindPath(findPathMap.aStarMap, startNode, endNode,
                        (int) MapNodeType.Land | (int) MapNodeType.Water | (int) MapNodeType.Obstacle, false);
                    if (findPath.Count > 0)
                    {
                        passPoint1.links.Add(passPoint);
                        passPoint.links.Add(passPoint1);
                        passPoint1.paths.Add(new PathInOneMapCache());
                        passPoint.paths.Add(new PathInOneMapCache());
                    }
                }
            }
        }

        private void RefreshPassPointPath(PassPoint passPoint, int index, int mapNodeTypes)
        {
            PassPoint nextPassPoint = passPoint.links[index];

            FindPathMap findPathMap = this._allMaps[passPoint.ownAStarKey];
            MapNode startNode = findPathMap.GetMapNodeByWorldPos(passPoint.worldPos);
            MapNode endNode = findPathMap.GetMapNodeByWorldPos(nextPassPoint.worldPos);

            List<MapNode> aStarPath =
                this._aStar.FindPath(findPathMap.aStarMap, startNode, endNode, mapNodeTypes, false);
            if (aStarPath.Count > 0)
            {
                PathInOneMap pathInOneMap = new PathInOneMap();
                pathInOneMap.root = findPathMap.root;
                for (int i = 0; i < aStarPath.Count; ++i)
                {
                    pathInOneMap.pathOnePoints.Add(findPathMap.GetPathOnePoint(aStarPath[i]));
                }

                PathInOneMap pathInOneMapNext = new PathInOneMap();
                pathInOneMapNext.root = findPathMap.root;
                pathInOneMapNext.pathOnePoints.AddRange(pathInOneMap.pathOnePoints);
                pathInOneMapNext.pathOnePoints.Reverse();
                int tmpIndex = nextPassPoint.links.IndexOf(passPoint);
                ((PathInOneMapCache) passPoint.paths[index]).ChangePath(mapNodeTypes, pathInOneMap);
                ((PathInOneMapCache) nextPassPoint.paths[tmpIndex]).ChangePath(mapNodeTypes, pathInOneMapNext);
            }
            else
            {
                int tmpIndex = nextPassPoint.links.IndexOf(passPoint);
                ((PathInOneMapCache) passPoint.paths[index]).ChangePath(mapNodeTypes, null);
                ((PathInOneMapCache) nextPassPoint.paths[tmpIndex]).ChangePath(mapNodeTypes, null);
            }
        }


        public void ChangeMapNodeType(FindPathMap findPathMap, List<MapPoint> positions, int mapNodeType)
        {
            for (int i = 0; i < positions.Count; ++i)
            {
                findPathMap.aStarMap.ChangeMapNodeType(positions[i].x, positions[i].y, mapNodeType);
            }
        }

        public void AddMap(string key, FindPathMap map)
        {
            this._allMaps[key] = map;
        }


        public FindPathMap GetMap(string key)
        {
            return this._allMaps[key];
        }

        public FindPathMap GetOwnMap(Vector3 worldPos)
        {
            foreach (var kv in this._allMaps)
            {
                if (kv.Value.IsOwn(worldPos))
                {
                    return kv.Value;
                }
            }

            return null;
        }

        public string GetOwnMapKey(Vector3 worldPos)
        {
            foreach (var kv in this._allMaps)
            {
                if (kv.Value.IsOwn(worldPos))
                {
                    return kv.Key;
                }
            }

            return null;
        }

        /**
     * 判断两个点的距离花费
     */
        private float GetDistance(UnityEngine.Vector3 pos1, UnityEngine.Vector3 pos2)
        {
            return UnityEngine.Vector3.Distance(pos1, pos2);
        }

    }

}
