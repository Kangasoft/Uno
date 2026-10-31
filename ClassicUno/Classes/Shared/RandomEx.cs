/* Classic Uno 2026
 * Version: 1.0.1
 * By: Jason James Newland
 * ©2026 - Kangasoft Software */
using System;

namespace ClassicUno.Classes.Shared
{
    public static class RandomEx
    {
        /* A single global generator used ONLY to create unique seeds for new threads.
         * It must be locked because System.Random is not thread-safe. */
        private static readonly Random GlobalSeeder = new Random();
        private static readonly object Lock = new object();

        /* Each thread gets its own isolated instance of Random. */
        [ThreadStatic]
        private static Random _localInstance;

        public static Random Shared
        {
            get
            {
                if (_localInstance != null)
                {
                    return _localInstance;
                }
                int seed;
                /* Lock only during the quick operation of grabbing a seed */
                lock (Lock)
                {
                    seed = GlobalSeeder.Next();
                }
                /* Initialize the thread-local instance with the unique seed */
                _localInstance = new Random(seed);
                return _localInstance;
            }
        }
    }
}
