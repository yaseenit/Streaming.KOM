using System;
using System.Net;

namespace KOM.DASHLib
{
    /// <summary>
    /// A <c>IDataSource</c> implementation to be used for HTTP/1 calls.
    /// </summary>
    class Http1DataSource : IDataSource
    {
        WebClient client = new WebClient();

        public byte[] GetBytes(Uri uri)
        {
            return client.DownloadData(uri);
        }

        public string GetText(Uri uri)
        {
            return client.DownloadString(uri);
        }
    }
}