namespace GameServer.Models
{
    using System;

    /// <summary>Represents an object that has a <see cref="Guid"/>.</summary>
    public class IdentifiableObject
    {
        /// <summary>Gets or sets the unique ID of this object.</summary>
        public Guid ID { get; set; }
    }
}