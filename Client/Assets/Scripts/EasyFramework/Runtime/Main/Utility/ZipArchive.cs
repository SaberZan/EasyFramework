using System;
using System.IO;
using UnityEngine;

namespace Easy
{
    /// <summary>
    /// 精简优化版ZIP解析.
    /// </summary>
    internal class ZipArchive
    {
        private const ushort MASK16BIT = 0xFFFF;
        private const uint MASK32BIT = 0xFFFFFFFF;

        internal class ReadOnlyStream : Stream
        {
            private Stream stream;
            private long offset;
            private long length;
            private long position;

            public ReadOnlyStream(Stream stream, long offset, long length)
            {
                this.stream = stream;
                this.offset = offset;
                this.position = offset;
                this.length = length;
            }

            public void Reset(Stream stream, long offset, long length)
            {
                this.stream = stream;
                this.offset = offset;
                this.position = offset;
                this.length = length;
            }

            public override long Length { get { return length; } }

            public override long Position { get { return position - offset; } set { position = value + offset; } }

            public long RealPosition { get { return position; } }

            public override bool CanRead { get { return stream.CanRead; } }

            public override bool CanSeek { get { return stream.CanSeek; } }

            public override bool CanWrite { get { return false; } }

            public override int Read(byte[] buffer, int offset, int count)
            {
                if (stream.Position != position)
                    stream.Seek(position, SeekOrigin.Begin);
                long endPosition = this.offset + this.length;
                if (position + count > endPosition)
                    count = (int)(endPosition - position);
                int bytesRead = stream.Read(buffer, offset, count);
                position += bytesRead;
                return bytesRead;
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                if (origin == SeekOrigin.Begin)
                    position = stream.Seek(this.offset + offset, SeekOrigin.Begin);
                else if (origin == SeekOrigin.End)
                    position = stream.Seek(this.offset + Length + offset, SeekOrigin.Begin);
                else
                    position = stream.Seek(offset, SeekOrigin.Current);
                return position;
            }

            public override void SetLength(long value)
            {
                throw new NotSupportedException();
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                throw new NotSupportedException();
            }

            public override void Flush()
            {
                throw new NotSupportedException();
            }

            protected override void Dispose(bool disposing)
            {
                stream = null;
                base.Dispose(disposing);
            }
        }

        internal class ZipReader : IDisposable
        {
            internal Stream BaseStream { get; set; }

            private readonly byte[] buffer = new byte[8];

            public ZipReader(Stream baseStream)
            {
                this.BaseStream = baseStream;
            }

            public ushort ReadUInt16()
            {
                BaseStream.Read(buffer, 0, 2);
                int byte1 = buffer[0] & 0xFF;
                int byte2 = buffer[1] & 0xFF;
                return (ushort)(byte1 + (byte2 << 8));
            }

            public uint ReadUInt32()
            {
                BaseStream.Read(buffer, 0, 4);
                int byte1 = buffer[0] & 0xFF;
                int byte2 = buffer[1] & 0xFF;
                int byte3 = buffer[2] & 0xFF;
                int byte4 = buffer[3] & 0xFF;
                return (uint)(byte1 + (byte2 << 8) + (byte3 << 16) + (byte4 << 24));
            }

            public ulong ReadUInt64()
            {
                BaseStream.Read(buffer, 0, 8);
                ulong byte1 = (ulong)(buffer[0] & 0xFF);
                ulong byte2 = (ulong)(buffer[1] & 0xFF);
                ulong byte3 = (ulong)(buffer[2] & 0xFF);
                ulong byte4 = (ulong)(buffer[3] & 0xFF);
                ulong byte5 = (ulong)(buffer[4] & 0xFF);
                ulong byte6 = (ulong)(buffer[5] & 0xFF);
                ulong byte7 = (ulong)(buffer[6] & 0xFF);
                ulong byte8 = (ulong)(buffer[7] & 0xFF);

                return (byte1 + (byte2 << 8) + (byte3 << 16) + (byte4 << 24) + (byte5 << 32) + (byte6 << 40) + (byte7 << 48) + (byte8 << 56));
            }

