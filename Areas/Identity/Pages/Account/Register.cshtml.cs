namespace Tournament.Areas.Identity.Pages.Account
{
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using Microsoft.EntityFrameworkCore;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Threading.Tasks;
    using Tournament.Data;
    using Tournament.Data.Models;
    using Tournament.Models;
    using Tournament.Services.Sms;

    public class RegisterModel : PageModel
    {
        private readonly UserManager<User> userManager;
        private readonly SignInManager<User> signInManager;
        private readonly TurnirDbContext context;
        private readonly ISmsSender smsSender;

        public RegisterModel(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            TurnirDbContext context,
            ISmsSender smsSender)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.context = context;
            this.smsSender = smsSender;
        }

        [BindProperty]
        public InputModel Input { get; set; }
        public string ReturnUrl { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            [Display(Name = "Имейл")]
            public string Email { get; set; }

            [Required]
            [Display(Name = "Парола")]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Required]
            [Display(Name = "Пълно име")]
            public string FullName { get; set; }

            [Required]
            [DataType(DataType.Password)]
            [Display(Name = "Потвърди паролата")]
            [Compare("Password", ErrorMessage = "Паролите не съвпадат.")]
            public string ConfirmPassword { get; set; }

            [Display(Name = "Стани мениджър")]
            public bool BecomeManager { get; set; }

            [Display(Name = "Тип турнир")]
            public TournamentType? TournamentType { get; set; }
        }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var user = new User
            {
                UserName = Input.Email,
                Email = Input.Email
            };

            var result = await userManager.CreateAsync(user, Input.Password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
                return Page();
            }

            if (Input.BecomeManager && Input.TournamentType.HasValue)
            {
                // 1. Създаване на дефолтен отбор
                var team = new Team
                {
                    Name = "Временен отбор",
                    CoachName = "Временен треньор",
                    LogoUrl = "",
                    FeePaid = false,
                    UserId = user.Id
                };

                context.Teams.Add(team);
                await context.SaveChangesAsync();

                // 2. Създаване на заявка
                // Намираме турнир от съответен тип, който е отворен за заявки
                var tournament = await context.Tournaments
                    .Where(t => t.Type == Input.TournamentType && t.IsOpenForApplications)
                    .OrderBy(t => t.StartDate)
                    .FirstOrDefaultAsync();

                if (tournament == null)
                {
                    TempData["Error"] = "Няма наличен турнир от избрания тип, който е отворен за заявки.";
                    return RedirectToPage("/Index");
                }

                var request = new ManagerRequest
                {
                    UserId = user.Id,
                    TeamId = team.Id,
                    TournamentType = Input.TournamentType.Value,
                    TournamentId = tournament.Id, // ✅ задаваме го динамично
                    JsonPayload = $"{{ \"user\": \"{user.Email}\" }}",
                    Status = RequestStatus.Pending,
                    IsApproved = false
                };

                context.ManagerRequests.Add(request);
                await context.SaveChangesAsync();

                // 3. Изпращане на SMS
                var message = $"Заявката за включване в турнира {Input.TournamentType} е приета.\nСлед превод на Х лв. по сметка ХХХХ БАНКА, регистрирайте отново с код: {user.Id}";
                await smsSender.SendSmsAsync("+359885773102", message); // ← Твоят номер
            }

            // 4. Изход от системата
            TempData["Message"] = "Регистрацията е успешна. Очаквайте одобрение.";
            return RedirectToPage("/Index");
        }
    }
}