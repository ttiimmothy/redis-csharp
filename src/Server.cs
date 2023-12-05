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
    var bytes = socket.Receive(buffer);
    var data = Encoding.UTF8.GetString(buffer, 0, bytes);
    Console.WriteLine("Received: {0}", data);
    var response = Encoding.UTF8.GetBytes("+PONG\r\n");
    socket.Send(response);
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