            public static long ReadInt64(byte[] bytes, int offset)
            {
                long byte1 = bytes[offset + 0] & 0xFF;
                long byte2 = bytes[offset + 1] & 0xFF;
                long byte3 = bytes[offset + 2] & 0xFF;
                long byte4 = bytes[offset + 3] & 0xFF;
                long byte5 = bytes[offset + 4] & 0xFF;
                long byte6 = bytes[offset + 5] & 0xFF;
                long byte7 = bytes[offset + 6] & 0xFF;
                long byte8 = bytes[offset + 7] & 0xFF;
                return (byte1 + (byte2 << 8) + (byte3 << 16) + (byte4 << 24) + (byte5 << 32) + (byte6 << 40) + (byte7 << 48) + (byte8 << 56));
            }

            public void Dispose()
            {
                BaseStream = null;
            }
        }

        private struct ZipEndOfDirectoryBlock
        {
            public const uint signatureConstant = 0x06054B50;
            public const int sizeOfBlockWithoutSignature = 18;
            public ushort numberOfThisDisk;
            public ushort numberOfTheDiskWithTheStartOfTheCentralDirectory;
            public ushort numberOfEntriesInTheDirectoryOnThisDisk;
            public ushort numberOfEntriesInTheDirectory;
            public uint offsetOfStartOfDirectoryWithRespectToTheStartingDiskNumber;

            public static bool TryReadBlock(ZipReader reader, out ZipEndOfDirectoryBlock eocdBlock)
            {
                eocdBlock = new ZipEndOfDirectoryBlock();
                if (reader.ReadUInt32() != signatureConstant)
                    return false;
                // eocdBlock.Signature = SignatureConstant;
                eocdBlock.numberOfThisDisk = reader.ReadUInt16();
                eocdBlock.numberOfTheDiskWithTheStartOfTheCentralDirectory = reader.ReadUInt16();
                eocdBlock.numberOfEntriesInTheDirectoryOnThisDisk = reader.ReadUInt16();
                eocdBlock.numberOfEntriesInTheDirectory = reader.ReadUInt16();
                //eocdBlock.SizeOfDirectory = reader.ReadUInt32();
                reader.BaseStream.Position += 4;
                eocdBlock.offsetOfStartOfDirectoryWithRespectToTheStartingDiskNumber = reader.ReadUInt32();
                ushort commentLength = reader.ReadUInt16();
                // eocdBlock.ArchiveComment = reader.ReadBytes(commentLength);
                reader.BaseStream.Position += commentLength;
                return true;
            }
        }

        private struct Zip64EndOfDirectoryLocator
        {
            public const uint signatureConstant = 0x07064B50;
            public const int sizeOfBlockWithoutSignature = 16;
            public ulong offsetOfZip64EOCD;

            public static bool TryReadBlock(ZipReader reader, out Zip64EndOfDirectoryLocator zip64EOCDLocator)
            {
                zip64EOCDLocator = new Zip64EndOfDirectoryLocator();
                if (reader.ReadUInt32() != signatureConstant)
                    return false;
                //zip64EOCDLocator.NumberOfDiskWithZip64EOCD = reader.ReadUInt32();
                reader.BaseStream.Position += 4;
                zip64EOCDLocator.offsetOfZip64EOCD = reader.ReadUInt64();
                //zip64EOCDLocator.TotalNumberOfDisks = reader.ReadUInt32();
                reader.BaseStream.Position += 4;
                return true;
            }
        }

        private struct Zip64EndOfDirectoryRecord
        {
            public ulong numberOfEntriesOnThisDisk;
            public ulong numberOfEntriesTotal;
            public ulong offsetOfDirectory;

            public static bool TryReadBlock(ZipReader reader, out Zip64EndOfDirectoryRecord zip64EOCDRecord)
            {
                zip64EOCDRecord = new Zip64EndOfDirectoryRecord();
                if (reader.ReadUInt32() != 0x06064B50) //Signature
                    return false;
                //zip64EOCDRecord.SizeOfThisRecord = reader.ReadUInt64();
                //zip64EOCDRecord.VersionMadeBy = reader.ReadUInt16();
                //zip64EOCDRecord.VersionNeededToExtract = reader.ReadUInt16();
                //zip64EOCDRecord.NumberOfThisDisk = reader.ReadUInt32();
                //zip64EOCDRecord.NumberOfDiskWithStartOfCD = reader.ReadUInt32();
                reader.BaseStream.Position += 8 + 2 + 2 + 4 + 4;
                zip64EOCDRecord.numberOfEntriesOnThisDisk = reader.ReadUInt64();
                zip64EOCDRecord.numberOfEntriesTotal = reader.ReadUInt64();
                //zip64EOCDRecord.SizeOfDirectory = reader.ReadUInt64();
                reader.BaseStream.Position += 8;
                zip64EOCDRecord.offsetOfDirectory = reader.ReadUInt64();
                return true;
            }
        }

