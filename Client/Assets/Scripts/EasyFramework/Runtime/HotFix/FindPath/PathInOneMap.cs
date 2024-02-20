using System.Collections.Generic;

namespace Easy
{
    public class PathInOneMap : PathInMap
    {

        public List<PathOnePoint> pathOnePoints = new List<PathOnePoint>();

        public UnityEngine.Transform root;

        /**
     * 需要重载
     * @param index 
     */
        public UnityEngine.Vector3 GetWorldPos(int index)
        {
            return this.root.position + this.pathOnePoints[index].locationPos;
        }
    }

}