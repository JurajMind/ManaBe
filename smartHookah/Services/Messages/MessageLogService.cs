using smartHookah.Models.Dto;
using System.Collections.Generic;
using MessageType = smartHookah.Models.Dto.MessageType;


namespace smartHookah.Services.Messages
{
    public class MessageLogService : IMessageLogService
    {
        private readonly List<DtoMessage> messages;
        MessageLogService()
        {
            messages = new List<DtoMessage>();
        }

        public List<DtoMessage> GetMessages()
        {
            return messages;
        }

        public void AddMessage(string text, string code, MessageType type)
        {
            this.AddMessage(new DtoMessage
            {
                Type = type,
                Code = code,
                Text = text
            });
        }

        public void AddMessage(DtoMessage message)
        {
            this.messages.Add(message);
        }
    }
}