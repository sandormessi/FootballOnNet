namespace Client.Models.Message.InitialMessages
{
    using System.Collections.Generic;

    /// <summary>Represents a Team in a <see cref="Game"/>.</summary>
    public class Team
    {
        /// <summary>Gets or sets the collection of <see cref="Player"/> object that this <see cref="Team"/> contains.</summary>
        public List<Player> Players { get; set; }
    }
}