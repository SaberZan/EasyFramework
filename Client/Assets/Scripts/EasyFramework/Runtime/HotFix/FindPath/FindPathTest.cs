namespace Easy
{

    using System.Collections.Generic;
    using UnityEngine;

    public class FindPathTest
    {
        public void Test()
        {
            GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
            //FindPath Test
            FindPathMapInitParams testFindMapInitParams = new FindPathMapInitParams();
            testFindMapInitParams.actualGap = 0.1f;
            testFindMapInitParams.defautMapNodeType = (int) MapNodeType.Land;
            testFindMapInitParams.eightDir = false;
            testFindMapInitParams.maxX = 10;
            testFindMapInitParams.maxY = 10;
            testFindMapInitParams.minHeight = -0.5f;
            testFindMapInitParams.maxHeight = 0.5f;
            testFindMapInitParams.offset = UnityEngine.Vector3.zero;
            testFindMapInitParams.root = plane.transform;
            FindPathMap testFindMap = new FindPathMap(testFindMapInitParams);
            FindPathMgr.Instance.AddMap("TestMap", testFindMap);
            FindPathMgr.Instance.ChangeMapNodeType(testFindMap,
                new List<MapPoint>() {new MapPoint(3, 3), new MapPoint(4, 4)}, (int) MapNodeType.Obstacle);
            List<PathInMap> findPath = FindPathMgr.Instance.FindPath(
                testFindMap.GetWorldPos(testFindMap.GetMapNode(1, 1)),
                testFindMap.GetWorldPos(testFindMap.GetMapNode(8, 8)), (int) MapNodeType.Land);
            //DebugerMgr.Instance().Log("findPath = " + findPath.length);
            for (int i = 0; i < findPath.Count; ++i)
            {
                if (findPath[i] is PathInOneMap pathInOneMap)
                {
                    for (int j = 0; j < pathInOneMap.pathOnePoints.Count; ++j)
                    {
                        //DebugerMgr.Instance().Log("pos = " + j + " " + findPath[i].pathOnePoints[j].locationPos);
                    }
                }
                else if (findPath[i] is PathCrossTwoMap pathCrossTwoMap)
                {
                    for (int j = 0; j < pathCrossTwoMap.pathOnePoints.Count; ++j)
                    {
                        //DebugerMgr.Instance().Log("pos = " + j + " " + findPath[i].pathOnePoints[j].locationPos);
                    }
                }
            }
        }
    }

}