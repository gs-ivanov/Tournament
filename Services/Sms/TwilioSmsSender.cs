namespace Tournament.Services.Sms
{
    using Microsoft.Extensions.Configuration;
    using System.Threading.Tasks;
    using Twilio;
    using Twilio.Rest.Api.V2010.Account;
    using Twilio.Types;

    public class TwilioSmsSender : ISmsSender
    {
        private readonly IConfiguration _configuration;

        public TwilioSmsSender(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendSmsAsync(string toPhone, string body)
        {
            var config = _configuration.GetSection("Twilio");
            var accountSid = config["AccountSid"];
            var authToken = config["AuthToken"];
            var fromNumber = config["FromNumber"];

            TwilioClient.Init(accountSid, authToken);

            var message = await MessageResource.CreateAsync(
                to: new PhoneNumber(toPhone),
                from: new PhoneNumber(fromNumber),
                body: body
            );

            System.Console.WriteLine($"SMS изпратен с SID: {message.Sid}");
        }
    }
}
