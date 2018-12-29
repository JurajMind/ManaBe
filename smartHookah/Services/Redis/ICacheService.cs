namespace smartHookah.Services.Redis
{
    public interface ICacheService
    {
        void Store<T>(string key, T value);

        T Get<T>(string key);

        void Invaldate(string key);
    }
}