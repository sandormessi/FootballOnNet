namespace GameServer.Network
{
    /// <summary>Represents the type of message the the clients will process.</summary>
    public enum MessageType : byte
    {
        OverallMatchData,
        Position,
        BallPosition,
        PositionCollection,
        Team,
        RemainingMatchTime,
        MatchTime,
        Pitch,
        Score,
        MatchResult,
        StatusReport
    }
}
