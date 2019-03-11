using System;
using System.IO;
using System.IO.Compression;
using System.Threading;

namespace GZipTest
{
    class Decompressor : BaseZip
    {
        /// <summary>
        /// Read file and send to decompress
        /// </summary>
        /// <param name="readPath">File path for reading data</param>
        /// <param name="writePath">File path for writing decompressed data</param>
        public void Decompress(string readPath, string writePath)
        {
            using (var readStream = new FileStream(readPath, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                if (readStream.Length == 0)
                    throw new Exception("File is empty");

                using (var writeStream = new FileStream(writePath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    var breader = new BinaryReader(readStream);
                    int index = 1;

                    blocksCount = breader.ReadInt32();//Read blocks count
                    for (int i = 0; i < blocksCount; i++)
                    {
                        int number = breader.ReadInt32();//Read info and value of block
                        int length = breader.ReadInt32();
                        byte[] blockValue = breader.ReadBytes(length);

                        //semaphore.WaitOne();
                        threadPool.AddTask(() => //Send block for decompress in threadpool
                        {
                            byte[] bufferDecompressed = DecompressBuffer(blockValue, length);
                            long offset = GetBlockPositionByIndex(number);

                            lock (lockObject)
                            {
                                writeStream.Seek(offset, SeekOrigin.Begin);
                                writeStream.Write(bufferDecompressed, 0, bufferDecompressed.Length);

                                if (index == blocksCount) //When the last task finished, signal about it
                                    resetEvent.Set();
                            }

                            Progress(index);
                            index++;
                        });
                    }

                    resetEvent.WaitOne();//Wait for all tasks
                }
            }
        }

        private long GetBlockPositionByIndex(int index)
        {
            var position = (index - 1) * (long)blockSize;
            return position;
        }

        /// <summary>
        /// Decompress array of bytes
        /// </summary>
        /// <param name="block">Block bytes</param>
        /// <param name="length">Length of block for decompressing </param>
        private byte[] DecompressBuffer(byte[] block, int length)
        {
            using (var stream = new MemoryStream(block, 0, length))
            {
                using (var compressionStream = new GZipStream(stream, CompressionMode.Decompress))
                {
                    using (var dest = new MemoryStream())
                    {
                        var buffer = new byte[blockSize];
                        int n = compressionStream.Read(buffer, 0, buffer.Length);
                        dest.Write(buffer, 0, n);

                        return dest.ToArray();
                    }
                }
            }
        }
    }
}
