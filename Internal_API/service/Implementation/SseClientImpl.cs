using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class SseClientImpl : IDisposable
{
    private readonly HttpResponse _response;
    private bool _isConnected = true;

    public SseClientImpl(HttpResponse response)
    {
        _response = response;

        _response.Headers["Content-Type"] = "text/event-stream";
        _response.Headers["Cache-Control"] = "no-cache";
        _response.Headers["Connection"] = "keep-alive";
        _response.Headers["Access-Control-Allow-Origin"] = "*"; // Optional for dev
    }

    public async Task SendAsync(string data)
    {
        if (!_isConnected) return;

        try
        {
            await _response.WriteAsync($"data: {data}\n\n", Encoding.UTF8);
            await _response.Body.FlushAsync();
        }
        catch (IOException)
        {
            _isConnected = false;
            // Log client disconnect
        }
        catch (ObjectDisposedException)
        {
            _isConnected = false;
        }
    }

    public async Task ListenAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested && _isConnected)
            {
                // Optionally send a heartbeat/ping every 30s to keep connection alive
                await _response.WriteAsync($": keep-alive\n\n");
                await _response.Body.FlushAsync();
                await Task.Delay(30000, cancellationToken); // 30s heartbeat
            }
        }
        catch (Exception)
        {
            _isConnected = false;
            // Optionally log disconnect
        }
    }

    public void Dispose()
    {
        _isConnected = false;
        // No explicit disposal needed for HttpResponse
    }
}
