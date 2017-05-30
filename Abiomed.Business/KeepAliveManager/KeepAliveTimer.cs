/*
 * Remote Link - Copyright 2017 ABIOMED, Inc.
 * --------------------------------------------------------
 * Description:
 * KeepAliveTimer.cs: Keep Alive Timer Class with custom callback for elapsed event
 * --------------------------------------------------------
 * Author: Alessandro Agnello 
*/
using System;
using System.Timers;

namespace Abiomed.Business
{
    public class KeepAliveTimer
    {
        private Timer _timer;
        private string _identifier = string.Empty;

        public KeepAliveTimer(string identifier, int keepAliveTimer, Action<object, ElapsedEventArgs, string> timerExpired)
        {
            _timer = new Timer(keepAliveTimer);
            _timer.AutoReset = false;
            _timer.Elapsed += (sender, e) => timerExpired(sender, e, identifier);
            _timer.Enabled = true;
        }

        public void PingTimer()
        {
            _timer.Stop();
            _timer.Start();
        }

        public void DestroyTimer()
        {
            _timer.Stop();
            _timer.Dispose();
        }
    }
}
