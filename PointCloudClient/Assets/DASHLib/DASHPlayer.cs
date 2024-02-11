using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

using UnityEngine;

namespace KOM.DASHLib
{
    public class DASHPlayer : MonoBehaviour
    {
        /// <summary>
        /// A fully qualified URL targeting a MPD file.
        /// </summary>
        [Tooltip("A fully qualified URL targeting a MPD file.")]
        public string URI = "";

        /// <summary>
        /// Start media playback immediatly if true.
        /// </summary>
        [Tooltip("Start media playback immediatly if true.")]
        public bool StartImmediatly = false;

        /// <summary>
        /// Context where playback will take place.
        /// </summary>
        [Tooltip("Context where playback will take place.")]
        public List<AContext> Contexts;

        /// <summary>
        /// AdaptationPolicy to use for rate control. Defaults to LowestBandwidthPolicy if unset.
        /// </summary>
        [Tooltip("AdaptationPolicy to use for rate control. Defaults to LowestBandwidthPolicy if unset.")]
        public AdaptationPolicy AdaptationPolicy;

        /// <summary>
        /// Loop playback if true. Otherwise stop playback if reaching end of timeline.
        /// </summary>
        [Tooltip("Loop playback if true. Otherwise stop playback if reaching end of timeline.")]
        public bool ShouldLoop = true;

        /// <summary>
        /// Print a textual representation of the timeline to the console at each update.
        /// </summary>
        [Tooltip("Print a textual representation of the timeline to the console at each update.")]
        public bool ShouldPrintTimeline = false;

        /// <summary>
        /// Total duration of buffered media elements to be played in the future.
        /// </summary>
        [Tooltip("Total duration of buffered media elements to be played in the future.")]
        public long MaxForwardBufferDuration = 3000;

        /// <summary>
        /// Service to handle media buffering requests.
        /// </summary>
        [Tooltip("Service to handle media buffering requests.")]
        public BufferingService BufferingService;

        /// <summary>
        /// Service to handle media preparation requests.
        /// </summary>
        [Tooltip("Service to handle media preparation requests.")]
        public PreparationService PreparationService;

        /// <summary>
        /// Service to handle media playback requests.
        /// </summary>
        [Tooltip("Service to handle media playback requests.")]
        public PlaybackService PlaybackService;

        /// <summary>
        /// Fired if the player state changes.
        /// </summary>
        public event EventHandler<StateChangedEventArgs> StateChangedEvent;

        /// <summary>
        /// Fired if preparation of a media element is requested.
        /// </summary>
        public event EventHandler<TimeslotEventArgs> PreparationRequestEvent;

        /// <summary>
        /// Fired if playback of a media element is requested.
        /// </summary>
        public event EventHandler<TimeslotEventArgs> PlaybackRequestEvent;

        /// <summary>
        /// Fired if buffering of a media element is requested.
        /// </summary>
        public event EventHandler<TimeslotEventArgs> BufferRequestEvent;

        /// <summary>
        /// The current state of the DASH player.
        /// </summary>
        public EDASHPlayerState State {get; private set;} = EDASHPlayerState.Stopped;

        /// <summary>
        /// BandwidthMeter instance used for performance tracking.
        /// </summary>
        public BandwidthMeter BandwidthMeter;

        /// <summary>
        /// Path to save metrics in the csv format at. Logging will be disabled if the path is empty.
        /// </summary>
        [Tooltip("Path to save metrics in the csv format at. Logging will be disabled if the path is empty.")]
        public string MetricsLogPath = "";

        private Timeline timeline = null;
        private Stopwatch requestStopwatch = new Stopwatch();
        private Stopwatch metricsStopwatch = new Stopwatch();
        private StreamWriter metricsWriter;

        public DASHPlayer()
        {
        }

