using System;
using UnityEngine;

namespace Easy
{
    public class FindPathMapInitParams
    {
        public int maxX;
        public int maxY;
        public int defautMapNodeType;
        public bool eightDir;
        public Transform root;
        public Vector3 offset;
        public float minHeight;
        public float maxHeight;
        public float actualGap;
    }

    public class FindPathMap
    {
        /**
     * 地图的所在的节点
     */
        public Transform root;

        /**
     * A*地图相对于root节点的偏移量
     */
        public Vector3 offset;

        /**
     * 地图最低高度
     */
        public float minHeight;

        /**
     * 地图最大高度
     */
        public float maxHeight;

        /**
     * 地图间隔
     */
        public float actualGap;

        /**
     * A*地图
     */
        public NodeMap aStarMap;


        public FindPathMap(FindPathMapInitParams findPathMapInitParams)
        {
            this.aStarMap = new NodeMap(findPathMapInitParams.maxX, findPathMapInitParams.maxY,
                findPathMapInitParams.defautMapNodeType, findPathMapInitParams.eightDir);
            this.root = findPathMapInitParams.root;
            this.offset = findPathMapInitParams.offset;
            this.minHeight = findPathMapInitParams.minHeight;
            this.maxHeight = findPathMapInitParams.maxHeight;
            this.actualGap = findPathMapInitParams.actualGap;
        }


        /**
     * 
     * @param worldPos 
     * @returns 
     */
        public bool IsOwn(Vector3 worldPos)
        {
            MapNode mapNode = this.GetMapNodeByWorldPos(worldPos);
            return mapNode != null;
        }

        /**
     * 获得地图的世界坐标
     */
        public Vector3 GetMapWorldPos()
        {
            return this.root.TransformPoint(this.offset);
        }

        /**
     * 通过地图节点获得世界坐标
     * @param mapNode 
     */
        public Vector3 GetWorldPos(MapNode mapNode)
        {
            Vector3 v3 = this.offset + new Vector3(mapNode.x * this.actualGap, 0, mapNode.y * this.actualGap);
            return this.root.TransformPoint(v3);
        }

        /**
     * 通过世界坐标得到地图节点
     * @param worldPos 
     * @returns 
     */
        public MapNode GetMapNodeByWorldPos(Vector3 worldPos)
        {
            MapNode mapNode = null;
            worldPos = this.root.InverseTransformPoint(worldPos);
            worldPos = worldPos - this.offset;
            if (worldPos.y > this.minHeight || worldPos.y < this.maxHeight)
            {
                int x = Mathf.RoundToInt(worldPos.x / this.actualGap);
                int y = Mathf.RoundToInt(worldPos.z / this.actualGap);
                mapNode = this.GetMapNode(x, y);
            }

            return mapNode;
        }

        /**
     * 通过世界坐标得到地图节点
     * @param worldPos 
     * @returns 
     */
        public MapNode GetMapNode(int x, int y)
        {
            return this.aStarMap.GetMapNode(x, y);
        }

        /**
     * 通过地图节点获得路点
     * @param mapNode 
     * @returns 
     */
        public PathOnePoint GetPathOnePoint(MapNode mapNode)
        {
            PathOnePoint pathOnePoint = new PathOnePoint();
            pathOnePoint.locationPos =
                new Vector3(mapNode.x * this.actualGap, 0, mapNode.y * this.actualGap) + this.offset;
            pathOnePoint.mapNodeType = mapNode.mapNodeType;
            return pathOnePoint;
        }
    }

}