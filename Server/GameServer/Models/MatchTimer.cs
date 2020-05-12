using System.Timers;

namespace GameServer.Models
{
    using System;

    public class MatchTimer
    {
        private readonly Timer timer;
        private DateTime startDate;

        public MatchTimer(int time)
        {
            timer = new Timer(time) 
            {
                AutoReset = false
            };

            timer.Elapsed += Timer_Elapsed;
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            OnTimeElapsed(e);
        }

        public void Start()
        {
            timer.Start();
            startDate = DateTime.Now;
        }

        public int RemainingTime
        {
            get
            {
                TimeSpan remainingTime = DateTime.Now - startDate;
                return Convert.ToInt32(remainingTime.TotalSeconds);
            }
        }

        public event ElapsedEventHandler TimeElapsed;

        protected virtual void OnTimeElapsed(ElapsedEventArgs e)
        {
            TimeElapsed?.Invoke(this, e);
        }
    }
}
