using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;

namespace Easy.EasyAsset
{
    /// <summary>
    /// 下载完成失败事件参数
    /// </summary>
    public class DownloadEventArg : EventArg
    {
        public string md5;
    }

    /// <summary>
    /// 下载ab包总控制器
    /// </summary>
    public class DownloadCtrl
    {
        /// <summary>
        /// 需要更新AssetsBundle
        /// </summary>
        private List<Downloader> _allDownloader = new List<Downloader>();

        /// <summary>
        /// 目标所有AB包被的资源信息表
        /// </summary>
        private Catalogs _targetCatalogs;

        /// <summary>
        /// 本地所有AB包被的资源信息表
        /// </summary>
        private Catalogs _localCatalogs;
        /// <summary>
        /// 下载失败
        /// </summary>
        private bool _downloadFail = false;

        /// <summary>
        /// 强更
        /// </summary>
        private bool _foreStoreUpdate = false;

        /// <summary>
        /// 强热更
        /// </summary>
        private bool _forceHotUpdate = false;

        /// <summary>
        /// 是否开启热更新
        /// </summary>
        private bool _hotUpdate = false;

        /// <summary>
        /// 最大下载数量
        /// </summary>
        private int _maxDownloadNum = 5;

        /// <summary>
        /// 资源加载器
        /// </summary>
        private EasyAssetsLoader _assetsLoader;

        /// <summary>
        /// 初始化成功回调
        /// </summary>
        private InitCompleteCallback _initComplete;

        /// <summary>
        /// 重置
        /// </summary>
        public void Reset() 
        {
#if UNITY_EDITOR && !AB_LOAD_BUNDLE

#else
            EventMgr.Instance.UnSubscribeByTarget(this);
#endif            
        }

        /// <summary>
        /// 检测更新
        /// </summary>
        /// <param name="complete"></param>
        public void CheckUpdate(EasyAssetsLoader assetsLoader,InitCompleteCallback complete)
        {
            _assetsLoader = assetsLoader;
            _initComplete = complete;
#if UNITY_EDITOR && !AB_LOAD_BUNDLE
            InitCallback(true);
#else
            EventMgr.Instance.SubscribeByTarget(this);
            EasyAssetConfig abEasyConfig = EasyFrameworkMain.Instance.config.GetEasyConfig<EasyAssetConfig>();
            _hotUpdate = abEasyConfig.hotUpdate;
            _forceHotUpdate = abEasyConfig.forceHotUpdate;
            _maxDownloadNum = abEasyConfig.maxDownloadNum;
            if (!_hotUpdate)
            {
                InitCallback(true);
                return;
            }

            _localCatalogs = FileMgr.Instance.GetTargetClassObject<Catalogs>(Const.localVersionPath, false);
            CheckVersion(abEasyConfig.requestVersionUrl);
#endif
        }

        private void CheckVersion(string requestVersionUrl)
        {
            UnityWebRequest request = UnityWebRequest.Get(requestVersionUrl);
            UnityWebRequestAsyncOperation rsp = request.SendWebRequest();
            rsp.completed += (res) =>
            {
                string targetJsonStr = request.downloadHandler.text;
                if (string.IsNullOrEmpty(targetJsonStr))
                {
                    CheckVersion(requestVersionUrl);
                    return;
                }

                Version abVersion = JsonConvert.DeserializeObject<Version>(targetJsonStr);
                if (_localCatalogs.version >= abVersion.version)
                {
                    CheckUpdateBundles();
                    return;
                }
                
                if(_localCatalogs.versionCode < abVersion.versionCode)
                {
                    _foreStoreUpdate = true;
                    InitCallback(true);
                    return;
                }

                CheckCatalogs(abVersion.requestCatalogsUrl);
                
            };
        }

        private void CheckCatalogs(string requestCatalogsUrl)
        {
            UnityWebRequest request = UnityWebRequest.Get(requestCatalogsUrl);
            UnityWebRequestAsyncOperation rsp = request.SendWebRequest();
            rsp.completed += (res) =>
            {
                //没有拉取到数据，加载失败
                //string inLineJsonStr = FileMgr.Instance.DecryptFileData(request.downloadHandler.text);
                string targetJsonStr = request.downloadHandler.text;
                if (string.IsNullOrEmpty(targetJsonStr))
                {
                    CheckCatalogs(requestCatalogsUrl);
                    return;
                }

                targetJsonStr = XOREncryption.DecryptData(targetJsonStr, XOREncryption.DEFAULT_ENCRYPT_KEY);
                try
                {
                    _targetCatalogs = JsonConvert.DeserializeObject<Catalogs>(targetJsonStr);
                    if(_forceHotUpdate)
                    {
                        _localCatalogs = null;   
                    } 
                    CheckUpdateBundles();
                }
                catch (System.Exception)
                {
                    CheckCatalogs(requestCatalogsUrl);
                }
                
            };
        }


