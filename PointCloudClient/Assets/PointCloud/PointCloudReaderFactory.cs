using System;

namespace KOM.PointCloud
{
    /// <summary>
    /// A factory class to provide a point cloud loader based on a
    /// MIME-Type.
    /// </summary>
    public static class PointCloudReaderFactory
    {
        /// <summary>
        /// Create a <c>PointCloudReader</c> instance fitting the
        /// MIME-Type.
        /// </summary>
        /// 
        /// <param name="mimeType">The requested MIME-Type.</param>
        /// 
        /// <returns>A point cloud reader for the given MIME-Type or
        /// throws an exception if not supported.</returns>
        public static IPointCloudReader Create(string mimeType)
        {
            switch (mimeType)
            {
                case "pointcloud/ply": return new PlyPointCloudReader();
                case "pointcloud/ply+zip": return new PlyZipPointCloudReader();
                case "pointcloud/drc": return new DrcPointCloudReader();
            }

            throw new NotSupportedException($"No reader found for MIME-Type {mimeType}");
        }
    }
}
