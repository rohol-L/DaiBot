namespace DaiBot.Core.Interface
{
    public interface IHandlerAsync : IHandlerBase
    {
        public Task<Response?> HandleAsync(MessageContext context);
    }
}
