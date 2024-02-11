using UnityEngine;
using Draco;
using System.Collections.Generic;

namespace KOM.PointCloud
{
    /// <summary>
    /// A class to load Draco encoded point clouds.
    /// </summary>
    public class DrcPointCloudReader : IPointCloudReader
    {
        public List<Mesh> CreateFromBytes(byte[] data)
        {
            var draco = new DracoMeshLoader();
            Mesh mesh = draco.ConvertDracoMeshToUnity(data).GetAwaiter().GetResult();

            return new List<Mesh>() { mesh };
        }
    }
}
