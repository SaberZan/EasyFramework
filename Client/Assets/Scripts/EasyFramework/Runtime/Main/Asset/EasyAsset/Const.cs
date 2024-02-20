using UnityEngine;

namespace Easy.EasyAsset
{
    public class Const
    {

        /// <summary>
        /// 原始版本存放路径
        /// </summary>
        public static string originalVersionPath = Application.streamingAssetsPath + "/AssetBundles/catalogs.txt";

        /// <summary>
        /// PAD存放路径
        /// </summary>
        public static string originalPlayAssetPackPath = Application.streamingAssetsPath + "/AssetBundles/playAssetPackConfig.txt";
        
        /// <summary>
        /// 本地版本存放路径 
        /// </summary>
        public static string localVersionPath = Application.persistentDataPath + "/AssetBundles/catalogs.txt";

        /// <summary>
        /// 原始ab包存放路径
        /// </summary>
        public static string originAssetBundleFolder = Application.streamingAssetsPath + "/AssetBundles/";

        /// <summary>
        /// 本地ab包存放路径
        /// </summary>
        public static string localAssetBundleFolder = Application.persistentDataPath + "/AssetBundles/";
        /// <summary>
        /// DLC 下载完成
        /// </summary>
        public const string DOWNLOAD_SUCCESS = "AB_DOWNLOAD_SUCCESS";

        /// <summary>
        /// DLC 下载失败
        /// </summary>
        public const string DOWNLOAD_FAIL = "AB_DOWNLOAD_FAIL";
    }

    /// <summary>
    /// ab包下载优先级
    /// </summary>
    public enum DownloadPriority
    {
        //必须被下载
        Must,
        //优先被下载
        High,
        //调用了才被下载
        Low
    }

    /// <summary>
    /// ab包下载中的情况
    /// </summary>
    public enum DownloadType
    {
        //下载未开始
        NoStart,
        //开始下载
        Start,
        //下载过程中
        InProgress,
        //下载完成
        Complete,
        //下载失败
        Fail,
    }

    /// <summary>
    /// 包所在位置
    /// </summary>
    public enum Location
    {
        InStreamAsset,
        InServer,
    }

    /// <summary>
    /// ab包类型
    /// </summary>
    public enum EasyAssetBundleType
    {
        UnityAssetBundle,
        RawAssetBundle,
    }
}