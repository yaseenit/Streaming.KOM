using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using UnityEngine;
using Unity.Collections;
using Unity.Jobs;

namespace KOM.DASHLib
{
    public class BufferingService : MonoBehaviour
    {
        public event EventHandler<TimeslotEventArgs> BufferStartEvent;
        public event EventHandler<TimeslotEventArgs> BufferEndEvent;
        public BandwidthMeter BandwidthMeter;
        private Queue<Timeslot> bufferQueue = new Queue<Timeslot>();

        public void Update()
        {
            if(bufferQueue.Count > 0)
            {
                StartCoroutine(this.bufferTimeslot(this.bufferQueue.Dequeue()));
            }
        }
        public void OnBufferingRequest(object sender, TimeslotEventArgs args)
        {
            this.bufferQueue.Enqueue(args.Timeslot);
        }

        private IEnumerator bufferTimeslot(Timeslot timeslot)
        {
            if(timeslot.IsBuffered())
            {
                yield break;
            }

            this.BufferStartEvent?.Invoke(this, new TimeslotEventArgs(timeslot));

            foreach (IRepresentation representation in timeslot.Selections)
            {
                if (representation.State > ERepresentationState.Unbuffered)
                {
                    continue;
                }

                this.BandwidthMeter?.BeginMeasurement(representation.URI.ToString());

                representation.State = ERepresentationState.Buffering;
                BufferJob job = new BufferJob
                {
                    Uri = ToNativeString(representation.URI.AbsoluteUri),
                    Data = new NativeQueue<byte>(Allocator.Persistent)
                };
                JobHandle handle = job.Schedule();
                yield return new WaitUntil(() => handle.IsCompleted);

                handle.Complete();

                NativeArray<byte> nativeData = job.Data.ToArray(Allocator.Temp);
                representation.Data = nativeData.ToArray();

                nativeData.Dispose();
                job.Uri.Dispose();
                job.Data.Dispose();
                representation.State = ERepresentationState.Buffered;


                this.BandwidthMeter?.EndMeasurement(representation.URI.ToString(), representation.Data.Length);
            }
            this.BufferEndEvent?.Invoke(this, new TimeslotEventArgs(timeslot));
        }
        private static NativeArray<byte> ToNativeString(string s)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(s);
            NativeArray<byte> native = new NativeArray<byte>(bytes.Length, Allocator.Persistent);
            native.CopyFrom(bytes);
            return native;
        }

        private static string FromNativeString(NativeArray<byte> bytes)
        {
            return Encoding.ASCII.GetString(bytes.ToArray());
        }

        private struct BufferJob : IJob
        {
            public NativeArray<byte> Uri;
            public NativeQueue<byte> Data;
            public void Execute()
            {
                Uri uri = new Uri(FromNativeString(Uri));
                byte[] data = DataSourceFactory.Create(uri.Scheme).GetBytes(uri);
                foreach(byte b in data)
                {
                    this.Data.Enqueue(b);
                }
            }
        }
    }
}