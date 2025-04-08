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
                // Проверка: дали вече има активна заявка от този потребител
                var existingRequest = await context.ManagerRequests
                    .FirstOrDefaultAsync(r => r.UserId == user.Id && !r.IsApproved);

                if (existingRequest != null)
                {
                    TempData["Error"] = "Вече сте кандидатствали за участие като мениджър. Очаквайте одобрение.";
                    return RedirectToPage("/Index");
                }

                // 1. Намиране на валиден турнир
                var tournament = await context.Tournaments
                    .Where(t => t.Type == Input.TournamentType && t.IsOpenForApplications)
                    .OrderBy(t => t.StartDate)
                    .FirstOrDefaultAsync();

                if (tournament == null)
                {
                    TempData["Error"] = "Няма наличен турнир от избрания тип, който е отворен за заявки. Опитайте с друг турнир";
                    return RedirectToPage("/Index");
                }

                // 2. Създаване на дефолтен отбор
                var team = new Team
                {
                    Name = "Временен отбор",
                    CoachName = "Временен треньор",
                    LogoUrl = "",
                    FeePaid = false,
                    UserId = user.Id
                };

                // 3. Създаване на заявка
                var request = new ManagerRequest
                {
                    UserId = user.Id,
                    TournamentType = Input.TournamentType.Value,
                    TournamentId = tournament.Id,
                    JsonPayload = $"{{ \"user\": \"{user.Email}\" }}",
                    Status = RequestStatus.Pending,
                    IsApproved = false
                };

                string message;

                try
                {
                    // Първо добавяме отбор (за да има ID за заявката)
                    context.Teams.Add(team);
                    await context.SaveChangesAsync();

                    // Свързваме заявката с току-що създадения отбор
                    request.TeamId = team.Id;
                    context.ManagerRequests.Add(request);
                    await context.SaveChangesAsync();

                    message = $"✅ Заявката за включване в турнира {Input.TournamentType} е приета.\nСлед превод на ХХХ лв. по сметка ###### в БАНКА, регистрирайте се отново с код: {user.UserName}";

                    await smsSender.SendSmsAsync("+359885773102", message);

                    TempData["Message"] = "Регистрацията е изпратена. Очаквайте одобрение по SMS.";

                    return RedirectToPage("/Index");
                }
                catch (Exception ex)
                {
                    message = $"❌ Регистрацията не бе успешна. Моля, опитайте отново.";

                    TempData["Error"] = "Възникна проблем при записа. Моля, опитайте отново.";

                    await smsSender.SendSmsAsync("+359885773102", message);

                    return RedirectToPage("/Index");
                }

                // 4. Изпращане на SMS (винаги – със съответното съобщение)
                //await smsSender.SendSmsAsync("+359885773102", message);

                // 5. Изход и потвърждение
                //TempData["Message"] = "Регистрацията е изпратена. Очаквайте одобрение по SMS.";
                //return RedirectToPage("/Index");

                //    // 1. Създаване на дефолтен отбор
                //    var team = new Team
                //    {
                //        Name = "Временен отбор",
                //        CoachName = "Временен треньор",
                //        LogoUrl = "",
                //        FeePaid = false,
                //        UserId = user.Id
                //    };

                //    //context.Teams.Add(team);
                //    //await context.SaveChangesAsync();

                //    // 2. Създаване на заявка
                //    // Намираме турнир от съответен тип, който е отворен за заявки
                //    var tournament = await context.Tournaments
                //        .Where(t => t.Type == Input.TournamentType && t.IsOpenForApplications)
                //        .OrderBy(t => t.StartDate)
                //        .FirstOrDefaultAsync();

                //    if (tournament == null)
                //    {
                //        TempData["Error"] = "Няма наличен турнир от избрания тип, който е отворен за заявки.";
                //        return RedirectToPage("/Index");
                //    }

                //    var request = new ManagerRequest
                //    {
                //        UserId = user.Id,
                //        TeamId = team.Id,
                //        TournamentType = Input.TournamentType.Value,
                //        TournamentId = tournament.Id, // ✅ задаваме го динамично
                //        JsonPayload = $"{{ \"user\": \"{user.Email}\" }}",
                //        Status = RequestStatus.Pending,
                //        IsApproved = false
                //    };

                //    var message = "";// $"Заявката за включване в турнира {Input.TournamentType} е приета.\nСлед превод на ХXXX лв. по сметка No ###### в БАНКА, регистрирайте се отново с код: {user.PhoneNumber}";

                //    try
                //    {
                //        context.Teams.Add(team);
                //        //await context.SaveChangesAsync();
                //        context.ManagerRequests.Add(request);
                //        await context.SaveChangesAsync();

                //    }
                //    catch (System.Exception)
                //    {
                //         message = $"Заявката за включване в турнира {Input.TournamentType} е приета.\nСлед превод на ХXXX лв. по сметка No ###### в БАНКА, регистрирайте се отново с код: {user.PhoneNumber}";
                //        await smsSender.SendSmsAsync("+359885773102", message); // ← Твоят номер
                //        TempData["Message"] = "Регистрацията не е успешна. Опитайте пак.";
                //        return RedirectToPage("/Index");
                //    }

                //    // 3. Изпращане на SMS
                //    message = $"Заявката за включване в турнира {Input.TournamentType} не е приета.\nОпитайте пак.";
                //        await smsSender.SendSmsAsync("+359885773102", message); // ← Твоят номер
                //}

                //// 4. Изход от системата
                //TempData["Message"] = "Регистрацията е успешна. \nОчаквайте одобрение след превод на посочената в СМС-а сума и регистрация след това.";
            }
            return RedirectToPage("/Index");
        }
    }
}