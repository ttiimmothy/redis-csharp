using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

public static class Program
{
  public static async Task Main()
  {
    var redis = new Redis();
    await redis.Start();
  }
}
public class Redis
{
  private readonly TcpListener _listener = new(IPAddress.Any, 6379);
  public async Task Start(CancellationToken token = default)
  {
    _listener.Start();
    while (!token.IsCancellationRequested)
    {
      var client = await _listener.AcceptTcpClientAsync().ConfigureAwait(false);
      _ = HandleClient(client, token);
    }
    1
    _listener.Stop();
  }
  private async Task HandleClient(TcpClient client, CancellationToken token = default)
  {
    using (client)
    {
      var buffer = new Byte[1024];
      var stream = client.GetStream();
      while (!token.IsCancellationRequested)
      {
        var bytes = await stream.ReadAsync(buffer, 0, buffer.Length, token).ConfigureAwait(false);
        if (bytes == 0)
        {
          return;
        }
        var data = Encoding.ASCII.GetString(buffer, 0, bytes);
        var lines = data.Split("\n") Select(x => x.Trim()).Where(x => !string.IsNullOrEmpty(x));
        foreach (var line in lines)
        {
          if (line.ToLower() == "ping")
          {
            var response = Encoding.UTF8.GetBytes("+PONG\r\n");
            await stream.WriteAsync(response, 0, response.Length, token).ConfigureAwait(false);
          }
          else
          {
            Console.WriteLine($"Unknown command '{line}'");
          }
        }
        if (socket.Poll(1000, SelectMode.SelectRead) && socket.Available == 0)
        {
          break;
        }
      }
      catch (SocketException e)
      {
        Console.WriteLine(e.Message);
        break;
      }
    }
  }
}