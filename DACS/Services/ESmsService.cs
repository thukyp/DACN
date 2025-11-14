using DACS.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Web; // Cần dùng để mã hóa URL
using Newtonsoft.Json;
using System.Text.Json.Serialization;
namespace DACS.Services
{
    public class ESmsService : ISmsService
    {
        private readonly ESmsSettings _settings;
        private readonly ILogger<ESmsService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public ESmsService(IOptions<ESmsSettings> settings,
                         ILogger<ESmsService> logger,
                         IHttpClientFactory httpClientFactory)
        {
            _settings = settings.Value;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        public class EsmsResponse
        {
            [JsonPropertyName("CodeResult")] // Dùng cái này nếu bạn dùng System.Text.Json
                                             // [JsonProperty("CodeResult")] // Dùng cái này nếu bạn dùng Newtonsoft.Json
            public string CodeResult { get; set; }

            [JsonPropertyName("ErrorMessage")]
            // [JsonProperty("ErrorMessage")]
            public string ErrorMessage { get; set; }
        }

        public async Task SendSmsAsync(string toNumber, string message)
        {
            if (string.IsNullOrEmpty(toNumber))
            {
                _logger.LogWarning("Không thể gửi SMS: Số điện thoại (toNumber) bị rỗng.");
                return;
            }

            var client = _httpClientFactory.CreateClient();

            // Mã hóa nội dung tin nhắn để đảm bảo không lỗi URL
            var encodedMessage = HttpUtility.UrlEncode(message);

            // Xây dựng URL theo tài liệu của eSMS (ví dụ)
            // SmsType=2 là loại tin nhắn CSKH (chăm sóc khách hàng)
            var url = $"{_settings.ApiUrl}?Phone={toNumber}&Content={encodedMessage}" +
                      $"&ApiKey={_settings.ApiKey}&SecretKey={_settings.SecretKey}" +
                      $"&SmsType=2&Brandname={_settings.Brandname}";

            try
            {
                var response = await client.GetAsync(url);
                var responseString = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    // GIẢI MÃ JSON
                    var esmsResponse = System.Text.Json.JsonSerializer.Deserialize<EsmsResponse>(responseString);
                    // hoặc: var esmsResponse = JsonConvert.DeserializeObject<EsmsResponse>(responseString);

                    // KIỂM TRA LÕI LOGIC
                    if (esmsResponse.CodeResult == "100")
                    {
                        _logger.LogInformation($"Gửi SMS tới {toNumber} THÀNH CÔNG (Code 100).");
                    }
                    else
                    {
                        // Đây là lỗi của bạn!
                        _logger.LogWarning($"Gửi SMS tới {toNumber} THẤT BẠI (Code {esmsResponse.CodeResult}): {esmsResponse.ErrorMessage}");
                    }
                }
                else
                {
                    _logger.LogWarning($"Gửi SMS tới {toNumber} thất bại (HTTP {response.StatusCode}). Phản hồi: {responseString}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi nghiêm trọng khi gọi API gửi SMS tới {toNumber}");
            }
        }
    }
}