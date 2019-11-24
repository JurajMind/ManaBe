using smartHookah.Models.Dto;
using System.Collections.Generic;

namespace smartHookah.Services.Messages
{
    public interface IMessageLogService
    {
        void AddMessage(string text, string code, MessageType type);

        void AddMessage(DtoMessage message);

        List<DtoMessage> GetMessages();
    }
}