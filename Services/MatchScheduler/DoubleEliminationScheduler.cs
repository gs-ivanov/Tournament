namespace Tournament.Services.MatchScheduler
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Tournament.Data.Models;

    public class DoubleEliminationScheduler : IMatchGenerator
    {
        public List<Match> Generate(List<Team> teams, Tournament tournament)
        {
            if (!IsPowerOfTwo(teams.Count))
                throw new InvalidOperationException("Double Elimination форматът изисква брой отбори, който е степен на 2 (напр. 4, 8, 16).");

            var matches = new List<Match>();
            var shuffled = teams.OrderBy(t => Guid.NewGuid()).ToList();
            DateTime roundDate = tournament.StartDate;

            // Първи кръг – Upper bracket
            for (int i = 0; i < shuffled.Count; i += 2)
            {
                matches.Add(new Match
                {
                    TeamAId = shuffled[i].Id,
                    TeamBId = shuffled[i + 1].Id,
                    TournamentId = tournament.Id,
                    PlayedOn = roundDate,
                    //Bracket = "Winners" // Указваме, че това е Winners Bracket
                });
            }

            // Загубилите от този кръг ще участват във втори кръг на Losers Bracket
            // но той се генерира чак след въвеждане на резултати, така че тук не го добавяме

            return matches;
        }

        private bool IsPowerOfTwo(int number)
        {
            return number > 1 && (number & (number - 1)) == 0;
        }
    }
}
    //public class DoubleEliminationScheduler : IMatchGenerator
    //{

//    public List<Match> Generate(List<Team> teams, Tournament tournament)
//    {
//        var matches = new List<Match>();


//        // Финал (ще се добави ръчно след резултатите от полуфиналите)
//        return matches;
//    }
//}
//}