        void Start()
        {
            SimpleTimer.ShowDirectly = false; // Disable timing printing
            this.Prepare(new Uri(this.URI));

            // Set up the event plumbing
            this.BufferRequestEvent += this.BufferingService.OnBufferingRequest;
            this.PlaybackRequestEvent += this.PlaybackService.OnPlaybackRequestEventHandler;
            this.PreparationRequestEvent += this.PreparationService.OnPreparationRequestEvent;

            this.PlaybackService.PreparationRequestEvent += this.PreparationService.OnPreparationRequestEvent;
            this.PlaybackService.PreparationRequestEvent += this.onPlaybackRequestEventHandler;
            this.PlaybackService.PlaybackEndEvent += this.OnPlaybackEndEventHandler;

            this.PreparationService.PreparationEndEvent += this.PlaybackService.OnPreparationEndEventHandler;
            this.PreparationService.BufferRequestEvent += this.BufferingService.OnBufferingRequest;

            this.BufferingService.BufferEndEvent += this.PreparationService.OnBufferEndEvent;
            this.BufferingService.BufferEndEvent += this.onBufferEndEventHandler;

            this.timeline.SeekEvent += this.OnSeekEventHandler;

            if (StartImmediatly)
            {
                this.Play();
            }

            if (MetricsLogPath != "")
            {
                metricsStopwatch.Start();
                UnityEngine.Debug.Log($"Writing metrics at {this.MetricsLogPath}");
                this.metricsWriter = new StreamWriter(this.MetricsLogPath);
                string row = "time (ms);period_index;player_state;media_state;relative_mean_quality;bandwidth (bytes/s);forward_buffer_duration (ms)";
                metricsWriter.WriteLine(row);
                UnityEngine.Debug.Log(row);
            }
        }

        private void Update()
        {
            if(ShouldPrintTimeline)
            {
                UnityEngine.Debug.Log(this.timeline.Stringify());
            }
            string row = $"{metricsStopwatch.ElapsedMilliseconds};{this.timeline.CurrentIndex};{this.State};{this.timeline.CurrentTimeslot.State};{this.timeline.CurrentTimeslot.RelativeMeanQuality};{this.BandwidthMeter.Bandwidth};{this.timeline.GetForwardBufferDuration()}";
            this.metricsWriter?.WriteLine(row);
            UnityEngine.Debug.Log(row);
        }

        /// <summary>
        /// Prepare the player by downloading the MPD file and setting up the timeline.
        /// </summary>
        /// <param name="uri">An URI targetting a MPD file</param>
        private void Prepare(Uri uri)
        {
            UnityEngine.Debug.Log($"Parse MPD from {uri.AbsoluteUri}");
            MPD mpd = new MPD(DataSourceFactory.Create(uri.Scheme).GetText(uri));
            UnityEngine.Debug.Log($"Found media of length {mpd.MediaPresentationDuration}ms");

            UnityEngine.Debug.Log("Setting up timeline");
            this.timeline = new Timeline(mpd, this.Contexts);

            if(this.MaxForwardBufferDuration > this.timeline.TotalPlaybackDuration)
            {
                UnityEngine.Debug.Log("Setting MaxForwardBufferDuration to TotalPlaybackDuration because it exceeds it");
                this.MaxForwardBufferDuration = this.timeline.TotalPlaybackDuration;
            }
            
        }

        public void Play()
        {
            if(this.State != EDASHPlayerState.Playing)
            {
                StateChangedEventArgs args = new StateChangedEventArgs(this.State, EDASHPlayerState.Playing);
                this.State = EDASHPlayerState.Playing;
                this.StateChangedEvent?.Invoke(this, args);

                StartCoroutine(ScheduleTimeslot(this.timeline.CurrentTimeslot));
            }
        }

        public void Pause()
        {
            if (this.State != EDASHPlayerState.Paused)
            {
                StateChangedEventArgs args = new StateChangedEventArgs(this.State, EDASHPlayerState.Paused);
                this.State = EDASHPlayerState.Paused;
                this.StateChangedEvent?.Invoke(this, args);
            }
        }

