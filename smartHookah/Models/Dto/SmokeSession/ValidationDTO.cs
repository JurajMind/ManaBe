namespace smartHookah.Models.Dto
{
    using System;

    public class ValidationDTO : DTO
    {
        public String Id { get; set; }
        public SessionState Flag { get; set; }
    }

    public enum SessionState
    {
        Completed, Live
    }
}