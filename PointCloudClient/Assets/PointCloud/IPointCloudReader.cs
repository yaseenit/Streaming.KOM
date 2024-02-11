using UnityEngine;
using System.Collections.Generic;

namespace KOM.PointCloud
{
    /// <summary>
    /// An interface allowing to implement point cloud creation from
    /// various data types.
    /// </summary>
    public interface IPointCloudReader
    {
        /// <summary>
        /// Create and return point clouds from the given data.
        /// </summary>
        /// 
        /// <param name="data">The data forming the point cloud.</param>
        /// 
        /// <returns>A list of point clouds created from the given data.
        /// </returns>
        public List<Mesh> CreateFromBytes(byte[] data);
    }
}
