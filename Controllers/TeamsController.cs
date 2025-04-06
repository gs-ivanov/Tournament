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
    using System;
    using Tournament.Services.PDF;

    public class TeamsController : Controller
    {
        private readonly TurnirDbContext _context;
        private readonly PdfService pdfService;

        public TeamsController(TurnirDbContext context,
            PdfService pdfService)
        {
            _context = context;
            this.pdfService = pdfService;
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

        [Authorize(Roles = "Editor")]
        [HttpGet]
        public  IActionResult Create()
        {
            if (TempData["CodeValidated"]?.ToString() != "true")
            {
                return  RedirectToAction("EnterCode");
            }

            return View();
        }

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
                UserId = userId,
                TeamId = team.Id,
                TournamentType = model.TournamentType,
                JsonPayload = ManagerRequest.GenerateJson(team, model.TournamentType),
                Status = RequestStatus.Pending
            };

            _context.ManagerRequests.Add(request);
            await _context.SaveChangesAsync();

            // Генериране на HTML за PDF сертификат
            var certificateId = Guid.NewGuid().ToString().Substring(0, 8); // кратък уникален код

            var html = $@"
<html>
<head>
    <style>
        body {{
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            padding: 40px;
            background-color: #ffffff;
        }}

        .certificate {{
            border: 5px double #333;
            padding: 40px;
            background-color: #fdfdfd;
            text-align: center;
        }}

        .logo {{
            margin-bottom: 20px;
        }}

        h1 {{
            font-size: 30px;
            margin-bottom: 20px;
            color: #1a1a1a;
        }}

        .info {{
            font-size: 18px;
            text-align: left;
            margin-top: 30px;
        }}

        .info p {{
            margin: 6px 0;
        }}

        .footer {{
            margin-top: 50px;
            display: flex;
            justify-content: space-between;
        }}

        .signature {{
            text-align: center;
        }}

        .stamp {{
            text-align: center;
        }}

        .serial {{
            font-size: 14px;
            color: #777;
            margin-top: 30px;
        }}
    </style>
</head>
<body>
    <div class='certificate'>
        <div class='logo'>
            <img src='https://yourdomain.com/images/logo.png' width='120' alt='Logo' />
        </div>

        <h1>СЕРТИФИКАТ ЗА УЧАСТИЕ</h1>
        <p>С този сертификат потвърждаваме, че отборът <strong>{team.Name}</strong></p>
        <p>е регистриран за участие в турнир от тип <strong>{model.TournamentType}</strong>.</p>

        <div class='info'>
            <p><strong>Треньор:</strong> {team.CoachName}</p>
            <p><strong>Имейл за контакт:</strong> {team.ContactEmail}</p>
            <p><strong>Дата:</strong> {DateTime.Now:dd.MM.yyyy}</p>
        </div>

        <div class='footer'>
            <div class='signature'>
                <img src='https://yourdomain.com/images/signature.png' width='100' /><br/>
                <span>Подпис</span>
            </div>
            <div class='stamp'>
                <img src='https://yourdomain.com/images/stamp.png' width='100' /><br/>
                <span>Печат</span>
            </div>
        </div>

        <div class='serial'>
            Сертификат № {certificateId}
        </div>
    </div>
</body>
</html>";

            // Генериране на PDF
            var pdfBytes = pdfService.GeneratePdfFromHtml(html);

            if (pdfBytes != null && pdfBytes.Length > 0)
            {
                TempData["Message"] = "Отборът е добавен и заявката за участие е подадена.";
                return File(pdfBytes, "application/pdf", "sertifikat.pdf");
            }

            TempData["Error"] = "Създаден е отбор, но сертификатът не беше генериран.";
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

        [Authorize(Roles = "Editor")]
        [HttpGet]
        public IActionResult EnterCode()
        {
            return View();
        }

        [Authorize(Roles = "Editor")]
        [HttpPost]
        public async Task<IActionResult> ValidateCode(string code)
        {
            var userId = User.Id();

            if (userId != code)
            {
                TempData["Error"] = "Невалиден код.";
                return RedirectToAction("EnterCode");
            }

            var request = await _context.ManagerRequests.FirstOrDefaultAsync(r =>
                r.UserId == userId && r.IsApproved == true);

            if (request == null)
            {
                TempData["Error"] = "Нямате одобрена заявка за участие.";
                return RedirectToAction("EnterCode");
            }

            TempData["CodeValidated"] = "true";
            return RedirectToAction("Create");
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