        /// <summary>
        /// 检测要更新的bundle
        /// </summary>
        private void CheckUpdateBundles()
        {
            if(_targetCatalogs != null)
            {
                CheckCatalogsDownload(_targetCatalogs);
            }
            if( _localCatalogs != null)
            {
                CheckCatalogsDownload(_localCatalogs);
            }
            DownloadStart();
        }

        /// <summary>
        /// 通过资源详情表判断要下载的bundle
        /// </summary>
        /// <param name="catalogs"></param>
        private void CheckCatalogsDownload(Catalogs catalogs)
        {
            foreach (EasyAssetBundleInfo abInfo in catalogs.allEasyAssetBundleInfos)
            {
#if UNITY_ANDROID
                if (_assetsLoader.playAssetPackCtrl.IsPlayerAssetContains(abInfo.md5, out PlayAssetPackConfigInfo configInfo))
                {
                    if(_assetsLoader.playAssetPackCtrl.IsAssetBundleDownloaded(abInfo.md5))
                    {
                        continue;
                    }

                    bool have = false;
                    foreach (Downloader abDownloader in _allDownloader)
                    {
                        if (abDownloader.fileName == configInfo.assetPackName)
                        {
                            if (abDownloader.version > catalogs.version)
                            {
                                abDownloader.version = catalogs.version;
                            }
                            have = true;
                            break;
                        }
                    }
                    if (!have)
                    {
                        Downloader abDownloader = new PlayAssetDeliveryAssetPackDownloader(configInfo.assetPackName, configInfo.size, abInfo.abDownloadPriority, catalogs.version, Const.localAssetBundleFolder, UpdateBundleCallBack);
                        _allDownloader.Add(abDownloader);
                    }

                }
                else
#endif
                {
                    FileInfo fileInfo = new FileInfo(Const.localAssetBundleFolder + abInfo.md5);
                    if (fileInfo.Exists && fileInfo.Length == abInfo.size)
                    {
                        string md5 = MD5Utility.GetFileMd5Hash(Const.localAssetBundleFolder + abInfo.md5);
                        if (abInfo.md5 == md5)
                        {
                            continue;
                        }
                    }

                    bool have = false;
                    foreach (Downloader abDownloader in _allDownloader)
                    {
                        if (abDownloader.fileName == abInfo.md5)
                        {
                            if (abDownloader.version > catalogs.version)
                            {
                                abDownloader.version = catalogs.version;
                            }
                            have = true;
                            break;
                        }
                    }
                    if (!have)
                    {
#if !UNITY_WEBGL
                        Downloader abDownloader = new HttpDownloader(abInfo.md5, abInfo.size, abInfo.abDownloadPriority, catalogs.version, Const.localAssetBundleFolder, UpdateBundleCallBack, catalogs.updateUrls);
#else
                        Downloader abDownloader = new WebHttpDownloader(abInfo.md5, abInfo.size, abInfo.abDownloadPriority, catalogs.version, Const.localAssetBundleFolder, UpdateBundleCallBack, catalogs.updateUrls);
#endif
                        _allDownloader.Add(abDownloader);
                    }
                }
            }
        }

        /// <summary>
        /// 更新完bundle的回调
        /// </summary>
        /// <param name="code"></param>
        /// <param name="fileName"></param>
        private void UpdateBundleCallBack(DownloadCode code, string fileName) 
        {
            DownloadEventArg arg = EventMgr.Instance.GetEventArg<DownloadEventArg>();
            arg.md5 = fileName;
            if(code == DownloadCode.Success)
            {
                EventMgr.Instance.DispatchEvent(Const.DOWNLOAD_SUCCESS, arg);
            }
            else
            {
                EventMgr.Instance.DispatchEvent(Const.DOWNLOAD_FAIL, arg);
            }
        }

        /// <summary>
        /// 下载开始,监听其他下载是否完成后,再开始下载
        /// </summary>
        /// <param name="eventArg"></param>
        [BindEvent(new string[] {Const.DOWNLOAD_SUCCESS, Const.DOWNLOAD_FAIL})]
        private void DownloadStart(EventArg eventArg) 
        {
            DownloadStart();
        }

        /// <summary>
        /// 下载开始
        /// </summary>
        private void DownloadStart() 
        {
            long needSize = GetNeedSize();
            long remainSize = Tools.GetFreeDiskSpace();
            if(remainSize < needSize)
            {
                DownloadFail();
                return;
            }

            int nowInDownload = 0;
            foreach (Downloader abDownloader in _allDownloader)
            {
                if(abDownloader.downloadType == DownloadType.Start || abDownloader.downloadType == DownloadType.InProgress) 
                {
                    ++nowInDownload;
                }
            }

            int remain = _maxDownloadNum - nowInDownload;
            if(remain > 0) 
            {
                foreach (Downloader abDownloader in _allDownloader)
                {
                    if(abDownloader.downloadType == DownloadType.NoStart || abDownloader.downloadType == DownloadType.Fail) 
                    {
                        --remain;
                        abDownloader.Start();
                    }
                    if(remain == 0) 
                    {
                        break;
                    }
                }
            }

            if(_localCatalogs != null && MustDownloadCompleted(_localCatalogs.version)) 
            {
                LocalMustDownloadSuccess();
            }
            
            if(_targetCatalogs != null && MustDownloadCompleted(_targetCatalogs.version)) 
            {
                TargetMustDownloadSuccess();
            }

            if(_targetCatalogs == null && _localCatalogs != null && DownloadCompleted(_localCatalogs.version))
            {
                DeleteInvalidFile();
            }

        }

