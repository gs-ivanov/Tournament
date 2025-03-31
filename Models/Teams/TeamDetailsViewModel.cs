namespace Tournament.Models.Teams
{
    using System.Collections.Generic;
    using Tournament.Data.Models;

    public class TeamDetailsViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Logo { get; set; }
        public int MatchNumbers { get; set; }
        public int Points { get; set; }
        public int Wins { get; set; }
        public int Losts { get; set; }
        public int Draws { get; set; }
        public int GoalsScored { get; set; }
        public int GoalsConceded { get; set; }
        public int GoalDifference => GoalsScored - GoalsConceded;
        public IEnumerable<Match> History { get; set; } = new List<Match>();

    }
}
