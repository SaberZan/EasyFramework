using System;
using System.Collections.Generic;
using UnityEngine.Serialization;

namespace Easy.EasyAsset
{
    /// <summary>
    /// 在所有AB包被的资源信息
    /// </summary>
    [Serializable]
    public class Catalogs
    {
        /// <summary>
        /// 版本号
        /// </summary>
        public long version;

        /// <summary>
        /// 版本号
        /// </summary>
        public long versionCode;

        /// <summary>
        /// 更新地址
        /// </summary>
        public List<string> updateUrls = new List<string>();

        /// <summary>
        /// ab资源信息列表
        /// </summary>
        public List<EasyAssetBundleInfo> allEasyAssetBundleInfos = new List<EasyAssetBundleInfo>();

        /// <summary>
        /// 主动调用的资源信息列表
        /// </summary>
        public List<EasyAssetInfo> allActiveEasyAssetInfos = new List<EasyAssetInfo>();
        
        /// <summary>
        /// 被动依赖的资源信息列表
        /// </summary>
        public List<EasyAssetInfo> allPassiveEasyAssetInfos = new List<EasyAssetInfo>();
        
        /// <summary>
        /// 资源类型列表
        /// </summary>
        public List<string> assemblyQualifiedNames = new List<string>();

        /// <summary>
        /// 资源类型列表
        /// </summary>
        [NonSerialized]
        public Dictionary<int, Type> types = new Dictionary<int, Type>();

        /// <summary>
        /// 所有主动依赖数据的字典库
        /// </summary>
        [NonSerialized]
        public Dictionary<int, Dictionary<string, EasyAssetInfo>> allActiveEasyAssetInfoDic = new Dictionary<int, Dictionary<string, EasyAssetInfo>>();

        public Dictionary<int, List<EasyAssetInfo>> ab2EasyAssetInfoDic = new Dictionary<int, List<EasyAssetInfo>>();

        public void Init()
        {
            for (int i = 0; i < allActiveEasyAssetInfos.Count; i++)
            {
                EasyAssetInfo easyAssetInfo = allActiveEasyAssetInfos[i];
                int assetNameLength = easyAssetInfo.asset.Length;
                if(!allActiveEasyAssetInfoDic.ContainsKey(assetNameLength))
                {
                    allActiveEasyAssetInfoDic.Add(assetNameLength, new Dictionary<string, EasyAssetInfo>(StringComparer.OrdinalIgnoreCase));
                }
                allActiveEasyAssetInfoDic[assetNameLength].Add(easyAssetInfo.asset, easyAssetInfo);
                if (!ab2EasyAssetInfoDic.ContainsKey(easyAssetInfo.abIndex))
                {
                    ab2EasyAssetInfoDic.Add(easyAssetInfo.abIndex, new List<EasyAssetInfo>());
                }
                ab2EasyAssetInfoDic[easyAssetInfo.abIndex].Add(easyAssetInfo);
            }
        }

        /// <summary>
        /// 获取AB包信息对象
        /// </summary>
        /// <param name="abName">ab包名</param>
        /// <param name="index">下标</param>
        /// <returns>信息对象</returns>
        public EasyAssetBundleInfo GetEasyAssetBundleInfoByName(string abName, out int index)
        {
            index = -1;
            for (int i = 0; i < allEasyAssetBundleInfos.Count; i++)
            {
                EasyAssetBundleInfo easyAssetBundleInfo = allEasyAssetBundleInfos[i];
                if (easyAssetBundleInfo.abName.Equals(abName))
                {
                    index = i;
                    return easyAssetBundleInfo;
                }
            }
            return null;
        }

        /// <summary>
        /// 获取AB包信息对象
        /// </summary>
        /// <param name="md5">abmd5</param>
        /// <returns>信息对象</returns>
        public EasyAssetBundleInfo GetEasyAssetBundleInfoByMD5(string md5, out int index)
        {
            index = -1;
            for (int i = 0; i < allEasyAssetBundleInfos.Count; i++)
            {
                EasyAssetBundleInfo easyAssetBundleInfo = allEasyAssetBundleInfos[i];
                if (easyAssetBundleInfo.md5.Equals(md5))
                {
                    index = i;
                    return easyAssetBundleInfo;
                }
            }
            return null;
        }

