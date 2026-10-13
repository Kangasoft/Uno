/* Classic Uno 2026
 * Version: 1.0
 * By: Jason James Newland
 * ©2026 - Kangasoft Software */
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace ClassicUno.Classes
{
    public class FolderSearch
    {
        private const string DefaultFileMask = "*.*";
        private const string DefaultDirectoryMask = "*";

        private DirectoryInfo _initialDirectory;

        private List<string> _directoryMasks;
        private List<string> _fileMasks;

        private bool _recursive;

        public event Action<string> OnFileFound;
        public event Action<FolderSearch> OnFileSearchCompleted;

        /* Constructor */
        public FolderSearch()
        {
            _directoryMasks = new List<string>();
            _fileMasks = new List<string>();
        }

        /* Search methods */
        public void BeginSearch(DirectoryInfo initialDirectory, string fileMask, string directoryMask, bool recursive)
        {
            if (initialDirectory != null)
            {
                _initialDirectory = initialDirectory;
            }
            else
            {
                return;
            }
            _fileMasks = string.IsNullOrEmpty(fileMask) ? new List<string> { DefaultFileMask } : ParseMask(fileMask);
            _directoryMasks = string.IsNullOrEmpty(directoryMask)
                ? new List<string> { DefaultDirectoryMask }
                : ParseMask(directoryMask);
            _recursive = recursive;
            /* Start background thread */
            void Starter() => ThreadSearch(_initialDirectory);
            var t = new Thread(Starter)
            {
                IsBackground = true
            };
            t.Start();
        }

        private void BeginSearch(DirectoryInfo baseDirectory)
        {
            if (!baseDirectory.Exists)
            {
                return;
            }
            try
            {
                foreach (
                    var f in
                        _fileMasks.Select(baseDirectory.GetFiles)
                            .SelectMany(files => files.Where(f => OnFileFound != null)))
                {
                    OnFileFound?.Invoke(f.FullName);
                }
            }
            catch (AccessViolationException)
            {
                /* Quietly handle exception */
            }
            try
            {
                if (!_recursive)
                {
                    return;
                }
                var directories = new List<DirectoryInfo>();
                foreach (var dm in _directoryMasks)
                {
                    directories.AddRange(baseDirectory.GetDirectories(dm));
                }
                foreach (var di in directories)
                {
                    BeginSearch(di);
                }
            }
            catch (AccessViolationException)
            {
                /* Quietly handle exception */
            }
        }

        /* Private methods */
        private static List<string> ParseMask(string mask)
        {
            if (string.IsNullOrEmpty(mask))
            {
                return null;
            }
            mask = mask.Trim(';');
            if (mask.Length == 0)
            {
                return null;
            }
            var masks = new List<string>();
            masks.AddRange(mask.Split(';'));
            return masks;
        }

        private void ThreadSearch(DirectoryInfo directory)
        {
            BeginSearch(directory);
            OnFileSearchCompleted?.Invoke(this);
        }
    }
}
