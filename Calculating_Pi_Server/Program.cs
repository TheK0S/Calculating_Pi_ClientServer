using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Calculating_Pi_Server
{
    internal class Program
    {
        static int Id = 0;
        static ConcurrentDictionary<int, Socket> clients = new ConcurrentDictionary<int, Socket>();

        static int numPoints = 0;
        static int insideCircle = 0;
        static object locker = new object();

        static IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
        static int port = 8080;

        static void Main(string[] args)
        {
            Task task = new Task(()=> { AcceptAsyncServer(); });
            task.Start();
            Thread.CurrentThread.Join();

            Console.WriteLine("Сервер остановлен");
            Console.ReadKey();
        }

        private static async Task AcceptAsyncServer()
        {
            Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            listener.Bind(new IPEndPoint(ipAddress, port));
            listener.Listen(10);
            Console.WriteLine($"Сервер ждет подключений на порту {port}");

            while (true)
            {
                Socket client = await listener.AcceptAsync();

                int clientId = Id++;

                Console.WriteLine($"Клиент с Id = {clientId} подключился к серверу");

                clients.TryAdd(clientId, client);

                _ = HandleClientMessagesAsync(clientId);
    }
}

        private static async Task HandleClientMessagesAsync(int clientId)
        {
            Socket client = clients[clientId];
            NetworkStream stream = new NetworkStream(client);

            byte[] buffer = new byte[1024];
            int bytesRead;

            try
            {
                while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Console.WriteLine($"\nСообщение от клиента {clientId}: {message}");

                    ObservableCollection<Point> points = JsonConvert.DeserializeObject<ObservableCollection<Point>>(message);

                    if (points == null) continue;

                    lock(locker)
                    {
                        numPoints += points.Count;
                    }

                    foreach (Point p in points)
                    {
                        if (p.X * p.X + p.Y * p.Y < 1)
                        {
                            lock(locker)
                            {
                                insideCircle++;
                            }
                        }
                    }
                                        
                    double pi = ((double)insideCircle / numPoints) * 4;
                    await Console.Out.WriteLineAsync($"\nТекущее значение Пи = {pi}");

                    BroadcastMessage(pi);
                }
            }
            catch (Exception ex)
            {
                await Console.Out.WriteLineAsync(ex.Message);
                if(clients.TryRemove(clientId, out client))
                    await Console.Out.WriteLineAsync($"Клиент с Id = {clientId} отключен");
            }
        }

        private static void BroadcastMessage(double pi)
        {
            byte[] messageBytes = Encoding.UTF8.GetBytes(pi.ToString());

            foreach (var client  in clients.Values)
            {
                if(client.Connected)
                {
                    client.Send(messageBytes);
                }
            }
        }
    }
}
