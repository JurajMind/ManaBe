using System.Collections.Generic;

namespace smartHookah.Models.Dto
{
    public class MessageDto : IMessageDto
    {
        public List<DtoMessage> Messages { get; set; }
    }

    public interface IMessageDto
    {
        List<DtoMessage> Messages { get; set; }
    }

    public class DtoMessage
    {
        public string Text { get; set; }

        public string Code { get; set; }

        public MessageType Type;
    }

    public enum MessageType
    {
        Info,
        Warning,
        Error
    }
}