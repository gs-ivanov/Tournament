namespace Tournament.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Tournament.Data;
    using Tournament.Data.Models;
    using Tournament.Models.Matches;
    using Tournament.Models.TeamRanking;
    using Tournament.Services.MatchResultNotifire;
    using Tournament.Services.MatchScheduler;
    using Tournament.Services.Sms; // ако имаш ISmsSender


    public class MatchesController : Controller
    {
        private readonly TurnirDbContext _context;
        private readonly IMatchSchedulerService _matchScheduler;
        private readonly IMatchResultNotifierService _notifier;
        private readonly ISmsSender _smsSender;


        public MatchesController(
            TurnirDbContext context,
            IMatchSchedulerService matchScheduler,
            IMatchResultNotifierService notifier,
            ISmsSender _smsSender)
        {
            this._context = context;
            this._matchScheduler = matchScheduler;
            this._notifier = notifier;
            this._smsSender = _smsSender;
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var match = await _context.Matches
                .Include(m => m.TeamA)
                .Include(m => m.TeamB)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (match == null)
                return NotFound();

            return View(match);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ScoreA,ScoreB")] Match updated)
        {
            if (!ModelState.IsValid)
            {
                var original = await _context.Matches
                    .Include(m => m.TeamA)
                    .Include(m => m.TeamB)
                    .FirstOrDefaultAsync(m => m.Id == id);

                return View(original);
            }

            // 🔄 Една заявка с Include
            var match = await _context.Matches
                .Include(m => m.TeamA)
                .Include(m => m.TeamB)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (match == null)
                return NotFound();

            match.ScoreA = updated.ScoreA;
            match.ScoreB = updated.ScoreB;

            await _context.SaveChangesAsync();

            TempData["Message"] = "Резултатът беше обновен.";

            // 📢 SMS известие
            await _smsSender.SendSmsAsync(
                "+359885773102",
                $"📢 Резултатът от мача {match.TeamA.Name} срещу {match.TeamB.Name} беше обновен: {match.ScoreA} : {match.ScoreB}."
            );

            return RedirectToAction("Index");
        }
        // GET: Matches
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var matches = await _context.Matches
                 .Include(m => m.TeamA)
                 .Include(m => m.TeamB)
                 .Include(m => m.Tournament)
                 .ToListAsync();

            return View(matches);
        }

        // GET: Matches/Create
        [Authorize(Roles = "Administrator")]
        public IActionResult Create()
        {
            if (!_context.Teams.Any())
            {
                var model = new MatchFormModel
                {
                    PlayedOn = DateTime.Now,
                    Teams = (List<SelectListItem>)_context.Teams.Select(t => new SelectListItem
                    {
                        Value = t.Id.ToString(),
                        Text = t.Name
                    })
                };

                return View(model);
            }

            return View();
        }

        // POST: Matches/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Create(MatchFormModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Teams = (List<SelectListItem>)_context.Teams.Select(t => new SelectListItem
                {
                    Value = t.Id.ToString(),
                    Text = t.Name
                });
                return View(model);
            }

            if (model.TeamAId == model.TeamBId)
            {
                ModelState.AddModelError("", "Не можеш да избираш един и същ отбор два пъти.");
                model.Teams = (List<SelectListItem>)_context.Teams.Select(t => new SelectListItem
                {
                    Value = t.Id.ToString(),
                    Text = t.Name
                });
                return View(model);
            }

            var match = new Match
            {
                TeamAId = model.TeamAId,
                TeamBId = model.TeamBId,
                PlayedOn = model.PlayedOn
            };

            if (model.PlayedOn <= DateTime.Now)
            {
                match.ScoreA = model.ScoreA;
                match.ScoreB = model.ScoreB;
            }
            else
            {
                TempData["Message"] = "Мачът е в бъдещето – резултатът ще бъде маркиран като 'Предстои'.";
            }

            _context.Matches.Add(match);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            var match = await _context.Matches.FindAsync(id);
            if (match == null) return NotFound();

            var teams = await _context.Teams.ToDictionaryAsync(t => t.Id, t => t.Name);

            var model = new MatchViewModel
            {
                Id = match.Id,
                TeamA = teams.ContainsKey(match.TeamAId) ? teams[match.TeamAId] : "???",
                TeamB = teams.ContainsKey(match.TeamBId) ? teams[match.TeamBId] : "???",
                PlayedOn = (DateTime)match.PlayedOn,
                ScoreA = match.ScoreA,
                ScoreB = match.ScoreB
            };

            return View(model);
        }

        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Delete(int id)
        {
            var match = await _context.Matches.FindAsync(id);
            if (match == null) return NotFound();

            var teams = await _context.Teams.ToDictionaryAsync(t => t.Id, t => t.Name);

            var model = new MatchViewModel
            {
                Id = match.Id,
                TeamA = teams.ContainsKey(match.TeamAId) ? teams[match.TeamAId] : "???",
                TeamB = teams.ContainsKey(match.TeamBId) ? teams[match.TeamBId] : "???",
                PlayedOn = (DateTime)match.PlayedOn,
                ScoreA = match.ScoreA,
                ScoreB = match.ScoreB
            };

            return View(model);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var match = await _context.Matches.FindAsync(id);
            if (match == null) return NotFound();

            _context.Matches.Remove(match);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Мачът беше успешно изтрит.";
            return RedirectToAction(nameof(Index));
        }

        //[HttpGet]
        //[Authorize(Roles = "Administrator,Editor")]
        //public async Task<IActionResult> Edit(int id)
        //{
        //    var match = await _context.Matches.FindAsync(id);
        //    if (match == null) return NotFound();

        //    var teams = await _context.Teams
        //        .Select(t => new SelectListItem
        //        {
        //            Value = t.Id.ToString(),
        //            Text = t.Name
        //        })
        //        .ToListAsync();

        //    var model = new MatchFormModel
        //    {
        //        Id = match.Id,
        //        TeamAId = match.TeamAId,
        //        TeamBId = match.TeamBId,
        //        ScoreA = match.ScoreA,
        //        ScoreB = match.ScoreB,
        //        PlayedOn = match.PlayedOn,
        //        Teams = teams
        //    };

        //    return View(model);
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //[Authorize(Roles = "Administrator,Editor")]
        //public async Task<IActionResult> Edit(int id, MatchFormModel model)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        model.Teams = await _context.Teams
        //            .Select(t => new SelectListItem
        //            {
        //                Value = t.Id.ToString(),
        //                Text = t.Name
        //            })
        //            .ToListAsync();

        //        return View(model);
        //    }

        //    if (model.TeamAId == model.TeamBId)
        //    {
        //        ModelState.AddModelError(string.Empty, "Отборите трябва да бъдат различни.");
        //        model.Teams = await _context.Teams
        //            .Select(t => new SelectListItem
        //            {
        //                Value = t.Id.ToString(),
        //                Text = t.Name
        //            })
        //            .ToListAsync();
        //        return View(model);
        //    }

        //    var match = await _context.Matches.FindAsync(id);
        //    if (match == null) return NotFound();

        //    match.TeamAId = model.TeamAId;
        //    match.TeamBId = model.TeamBId;
        //    match.ScoreA = model.ScoreA;
        //    match.ScoreB = model.ScoreB;
        //    match.PlayedOn = model.PlayedOn ?? DateTime.UtcNow;

        //    await _context.SaveChangesAsync();

        //    TempData["Message"] = "✅ Мачът е обновен успешно.";

        //    // Проверка: Завършени ли са всички мачове от турнира?
        //    var allCompleted = await _context.Matches
        //        .Where(m => m.TournamentId == match.TournamentId)
        //        .AllAsync(m => m.ScoreA.HasValue && m.ScoreB.HasValue);

        //    if (allCompleted)
        //    {
        //        TempData["Message"] = "🏁 Всички мачове са завършени. Класиране е налично.";
        //        return RedirectToAction("Ranking", new { tournamentId = match.TournamentId });
        //    }

        //    return RedirectToAction("Index");
        //}

        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> GenerateSchedule(int tournamentId)
        {
            // Вземи одобрените отбори за турнира


            TempData["Message"] = $"Unused. See Generate RoundRobin Matches";
            return RedirectToAction("Index");
        }


        [Authorize(Roles = "Administrator,Editor")]
        public async Task<IActionResult> Ranking(int tournamentId)
        {
            var matches = await _context.Matches
                .Include(m => m.TeamA)
                .Include(m => m.TeamB)
                .Where(m => m.TournamentId == tournamentId && m.ScoreA.HasValue && m.ScoreB.HasValue)
                .ToListAsync();

            var teams = await _context.Teams.ToListAsync();
            var rankings = teams
                .Select(t => new TeamRankingViewModel { TeamName = t.Name })
                .ToDictionary(r => r.TeamName);

            foreach (var match in matches)
            {
                var teamA = match.TeamA.Name;
                var teamB = match.TeamB.Name;
                var scoreA = match.ScoreA.Value;
                var scoreB = match.ScoreB.Value;

                var a = rankings[teamA];
                var b = rankings[teamB];

                a.Played++;
                b.Played++;

                a.GoalsFor += scoreA;
                a.GoalsAgainst += scoreB;

                b.GoalsFor += scoreB;
                b.GoalsAgainst += scoreA;

                if (scoreA > scoreB)
                {
                    a.Wins++; a.Points += 3;
                    b.Losses++;
                }
                else if (scoreA < scoreB)
                {
                    b.Wins++; b.Points += 3;
                    a.Losses++;
                }
                else
                {
                    a.Draws++; b.Draws++;
                    a.Points += 1; b.Points += 1;
                }
            }

            var final = rankings.Values
                .OrderByDescending(r => r.Points)
                .ThenByDescending(r => r.GoalDifference)
                .ThenByDescending(r => r.GoalsFor)
                .ToList();

            ViewBag.TournamentName = (await _context.Tournaments.FindAsync(tournamentId))?.Name;

            return View(final);
        }
        //[Authorize(Roles = "Administrator")]
        //public async Task<IActionResult> GenerateMatches()
        //{
        //    var startDate = DateTime.Today.AddDays(1);
        //    int count = await _matchScheduler.GenerateScheduleAsync(startDate);

        //    TempData["Message"] = $"Генерирани са {count} мача, започвайки от {startDate:dd.MM.yyyy}.";
        //    return RedirectToAction("Index");
        //}

    }

}
