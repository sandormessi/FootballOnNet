namespace Client.Models
{
    using System;

    /// <summary>Represents the position of a <see cref="Player"/> or a <see cref="Ball"/> in a <see cref="Game"/>.</summary>
    public class Position
    {
        /// <summary>Gets the X coordinate of this <see cref="Position"/>.</summary>
        public int X { get; set; }
        /// <summary>Gets the Y coordinate of this <see cref="Position"/>.</summary>
        public int Y { get; set; }

        /// <summary>Gets the object's <see cref="Guid"/> that this <see cref="Position"/> associated with.</summary>
        public Guid Id { get; set; }
    }
}
