using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Сalculating_Pi_ClientServer
{
    public class Connections
    {
        static Socket m_socket;
        static string m_serverIp = "127.0.0.1";
        static int m_serverPort = 8080;



        public static async Task ConnectToServer()
        {
            if (m_socket != null) return;

            m_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            await m_socket.ConnectAsync(m_serverIp, m_serverPort);
        }
        public static void DisconnectFromServer()
        {
            if (m_socket.Connected) return;
            if (!m_socket.Connected) return;
            
            m_socket.Shutdown(SocketShutdown.Both);
            m_socket.Close();
            m_socket.Dispose();
        }

        public static async Task ReceivePiAsync()
        {
            byte[] buffer = new byte[256];
            int bytesRead;
            try
            {
                while ((bytesRead = await m_socket.ReceiveAsync(new ArraySegment<byte>(buffer), SocketFlags.None)) > 0)
                {
                    string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    double pi;

                    if (!Double.TryParse(response, out pi)) continue;

                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        MainWindow.viewModel.Pi = pi;
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при получении сообщений от сервера: {ex.Message}", "Client error");
            }
            finally
            {
                m_socket.Disconnect(true);
            }
        }

        public static async Task SendPointListAsync(ObservableCollection<Point> points)
        {
            if (!m_socket.Connected) { MessageBox.Show("Прежде чем отправить точки на сервер, подключитесь к серверу"); return; }
            if(points == null) return;

            string jsonPoints = JsonConvert.SerializeObject(points);
            byte[] messege = Encoding.UTF8.GetBytes(jsonPoints);

            await m_socket.SendAsync(new ArraySegment<byte>(messege), SocketFlags.None);
        }
    }
}
