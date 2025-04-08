namespace Tournament.Areas.Identity.Pages.Account
{
    using global::Tournament.Data;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using Microsoft.EntityFrameworkCore;
    using System.Threading.Tasks;

    public class ConfirmManagerRequestModel : PageModel
    {
        private readonly TurnirDbContext context;

        public ConfirmManagerRequestModel(TurnirDbContext context)
        {
            this.context = context;
        }

        [BindProperty]
        public string ConfirmationCode { get; set; }

        [BindProperty]
        public string Email { get; set; }

        [BindProperty]
        public string ReceiptNumber { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrWhiteSpace(ConfirmationCode) || string.IsNullOrWhiteSpace(Email))
            {
                TempData["Error"] = "Моля, въведете валиден код и имейл.";
                return Page();
            }

            var user = await context.Users
                .FirstOrDefaultAsync(u => u.Email == Email);

            if (user == null)
            {
                TempData["Error"] = "Грешен код или имейл.";
                return Page();
            }

            var request = await context.ManagerRequests
                .FirstOrDefaultAsync(r => r.UserId == user.Id && !r.IsApproved);

            if (request == null)
            {
                TempData["Error"] = "Няма чакаща заявка за този потребител или потребителят вече е одобрен.";
                return Page();
            }

            request.IsApproved = true;
            request.FeePaid = true;

            await context.SaveChangesAsync();

            TempData["Message"] = "✅ Участието е потвърдено. Отборът ще бъде включен в турнира.";
            return RedirectToPage("/Index");
        }
    }
}
