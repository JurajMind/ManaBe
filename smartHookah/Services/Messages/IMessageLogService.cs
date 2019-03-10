using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using smartHookah.Models.Dto;

namespace smartHookah.Services.Messages
{
    public interface IMessageLogService
    {
        void AddMessage(string text, string code, MessageType type);

        void AddMessage(DtoMessage message);

        List<DtoMessage> GetMessages();
    }
}