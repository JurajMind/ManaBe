namespace smartHookah
{
    public interface IEmailService
    {
        void SendTemplateAsync(string mailAdress, string subject, string template, object model);
    }
}