        /// <summary>
        /// 获取AB包信息对象
        /// </summary>
        /// <param name="index">下标</param>
        /// <returns>信息对象</returns>
        public EasyAssetBundleInfo GetEasyAssetBundleInfoByIndex(int index)
        {
            if(allEasyAssetBundleInfos.Count > index)
            {
                return allEasyAssetBundleInfos[index];
            }
            return null;
        }

        /// <summary>
        /// 获取资源对象信息
        /// </summary>
        /// <param name="assetName">包内资源名称</param>
        /// <param name="index">下标</param>
        /// <returns>该AB包信息对象</returns>
        public EasyAssetBundleInfo GetEasyAssetBundleInfoByAsset(string assetName)
        {
            EasyAssetInfo easyAssetInfo = GetEasyAssetInfoByAsset(assetName);
            if(easyAssetInfo != null)
            {
                return GetEasyAssetBundleInfoByIndex(easyAssetInfo.abIndex);
            }
            return null;
        }

        /// <summary>
        /// 获取所有资源名，通过AB名称
        /// </summary>
        /// <param name="assetName">包内资源名称</param>
        /// <returns>所有资源名列表</returns>
        public EasyAssetInfo GetEasyAssetInfoByAsset(string assetName)
        {
            int assetNameLength = assetName.Length;
            if(allActiveEasyAssetInfoDic.ContainsKey(assetNameLength) && allActiveEasyAssetInfoDic[assetNameLength].TryGetValue(assetName, out EasyAssetInfo easyAssetInfo))
            {
                return easyAssetInfo;
            }
            return null;
        }
        
        /// <summary>
        /// 获取所有资源名，通过AB名称
        /// </summary>
        /// <param name="abName">AB包名称</param>
        /// <returns>所有资源名列表</returns>
        public List<EasyAssetInfo> GetAllEasyAssetInfoByEasyAssetBundleName(string abName)
        {
            int index = allEasyAssetBundleInfos.FindIndex(abInfo => abInfo.abName.Equals(abName));
            return ab2EasyAssetInfoDic[index];
        }

        /// <summary>
        /// 获取所有资源名，通过AB名称
        /// </summary>
        /// <param name="md5">包内资源名称</param>
        /// <returns>所有资源名列表</returns>
        public List<EasyAssetInfo> GetAllEasyAssetInfoByEasyAssetBundleMD5(string md5)
        {
            int index = allEasyAssetBundleInfos.FindIndex(abInfo => abInfo.md5.Equals(md5));
            return ab2EasyAssetInfoDic[index];
        }

        /// <summary>
        /// 获取所有资源名，通过AB名称
        /// </summary>
        /// <param name="assetName">AB包名称</param>
        /// <returns>所有资源名列表</returns>
        public Type GetTypeByAssetName(string assetName)
        {
            var easyAssetInfo = GetEasyAssetInfoByAsset(assetName);
            if(easyAssetInfo != null)
            {
                return GetTypeByAssetTypeIndex(easyAssetInfo.typeIndex);
            }
            return null;
        }

        /// <summary>
        /// 获取所有资源名，通过index
        /// </summary>
        /// <param name="index">index</param>
        /// <returns>所有资源名列表</returns>
        public Type GetTypeByAssetTypeIndex(int index)
        {
            if (index < 0 || index >= assemblyQualifiedNames.Count)
            {
                return null;
            }
            if(!types.ContainsKey(index))
            {
                var type = Type.GetType(assemblyQualifiedNames[index]);
                types.Add(index, type);
            }
            return types[index];
        }

        /// <summary>
        /// 获取资源ab包依赖
        /// </summary>
        /// <param name="assetName">资源名</param>
        /// <returns>所有资源名列表</returns>
        public List<int> GetDependEasyAssetBundleIndexesByAssetName(string assetName)
        {
            var easyAssetInfo = GetEasyAssetInfoByAsset(assetName);
            if(easyAssetInfo != null)
            {
                return easyAssetInfo.needABIndexes;
            }
            return null;
        }
    }

}