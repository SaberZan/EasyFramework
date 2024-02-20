using System.Collections.Generic;

namespace Easy
{
    public class PathCrossTwoMapCache : PathCache
    {
        public Dictionary<int, PathCrossTwoMap> caches = new Dictionary<int, PathCrossTwoMap>();

        public bool IsHadPath(int mapNodeTypes)
        {
            return this.caches.ContainsKey(mapNodeTypes);
        }

        public void ChangePath(int mapNodeTypes, PathCrossTwoMap pathCrossTwoMap)
        {
            this.caches[mapNodeTypes] = pathCrossTwoMap;
        }

        public PathCrossTwoMap GetPath(int mapNodeTypes)
        {
            return caches[mapNodeTypes];
        }
    }

}