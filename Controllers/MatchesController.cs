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
    using Tournament.Services.MatchResultNotifire;
    using Tournament.Services.MatchScheduler;

    public class MatchesController : Controller
    {
        private readonly TurnirDbContext _context;
        private readonly IMatchSchedulerService _matchScheduler;
        private readonly IMatchResultNotifierService _notifier;

        public MatchesController(
            TurnirDbContext context,
            IMatchSchedulerService matchScheduler,
            IMatchResultNotifierService notifier)
        {
            _context = context;
            _matchScheduler = matchScheduler;
            _notifier = notifier;
        }
        // GET: Matches
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var matches = await _context.Matches.ToListAsync();

            var teams = await _context.Teams.ToDictionaryAsync(t => t.Id, t => t.Name);

            var model = matches.Select(m => new MatchViewModel
            {
                Id = m.Id,
                TeamAName = teams.ContainsKey(m.TeamAId) ? teams[m.TeamAId] : "???",
                TeamBName = teams.ContainsKey(m.TeamBId) ? teams[m.TeamBId] : "???",
                PlayedOn = (DateTime)m.PlayedOn,
                ScoreA = m.ScoreA,
                ScoreB = m.ScoreB
            });

            return View(model);
        }

        // GET: Matches/Create
        [Authorize(Roles = "Administrator")]
        public IActionResult Create()
        {
            var model = new MatchFormModel
            {
                PlayedOn = DateTime.Now,
                Teams = _context.Teams.Select(t => new SelectListItem
                {
                    Value = t.Id.ToString(),
                    Text = t.Name
                })
            };

            return View(model);
        }

        // POST: Matches/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Create(MatchFormModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Teams = _context.Teams.Select(t => new SelectListItem
                {
                    Value = t.Id.ToString(),
                    Text = t.Name
                });
                return View(model);
            }

            if (model.TeamAId == model.TeamBId)
            {
                ModelState.AddModelError("", "Не можеш да избираш един и същ отбор два пъти.");
                model.Teams = _context.Teams.Select(t => new SelectListItem
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
                TeamAName = teams.ContainsKey(match.TeamAId) ? teams[match.TeamAId] : "???",
                TeamBName = teams.ContainsKey(match.TeamBId) ? teams[match.TeamBId] : "???",
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
                TeamAName = teams.ContainsKey(match.TeamAId) ? teams[match.TeamAId] : "???",
                TeamBName = teams.ContainsKey(match.TeamBId) ? teams[match.TeamBId] : "???",
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



        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Edit(int id)
        {
            var match = await _context.Matches.FindAsync(id);
            if (match == null) return NotFound();

            var model = new MatchFormModel
            {
                TeamAId = match.TeamAId,
                TeamBId = match.TeamBId,
                PlayedOn = (DateTime)match.PlayedOn,
                ScoreA = match.ScoreA,
                ScoreB = match.ScoreB,
                Teams = await _context.Teams
                    .Select(t => new SelectListItem
                    {
                        Value = t.Id.ToString(),
                        Text = t.Name
                    }).ToListAsync()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Edit(int id, MatchFormModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Teams = await _context.Teams
                    .Select(t => new SelectListItem
                    {
                        Value = t.Id.ToString(),
                        Text = t.Name
                    }).ToListAsync();
                return View(model);
            }

            var match = await _context.Matches.FindAsync(id);
            if (match == null) return NotFound();

            bool wasResultMissing = true;// !match.ScoreA.HasValue && !match.ScoreB.HasValue;
            bool isNowFilled =  model.ScoreA.HasValue && model.ScoreB.HasValue;


            match.TeamAId = model.TeamAId;
            match.TeamBId = model.TeamBId;
            match.PlayedOn = model.PlayedOn;

            if (model.PlayedOn <= DateTime.Now)
            {
                match.ScoreA = model.ScoreA;
                match.ScoreB = model.ScoreB;
            }
            else
            {
                match.ScoreA = null;
                match.ScoreB = null;
                TempData["Message"] = "Мачът е в бъдещето – резултатът ще бъде маркиран като 'Предстои'.";
            }

            _context.Matches.Update(match);
            await _context.SaveChangesAsync();

            if (wasResultMissing && isNowFilled)
            {
                await _notifier.NotifyAsync(match.Id);
            }

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> GenerateSchedule(int tournamentId)
        {
            // Вземи одобрените отбори за турнира
            var teams = await _context.Teams
                .Where(t => _context.ManagerRequests.Any(r =>
                    r.TeamId == t.Id &&
                    r.TournamentId == tournamentId &&
                    r.IsApproved &&
                    r.FeePaid))
                .ToListAsync();

            if (teams.Count < 4)
            {
                TempData["Error"] = "Нужни са поне 4 одобрени отбора за съставяне на график.";
                return RedirectToAction("Index");
            }

            // Проверка дали вече има срещи
            bool hasMatches = await _context.Matches
                .AnyAsync(m => m.TournamentId == tournamentId);

            if (hasMatches)
            {
                TempData["Error"] = "Срещите за този турнир вече са генерирани.";
                return RedirectToAction("Index");
            }

            // Генериране на мачове всеки срещу всеки
            var matches = new List<Match>();

            for (int i = 0; i < teams.Count; i++)
            {
                for (int j = i + 1; j < teams.Count; j++)
                {
                    matches.Add(new Match
                    {
                        TournamentId = tournamentId,
                        TeamAId = teams[i].Id,
                        TeamBId = teams[j].Id
                    });
                }
            }

            _context.Matches.AddRange(matches);
            await _context.SaveChangesAsync();

            TempData["Message"] = $"Генерирани са {matches.Count} мача.";
            return RedirectToAction("Index");
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
