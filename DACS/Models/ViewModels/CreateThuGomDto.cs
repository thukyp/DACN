namespace DACS.Models.ViewModels
{
    public class CreateThuGomDto
    {
        public string M_LoaiSP { get; set; }
        public string M_SanPham { get; set; }
        public string ByproductUnit { get; set; }
        public double ByproductQuantity { get; set; }
        public decimal? ByproductValue { get; set; }
        public string? ByproductDescription { get; set; }

        // Thông tin địa chỉ
        public string SupplierProvince { get; set; }
        public string SupplierDistrict { get; set; }
        public string SupplierWard { get; set; }
        public string SupplierStreet { get; set; }
        public string SupplierPhone { get; set; }

        public DateTime PickupReadyTime { get; set; }
        public string? SupplierNotes { get; set; }

        // Đặc tính
        public bool CharBulky { get; set; }
        public bool CharWet { get; set; }
        public bool CharDry { get; set; }
        public bool CharMoisture { get; set; }
        public bool CharImpure { get; set; }
        public bool CharProcessed { get; set; }
        public bool CharAmUot { get; internal set; }
    }
}
