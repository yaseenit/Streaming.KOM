using System;

namespace KOM.DASHLib
{
    /// <summary>
    /// A DASH media representation defined by a set of properties
    /// consisting of key value pairs.
    /// </summary>
    public interface IRepresentation
    {
        /// <summary>
        /// A unique ID of the representation.
        /// </summary>
        public string ID { get; }

        /// <summary>
        /// The URI where the representation can be retrieved from.
        /// </summary>
        public Uri URI { get; }

        /// <summary>
        /// The expected bandwidth in bytes per second.
        /// </summary>
        public long Bandwidth { get; }

        /// <summary>
        /// An <c>AdaptationSet</c> where the representation originates
        /// from.
        /// </summary>
        public AdaptationSet AdaptationSet { get; }

        /// <summary>
        /// The retrieved data of the representation.
        /// </summary>
        public byte[] Data { get; set; }

        /// <summary>
        /// The current state of the representation.
        /// </summary>
        /// <value></value>
        public ERepresentationState State { set; get; }

        /// <summary>
        /// Prepare the media for playback.
        /// </summary>
        public void Prepare();
    }
}
