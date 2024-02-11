using System;

namespace KOM.DASHLib
{
    /// <summary>
    /// A factory class to create <c>IDataSource</c> instances based
    /// on a scheme.
    /// 
    /// Supports currently the "file" and "http" schemes.
    /// </summary>
    public class DataSourceFactory
    {
        /// <summary>
        /// Create a new <c>IDataSource</c> instance.
        /// </summary>
        /// 
        /// <param name="scheme">The required scheme of the data source.</param>
        /// <returns>A new <c>IDataSource</c> instance</returns>
        public static IDataSource Create(string scheme)
        {
            if (scheme == "file")
            {
                return new FilesystemDataSource();
            }

            if (scheme == "http")
            {
                return new Http1DataSource();
            }

            throw new NotSupportedException("Unsupported scheme " + scheme);
        }
    }
}