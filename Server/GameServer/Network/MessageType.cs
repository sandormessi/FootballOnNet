namespace GameServer.Network
{
    /// <summary>Represents the type of message the the clients will process.</summary>
    public enum MessageType : byte
    {
        OverallMatchData,
        BallPosition,
        PositionCollection,
        Team,
        Pitch,
        MatchResult,
    }
}
