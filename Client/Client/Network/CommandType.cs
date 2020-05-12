﻿namespace FootballClient.Network
{
    public enum CommandType : byte
    {
        /// <summary>Data will be queried.</summary>
        Get,
        /// <summary>Data will be queried, continuously.</summary>
        ContinuousGet,
        /// <summary>Data will be set.</summary>
        Set,
        /// <summary>Data will be set, continuously.</summary>
        ContinuousSet
    }
}