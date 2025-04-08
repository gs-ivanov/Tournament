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
        public async Task<IActionResult> EnterCode(string code, string receiptNumber)
        {
            if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(receiptNumber))
            {
                TempData["Error"] = "Моля, попълнете всички полета.";
                return View();
            }

            var request = await _context.ManagerRequests
                .Include(r => r.User)
                .FirstOrDefaultAsync(r =>
                    r.UserId == code &&
                    r.IsApproved &&
                    !r.FeePaid);

            if (request == null)
            {
                TempData["Error"] = "Невалиден код или вече е използван.";
                return View();
            }

            // Задаваме номер на разписка и отбелязваме таксата като платена
            request.ReceiptNumber = receiptNumber;
            request.FeePaid = true;
            await _context.SaveChangesAsync();

            // Запазваме код в TempData за следващата стъпка
            TempData["VerifiedManagerId"] = code;

            return RedirectToAction("Create", "Teams");
        }
    }
}
