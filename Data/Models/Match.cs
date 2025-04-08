namespace Tournament.Data.Models
{
    using System;

    public class Match
    {
        public int Id { get; set; }
        public int TournamentId { get; set; }

        public int TeamAId { get; set; }
        public Team TeamA { get; set; }

        public int TeamBId { get; set; }
        public Team TeamB { get; set; }

        public int? ScoreA { get; set; }
        public int? ScoreB { get; set; }

        public DateTime? PlayedOn { get; set; }
    }
    //public class Match
    //{
    //    public int Id { get; set; }
    //    public int TeamAId { get; set; }
    //    public int TeamBId { get; set; }
    //    public DateTime MatchDate { get; set; }
    //    public int? ScoreA { get; set; }
    //    public int? ScoreB { get; set; }

    //}
}
