using System.Collections.Generic;

namespace Tournament.Models
{
    public class Team
    {
        public int Id { get; init; }

        public string Name { get; set; }

        public string Town { get; set; }

        public int Points { get; set; }

        public IEnumerable<Player> Players { get; init; } = new List<Player>();
    }
}
