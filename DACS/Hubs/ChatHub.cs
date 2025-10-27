// File: /Hubs/ChatHub.cs
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

public class ChatHub : Hub
{
    // HÀM 1: Khách hàng (User) gọi hàm này để gửi tin cho Admin
    public async Task SendMessageToAdmin(string user, string message)
    {
        // Lấy ID kết nối của người gửi
        var connectionId = Context.ConnectionId;

        // Gửi tin nhắn này TỚI NHÓM "Admins"
        // Admin sẽ lắng nghe sự kiện tên là "ReceiveUserMessage"
        await Clients.Group("Admins").SendAsync("ReceiveUserMessage", connectionId, user, message);
    }

    // HÀM 2: Admin (QuanLyND) gọi hàm này để trả lời 1 khách hàng cụ thể
    public async Task SendReplyToUser(string connectionId, string message)
    {
        // Gửi tin nhắn trả lời TỚI một ConnectionId cụ thể (người khách đó)
        // Khách hàng sẽ lắng nghe sự kiện "ReceiveAdminReply"
        await Clients.Client(connectionId).SendAsync("ReceiveAdminReply", message);
    }

    // HÀM 3: Admin (QuanLyND) gọi hàm này khi họ vừa kết nối
    public async Task JoinAdminGroup()
    {
        // Thêm admin này vào một nhóm riêng để họ nhận được tất cả tin nhắn
        // từ tất cả khách hàng
        await Groups.AddToGroupAsync(Context.ConnectionId, "Admins");
    }
}