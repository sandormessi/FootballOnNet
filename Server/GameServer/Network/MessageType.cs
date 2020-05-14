namespace GameServer.Network
{
    /// <summary>Represents the type of message the the clients will process.</summary>
    public enum MessageType : byte
    {
        /// <summary>The overall current result of the match.</summary>
        OverallMatchData,
        /// <summary>The current position of the ball.</summary>
        BallPosition,
        /// <summary>Collection of positions.</summary>
        PositionCollection,
        /// <summary>The away team.</summary>
        Team,
        /// <summary>The pitch.</summary>
        Pitch,
        /// <summary>The final result at the end of the match.</summary>
        MatchResult,
    }
}
