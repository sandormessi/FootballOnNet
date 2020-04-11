namespace FootballClient.Models.Message.InitialMessages
{
    using System.Collections.Generic;

    public class OverallMatchStanding
    {
        public MatchStanding Standing { get; set; } = new MatchStanding();
        public MatchTime TimeElapsed { get; set; }
        public PositionCollection PositionCollection { get; set; } = new PositionCollection();
        public Position BallPosition { get; set; }
        public List<Score> Scores { get; set; } = new List<Score>();
    }
}
