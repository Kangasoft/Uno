/* Classic Uno 2026
 * Version: 1.0.1
 * By: Jason James Newland
 * ©2026 - Kangasoft Software */
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using ClassicUno.Classes.DirectSound.Core;

namespace ClassicUno.Classes.DirectSound
{
    public class Sound : IDisposable
    {
        #region Fields
        private Timer _play;
        private IBasicAudio _audio;
        private IGraphBuilder _graph;
        private IMediaControl _media;
        private int _pan;
        private IMediaPosition _position;
        private int _volume;

        private readonly object _sync = new object();
        private int _playbackVersion;

        private int _ending;
        private bool _disposed;

        public event Action<Sound> OnMediaEnded;
        public event Action<Sound, double> OnMediaPositionChanged;
        #endregion

        #region Constructors
        public Sound()
        {
            _volume = 100;
            _pan = 50;
        }

        public Sound(string fileName)
        {
            Clip = fileName;
            _volume = 100;
            _pan = 50;
        }
        #endregion

        #region Properties
        public string Clip { get; set; }

        public int Volume
        {
            get
            {
                lock (_sync)
                {
                    return _volume;
                }
            }
            set
            {
                lock (_sync)
                {
                    _volume = Math.Max(0, Math.Min(100, value));
                    _audio?.SetVolume((_volume - 100) * 50);
                }
            }
        }

        public int Pan
        {
            get
            {
                lock (_sync)
                {
                    return _pan;
                }
            }
            set
            {
                lock (_sync)
                {
                    _pan = Math.Max(0, Math.Min(100, value));
                    _audio?.SetBalance((_pan - 50) * 200);
                }
            }
        }

        public double Position
        {
            get
            {
                lock (_sync)
                {
                    if (_disposed || _position == null)
                    {
                        throw new InvalidOperationException("Cannot get the position when no media is open.");
                    }

                    _position.GetCurrentPosition(out var pos);
                    return pos;
                }
            }
            set
            {
                lock (_sync)
                {
                    if (_disposed || _position == null)
                    {
                        throw new InvalidOperationException("Cannot set the position when no media is open.");
                    }

                    _position.GetDuration(out var duration);

                    if (value >= 0 && value <= duration)
                    {
                        _position.SetCurrentPosition(value);
                    }
                }
            }
        }

        public double Duration
        {
            get
            {
                lock (_sync)
                {
                    if (_disposed || _position == null)
                    {
                        throw new InvalidOperationException("Cannot get the duration when no media is open.");
                    }

                    _position.GetDuration(out var duration);
                    return duration;
                }
            }
        }
        #endregion

        #region Public methods
        public void PlayAsync(bool noEvents = false)
        {
            /* This does NOT check for cross-threading, so if events are raised, it's up to the code at the other
             * end to do Invoke/BeginInvoke (if dealing with UI) */
            var t = new Thread(() => Play(noEvents)) {IsBackground = true};
            t.Start();
        }

        public void Play(bool noEvents = false)
        {
            lock (_sync)
            {
                if (_disposed)
                {
                    return;
                }

                CloseLocked();

                if (!OpenLocked())
                {
                    return;
                }

                try
                {
                    if (_media == null || _media.Run() < 0)
                    {
                        CloseLocked();
                        return;
                    }

                    _ending = 0;

                    _audio?.SetVolume((_volume - 100) * 50);
                    _audio?.SetBalance((_pan - 50) * 200);

                    if (!noEvents)
                    {
                        _play = new Timer(TimerTick, null, 0, 200);
                    }
                }
                catch (Exception)
                {
                    StopLocked();
                    CloseLocked();
                    Debug.Assert(true);
                }
            }
        }

        public void Stop()
        {
            lock (_sync)
            {
                StopLocked();
            }
        }

        public void Pause()
        {
            lock (_sync)
            {
                if (_disposed || _media == null)
                {
                    return;
                }

                try
                {
                    _media.Pause();

                    _play?.Dispose();
                    _play = null;

                    /* Invalidate any position reading already in progress. */
                    _playbackVersion++;
                }
                catch (Exception)
                {
                    Debug.Assert(true);
                }
            }
        }

