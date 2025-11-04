using Microsoft.Extensions.Logging;
using Nethereum.Contracts;
using Nethereum.Hex.HexTypes;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using System;
using System.Globalization;
using System.Numerics; // Cần cho BigInteger
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DACS.Services
{
    public class BlockchainService
    {
        private readonly Web3 _web3;
        private readonly string _contractAddress = "0xE8c0abB166387D7505B490528e33d4d0A08d013e"; // <-- DÁN VÀO ĐÂY
        private readonly string _abi = @"[
	{
		""inputs"": [
			{
				""internalType"": ""string"",
				""name"": ""_sqlRecordId"",
				""type"": ""string""
			},
			{
				""internalType"": ""string"",
				""name"": ""_recordType"",
				""type"": ""string""
			},
			{
				""internalType"": ""string"",
				""name"": ""_dataHash"",
				""type"": ""string""
			}
		],
		""name"": ""createTraceRecord"",
		""outputs"": [],
		""stateMutability"": ""nonpayable"",
		""type"": ""function""
	},
	{
		""anonymous"": false,
		""inputs"": [
			{
				""indexed"": true,
				""internalType"": ""string"",
				""name"": ""sqlRecordId"",
				""type"": ""string""
			},
			{
				""indexed"": false,
				""internalType"": ""string"",
				""name"": ""recordType"",
				""type"": ""string""
			},
			{
				""indexed"": false,
				""internalType"": ""string"",
				""name"": ""dataHash"",
				""type"": ""string""
			},
			{
				""indexed"": false,
				""internalType"": ""uint256"",
				""name"": ""timestamp"",
				""type"": ""uint256""
			}
		],
		""name"": ""RecordCreated"",
		""type"": ""event""
	},
	{
		""inputs"": [
			{
				""internalType"": ""string"",
				""name"": ""_sqlRecordId"",
				""type"": ""string""
			},
			{
				""internalType"": ""string"",
				""name"": ""_recordType"",
				""type"": ""string""
			}
		],
		""name"": ""getTraceRecord"",
		""outputs"": [
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
				""internalType"": ""uint256"",
				""name"": """",
				""type"": ""uint256""
			}
		],
		""stateMutability"": ""view"",
		""type"": ""function""
	},
	{
		""inputs"": [
			{
				""internalType"": ""string"",
				""name"": """",
				""type"": ""string""
			}
		],
		""name"": ""traceRecords"",
		""outputs"": [
			{
				""internalType"": ""string"",
				""name"": ""sqlRecordId"",
				""type"": ""string""
			},
			{
				""internalType"": ""string"",
				""name"": ""recordType"",
				""type"": ""string""
			},
			{
				""internalType"": ""string"",
				""name"": ""dataHash"",
				""type"": ""string""
			},
			{
				""internalType"": ""uint256"",
				""name"": ""timestamp"",
				""type"": ""uint256""
			}
		],
		""stateMutability"": ""view"",
		""type"": ""function""
	}
]"; // <-- DÁN VÀO ĐÂY
        private readonly string _privateKey = "0x798001e4defbfd8cf67098fb48c93cab42789e7b802e84782d54669260254bf3"; // <-- DÁN VÀO ĐÂY
        private readonly string _ganacheUrl = "http://127.0.0.1:7545";
        private readonly ILogger<BlockchainService> _logger;

        public BlockchainService(ILogger<BlockchainService> logger)
        {
            _logger = logger;
            try
            {
                var account = new Account(_privateKey);
                _web3 = new Web3(account, _ganacheUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Không thể khởi tạo Web3. Key hoặc URL sai?");
                _web3 = null;
            }
        }

        public async Task<string> WriteTraceRecord(string sqlRecordId, string recordType, string dataHash)
        {
            if (_web3 == null) { /* ... (xử lý lỗi) ... */ return null; }
            Contract contract;
            Function createRecordFunction;
            try
            {
                contract = _web3.Eth.GetContract(_abi, _contractAddress);
                createRecordFunction = contract.GetFunction("createTraceRecord");
            }
            catch (Exception ex) { /* ... (xử lý lỗi) ... */ return null; }

            try
            {
                _logger.LogInformation("Đang ghi... ID: {SqlRecordId}, Type: {RecordType}", sqlRecordId, recordType);
                var transactionReceipt = await createRecordFunction.SendTransactionAndWaitForReceiptAsync(
                    from: _web3.TransactionManager.Account.Address,
                    gas: new HexBigInteger(3000000),
                    value: new HexBigInteger(0),
                    functionInput: new object[] { sqlRecordId, recordType, dataHash }
                );
                _logger.LogInformation("Ghi thành công. TxHash: {TransactionHash}", transactionReceipt.TransactionHash);
                return transactionReceipt.TransactionHash;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi ghi blockchain cho ID {SqlRecordId}", sqlRecordId);
                return null;
            }
        }

        public string CreateSha256Hash(string input)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        public async Task<List<HistoryStepDTO>> GetLotHistoryAsync(string sqlRecordId)
        {
            var historySteps = new List<HistoryStepDTO>();
            if (_web3 == null) return historySteps;

            var eventTypesToQuery = new[]
            {
                "ThuGom_Scheduled", "ThuGom_Started", "ThuGom_Completed", "ThuGom_Failed"
            };

            Contract contract;
            Function getRecordFunction;
            try
            {
                contract = _web3.Eth.GetContract(_abi, _contractAddress);
                getRecordFunction = contract.GetFunction("getTraceRecord");
            }
            catch (Exception ex) { /* ... (xử lý lỗi) ... */ return historySteps; }

            _logger.LogInformation("Bắt đầu truy vết lịch sử cho ID: {SqlRecordId}", sqlRecordId);

            foreach (var eventType in eventTypesToQuery)
            {
                var step = new HistoryStepDTO { /* ... */ };
                try
                {
                    var result = await getRecordFunction.CallAsync<Tuple<string, string, BigInteger>>(
                        sqlRecordId, eventType
                    );

                    if (result != null && result.Item3 > 0)
                    {
                        step.IsFound = true;
                        step.DataHash = result.Item2;
                        step.Timestamp = ConvertUnixTimestamp(result.Item3);
                        step.EventType = eventType;
                        historySteps.Add(step);
                        _logger.LogInformation("... Tìm thấy sự kiện: {EventType}", eventType);
                    }
                }
                catch (Exception) { /* Lỗi "Record not found" là bình thường */ }
            }
            return historySteps.OrderBy(s => s.Timestamp).ToList();
        }

        private DateTime ConvertUnixTimestamp(BigInteger unixTimestamp)
        {
            if (!double.TryParse(unixTimestamp.ToString(), out double timestampDouble)) return DateTime.MinValue;
            DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds((long)timestampDouble);
            return dateTimeOffset.LocalDateTime;
        }

    }

    public class HistoryStepDTO
    {
        public string EventType { get; set; }
        public string DataHash { get; set; }
        public DateTime Timestamp { get; set; }
        public bool IsFound { get; set; }
        public string SqlRecordId { get; set; }
    }
}