        public void Stop()
        {
            if(this.State != EDASHPlayerState.Stopped)
            {
                StateChangedEventArgs args = new StateChangedEventArgs(this.State, EDASHPlayerState.Stopped);
                this.State = EDASHPlayerState.Stopped;
                this.StateChangedEvent?.Invoke(this, args);

                this.timeline.CurrentIndex = 0;
                foreach (AContext context in this.Contexts)
                {
                    context.Reset();
                }                
            }
        }

        private void OnPlaybackEndEventHandler(object sender, TimeslotEventArgs args)
        {
            this.requestStopwatch.Stop();

            if(this.State != EDASHPlayerState.Playing)
            {
                return;
            }

            if(this.timeline.CurrentIndex == this.timeline.Timeslots.Count-1)
            {
                if(this.ShouldLoop)
                {
                    this.timeline.Advance();
                }
                else
                {
                    this.Stop();
                }
            }
            else
            {
                this.timeline.Advance();
            }

            args.Timeslot.Unbuffer();
            if(this.timeline.GetForwardBufferDuration()+args.Timeslot.Next.Duration < this.MaxForwardBufferDuration && this.timeline.GetForwardBuffer().Count > 0)
            {
                this.BufferRequestEvent?.Invoke(this, new TimeslotEventArgs(this.timeline.GetForwardBuffer().Last().Next));
            }
        }

        private void OnSeekEventHandler(object sender, SeekEventArgs args)
        {
            if (this.State == EDASHPlayerState.Playing)
            {
                long actual = requestStopwatch.Elapsed.Milliseconds;
                long expected = timeline.Timeslots[args.OldIndex].Duration;
                long diff = expected - actual;
                long timeToPlay = diff >= 0 ? diff : 0;

                Timeslot timeslot = this.timeline.Timeslots[args.NewIndex];
                StartCoroutine(ScheduleTimeslot(timeslot, timeToPlay));
            }
        }

        private IEnumerator ScheduleTimeslot(Timeslot slot, long offset=0)
        {
            yield return new WaitForSeconds((float)offset / 1000f);
            this.PlaybackRequestEvent?.Invoke(this, new TimeslotEventArgs(slot));
        }

        private void onPlaybackRequestEventHandler(object sender, TimeslotEventArgs args)
        {
            this.requestStopwatch.Reset();
            this.requestStopwatch.Start();
        }

        private void onBufferEndEventHandler(object sender, TimeslotEventArgs args)
        {
            if (this.State == EDASHPlayerState.Stopped)
            {
                return;
            }
            this.AdaptationPolicy.Adapt(this.timeline.GetForwardHorizon(this.MaxForwardBufferDuration));

            Timeslot timeslot = this.timeline.CurrentTimeslot;
            if(this.timeline.GetForwardBufferDuration() + args.Timeslot.Next.Duration > this.MaxForwardBufferDuration)
            {
                this.PreparationRequestEvent?.Invoke(this, new TimeslotEventArgs(timeslot));
                return;
            }
            else
            {
                this.BufferRequestEvent?.Invoke(this, new TimeslotEventArgs(args.Timeslot.Next));
            }
        }
    }



    public class StateChangedEventArgs : EventArgs
    {
        public EDASHPlayerState OldState { get; private set; }
        public EDASHPlayerState NewState { get; private set; }

        public StateChangedEventArgs(EDASHPlayerState oldState, EDASHPlayerState newState)
        {
            this.OldState = oldState;
            this.NewState = newState;
        }
    }

    public class TimeslotEventArgs : EventArgs
    {
        public Timeslot Timeslot { get; private set; }

        public TimeslotEventArgs(Timeslot timeslot)
        {
            this.Timeslot = timeslot;
        }
    }

    public class IRepresentationEventArgs : EventArgs
    {
        public IRepresentation Representation { get; private set; }

        public IRepresentationEventArgs(IRepresentation representation)
        {
            this.Representation = representation;
        }
    }
}