using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

var server = new TcpListener(IPAddress.Any, 6379);
try
{
  server.Start();
  while (true)
  {
    Console.WriteLine("Waiting for a connection...");
    using var socket = server.AcceptSocket();
    Console.WriteLine("Connected!");
    var buffer = new byte[1024];
    while (true)
    {
      try
      {
        var bytes = socket.Receive(buffer);
        var data = Encoding.ASCII.GetString(buffer, 0, bytes);
        var lines = data.Split("\n")
                        .Select(x => x.Trim())
                        .Where(x => !string.IsNullOrEmpty(x));
        foreach (var line in lines)
        {
          if (line.ToLower() == "ping")
          {
            var response = Encoding.UTF8.GetBytes("+PONG\r\n");
            socket.Send(response);
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
catch (SocketException e)
{
  Console.WriteLine(e.Message);
}
finally
{
  server.Stop();
}
