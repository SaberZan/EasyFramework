using System;
using System.Net;
using System.Threading;
using System.IO;
using System.Collections.Generic;

namespace Easy.EasyAsset
{
    public enum DownloadCode
    {
        None,
        Success,
        DownloadFail,           //下载失败
        DownloadContentEmpty,    // 需要下载的文件内容为空
        TempFileMissing,            // 临时文件丢失
        FinalFail,            // 最终失败
    }

    public abstract class Downloader : IComparable<Downloader>
    {
        /// <summary>
        /// AB包名 md5码
        /// </summary>
        public string fileName;
        /// <summary>
        /// AB包大小
        /// </summary>
        public long size;
        /// <summary>
        /// 属于哪个版本
        /// </summary>
        public long version;
        /// <summary>
        /// 下载标志
        /// </summary>
        public DownloadType downloadType;
        /// <summary>
        /// 临时保存的文件夹
        /// </summary>
        public string saveDirPath;
        /// <summary>
        /// 是否必须下载
        /// </summary>
        public DownloadPriority downloadPriority;
        /// <summary>
        /// 下载权重值
        /// </summary>
        public int downloadWeight;
        /// <summary>
        /// 当前AB包下载的大小
        /// </summary>
        public long currentSize;
        /// <summary>
        /// 保存位置
        /// </summary>
        public string ABFilePath => saveDirPath + fileName;
        /// <summary>
        /// 完成回调
        /// </summary>
        public Action<DownloadCode, string> _callback = null;
        /// <summary>
        /// 下载失败计数
        /// </summary>
        private int _downloadErrorCount = 0;

        public Downloader(string fileName, long size, DownloadPriority downloadPriority, long version, string saveDirPath, Action<DownloadCode, string> callback)
        {
            this.fileName = fileName;
            this.size = size;
            this.version = version;
            this.downloadPriority = downloadPriority;
            this.saveDirPath = saveDirPath;
            downloadType = DownloadType.NoStart;
            _callback = callback;
        }

        /// 
        /// 开始下载
        /// 
        /// 超时时间（秒）
        public virtual void Start(int timeout = 10)
        {
            downloadType = DownloadType.Start;
        }
        
        /// 
        /// 停止下载
        /// 
        /// 超时时间（秒）
        public virtual void Dispose(bool force = false)
        {

        }


        /// <summary>
        /// 优先级比较
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(Downloader other)
        {
            if(version == other.version)
            {
                if(downloadPriority == other.downloadPriority)
                {
                    return other.downloadWeight - downloadWeight;
                }
                return downloadPriority - other.downloadPriority;
            }
            else
            {
                return version < other.version ? -1 : 1;
            }
        }
    }
}