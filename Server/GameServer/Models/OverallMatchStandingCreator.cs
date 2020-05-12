namespace GameServer.Models
{
    using GameServer.Models.Message.InitialMessages;

    internal static class OverallMatchStandingCreator
    {
        internal static OverallMatchStanding Create(Game game, MatchTimer timer, bool isHome)
        {
            MatchStanding matchStanding = MatchStandingCreator.Create(game);

            var standing = new OverallMatchStanding
            {
                Scores = game.Scores,
                Standing = matchStanding,
                TimeElapsed = new MatchTime { Time = timer.RemainingTime },
                PositionCollection = isHome ? game.HomePositions : game.AwayPositions,
                BallPosition = game.BallPosition
            };

            return standing;
        }
    }
}
