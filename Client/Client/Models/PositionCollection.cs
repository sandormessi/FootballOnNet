namespace Client.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class PositionCollection
    {
        public List<Position> Positions { get; } = new List<Position>();

        public void AddPosition(Position position)
        {
            Position foundItem = Positions.FirstOrDefault(x => x.Id == position.Id);
            if (!(foundItem is null))
            {
                throw new InvalidOperationException("The specified position already exists.");
            }

            Positions.Add( position);
        }

        public void SetPosition(Position position)
        {
            Position foundItem = Positions.FirstOrDefault(x => x.Id == position.Id);
            if (foundItem is null)
            {
                throw new InvalidOperationException("The specified position does not exist.");
            }

            foundItem.X = position.X;
            foundItem.Y = position.Y;
        }
        
        public Position GetPosition(Guid id)
        {
            Position foundItem = Positions.FirstOrDefault(x => x.Id == id);
            if (foundItem is null)
            {
                throw new InvalidOperationException("The specified position does not exist.");
            }

            return foundItem;
        }
    }
}
