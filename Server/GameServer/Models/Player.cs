namespace GameServer.Models
{
    using System;

    /// <summary>Represents a player int a <see cref="Team"/>.</summary>
    public class Player : IEquatable<Player>
    {
        ///// <summary>Initializes a new instance from <see cref="Player"/> with the specified name and type.</summary>
        ///// <param name="name">The name of this <see cref="Player"/>.</param>
        ///// <param name="type">The type of this <see cref="Player"/>.</param>
        //public Player(string name, PlayerType type)
        //{
        //    Type = type;
        //    Name = string.IsNullOrWhiteSpace(name)
        //        ? throw new ArgumentException($"The name of the {nameof(Player)} cannot be null, empty string or contain only whitespaces.")
        //        : name;

        //    ID = new Guid();
        //}

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
