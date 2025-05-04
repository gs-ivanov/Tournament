namespace Tournament.Services.MatchScheduler
{
    using System.Collections.Generic;
    using System;
    using Tournament.Data;
    using Tournament.Data.Models;
    using System.Linq;

    internal class DoubleEliminationScheduler : IMatchGeneratorDbl
    {
        private readonly TurnirDbContext _context;

        public DoubleEliminationScheduler(TurnirDbContext context)
        {
            _context =context;
        }

        public List<Round> GenerateRounds(TurnirDbContext dbContext, List<Data.Models.Team> teams, Tournament tournament)
        {
            // Remove any existing matches for this tournament
            _context.Matches.RemoveRange(_context.Matches.Where(m => m.TournamentId == tournament.Id));
            _context.SaveChanges();

            if (teams == null || teams.Count < 2)
                throw new ArgumentException("At least two teams are required.");

            // Random seeding
            var rnd = new Random();
            teams = teams.OrderBy(t => rnd.Next()).ToList();

            // Pad to next power of two
            int size = 1;
            while (size < teams.Count) size <<= 1;
            for (int i = teams.Count; i < size; i++)
                teams.Add(null);  // bye

            var rounds = new List<Round>();
            var winnersQueue = new Queue<Data.Models.Team>(teams);
            int totalWBRounds = (int)Math.Log(size, 2);
            var losersQueue = new Queue<Data.Models.Team>();

            // Winners bracket
            for (int r = 1; r <= totalWBRounds; r++)
            {
                var round = new Round
                {
                    Number = r,
                    Bracket = BracketType.Winners,
                    Name = r == totalWBRounds ? "Winners Final" : $"Winners Round {r}"
                };

                var nextWinners = new List<Team>();
                while (winnersQueue.Count >= 2)
                {
                    var a = winnersQueue.Dequeue();
                    var b = winnersQueue.Dequeue();
                    var match = new Match
                    {
                        TournamentId = tournament.Id,
                        TeamAId = (int)a?.Id,
                        TeamBId = (int)b?.Id,
                        Round = r,
                        Bracket = BracketType.Winners,
                        PlayedOn = tournament.StartDate.AddDays((r - 1) * 2)
                    };
                    _context.Matches.Add(match);
                    _context.SaveChanges();

                    nextWinners.Add(match.WinnerTeam);
                    if (match.LoserTeam != null)
                        losersQueue.Enqueue(match.LoserTeam);

                    round.Matches.Add(match);
                }

                winnersQueue = new Queue<Team>(nextWinners);
                rounds.Add(round);
            }

            // Losers bracket
            for (int r = 1; r <= totalWBRounds && losersQueue.Count > 1; r++)
            {
                var round = new Round
                {
                    Number = totalWBRounds + r,
                    Bracket = BracketType.Losers,
                    Name = r == totalWBRounds ? "Losers Final" : $"Losers Round {r}"
                };

                var nextLosers = new List<Team>();
                while (losersQueue.Count >= 2)
                {
                    var a = losersQueue.Dequeue();
                    var b = losersQueue.Dequeue();
                    var match = new Match
                    {
                        TournamentId = tournament.Id,
                        TeamAId = a.Id,
                        TeamBId = b.Id,
                        Round = round.Number,
                        Bracket = BracketType.Losers,
                        PlayedOn = tournament.StartDate.AddDays((round.Number - 1) * 2)
                    };
                    _context.Matches.Add(match);
                    _context.SaveChanges();

                    nextLosers.Add(match.WinnerTeam);
                    round.Matches.Add(match);
                }

                losersQueue = new Queue<Team>(nextLosers);
                rounds.Add(round);
            }

            // Championship
            var champRound = new Round
            {
                Number = rounds.Max(r => r.Number) + 1,
                Bracket = BracketType.Championship,
                Name = "Championship"
            };
            var wf = rounds.First(r => r.Bracket == BracketType.Winners && r.Name.Contains("Final")).Matches.First();
            var lf = rounds.First(r => r.Bracket == BracketType.Losers && r.Name.Contains("Final")).Matches.First();
            var finalMatch = new Match
            {
                TournamentId = tournament.Id,
                SourceMatchAId = wf.Id,
                SourceMatchBId = lf.Id,
                Round = champRound.Number,
                Bracket = BracketType.Championship,
                PlayedOn = tournament.StartDate.AddDays(totalWBRounds * 3)
            };
            _context.Matches.Add(finalMatch);
            _context.SaveChanges();

            champRound.Matches.Add(finalMatch);
            rounds.Add(champRound);

            return rounds;
        }
    }
}