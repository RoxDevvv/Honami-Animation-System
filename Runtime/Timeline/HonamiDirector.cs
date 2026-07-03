using UnityEngine;

namespace HonamiAnimationSystem.Runtime.Timeline
{
    /// <summary>
    /// Runtime player that samples a <see cref="HonamiTimeline"/> against scene objects.
    /// </summary>
    [AddComponentMenu("Honami/Honami Director")]
    public sealed class HonamiDirector : MonoBehaviour
    {
        public HonamiTimeline timeline;
        public bool playOnAwake = true;
        public bool loop = false;

        private float _time;
        private bool _playing;

        private void Start()
        {
            if (playOnAwake)
            {
                Play();
            }
        }

        private void Update()
        {
            if (!_playing || timeline == null)
            {
                return;
            }

            _time += Time.deltaTime;
            float duration = timeline.GetDuration();

            if (_time >= duration)
            {
                if (loop)
                {
                    _time %= duration;
                    ResetEventFiredFlags();
                }
                else
                {
                    _time = duration;
                    _playing = false;
                }
            }

            SampleTimeline(_time);
        }

        /// <summary>
        /// Starts timeline playback from the beginning.
        /// </summary>
        public void Play()
        {
            _time = 0f;
            _playing = true;
            ResetEventFiredFlags();
        }

        /// <summary>
        /// Pauses playback at the current timeline time.
        /// </summary>
        public void Pause() => _playing = false;

        /// <summary>
        /// Resumes playback from the current timeline time.
        /// </summary>
        public void Resume() => _playing = true;

        /// <summary>
        /// Stops playback and resets the timeline time to zero.
        /// </summary>
        public void Stop()
        {
            _playing = false;
            _time = 0f;
        }

        /// <summary>
        /// Moves the director to an absolute timeline time and samples it immediately.
        /// </summary>
        public void Seek(float time)
        {
            _time = Mathf.Clamp(time, 0f, timeline != null ? timeline.GetDuration() : 0f);
            SampleTimeline(_time);
        }

        private void SampleTimeline(float time)
        {
            if (timeline == null)
            {
                return;
            }

            foreach (var track in timeline.tracks)
            {
                if (track.muted)
                {
                    continue;
                }

                if (track.trackType == HonamiTimelineTrackType.Animation)
                {
                    SampleAnimationTrack(track, time);
                }
                else
                {
                    FireEvents(track, time);
                }
            }
        }

        private void SampleAnimationTrack(HonamiTimelineTrack track, float time)
        {
            if (track.target == null)
            {
                return;
            }

            if (!track.target.TryGetComponent<Animator>(out _))
            {
                return;
            }

            foreach (var clip in track.clips)
            {
                if (clip.clip == null)
                {
                    continue;
                }

                float localTime = time - clip.startTime;
                if (localTime < 0f || localTime > clip.GetDuration())
                {
                    continue;
                }

                clip.clip.SampleAnimation(track.target, clip.GetSampleTime(localTime));
            }
        }

        private void FireEvents(HonamiTimelineTrack track, float time)
        {
            foreach (var evt in track.events)
            {
                if (evt.hasFired || time < evt.time)
                {
                    continue;
                }

                evt.hasFired = true;

                if (evt.eventType == HonamiTimelineEventType.Broadcast)
                {
                    SendMessage(evt.eventId, SendMessageOptions.DontRequireReceiver);
                }
                else
                {
                    evt.onFire?.Invoke();
                }
            }
        }

        private void ResetEventFiredFlags()
        {
            if (timeline == null)
            {
                return;
            }

            foreach (var track in timeline.tracks)
            {
                foreach (var evt in track.events)
                {
                    evt.hasFired = false;
                }
            }
        }
    }
}
