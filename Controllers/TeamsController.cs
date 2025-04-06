namespace Tournament.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Tournament.Data;
    using Tournament.Data.Models;
    using Tournament.Models;
    using Tournament.Models.Teams;
    using Tournament.Infrastructure.Extensions;

    public class TeamsController : Controller
    {
        private readonly TurnirDbContext _context;

        public TeamsController(TurnirDbContext context)
        {
            _context = context;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var teams = await _context.Teams
                .Select(t => new TeamViewModel
                {
                    Id = t.Id,
                    Name = t.Name,
                    CoachName = t.CoachName,
                    LogoUrl = t.LogoUrl,
                    ContactEmail = t.ContactEmail,
                    FeePaid = t.FeePaid
                })
                .ToListAsync();

            return View(teams);
        }

        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            var team = await _context.Teams
                .Where(t => t.Id == id)
                .Select(t => new TeamViewModel
                {
                    Id = t.Id,
                    Name = t.Name,
                    CoachName = t.CoachName,
                    LogoUrl = t.LogoUrl,
                    ContactEmail = t.ContactEmail,
                    FeePaid = t.FeePaid
                })
                .FirstOrDefaultAsync();

            if (team == null)
                return NotFound();

            return View(team);
        }

        ////// GET: Teams/Create
        ////[Authorize(Roles = "Administrator")]
        ////public IActionResult Create()
        ////{
        ////    return View(new CreateTeamViewModel());
        ////}

        ////// POST: Teams/Create
        ////[HttpPost]
        ////[ValidateAntiForgeryToken]
        ////[Authorize(Roles = "Administrator")]
        ////public async Task<IActionResult> Create(CreateTeamViewModel model)
        ////{
        ////    if (!ModelState.IsValid)
        ////        return View(model);

        ////    var team = new Team
        ////    {
        ////        Name = model.Name,
        ////        CoachName = model.CoachName,
        ////        LogoUrl = model.LogoUrl,
        ////        ContactEmail = model.ContactEmail,
        ////        FeePaid = model.FeePaid
        ////    };

        ////    _context.Teams.Add(team);
        ////    await _context.SaveChangesAsync();

        ////    TempData["Message"] = $"Отборът \"{team.Name}\" е създаден.";
        ////    return RedirectToAction(nameof(Index));
        ////}


        [Authorize(Roles = "Editor")]
        public IActionResult Create()
        {
            return View();
        }

        //[HttpGet]
        //public IActionResult Create()
        //{
        //    var model = new CreateTeamViewModel();
        //    return View(model);
        //}

        [HttpPost]
        [Authorize(Roles = "Editor")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateTeamViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = User.Id();

            var team = new Team
            {
                Name = model.Name,
                CoachName = model.CoachName,
                LogoUrl = model.LogoUrl,
                ContactEmail = model.ContactEmail,
                FeePaid = false,
                UserId = userId
            };

            _context.Teams.Add(team);
            await _context.SaveChangesAsync();

            var request = new ManagerRequest
            {
                UserId =userId,
                TeamId = team.Id,
                TournamentType = model.TournamentType,
                JsonPayload = ManagerRequest.GenerateJson(team, model.TournamentType),
                Status = RequestStatus.Pending
            };

            _context.ManagerRequests.Add(request);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Отборът е добавен и заявката за участие е подадена.";
            return RedirectToAction("Index", "Home");
        }

        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Edit(int id)
        {
            var team = await _context.Teams.FindAsync(id);
            if (team == null) return NotFound();

            var model = new EditTeamViewModel
            {
                Id = team.Id,
                Name = team.Name,
                CoachName = team.CoachName,
                LogoUrl = team.LogoUrl,
                ContactEmail = team.ContactEmail,
                FeePaid = team.FeePaid
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Edit(EditTeamViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var team = await _context.Teams.FindAsync(model.Id);
            if (team == null) return NotFound();

            team.Name = model.Name;
            team.CoachName = model.CoachName;
            team.LogoUrl = model.LogoUrl;
            team.ContactEmail = model.ContactEmail;
            team.FeePaid = model.FeePaid;

            _context.Teams.Update(team);
            await _context.SaveChangesAsync();

            TempData["Message"] = $"Отборът \"{team.Name}\" е обновен.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Teams/Delete/5
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Delete(int id)
        {
            var team = await _context.Teams
                .Where(t => t.Id == id)
                .Select(t => new TeamViewModel
                {
                    Id = t.Id,
                    Name = t.Name,
                    CoachName = t.CoachName,
                    LogoUrl = t.LogoUrl,
                    ContactEmail = t.ContactEmail,
                    FeePaid = t.FeePaid
                })
                .FirstOrDefaultAsync();

            if (team == null)
                return NotFound();

            return View(team);
        }

        // POST: Teams/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var team = await _context.Teams.FindAsync(id);
            if (team == null)
                return NotFound();

            _context.Teams.Remove(team);
            await _context.SaveChangesAsync();

            TempData["Message"] = $"Отборът \"{team.Name}\" беше изтрит.";
            return RedirectToAction(nameof(Index));
        }

        private bool TeamExists(int id)
        {
            return _context.Teams.Any(e => e.Id == id);
        }

        // GET: Teams/CreateMultiple
        public IActionResult CreateMultiple()
        {
            return View(new TeamFormModel());
        }

        // POST: Teams/CreateMultiple
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateMultiple(TeamFormModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Проверка за вече съществуващи отбори
            if (_context.Teams.Any())
            {
                TempData["Message"] = "Вече има записи в базата. Изчисти ги преди да добавиш нови.";
                return RedirectToAction(nameof(Index));
            }

            var teams = SeedTeams()
                .Take(model.TeamCount)
                .ToList();

            _context.Teams.AddRange(teams);
            await _context.SaveChangesAsync();

            TempData["Message"] = $"Успешно добавени {teams.Count} отбора.";
            return RedirectToAction(nameof(Index));
        }

        private static List<Team> SeedTeams()
        {
            List<string> teamNames = new()
            {
                "Лудогорец",
                "Крумовград",
                "Левски София",
                "Локомотив Пловдив",
                "Славия София",
                "Черно море",
                "Арда",
                "Ботев Враца",
                "ЦСКА София",
                "Септември София",
                "Спартак Варна",
                "Ботев Пловдив",
                "Берое",
                "Хебър",
                "ЦСКА 1948",
                "Миньор Перник"
            };

            List<string> teamLogos = new()
            {
                "/logos/ludogorec.png",
                "/logos/krumovgrad.png",
                "/logos/levski.png",
                "/logos/lokomotivplovdiv.png",
                "/logos/slavia.png",
                "/logos/chernomore.png",
                "/logos/arda.png",
                "/logos/botevvraca.png",
                "/logos/cskasofia.png",
                "/logos/septemvri.png",
                "/logos/spartakvarna.png",
                "/logos/botevplovdiv.png",
                "/logos/beroe.png",
                "/logos/hebar.png",
                "/logos/cska1948.png",
                "/logos/minyor.png"
            };

            var teams = new List<Team>();
            for (int i = 0; i < teamNames.Count; i++)
            {
                teams.Add(new Team
                {
                    Name = teamNames[i],
                    CoachName = "Н/Д",
                    ContactEmail = $"team{i + 1}@mail.bg",
                    FeePaid = false,
                    LogoUrl = teamLogos[i]
                });
            }

            return teams;
        }
    }
}
