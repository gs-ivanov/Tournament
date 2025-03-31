namespace Tournament.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Tournament.Data;
    using Tournament.Data.Models;
    using Tournament.Models.Matches;

    using static Tournament.WebConstants;

    public class MatchesController : Controller
    {
        private readonly TurnirDbContext data;

        public MatchesController(TurnirDbContext context)
        {
            data = context;
        }

        public IActionResult GenerateKnockoutMatches()
        {

            var matchesExists = data.Matches.ToList();

            if (matchesExists.Count() > 0)
            {
                TempData[GlobalMessageKey] = "График вече е създаден. Ако искаш нов график, избери 'Нулиране на график - 'Елемениране'.";
                return RedirectToAction(nameof(Index), "Teams");
            }

            var teams = data.Teams.ToList();
            if (teams.Count % 2 != 0)
            {
                TempData[GlobalMessageKey] = "Броят на отборите трябва да е степен на 2 (например 4, 8, 16).";
                return RedirectToAction(nameof(Index), "Teams");
            }

            List<Match> matches = new List<Match>();
            Random rand = new();

            // Разбъркваме отборите за случайни двойки
            teams = teams.OrderBy(t => rand.Next()).ToList();

            for (int i = 0; i < teams.Count; i += 2)
            {
                matches.Add(new Match
                {
                    HomeTeamId = teams[i].Id,
                    AwayTeamId = teams[i + 1].Id,
                    MatchDate = DateTime.Now.AddDays(i), // Примерна дата
                    HomeTeamGoals = null,
                    AwayTeamGoals = null
                });
            }


            data.Matches.AddRange(matches);
            data.SaveChanges();

            TempData[GlobalMessageKey] = "Графикът е успешно генериран!";

            return RedirectToAction(nameof(Index));
        }

        public IActionResult GenerateEliminationBracket()
        {
            var teams = data.Teams.ToList();

            if (teams.Count != 8)
            {
                TempData[GlobalMessageKey] = "Броят на отборите трябва да е 8.";// степен на 2 (например 4, 8, 16).";
                return RedirectToAction(nameof(Index), "Teams");
            }

            // Разбъркване на отборите на случаен принцип
            var random = new Random();
            teams = teams.OrderBy(t => random.Next()).ToList();

            // Създаване на мачовете за четвъртфиналите
            List<Match> matches = new List<Match>();
            for (int i = 0; i < teams.Count; i += 2)
            {
                matches.Add(new Match
                {
                    HomeTeamId = teams[i].Id,
                    AwayTeamId = teams[i + 1].Id,
                    MatchDate = DateTime.Now.AddDays(7), // Първият кръг е след 7 дни
                    HomeTeamGoals = null,
                    AwayTeamGoals = null
                });
            }

            data.Matches.AddRange(matches);
            data.SaveChanges();

            return RedirectToAction(nameof(Index));
        }


        public IActionResult GenerateSchedule()
        {

            var matchesExists = data.Matches.ToList();

            if (matchesExists.Count() > 0)
            {
                TempData[GlobalMessageKey] = "График вече е създаден. Ако искаш нов график, избери 'Нулиране на графика'.";
                return RedirectToAction(nameof(Index), "Teams");
            }

            var teams = data.Teams.ToList();
            if (teams.Count < 4)
            {
                TempData[GlobalMessageKey] = "Трябва да има най-малко 4 отбора, за да се генерира график.";
                return RedirectToAction(nameof(Index), "Teams");
            }

            List<Match> matches = new List<Match>();
            DateTime startDate = DateTime.Now.AddDays(7); // Започваме след една седмица

            for (int i = 0; i < teams.Count; i++)
            {
                for (int j = i + 1; j < teams.Count; j++)
                {
                    matches.Add(new Match { HomeTeamId = teams[i].Id, AwayTeamId = teams[j].Id, MatchDate = startDate });
                    matches.Add(new Match { HomeTeamId = teams[j].Id, AwayTeamId = teams[i].Id, MatchDate = startDate });
                }
                    startDate = startDate.AddDays(7);
            }

            data.Matches.AddRange(matches);
            data.SaveChanges();

            TempData[GlobalMessageKey] = "Графикът е успешно генериран!";

            return RedirectToAction(nameof(Index));
        }



        public IActionResult Index()
        {
            var matches = data.Matches
                .OrderBy(m => m.MatchDate)
                .Select(m => new MatchViewModel
                {
                    Id = m.Id,
                    HomeTeam = m.HomeTeam.Name,
                    AwayTeam = m.AwayTeam.Name,
                    HomeTeamGoals = (int)m.HomeTeamGoals,
                    AwayTeamGoals = (int)m.AwayTeamGoals,
                    MatchDate = m.MatchDate
                })
                .ToList();

            return View(matches);
        }


        [Authorize(Roles = "Administrator")]
        public IActionResult Reset()
        {
            var teamsExists = data.Teams.ToList();
            var matchesExists = data.Matches.ToList();

            if (matchesExists.Count == 0 && teamsExists.Count == 0)
            {
                TempData[GlobalMessageKey] = "Графика вече e нулиран. Ако искаш нов график, избери 'Settings ...'.";

                return RedirectToAction(nameof(Index), "Teams");
            }

            string mesage = "READY TO REMOVE ALL RECORDS IN MATCHES TABLE!!!";

            ViewBag.Msg = mesage;
            return View();
        }

        [HttpPost, ActionName("Reset")]
        [Authorize(Roles = "Administrator")]
        public IActionResult ResetConfirmed()
        {
            List<Match> itemsToDelete = this.data.Matches.ToList();

            this.data.Matches.RemoveRange(itemsToDelete);
            this.data.SaveChanges();

            TempData[GlobalMessageKey] = "Графикът e нулиран. За нов график - избери 'Generate Schedule'.";

            //return RedirectToAction(nameof(AllTeams));
            return RedirectToAction("Index", "Teams");

        }

        [Authorize(Roles = "Editor")]
        public IActionResult Edit(int id)
        {
            var match = data.Matches
                .Where(m => m.Id == id)
                .Select(m => new MatchFormModel
                {
                    Id = m.Id,
                    HomeTeam = m.HomeTeam.Name,
                    AwayTeam = m.AwayTeam.Name,
                    MatchDate = m.MatchDate,
                    HomeTeamGoals = (int)m.HomeTeamGoals,
                    AwayTeamGoals = (int)m.AwayTeamGoals
                })
                .FirstOrDefault();

            if (match == null)
            {
                return NotFound();
            }

            return View(match);
        }


        //[HttpPost, ActionName("Edit")]
        //public IActionResult EditMatches(int id, MatchFormModel match)
        [HttpPost]
        [Authorize(Roles = "Editor")]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,HomeTeamId,AwayTeamId,MatchDate,HomeTeamGoals,AwayTeamGoals")] Match match)
        {
            if (id != match.Id)
            {
                return NotFound();
            }

            var existingMatch = await data.Matches
                .Include(m => m.HomeTeam)
                .Include(m => m.AwayTeam)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (existingMatch == null)
            {
                return NotFound();
            }

            var errSrc = $"{match.MatchDate} {existingMatch.HomeTeam.Name} vs  {existingMatch.AwayTeam.Name}";

            // 1. Забраняваме повторното задаване на резултат
            if (existingMatch.HomeTeamGoals.HasValue || existingMatch.AwayTeamGoals.HasValue)
            {
                TempData[GlobalMessageKey] = $"Резултатът вече е въведен и не може да се променя {errSrc}.";

                return RedirectToAction(nameof(Index));

            }

            // 2. Проверяваме дали има мачове с по-ранна дата, които нямат въведен резултат
            bool hasUnfinishedMatches = await this.data.Matches
                .AnyAsync(m => m.MatchDate < match.MatchDate && (!m.HomeTeamGoals.HasValue || !m.AwayTeamGoals.HasValue));

            if (hasUnfinishedMatches)
            {
                TempData[GlobalMessageKey] = $"Не можете да въвеждате резултат за този кръг: {errSrc}, докато предходният не е завършен.";

                return RedirectToAction(nameof(Index));
            }



            // Проверка дали резултатът вече е въведен
            if (existingMatch.HomeTeamGoals != null || existingMatch.AwayTeamGoals != null)
            {
                TempData[GlobalMessageKey] = $"Резултатът вече е въведен и не може да бъде редактиран ({errSrc}).";

                return RedirectToAction(nameof(Index));
            }

            // Проверка дали резултатът не е null
            if (match.HomeTeamGoals == null || match.AwayTeamGoals == null || match.HomeTeamGoals < 0 || match.AwayTeamGoals < 0)
            {
                TempData[GlobalMessageKey] = $"Резултатът не може да бъде различен от число нула или по-голямо ({errSrc}).";

                return RedirectToAction(nameof(Index));
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Запазване на резултатите в мача
                    existingMatch.HomeTeamGoals = match.HomeTeamGoals;
                    existingMatch.AwayTeamGoals = match.AwayTeamGoals;

                    // Актуализиране на статистиките на отборите
                    UpdateTeamStats(existingMatch);

                    await data.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MatchExists(match.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                return RedirectToAction(nameof(Index), "Teams");
            }

            return View(match);
        }

        private bool MatchExists(int id)
        {
            return this.data.Matches.Any(m => m.Id == id);
        }


        private void UpdateTeamStats(Match match)
        {
            var homeTeam = match.HomeTeam;
            var awayTeam = match.AwayTeam;

            if (homeTeam == null || awayTeam == null)
            {
                return;
            }

            // Обновяване на головете
            homeTeam.GoalsScored += match.HomeTeamGoals ?? 0;
            homeTeam.GoalsConceded += match.AwayTeamGoals ?? 0;
            homeTeam.MatchNumbers ++;
            awayTeam.GoalsScored += match.AwayTeamGoals ?? 0;
            awayTeam.GoalsConceded += match.HomeTeamGoals ?? 0;
            awayTeam.MatchNumbers++;

            // Определяне на резултата и актуализиране на класирането
            if (match.HomeTeamGoals > match.AwayTeamGoals)
            {
                homeTeam.Wins++;
                awayTeam.Losts++;
                homeTeam.Points += 3;
            }
            else if (match.HomeTeamGoals < match.AwayTeamGoals)
            {
                awayTeam.Wins++;
                homeTeam.Losts++;
                awayTeam.Points += 3;
            }
            else
            {
                homeTeam.Draws++;
                awayTeam.Draws++;
                homeTeam.Points += 1;
                awayTeam.Points += 1;
            }

            this.data.Teams.Update(homeTeam);
            this.data.Teams.Update(awayTeam);
        }
    }
}
