namespace Tournament.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Tournament.Services.Email;
    using System.Threading.Tasks;

    public class TestEmailController : Controller
    {
        private readonly IEmailSender _emailSender;

        public TestEmailController(IEmailSender emailSender)
        {
            _emailSender = emailSender;
        }

        public async Task<IActionResult> Send()
        {
            var to = "gs.ivanov50@gmail.com"; // замени с твоя адрес
            var subject = "⚽ Тестване на Email от Tournament";
            var body = "Това е тестово известие от приложението Tournament.\nУспешно сме свързали Gmail SMTP.";

            await _emailSender.SendAsync(to, subject, body);

            return Content("✅ Изпратено успешно!");
        }

        public IActionResult Test() => View();
    }
}
