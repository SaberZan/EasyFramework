namespace Easy
{
    public enum NormalInitOrderIndexEnum
    {
        QualityMgr = 1 * 100 + 1,
        ProxyMgr = 1 * 100 + 2,
        ConfigMgr = 1 * 100 + 3,
        SaveDataMgr = 2 * 100 + 1,
        DataPatchMgr = 3 * 100 + 1,
        CacheDataMgr = 4 * 100 + 1,
        CoroutineMgr = 5 * 100 + 1,
        UIMgr = 6 * 100 + 1,
    }
}