#if UNITY_ANDROID

using System;
using System.Net;
using System.Threading;
using System.IO;
using System.Collections.Generic;
using Google.Play.AssetDelivery;

namespace Easy.EasyAsset
{
    public class PlayAssetDeliveryAssetPackDownloader : Downloader
    {
        /// <summary>
        /// 下载请求
        /// </summary>
        private PlayAssetPackRequest _request;

        /// <summary>
        /// 错误
        /// </summary>
        public DownloadCode code = DownloadCode.None;

        public PlayAssetDeliveryAssetPackDownloader(string fileName, long size, DownloadPriority downloadPriority, long abVersion, string saveDirPath, Action<DownloadCode, string> callback) :base(fileName, size, downloadPriority, abVersion, saveDirPath, callback)
        {

        }

        /// 
        /// 开始下载
        /// 
        /// 超时时间（秒）
        public override void Start(int timeout = 10)
        {
            StartDownload(timeout);
        }

        /// <summary>
        /// 线程开始下载
        /// </summary>
        /// <param name="timeout"></param>
        public async void StartDownload(object timeout)
        {
            _request = PlayAssetDelivery.RetrieveAssetPackAsync(fileName, true);
            while(!_request.IsDone)
            {
                if (_request.Status == AssetDeliveryStatus.WaitingForWifi)
                {
                    var asyncOperation = PlayAssetDelivery.ShowCellularDataConfirmation();
                    while(!asyncOperation.IsDone)
                    {
                        await UniTask.Yield();
                    }

                    if (asyncOperation.Error != AssetDeliveryErrorCode.NoError || asyncOperation.GetResult() != ConfirmationDialogResult.Accepted)
                    {
                        // 错误或者用户取消
                        code = DownloadCode.DownloadFail;
                        OnFinal();
                        return;
                    }
                    else
                    {
                        while (_request.Status == AssetDeliveryStatus.WaitingForWifi)
                        {
                            await UniTask.Yield();
                        }
                    }
                }
                currentSize = (long)(size * _request.DownloadProgress);
                await UniTask.Yield();
            }

            if (_request.Error != AssetDeliveryErrorCode.NoError)
            {
                code = DownloadCode.DownloadFail;
                _request = null;
            }
            else
            {
                currentSize = size;
                code = DownloadCode.Success;
            }
            OnFinal();
        }

        /// 
        /// 清理
        /// 
        public override void Dispose(bool force = false)
        {
            if (_request == null)
            {
                return;
            }

            _request.AttemptCancel();
        }

        /// <summary>
        /// 最终结果判断
        /// </summary>
        public void OnFinal()
        {
            if(code == DownloadCode.Success)
            {
                downloadType = DownloadType.Complete;
            }
            else
            {
                downloadType = DownloadType.Fail;
            }
            _callback?.Invoke(code, fileName);
        }
    }
}

#endif