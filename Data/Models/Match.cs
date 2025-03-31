namespace Tournament.Data.Models
{
    using System;

    public class Match
    {
        public int Id { get; set; }

        public int HomeTeamId { get; set; }
        public Team HomeTeam { get; set; }

        public int AwayTeamId { get; set; }
        public Team AwayTeam { get; set; }

        public DateTime MatchDate { get; set; }

        // Променяме полетата за резултати на nullable
        public int? HomeTeamGoals { get; set; } // Nullable int
        public int? AwayTeamGoals { get; set; } // Nullable int
    }

}
