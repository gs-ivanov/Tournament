namespace Tournament.Areas.Identity.Pages.Account
{
    using System.ComponentModel.DataAnnotations;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using Microsoft.EntityFrameworkCore;
    using Tournament.Data;
    using Tournament.Data.Models;
    using Tournament.Services.Sms;

    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private readonly UserManager<User> userManager;
        private readonly SignInManager<User> signInManager;
        private readonly TurnirDbContext context;
        private readonly ISmsSender smsSender;




        public LoginModel(
            SignInManager<User> signInManager,
            TurnirDbContext context,
            ISmsSender smsSender
            )
        {
            this.signInManager = signInManager;
            this.context = context;
            this.smsSender = smsSender;
        }
        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Display(Name = "Remember Me?")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl ??= Url.Content("~/");

            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var result = await signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                var user = await userManager.FindByEmailAsync(Input.Email);

                var approvedRequest = await context.ManagerRequests
                    .Include(r => r.Team)
                    .FirstOrDefaultAsync(r =>
                        r.UserId == user.Id &&
                        r.IsApproved &&
                        (r.Team.Name == "Временен отбор" || string.IsNullOrEmpty(r.Team.Name)));

                if (approvedRequest != null)
                {
                    // Насочваме към въвеждане на код
                    TempData["VerifiedManagerId"] = user.Id;
                    return RedirectToAction("EnterCode", "VerifyCode");
                }

                return LocalRedirect(returnUrl);
            }

            ModelState.AddModelError(string.Empty, "Грешен опит за вход.");
            return Page();
        }
    }
}