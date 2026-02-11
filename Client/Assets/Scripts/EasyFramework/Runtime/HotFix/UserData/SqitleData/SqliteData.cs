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
        public DataStage _dataStage = DataStage.Normal;

        [SQLite.Ignore]
        public DataStage dataStage
        {
            set
            {
                _dataStage = value;
                if (_dataStage != DataStage.Normal)
                {
                    SqliteMgr.Instance.TriggerSync();
                }
            }
            get
            {
                return _dataStage;
            }
        }
    }

}