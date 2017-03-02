/*
 * Remote Link - Copyright 2017 ABIOMED, Inc.
 * --------------------------------------------------------
 * Description:
 * KeepAliveTimer.cs: Keep Alive Timer
 * --------------------------------------------------------
 * Author: Alessandro Agnello 
*/

using System;
using System.Timers;

namespace Abiomed.Models
{
    public class KeepAliveTimer
    {
        public Timer _timer;
        public string _identifier = string.Empty;
        public event EventHandler ThresholdReached;

         public KeepAliveTimer(string identifier, int keepAliveTimer)
        {            
            _timer = new Timer(keepAliveTimer);            
            _timer.AutoReset = false;
            _timer.Elapsed += SendCancelRequest;
            _identifier = identifier;
            _timer.Enabled = true;
        }

        public void UpdateTimer()
        {                        
            _timer.Stop();
            _timer.Start();
        }

        public void DestroyTimer()
        {
            _timer.Stop();
            _timer.Dispose();
        }

        private void SendCancelRequest(object sender, ElapsedEventArgs e)
        {
            _timer.Enabled = false;
            _timer.Stop();

            CommunicationsEvent eventArgs = new CommunicationsEvent();
            eventArgs.Identifier = _identifier;

            // Send request to stop service
            ThresholdReached?.Invoke(sender, eventArgs);
        }
    }
}
