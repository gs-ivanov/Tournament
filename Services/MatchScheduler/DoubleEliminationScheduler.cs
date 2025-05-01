namespace Tournament.Services.MatchScheduler
{
    using System.Collections.Generic;
    using Tournament.Data.Models;

    public class DoubleEliminationScheduler : IMatchGenerator
    {

        public List<Match> Generate(List<Team> teams, Tournament tournament)
        {
            var matches = new List<Match>();


            // Финал (ще се добави ръчно след резултатите от полуфиналите)
            return matches;
        }
    }
}

