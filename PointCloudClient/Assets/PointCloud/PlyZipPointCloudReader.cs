using System.Collections.Generic;

using System.IO;
using System.IO.Compression;

using UnityEngine;

namespace KOM.PointCloud
{
    /// <summary>
    /// A class to load point clouds encoded in the binary Ply format
    /// and wrapped in a zip file.
    /// </summary>
    public class PlyZipPointCloudReader : IPointCloudReader
    {
        public List<Mesh> CreateFromBytes(byte[] data)
        {
            MemoryStream stream = new MemoryStream(data);
            stream.Position = 0;
            ZipArchive zip = new ZipArchive(stream);

            List<Mesh> meshes = new List<Mesh>();
            IPointCloudReader reader = PointCloudReaderFactory.Create("pointcloud/ply");
            foreach (ZipArchiveEntry entry in zip.Entries)
            {
                meshes.AddRange(reader.CreateFromBytes(ReadEntry(entry)));
            }
            return meshes;
        }

        /// <summary>
        /// Read the given zip entry into memory and return the data.
        /// </summary>
        /// 
        /// <param name="entry">The entry to load data from.</param>
        /// 
        /// <returns>The data contained by the zip entry.</returns>
        private byte[] ReadEntry(ZipArchiveEntry entry)
        {
            MemoryStream entryStream = new MemoryStream();
            entry.Open().CopyTo(entryStream);
            entryStream.Position = 0;

            return entryStream.ToArray();
        }
    }
}
