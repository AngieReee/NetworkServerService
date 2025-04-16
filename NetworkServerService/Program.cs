using System.Diagnostics;
using System.Net.Sockets;
using System.Net;
using System.Text;
using NetworkServerService.Models;

namespace NetworkServerService
{
    internal class Program
    {
        private const int Port = 5000;
        private const int BufferSize = 1024 + 2;

        public static async Task Main(string[] args)
        {
            TcpListener listener = new TcpListener(IPAddress.Any, Port);
            listener.Start();
            Console.WriteLine($"Сервер запущен на порту {Port}...");

            while (true)
            {
                try
                {
                    using (TcpClient client = await listener.AcceptTcpClientAsync())
                    {
                        Console.WriteLine("Подключен клиент");
                        await HandleClientAsync(client);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка: {ex.Message}");
                }
            }
        }

        private static async Task HandleClientAsync(TcpClient client)
        {
            using (NetworkStream stream = client.GetStream())
            {
                byte[] buffer = new byte[BufferSize];
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);

                if (bytesRead < 2)
                {
                    Console.WriteLine("Ошибка: Слишком короткое сообщение");
                    return;
                }

                byte[] receivedData = new byte[bytesRead - 2];
                Array.Copy(buffer, receivedData, bytesRead - 2);

                ushort receivedCrc = BitConverter.ToUInt16(buffer, bytesRead - 2);
                ushort calculatedCrc = Crc16.Calculate(receivedData);

                if (calculatedCrc == receivedCrc)
                {
                    string message = Encoding.UTF8.GetString(receivedData);
                    Console.WriteLine($"Получено сообщение: {message}");

                    string response = "Сообщение получено успешно";
                    byte[] responseData = Encoding.UTF8.GetBytes(response);
                    ushort responseCrc = Crc16.Calculate(responseData);

                    byte[] fullResponse = new byte[responseData.Length + 2];
                    Buffer.BlockCopy(responseData, 0, fullResponse, 0, responseData.Length);
                    Buffer.BlockCopy(Crc16.ToBytes(responseCrc), 0, fullResponse, responseData.Length, 2);

                    await stream.WriteAsync(fullResponse, 0, fullResponse.Length);
                }
                else
                {
                    Console.WriteLine("Ошибка CRC! Поврежденные данные!");

                    string error = "Ошибка CRC";
                    byte[] errorData = Encoding.UTF8.GetBytes(error);
                    await stream.WriteAsync(errorData, 0, errorData.Length);
                }
            }
        }
    }
}
