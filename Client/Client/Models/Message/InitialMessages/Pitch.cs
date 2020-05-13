namespace Client.Models.Message.InitialMessages
{
    /// <summary>Represents the football pitch.</summary>
    public class Pitch : IdentifiableObject
    {
        /// <summary>Gets or sets the width of this <see cref="Pitch"/>.</summary>
        public int Width { get; }
        /// <summary>Gets or sets the height of this <see cref="Pitch"/>.</summary>
        public int Height { get; }
    }
}
