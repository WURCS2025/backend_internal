namespace Internal_API.service.Implementation
{
    public class SseClientImpl
    {
        private readonly HttpResponse _response;

        public SseClientImpl(HttpResponse response)
        {
            _response = response;
            _response.Headers.Add("Content-Type", "text/event-stream");
            _response.Headers.Add("Cache-Control", "no-cache");
            _response.Headers.Add("Connection", "keep-alive");
        }

        public async Task SendAsync(string data)
        {
            await _response.WriteAsync($"data: {data}\n\n");
            await _response.Body.FlushAsync();
        }

        public async Task ListenAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(1000, cancellationToken);
            }
        }
    }

}
