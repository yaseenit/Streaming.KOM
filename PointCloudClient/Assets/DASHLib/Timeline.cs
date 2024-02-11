using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace KOM.DASHLib
{
    /// <summary>
    /// A class representing the media timeline.
    /// </summary>
    public class Timeline
    {
        /// <summary>
        /// A list of <c>TimeSlot> objects that form the timeline.
        /// </summary>
        public List<Timeslot> Timeslots = new List<Timeslot>();
        public event EventHandler<SeekEventArgs> SeekEvent;
        private int playbackIndex = 0;
        public int CurrentIndex
        {
            get
            {
                return playbackIndex;// this.findIndex(this.PlaybackTime);
            }
            set
            {
                if(value < 0 || value >= this.Timeslots.Count)
                {
                    throw new IndexOutOfRangeException($"Index {value} is out of range [0, {this.Timeslots.Count-1}]");
                }

                int oldValue = this.playbackIndex;
                if(value != oldValue)
                {
                    this.playbackIndex = value;
                    this.SeekEvent?.Invoke(this, new SeekEventArgs(oldValue, this.playbackIndex));
                }
            }
        }

        public Timeslot CurrentTimeslot
        {
            get
            {
                return this.Timeslots[this.CurrentIndex];
            }
        }

        public long TotalPlaybackDuration
        {
            get
            {
                return this.Timeslots.Last().End;
            }
        }
        public Timeline()
        {
        }

        public Timeline(MPD mpd, List<AContext> contexts)
        {
            this.Prepare(mpd, contexts);
        }

        public void Prepare(MPD mpd, List<AContext> contexts)
        {
            this.Timeslots = new List<Timeslot>(mpd.Periods.Count);

            for (int i = 0; i < mpd.Periods.Count; i++)
            {
                // Setup timeline for point clouds
                List<List<IRepresentation>> representations = new List<List<IRepresentation>>();
                foreach (AdaptationSet adaptationSet in mpd.Periods[i].AdaptationSets)
                {
                    foreach (AContext context in contexts)
                    {
                        if (context.SupportedMimeTypes.Any(mimeType => mimeType.Contains(adaptationSet.MimeType)))
                        {
                            List<IRepresentation> alternatives = new List<IRepresentation>();
                            foreach (IRepresentation representation in adaptationSet.Representations)
                            {
                                alternatives.Add(context.Register(representation));
                            }
                            representations.Add(alternatives);
                            break;
                        }
                    }
                    // TODO: Handle case of no fitting context
                }
                Timeslot timeslot = new Timeslot(mpd.Periods[i].Start, mpd.Periods[i].End, representations.ToArray());
                this.Timeslots.Add(timeslot);
            }

            // Set neighbouring timeslots. First and last slot wrap around.
            for (int i = 0; i < this.Timeslots.Count; i++)
            {
                Timeslot timeslot = this.Timeslots[i];
                if (i == 0)
                {
                    timeslot.Previous = this.Timeslots.Last();
                }
                else
                {
                    timeslot.Previous = this.Timeslots[i - 1];
                }

                if (i == mpd.Periods.Count - 1)
                {
                    timeslot.Next = this.Timeslots[0];
                }
                else
                {
                    timeslot.Next = this.Timeslots[i + 1];
                }
            }

            this.CurrentIndex = 0;
        }

        public int FindIndex(long milliseconds)
        {
            for(int i = 0; i < this.Timeslots.Count; i++)
            {
                if(this.Timeslots[i].Start <= milliseconds && milliseconds <= this.Timeslots[i].End)
                {
                    return i;
                }
            }

            throw new IndexOutOfRangeException($"Time {milliseconds} is out of range [{this.Timeslots[0].Start}, {this.Timeslots[0].End}]");
        }

        public string Stringify(int width=80, string empty=" ", string index=">", string played="#", string prepared="|", string buffered=".")
        {
            int timelineLength = this.Timeslots.Count;
            int cursorIndex = this.CurrentIndex * width / timelineLength;

            string bar = "";
            for (int i = 0; i < width-2; i++)
            {
                int timelineIndex = timelineLength * i / width;
                if (i == cursorIndex)
                {
                    bar += $"{index}";
                }
                else if (this.Timeslots[timelineIndex].IsPlayed())
                {
                    bar += $"{played}";
                }
                else if (this.Timeslots[timelineIndex].IsPrepared())
                {
                    bar += $"{prepared}";
                }
                else if (this.Timeslots[timelineIndex].IsBuffered())
                {
                    bar += $"{buffered}";
                }
                else
                {
                    bar += $"{empty}";
                }
            }
            return $"[{bar}]";
        }

        public long GetForwardBufferDuration()
        {
            List<Timeslot> forwardBuffer = GetForwardBuffer();
            long forwardBufferDuration = 0;
            foreach(Timeslot timeslot in forwardBuffer)
            {
                forwardBufferDuration += timeslot.Duration;
            }
            return forwardBufferDuration;
        }

        public List<Timeslot> GetForwardBuffer()
        {
            int bufferedTimeslots = 0;
            long bufferDuration = 0;
            Timeslot timeslot = this.Timeslots[this.CurrentIndex];
            List<Timeslot> forwardBuffer = new List<Timeslot>();
            while (timeslot.IsBuffered() && bufferDuration < this.TotalPlaybackDuration)
            {
                bufferedTimeslots++;
                bufferDuration += timeslot.Duration;
                forwardBuffer.Add(timeslot);
                timeslot = timeslot.Next;
            }
            return forwardBuffer;
        }

        public List<Timeslot> GetForwardHorizon(long maxDuration)
        {
            if(maxDuration > this.TotalPlaybackDuration)
            {
                throw new ArgumentException("Forward horizon must be smaller or equal then the total media duration.");
            }

            long horizonDuration = 0;
            Timeslot timeslot = this.Timeslots[this.CurrentIndex];
            List<Timeslot> forwardHorizon = new List<Timeslot>();
            while (horizonDuration < maxDuration)
            {
                horizonDuration += timeslot.Duration;
                forwardHorizon.Add(timeslot);
                timeslot = timeslot.Next;
            }
            return forwardHorizon;
        }

        public long GetTotalBufferDuration()
        {
            long bufferDuration = 0;
            foreach (Timeslot timeslot in this.Timeslots)
            {
                if(timeslot.IsBuffered())
                {
                    bufferDuration += timeslot.Duration;
                }
            }
            return bufferDuration;
        }

        public void Advance()
        {
            if (this.CurrentIndex + 1 < this.Timeslots.Count)
            {
                this.CurrentIndex += 1;
            }
            else
            {
                this.CurrentIndex = 0;
            }
        }
    }

    /// <summary>
    /// A helper class to ease handling media periods.
    /// </summary>
    public class Timeslot
    {
        /// <summary>
        /// Starting time of this timeslot in milliseconds.
        /// </summary>
        public long Start;

        /// <summary>
        /// Ending time of this timeslot in milliseconds.
        /// </summary>
        public long End;

        /// <summary>
        /// Duration of this timeslot in milliseconds.
        /// </summary>
        public long Duration { get { return End - Start; } }

        /// <summary>
        /// An array of containing a list of alternatives per
        /// adaptation set.
        /// </summary>
        public List<IRepresentation>[] Alternatives;

        private IRepresentation[] selections;
        /// <summary>
        /// An array containing the representations that should be used
        /// for playback.
        /// </summary>
        public IRepresentation[] Selections
        {
            get
            {
                return this.selections;
            }
            set
            {
                for(int i = 0; i < this.selections.Length; i++)
                {
                    // Unbuffer values that have changed
                    if(this.selections[i] != value[i])
                    {
                        this.selections[i].State = ERepresentationState.Unbuffered;
                        this.selections[i].Data = null;
                    }
                    else
                    {
                        value[i] = this.selections[i];
                    }
                    this.selections = value;
                }
            }


        }

        public float[] RelativeQualities
        {
            get
            {
                float[] qualities = new float[Selections.Length];

                for(int i = 0; i < Selections.Length; i++)
                {
                    if(Alternatives[i].Count == 1)
                    {
                        qualities[i] = 1.0f;
                    }
                    else
                    {
                        qualities[i] = (float)Alternatives[i].FindIndex(s => s.Equals(Selections[i])) / (float)(Alternatives[i].Count - 1);
                    }
                }
                return qualities;
            }
        }

        public float RelativeMeanQuality
        {
            get
            {
                float[] qualities = RelativeQualities;
                float sum = 0;
                foreach(float quality in qualities)
                {
                    sum += quality;
                }

                return sum / (float)qualities.Length;
            }
        }

        public Timeslot Next = null;
        public Timeslot Previous = null;

        public ERepresentationState State {
            get {
                ERepresentationState state = ERepresentationState.Played;
                foreach (IRepresentation media in Selections)
                {
                    if (media.State < state)
                    {
                        state = media.State;
                    }
                }
                return state;

            }
        }

        /// <summary>
        /// Check if all representations from <cref>Selections</cref>
        /// have finished playback.
        /// </summary>
        /// <returns>Return <c>True</c> if all representations have be
        /// played, else <c>False</c>.</returns>
        public bool IsPlayed()
        {
            foreach (IRepresentation media in Selections)
            {
                if (media.State < ERepresentationState.Played)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Check if all representations from <cref>Selections</cref>
        /// have been prepared for playback.
        /// </summary>
        /// <returns>Return <c>True</c> if all representations have be
        /// prepared, else <c>False</c>.</returns>
        public bool IsPrepared()
        {
            foreach(IRepresentation media in Selections)
            {
                if(media.State < ERepresentationState.Prepared)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Check if all representations from <cref>Selections</cref>
        /// have finished buffering.
        /// </summary>
        /// <returns>Return <c>True</c> if all representations have be
        /// been buffered, else <c>False</c>.</returns>
        public bool IsBuffered()
        {
            foreach (IRepresentation media in Selections)
            {
                if (media.State < ERepresentationState.Buffered)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Create a new <c>TimeSlot</c> instance.
        /// </summary>
        /// 
        /// <param name="start">Starting time of this timeslot in milliseconds.</param>
        /// <param name="end">Ending time of this timeslot in milliseconds.</param>
        /// <param name="alternatives">A list of <c>IRepresentation</c>
        /// objects that may be used for playback and can be selected by
        /// setting <cref>Selections</cref>.</param>
        public Timeslot(long start, long end, List<IRepresentation>[] alternatives)
        {
            this.Start = start;
            this.End = end;
            
            this.Alternatives = alternatives;
            this.selections = new IRepresentation[Alternatives.Length];
            for(int i = 0; i < Alternatives.Length; i++)
            {
                this.Selections[i] = this.Alternatives[i].First();
            }
        }

        public void Unbuffer()
        {
            foreach(IRepresentation representation in this.Selections)
            {
                representation.State = ERepresentationState.Unbuffered;
                representation.Data = null;
            }
        }
    }

    public class SeekEventArgs
    {
        public int OldIndex {get; private set;}= 0;
        public int NewIndex {get; private set;} = 0;

        public SeekEventArgs(int oldIndex, int newIndex)
        {
            this.OldIndex = oldIndex;
            this.NewIndex = newIndex;
        }
    }
}
