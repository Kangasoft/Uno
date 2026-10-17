/* Classic Uno 2026
 * Version: 1.0
 * By: Jason James Newland
 * ©2026 - Kangasoft Software */
using System;
using System.ComponentModel;

namespace ClassicUno.Classes.UI
{
    public class UiSynchronize
    {
        /* This class eliminates the need for delegates when calling InvokeRequired/BeginInvoke invocation on UI objects */
        private readonly ISynchronizeInvoke _sync;

        public UiSynchronize(ISynchronizeInvoke sync)
        {
            _sync = sync;
        }

        public void Execute(Action action)
        {
            /* It shouldn't be null, as the constructor forces us to use a synchronous object */
            _sync?.BeginInvoke(action, null);
        }
    }
}
