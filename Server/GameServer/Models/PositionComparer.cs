namespace GameServer.Models
{
    public static class PositionComparer
    {
        public static bool Equals(Position position1, Position position2)
        {
            if (position1 is null)
            {
                throw new System.ArgumentNullException(nameof(position1));
            }

            if (position2 is null)
            {
                throw new System.ArgumentNullException(nameof(position2));
            }

            return (position1.X == position2.X) && (position1.Y == position2.Y);
        }
    }
}
