namespace DaiBot.Core.Interface
{
    public interface IBotConfig
    {
        public string BotName { get; }

        public string? this[string key] { get; }

        public void Save();

        public void ReLoad();

        public T? Value<T>(string key);

        public List<string> GetList(string key);
    }
}
