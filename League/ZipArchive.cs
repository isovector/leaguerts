using System;
using System.Collections.Generic;
using System.Text;
using ICSharpCode.SharpZipLib.Zip;
using System.IO;

namespace LeagueEngine.Zip {
    /// <summary>
    /// Provides logical functions to wrap the ICSharpCode.SharpZipLib library
    /// </summary>
    public class ZipArchive : IDisposable {
        /// <summary>
        /// The archive to read
        /// </summary>
        public ZipFile Archive;

        /// <summary>
        /// The stream of the archive
        /// </summary>
        public ZipInputStream ArchiveStream;


        /// <summary>
        /// Loads a zip
        /// </summary>
        /// <param name="file">The file to load</param>
        public ZipArchive(string file) {
            Archive = new ZipFile(file);
            ArchiveStream = new ZipInputStream(File.OpenRead(file));
        }

        /// <summary>
        /// Checks whether an internal file exists
        /// </summary>
        /// <param name="asset">The file to check</param>
        /// <returns>Does asset exist?</returns>
        public bool FileExists(string asset) {
            return Archive.GetEntry(asset.Replace('\\', '/')) != null;
        }

        /// <summary>
        /// Gets a stream to the requested file
        /// </summary>
        /// <param name="asset">The file to load</param>
        /// <returns>A stream to the file</returns>
        public Stream OpenFile(string asset) {
            // We can't actually do this, so we make our own stream and 
            // then read the data out of the zip and into our stream.


            // XNA gives assets with \s, but zips like /s
            ZipEntry file = Archive.GetEntry(asset.Replace('\\', '/'));
            MemoryStream ms = new MemoryStream((int)file.Size);

            ZipEntry entry;
            while ((entry = ArchiveStream.GetNextEntry()) != null)
                if (entry.Name == file.Name) {
                    byte[] data = new byte[2048];
                    int size;
                    while ((size = ArchiveStream.Read(data, 0, 2048)) != 0) {
                        ms.Write(data, 0, size);
                    }
                }
            ms.Seek(0, SeekOrigin.Begin);

            ArchiveStream = new ZipInputStream(File.OpenRead(Archive.Name));
            return ms;
        }

        /// <summary>
        /// Destroys all resources
        /// </summary>
        public void Dispose() {
            Archive.Close();
            ArchiveStream.Close();
        }
    }
}
