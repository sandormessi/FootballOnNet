namespace GameServer.Models
{
    using System;

    using GameServer.Models.Internal;

    internal class TeamDataController
    {
        private readonly TeamData teamData1;
        private readonly TeamData teamData2;

        public TeamDataController(TeamData teamData1, TeamData teamData2)
        {
            this.teamData1 = teamData1 ?? throw new ArgumentNullException(nameof(teamData1));
            this.teamData2 = teamData2 ?? throw new ArgumentNullException(nameof(teamData2));
            this.teamData1.TeamIsReady += TeamData1_TeamIsReady;
            this.teamData2.TeamIsReady += TeamData2_TeamIsReady;   
        }

        private void TeamData2_TeamIsReady(object sender, EventArgs e)
        {
            if (teamData1.TeamReady)
            {
                OnTeamsAreReady();
            }
        }

        private void TeamData1_TeamIsReady(object sender, EventArgs e)
        {
            if (teamData2.TeamReady)
            {
                OnTeamsAreReady();
            }
        }

        public event EventHandler TeamsAreReady;

        protected virtual void OnTeamsAreReady()
        {
            TeamsAreReady?.Invoke(this, EventArgs.Empty);
        }
    }
}
