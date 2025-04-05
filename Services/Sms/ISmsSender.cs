namespace Tournament.Services.Sms
{
    using System.Threading.Tasks;

    public interface ISmsSender
    {
        Task SendSmsAsync(string toPhone, string body);
    }
}
