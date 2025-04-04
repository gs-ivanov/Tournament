﻿namespace Tournament.Models.TeamRanking
{
    // ViewModel за класиране
    public class TeamRankingViewModel
    {
        public string TeamName { get; set; }
        public int Wins { get; set; }
        public int Draws { get; set; }
        public int Losses { get; set; }
        public int Points { get; set; }
        public int Played => Wins + Draws + Losses;
    }
}
