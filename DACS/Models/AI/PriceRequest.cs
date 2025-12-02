using System.ComponentModel.DataAnnotations;

namespace DACS.Models.AI
{
    public class PriceRequest
    {
        public string Province { get; set; }
        public string District { get; set; }
        public string Ward { get; set; }
        public string Street { get; set; }
        public string TenLoaiKho { get; set; }
        public double BaseValue { get; set; }
        public bool IsWet { get; set; }
        public bool IsBulky { get; set; }
    }
}
