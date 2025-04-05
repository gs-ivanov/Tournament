namespace Tournament.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using System.Linq;
    using System.Threading.Tasks;
    using Tournament.Data;
    using Tournament.Models.Menagers;

    [Authorize(Roles = "Editor")]
    public class MenagerController : Controller
    {
        private readonly TurnirDbContext _context;

        public MenagerController(TurnirDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value;

            var team = await _context.Teams.FirstOrDefaultAsync(t => t.UserId == userId);
            var request = await _context.ManagerRequests
                .Include(r => r.Team)
                .FirstOrDefaultAsync(r => r.UserId == userId);

            var model = new MenagerDashboardViewModel
            {
                Team = team,
                Request = request
            };

            return View(model);
        }
    }

}
