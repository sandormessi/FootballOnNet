using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Models
{
    public class Scores
    {
        public Guid Id { get; set; }
        public string Player1Name { get; set; }
        public string Player2Name { get; set; }
        public int Player1Goal { get; set; }
        public int Player2Goal { get; set; }
        public DateTime GoalTime { get; set; }

        public Scores()
        {
            this.Id = Guid.NewGuid();
            this.GoalTime = DateTime.Now;

        }
    }

    public class Player
    {
        public string NickName { get; set; }
    }
}
