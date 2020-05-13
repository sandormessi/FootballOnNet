namespace Client.Models
{
    using System;

    using Client.Models.Message.InitialMessages;

    /// <summary>Represents a player int a <see cref="Team"/>.</summary>
    public class Player : IEquatable<Player>
    {
        /// <summary>Gets or sets the name of this <see cref="Player"/>.</summary>
        public string Name { get; set; }
        /// <summary>Gets or sets the unique ID of this <see cref="Player"/>.</summary>
        public Guid ID { get; set; }
        /// <summary>Gets or sets the position where this <see cref="Player"/> plays.</summary>
        public PlayerType Type { get; set; }

        /// <summary>Determines whether the <paramref name="other"/> equals to this.</summary>
        /// <param name="other">The other <see cref="Player"/>.</param>
        /// <returns>Returns True if they are equal, otherwise returns False.</returns>
        public bool Equals(Player other)
        {
            if (other is null)
            {
                return false;
            }

            return ID == other.ID;
        }
    }
}