        public void Resume()
        {
            lock (_sync)
            {
                if (_disposed || _media == null)
                {
                    return;
                }

                try
                {
                    /* Avoid creating multiple timers. */
                    if (_play != null || _media.Run() < 0)
                    {
                        return;
                    }
                    _play = new Timer(TimerTick, null, 0, 200);
                }
                catch (Exception)
                {
                    Debug.Assert(true);
                }
            }
        }

        public void Dispose()
        {
            lock (_sync)
            {
                if (_disposed)
                {
                    return;
                }

                _disposed = true;
                CloseLocked();
            }
        }
        #endregion

        #region Helper methods
        /* Thread-safe graph manipulation for DirectShow */
        private bool OpenLocked()
        {
            if (_disposed || string.IsNullOrEmpty(Clip) || !File.Exists(Clip))
            {
                return false;
            }

            object graphObject = null;

            try
            {
                var graphType = Type.GetTypeFromCLSID(ClsId.FilterGraph);

                if (graphType == null)
                {
                    return false;
                }

                graphObject = Activator.CreateInstance(graphType);

                _graph = (IGraphBuilder)graphObject;
                graphObject = null;

                if (_graph.RenderFile(Clip, null) < 0)
                {
                    CloseLocked();
                    return false;
                }

                _media = _graph;
                _position = (IMediaPosition)_graph;
                _audio = (IBasicAudio)_graph;

                return true;
            }
            catch (Exception)
            {
                CloseLocked();
                Debug.Assert(true);
                return false;
            }
            finally
            {
                if (graphObject != null &&
                    Marshal.IsComObject(graphObject))
                {
                    Marshal.ReleaseComObject(graphObject);
                }
            }
        }

        private void StopLocked()
        {
            _playbackVersion++;

            var timer = _play;
            _play = null;
            timer?.Dispose();

            try
            {
                _media?.Stop();
            }
            catch (Exception)
            {
                Debug.Assert(true);
            }
        }

        private void CloseLocked()
        {
            _playbackVersion++;

            var timer = _play;
            _play = null;
            timer?.Dispose();

            try
            {
                _media?.Stop();
            }
            catch (Exception)
            {
                Debug.Assert(true);
            }

            _position = null;
            _media = null;
            _audio = null;

            if (_graph == null)
            {
                return;
            }

            try
            {
                Marshal.ReleaseComObject(_graph);
            }
            catch (Exception)
            {
                Debug.Assert(true);
            }

            _graph = null;
        }
        #endregion

        #region Timer
        /* Timer callback for getting the current position of the streaming audio */
        private void TimerTick(object sender)
        {
            double pos;
            double duration;
            int version;

            lock (_sync)
            {
                if (_disposed || _position == null || _graph == null)
                {
                    return;
                }

                version = _playbackVersion;

                try
                {
                    _position.GetCurrentPosition(out pos);
                    _position.GetDuration(out duration);
                }
                catch (Exception)
                {
                    return;
                }
            }

            /* Never raise external events while holding _sync. */
            OnMediaPositionChanged?.Invoke(this, pos);

            if (pos < duration)
            {
                return;
            }

            lock (_sync)
            {
                /* Playback may have stopped or been replaced during the event. */
                if (_disposed || version != _playbackVersion || _position == null || _graph == null)
                {
                    return;
                }

                /* Ensure only one callback handles the end of this track. */
                if (Interlocked.Exchange(ref _ending, 1) != 0)
                {
                    return;
                }

                _playbackVersion++;

                var timer = _play;
                _play = null;
                timer?.Dispose();

                try
                {
                    _media?.Stop();
                }
                catch (Exception)
                {
                    Debug.Assert(true);
                }
            }

            /* The music manager can dispose this Sound and start the next track. */
            OnMediaEnded?.Invoke(this);
        }
        #endregion
    }
}