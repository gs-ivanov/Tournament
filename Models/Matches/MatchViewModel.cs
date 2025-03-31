namespace Tournament.Models.Matches
{
    using System;

    public class MatchViewModel
    {
        public int Id { get; set; }

        public string HomeTeam { get; set; }

        public string AwayTeam { get; set; }

        public DateTime MatchDate { get; set; }

        public int? HomeTeamGoals { get; set; }

        public int? AwayTeamGoals { get; set; }
    }
}
