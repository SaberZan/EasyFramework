namespace Easy
{
    public enum DataStage
    {
        Normal,
        Added,
        Changed,
        Deleted
    }

    /// <summary>
    /// 用户持久化数据类
    /// </summary>
    public abstract class SqliteData
    {
        [SQLite.Ignore]
        public DataStage dataStage { set; get; } = DataStage.Normal;
    }

}