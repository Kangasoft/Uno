/* Classic Uno 2026
 * Version: 1.0
 * By: Jason James Newland
 * ©2026 - Kangasoft Software */
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ClassicUno.Classes.DirectSound;
using ClassicUno.Classes.Logic.Player;
using ClassicUno.Classes.Settings.SettingsData;

namespace ClassicUno.Classes.Helpers.Management
{
    public enum VoiceType
    {
        Pass = 0,
        Uno = 1
    }

    public enum SoundType
    {
        Shuffle = 0,
        Deal = 1,
        Drop = 2,
        Draw = 3
    }

    internal class AudioData
    {
        public SoundType Type { get; set; }

        public Sound Player { get; set; }
    }

    internal class VoiceData
    {
        public int VoiceIndex { get; set; }

        public VoiceType VoiceType { get; set; }

        public PlayerGender Gender { get; set; }

        public Sound Voice { get; set; }
    }

    public static class AudioManager
    {
        /* Easier way to manage and play external sounds */
        private static readonly Random RandomVoice = new Random();

        private static readonly List<VoiceData> Voices = new List<VoiceData>();
        private static readonly List<AudioData> Sounds = new List<AudioData>();
        private static readonly List<string> Music = new List<string>();

        private static int _voiceVolume;
        private static int _effectsVolume;
        private static int _musicVolume;

        private static Sound _music;
        private static int _musicIndex;

        public static void Initialize()
        {
            var voiceSearch = new FolderSearch();
            voiceSearch.OnFileFound += VoiceSearchFileFound;

            Sounds.AddRange(
                new[]
                {
                    LoadSound(Utils.MainDir(@"\data\sound\card-shuffle.wav"), SoundType.Shuffle),
                    LoadSound(Utils.MainDir(@"\data\sound\card-deal.wav"), SoundType.Deal),
                    LoadSound(Utils.MainDir(@"\data\sound\card-drop.wav"), SoundType.Drop),
                    LoadSound(Utils.MainDir(@"\data\sound\card-draw.wav"), SoundType.Draw)
                });

            SetVoiceVolume(SettingsManager.Settings.Options.Sounds.VoiceVolume);
            SetEffectsVolume(SettingsManager.Settings.Options.Sounds.EffectsVolume);
            SetMusicVolume(SettingsManager.Settings.Options.Sounds.MusicVolume);

            var voice = new DirectoryInfo(Utils.MainDir(@"data\voice\", false));
            voiceSearch.BeginSearch(voice, "*.mp3", "*", false);

            var musicSearch = new FolderSearch();
            musicSearch.OnFileSearchCompleted += MusicSearchCompleted;
            musicSearch.OnFileFound += MusicSearchFileFound;

            var music = new DirectoryInfo(Utils.MainDir(@"data\music\", false));
            musicSearch.BeginSearch(music, "*.mp3", "*", false);
        }

        public static void SetVoiceVolume(int volume)
        {
            _voiceVolume = volume;
            if (Voices.Count == 0)
            {
                return;
            }
            foreach (var s in Voices.Where(s => s.Voice != null))
            {
                s.Voice.Volume = volume;
            }
        }

        public static void SetEffectsVolume(int volume)
        {
            _effectsVolume = volume;
            if (Sounds.Count == 0)
            {
                return;
            }
            foreach (var s in Sounds.Where(s => s.Player != null))
            {
                s.Player.Volume = volume;
            }
        }

        public static void SetMusicVolume(int volume)
        {
            _musicVolume = volume;
            if (Sounds.Count == 0 || _music == null)
            {
                return;
            }
            _music.Volume = volume;
        }

        public static void PlayVoice(VoiceType type, IPlayer player)
        {
            if (!SettingsManager.Settings.Options.Sounds.EnableVoice)
            {
                return;
            }
            foreach (var v in Voices.Where(v => v.VoiceType == type && v.VoiceIndex == player.VoiceIndex && v.Gender == player.NameData.Gender))
            {
                if (v.Voice.Pan != player.AudioPan)
                {
                    /* Set panning of voice ONCE */
                    v.Voice.Pan = player.AudioPan;
                }
                v.Voice?.PlayAsync(true);
                return;
            }
        }

        public static void PlayEffect(SoundType type)
        {
            if (!SettingsManager.Settings.Options.Sounds.EnableEffects)
            {
                return;
            }
            foreach (var s in Sounds.Where(s => s.Type == type))
            {
                s.Player?.PlayAsync(true);
                return;
            }
        }

        public static void PlayMusic(bool next = false)
        {
            if (!SettingsManager.Settings.Options.Sounds.EnableMusic || Music.Count == 0)
            {
                return;
            }
            if (next)
            {
                _musicIndex++;
                if (_musicIndex > Music.Count - 1)
                {
                    _musicIndex = 0;
                    Music.Shuffle();
                }
            }
            _music = new Sound(Music[_musicIndex]) { Volume = _musicVolume };
            _music.OnMediaEnded += OnMusicEnd;
            _music.PlayAsync();
        }

        public static void PauseMusic()
        {
            _music?.Pause();
        }

        public static void ResumeMusic()
        {
            _music?.Resume();
        }

        public static void StopMusic()
        {
            _music?.Stop();
        }

        private static void VoiceSearchFileFound(string file)
        {
            /* Parse file name */
            var fileName = Path.GetFileNameWithoutExtension(file).ToUpper();
            var parts = fileName.Split(new[] {'-'}, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 3)
            {
                return;
            }

            if (!int.TryParse(parts[1], out var index))
            {
                return;
            }
            PlayerGender gender;
            switch (parts[0])
            {
                case "MALE":
                    gender = PlayerGender.Male;
                    break;

                default:
                    /* Female (|) */
                    gender = PlayerGender.Female;
                    break;
            }

            VoiceType type;
            switch (parts[2])
            {
                case "PASS":
                    type = VoiceType.Pass;
                    break;

                default:
                    /* UNO! */
                    type = VoiceType.Uno;
                    break;
            }
            /* Create the class */
            var vd = new VoiceData
            {
                VoiceIndex = index - 1,
                Gender = gender,
                VoiceType = type,
                Voice = new Sound(file) {Pan = 50, Volume = _voiceVolume}
            };
            Voices.Add(vd);
        }

        public static int GetRandomVoice(PlayerGender gender)
        {
            if (Voices.Count == 0)
            {
                return -1;
            }
            /* Find random voice index */
            var match = Voices.Where(o => o.Gender == gender).ToList();
            if (match.Count == 0)
            {
                return -1;
            }
            return match[RandomVoice.Next(match.Count)].VoiceIndex;
        }

        /* Music playback callback */
        private static void OnMusicEnd(Sound sound)
        {
            _music.OnMediaEnded -= OnMusicEnd;
            /* Get next track */
            _musicIndex++;
            if (_musicIndex > Music.Count - 1)
            {
                _musicIndex = 0;
                Music.Shuffle();
            }
            _music = new Sound(Music[_musicIndex]) { Volume = _musicVolume };
            _music.OnMediaEnded += OnMusicEnd;
            _music.PlayAsync();
        }

        private static void MusicSearchFileFound(string file)
        {
            Music.Add(file);
        }

        private static void MusicSearchCompleted(FolderSearch search)
        {
            Music.Shuffle();
            /* Begin music playback */
            PlayMusic();
        }

        /* Private load method */
        private static AudioData LoadSound(string file, SoundType type)
        {
            var data = new AudioData { Type = type };
            if (File.Exists(file))
            {
                data.Player = new Sound(file) { Volume = _effectsVolume };
            }
            return data;
        }
    }
}
