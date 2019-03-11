using System;
using System.IO;
using System.IO.Compression;
using System.Threading;

namespace GZipTest
{
    class Compressor : BaseZip
    {
        /// <summary>
        /// Read and divide file to blocks and send to compress
        /// </summary>
        /// <param name="readPath">File path for reading data</param>
        /// <param name="writePath">File path for writing compressed data</param>
        public void Compress(string readPath, string writePath)
        {
            using (var readStream = new FileStream(readPath, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                if (readStream.Length == 0)
                    throw new Exception("File is empty");

                using (var bwriter = new BinaryWriter(new FileStream(writePath, FileMode.Create, FileAccess.Write, FileShare.None)))
                {
                    blocksCount = (int)(readStream.Length / blockSize);//Determine the count of blocks

                    if (readStream.Length % blockSize > 0)
                        blocksCount++;

                    bwriter.Write(blocksCount);//Write blocks count to file

                    int readedBlocks = 1;
                    int writedBlocks = 1;
                    for (int i = 0; i < blocksCount; i++)
                    {
                        var block = new byte[blockSize];
                        int blockLength = readStream.Read(block, 0, blockSize);//Read bytes of block from file
                        int ind = readedBlocks;
                        threadPool.AddTask(() =>  //Send block for compress in threadpool
                        {
                            byte[] bufferCompressed = CompressBuffer(block, blockLength);
                            lock (lockObject)
                            {
                                bwriter.Write(ind);
                                bwriter.Write(bufferCompressed.Length);
                                bwriter.Write(bufferCompressed, 0, bufferCompressed.Length);
                            }

                            if (writedBlocks == blocksCount)
                                resetEvent.Set();

                            Progress(writedBlocks);
                            writedBlocks++;
                        });

                        readedBlocks++;
                    }

                    resetEvent.WaitOne();//Wait for all tasks
                }
            }
        }

        /// <summary>
        /// Compress array of bytes
        /// </summary>
        /// <param name="block">Block bytes</param>
        /// <param name="length">Length of block for compressing </param>
        private byte[] CompressBuffer(byte[] block, int length)
        {
            using (var stream = new MemoryStream())
            {
                using (var compressionStream = new GZipStream(stream, CompressionMode.Compress))
                {
                    compressionStream.Write(block, 0, length);
                }

                return stream.ToArray();
            }
        }
    }
}
