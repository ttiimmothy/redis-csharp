using System.Collections.Concurrent;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using codecrafters_redis;

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
  private readonly ConcurrentDictionary<string, string> _values = new();
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
        Console.WriteLine("Received: {0}", data.Replace("\r\n", "|"));
        var cmd = new RespParser().Parse(data) as RespArray;
        if (cmd == null)
        {
          Console.WriteLine($"Unknown command '{data}'");
          continue;
        }
        byte[]? response = null;
        switch (cmd.Items[0])
        {
          case RespString rs:
            switch (rs.Value.ToLower())
            {
              case "ping":
                response = Encoding.ASCII.GetBytes("+PONG\r\n");
                break;
              case "echo":
                var echoString = (RespString)cmd.Items[1];
                response = Encoding.ASCII.GetBytes($"+{echoString.Value}\r\n");
                break;
              case "set":
                var key = (RespString)cmd.Items[1];
                var value = (RespString)cmd.Items[2];
                _values[key.Value] = value.Value;
                response = Encoding.ASCII.GetBytes("+OK\r\n");
                break;
              case "get":
                var getKey = (RespString)cmd.Items[1];
                if (_values.TryGetValue(getKey.Value, out var getValue))
                {
                  response = Encoding.ASCII.GetBytes($"${getValue.Length}\r\n{getValue}\r\n");
                }
                break;
              default:
                Console.WriteLine($"Unknown command '{rs.Value}'");
                break;
            }
            break;
          default:
            Console.WriteLine("Unknown type");
            break;
        }
        if (response != null)
        {
          await stream.WriteAsync(response, 0, response.Length, token).ConfigureAwait(false);
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