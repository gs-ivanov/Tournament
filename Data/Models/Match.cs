namespace Tournament.Data.Models
{
    using System;

    public class Match
    {
        public int Id { get; set; }

        public int TournamentId { get; set; }
        public Tournament Tournament { get; set; }

        public int TeamAId { get; set; }                  // ✅ Ясен FK
        public Team TeamA { get; set; }

        public int TeamBId { get; set; }                  // ✅ Ясен FK
        public Team TeamB { get; set; }

        public int? ScoreA { get; set; }
        public int? ScoreB { get; set; }

        public DateTime? PlayedOn { get; set; }
    }
}
