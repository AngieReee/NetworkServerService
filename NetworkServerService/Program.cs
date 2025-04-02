using System.Diagnostics;
using System.Net.Sockets;
using System.Net;
using System.Text;

namespace NetworkServerService
{
    internal class Program
    {
        static async Task Main()
        {
            var server = new TcpListener(IPAddress.Any, 8080);
            server.Start();

            Debug.WriteLine("Сервер запущен. Ожидание подключений...");

            while (true)
            {
                var client = await server.AcceptTcpClientAsync();
                _ = HandleClientAsync(client);
            }
        }

        private static async Task HandleClientAsync(TcpClient client)
        {
            using (client)
            using (var stream = client.GetStream())
            {
                Debug.WriteLine("Новое подключение");

                while (client.Connected)
                {
                    var buffer = new byte[1024];
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);

                    if (bytesRead > 0)
                    {
                        var message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        Debug.WriteLine($"Клиент: {message}");

                        var response = $"Сервер: {message}";
                        var responseData = Encoding.UTF8.GetBytes(response);
                        await stream.WriteAsync(responseData, 0, responseData.Length);
                    }
                }
            }
        }
    }
}
