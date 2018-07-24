namespace smartHookahCommon
{
    public interface IRedisService
    {
        string GetHookahId(string sessionId);
    }
}