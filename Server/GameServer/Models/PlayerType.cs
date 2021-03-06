﻿namespace GameServer.Models
{
    using GameServer.Models.Message.InitialMessages;

    /// <summary>Represents the position of a <see cref="Player"/> in a <see cref="Team"/>.</summary>
    public enum PlayerType
    {
        Goalkeeper,
        Defender,
        Midfielder,
        Attacker
    }
}