        public void InitCallback(bool value)
        {
            if(_initComplete != null)
            {
                var tmp = _initComplete;
                _initComplete = null;
                tmp.Invoke(value);
            }
        }

        /// <summary>
        /// 本地资源列表里,必须下载完的部分下载完成
        /// </summary>
        public void LocalMustDownloadSuccess()
        {
            if(!_forceHotUpdate || _targetCatalogs == null)
            {
                InitCallback(true);
            }
        }

        /// <summary>
        /// 目标资源列表里,必须下载完的部分下载完成
        /// </summary>
        public void TargetMustDownloadSuccess()
        {
            if(_forceHotUpdate)
            {
                ChangeCatalogs();
                InitCallback(true);
            }
        }

        /// <summary>
        /// 下载失败
        /// </summary>
        public void DownloadFail()
        {
            _downloadFail = true;
            foreach (Downloader abDownloader in _allDownloader)
            {
                abDownloader.Dispose(true);
            }
            InitCallback(false);
        }

        /// <summary>
        /// 删除无效文件
        /// </summary>
        public void DeleteInvalidFile()
        {
            List<string> files = Directory.GetFiles(Const.localAssetBundleFolder).ToList();
            for(int i = 0; i < files.Count; i++)
            {
                files[i] = Path.GetFileName(files[i]);
            }
            foreach (var abInfo in _localCatalogs.allEasyAssetBundleInfos)
            {
                int index = files.IndexOf(abInfo.md5);
                if(index != -1)
                {
                    files.RemoveAt(index);
                }
            }
        }

        /// <summary>
        /// 目标资源列表文件覆盖本地资源列表文件
        /// </summary>
        private void ChangeCatalogs()
        {
            if(_targetCatalogs != null)
            {
                FileMgr.Instance.SetTargetClassObjectToFile<Catalogs>(Const.localVersionPath, _targetCatalogs);
            }
        }

        /// <summary>
        /// 是否需要强更
        /// </summary>
        /// <returns></returns>
        public bool NeedForeUpdate()
        {
            return _foreStoreUpdate;
        }

        /// <summary>
        /// 是否下载失败
        /// </summary>
        /// <returns></returns>
        public bool IsDownLoadFail()
        {
            return _downloadFail;
        }

        /// <summary>
        /// 开始下载ab包
        /// </summary>
        /// <param name="md5"></param>
        public void StartDownload(string md5)
        {
            if(_allDownloader.Count == 0)
            {
                return;
            }
            
            foreach (Downloader downloader in _allDownloader)
            {
                if(downloader.fileName == md5 && (downloader.downloadType == DownloadType.NoStart || downloader.downloadType == DownloadType.Fail )) 
                {
                    if(downloader.downloadPriority == DownloadPriority.Low)
                    {
                        downloader.downloadPriority = DownloadPriority.High;
                    }
                    downloader.downloadWeight += 1;
                }
            }

            DownloadStart();
        }

        /// <summary>
        /// 下载完成
        /// </summary>
        /// <param name="version"></param>
        /// <returns></returns>
        public bool DownloadCompleted(long version) 
        {
            return !_allDownloader.Exists(downloader => downloader.version == version && downloader.downloadType != DownloadType.Complete);
        }

        /// <summary>
        /// 必须下载的资源下载完成
        /// </summary>
        /// <param name="version"></param>
        /// <returns></returns>
        public bool MustDownloadCompleted(long version) 
        {
            return !_allDownloader.Exists(downloader => downloader.version == version && downloader.downloadPriority == DownloadPriority.Must && downloader.downloadType != DownloadType.Complete);
        }

        /// <summary>
        /// 下载进度
        /// </summary>
        /// <returns></returns>
        public float Progress() 
        {
            GetSizeInfo(out long downloadedSize,out long totalSize);
            return downloadedSize * 1.0f / totalSize;
        }

        /// <summary>
        /// 还需要的空间大小
        /// </summary>
        /// <returns></returns>
        public long GetNeedSize()
        {
            GetSizeInfo(out long downloadedSize,out long totalSize);
            return totalSize - downloadedSize;
        }

        /// <summary>
        /// 已下载的空间大小和总共需要的工具大小
        /// </summary>
        /// <param name="downloadedSize"></param>
        /// <param name="totalSize"></param>
        public void GetSizeInfo(out long downloadedSize, out long totalSize)
        {
            downloadedSize = 0;
            totalSize = 0;
            foreach (Downloader downloader in _allDownloader)
            {
                downloadedSize += downloader.downloadType == DownloadType.Complete?  downloader.size : downloader.currentSize;
                totalSize += downloader.size;
            }
        }
    }
}