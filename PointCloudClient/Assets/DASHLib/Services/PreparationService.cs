using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace KOM.DASHLib
{
    public class PreparationService : MonoBehaviour
    {
        public event EventHandler<TimeslotEventArgs> PreparationStartEvent;
        public event EventHandler<TimeslotEventArgs> PreparationEndEvent;
        public event EventHandler<TimeslotEventArgs> BufferRequestEvent;

        private List<Timeslot> requested = new List<Timeslot>();
        public PreparationService()
        {
        }

        public void OnPreparationRequestEvent(object sender, TimeslotEventArgs args)
        {
            if (this.requested.Contains(args.Timeslot))
            {
                return;
            }

            if(args.Timeslot.IsBuffered())
            {
                this.requested.Add(args.Timeslot);
                StartCoroutine(this.prepareTimeslot(args.Timeslot));
                this.requested.Remove(args.Timeslot);
            }
            else
            {
                this.BufferRequestEvent?.Invoke(this, new TimeslotEventArgs(args.Timeslot));
            }
        }

        public void OnBufferEndEvent(object sender, TimeslotEventArgs args)
        {
            StartCoroutine(this.prepareTimeslot(args.Timeslot));
        }

        private IEnumerator prepareTimeslot(Timeslot slot)
        {
            if(slot.IsPrepared())
            {
                yield break;
            }

            this.PreparationStartEvent?.Invoke(this, new TimeslotEventArgs(slot));
            foreach(IRepresentation representation in slot.Selections)
            {
                representation.Prepare();
                yield return null;
            }
            this.PreparationEndEvent?.Invoke(this, new TimeslotEventArgs(slot));
        }
    }
}