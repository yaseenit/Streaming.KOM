using System;
using System.IO;

namespace KOM.DASHLib
{
    /// <summary>
    /// A <c>IDataSource</c> implementation to be used for file system
    /// access.
    /// </summary>
    class FilesystemDataSource : IDataSource
    {
        public byte[] GetBytes(Uri uri)
        {
            return File.ReadAllBytes(uri.AbsolutePath);
        }

        public string GetText(Uri uri)
        {
            return File.ReadAllText(uri.AbsolutePath);
        }
    }
}