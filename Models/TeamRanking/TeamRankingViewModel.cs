namespace Tournament.Models.TeamRanking
{
    public class TeamRankingViewModel
    {
        public string TeamName { get; set; }
        public int Played { get; set; }
        public int Wins { get; set; }
        public int Draws { get; set; }
        public int Losses { get; set; }
        public int GoalsFor { get; set; }
        public int GoalsAgainst { get; set; }
        public int GoalDifference => GoalsFor - GoalsAgainst;
        public int Points { get; set; }
    }

    //// ViewModel за класиране
    //public class TeamRankingViewModel
    //{
    //    public string TeamName { get; set; }
    //    public int Wins { get; set; }
    //    public int Draws { get; set; }
    //    public int Losses { get; set; }
    //    public int Points { get; set; }
    //    public int Played => Wins + Draws + Losses;
    //}
}
