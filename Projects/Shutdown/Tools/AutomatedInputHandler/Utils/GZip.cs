// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-04-24
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.IO;
using System.IO.Compression;

namespace Utils {
    public static class GZip {

        /// <summary>
        /// Compresses the specified file.
        /// </summary>
        /// <param name="filename">File which should be compressed.</param>
        /// <param name="destFilename">Optional name of the output file (default = [input filename].gz</param>
        public static void Compress(string filename, string destFilename = null) { Compress(new FileInfo(filename), destFilename); }

        /// <summary>
        /// Compresses the specified file.
        /// </summary>
        /// <param name="fi">Fileinfo for the file which should be compressed which should be compressed.</param>
        /// <param name="destFilename">Optional name of the output file (default = [input filename].gz</param>
        public static void Compress(FileInfo fi, string destFilename = null) {
            if (fi.Extension == ".gz")
                return;

            byte[] b;
            using (var f = fi.OpenRead()) {
                b = new byte[f.Length];
                f.Read(b, 0, (int)f.Length);
            }

            using (var outFile = File.Create(fi.FullName + ".gz")) {
                using (var gz = new GZipStream(outFile, CompressionMode.Compress, false)) {
                    gz.Write(b, 0, b.Length);
                }
            }
        }

        /// <summary>
        /// Compresses the specified memory stream.
        /// </summary>
        /// <param name="memory">The memory stream which should be compressed.</param>
        /// <returns>A new memory stream which contains the compressed data.</returns>
        public static MemoryStream Compress(MemoryStream memory) {
            byte[] b = new byte[memory.Length];
            memory.Read(b, 0, (int)memory.Length);
            MemoryStream result = new MemoryStream();
            using (var gz = new GZipStream(result, CompressionMode.Compress, false)) gz.Write(b, 0, b.Length);
            return result;
        }

        /// <summary>
        /// Compresses the specified data array.
        /// </summary>
        /// <param name="data">Byte array which should be compressed.</param>
        /// <returns>A new byte array which contains the compressed data.</returns>
        public static byte[] Compress(byte[] data) {
            using (MemoryStream stream = new MemoryStream()) {
                using (var gz = new GZipStream(stream, CompressionMode.Compress, false)) gz.Write(data, 0, data.Length);
                return stream.ToArray();
            }
        }

        /// <summary>
        /// Decompresses the specified file.
        /// </summary>
        /// <param name="filename">File which should be decomressed.</param>
        /// <param name="destFilename">Optional name of the output file (default = filename without .gz extension).</param>
        public static void Decompress(string filename, string destFilename = null) { Decompress(new FileInfo(filename), destFilename); }

        /// <summary>
        /// Decompresses the specified file.
        /// </summary>
        /// <param name="fi">File which should be decomressed.</param>
        /// <param name="destFilename">Optional name of the output file (default = filename without .gz extension).</param>
        public static void Decompress(FileInfo fi, string destFilename = null) {
            if (fi.Extension != ".gz")
                return;

            byte[] b;
            using (FileStream f = fi.OpenRead()) {
                b = new byte[f.Length];
                f.Read(b, 0, (int)f.Length);
            }

            using (var outFile = File.Create(fi.FullName.Substring(0, fi.FullName.Length - 3))) {
                using (var gz = new GZipStream(new MemoryStream(b), CompressionMode.Decompress, false)) {
                    const int size = 4096;
                    byte[] buffer = new byte[size];
                    int count;
                    do {
                        count = gz.Read(buffer, 0, size);
                        if (count > 0) outFile.Write(buffer, 0, count);
                    } while (count > 0);
                }
            }
        }

        /// <summary>
        /// Decompresses the specified memory stream.
        /// </summary>
        /// <param name="memory">The memory stream which should be decompressed.</param>
        /// <returns>A new memory stream which contains the decompressed data.</returns>
        public static MemoryStream Decompress(MemoryStream memory) {
            var stream = new MemoryStream();
            using (var gz = new GZipStream(memory, CompressionMode.Decompress, false)) {
                const int size = 4096;
                byte[] buffer = new byte[size];
                int count;
                do {
                    count = gz.Read(buffer, 0, size);
                    if (count > 0) stream.Write(buffer, 0, count);
                } while (count > 0);
            }

            return stream;
        }

        /// <summary>
        /// Decompresses the specified data array.
        /// </summary>
        /// <param name="data">Byte array which should be decompressed.</param>
        /// <returns>A new byte array which contains the decompressed data.</returns>
        public static byte[] Decompress(byte[] data) {
            using (var stream = new MemoryStream()) {
                using (var gz = new GZipStream(new MemoryStream(data), CompressionMode.Decompress, false)) {
                    const int size = 4096;
                    byte[] buffer = new byte[size];
                    int count;
                    do {
                        count = gz.Read(buffer, 0, size);
                        if (count > 0) stream.Write(buffer, 0, count);
                    } while (count > 0);
                }

                return stream.ToArray();
            }
        }
    }
}