namespace GameServer.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using GameServer.Models.Message.InitialMessages;

    /// <summary>Represents the Game itself.</summary>
    public class Game
    {
        private readonly object syncObject = new object();

        private readonly Position positionOfHomeGoal;
        private readonly Position positionOfAwayGoal;

        public Game(Pitch pith, Team homeTeam, Team awayTeam, MatchTime time)
        {
            Pith = pith ?? throw new ArgumentNullException(nameof(pith));
            HomeTeam = homeTeam ?? throw new ArgumentNullException(nameof(homeTeam));
            AwayTeam = awayTeam ?? throw new ArgumentNullException(nameof(awayTeam));
            Time = time ?? throw new ArgumentNullException(nameof(time));
            HomePositions = new PositionCollection();
            AwayPositions = new PositionCollection();

            // THIS SHOULD BE FIXED LATER
            positionOfHomeGoal = new Position { X = 1, Y = 1 };
            positionOfAwayGoal = new Position { X = 2, Y = 2 };
            Ball = new Ball();
        }

        public Ball Ball { get; }

        public Position BallPosition { get; private set; } = new Position { X = 50, Y = 50 };

        /// <summary>Gets the positions of the <see cref="Player"/> objects int he home team.</summary>
        public PositionCollection HomePositions { get; private set; }
        /// <summary>Gets the positions of the <see cref="Player"/> objects int the away team.</summary>
        public PositionCollection AwayPositions { get; private set; }

        /// <summary>Gets or set the pith of this <see cref="Game"/>.</summary>
        public Pitch Pith { get; }

        /// <summary>Gets or sets the <see cref="Team"/> who plays at home.</summary>
        public Team HomeTeam { get; }

        /// <summary>Gets or sets the <see cref="Team"/> who plays away from home.</summary>
        public Team AwayTeam { get; }

        /// <summary>Gets the time of the game (match).</summary>
        public MatchTime Time { get; }

        /// <summary>Gets or sets the scores that the two teams Scored.</summary>
        public List<Score> Scores { get; } = new List<Score>();

        public void ProcessPositionCollection(PositionCollection positionCollection, bool isHome)
        {
            if (positionCollection == null)
            {
                throw new ArgumentNullException(nameof(positionCollection));
            }

            if (isHome)
            {
                HomePositions = positionCollection;
            }
            else
            {
                AwayPositions = positionCollection;
            }

            DetermineScore();
            DetermineBallPossessor();
        }
        public void ProcessBallPosition(Position ballPosition)
        {
            BallPosition = ballPosition ?? throw new ArgumentNullException(nameof(ballPosition));

            lock (syncObject)
            {
                DetermineScore();
                DetermineBallPossessor();
            }
        }



        private void DetermineScore()
        {
            IEnumerable<Position> allPosition = AwayPositions.Positions.Union(HomePositions.Positions);

            if ((BallPosition == positionOfHomeGoal) || (BallPosition == positionOfAwayGoal))
            {
                DetermineBallPossessor();
                Scores.Add(new Score { Scorer = Ball.Owner });
            }
        }
        private void DetermineBallPossessor()
        {
            IEnumerable<Position> allPosition = AwayPositions.Positions.Union(HomePositions.Positions);
            Position ballPossessor = allPosition.SingleOrDefault(x => x == BallPosition);
            if (ballPossessor is null)
            {
                return;
            }
            IEnumerable<Player> allPlayers = HomeTeam.Players.Union(AwayTeam.Players);
            Ball.Owner = allPlayers.Single(x => x.Id == ballPossessor.Id);
        }
    }
}
