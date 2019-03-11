using System;
using System.Threading;

namespace GZipTest
{
    class BaseZip : IDisposable
    {
        protected readonly int blockSize = 1048576; //1024*1024 1MB
        protected readonly CustomThreadPool threadPool;
        protected readonly AutoResetEvent resetEvent;
        protected readonly object lockObject = new object();

        protected int blocksCount;

        public BaseZip()
        {
            threadPool = new CustomThreadPool();
            resetEvent = new AutoResetEvent(false);
        }

        public void Dispose()
        {
            threadPool.Dispose();
        }

        protected void Progress(int index)
        {
            int value = index * 100 / blocksCount;
            Console.Write("\r Progress: {0}%", value);
        }
    }
}