        private struct ZipGenericExtraField
        {
            public ushort tag;
            public ushort size;
            public byte[] data;

            public static bool TryReadBlock(ZipReader reader, long endExtraField, out ZipGenericExtraField field)
            {
                field = new ZipGenericExtraField();
                if (endExtraField - reader.BaseStream.Position < 4)
                    return false;
                field.tag = reader.ReadUInt16();
                field.size = reader.ReadUInt16();
                if (endExtraField - reader.BaseStream.Position < field.size)
                    return false;
                field.data = new byte[field.size];
                reader.BaseStream.Read(field.data, 0, field.size);
                return true;
            }
        }

        private struct Zip64ExtraField
        {
            private ushort size;
            public long uncompressedSize;
            public long compressedSize;
            public long localHeaderOffset;

            public static Zip64ExtraField GetJustZip64Block(ZipReader reader, bool readUncompressedSize, bool readCompressedSize, bool readLocalHeaderOffset, bool readStartDiskNumber)
            {
                Zip64ExtraField zip64Field = new Zip64ExtraField();
                zip64Field.compressedSize = -1;
                zip64Field.uncompressedSize = -1;
                zip64Field.localHeaderOffset = -1;
                ZipGenericExtraField extraField;
                while (ZipGenericExtraField.TryReadBlock(reader, reader.BaseStream.Length, out extraField))
                {
                    if (extraField.tag != 1)//Tag
                        continue;
                    zip64Field.size = extraField.size;
                    ushort expectedSize = 0;
                    if (readUncompressedSize)
                        expectedSize += 8;
                    if (readCompressedSize)
                        expectedSize += 8;
                    if (readLocalHeaderOffset)
                        expectedSize += 8;
                    if (readStartDiskNumber)
                        expectedSize += 4;
                    if (expectedSize != zip64Field.size)
                        continue;
                    if (readUncompressedSize)
                        zip64Field.uncompressedSize = ZipReader.ReadInt64(extraField.data, 0);
                    if (readCompressedSize)
                        zip64Field.compressedSize = ZipReader.ReadInt64(extraField.data, 8);
                    if (readLocalHeaderOffset)
                        zip64Field.localHeaderOffset = ZipReader.ReadInt64(extraField.data, 16);
                    //if (readStartDiskNumber) zip64Field.startDiskNumber = reader.ReadInt32();
                    // original values are unsigned, so implies value is too big to fit in signed integer
                    if (zip64Field.uncompressedSize < 0)
                        throw new IOException("FieldTooBigUncompressedSize");
                    if (zip64Field.compressedSize < 0)
                        throw new IOException("FieldTooBigCompressedSize");
                    if (zip64Field.localHeaderOffset < 0)
                        throw new IOException("FieldTooBigLocalHeaderOffset");
                    //if (zip64Field.startDiskNumber < 0) throw new IOException("FieldTooBigStartDiskNumber");
                    return zip64Field;
                }
                return zip64Field;
            }
        }

        internal struct ZipLocalFileHeader
        {
            public static bool TrySkipBlock(ZipReader reader)
            {
                if (reader.ReadUInt32() != 0x04034B50) //signature
                    return false;
                if (reader.BaseStream.Length < reader.BaseStream.Position + 22)
                    return false;
                reader.BaseStream.Seek(22, SeekOrigin.Current);
                ushort filenameLength = reader.ReadUInt16();
                ushort extraFieldLength = reader.ReadUInt16();
                if (reader.BaseStream.Length < reader.BaseStream.Position + filenameLength + extraFieldLength)
                    return false;
                reader.BaseStream.Seek(filenameLength + extraFieldLength, SeekOrigin.Current);
                return true;
            }
        }

        internal class ZipDirectoryFileHeader
        {
            public long compressedSize;
            public long uncompressedSize;
            public ushort filenameLength;
            private ushort extraFieldLength;
            private ushort fileCommentLength;
            public long relativeOffsetOfLocalHeader;
            public byte[] filename = new byte[64];
            private ReadOnlyStream readOnlyStream;
            private ZipReader zipReader;

