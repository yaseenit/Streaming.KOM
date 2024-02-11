using System;
using System.Collections.Generic;
using UnityEngine;

namespace KOM.DASHLib
{
    /// <summary>
    /// An abstract class that adapts the media of a <c>Timeline</c> by
    /// some kind metric.
    /// </summary>
    public abstract class AdaptationPolicy : MonoBehaviour
    {
        /// <summary>
        /// Adapts the given list of <c>Timeslot</c>s by doing nothing by default
        /// and should be overriden.
        ///
        /// <param name="window">The target to adapt.</param>
        /// </summary>
        public abstract void Adapt(List<Timeslot> window);
    }
}