using System;
using System.IO;
using UnityEngine;

namespace Easy
{
    /// <summary>
    /// 安卓文件系统流。
    /// </summary>
    public class AndroidFileStream : Stream
    {
        private static readonly string SplitFlag = "!/assets/";
        private static readonly int SplitFlagLength = SplitFlag.Length;
        private static readonly AndroidJavaObject _AssetManager = null;
        private static readonly IntPtr _InternalReadMethodId = IntPtr.Zero;
        private static readonly jvalue[] _InternalReadArgs = null;

        private readonly AndroidJavaObject m_FileStream;
        private readonly IntPtr m_FileStreamRawObject;

        static AndroidFileStream()
        {
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            if (unityPlayer == null)
            {
                throw new Exception("Unity player is invalid.");
            }

            AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            if (currentActivity == null)
            {
                throw new Exception("Current activity is invalid.");
            }

            AndroidJavaObject assetManager = currentActivity.Call<AndroidJavaObject>("getAssets");
            if (assetManager == null)
            {
                throw new Exception("Asset manager is invalid.");
            }

            _AssetManager = assetManager;

            IntPtr inputStreamClassPtr = AndroidJNI.FindClass("java/io/InputStream");
            _InternalReadMethodId = AndroidJNIHelper.GetMethodID(inputStreamClassPtr, "read", "([BII)I");
            _InternalReadArgs = new jvalue[3];

            AndroidJNI.DeleteLocalRef(inputStreamClassPtr);
            currentActivity.Dispose();
            unityPlayer.Dispose();
        }

        /// <summary>
        /// 初始化安卓文件系统流的新实例。
        /// </summary>
        /// <param name="fullPath">要加载的文件系统的完整路径。</param>
        public AndroidFileStream(string fullPath)
        {
            if (string.IsNullOrEmpty(fullPath))
            {
                throw new Exception("Full path is invalid.");
            }

            int position = fullPath.LastIndexOf(SplitFlag, StringComparison.Ordinal);
            if (position < 0)
            {
                throw new Exception("Can not find split flag in full path.");
            }

            string fileName = fullPath.Substring(position + SplitFlagLength);
            m_FileStream = InternalOpen(fileName);
            if (m_FileStream == null)
            {
                throw new Exception($"Open file '{fullPath}' from Android asset manager failure.");
            }

            m_FileStreamRawObject = m_FileStream.GetRawObject();
        }

        /// <summary>
        /// 获取或设置文件系统流位置。
        /// </summary>
        public override long Position
        {
            get
            {
                throw new Exception("Get position is not supported in AndroidFileStream.");
            }
            set
            {
                Seek(value, SeekOrigin.Begin);
            }
        }

        /// <summary>
        /// 获取文件系统流长度。
        /// </summary>
        public override long Length
        {
            get
            {
                return InternalAvailable();
            }
        }

        /// <summary>
        /// 是否可读取
        /// </summary>
        public override bool CanRead => true;

        /// <summary>
        /// 是否可查询
        /// </summary>
        public override bool CanSeek => true;

        /// <summary>
        /// 是否可写
        /// </summary>
        public override bool CanWrite => false;

        /// <summary>
        /// 设置文件系统流长度。
        /// </summary>
        /// <param name="length">要设置的文件系统流的长度。</param>
        public override void SetLength(long length)
        {
            throw new Exception("SetLength is not supported in AndroidFileStream.");
        }

        /// <summary>
        /// 定位文件系统流位置。
        /// </summary>
        /// <param name="offset">要定位的文件系统流位置的偏移。</param>
        /// <param name="origin">要定位的文件系统流位置的方式。</param>
        public override long Seek(long offset, SeekOrigin origin)
        {
            if (origin == SeekOrigin.End)
            {
                return Seek(Length + offset, SeekOrigin.Begin);
            }

            if (origin == SeekOrigin.Begin)
            {
                InternalReset();
            }

            while (offset > 0)
            {
                long skip = InternalSkip(offset);
                if (skip < 0)
                {
                    return 0L;
                }

                offset -= skip;
            }
            return offset;
        }

