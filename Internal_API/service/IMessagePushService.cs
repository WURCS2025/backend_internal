
using Internal_API.service.Implementation;

namespace Internal_API.service
{
    public interface IMessagePushService
    {
        Task PushAsync(string message);
        void Register(SseClientImpl client);
        void Unregister(SseClientImpl client);
    }

}
