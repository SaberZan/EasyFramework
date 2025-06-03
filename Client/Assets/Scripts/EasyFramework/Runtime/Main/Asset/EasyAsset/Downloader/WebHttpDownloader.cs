using System;
using System.Net;
using System.Threading;
using System.IO;
using System.Collections.Generic;


namespace Easy.EasyAsset
{
    public class WebHttpDownloader : Downloader
    {
        /// <summary>
        /// 临时保存位置
        /// </summary>
        public string TempFilePath => saveDirPath + fileName + ".temp";
        /// <summary>
        /// 文件流，用来将接收到的数据写入文件
        /// </summary>
        private FileStream _fileStream = null; 
        /// <summary>
        /// 下载请求
        /// </summary>
        public HttpWebRequest httpWebRequest = null;
        /// <summary>
        /// 下载请求
        /// </summary>
        public HttpWebResponse httpWebResponse = null;
        /// <summary>
        /// 错误
        /// </summary>
        public WebExceptionStatus status = WebExceptionStatus.UnknownError;
        /// <summary>
        /// 错误
        /// </summary>
        public DownloadCode code = DownloadCode.None;
        /// <summary>
        /// 当前已经下载的数据长度
        /// </summary>
        private long _currLength = 0;
        /// <summary>
        /// 文件总数据长度
        /// </summary>
        private long _totalLength = 0;
        /// <summary>
        /// 本次需要下载的数据长度
        /// </summary>
        private long _contentLength = 0;
        /// <summary>
        /// 下载失败计数
        /// </summary>
        private int _downloadErrorCount = 0;
        /// <summary>
        /// 从哪个下载路径下载
        /// </summary>
        private int _updateUrlsIndex = 0;
        /// <summary>
        /// 下载的路径
        /// </summary>
        public List<string> updateUrls;
        /// <summary>
        /// 下载线程中断
        /// </summary>
        public EasyCancellationToken token;

        public WebHttpDownloader(string fileName, long size, DownloadPriority downloadPriority, long version, string saveDirPath, Action<DownloadCode, string> callback, List<string> urls) :base(fileName, size, downloadPriority, version, saveDirPath, callback)
        {
            updateUrls = urls;
        }

        /// 
        /// 开始下载
        /// 
        /// 超时时间（秒）
        public override void Start(int timeout = 10)
        {
            downloadType = DownloadType.Start;
            token = new EasyCancellationToken();
            StartDownload(timeout, token);
        }

        /// <summary>
        /// 线程开始下载
        /// </summary>
        /// <param name="timeout"></param>
        public async void StartDownload(object timeout, EasyCancellationToken  cancellationToken)
        {

            try
            {
                string path = TempFilePath;
                FileInfo fileInfo = new FileInfo(path);
                if (fileInfo.Exists && fileInfo.Length == size)
                {
                    MoveFile();
                }
                else
                {
                    if (fileInfo.Exists && fileInfo.Length > size)
                    {
                        fileInfo.Delete();
                    }

                    _fileStream = new FileStream(TempFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                    _currLength = _fileStream.Length;
                    _fileStream.Position = _currLength;
                    currentSize = _currLength;

                    HttpWebRequest httpWebRequest = HttpWebRequest.CreateHttp(updateUrls[_updateUrlsIndex] + fileName);
                    httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                    httpWebRequest.Method = "Get";
                    httpWebRequest.Timeout = (int)timeout;
                    httpWebRequest.AddRange(_currLength);

                    _contentLength = httpWebResponse.ContentLength;
                    if (_contentLength <= 0)
                    {
                        code = DownloadCode.DownloadContentEmpty;
                    }

                    _totalLength = this._contentLength + _currLength;

                    Stream stream = httpWebResponse.GetResponseStream();
                    byte[] bytes = new byte[1024];
                    long downloadedByte = 0;
                    while (downloadedByte < _contentLength)
                    {
                        if(cancellationToken.IsCanceled)
                        {
                            break;
                        }

                        int size = stream.Read(bytes, 0, (int)bytes.Length);
                        downloadedByte += size;
                        
                        _fileStream.Write(bytes, 0, size);
                        _currLength += size;
                        currentSize = _currLength;
                        await EasyTaskRunner.Yeild(cancellationToken : cancellationToken);
                    }

                    if(_fileStream != null)
                    {
                        _fileStream.Close();
                        _fileStream.Dispose();
                        _fileStream = null;
                    }
                    if(currentSize == _currLength)
                    {
                        MoveFile();
                    }
                }
            }
            catch(WebException ex)
            {
                status = ex.Status;
                code = DownloadCode.DownloadFail;
            }
            finally
            {
                Dispose();
            }
        }

        /// 
        /// 清理
        /// 
        public void MoveFile()
        {
            // 如果下载完成后，临时文件如果被意外删除了，也抛出错误提示
            if (!File.Exists(TempFilePath))
            {
                code = DownloadCode.TempFileMissing;
                return;
            }
            // 如果下载的文件已经存在，就删除原文件
            if (File.Exists(ABFilePath))
            {
                File.Delete(ABFilePath);
            }

            if(fileName != MD5Utility.GetFileMd5Hash(TempFilePath))
            {
                File.Delete(TempFilePath);
                code = DownloadCode.DownloadFail;
            }
            else
            {
                File.Move(TempFilePath, ABFilePath);
                code = DownloadCode.Success;
            }
        }

        /// 
        /// 清理
        /// 
        public override void Dispose(bool force = false)
        {
            if(_fileStream != null)
            {
                _fileStream.Close();
                _fileStream.Dispose();
                _fileStream = null;
            }
            if (httpWebRequest!=null)
            {
                // 如果下载没有完成，就中止
                httpWebResponse.Dispose();
                httpWebResponse.Close();
                httpWebResponse = null;
                httpWebRequest.Abort();
                httpWebRequest = null;
            }

            if(!force)
            {
                OnFinal();
            }

            token?.Cancel();
            token = null;
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
                if (code == DownloadCode.DownloadContentEmpty || code == DownloadCode.DownloadFail)
                {
                    if (status == WebExceptionStatus.ConnectFailure)
                    {
                        ++_updateUrlsIndex;
                    }
                    else if (status == WebExceptionStatus.ProtocolError)
                    {
                        ++_updateUrlsIndex;
                    }
                    ++_downloadErrorCount;
                    if(_downloadErrorCount > 100)
                    {
                        code = DownloadCode.FinalFail;
                    }
                }
            }
            _callback?.Invoke(code, fileName);
        }
    }
}