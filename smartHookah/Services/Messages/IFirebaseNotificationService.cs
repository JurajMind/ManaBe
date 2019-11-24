using System.Collections.Generic;
using System.Threading.Tasks;

namespace smartHookah.Services.Messages
{
    public interface IFirebaseNotificationService
    {
        Task<bool> NotifyAsync(int personId, string title, string body, Dictionary<string, string> data);
    }
}