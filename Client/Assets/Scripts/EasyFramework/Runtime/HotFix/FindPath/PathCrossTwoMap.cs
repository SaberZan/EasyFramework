namespace Easy
{

    using System.Collections.Generic;

    public class PathCrossTwoMap : PathInMap
    {

        public List<PathOnePoint> pathOnePoints;

        public UnityEngine.Transform root;

        /**
     * 如果是传送点，距离要重载
     * @returns 
     */
        public float GetPassDis()
        {
            return UnityEngine.Vector3.Distance(this.pathOnePoints[0].locationPos,
                this.pathOnePoints[this.pathOnePoints.Count - 1].locationPos);
        }
    }

}