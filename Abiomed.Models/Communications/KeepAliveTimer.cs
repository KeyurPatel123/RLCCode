using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;

namespace Abiomed.Models
{
    public class KeepAliveTimer
    {
        public Timer _timer;
        public string _identifier = string.Empty;
        public event EventHandler ThresholdReached;

        public KeepAliveTimer(string identifier)
        {            
            _timer = new Timer(5000);            
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
