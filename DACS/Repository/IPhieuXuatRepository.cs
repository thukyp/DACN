// File: Repositories/IPhieuXuatRepository.cs
using DACS.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DACS.Repositories
{
    public interface IPhieuXuatRepository
    {
        /// <summary>
        /// Thêm mới Phiếu xuất và các chi tiết, đồng thời cập nhật Tồn kho.
        /// Thực hiện trong một transaction.
        /// </summary>
        /// <param name="phieuXuat">Đối tượng PhieuXuat (phải bao gồm ChiTietPhieuXuats).</param>
        /// <exception cref="InvalidOperationException">Ném ra nếu không đủ tồn kho.</exception>
        /// <returns>Task.</returns>
        Task AddPhieuXuatAsync(PhieuXuat phieuXuat);

        /// <summary>
        /// Lấy Phiếu xuất theo Mã.
        /// </summary>
        /// <param name="maPhieuXuat">Mã phiếu xuất.</param>
        /// <param name="includeDetails">True để kèm theo chi tiết phiếu xuất và thông tin sản phẩm/đơn vị.</param>
        /// <returns>Đối tượng PhieuXuat hoặc null nếu không tìm thấy.</returns>
        Task<PhieuXuat?> GetPhieuXuatByIdAsync(int maPhieuXuat, bool includeDetails = true);

        /// <summary>
        /// Lấy danh sách Phiếu xuất phân trang và lọc theo ngày.
        /// </summary>
        /// <param name="tuNgay">Ngày bắt đầu lọc.</param>
        /// <param name="denNgay">Ngày kết thúc lọc.</param>
        /// <param name="pageIndex">Số trang hiện tại (bắt đầu từ 1).</param>
        /// <param name="pageSize">Số lượng mục trên mỗi trang.</param>
        /// <param name="trackChanges">True để theo dõi thay đổi (dùng khi cần cập nhật).</param>
        /// <returns>Tuple chứa danh sách phiếu xuất và tổng số lượng.</returns>
        Task<(IEnumerable<PhieuXuat> Items, int TotalCount)> GetPagedPhieuXuatAsync(
            DateTime? tuNgay,
            DateTime? denNgay,
            int pageIndex,
            int pageSize,
            bool trackChanges = false);

        // Có thể thêm các phương thức khác:
        // Task UpdatePhieuXuatAsync(PhieuXuat phieuXuat); // Cần cẩn thận với việc cập nhật tồn kho lại
        // Task DeletePhieuXuatAsync(int maPhieuXuat); // Cần cẩn thận với việc hoàn lại tồn kho
    }
}