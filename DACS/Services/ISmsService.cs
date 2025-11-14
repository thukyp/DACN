using System.Threading.Tasks;

namespace DACS.Services
{
    public interface ISmsService
    {
        // toNumber: Số điện thoại (ví dụ: 0912345678)
        // message: Nội dung tin nhắn
        Task SendSmsAsync(string toNumber, string message);
    }
}