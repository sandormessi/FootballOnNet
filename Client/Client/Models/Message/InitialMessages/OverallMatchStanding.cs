namespace Client.Models.Message.InitialMessages
{
    public class OverallMatchStanding
    {
        public MatchStanding Standing { get; set; } = new MatchStanding();
        public MatchTime TimeElapsed { get; set; }
        public PositionCollection PositionCollection { get; set; } = new PositionCollection();
        public Position BallPosition { get; set; }
    }
}
