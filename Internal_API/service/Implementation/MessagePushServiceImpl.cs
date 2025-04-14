

namespace Internal_API.service.Implementation
{
        public class MessagePushServiceImpl : IMessagePushService
    {
        private readonly List<SseClientImpl> _clients = new();

        public void Register(SseClientImpl client)
        {
            lock (_clients) _clients.Add(client);
        }

        public void Unregister(SseClientImpl client)
        {
            lock (_clients) _clients.Remove(client);
        }

        public async Task PushAsync(string message)
        {
            List<SseClientImpl> clientsCopy;
            lock (_clients) clientsCopy = new List<SseClientImpl>(_clients);

            foreach (var client in clientsCopy)
            {
                try { await client.SendAsync(message); }
                catch { /* handle disconnected clients if needed */ }
            }
        }
    }

}
