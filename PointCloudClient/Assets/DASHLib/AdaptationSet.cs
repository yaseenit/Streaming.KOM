using System.Collections.Generic;
using System.Linq;

namespace KOM.DASHLib
{
    /// <summary>
    /// Holds meta data about a DASH <c>Adaptation Set</c> and contains its representations.
    /// </summary>
    public class AdaptationSet
    {
        /// <summary>
        /// The unique ID of the adaptation set.
        /// </summary>
        public string ID {get;}

        /// <summary>
        /// The MIME type of the adaptation set.
        /// </summary>
        public string MimeType {get;}

        /// <summary>
        /// A <c>List</c> of <c>Representation</c> objects, which represent the adapation set in differing quality levels.
        /// </summary>
        public readonly List<IRepresentation> Representations = new List<IRepresentation>();

        /// <summary>
        /// A representation that should be used for playback. Must be
        /// one of the <c>IRepresentation</c> entries in
        /// <cref>Representations</cref>.
        /// </summary>
        public IRepresentation Selection { get; set; }

        /// <summary>
        /// The period containing this adaptation set.
        /// </summary>
        public Period Period;

        /// <summary>
        /// Initializes a new adaption set of the specified MIME-Type.
        /// </summary>
        public AdaptationSet(string id, string mimeType, Period parent)
        {
            this.ID = id;
            this.MimeType = mimeType;
            this.Period = parent;
        }
    }
}