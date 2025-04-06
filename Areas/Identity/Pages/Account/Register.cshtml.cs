namespace Tournament.Areas.Identity.Pages.Account
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Microsoft.EntityFrameworkCore;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Threading.Tasks;
    using Tournament.Data;
    using Tournament.Data.Models;
    using Tournament.Models;

    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly TurnirDbContext context;
        private readonly SignInManager<User> signInManager;
        private readonly UserManager<User> userManager;

        public RegisterModel(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            TurnirDbContext context)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.context = context;
        }

        public List<SelectListItem> AvailableTournaments { get; set; }


        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Role { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            [Display(Name = "Имейл")]
            public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "Паролата трябва да е поне {2} символа.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Парола")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Потвърди паролата")]
            [Compare("Password", ErrorMessage = "Паролите не съвпадат.")]
            public string ConfirmPassword { get; set; }

            [Display(Name = "Стани мениджър")]
            public bool BecomeManager { get; set; }

            [Required]
            [Display(Name = "Турнир")]
            public int TournamentId { get; set; }


            [Display(Name = "Тип турнир")]
            public TournamentType? TournamentType { get; set; }

            [Required]
            [Display(Name = "Име и фамилия")]
            public string FullName { get; set; }
        }


        public async Task OnGetAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            // Автоматично избира първия отворен турнир (или фиксиран Id = 1)
            var openTournament = await context.Tournaments
                .Where(t => t.IsOpenForApplications)
                .OrderBy(t => t.StartDate)
                .FirstOrDefaultAsync();

            Input = new InputModel
            {
                TournamentId = openTournament?.Id ?? 1 // ако няма нищо в базата
            };
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            if (ModelState.IsValid)
            {
                var user = new User
                {
                    UserName = Input.Email,
                    Email = Input.Email,
                    FullName = Input.FullName
                };

                var result = await this.userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    if (Input.BecomeManager)
                    {
                        // Добавяме в роля Editor
                        await userManager.AddToRoleAsync(user, "Editor");

                        // Създаваме заявка
                        var request = new ManagerRequest
                        {
                            UserId = user.Id,
                            TournamentId = Input.TournamentId, // ✅ ВАЖНО!
                            TournamentType = Input.TournamentType.Value,
                            JsonPayload = ManagerRequest.GenerateJson(user.Email, Input.TournamentType.Value),
                            Status = RequestStatus.Pending
                        };

                        context.ManagerRequests.Add(request);
                        await context.SaveChangesAsync();
                    }
                    else
                    {
                        await userManager.AddToRoleAsync(user, "Fan");
                    }
                    //var roleFromQuery = Request.Query["role"].ToString();

                    if (Role == "Manager")
                    {
                        await userManager.AddToRoleAsync(user, "Editor");  // добавяне в роля „Менажер“
                        user.IsManager = true;                              // задаване на флага
                        await userManager.UpdateAsync(user);               // задължително – запазва промените
                    }

                    await signInManager.SignInAsync(user, isPersistent: false);
                    return LocalRedirect(returnUrl);
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return Page();
        }
    }
}