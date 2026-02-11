using System;
using System.Collections;
using System.Collections.Generic;
using Easy;
using SQLite;
using UnityEngine;

namespace Easy
{
    public class SqliteMgr : Singleton<SqliteMgr>
    {
        private Dictionary<Type, SqliteDataList> dataDict = new Dictionary<Type, SqliteDataList>();
        private SQLiteAsyncConnection db;

        private bool triggerTag = false;

        public override void BeforeRestart()
        {
            db?.CloseAsync();
            db = null;
        }

        public override void Init(InitCompleteCallback complete)
        {
            InitData().Trigger();
            complete?.Invoke(true);
        }

        private async EasyVoidTask InitData()
        {
            db = new SQLiteAsyncConnection(Application.persistentDataPath + "/sqlite.db", true);
            List<Type> types = EasyFrameworkMain.Instance.GetTypes();
            foreach (var t in types)
            {
                if (typeof(SqliteData).IsAssignableFrom(t) && t != typeof(SqliteData))
                {
                    await db.CreateTableAsync(t);
                }
            }
        }

        public async EasyTask<TableMapping> GetTableMapping<T>() where T : SqliteData, new()
        {
            return await db.GetMappingAsync<T>();
        }

        public async EasyTask<SqliteDataList<T>> GetTable<T>() where T : SqliteData, new()
        {
            if (!dataDict.ContainsKey(typeof(T)))
            {
                var dbList = await db.Table<T>().ToListAsync();
                if (!dataDict.ContainsKey(typeof(T)))
                {
                    dataDict.Add(typeof(T), new SqliteDataList<T>());
                }
                var list = (SqliteDataList<T>)dataDict[typeof(T)];
                list.Clear();
                list.AddRange(dbList);
                list.InitComplete();
            }
            return (SqliteDataList<T>)dataDict[typeof(T)];
        }

        public async EasyVoidTask UpdateTable<T>(T t) where T : SqliteData, new()
        {
            await db.InsertOrReplaceAsync(t);
        }

        public async EasyVoidTask DeleteTable<T>(T t) where T : SqliteData, new()
        {
            await db.DeleteAsync(t);
        }

        public async EasyEmptyTask Drop<T>() where T : SqliteData, new()
        {
            var tableMapping = await db.GetMappingAsync<T>();
            await db.DropTableAsync(tableMapping);
        }

        public void TriggerSync()
        {
            if (triggerTag)
                return;
            triggerTag = true;
            CoroutineMgr.Instance.StartCoroutine(SyncData());
        }

        IEnumerator SyncData()
        {
            yield return new WaitForEndOfFrame();
            foreach (var item in dataDict)
            {
                item.Value.SyncData();
            }
            triggerTag = false;
        }

    }
}