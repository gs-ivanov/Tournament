namespace Tournament.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Tournament.Data;
    using Tournament.Models;

    [Authorize(Roles = "Administrator")]
    public class ManagerRequestsController : Controller
    {
        private readonly TurnirDbContext _context;

        public ManagerRequestsController(TurnirDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string status = "Pending")
        {
            var query = _context.ManagerRequests
                .Include(r => r.Team)
                .Include(r => r.User)
                .AsQueryable();

            if (Enum.TryParse<RequestStatus>(status, out var parsedStatus))
            {
                query = query.Where(r => r.Status == parsedStatus);
            }

            ViewData["CurrentStatus"] = status;
            return View(await query.ToListAsync());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var request = await _context.ManagerRequests
                .Include(r => r.Team)
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null)
                return NotFound();

            request.Status = RequestStatus.Approved;
            request.Team.FeePaid = true;

            await _context.SaveChangesAsync();

            TempData["Message"] = $"✅ Заявката от {request.User.FullName} за отбор '{request.Team.Name}' беше одобрена.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            var request = await _context.ManagerRequests
                .Include(r => r.User)
                .Include(r => r.Team)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null)
                return NotFound();

            request.Status = RequestStatus.Rejected;

            await _context.SaveChangesAsync();

            TempData["Message"] = $"❌ Заявката от {request.User.FullName} за отбор '{request.Team.Name}' беше отхвърлена.";
            return RedirectToAction(nameof(Index));
        }
    }
}
