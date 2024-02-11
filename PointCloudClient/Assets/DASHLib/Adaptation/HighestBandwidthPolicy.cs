using System.Collections.Generic;
using System.Linq;

namespace KOM.DASHLib
{
    /// <summary>
    /// A class to adapt the timeline by choosing the highest bandwidth.
    /// </summary>
    public class HighestBandwidthPolicy : AdaptationPolicy
    {
        /// <summary>
        /// Adapts the timeline by always choosing the highest bandwidth.
        ///
        /// <param name="timeline">The target to adapt.</param>
        /// </summary>
        override
        public void Adapt(List<Timeslot> window)
        {
            for(int i = 0; i < window.Count; i++)
            {
                Timeslot timeslot = window[i];
                for(int j = 0; j < timeslot.Alternatives.Length; j++)
                {
                    List<IRepresentation> alternatives = timeslot.Alternatives[j];
                    IRepresentation selection = alternatives.First();
                    foreach (IRepresentation alternative in alternatives)
                    {
                        if (alternative.Bandwidth > selection.Bandwidth)
                        {
                            selection = alternative;
                        }
                    }
                    timeslot.Selections[j] = selection;
                }
            }
        }
    }
}