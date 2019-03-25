using System.Threading.Tasks;

namespace smartHookah.Services.Messages
{
    public interface IFirebaseNotificationService
    {
        Task<bool> NotifyAsync(string to, string title, string body);
    }
}