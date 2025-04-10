namespace Tournament.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using System.Threading.Tasks;
    using Tournament.Data;

    public class VerifyCodeController : Controller
    {
        private readonly TurnirDbContext _context;

        public VerifyCodeController(TurnirDbContext context)
        {
            _context = context;
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
                .FirstOrDefaultAsync(r => r.UserId == user.Id && !r.IsApproved);

            if (request == null)
            {
                var msg=$"Няма активна заявка за потвърждение. Заявката на мениджър с Е-мейл {user.Email} вдче е потвърдена IsApproved==true";
                TempData["Error"] = msg ;
                return View();
            }

            // ⚠️ Ако искаш, можеш да запазиш номера на разписката тук
            // request.ReceiptNumber = receiptNumber;

            request.IsApproved = true;
            request.FeePaid = true;

            await _context.SaveChangesAsync();

            TempData["Message"] = "✅ Участието е потвърдено. Можете да регистрирате реален отбор.";
            return RedirectToAction("Index", "Home");
        }

    }
}

//[HttpPost]
//public async Task<IActionResult> EnterCode(string code, string receiptNumber)
//{
//    if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(receiptNumber))
//    {
//        TempData["Error"] = "Моля, попълнете всички полета.";
//        return View();
//    }

//    var request = await _context.ManagerRequests
//        .Include(r => r.User)
//        .FirstOrDefaultAsync(r =>
//            r.UserId == code &&
//            r.IsApproved &&
//            !r.FeePaid);

//    if (request == null)
//    {
//        TempData["Error"] = "Невалиден код или вече е използван.";
//        return View();
//    }

//    // Задаваме номер на разписка и отбелязваме таксата като платена
//    request.ReceiptNumber = receiptNumber;
//    request.FeePaid = true;
//    await _context.SaveChangesAsync();

//    // Запазваме код в TempData за следващата стъпка
//    TempData["VerifiedManagerId"] = code;

//    return RedirectToAction("Create", "Teams");
//}
//}
