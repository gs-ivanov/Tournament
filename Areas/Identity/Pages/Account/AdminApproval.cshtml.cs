﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Tournament.Data; // или TurnirDbContext
using Tournament.Models; // за ManagerRequest, TournamentType, User
using Tournament.Services.Sms; // ако имаш ISmsSender


[Authorize(Roles = "Admin")]
public class AdminApprovalModel : PageModel
{
    private readonly TurnirDbContext _context;
    private readonly ISmsSender _smsSender;

    public AdminApprovalModel(TurnirDbContext context, ISmsSender smsSender)
    {
        _context = context;
        _smsSender = smsSender;
    }

    [BindProperty(SupportsGet = true)]
    public string UserId { get; set; }

    public async Task<IActionResult> OnPostAsync()
    {
        var request = await _context.ManagerRequests
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.UserId == UserId);

        if (request == null)
        {
            return NotFound();
        }

        request.IsApproved = true;
        request.Status = RequestStatus.Approved;

        await _context.SaveChangesAsync();

        // Изпращане на SMS (примерно)
        var message = $"Вашият код за регистриране на отбор е: {UserId}";
        await _smsSender.SendSmsAsync(request.User.PhoneNumber, message);

        TempData["Message"] = "Заявката беше одобрена и SMS беше изпратен.";
        return RedirectToPage("/Index");
    }
}
