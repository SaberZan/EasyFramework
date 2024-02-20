using System.Security.Cryptography;
using System.Text;
using System.IO;

namespace Easy
{
    public class MD5Utility
    {
        private static string GenHash(byte[] data)
        {
            // 创建一个 Stringbuilder 来收集字节并创建字符串
            StringBuilder sBuilder = new StringBuilder();
            // 循环遍历哈希数据的每一个字节并格式化为十六进制字符串
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            // 返回十六进制字符串
            return sBuilder.ToString();
        }

        public static string GetMd5Hash(byte[] buffer)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] data = md5.ComputeHash(buffer);
            return GenHash(data);
        }

        //计算字符串MD5
        public static string GetMd5Hash(string input)
        {
            if (input == null)
            {
                return null;
            }
            // 将输入字符串转换为字节数组并计算哈希数据
            byte[] buffer = Encoding.UTF8.GetBytes(input);
            return GetMd5Hash(buffer);
        }

        //计算文件MD5
        public static string GetFileMd5Hash(string path)
        {
            if (path == null)
            {
                return null;
            }
            // 将输入字符串转换为字节数组并计算哈希数据
            byte[] buffer = File.ReadAllBytes(path);
            return GetMd5Hash(buffer);
        }

        //计算文件MD5
        public static string GetStreamMd5Hash(string path)
        {
            if (path == null)
            {
                return null;
            }

            MD5 md5 = new MD5CryptoServiceProvider();
            FileStream fileStream = File.Open(path, FileMode.Open);
            byte[] data = md5.ComputeHash(fileStream);
            fileStream.Dispose();
            return GenHash(data);
        }
    }
}