            public static bool TryReadBlock(ZipReader reader, ZipDirectoryFileHeader header)
            {
                if (reader.ReadUInt32() != 0x02014B50)//signature
                    return false;
                // header.VersionMadeBySpecification = reader.ReadByte();
                // header.VersionMadeByCompatibility = reader.ReadByte();
                // header.VersionNeededToExtract = reader.ReadUInt16();
                // header.GeneralPurposeBitFlag = reader.ReadUInt16();
                // header.CompressionMethod = reader.ReadUInt16();
                //  header.LastModified = reader.ReadUInt32();
                //  header.Crc32 = reader.ReadUInt32();
                reader.BaseStream.Position += 1 + 1 + 2 + 2 + 2 + 4 + 4;
                uint compressedSizeSmall = reader.ReadUInt32();
                uint uncompressedSizeSmall = reader.ReadUInt32();
                header.filenameLength = reader.ReadUInt16();
                header.extraFieldLength = reader.ReadUInt16();
                header.fileCommentLength = reader.ReadUInt16();
                ushort diskNumberStartSmall = reader.ReadUInt16();
                //header.InternalFileAttributes = reader.ReadUInt16();
                //header.ExternalFileAttributes = reader.ReadUInt32();
                reader.BaseStream.Position += 2 + 4;
                uint relativeOffsetOfLocalHeaderSmall = reader.ReadUInt32();
                if (header.filenameLength > header.filename.Length)
                    header.filename = new byte[header.filenameLength];
                reader.BaseStream.Read(header.filename, 0, header.filenameLength);

                bool uncompressedSizeInZip64 = uncompressedSizeSmall == MASK32BIT;
                bool compressedSizeInZip64 = compressedSizeSmall == MASK32BIT;
                bool relativeOffsetInZip64 = relativeOffsetOfLocalHeaderSmall == MASK16BIT;
                bool diskNumberStartInZip64 = diskNumberStartSmall == MASK16BIT;
                long endExtraFields = reader.BaseStream.Position + header.extraFieldLength;
                //header.ExtraFields = null;
                if (header.readOnlyStream == null)
                    header.readOnlyStream = new ReadOnlyStream(reader.BaseStream, reader.BaseStream.Position, header.extraFieldLength);
                else
                    header.readOnlyStream.Reset(reader.BaseStream, reader.BaseStream.Position, header.extraFieldLength);
                if (header.zipReader == null)
                    header.zipReader = new ZipReader(header.readOnlyStream);
                Zip64ExtraField zip64 = Zip64ExtraField.GetJustZip64Block(header.zipReader, uncompressedSizeInZip64, compressedSizeInZip64, relativeOffsetInZip64, diskNumberStartInZip64);
                reader.BaseStream.Position = endExtraFields + header.fileCommentLength;
                //header.FileComment = null;
                header.uncompressedSize = zip64.uncompressedSize < 0 ? uncompressedSizeSmall : zip64.uncompressedSize;
                header.compressedSize = zip64.compressedSize < 0 ? compressedSizeSmall : zip64.compressedSize;
                header.relativeOffsetOfLocalHeader = zip64.localHeaderOffset < 0 ? relativeOffsetOfLocalHeaderSmall : zip64.localHeaderOffset;
                //header.DiskNumberStart = zip64.StartDiskNumber < 0 ? diskNumberStartSmall : zip64.startDiskNumber;
                return true;
            }
        }

