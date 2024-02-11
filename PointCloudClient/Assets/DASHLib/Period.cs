using System;
using System.Collections.Generic;

namespace KOM.DASHLib
{
    /// <summary>
    /// This class represents a DASH media period with a defined place on the timeline.
    /// Also holds a list of adaptation sets available in this period.
    /// </summary>
    public class Period
    {
        /// <summary>
        /// The starting time of the period in milliseconds.
        /// </summary>
        public long Start {get;}

        /// <summary>
        /// The ending time of the period in milliseconds.
        /// </summary>
        public long End {get;}

        /// <summary>
        /// The duration of the period in milliseconds.
        /// </summary>
        public long Duration
        {
            get {return End-Start;}
        }

        /// <summary>
        /// A list of adaptation sets in this period that may be used
        /// for playback
        /// </summary>
        public List<AdaptationSet> AdaptationSets = new List<AdaptationSet>();

        /// <summary>
        /// Initializes a new DASH <c>Period</c> with the specified start and end time in milliseconds.
        /// </summary>
        /// <param name="start">The start of the period in milliseconds.</param>
        /// <param name="end">The end of the period in milliseconds.</param>
        public Period(long start, long end)
        {
            if (start >= end)
            {
                throw new ArgumentException("Period must start before it ends");
            }
            this.Start = start;
            this.End = end;
        }
    }
}