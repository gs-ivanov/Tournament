namespace Tournament.Services.Email
{
    using System.Threading.Tasks;

        public interface IEmailSender
        {
            Task SendAsync(string toEmail, string subject, string body);
        }
}
