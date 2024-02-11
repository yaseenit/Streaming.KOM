using System;

namespace KOM.DASHLib
{
    /// <summary>
    /// An interface to allow retrieving data from various sources.
    /// </summary>
    public interface IDataSource
    {
        /// <summary>
        /// Retrieve data from the given URI and return a byte
        /// representation of it.
        /// </summary>
        /// 
        /// <param name="uri">The target URI.</param>
        /// <returns>A byte representation of the data</returns>
        byte[] GetBytes(Uri uri);

        /// <summary>
        /// Retrieve data from the given URI and return a string
        /// representation of it.
        /// </summary>
        /// 
        /// <param name="uri">The target URI.</param>
        /// <returns>A string representation of the data</returns>
        string GetText(Uri uri);
    }
}