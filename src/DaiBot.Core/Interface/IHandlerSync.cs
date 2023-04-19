namespace DaiBot.Core.Interface
{
    public interface IHandler : IHandlerBase
    {
        public Response? Handle(MessageContext context);
    }
}
