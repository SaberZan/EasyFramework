using System;
using System.Text;

namespace Easy
{
    //简单的二进制位异或处理，加密与解密使用相同的接口与参数
    public class XOREncryption
    {
        /// <summary>
        /// 是否加密
        /// </summary>

        public static bool IsEncrypt 
        {
            get 
            {
#if ENCRYPT
                return true;
#else
                return false;
#endif
            }
        }

        
        /// <summary>
        /// 默认加密密钥 256bit keys
        /// </summary>
        public static readonly byte[] DEFAULT_ENVRYPT_BYTES = new byte[]
        {
            0x51,0x4e,0x8c,0x39,0x69,0x99,0x86,0x3b,
            0x76,0xca,0x43,0xa5,0x62,0xa2,0xef,0xee,
            0x0c,0x94,0xdd,0xed,0x5c,0x17,0x21,0xfe,
            0x22,0xf8,0x6c,0x87,0x3f,0xda,0x2e,0xd3,
            0xfa,0x3b,0x85,0x53,0x7f,0xf3,0xc1,0x22,
            0x45,0xdd,0xd8,0xa7,0x3e,0xd7,0xaa,0xe5,
            0x1c,0xdd,0x29,0x21,0x5f,0xa3,0xba,0xb1,
            0xb5,0x02,0x59,0xaa,0x2a,0xc6,0x71,0xb6,
            0xf4,0x96,0x66,0x9c,0xf4,0xe4,0xc8,0x01,
            0xbe,0xe6,0x69,0x66,0x00,0x4a,0xc3,0x27,
            0x0b,0x70,0x12,0x2d,0x50,0xe4,0x10,0x12,
            0x23,0x4a,0x5c,0x2b,0x47,0x73,0x78,0xbb,
            0xe6,0x86,0x80,0x68,0x37,0xe7,0xca,0xb9,
            0xb2,0xe0,0x82,0xc0,0x90,0x4b,0x3b,0x2a,
            0x68,0x8f,0xeb,0xba,0x60,0x3e,0x0f,0xf6,
            0x79,0xc4,0x64,0x1c,0x0e,0xd9,0x37,0xca,
            0xf7,0x5c,0xf2,0x6d,0x45,0x43,0xf4,0x11,
            0x18,0x06,0x08,0x3a,0x1d,0x77,0x27,0xb8,
            0xb7,0xb2,0xdb,0x22,0x26,0x59,0xa8,0x3f,
            0xd0,0x6d,0xee,0x8b,0xbb,0x78,0xa1,0xfe,
            0x56,0xb2,0x6e,0x85,0x08,0x54,0x86,0x80,
            0x06,0xf9,0x3d,0x71,0xb0,0x33,0x75,0x87,
            0x7e,0xb1,0x3b,0x91,0xe6,0xf7,0xe6,0xc6,
            0xcc,0xba,0xc9,0xa9,0xab,0x67,0xed,0x87,
            0x73,0x6b,0xed,0x24,0x6d,0x2f,0xb7,0x59,
            0x0c,0xc9,0x73,0xbb,0xa9,0xdf,0x26,0x97,
            0x9c,0x86,0x23,0x1e,0xa2,0x3b,0xc5,0xb7,
            0xe6,0xb7,0x88,0x14,0x8e,0xc5,0x80,0x92,
            0x2f,0x80,0x35,0x18,0xa3,0x9f,0x35,0xd6,
            0xa3,0xbb,0x14,0x34,0xe0,0x95,0x7b,0xdd,
            0x56,0x2f,0x4f,0x64,0x35,0x28,0x2e,0xf3,
            0xe5,0x4b,0xf6,0x5f,0xc4,0xd3,0xd8,0xb4
        };
        
        /// <summary>
        /// 默认加密密钥 字符串
        /// </summary>
        public const string DEFAULT_ENCRYPT_KEY = "sXMPOubp6uD3Ulz3BBXQA7ZZ1zSiwIqzoV0kTjpMEiClw2QOR6T229OYrIg59qPP";


        /// <summary>
        /// 简单的加解密
        /// </summary>
        /// <param name="data"></param>
        /// <param name="len"></param>
        /// <param name="key"></param>
        public static void Crypto(byte[] data, int len, int key)
        {
            int k = data.Length + key;
            byte a = (byte)((k >> 0) & 0xFF);
            byte b = (byte)((k >> 8) & 0xFF);
            byte c = (byte)((k >> 16) & 0xFF);
            byte d = (byte)((k >> 24) & 0xFF);
            len -= len % 4;
            for (int j = 0; j < len; j += 4)
            {
                data[j + 0] = (byte)(((a + j) ^ data[j + 0] ^ b) & 0xFF);
                data[j + 1] = (byte)(((b + j) ^ data[j + 1] ^ c) & 0xFF);
                data[j + 2] = (byte)(((c + j) ^ data[j + 2] ^ a) & 0xFF);
                data[j + 3] = (byte)(((d + j) ^ data[j + 3] ^ d) & 0xFF);
            }
        }

