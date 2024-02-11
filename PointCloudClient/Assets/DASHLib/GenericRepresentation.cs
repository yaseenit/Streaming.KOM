using System;
using System.Collections.Generic;

namespace KOM.DASHLib
{
    /// <summary>
    /// A DASH media representation defined by a set of properties consisting of key value pairs.
    /// </summary>
    public class GenericRepresentation : IRepresentation
    {
        public string ID {get;}
        public Uri URI {get;}
        public long Bandwidth {get;}
        public AdaptationSet AdaptationSet {get;}

        protected byte[] data = null;
        public byte[] Data
        {
            get { return this.data; }
            set { this.data = value; }
        }

        private ERepresentationState state = ERepresentationState.Unbuffered;
        public ERepresentationState State
        {
            set
            {
                var oldState = this.state;
                this.state = value;
                //UnityEngine.Debug.Log($"{this.AdaptationSet.Period.Start}: {oldState} -> {value}");
            }
            get { return this.state; }
        }

        public void Prepare() { UnityEngine.Debug.Log("Generic"); }

        /// <summary>
        /// Initializes a new representation.
        /// </summary>
        /// 
        /// <param name="id">A unique ID.</param>
        /// <param name="uri">The target URI.</param>
        /// <param name="bandwidth">The expected bandwidth in bytes per
        /// second.</param>
        /// <param name="parent">The parent adaptation set.</param>
        public GenericRepresentation(string id, Uri uri, long bandwidth, AdaptationSet parent)
        {
            this.ID = id;
            this.URI = uri;
            this.Bandwidth = bandwidth;
            this.AdaptationSet = parent;
        }

        /// <summary>
        /// Initializes a new representation by copying another one.
        /// </summary>
        /// <param name="representation">The representation to copy
        /// from.</param>
        public GenericRepresentation(GenericRepresentation representation)
        {
            this.ID = representation.ID;
            this.URI = representation.URI;
            this.Bandwidth = representation.Bandwidth;
            this.AdaptationSet = representation.AdaptationSet;
            this.state = representation.State;
            this.data = representation.Data;
        }
    }
}