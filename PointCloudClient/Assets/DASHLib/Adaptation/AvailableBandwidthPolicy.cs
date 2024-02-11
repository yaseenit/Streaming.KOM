using System.Collections.Generic;
using System.Linq;

namespace KOM.DASHLib
{
    /// <summary>
    /// A class to adapt the timeline by choosing the highest
    /// streameable bandwidth.
    /// </summary>
    public class AvailableBandwidthPolicy : AdaptationPolicy
    {
        public BandwidthMeter BandwidthMeter;
        public AvailableBandwidthPolicy(BandwidthMeter meter)
        {
            this.BandwidthMeter = meter;
        }

        /// <summary>
        /// Adapts the timeline by always choosing the media with the
        /// highest bandwidth, but below the available bandwidth.
        ///
        /// <param name="timeline">The target to adapt.</param>
        /// </summary>
        override
        public void Adapt(List<Timeslot> window)
        {
            float availableBandwidth = this.BandwidthMeter.Bandwidth;
            for(int i = 1; i < window.Count; i++)
            {
                Timeslot timeslot = window[i];
                IRepresentation[] selections = new IRepresentation[timeslot.Selections.Length];
                for (int j = 0; j < timeslot.Alternatives.Length; j++)
                {
                    List<IRepresentation> alternatives = timeslot.Alternatives[j];
                    IRepresentation selection = alternatives.First();
                    
                    foreach (IRepresentation alternative in alternatives)
                    {
                        if (alternative.Bandwidth > selection.Bandwidth && alternative.Bandwidth < availableBandwidth)
                        {
                            selection = alternative;
                        }
                    }
                    selections[j] = selection;
                }
                timeslot.Selections = selections;
            }
        }
    }
}