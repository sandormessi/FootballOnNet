namespace GameServer.Models
{
    /// <summary>Represents a score (goal) in a <see cref="Game"/>.</summary>
    public class Score
    {
        /// <summary>Gets or sets the player who scored the goal.</summary>
        public Player Scorer { get; set; }
    }
}
