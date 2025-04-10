namespace Tournament.Areas.Identity.Pages.Account
{
    using global::Tournament.Data;
    using global::Tournament.Data.Models;
    using global::Tournament.Models;
    using global::Tournament.Services.Sms;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Threading.Tasks;

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
        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (Input.BecomeManager && !Input.TournamentType.HasValue)
            {
                TempData["Error"] = "Моля, изберете тип турнир.";
                return Page();
            }

            // Опитваме в try само ако всичко е коректно
            try
            {
                // Намираме турнир по тип
                var tournament = await context.Tournaments
                    .Where(t => t.Type == Input.TournamentType && t.IsOpenForApplications)
                    .OrderBy(t => t.StartDate)
                    .FirstOrDefaultAsync();

                if (Input.BecomeManager && tournament == null)
                {
                    TempData["Error"] = "Няма наличен турнир от избрания тип, който е отворен за заявки. Опитайте с друг турнир";
                    return RedirectToPage("/Index");
                }

                // 1. Създаване на потребител (само ако всичко дотук е ок)
                var user = new User
                {
                    UserName = Input.Email,
                    Email = Input.Email,
                    FullName = Input.FullName
                };

                var result = await userManager.CreateAsync(user, Input.Password);

                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }

                    return Page();
                }

                // 2. Ако не е мениджър
                if (!Input.BecomeManager)
                {
                    await userManager.AddToRoleAsync(user, "Fan");
                    await signInManager.SignInAsync(user, isPersistent: false);
                    return LocalRedirect(returnUrl);
                }

                // 3. Мениджър — допълнителна проверка дали вече има заявка
                var existing = await context.ManagerRequests
                    .AnyAsync(r => r.UserId == user.Id && r.TournamentId == tournament.Id);

                if (existing)
                {
                    TempData["Error"] = "Вече сте кандидатствали за участие в този турнир.";
                    return RedirectToPage("/Index");
                }

                // 4. Добавяме роля и служебен отбор
                await userManager.AddToRoleAsync(user, "Editor");

                var team = new Team
                {
                    Name = "Временен отбор",
                    CoachName = "Временен треньор",
                    UserId = user.Id,
                    FeePaid = false
                };

                context.Teams.Add(team);
                await context.SaveChangesAsync(); // Тук вече имаме team.Id

                // 5. Заявка
                var request = new ManagerRequest
                {
                    UserId = user.Id,
                    TeamId = team.Id,
                    TournamentId = tournament.Id,
                    TournamentType = Input.TournamentType.Value,
                    Status = RequestStatus.Pending,
                    IsApproved = false,
                    FeePaid = false
                };

                context.ManagerRequests.Add(request);
                await context.SaveChangesAsync();

                // 6. Изпращаме SMS
                var smsText = $"Заявката за участие в турнира {Input.TournamentType} е приета. Ваш код: {user.Id}";
                await smsSender.SendSmsAsync("+359885773102", smsText);

                TempData["Message"] = "Регистрацията е успешна. Очаквайте одобрение.";
                return RedirectToPage("/Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Грешка: {ex.Message}";
                return RedirectToPage("/Index");
            }
        }
        ////public async Task<IActionResult> OnPostAsync()
        ////{
        ////    if (!ModelState.IsValid) return Page();

        ////    var user = new User
        ////    {
        ////        UserName = Input.Email,
        ////        Email = Input.Email
        ////    };

        ////    var result = await userManager.CreateAsync(user, Input.Password);

        ////    if (!result.Succeeded)
        ////    {
        ////        foreach (var error in result.Errors)
        ////            ModelState.AddModelError(string.Empty, error.Description);
        ////        return Page();
        ////    }

        ////    if (Input.BecomeManager && Input.TournamentType.HasValue)
        ////    {
        ////        // Проверка: дали вече има активна заявка от този потребител
        ////        var existingRequest = await context.ManagerRequests
        ////            .FirstOrDefaultAsync(r => r.UserId == user.Id && !r.IsApproved);

        ////        if (existingRequest != null)
        ////        {
        ////            TempData["Error"] = "Вече сте кандидатствали за участие като мениджър. Очаквайте одобрение.";
        ////            return RedirectToPage("/Index");
        ////        }

        ////        // 1. Намиране на валиден турнир
        ////        var tournament = await context.Tournaments
        ////            .Where(t => t.Type == Input.TournamentType && t.IsOpenForApplications)
        ////            .OrderBy(t => t.StartDate)
        ////            .FirstOrDefaultAsync();

        ////        if (tournament == null)
        ////        {
        ////            TempData["Error"] = "Няма наличен турнир от избрания тип, който е отворен за заявки. Опитайте с друг турнир";
        ////            return RedirectToPage("/Index");
        ////        }

        ////        // 2. Създаване на дефолтен отбор
        ////        var team = new Team
        ////        {
        ////            Name = "Временен отбор",
        ////            CoachName = "Временен треньор",
        ////            LogoUrl = "",
        ////            FeePaid = false,
        ////            UserId = user.Id
        ////        };

        ////        // 3. Създаване на заявка
        ////        var request = new ManagerRequest
        ////        {
        ////            UserId = user.Id,
        ////            TournamentType = Input.TournamentType.Value,
        ////            TournamentId = tournament.Id,
        ////            JsonPayload = $"{{ \"user\": \"{user.Email}\" }}",
        ////            Status = RequestStatus.Pending,
        ////            IsApproved = false
        ////        };

        ////        string message;

        ////        try
        ////        {
        ////            // Първо добавяме отбор (за да има ID за заявката)
        ////            context.Teams.Add(team);
        ////            await context.SaveChangesAsync();

        ////            // Свързваме заявката с току-що създадения отбор
        ////            request.TeamId = team.Id;
        ////            context.ManagerRequests.Add(request);
        ////            await context.SaveChangesAsync();

        ////            message = $"✅ Заявката за включване в турнира {Input.TournamentType} е приета.\nСлед превод на ХХХ лв. по сметка ###### в БАНКА, регистрирайте се отново с код: {user.UserName}";

        ////            await smsSender.SendSmsAsync("+359885773102", message);

        ////            TempData["Message"] = "Регистрацията е изпратена. Очаквайте одобрение по SMS.";

        ////            return RedirectToPage("/Index");
        ////        }
        ////        catch (Exception ex)
        ////        {
        ////            message = $"❌ Регистрацията не бе успешна. Моля, опитайте отново.";

        ////            TempData["Error"] = "Възникна проблем при записа. Моля, опитайте отново.";

        ////            await smsSender.SendSmsAsync("+359885773102", message);

        ////            return RedirectToPage("/Index");
        ////        }
        ////    }
        ////    return RedirectToPage("/Index");
        ////}
    }
}