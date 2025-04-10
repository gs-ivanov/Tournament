namespace Tournament.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using System.Linq;
    using System.Threading.Tasks;
    using Tournament.Data;
    using Tournament.Data.Models;

    public class TournamentsController : Controller
    {
        private readonly TurnirDbContext _context;

        public TournamentsController(TurnirDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var tournaments = await _context.Tournaments
                .Include(t => t.Matches) // ← това е важното за проверката "има мачове"
                .ToListAsync();

            return View(tournaments);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Tournament tournament)
        {
            if (!ModelState.IsValid)
            {
                return View(tournament);
            }

            _context.Tournaments.Add(tournament);
            await _context.SaveChangesAsync();

            TempData["Message"] = "✅ Турнирът е създаден успешно.";
            return RedirectToAction("Index");
        }


        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var tournament = await _context.Tournaments
                .Include(t => t.Matches)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tournament == null)
            {
                return NotFound();
            }

            if (tournament.Matches.Any())
            {
                TempData["Error"] = "❌ Турнирът има свързани мачове и не може да бъде изтрит.";
                return RedirectToAction("Index");
            }

            _context.Tournaments.Remove(tournament);
            await _context.SaveChangesAsync();

            TempData["Message"] = "✅ Турнирът е изтрит успешно.";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var tournament = await _context.Tournaments.FindAsync(id);
            if (tournament == null)
            {
                return NotFound();
            }

            return View(tournament);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Tournament updated)
        {
            if (id != updated.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(updated);
            }

            try
            {
                _context.Tournaments.Update(updated);
                await _context.SaveChangesAsync();

                TempData["Message"] = "✅ Турнирът е обновен успешно.";
                return RedirectToAction("Index");
            }
            catch
            {
                TempData["Error"] = "⚠ Грешка при обновяване на турнира.";
                return View(updated);
            }
        }

    }
}
