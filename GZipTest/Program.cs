using System;
using System.IO;
using System.IO.Compression;
using System.Diagnostics;

namespace GZipTest
{
    class Program
    {
        private static CompressionMode mode;
        private static string pathFileOriginal;
        private static string pathFileResult;

        static void Main(string[] args)
        {
            var watch = new Stopwatch();
            watch.Start();
            try
            {
                GetArguments(args);
                LaunchZip();

                watch.Stop();
                Console.WriteLine();
                Console.WriteLine("Process finished with time: {0}", watch.Elapsed);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return;
        }

        /// <summary>
        /// Choose necessary mode and start process
        /// </summary>
        private static void LaunchZip()
        {
            if (mode == CompressionMode.Compress)
            {
                using (var compressor = new Compressor())
                {
                    compressor.Compress(pathFileOriginal, pathFileResult);
                }
            }
            else
            {
                using (var decompressor = new Decompressor())
                {
                    decompressor.Decompress(pathFileOriginal, pathFileResult);
                }
            }
        }

        /// <summary>
        /// Getting and validation arguments
        /// </summary>
        private static void GetArguments(string[] args)
        {
            if (args.Length != 3)
                throw new Exception("Bad arguments");

            if (args[0] == "compress")
                mode = CompressionMode.Compress;
            else if (args[0] == "decompress")
                mode = CompressionMode.Decompress;
            else
                throw new Exception("Incorrect mode");

            if (!File.Exists(args[1]))
                throw new Exception("Original file doesn't exist");

            pathFileOriginal = args[1];
            pathFileResult = args[2];
        }
    }
}
