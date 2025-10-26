using DACS.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace DACS.Services
{
    public class SocketServer
    {
        private readonly IServiceProvider _serviceProvider;

        public SocketServer(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task StartAsync()
        {
            TcpListener listener = new TcpListener(IPAddress.Any, 8888);
            listener.Start();
            Console.WriteLine("🟢 Server đang lắng nghe tại cổng 8888...");

            while (true)
            {
                var client = await listener.AcceptTcpClientAsync();
                _ = HandleClientAsync(client);
            }
        }

        private async Task HandleClientAsync(TcpClient client)
        {
            Console.WriteLine("🔗 Client đã kết nối.");

            using var stream = client.GetStream();
            byte[] buffer = new byte[4096];

            int byteCount;
            try
            {
                byteCount = await stream.ReadAsync(buffer, 0, buffer.Length);
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Lỗi đọc từ client: " + ex.Message);
                client.Close();
                return;
            }

            string message = Encoding.UTF8.GetString(buffer, 0, byteCount);
            Console.WriteLine($"📩 Nhận từ client: {message}");

            string response = "❌ Không có phản hồi từ chatbot.";

            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var homeController = scope.ServiceProvider.GetRequiredService<HomeController>();

                    // Tránh lỗi user null trong controller
                    var result = await SafeAsk(homeController, message);
                    response = result ?? "❌ Chatbot không trả lời.";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("⚠️ Lỗi xử lý server: " + ex.Message);
                response = "⚠️ Lỗi xử lý server.";
            }

            // gửi phản hồi
            try
            {
                byte[] responseBytes = Encoding.UTF8.GetBytes(response);
                await stream.WriteAsync(responseBytes, 0, responseBytes.Length);
                await stream.FlushAsync();
                Console.WriteLine($"📤 Đã gửi phản hồi: {response}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Lỗi khi gửi phản hồi: {ex.Message}");
            }

            client.Close();
        }

        // Hàm wrapper an toàn, tránh lỗi user null
        private async Task<string> SafeAsk(HomeController homeController, string message)
        {
            try
            {
                var result = await homeController.Ask(message) as JsonResult;
                if (result?.Value is null) return null;

                dynamic val = result.Value;
                return val.response ?? null;
            }
            catch (Exception ex)
            {
                Console.WriteLine("⚠️ Lỗi SafeAsk: " + ex.Message);
                return null;
            }
        }
    }
}
