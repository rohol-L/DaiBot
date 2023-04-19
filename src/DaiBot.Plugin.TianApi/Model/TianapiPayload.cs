namespace DaiBot.Plugin.TianApi.Model
{
    public class TianapiPayload<T>
    {
        public int Code { get; set; }
        public string? Msg { get; set; }
        public T? Result { get; set; }
    }

    public class TianapiContent
    {
        public string? Content { get; set; }
    }
    public class TianapiNewsList
    {
        public List<TianapiNews> Newslist { get; set; } = new();
    }

    public class TianapiNews
    {
        public string? Title { get; set; }
    }
}
