namespace GameServer.Models
{
    using System.Linq;

    public static class MatchStandingCreator
    {
        public static MatchStanding Create(Game game)
        {
            int homeGoals = game.Scores.Count(x => game.HomeTeam.Players.FirstOrDefault(y => y.ID == x.Scorer.ID) != null);
            int awayGoals = game.Scores.Count(x => game.AwayTeam.Players.FirstOrDefault(y => y.ID == x.Scorer.ID) != null);

            return new MatchStanding { HomeGoals = homeGoals, AwayGoals = awayGoals };
        }
    }
}
