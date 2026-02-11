using System;
using System.Collections.Generic;

namespace Easy
{
    /// <summary>
    /// 用户持久化数据类
    /// </summary>
    public class SqliteDataList
    {
        protected bool initComplete = false;
        public void InitComplete()
        {
            initComplete = true;
        }

        public virtual void SyncData()
        {

        }
    }

    public class SqliteDataList<T> : SqliteDataList where T : SqliteData, new()
    {
        public List<T> list = new List<T>();

        public void Add(T item)
        {
            list.Add(item);
            if (initComplete)
            {
                item.dataStage = DataStage.Added;
            }
        }

        public void AddRange(IEnumerable<T> collection)
        {
            list.AddRange(collection);
            if (initComplete)
            {
                foreach (var item in collection)
                {
                    item.dataStage = DataStage.Added;
                }
            }
        }

        public void Remove(T item)
        {
            list.Remove(item);
        }

        public void Clear()
        {
            list.Clear();
        }

        public T GetFrist()
        {
            if (list.Count > 0)
            {
                return list[0];
            }
            return null;
        }

        public T GetData(Predicate<T> match)
        {
            return list.Find(match);
        }

        public override void SyncData()
        {
            for (int i = list.Count - 1; i >= 0; --i)
            {
                if (list[i].dataStage == DataStage.Changed || list[i].dataStage == DataStage.Added)
                {
                    SqliteMgr.Instance.UpdateTable<T>(list[i]).Trigger();
                    list[i].dataStage = DataStage.Normal;
                }
                else if (list[i].dataStage == DataStage.Deleted)
                {
                    SqliteMgr.Instance.DeleteTable<T>(list[i]).Trigger();
                    list.RemoveAt(i);
                }
            }
        }
    }

}