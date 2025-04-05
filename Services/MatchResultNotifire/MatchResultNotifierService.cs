namespace Tournament.Services.MatchResultNotifire
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Net.Mail;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using Tournament.Data;
    using Tournament.Models;
    using Twilio;
    using Twilio.Rest.Api.V2010.Account;
    using Twilio.Types;

    public class MatchResultNotifierService: IMatchResultNotifierService
    {
        private readonly TurnirDbContext _context;

        public MatchResultNotifierService(TurnirDbContext context)
        {
            _context = context;
        }

        public async Task NotifyAsync(int matchId)
        {
            var match = await _context.Matches.FindAsync(matchId);
            if (match == null || match.ScoreA == null || match.ScoreB == null)
                return;

            var subs = await _context.MatchSubscriptions
                .Include(s => s.User)
                .Where(s => s.MatchId == matchId)   // && s.Type == NotificationType.Email)
                .ToListAsync();

            foreach (var sub in subs)
            {
                if (sub.Type==NotificationType.Email)
                {
                    //await SendGmailAsync(sub.User.Email, $"Резултат от мач", $"{match.ScoreA} - {match.ScoreB}");
                    ////await SendEmailAsync(sub.User.Email, $"Резултат от мач", $"{match.ScoreA} - {match.ScoreB}");

                }
                else
                {
                    //await SendSmsAsync(sub.User.PhoneNumber, $"Резултат от мач: {match.ScoreA} - {match.ScoreB}");
                }
            }

        }

        //// Corected code Email to Gmail
        //private async Task SendEmailAsync(string toEmail, string subject, string body)
        //{
        //    var message = new MailMessage();
        //    message.To.Add(toEmail);
        //    message.Subject = subject;
        //    message.Body = body;
        //    message.From = new MailAddress("yourgmail@gmail.com");

        //    using var smtp = new SmtpClient("smtp.gmail.com", 587)
        //    {
        //        Credentials = new NetworkCredential("yourgmail@gmail.com", "ВАШИЯТ_16_ЦИФРЕН_APP_PASSWORD"),
        //        EnableSsl = true
        //    };

        //    await smtp.SendMailAsync(message);
        //}


        ////Email to Gmail

        //private async Task SendGmailAsync(string toEmail, string subject, string body)
        //{
        //    var message = new MailMessage();
        //    message.To.Add(toEmail);
        //    message.Subject = subject;
        //    message.Body = body;
        //    message.From = new MailAddress("yourgmail@gmail.com");

        //    using var smtp = new SmtpClient("smtp.gmail.com", 587)
        //    {
        //        //Credentials = new NetworkCredential("gs.ivanov50@gmail.com", ""),
        //        //EnableSsl = true
        //    };

        //    await smtp.SendMailAsync(message);
        //}

        //Email to Mailtrap
        //private async Task SendEmailAsync(string toEmail, string subject, string body)
        //{
        //    var message = new MailMessage();
        //    message.To.Add(toEmail);
        //    message.Subject = subject;
        //    message.Body =$"Rezultata ot matcha e {body}" ;
        //    message.From = new MailAddress("from@example.com");


        //    using var smtp = new SmtpClient("sandbox.smtp.mailtrap.io", 2525)
        //    {
        //        Credentials = new NetworkCredential("baeb7fd6d8255d", "a8900ef61c0ef1"),
        //        EnableSsl = true
        //    };
        //    await smtp.SendMailAsync(message);
        //}


        //public async Task SendSmsAsync(string toPhone, string body)
        //{

        //}

        public async Task SendSmsAsync(string toPhone, string body)
        {
            const string accountSid = "ACed45f427b2150239e5694f353a2dfccf";
            const string authToken = "3e1c6bdbb3e8ac9468a17dd1354881f0";

            TwilioClient.Init(accountSid, authToken);

            var message = await MessageResource.CreateAsync(
                to: new PhoneNumber("+359885773102"),         // +359..., +1..., и т.н.
                from: new PhoneNumber("+19519440626"),
                body: body
            );

            Console.WriteLine($"SMS изпратен с SID: {message.Sid}");
        }

    }
}
