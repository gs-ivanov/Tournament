namespace Tournament.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Microsoft.EntityFrameworkCore;
    using Tournament.Data;
    using Tournament.Data.Models;
    using System.Threading.Tasks;
    using Tournament.Models.Matches;
    using System.Linq;
    using System;

    public class MatchesController : Controller
    {
        private readonly TurnirDbContext _context;

        public MatchesController(TurnirDbContext context)
        {
            _context = context;
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
                MatchDate = m.MatchDate,
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
                MatchDate = DateTime.Now,
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
                MatchDate = model.MatchDate,
                ScoreA = model.ScoreA,
                ScoreB = model.ScoreB
            };

            _context.Matches.Add(match);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }

}
