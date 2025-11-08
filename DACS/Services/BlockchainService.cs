using DACS.Models.Blockchain;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Nethereum.Contracts;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Nethereum.ABI.FunctionEncoding.Attributes; // <<< THÊM USING NÀY
using Nethereum.Hex.HexTypes; // <<< THÊM USING NÀY

namespace DACS.Services
{
    public class BlockchainService
    {
        private readonly Web3 _web3;
        private readonly Contract _contract;
        private readonly Nethereum.Web3.Accounts.Account _account;
        private readonly string _contractAddress;
        private readonly ILogger<BlockchainService> _logger;

        // (ABI của bạn giữ nguyên)
        private readonly string _abi = @"[
	{
		""anonymous"": false,
		""inputs"": [
			{
				""indexed"": true,
				""internalType"": ""string"",
				""name"": ""lotId"",
				""type"": ""string""
			},
			{
				""indexed"": false,
				""internalType"": ""uint256"",
				""name"": ""timestamp"",
				""type"": ""uint256""
			},
			{
				""indexed"": false,
				""internalType"": ""string"",
				""name"": ""status"",
				""type"": ""string""
			},
			{
				""indexed"": false,
				""internalType"": ""string"",
				""name"": ""location"",
				""type"": ""string""
			}
		],
		""name"": ""HistoryAdded"",
		""type"": ""event""
	},
	{
		""inputs"": [
			{
				""internalType"": ""string"",
				""name"": ""lotId"",
				""type"": ""string""
			},
			{
				""internalType"": ""string"",
				""name"": ""status"",
				""type"": ""string""
			},
			{
				""internalType"": ""string"",
				""name"": ""location"",
				""type"": ""string""
			},
			{
				""internalType"": ""string"",
				""name"": ""metadata"",
				""type"": ""string""
			}
		],
		""name"": ""addHistory"",
		""outputs"": [],
		""stateMutability"": ""nonpayable"",
		""type"": ""function""
	},
	{
		""inputs"": [
			{
				""internalType"": ""string"",
				""name"": ""lotId"",
				""type"": ""string""
			},
			{
				""internalType"": ""uint256"",
				""name"": ""index"",
				""type"": ""uint256""
			}
		],
		""name"": ""getHistoryByIndex"",
		""outputs"": [
			{
				""internalType"": ""uint256"",
				""name"": """",
				""type"": ""uint256""
			},
			{
				""internalType"": ""string"",
				""name"": """",
				""type"": ""string""
			},
			{
				""internalType"": ""string"",
				""name"": """",
				""type"": ""string""
			},
			{
				""internalType"": ""string"",
				""name"": """",
				""type"": ""string""
			}
		],
		""stateMutability"": ""view"",
		""type"": ""function""
	},
	{
		""inputs"": [
			{
				""internalType"": ""string"",
				""name"": ""lotId"",
				""type"": ""string""
			}
		],
		""name"": ""getHistoryCount"",
		""outputs"": [
			{
				""internalType"": ""uint256"",
				""name"": """",
				""type"": ""uint256""
			}
		],
		""stateMutability"": ""view"",
		""type"": ""function""
	}
]";

        public BlockchainService(IConfiguration configuration, ILogger<BlockchainService> logger)
        {
            _logger = logger;
            var privateKey = configuration["Blockchain:PrivateKey"];
            var rpcUrl = configuration["Blockchain:RpcUrl"];
            _contractAddress = configuration["Blockchain:ContractAddress"];

            _account = new Nethereum.Web3.Accounts.Account(privateKey);
            _web3 = new Web3(_account, rpcUrl);
            _contract = _web3.Eth.GetContract(_abi, _contractAddress);

            // <<< ================= THÊM 3 DÒNG NÀY ================= >>>
            _logger.LogWarning("--- CẤU HÌNH BLOCKCHAIN SERVICE ĐANG CHẠY ---");
            _logger.LogWarning("RPC URL (Ganache): {RpcUrl}", rpcUrl);
            _logger.LogWarning("TÀI KHOẢN (Người gửi): {AccountAddress}", _account.Address);
            _logger.LogWarning("HỢP ĐỒNG (Người nhận): {ContractAddress}", _contractAddress);
            // <<< ================= KẾT THÚC THÊM ================= >>>
        }

        public async Task<string> GhiNhatKyAsync(string lotId, string status, string location, string metadata)
        {
            try
            {
                var addFunction = _contract.GetFunction("addHistory");

                // 🧮 Ước lượng gas
                var gas = await addFunction.EstimateGasAsync(_account.Address, null, null, lotId, status, location, metadata);
                // ⚙️ Cộng thêm 15% gas buffer cho an toàn
                gas = new HexBigInteger(gas.Value + (gas.Value / 6));

                // 🚀 Gửi giao dịch
                var txHash = await addFunction.SendTransactionAsync(_account.Address, gas, null, lotId, status, location, metadata);
                _logger.LogInformation($"✅ Giao dịch đã gửi (Sent). TxHash = {txHash}");

                // 🕓 CHỜ XÁC NHẬN
                var receipt = await _web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(txHash);
                int retry = 0;
                while (receipt == null && retry < 10)
                {
                    await Task.Delay(2000);
                    receipt = await _web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(txHash);
                    retry++;
                }

                // <<< ================= THÊM KIỂM TRA STATUS ================= >>>
                if (receipt == null)
                {
                    _logger.LogError("❌ GIAO DỊCH THẤT BẠI: Không nhận được Receipt (Timeout). TxHash: {TxHash}", txHash);
                    throw new Exception($"Transaction receipt timeout for {txHash}.");
                }
                else if (receipt.Status.Value == 0) // <<< KIỂM TRA REVERT
                {
                    _logger.LogError("❌ GIAO DỊCH THẤT BẠI (Reverted). Block: {BlockNumber}. TxHash: {TxHash}", receipt.BlockNumber.Value, txHash);
                    throw new Exception($"Transaction Reverted: {txHash}. Lỗi Gas Limit hoặc Logic Hợp đồng.");
                }
                else
                {
                    // (receipt.Status.Value == 1) -> THÀNH CÔNG
                    _logger.LogInformation($"📬 Giao dịch đã xác nhận THÀNH CÔNG trong block {receipt.BlockNumber.Value}");
                }
                // <<< ================= KẾT THÚC SỬA ================= >>>

                return txHash;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi ghi Blockchain.");
                throw;
            }
        }

        // <<< ================= SỬA LẠI HÀM NÀY ================= >>>
        public async Task<List<TraceEventDTO>> GetHistoryAsync(string maLo)
        {
            var result = new List<TraceEventDTO>();
            try
            {
                var getCountFunction = _contract.GetFunction("getHistoryCount");
                var count = await getCountFunction.CallAsync<BigInteger>(maLo);
                _logger.LogInformation("📦 Lô {MaLo} có {Count} bản ghi.", maLo, count);

                var getByIndexFunction = _contract.GetFunction("getHistoryByIndex");

                for (int i = 0; i < (int)count; i++)
                {
                    // 1. Gọi hàm và hứng bằng DTO chuyên dụng cho Output
                    var eventRaw = await getByIndexFunction.CallDeserializingToObjectAsync<GetHistoryByIndexOutputDTO>(maLo, i);

                    // 2. Chuyển đổi thủ công sang DTO chính của bạn
                    if (eventRaw != null)
                    {
                        result.Add(new TraceEventDTO
                        {
                            Timestamp = eventRaw.Timestamp,
                            Status = eventRaw.Status,
                            Location = eventRaw.Location,
                            Metadata = eventRaw.Metadata
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi đọc Blockchain.");
            }
            return result;
        }
        // <<< ================= KẾT THÚC SỬA ================= >>>


        // (Hàm TestBlockchainAsync giữ nguyên)
        public async Task TestBlockchainAsync()
        {
            try
            {
                _logger.LogInformation("===== BẮT ĐẦU KIỂM TRA GHI & ĐỌC BLOCKCHAIN =====");
                string lotId = "TEST_AUTO_" + DateTime.Now.ToString("yyyyMMddHHmmss");

                await GhiNhatKyAsync(lotId, "TEST_GHI", "KHO_A", "Metadata thử nghiệm");

                var events = await GetHistoryAsync(lotId);
                if (events.Count > 0)
                {
                    _logger.LogInformation("✅ Đọc lại thành công, tổng {Count} bản ghi.", events.Count);
                    foreach (var e in events)
                        _logger.LogInformation($"🕓 {e.Timestamp} | {e.Status} @ {e.Location} | {e.Metadata}");
                }
                else
                {
                    _logger.LogWarning("⚠️ Không tìm thấy bản ghi nào trong blockchain cho {LotId}", lotId);
                }

                _logger.LogInformation("===== KIỂM TRA HOÀN TẤT =====");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi trong quá trình test tự động Blockchain.");
            }
        }
    }

    // <<< ================= THÊM CLASS NÀY VÀO ================= >>>
    // Class này chỉ dùng để "hứng" 4 giá trị trả về không tên của hàm getHistoryByIndex
    [FunctionOutput]
    public class GetHistoryByIndexOutputDTO : IFunctionOutputDTO
    {
        [Parameter("uint256", "", 1)]
        public BigInteger Timestamp { get; set; }

        [Parameter("string", "", 2)]
        public string Status { get; set; }

        [Parameter("string", "", 3)]
        public string Location { get; set; }

        [Parameter("string", "", 4)]
        public string Metadata { get; set; }
    }
    // <<< ================= KẾT THÚC THÊM ================= >>>
}