using System.IO;
 
namespace Easy.EasyAsset
{
    /// <summary>
    /// ab解密流
    /// </summary>
    public class XOREncryptFileStream : FileStream
    {
        public XOREncryptFileStream(string path, FileMode mode, FileAccess access, FileShare share, int bufferSize, bool useAsync) : base(path, mode, access, share, bufferSize, useAsync)
        {

        }

        public XOREncryptFileStream(string path, FileMode mode) : base(path, mode)
        {

        }

        public override int Read(byte[] array, int offset, int count)
        {
            long prePosition = Position;
            var len = base.Read(array, offset, count);
            XOREncryption.DecryptData(array, offset, len, XOREncryption.DEFAULT_ENCRYPT_KEY, prePosition + Length);
            return len;
        }
    }  

    public class XOREncryptStream : Stream
    {
        public FileStream fileStream;

        public int fileStreamOffset; 

        private long position;

        private long length;

        public override bool CanRead => true;

        public override bool CanSeek => true;

        public override bool CanWrite => false;

        public override long Length => length;

        public override long Position { get => position; set => position = value; }

        public XOREncryptStream(FileStream fileStream, int fileStreamOffset, long len) : base()
        {
            this.fileStream = fileStream;
            this.fileStreamOffset = fileStreamOffset;
            SetLength(len);
        }

        public override long Seek(long offset, SeekOrigin loc)
        {
            var seekPos = fileStream.Seek(offset + fileStreamOffset, loc);
            return seekPos - fileStreamOffset;
        }

        public override int Read(byte[] array, int offset, int count)
        {
            long prePosition = Position;
            var len = fileStream.Read(array, offset, count);
            XOREncryption.DecryptData(array, offset, len, XOREncryption.DEFAULT_ENCRYPT_KEY, prePosition + Length);
            return len;
        }

        public override void Flush()
        {

        }

        public override void SetLength(long value)
        {
            length = value;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {

        }

        protected override void Dispose(bool disposing)
        {
            fileStream.Dispose();
            base.Dispose(disposing);
        }
    }
}