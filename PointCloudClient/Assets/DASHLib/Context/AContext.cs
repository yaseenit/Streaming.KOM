using System.Collections.Generic;
using UnityEngine;

namespace KOM.DASHLib
{
    /// <summary>
    /// A generic rendering context that has to be implemented for
    /// specific content types.
    /// </summary>
    public abstract class AContext : MonoBehaviour
    {
        /// <summary>
        /// The type of this context.
        /// </summary>
        public abstract ContextType Type { get; }

        /// <summary>
        /// A list containing the supported MIME-types.
        /// </summary>
        public abstract List<string> SupportedMimeTypes { get; }

        /// <summary>
        /// Register the media element to allow media preparation by
        /// this context.
        /// 
        /// If an unregistered media element is rendered, than only
        /// lazy loading is supported.
        /// </summary>
        public abstract IRepresentation Register(IRepresentation representation);

        /// <summary>
        /// Render the media element in the context
        /// </summary>
        public abstract void  Render(IRepresentation media);

        /// <summary>
        /// Reset the context to its initial state.
        /// </summary>
        public abstract void Reset();
    }
}
