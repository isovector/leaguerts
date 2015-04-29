using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using System.IO;
using LeagueEngine.Zip;

namespace LeagueEngine {
    /// <summary>
    /// Reads content from a ZIP archive
    /// </summary>
    public class ZipContentManager : ContentManager {
        /// <summary>
        /// The internal archive used
        /// </summary>
        public ZipArchive Archive;

        /// <summary>
        /// Creates a new ZipContentManager
        /// </summary>
        /// <param name="services">The game's services</param>
        /// <param name="file">The path to the ZIP archive</param>
        public ZipContentManager(GameServiceContainer services, string file)
            : base(services) {
            Archive = new ZipArchive("Content/" + file);
        }

        protected override Stream OpenStream(string assetName) {
            return Archive.OpenFile(assetName + ".xnb");
        }

        protected override void Dispose(bool disposing) {
            Archive.Dispose();
            base.Dispose(disposing);
        }
    }
}
