using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Models
{
    public class Position
    {
        public Guid Id { get; set; }
        public DateTime OccurenceTime { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        /// <summary>
        /// Ebbe ez a három érték mehet bele pl:  
        ///        PlayerPosition,
        ///        FootballPosition,
        ///        FootballFieldSize
        /// Ez reprezentálja, hogy az adott pozició mihez is tartozik
        /// </summary>
        public string PositionType { get; set; }

        public Position()
        {
            this.Id = Guid.NewGuid();
            this.OccurenceTime = DateTime.Now;
        }
    }
}
