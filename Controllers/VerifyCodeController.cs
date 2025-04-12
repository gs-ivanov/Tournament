namespace Tournament.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using System.Linq;
    using System.Threading.Tasks;
    using Tournament.Data;
    using Tournament.Services.Sms;

    public class VerifyCodeController : Controller
    {
        private readonly TurnirDbContext _context;
        private readonly ISmsSender _smsSender;


        public VerifyCodeController(TurnirDbContext context, ISmsSender smsSender)
        {
            _context = context;
            _smsSender = smsSender;
        }

        [HttpGet]
        public IActionResult EnterCode()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnterCode(string email, string receiptNumber)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(receiptNumber))
            {
                TempData["Error"] = "Моля, попълнете всички полета.";
                return View();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                TempData["Error"] = "Невалиден имейл.";
                return View();
            }

            var request = await _context.ManagerRequests
                .Include(r => r.Team)
                .FirstOrDefaultAsync(r => r.User.Email == email && !r.IsApproved);

            if (request == null)
            {
                TempData["Error"] = "Няма чакаща заявка за този имейл.";
                return RedirectToAction("EnterCode");
            }

            // Потвърждаваме заявката
            request.IsApproved = true;
            request.FeePaid = true;

            // Добавяме отбора към турнира, ако още не е
            var tournament = await _context.Tournaments
                .Include(t => t.Teams)
                .FirstOrDefaultAsync(t => t.Id == request.TournamentId);

            if (tournament != null && !tournament.Teams.Any(t => t.Id == request.TeamId))
            {
                var team = await _context.Teams.FindAsync(request.TeamId);
                if (team != null)
                {
                    tournament.Teams.Add(team);
                }
            }

            // Записваме промените
            await _context.SaveChangesAsync();

            // ✅ Проверка за точно 4 отбора, свързани с турнира
            if (tournament.Teams.Count == 4)
            {
                TempData["Message"] = "Можеш да генерираш график с 4 отбора. Логвай се като админ и от падащо меню инициирай генерирането.";
            }
            else
            {
                TempData["Message"] = "Участието е потвърдено.";
            }

            // Изпращаме SMS
            await _smsSender.SendSmsAsync("+359885773102", $"✅ Отборът {request.Team.Name} е включен в турнира {tournament.Name}.");

            return RedirectToAction("Index", "Home");
        }

        //////    // Добавяме отбора към турнира, ако още не е
        //////    var tournament = await _context.Tournaments
        //////        .Include(t => t.Teams)
        //////        .FirstOrDefaultAsync(t => t.Id == request.TournamentId);

        //////    if (tournament != null && !tournament.Teams.Any(t => t.Id == request.TeamId))
        //////    {
        //////        var team = await _context.Teams.FindAsync(request.TeamId);
        //////        if (team != null)
        //////        {
        //////            tournament.Teams.Add(team);
        //////        }
        //////    }

        //////    // Записваме промените
        //////    await _context.SaveChangesAsync();

        //////    // Проверка за 4 одобрени отбора за турнира
        //////    var approvedCount = await _context.Teams
        //////        .CountAsync(t => t.TournamentId == tournament.Id && t.IsApproved && t.FeePaid);

        //////    if (approvedCount == 4)
        //////    {
        //////        TempData["Message"] = "Можеш да генерираш график с 4 отбора. Логвай се като админ и от падащо меню инициирай генерирането.";
        //////    }
        //////    else
        //////    {
        //////        TempData["Message"] = "Участието е потвърдено.";
        //////    }



        //[HttpGet]
        ////[Authorize(Roles = "Editor")]
        //public IActionResult EnterCode()
        //{
        //    //if (User.IsInRole("Administrator,Editor"))
        //    //{
        //    //    TempData["Error"] = "Managers only!!!!";
        //    //}
        //    return View();
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> EnterCode(string email, string receiptNumber)
        //{
        //    if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(receiptNumber))
        //    {
        //        TempData["Error"] = "Моля, попълнете всички полета.";
        //        return View();
        //    }

        //    var user = await _context.Users
        //        .FirstOrDefaultAsync(u => u.Email == email);

        //    if (user == null)
        //    {
        //        TempData["Error"] = "Невалиден имейл.";
        //        return View();
        //    }
        //    //*******************
        //    var request = await _context.ManagerRequests
        //        .Include(r => r.Team)
        //        .FirstOrDefaultAsync(r => r.User.Email == email && !r.IsApproved);

        //    if (request == null)
        //    {
        //        TempData["Error"] = "Няма чакаща заявка за този имейл.";
        //        return RedirectToAction("EnterCode");
        //    }

        //    // Потвърждаваме заявката
        //    request.IsApproved = true;
        //    request.FeePaid = true;

        //    // Добавяме отбора към турнира, ако още не е
        //    var tournament = await _context.Tournaments
        //        .Include(t => t.Teams)
        //        .FirstOrDefaultAsync(t => t.Id == request.TournamentId);

        //if (tournament != null && !tournament.Teams.Any(t => t.Id == request.TeamId))
        //{
        //    var team = await _context.Teams.FindAsync(request.TeamId);
        //    if (team != null)
        //    {
        //        tournament.Teams.Add(team);
        //    }
        //    }

        //    // Записваме промените
        //    await _context.SaveChangesAsync();

        //    // Изпращаме SMS (по избор)
        //    await _smsSender.SendSmsAsync("+359885773102", $"✅ Отборът {request.Team.Name} е включен в турнира {tournament.Name}.");

        //    // Потвърждение към потребителя
        //    TempData["Message"] = "Участието е потвърдено.";
        //    return RedirectToAction("Index", "Home");
        //}
    }
}
