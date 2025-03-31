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
    using Tournament.Models.Teams;

    using static Tournament.WebConstants;

    [Authorize]
    public class TeamsController : Controller
    {
        private readonly TurnirDbContext data;

        public TeamsController(TurnirDbContext data)
        {
            this.data = data;
        }

        public async Task<IActionResult> Index()
        {
            var teams = await data.Teams
                .OrderByDescending(t => t.Points)
                .ThenByDescending(t => t.GoalsScored)
                .ToListAsync();
            return View(teams);
        }

        [Authorize(Roles = "Administrator")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Create(Team team)
        {
            if (ModelState.IsValid)
            {
                data.Add(team);
                await data.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(team);
        }

        [Authorize(Roles = "Administrator")]
        public IActionResult CreateMultiple()
        {
            var defaultTeams =new TeamFormModel {TeamCount=16 };
            return View(defaultTeams);
        }

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> CreateMultiple(TeamFormModel team)
        {
            if (team.TeamCount == 0)
            {
                TempData[GlobalMessageKey] = "Списък от нула отбори е невъзможен!? избери 'Settings ...'.";

                return RedirectToAction(nameof(Index));
            }

            var teams = SeedTeams();
            var actualNumberTeams = teams
                .Take(team.TeamCount)
                .ToList();

            if (this.data.Teams.Count() != 0)
            {
                TempData[GlobalMessageKey] = "Има стари записи в БД, за изчистване избери 'Settings ...'.";
                return RedirectToAction(nameof(Index));
            }

                await data.AddRangeAsync(actualNumberTeams);
                await data.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var team = await data.Teams.FindAsync(id);
            if (team == null) return NotFound();

            return View(team);
        }

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Edit(int id, Team team)
        {
            if (id != team.Id) return NotFound();

            if (ModelState.IsValid)
            {
                data.Update(team);
                await data.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(team);
        }

        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var team = await data.Teams.FindAsync(id);
            if (team == null) return NotFound();

            return View(team);
        }

        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var team = await data.Teams.FindAsync(id);
            if (team != null)
            {
                data.Teams.Remove(team);
                await data.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Details(int id)
        {
            var match = this.data.Matches
                .Where(m => m.HomeTeamId == id && m.HomeTeamGoals != null)
                .Select(m => m)
                .ToList();

            var team = this.data.Teams
                .Where(t => t.Id == id)
                .Select(t => new TeamDetailsViewModel
                {
                    Id = t.Id,
                    Name = t.Name,
                    Logo = t.Logo,
                    MatchNumbers = t.MatchNumbers,
                    Points = t.Points,
                    Wins = t.Wins,
                    Losts = t.Losts,
                    GoalsScored = t.GoalsScored,
                    GoalsConceded = t.GoalsConceded,
                    Draws = t.Draws
                })
                .FirstOrDefault();

            if (team == null)
            {
                return NotFound();
            }

            return View(team);
        }

        [Authorize(Roles = "Administrator")]
        public IActionResult Reset()
        {
            var teamsExists = data.Teams.ToList();

            if (teamsExists.Count == 0)
            {
                TempData[GlobalMessageKey] = "Списъкът вече е нулиран. Ако искаш нов списък, избери 'Settings ...'.";

                return RedirectToAction(nameof(Index), "Teams");
            }

            string mesage = "READY TO REMOVE ALL RECORDS IN TEAMS TABLE!!!";
            ViewBag.Msg = mesage;
            return View();
        }

        [HttpPost, ActionName("Reset")]
        [Authorize(Roles = "Administrator")]
        public IActionResult ResetConfirmed()
        {
            List<Match> itemsToDelete = this.data.Matches.ToList();

            this.data.Matches.RemoveRange(itemsToDelete);
            this.data.SaveChanges();

            // Нулиране на статистиките на отборите
            var teams = this.data.Teams.ToList();
            this.data.Teams.RemoveRange(teams);
            this.data.SaveChanges();

            ViewBag.TempDataType = "Success";
            TempData[GlobalMessageKey] = "Списъкат е нулиран успешно. За нов списък - избери 'Generate Schedule'.";

            //return RedirectToAction(nameof(AllTeams));
            return RedirectToAction("Index", "Teams");

        }



        private List<Team> SeedTeams()
        {

            List<string> teamNames = new List<string>() { "Лудогорец", "Крумовград", "Левски София", "Локомотив Пловдив", "Славия София", "Черно море", "Арда", "Ботев Враца", "ЦСКА София", "Септември София", "Спартак Варна", "Ботев Пловдив", "Ботев Враца", "Берое", "Хебър", "ЦСКА 1948" };
            List<string> teamLogos = new List<string>() { "/logos/ludogorec.png", "/logos/krumovgrad.png", "/logos/levski.png", "/logos/lokomotivplovdiv.png", "/logos/slavia.png", "/logos//logos/chernomore.png", "/logos/arda.png", "/logos/botevvraca.png", "/logos/cskasofia.png", "/logos/septemvri.png", "/logos/spartakvarna.png", "/logos/botevplovdiv.png", "/logos/botevvraca.png", "/logos/beroe.png", "/logos/hebar.png", "/logos/cska1948.png" };

            List<Team> teams = new()
            {
                new Team { Name = teamNames[0], Logo = teamLogos[0] },
                new Team { Name = teamNames[1], Logo = teamLogos[1] },
                new Team { Name = teamNames[2], Logo = teamLogos[2] },
                new Team { Name = teamNames[3], Logo = teamLogos[3] },
                new Team { Name = teamNames[4], Logo = teamLogos[4] },
                new Team { Name = teamNames[5], Logo = teamLogos[5] },
                new Team { Name = teamNames[6], Logo = teamLogos[6] },
                new Team { Name = teamNames[7], Logo = teamLogos[7] },
                new Team { Name = teamNames[8], Logo = teamLogos[8] },
                new Team { Name = teamNames[9], Logo = teamLogos[9] },
                new Team { Name = teamNames[10], Logo = teamLogos[10] },
                new Team { Name = teamNames[11], Logo = teamLogos[11] },
                new Team { Name = teamNames[12], Logo = teamLogos[12] },
                new Team { Name = teamNames[13], Logo = teamLogos[13] },
                new Team { Name = teamNames[14], Logo = teamLogos[14] },
                new Team { Name = teamNames[15], Logo = teamLogos[15] },
            };

            return teams;
        }

    }
}