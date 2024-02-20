using System.IO;
using System.Security.Cryptography;

namespace UnityEngine.ResourceManagement.ResourceProviders
{
    /// <summary>
    /// AA加密
    /// </summary>
    public class AAAssetsAES : IDataConverter
    {
        byte[] Key
        {
            get
            {
                return System.Text.Encoding.ASCII.GetBytes("ABCDEFGHIJKLMNOP");//修改此处密钥,需要16位,正常ASCII码均可
            }
        }
        SymmetricAlgorithm _algorithm;
        SymmetricAlgorithm Algorithm
        {
            get
            {
                if (_algorithm == null)
                {
                    _algorithm = new AesManaged();
                    _algorithm.Padding = PaddingMode.Zeros;
                    var initVector = new byte[_algorithm.BlockSize / 8];
                    for (int i = 0; i < initVector.Length; i++)
                        initVector[i] = (byte)i;
                    _algorithm.IV = initVector;
                    _algorithm.Key = Key;
                    _algorithm.Mode = CipherMode.ECB;
                }
                return _algorithm;
            }
        }

        public Stream CreateReadStream(Stream input, string id)
        {
            return new CryptoStream(input,
            Algorithm.CreateDecryptor(Algorithm.Key, Algorithm.IV),
            CryptoStreamMode.Read);
        }

        public Stream CreateWriteStream(Stream input, string id)
        {
            return new CryptoStream(input,
            Algorithm.CreateEncryptor(Algorithm.Key, Algorithm.IV),
            CryptoStreamMode.Write);
        }
    }
}