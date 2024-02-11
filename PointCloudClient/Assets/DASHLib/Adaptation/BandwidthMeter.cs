using System;
using System.Diagnostics;
using System.Collections.Generic;

using UnityEngine;

namespace KOM.DASHLib
{
    /// <summary>
    /// A class to help measuring the bandwidth.
    /// </summary>
    public class BandwidthMeter : MonoBehaviour
    {
        private float bandwidth = 0.0f;
        /// <summary>
        /// The measured bandwidth in bytes/second.
        /// </summary>
        public float Bandwidth
        {
            get {return bandwidth;}
        }

        public event EventHandler Measured;

        private Dictionary<string, Stopwatch> downloads = new Dictionary<string, Stopwatch>();

        /// <summary>
        /// Starts the bandwidth measurement with the given id.
        /// </summary>
        /// <param name="id">A unique id of the measurement.</param>
        public void BeginMeasurement(string id)
        {
            if(this.downloads.ContainsKey(id))
            {
                this.downloads.Remove(id);
            }
            this.downloads.Add(id, new Stopwatch());
            this.downloads[id].Start();
        }

        /// <summary>
        /// Stops the bandwidth measurement with the given id and calculates
        /// the bandwidth.
        /// </summary>
        /// 
        /// <param name="id">The unique id of the measurement.</param>
        /// <param name="bytes">The number of bytes received since start.</param>
        public void EndMeasurement(string id, long bytes)
        {
            if(!this.downloads.ContainsKey(id))
            {
                return;
            }
            this.downloads[id].Stop();
            this.bandwidth = (float)((float)bytes / (float)this.downloads[id].Elapsed.Milliseconds * 1000.0f); // bytes/second
            this.downloads.Remove(id);
            this.Measured?.Invoke(this, EventArgs.Empty);
        }
    }
}