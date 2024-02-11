using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace KOM.DASHLib
{
    public class PlaybackService : MonoBehaviour
    {
        public event EventHandler<TimeslotEventArgs> PlaybackStartEvent;
        public event EventHandler<TimeslotEventArgs> PlaybackEndEvent;
        public event EventHandler<TimeslotEventArgs> PreparationRequestEvent;
        public PointCloudContext Context;

        private List<Timeslot> requested = new List<Timeslot>();
        
        public PlaybackService()
        {
        }

        public void OnPlaybackRequestEventHandler(object sender, TimeslotEventArgs args)
        {
            if(args.Timeslot.IsPrepared())
            {
                StartCoroutine(this.Playback(args.Timeslot));
            }
            else
            {
                this.requested.Add(args.Timeslot);
                this.PreparationRequestEvent?.Invoke(this, new TimeslotEventArgs(args.Timeslot));
            }
        }

        public void OnPreparationEndEventHandler(object sender, TimeslotEventArgs args)
        {
            if (this.requested.Contains(args.Timeslot)) // Play only if it was requested to be played
            {
                this.requested.Remove(args.Timeslot);
                StartCoroutine(this.Playback(args.Timeslot));
            }
            
        }

        private IEnumerator Playback(Timeslot slot)
        {
            this.PlaybackStartEvent?.Invoke(this, new TimeslotEventArgs(slot));
            this.Context.Reset();
            foreach(var representation in slot.Selections)
            {
                this.Context.Render(representation);
                yield return null;
            }
            this.PlaybackEndEvent?.Invoke(this, new TimeslotEventArgs(slot));
        }
    }
}