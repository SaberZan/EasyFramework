namespace Easy
{
    public enum MapNodeType
    {
        Land = 1 << 0, //地面
        Water = 1 << 1, //水面
        Obstacle = 1 << 2, //障碍
        AlwaysObstacle = 1 << 3, //永久障碍
        Air = 1 << 4, //空气
    }

}