        /// <summary>
        /// 解密文件内容（字符串）
        /// </summary>
        /// <param name="contentStr">待解密的文件内容字符串</param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string DecryptData(string contentStr, string key)
        {
            if (!IsEncrypt)
                return contentStr;
            string decryptStr = EncryptData(contentStr, key);
            return decryptStr;
        }

        /// <summary>
        /// 解密文件内容（字符串）
        /// </summary>
        /// <param name="contentStr">待解密的文件内容字符串</param>
        /// <param name="encryptBytes"></param>
        /// <returns></returns>
        public static string DecryptData(string contentStr, byte[] encryptBytes)
        {
            if (!IsEncrypt)
                return contentStr;
            string decryptStr = EncryptData(contentStr, encryptBytes);
            return decryptStr;
        }

        /// <summary>
        /// 解密文件内容（字节数组）
        /// </summary>
        /// <param name="buffer">待解密的文件内容字节数组</param>
        /// <param name="offset">从encryptKey的哪个位置开始</param>
        /// <param name="len">长度</param>
        /// <param name="keyBytes"></param>
        /// <param name="keyOffset">加密字偏移量</param>
        /// <returns></returns>
        public static void DecryptData(byte[] buffer, long offset, long len, string key, long keyOffset)
        {
            if (!IsEncrypt)
                return;
            EncryptData(buffer, offset, len, key, keyOffset);
        }

        /// <summary>
        /// 解密文件内容（字节数组）
        /// </summary>
        /// <param name="buffer">待解密的文件内容字节数组</param>
        /// <param name="offset">从encryptKey的哪个位置开始</param>
        /// <param name="len">长度</param>
        /// <param name="keyBytes"></param>
        /// <param name="keyOffset">加密字偏移量</param>
        /// <returns></returns>
        public static void DecryptData(byte[] buffer, long offset, long len, byte[] keyBytes, long keyOffset)
        {
            if (!IsEncrypt)
                return;
            EncryptData(buffer, offset, len, keyBytes, keyOffset);
        }

        /// <summary>
        /// 加密文件内容(字符串)
        /// </summary>
        /// <param name="contentStr">文件内容字符串</param>
        /// <param name="key"></param>
        /// <returns>加密后的内容</returns>
        public static string EncryptData(string contentStr, string key)
        {
            if (!IsEncrypt)
                return contentStr;
            byte[] buffer = Encoding.UTF8.GetBytes(contentStr);
            EncryptData(buffer, 0, -1, key, buffer.Length);
            return Encoding.UTF8.GetString(buffer);
        }

        /// <summary>
        /// 加密文件内容(字符串)
        /// </summary>
        /// <param name="contentStr">文件内容字符串</param>
        /// <param name="keyBytes"></param>
        /// <returns>加密后的内容</returns>
        public static string EncryptData(string contentStr, byte[] keyBytes)
        {
            if (!IsEncrypt)
                return contentStr;
            byte[] buffer = Encoding.UTF8.GetBytes(contentStr);
            EncryptData(buffer, 0, -1, keyBytes, buffer.Length);
            return Encoding.UTF8.GetString(buffer);
        }

        /// <summary>
        /// 加密文件内容（字节数组）
        /// </summary>
        /// <param name="buffer">文件内容字节数组</param>
        /// <param name="offset">从buffer的哪个位置开始</param>
        /// <param name="len">长度</param>
        /// <param name="key">加密key</param>
        /// <param name="keyOffset">加密字偏移量</param>
        public static void EncryptData(byte[] buffer, long offset, long len, string key, long keyOffset)
        {
            if (!IsEncrypt)
                return;
            byte[] encryptBytes = Encoding.UTF8.GetBytes(key);
            EncryptData(buffer, offset, len, encryptBytes, keyOffset);
        }

        /// <summary>
        /// 加密文件内容（字节数组）
        /// </summary>
        /// <param name="buffer">文件内容字节数组</param>
        /// <param name="offset">从buffer的哪个位置开始</param>
        /// <param name="len">长度</param>
        /// <param name="keyBytes">加密字节流</param>
        /// <param name="keyOffset">加密字偏移量</param>
        /// <returns></returns>
        public static void EncryptData(byte[] buffer, long offset, long len, byte[] keyBytes, long keyOffset)
        {
            if (!IsEncrypt)
                return;
            len = len == -1 ? buffer.LongLength - offset : Math.Min(buffer.Length - offset, len );
            long enCount = keyBytes.Length;
            for (long i = 0; i < len; i++)
            {
                buffer[offset + i] ^= keyBytes[(keyOffset + i) % enCount];
            }
        }
    }
}
