namespace Tournament.Data.Models
{
    using global::Tournament.Models;
    using System;
    using System.Collections.Generic;

    public class Tournament
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public TournamentType Type { get; set; }
        public DateTime StartDate { get; set; }
        public List<Team> Teams { get; set; } = new();
    }
}