        internal static void ReadEndOfDirectory(Stream stream, ZipReader reader, out long expectedNumberOfEntries, out long directoryStart)
        {
            try
            {
                stream.Seek(-ZipEndOfDirectoryBlock.sizeOfBlockWithoutSignature, SeekOrigin.End);
                if (!SeekEndOfSignature(stream, ZipEndOfDirectoryBlock.signatureConstant))
                    throw new IOException("SignatureConstant");
                long eocdStart = stream.Position;
                // read the EOCD
                ZipEndOfDirectoryBlock eocd;
                bool eocdProper = ZipEndOfDirectoryBlock.TryReadBlock(reader, out eocd);
                Debug.Assert(eocdProper); // we just found this using the signature finder, so it should be okay
                if (eocd.numberOfThisDisk != eocd.numberOfTheDiskWithTheStartOfTheCentralDirectory)
                    throw new IOException("SplitSpanned");
                directoryStart = eocd.offsetOfStartOfDirectoryWithRespectToTheStartingDiskNumber;
                if (eocd.numberOfEntriesInTheDirectory != eocd.numberOfEntriesInTheDirectoryOnThisDisk)
                    throw new IOException("SplitSpanned");
                expectedNumberOfEntries = eocd.numberOfEntriesInTheDirectory;
                // only bother looking for zip64 EOCD stuff if we suspect it is needed because some value is FFFFFFFFF
                // because these are the only two values we need, we only worry about these
                // if we don't find the zip64 EOCD, we just give up and try to use the original values
                if (eocd.numberOfThisDisk == MASK16BIT || eocd.offsetOfStartOfDirectoryWithRespectToTheStartingDiskNumber == MASK32BIT || eocd.numberOfEntriesInTheDirectory == MASK16BIT)
                {
                    // we need to look for zip 64 EOCD stuff
                    // seek to the zip 64 EOCD locator
                    stream.Seek(eocdStart - Zip64EndOfDirectoryLocator.sizeOfBlockWithoutSignature, SeekOrigin.Begin);
                    // if we don't find it, assume it doesn't exist and use data from normal eocd
                    if (SeekEndOfSignature(stream, Zip64EndOfDirectoryLocator.signatureConstant))
                    {
                        // use locator to get to Zip64EOCD
                        Zip64EndOfDirectoryLocator locator;
                        bool zip64eocdLocatorProper = Zip64EndOfDirectoryLocator.TryReadBlock(reader, out locator);
                        Debug.Assert(zip64eocdLocatorProper); // we just found this using the signature finder, so it should be okay
                        if (locator.offsetOfZip64EOCD > long.MaxValue)
                            throw new IOException("FieldTooBigOffsetToZip64EOCD");
                        stream.Seek((long)locator.offsetOfZip64EOCD, SeekOrigin.Begin);
                        Zip64EndOfDirectoryRecord record;
                        if (!Zip64EndOfDirectoryRecord.TryReadBlock(reader, out record))
                            throw new IOException("Zip64EOCDNotWhereExpected");
                        if (record.numberOfEntriesTotal > long.MaxValue)
                            throw new IOException("FieldTooBigNumEntries");
                        if (record.offsetOfDirectory > long.MaxValue)
                            throw new IOException("FieldTooBigOffsetToCD");
                        if (record.numberOfEntriesTotal != record.numberOfEntriesOnThisDisk)
                            throw new IOException("SplitSpanned");
                        expectedNumberOfEntries = (long)record.numberOfEntriesTotal;
                        directoryStart = (long)record.offsetOfDirectory;
                    }
                }
                if (directoryStart > stream.Length)
                    throw new IOException("FieldTooBigOffsetToCD");
            } catch (EndOfStreamException ex)
            {
                throw new IOException("CDCorrupt", ex);
            } catch (IOException ex)
            {
                throw new IOException("CDCorrupt", ex);
            }
        }

        private static bool SeekEndOfSignature(Stream stream, uint signature)
        {
            int pointer = 0;
            uint sign = 0;
            byte[] buffer = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32 };
            bool outOfBytes = false;
            while (!outOfBytes)
            {
                if (stream.Position >= buffer.Length)
                {
                    stream.Seek(-buffer.Length, SeekOrigin.Current);
                    stream.Read(buffer, 0, buffer.Length);
                    stream.Seek(-buffer.Length, SeekOrigin.Current);
                    pointer = buffer.Length - 1;
                    outOfBytes = false;
                } else
                {
                    int bytesToRead = (int)stream.Position;
                    stream.Seek(0, SeekOrigin.Begin);
                    stream.Read(buffer, 0, bytesToRead);
                    stream.Seek(0, SeekOrigin.Begin);
                    pointer = bytesToRead - 1;
                    outOfBytes = true;
                }

                Debug.Assert(pointer < buffer.Length);
                while (pointer >= 0)
                {
                    sign = (sign << 8) | ((uint)buffer[pointer]);
                    if (sign == signature)
                    {
                        stream.Seek(pointer, SeekOrigin.Current);
                        return true;
                    } else
                        pointer--;
                }
            }
            return false;
        }
    }
}
