namespace Tournament.Services.MatchScheduler
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Tournament.Data.Models;

    public class KnockoutScheduler : IMatchGenerator
    {
        public List<Match> Generate(List<Team> teams, Tournament tournament)
        {
            var matches = new List<Match>();

            if (teams.Count != 4)
                throw new InvalidOperationException("Knockout форматът изисква точно 4 отбора за 1/2 финали и финал.");

            var shuffled = teams.OrderBy(t => Guid.NewGuid()).ToList();

            // Полуфинали
            var semi1 = new Match
            {
                TeamA = shuffled[0],
                TeamB = shuffled[1],
                TournamentId = tournament.Id,
                PlayedOn = tournament.StartDate
            };
            var semi2 = new Match
            {
                TeamA = shuffled[2],
                TeamB = shuffled[3],
                TournamentId = tournament.Id,
                PlayedOn = tournament.StartDate.AddDays(2)
            };

            matches.Add(semi1);
            matches.Add(semi2);

            // Финал (ще се добави ръчно след резултатите от полуфиналите)
            return matches;
        }
    }
}

