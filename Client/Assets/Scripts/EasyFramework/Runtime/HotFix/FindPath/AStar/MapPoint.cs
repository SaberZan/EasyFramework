namespace Easy
{
    public class MapPoint
    {
        public int x;

        public int y;

        public string str;

        public MapPoint(int x, int y)
        {
            this.x = x;
            this.y = y;
            this.str = this.x + "_" + this.y;
        }

        public bool Equals(MapPoint other)
        {
            return this.x == other.x && this.y == other.y;
        }

        public string ToString()
        {
            return this.str;
        }
    }

}
