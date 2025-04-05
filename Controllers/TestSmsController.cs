namespace Tournament.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using System.Threading.Tasks;
    using Tournament.Services.Sms;

    public class TestSmsController : Controller
    {
        private readonly ISmsSender _smsSender;

        public TestSmsController(ISmsSender smsSender)
        {
            _smsSender = smsSender;
        }

        public async Task<IActionResult> Send()
        {
            var to = "+359885773102"; // или друг твой номер
            var message = "⚽ Test SMS от Tournament чрез Twilio.";

            await _smsSender.SendSmsAsync(to, message);

            return Content("✅ SMS изпратен успешно!");
        }
    }
}
