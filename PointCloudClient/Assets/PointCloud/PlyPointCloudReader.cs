using System.Collections.Generic;
using System.IO;

using UnityEngine;

namespace KOM.PointCloud
{
    /// <summary>
    /// A class to load point clouds encoded as binary Ply.
    /// </summary>
    public class PlyPointCloudReader : IPointCloudReader
    {
        public List<Mesh> CreateFromBytes(byte[] data)
        {
            MemoryStream stream = new MemoryStream(data);
            stream.Position = 0;
            Mesh mesh = new PlyReader().ImportAsMesh(stream,"");

            return new List<Mesh>() {mesh};
        }
    }
}
