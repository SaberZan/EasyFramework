namespace Easy
{

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using UnityEngine;
    using Easy;

    //补丁,数据后加载
    [NormalInit]
    [OrderIndex((int) NormalInitOrderIndexEnum.DataPatchMgr)]
    public class SaveDataPatchMgr : Singleton<SaveDataPatchMgr>
    {
        public override void BeforeRestart()
        {

        }

        public override void Init(InitCompleteCallback complete)
        {
            AddPatch();
            complete.Invoke(true);
        }

        int GetSortId(string version)
        {
            string[] splitstr = version.Split('.');
            int sortidx = int.Parse(splitstr[0]) * 1000 + int.Parse(splitstr[1]) * 100;
            return sortidx;
        }


        public void AddPatch()
        {
            var types = EasyFrameworkMain.Instance.GetTypes();

            string curversion = SaveDataMgr.Instance.Get<PrimaryData>().version;
            //玩家当前code
            int curSortId = GetSortId(curversion);
            
            List<object> listType = new List<object>();
            foreach (var t in types)
            {
                if (t.BaseType == typeof(PatchBase))
                {
                    var data = t.GetConstructor(Type.EmptyTypes)?.Invoke(null);
                    if (data != null)
                    {
                        PropertyInfo property =
                            t.GetProperty("patchVersion", BindingFlags.Instance | BindingFlags.Public);
                        string strversion = property.GetValue(data, null) as string;
                        int tmpSortId = GetSortId(strversion);
                        if (tmpSortId > curSortId)
                        {
                            listType.Add(data);
                        }
                    }
                }
            }
            
            //排序
            listType.Sort((object t1, object t2) =>
            {
                string v1 = t1.GetType().GetProperty("patchVersion", BindingFlags.Instance | BindingFlags.Public)
                    .GetValue(t1, null) as string;
                string v2 = t2.GetType().GetProperty("patchVersion", BindingFlags.Instance | BindingFlags.Public)
                    .GetValue(t2, null) as string;
                int sort1 = GetSortId(v1);
                int sort2 = GetSortId(v2);
                return sort1 < sort2 ? -1 : 1;
            });
            
            foreach (var t in listType)
            {
                MethodInfo method = t.GetType().GetMethod("AddPatch", BindingFlags.Instance | BindingFlags.Public);
                if (method != null)
                {
                    method.Invoke(t, new object[] { });
                }
                else
                {
                    EasyLogger.LogError("版本补丁函数AddPatch未实现");
                }

            }
            
            SaveDataMgr.Instance.Get<PrimaryData>().version = Application.version;
        }

    }

}