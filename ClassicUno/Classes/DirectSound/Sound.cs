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
    public class Sound
    {
        private Timer _play;
        private IBasicAudio _audio;
        private IGraphBuilder _graph;
        private IMediaControl _media;
        private int _pan;
        private IMediaPosition _position;
        private int _volume;

        public event Action<Sound> OnMediaEnded;
        public event Action<Sound, double> OnMediaPositionChanged;

        public string Clip { get; set; }

        /* Constructor/destuctor */
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

        ~Sound()
        {
            Close();
        }

        /* Public properties */
        public int Volume
        {
            get => _volume;
            set
            {
                _volume = value;
                if (_volume < 0)
                {
                    _volume = 0;
                }
                if (_volume > 100)
                {
                    _volume = 100;
                }
                _audio?.SetVolume((_volume - 100)*50);
            }
        }

        public int Pan
        {
            get => _pan;
            set
            {
                _pan = value;
                if (_pan < 0)
                {
                    _pan = 0;
                }
                if (_pan > 100)
                {
                    _pan = 100;
                }
                _audio?.SetBalance((_pan - 50)*200);
            }
        }

        public double Position
        {
            get
            {
                _position.GetCurrentPosition(out var pos);
                return pos;
            }
            set
            {
                _position.GetDuration(out var duration);
                if (value <= duration)
                {
                    _position.SetCurrentPosition(value);
                }
            }
        }

        public double Duration
        {
            get
            {
                _position.GetDuration(out var duration);
                return duration;
            }
        }

        /* Public methods */
        public void PlayAsync(bool noEvents = false)
        {
            /* This does NOT check for cross-threading, so if events are raised, it's up to the code at the other
             * end to do Invoke/BeginInvoke (if dealing with UI) */
            var t = new Thread(() => Play(noEvents)) {IsBackground = true};
            t.Start();
        }

        public void Play(bool noEvents = false)
        {
            Close();
            if (!Open() || _media == null || _media.Run() < 0)
            {
                return;
            }
            try
            {
                _audio.SetVolume((_volume - 100) * 50);
                _audio.SetBalance((_pan - 50) * 200);
                /* This parameter is used if the sound being played is only a few 100ms; it prevents the timer from ever being created,
                 * or firing the events. This causes exceptions and sometimes even the sound not to be heard at all. This is because of
                 * the Stop() function being called within the timer if the position == length, and at 200ms time interval, still causes
                 * problems... if the file is longer than a second, no problem! */
                if (!noEvents)
                {
                    _play = new Timer(TimerTick, null, 0, 200);
                }
            }
            catch (Exception)
            {
                /* Silently ignore - this will sometimes happen when playing short files (1sec or so) in quick succession */
                Debug.Assert(true);
            }
        }

        public void Stop()
        {
            _media?.Stop();
            if (_play == null)
            {
                return;
            }
            _play.Dispose();
            _play = null;
        }

        public void Pause()
        {
            _media?.Pause();
           if (_play == null)
           {
               return;
           }
           _play.Dispose();
           _play = null;
        }

        public void Resume()
        {
            if (_media?.Run() < 0)
            {
                return;
            }
            _play = new Timer(TimerTick, null, 0, 200);
        }

        /* Private helper methods */    
        private bool Open()
        {
            bool flag;
            if (string.IsNullOrEmpty(Clip) || !File.Exists(Clip))
            {
                return false;
            }
            object o = null;
            try
            {
                var typeFromClsid = Type.GetTypeFromCLSID(ClsId.FilterGraph);
                if (typeFromClsid == null)
                {
                    return false;
                }
                o = Activator.CreateInstance(typeFromClsid);
                _graph = (IGraphBuilder) o;
                o = null;
                if (_graph.RenderFile(Clip, null) < 0)
                {
                    return false;
                }
                _media = _graph;
                _position = (IMediaPosition) _graph;
                _audio = _graph as IBasicAudio;                   
                flag = true;
            }
            catch (Exception)
            {
                flag = false;
            }
            finally
            {
                if (o != null)
                {
                    Marshal.ReleaseComObject(o);
                }
            }
            return flag;
        }

        private void Close()
        {
            _position = null;
            _media = null;
            _audio = null;
            if (_graph != null)
            {
                try
                {
                    Marshal.ReleaseComObject(_graph);
                }
                catch (Exception)
                {
                    /* Silently ignore - this will sometimes happen when playing short files (1sec or so) in quick succession */
                    Debug.Assert(true);
                }
                _graph = null;
            }
            GC.Collect();
        }

        /* Timer callback for getting the current position of the streaming audio */
        private void TimerTick(object sender)
        {
            double pos;
            double duration;
            try
            {
                _position.GetCurrentPosition(out pos);
                _position.GetDuration(out duration);
            }
            catch (Exception)
            {
                /* Silently ignore - this will sometimes happen when playing short files (1sec or so) in quick succession */
                return;
            }
            OnMediaPositionChanged?.Invoke(this, pos);
            if (pos < duration)
            {
                return;
            }
            Stop();
            OnMediaEnded?.Invoke(this);
        }
    }
}