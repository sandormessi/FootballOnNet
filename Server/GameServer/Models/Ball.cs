namespace GameServer.Models
{
    /// <summary>Represents the ball in a <see cref="Game"/>.</summary>
    public class Ball : IdentifiableObject
    {
        /// <summary>Gets the <see cref="Player"/> who currently owns this <see cref="Ball"/>.</summary>
        public Player Owner { get; set; }

        public override string ToString()
        {
            return $"{Owner.Name}, ID: {Id}";
        }
    }
}
