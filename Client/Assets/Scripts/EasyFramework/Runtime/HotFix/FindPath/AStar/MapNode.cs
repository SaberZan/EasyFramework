namespace Easy
{

    public class MapNode : MapPoint
    {
        public int key;

        public int g;

        public int h;

        public int f => this.g + this.h;

        public MapNode parent;

        public int mapNodeType;

        public MapNode(int x, int y, int mapNodeType, int key) : base(x, y)
        {
            this.mapNodeType = mapNodeType;
            this.key = key;
        }

        public bool IsContain(int mapNodeTypes)
        {
            return this.mapNodeType == (mapNodeTypes & this.mapNodeType);
        }

        public bool IsReachable(int mapNodeTypes)
        {
            return this.mapNodeType == (mapNodeTypes & this.mapNodeType);
        }

        public bool IsObstacle(int mapNodeTypes)
        {
            return this.mapNodeType != (mapNodeTypes & this.mapNodeType);
        }

        public int GetKey()
        {
            return this.key;
        }

        public void Clear()
        {
            this.g = 0;
            this.h = 0;
            this.parent = null;
        }
    }

}
