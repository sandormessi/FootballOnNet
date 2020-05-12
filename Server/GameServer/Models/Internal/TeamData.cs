namespace GameServer.Models.Internal
{
    using System;

    using GameServer.Models.Message.InitialMessages;

    internal class TeamData
    {
        private bool teamReady;

        public int MessageRead { get; set; }
        public Team Team { get; set; }
        public PositionCollection PositionCollection { get; set; }

        public bool TeamReady
        {
            get
            {
                return teamReady;
            }
            set
            {
                if (value)
                {
                    OnTeamIsReady();
                }
                teamReady = value;
            }
        }

        public bool DataSendingInProgress { get; set; }
        public bool DataSendingStopRequest { get; set; }

        public event EventHandler TeamIsReady;

        protected virtual void OnTeamIsReady()
        {
            TeamIsReady?.Invoke(this, EventArgs.Empty);
        }
    }
}