        /// <summary>
        /// 从文件系统流中读取一个字节。
        /// </summary>
        /// <returns>读取的字节，若已经到达文件结尾，则返回 -1。</returns>
        public override int ReadByte()
        {
            return InternalRead();
        }

        /// <summary>
        /// 从文件系统流中读取二进制流。
        /// </summary>
        /// <param name="buffer">存储读取文件内容的二进制流。</param>
        /// <param name="startIndex">存储读取文件内容的二进制流的起始位置。</param>
        /// <param name="length">存储读取文件内容的二进制流的长度。</param>
        /// <returns>实际读取了多少字节。</returns>
        public override int Read(byte[] buffer, int startIndex, int length)
        {
            byte[] result = null;
            int bytesRead = InternalRead(length, out result);
            Array.Copy(result, 0, buffer, startIndex, bytesRead);
            return bytesRead;
        }

        /// <summary>
        /// 向文件系统流中写入一个字节。
        /// </summary>
        /// <param name="value">要写入的字节。</param>
        public override void WriteByte(byte value)
        {
            throw new Exception("WriteByte is not supported in AndroidFileStream.");
        }

        /// <summary>
        /// 向文件系统流中写入二进制流。
        /// </summary>
        /// <param name="buffer">存储写入文件内容的二进制流。</param>
        /// <param name="startIndex">存储写入文件内容的二进制流的起始位置。</param>
        /// <param name="length">存储写入文件内容的二进制流的长度。</param>
        public override void Write(byte[] buffer, int startIndex, int length)
        {
            throw new Exception("Write is not supported in AndroidFileStream.");
        }

        /// <summary>
        /// 将文件系统流立刻更新到存储介质中。
        /// </summary>
        public override void Flush()
        {
            throw new Exception("Flush is not supported in AndroidFileStream.");
        }

        /// <summary>
        /// 关闭文件系统流。
        /// </summary>
        public override void Close()
        {
            InternalClose();
            m_FileStream.Dispose();
        }

        private AndroidJavaObject InternalOpen(string fileName)
        {
            return _AssetManager.Call<AndroidJavaObject>("open", fileName);
        }

        private int InternalAvailable()
        {
            return m_FileStream.Call<int>("available");
        }

        private void InternalClose()
        {
            m_FileStream.Call("close");
        }

        private int InternalRead()
        {
            return m_FileStream.Call<int>("read");
        }

        private int InternalRead(int length, out byte[] result)
        {
#if UNITY_2019_2_OR_NEWER
#pragma warning disable CS0618
#endif
            IntPtr resultPtr = AndroidJNI.NewByteArray(length);
#if UNITY_2019_2_OR_NEWER
#pragma warning restore CS0618
#endif
            int offset = 0;
            int bytesLeft = length;
            while (bytesLeft > 0)
            {
                _InternalReadArgs[0] = new jvalue() { l = resultPtr };
                _InternalReadArgs[1] = new jvalue() { i = offset };
                _InternalReadArgs[2] = new jvalue() { i = bytesLeft };
                int bytesRead = AndroidJNI.CallIntMethod(m_FileStreamRawObject, _InternalReadMethodId, _InternalReadArgs);
                if (bytesRead <= 0)
                {
                    break;
                }

                offset += bytesRead;
                bytesLeft -= bytesRead;
            }

#if UNITY_2019_2_OR_NEWER
#pragma warning disable CS0618
#endif
            result = AndroidJNI.FromByteArray(resultPtr);
#if UNITY_2019_2_OR_NEWER
#pragma warning restore CS0618
#endif
            AndroidJNI.DeleteLocalRef(resultPtr);
            return offset;
        }

        private void InternalReset()
        {
            m_FileStream.Call("reset");
        }

        private long InternalSkip(long offset)
        {
            return m_FileStream.Call<long>("skip", offset);
        }
    }
}