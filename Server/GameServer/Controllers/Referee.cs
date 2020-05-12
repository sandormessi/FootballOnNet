namespace GameServer.Controllers
{
    using System;

    using GameServer.Models;

    public class Referee
    {
        private static Position PositionOfHomeGoal { get; } = new Position { X = 0, Y = 250 };
        private static Position PositionOfAwayGoal { get; } = new Position { X = 500, Y = 250 };

        public Referee(PositionCollection homePositionCollection, PositionCollection awayPositionCollection)
        {
            HomePositionCollection = homePositionCollection ?? throw new ArgumentNullException(nameof(homePositionCollection));
            AwayPositionCollection = awayPositionCollection ?? throw new ArgumentNullException(nameof(awayPositionCollection));
        }

        public PositionCollection HomePositionCollection { get; }
        public PositionCollection AwayPositionCollection { get; }

        public Position BallPosition { get; set; }

        public bool IsGoal(out bool isHome)
        {
            isHome = false;

            foreach (Position homePosition in HomePositionCollection.Positions)
            {
                if ((homePosition == PositionOfAwayGoal) && (BallPosition == homePosition))
                {
                    isHome = true;
                    return true;
                }
            }

            foreach (Position awayPosition in AwayPositionCollection.Positions)
            {
                if ((awayPosition == PositionOfHomeGoal) && (BallPosition == awayPosition))
                {
                    isHome = false;
                    return true;
                }
            }

            return false;
        }
    }
}
