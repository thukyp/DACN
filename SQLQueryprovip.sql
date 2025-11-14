USE QuanLyPhuPham
GO

-- =================================================================
-- BƯỚC 1: SỬA CẤU TRÚC BẢNG ĐỂ KHỚP VỚI LOGIC MỚI
-- (Xóa cột SoLuong cũ ra khỏi bảng SanPhams)
-- =================================================================
IF EXISTS(SELECT 1 FROM sys.columns 
          WHERE Name = N'SoLuong'
          AND Object_ID = Object_ID(N'dbo.SanPhams'))
BEGIN
    PRINT 'Phat hien cot SoLuong. Dang tien hanh xoa...'
    ALTER TABLE SanPhams DROP COLUMN SoLuong;
    PRINT 'Da xoa cot SoLuong khoi SanPhams thanh cong.'
END
GO


-- =================================================================
-- BƯỚC 2: CHẠY TOÀN BỘ SCRIPT INSERT DỮ LIỆU TRONG MỘT TRANSACTION
-- (Đã xóa tất cả từ khóa 'GO' và thêm lệnh DELETE)
-- =================================================================
BEGIN TRANSACTION;
BEGIN TRY
    PRINT 'Bat dau Transaction... Dang xoa du lieu cu...'
    -- Xóa dữ liệu cũ theo thứ tự (Con trước, Cha sau)
    -- (Bỏ qua lỗi nếu bảng không tồn tại hoặc không có dữ liệu)
    IF OBJECT_ID('dbo.ChiTietThuGoms', 'U') IS NOT NULL DELETE FROM ChiTietThuGoms WHERE MaLoTonKho IS NOT NULL;
    IF OBJECT_ID('dbo.LoTonKhos', 'U') IS NOT NULL DELETE FROM LoTonKhos;
    IF OBJECT_ID('dbo.ChiTietPhieuXuats', 'U') IS NOT NULL DELETE FROM ChiTietPhieuXuats;
    IF OBJECT_ID('dbo.ChiTietDatHangs', 'U') IS NOT NULL DELETE FROM ChiTietDatHangs;
    IF OBJECT_ID('dbo.DonHangs', 'U') IS NOT NULL DELETE FROM DonHangs;
    IF OBJECT_ID('dbo.SanPhams', 'U') IS NOT NULL DELETE FROM SanPhams;
    IF OBJECT_ID('dbo.LoaiSanPhams', 'U') IS NOT NULL DELETE FROM LoaiSanPhams;
    IF OBJECT_ID('dbo.DonViTinhs', 'U') IS NOT NULL DELETE FROM DonViTinhs;
    IF OBJECT_ID('dbo.KhoHangs', 'U') IS NOT NULL DELETE FROM KhoHangs;
    IF OBJECT_ID('dbo.PhuongThucThanhToans', 'U') IS NOT NULL DELETE FROM PhuongThucThanhToans;
    IF OBJECT_ID('dbo.XaPhuongs', 'U') IS NOT NULL DELETE FROM XaPhuongs;
    IF OBJECT_ID('dbo.QuanHuyens', 'U') IS NOT NULL DELETE FROM QuanHuyens;
    IF OBJECT_ID('dbo.TinhThanhPhos', 'U') IS NOT NULL DELETE FROM TinhThanhPhos;
    IF OBJECT_ID('dbo.CauHoiThuongGap', 'U') IS NOT NULL DELETE FROM CauHoiThuongGap;
    IF OBJECT_ID('dbo.ChatHistory', 'U') IS NOT NULL DELETE FROM ChatHistory;
    PRINT 'Da xoa du lieu cu thanh cong. Bat dau them du lieu moi...'

    -- 1. INSERT Đơn Vị Tính
    INSERT INTO DonViTinhs(M_DonViTinh, TenLoaiTinh)
    VALUES ('kg', N'Kilogram');

    -- 2. INSERT Loại Sản Phẩm
    INSERT INTO dbo.LoaiSanPhams(M_LoaiSP,TenLoai)
    VALUES
    ('LSP001',N'Đã qua xử lý'),
    ('LSP002',N'Phụ phẩm thô'),
    ('LSP003',N'Thức ăn chăn nuôi'),
    ('LSP004',N'Phân bón'),
    ('LSP005',N'Năng lượng sinh khối');

    -- 3. INSERT Sản Phẩm (ĐÃ XÓA CỘT 'SoLuong')
    INSERT INTO dbo.SanPhams(M_SanPham, M_LoaiSP, M_DonViTinh, TenSanPham, Gia, MoTa, TrangThai, NgayTao, AnhSanPham, HanSuDung)
    VALUES
    ('SP002', 'LSP002', 'kg', N'Vỏ cà phê khô', 70000, N'Vỏ cà phê phơi khô, dùng để đốt lò sinh học.', N'Còn hàng', GETUTCDATE(), '/images/Products/vo_ca_phe.png', '2026-01-15'),
    ('SP003', 'LSP002', 'kg', N'Rơm mì', 45000, N'Rơm cây mì sau thu hoạch, làm phân xanh hoặc chất độn chuồng.', N'Còn hàng', GETUTCDATE(), '/images/Products/rom_kho_bo.png', '2025-11-20'),
    ('SP004', 'LSP002', 'kg', N'Xơ dừa', 60000, N'Xơ dừa phơi khô, dùng làm giá thể trồng cây hoặc đệm lót.', N'Còn hàng', GETUTCDATE(), '/images/Products/Xo_dua.png', '2025-10-01'),
    ('SP001', 'LSP001', 'kg', N'Trấu nghiền', 80000, N'Trấu đã nghiền nhỏ, dùng làm nguyên liệu sản xuất viên nén.', N'Còn hàng', GETUTCDATE(), '/images/Products/vo_trau_xay.png', '2025-12-31'),
    ('SP005', 'LSP001', 'kg', N'Vỏ cà phê đã xử lý', 90000, N'Vỏ cà phê đã qua xử lý, dùng làm chất đốt công nghiệp.', N'Còn hàng', GETUTCDATE(), '/images/Products/vo_ca_phe_xu_ly.png', '2026-01-15'),
    ('SP006', 'LSP003', 'kg', N'Bã mía', 40000, N'Bã mía dùng làm thức ăn bổ sung cho bò.', N'Còn hàng', GETUTCDATE(), '/images/Products/ba_mia_khoi.png', '2025-09-30'),
    ('SP007', 'LSP003', 'kg', N'Bắp khô nghiền', 75000, N'Bắp khô nghiền dùng cho chăn nuôi gia súc.', N'Còn hàng', GETUTCDATE(), '/images/Products/cui_ngo.png', '2025-10-15'),
    ('SP008', 'LSP004', 'kg', N'Phân compost', 65000, N'Phân hữu cơ compost từ phụ phẩm nông nghiệp.', N'Còn hàng', GETUTCDATE(), '/images/Products/Phan_Compost.png', '2026-02-28'),
    ('SP009', 'LSP004', 'kg', N'Phân hữu cơ vi sinh', 85000, N'Phân vi sinh cao cấp từ vỏ cà phê và bã mía.', N'Còn hàng', GETUTCDATE(), '/images/Products/phan_vi_sinh.jpg', '2026-03-10'),
    ('SP010', 'LSP005', 'kg', N'Viên nén trấu', 95000, N'Năng lượng sinh khối từ trấu ép viên.', N'Còn hàng', GETUTCDATE(), '/images/Products/vien_nen_trau.png', '2026-05-01'),
    ('SP011', 'LSP005', 'kg', N'Gỗ', 100000, N'Năng lượng sinh khối từ gỗ .', N'Còn hàng', GETUTCDATE(), '/images/Products/Go.png', '2026-06-01');

    -- 4. INSERT Phương Thức Thanh Toán
    INSERT INTO PhuongThucThanhToans (M_PhuongThuc, TenPhuongThuc)
    VALUES 
    ('PT001', N'Thanh toán khi nhận hàng'),
    ('PT002', N'Chuyển khoản ngân hàng'),
    ('PT003', N'Ví điện tử MoMo'),
    ('PT004', N'Ví điện tử ZaloPay');

    -- 5. INSERT Địa Chỉ Hành Chính (Tỉnh, Huyện, Xã)
    INSERT INTO TinhThanhPhos (MaTinh, TenTinh) VALUES
    ('T00', N'Chưa cập nhật'),
    ('T01', N'Thành phố Hà Nội'),
    ('T02', N'Thành phố Hồ Chí Minh'),
    ('T03', N'Thành phố Đà Nẵng'),
    ('T04', N'Tỉnh Bình Dương'),
    ('T05', N'Thành phố Cần Thơ');

    INSERT INTO QuanHuyens (MaQuan, TenQuan, MaTinh) VALUES
    ('Q0100', N'Chưa cập nhật','T00'),
    ('Q0101', N'Quận Hoàn Kiếm', 'T01'),
    ('Q0102', N'Quận Ba Đình', 'T01'),
    ('Q0103', N'Quận Đống Đa', 'T01'),
    ('Q0104', N'Quận Hai Bà Trưng', 'T01'),
    ('Q0105', N'Quận Cầu Giấy', 'T01'),
    ('Q0201', N'Quận 1', 'T02'),
    ('Q0202', N'Quận 3', 'T02'),
    ('Q0203', N'Quận Bình Thạnh', 'T02'),
    ('Q0204', N'Quận Tân Bình', 'T02'),
    ('Q0205', N'Thành phố Thủ Đức', 'T02'),
    ('Q0301', N'Quận Hải Châu', 'T03'),
    ('Q0302', N'Quận Thanh Khê', 'T03'),
    ('Q0303', N'Quận Sơn Trà', 'T03'),
    ('Q0304', N'Quận Ngũ Hành Sơn', 'T03'),
    ('Q0305', N'Quận Liên Chiểu', 'T03'),
    ('Q0401', N'Thành phố Thủ Dầu Một', 'T04'),
    ('Q0402', N'Thành phố Dĩ An', 'T04'),
    ('Q0403', N'Thành phố Thuận An', 'T04'),
    ('Q0404', N'Thị xã Bến Cát', 'T04'),
    ('Q0405', N'Huyện Bắc Tân Uyên', 'T04'),
    ('Q0501', N'Quận Ninh Kiều', 'T05'),
    ('Q0502', N'Quận Bình Thuỷ', 'T05'),
    ('Q0503', N'Quận Cái Răng', 'T05'),
    ('Q0504', N'Quận Ô Môn', 'T05'),
    ('Q0505', N'Huyện Phong Điền', 'T05');

    INSERT INTO XaPhuongs (MaXa, TenXa, MaQuan) VALUES
    ('X010100', N'Chưa cập nhật', 'Q0100'),
    ('X010101', N'Phường Hàng Trống', 'Q0101'),
    ('X010102', N'Phường Lý Thái Tổ', 'Q0101'),
    ('X010103', N'Phường Trần Hưng Đạo', 'Q0101'),
    ('X010104', N'Phường Tràng Tiền', 'Q0101'),
    ('X010105', N'Phường Hàng Buồm', 'Q0101'),
    ('X010201', N'Phường Phúc Xá', 'Q0102'),
    ('X010202', N'Phường Trúc Bạch', 'Q0102'),
    ('X010203', N'Phường Vĩnh Phúc', 'Q0102'),
    ('X010204', N'Phường Cống Vị', 'Q0102'),
    ('X010205', N'Phường Liễu Giai', 'Q0102'),
    ('X010301', N'Phường Cát Linh', 'Q0103'),
    ('X010302', N'Phường Hàng Bột', 'Q0103'),
    ('X010303', N'Phường Láng Hạ', 'Q0103'),
    ('X010304', N'Phường Láng Thượng', 'Q0103'),
    ('X010305', N'Phường Khâm Thiên', 'Q0103'),
    ('X010401', N'Phường Nguyễn Du', 'Q0104'),
    ('X010402', N'Phường Lê Đại Hành', 'Q0104'),
    ('X010403', N'Phường Bùi Thị Xuân', 'Q0104'),
    ('X010404', N'Phường Phố Huế', 'Q0104'),
    ('X010405', N'Phường Đống Mác', 'Q0104'),
    ('X010501', N'Phường Nghĩa Đô', 'Q0105'),
    ('X010502', N'Phường Nghĩa Tân', 'Q0105'),
    ('X010503', N'Phường Mai Dịch', 'Q0105'),
    ('X010504', N'Phường Dịch Vọng', 'Q0105'),
    ('X010505', N'Phường Yên Hoà', 'Q0105'),
    ('X020101', N'Phường Tân Định', 'Q0201'),
    ('X020102', N'Phường Đa Kao', 'Q0201'),
    ('X020103', N'Phường Bến Nghé', 'Q0201'),
    ('X020104', N'Phường Bến Thành', 'Q0201'),
    ('X020105', N'Phường Nguyễn Thái Bình', 'Q0201'),
    ('X020201', N'Phường Võ Thị Sáu', 'Q0202'),
    ('X020202', N'Phường 1', 'Q0202'),
    ('X020203', N'Phường 2', 'Q0202'),
    ('X020204', N'Phường 4', 'Q0202'),
    ('X020205', N'Phường 5', 'Q0202'),
    ('X020301', N'Phường 1', 'Q0203'),
    ('X020302', N'Phường 2', 'Q0203'),
    ('X020303', N'Phường 3', 'Q0203'),
    ('X020304', N'Phường 5', 'Q0203'),
    ('X020305', N'Phường 6', 'Q0203'),
    ('X020401', N'Phường 1', 'Q0204'),
    ('X020402', N'Phường 2', 'Q0204'),
    ('X020403', N'Phường 3', 'Q0204'),
    ('X020404', N'Phường 4', 'Q0204'),
    ('X020405', N'Phường 5', 'Q0204'),
    ('X020501', N'Phường An Khánh', 'Q0205'),
    ('X020502', N'Phường An Lợi Đông', 'Q0205'),
    ('X020503', N'Phường An Phú', 'Q0205'),
    ('X020504', N'Phường Bình Chiểu', 'Q0205'),
    ('X020505', N'Phường Bình Thọ', 'Q0205'),
    ('X030101', N'Phường Hải Châu I', 'Q0301'),
    ('X030102', N'Phường Hải Châu II', 'Q0301'),
    ('X030103', N'Phường Thạch Thang', 'Q0301'),
    ('X030104', N'Phường Thanh Bình', 'Q0301'),
    ('X030105', N'Phường Thuận Phước', 'Q0301'),
    ('X030201', N'Phường Tam Thuận', 'Q0302'),
    ('X030202', N'Phường Thanh Khê Đông', 'Q0302'),
    ('X030203', N'Phường Thanh Khê Tây', 'Q0302'),
    ('X030204', N'Phường Xuân Hà', 'Q0302'),
    ('X030205', N'Phường An Khê', 'Q0302'),
    ('X030301', N'Phường An Hải Bắc', 'Q0303'),
    ('X030302', N'Phường An Hải Đông', 'Q0303'),
    ('X030303', N'Phường An Hải Tây', 'Q0303'),
    ('X030304', N'Phường Mân Thái', 'Q0303'),
    ('X030305', N'Phường Nại Hiên Đông', 'Q0303'),
    ('X030401', N'Phường Mỹ An', 'Q0304'),
    ('X030402', N'Phường Khuê Mỹ', 'Q0304'),
    ('X030403', N'Phường Hoà Quý', 'Q0304'),
    ('X030404', N'Phường Hoà Hải', 'Q0304'),
    ('X030405', N'Phường A', 'Q0304'),
    ('X030501', N'Phường Hòa Hiệp Bắc', 'Q0305'),
    ('X030502', N'Phường Hòa Hiệp Nam', 'Q0305'),
    ('X030503', N'Phường Hòa Khánh Bắc', 'Q0305'),
    ('X030504', N'Phường Hòa Khánh Nam', 'Q0305'),
    ('X030505', N'Phường Hòa Minh', 'Q0305'),
    ('X040101', N'Phường Phú Cường', 'Q0401'),
    ('X040102', N'Phường Hiệp Thành', 'Q0401'),
    ('X040103', N'Phường Chánh Nghĩa', 'Q0401'),
    ('X040104', N'Phường Phú Thọ', 'Q0401'),
    ('X040105', N'Phường Phú Hòa', 'Q0401'),
    ('X040201', N'Phường Dĩ An', 'Q0402'), -- Mã Xã 'X040201' bị sai trong script gốc (XD40201), đã sửa
    ('X040202', N'Phường Tân Bình', 'Q0402'),
    ('X040203', N'Phường Tân Đông Hiệp', 'Q0402'),
    ('X040204', N'Phường Bình An', 'Q0402'),
    ('X040205', N'Phường Bình Thắng', 'Q0402'),
    ('X040301', N'Phường An Thạnh', 'Q0403'),
    ('X040302', N'Phường Lái Thiêu', 'Q0403'),
    ('X040303', N'Phường Bình Chuẩn', 'Q0403'),
    ('X040304', N'Phường Thuận Giao', 'Q0403'),
    ('X040305', N'Phường An Phú', 'Q0403'),
    ('X040401', N'Phường Mỹ Phước', 'Q0404'),
    ('X040402', N'Phường Chánh Phú Hòa', 'Q0404'),
    ('X040403', N'Phường Thới Hòa', 'Q0404'),
    ('X040404', N'Xã An Điền', 'Q0404'),
    ('X040405', N'Xã An Tây', 'Q0404'),
    ('X040501', N'Thị trấn Tân Thành', 'Q0405'),
    ('X040502', N'Xã Tân Bình', 'Q0405'),
    ('X040503', N'Xã Bình Mỹ', 'Q0405'),
    ('X040504', N'Xã Tân Lập', 'Q0405'),
    ('X040505', N'Xã Đất Cuốc', 'Q0405'),
    ('X050101', N'Phường Cái Khế', 'Q0501'),
    ('X050102', N'Phường An Hòa', 'Q0501'),
    ('X050103', N'Phường Thới Bình', 'Q0501'),
    ('X050104', N'Phường An Nghiệp', 'Q0501'),
    ('X050105', N'Phường An Cư', 'Q0501'),
    ('X050201', N'Phường Bình Thuỷ', 'Q0502'),
    ('X050202', N'Phường Trà An', 'Q0502'),
    ('X050203', N'Phường Trà Nóc', 'Q0502'),
    ('X050204', N'Phường Thới An Đông', 'Q0502'),
    ('X050205', N'Phường An Thới', 'Q0502'),
    ('X050301', N'Phường Lê Bình', 'Q0503'),
    ('X050302', N'Phường Hưng Phú', 'Q0503'),
    ('X050303', N'Phường Hưng Thạnh', 'Q0503'),
    ('X050304', N'Phường Ba Láng', 'Q0503'),
    ('X050305', N'Phường Thường Thạnh', 'Q0503'),
    ('X050401', N'Phường Châu Văn Liêm', 'Q0504'),
    ('X050402', N'Phường Thới Hòa', 'Q0504'),
    ('X050403', N'Phường Thới Long', 'Q0504'),
    ('X050404', N'Phường Long Hưng', 'Q0504'),
    ('X050405', N'Phường Thới An', 'Q0504'),
    ('X050501', N'Thị trấn Phong Điền', 'Q0505'),
    ('X050502', N'Xã Nhơn Ái', 'Q0505'),
    ('X050503', N'Xã Giai Xuân', 'Q0505'),
    ('X050504', N'Xã Tân Thới', 'Q0505'),
    ('X050505', N'Xã Trường Long', 'Q0505');

    -- 6. INSERT Kho Hàng
    INSERT INTO KhoHangs(MaKho, TenKho, DiaChi, SucChuaTomTat, TrangThai, TenLoaiKho)
    VALUES 
    ('K001', N'Kho chính - Phụ phẩm khô', N'Bình Dương', N'50 tấn', 0, N'Kho phụ phẩm'),
    ('K002', N'Kho lạnh - Phân bón & Thức ăn', N'Long An', N'30 tấn', 0, N'Kho phân bón'),
    ('K003', N'Kho năng lượng sinh khối', N'Đắk Lắk', N'100 tấn', 1, N'Kho năng lượng'),
    ('K004', N'Kho nguyên liệu chế biến', N'Lâm Đồng', N'40 tấn', 0, N'Kho đã xử lý'),
    ('K005', N'Kho thức ăn chăn nuôi', N'Đồng Nai', N'35 tấn', 0, N'Kho thức ăn'),
    ('K006', N'Kho rơm và phụ phẩm thô', N'An Giang', N'60 tấn', 0, N'Kho chứa rơm');

    -- 7. INSERT VÀO BẢNG 'LoTonKhos' MỚI
    INSERT INTO LoTonKhos (MaLoTonKho, M_SanPham, MaKho, NgayNhapKho, KhoiLuongBanDau, KhoiLuongConLai, M_DonViTinh)
    VALUES 
    ('L001', 'SP001', 'K004', '2025-01-01', 100, 100, 'kg'), -- Trấu nghiền
    ('L002', 'SP005', 'K004', '2025-01-02', 60,  60,  'kg'), -- Vỏ cà phê đã xử lý
    ('L003', 'SP002', 'K001', '2025-01-03', 50,  50,  'kg'), -- Vỏ cà phê khô
    ('L004', 'SP003', 'K006', '2025-01-04', 200, 200, 'kg'), -- Rơm mì
    ('L005', 'SP004', 'K006', '2025-01-05', 80,  80,  'kg'), -- Xơ dừa
    ('L006', 'SP006', 'K005', '2025-01-06', 120, 120, 'kg'), -- Bã mía
    ('L007', 'SP007', 'K002', '2025-01-07', 90,  90,  'kg'), -- Bắp khô nghiền
    ('L008', 'SP008', 'K002', '2025-01-08', 70,  70,  'kg'), -- Phân compost
    ('L009', 'SP009', 'K002', '2025-01-09', 80,  80,  'kg'), -- Phân hữu cơ vi sinh
    ('L010', 'SP010', 'K003', '2025-01-10', 200, 200, 'kg'), -- Viên nén trấu
    ('L011', 'SP011', 'K003', '2025-01-11', 180, 180, 'kg'); -- Gỗ

    -- 8. INSERT Dữ Liệu Chatbot
    INSERT INTO CauHoiThuongGap (Intent, TrainingSentence, Response, ProductName)
    VALUES
    ('hoi_san_pham', N'Trấu có công dụng gì?', N'Trấu nghiền được dùng làm nguyên liệu sản xuất viên nén hoặc chất đốt sinh học.', N'Trấu nghiền'),
    ('hoi_san_pham', N'Cho tôi biết thông tin về vỏ cà phê khô.', N'Vỏ cà phê khô là phụ phẩm phơi khô, dùng để đốt lò sinh học.', N'Vỏ cà phê khô'),
    ('hoi_san_pham', N'Xơ dừa dùng làm gì vậy?', N'Xơ dừa được dùng làm giá thể trồng cây hoặc đệm lót sinh học.', N'Xơ dừa'),
    ('hoi_san_pham', N'Phân compost là gì?', N'Phân compost là phân hữu cơ từ phụ phẩm nông nghiệp, giàu dinh dưỡng cho cây trồng.', N'Phân compost'),
    ('hoi_san_pham', N'Bã mía có ích gì?', N'Bã mía được dùng làm thức ăn bổ sung cho bò hoặc nguyên liệu sản xuất phân hữu cơ.', N'Bã mía');

    INSERT INTO CauHoiThuongGap (Intent, TrainingSentence, Response, ProductName)
    VALUES
    ('hoi_gia', N'Trấu nghiền giá bao nhiêu?', N'Trấu nghiền hiện có giá khoảng 80.000đ/kg.', N'Trấu nghiền'),
    ('hoi_gia', N'Giá vỏ cà phê khô là bao nhiêu?', N'Vỏ cà phê khô hiện có giá khoảng 70.000đ/kg.', N'Vỏ cà phê khô'),
    ('hoi_gia', N'Cho tôi biết giá xơ dừa.', N'Xơ dừa khô có giá khoảng 60.000đ/kg.', N'Xơ dừa'),
    ('hoi_gia', N'Phân compost giá bao nhiêu?', N'Phân compost có giá khoảng 65.000đ/kg.', N'Phân compost'),
    ('hoi_gia', N'Giá viên nén trấu là bao nhiêu?', N'Viên nén trấu hiện có giá 95.000đ/kg.', N'Viên nén trấu');

    INSERT INTO CauHoiThuongGap (Intent, TrainingSentence, Response)
    VALUES
    ('ban_san_pham', N'Tôi có mớ trấu ở nhà muốn bán.', N'Vui lòng cung cấp thêm thông tin sản phẩm để hệ thống hỗ trợ bạn đăng bán.'),
    ('ban_san_pham', N'Tôi cần bán ít vỏ cà phê.', N'Bạn có thể vào mục "Đăng bán" và điền thông tin về vỏ cà phê.'),
    ('ban_san_pham', N'Tôi có bã mía muốn bán.', N'Bã mía là phụ phẩm phổ biến, bạn có thể đăng bán ngay trên hệ thống.'),
    ('ban_san_pham', N'Tôi muốn bán xơ dừa.', N'Bạn có thể gửi thông tin sản phẩm để chúng tôi giúp đăng bán.'),
    ('ban_san_pham', N'Tôi có ít phân compost muốn bán.', N'Hệ thống sẽ hỗ trợ bạn đăng bán phân compost nếu bạn cung cấp thêm thông tin.');
    
    INSERT INTO CauHoiThuongGap (Intent, TrainingSentence, Response)
    VALUES
    ('mua_san_pham', N'Tôi muốn mua trấu.', N'Bạn có muốn mua sản phẩm trấu này không?'),
    ('mua_san_pham', N'Tôi cần mua ít vỏ cà phê.', N'Bạn có muốn mua sản phẩm vỏ cà phê này không?'),
    ('mua_san_pham', N'Tôi muốn mua bã mía.', N'Bạn có muốn mua sản phẩm bã mía này không?'),
    ('mua_san_pham', N'Tôi cần mua xơ dừa.', N'Bạn có muốn mua sản phẩm xơ dừa này không?'),
    ('mua_san_pham', N'Tôi muốn mua phân compost.', N'Bạn có muốn mua sản phẩm phân compost này không?');
    
    INSERT INTO CauHoiThuongGap (Intent, TrainingSentence, Response)
    VALUES
    ('chao_hoi', N'Xin chào', N'Chào bạn! Tôi có thể giúp gì cho bạn hôm nay?'),
    ('chao_hoi', N'Hello', N'Chào bạn, bạn cần hỗ trợ về sản phẩm nào?'),
    ('chao_hoi', N'Chào shop', N'Chào bạn, rất vui được hỗ trợ bạn!');

    INSERT INTO CauHoiThuongGap (Intent, TrainingSentence, Response)
    VALUES
    ('cam_on', N'Cảm ơn nha', N'Không có gì, rất vui được giúp bạn!'),
    ('cam_on', N'Cảm ơn bạn', N'Rất hân hạnh được hỗ trợ!');

    INSERT INTO CauHoiThuongGap (Intent, TrainingSentence, Response)
    VALUES
    ('tam_biet', N'Tạm biệt', N'Hẹn gặp lại bạn nhé!'),
    ('tam_biet', N'Bye', N'Chúc bạn một ngày tốt lành!');

    -- 9. Sửa Kiểu Dữ Liệu ChatHistory và Thêm Dữ Liệu
    -- (Lệnh này chỉ nên chạy MỘT LẦN, nếu chạy lại sẽ lỗi nếu đã đổi)
    IF COL_LENGTH('ChatHistory', 'NgayChat') IS NOT NULL AND SQL_VARIANT_PROPERTY(COLUMNPROPERTY(OBJECT_ID('ChatHistory'), 'NgayChat', 'BaseType'), 'BaseType') = 'datetime'
    BEGIN
        ALTER TABLE ChatHistory ALTER COLUMN NgayChat datetime2;
    END

    INSERT INTO ChatHistory(CauHoi, CauTraLoi,NgayChat)
    VALUES
    (N'Chính sách vận chuyển', N'Chúng tôi giao hàng trong 3-5 ngày tùy khu vực.', GETUTCDATE());


    -- Nếu tất cả thành công, commit transaction
    COMMIT TRANSACTION;
    PRINT 'Đã cập nhật và chèn dữ liệu (dùng LoTonKho) thành công.';

END TRY
BEGIN CATCH
    -- Nếu có lỗi, rollback
    ROLLBACK TRANSACTION;
    PRINT 'Gặp lỗi! Đã Rollback. Vui lòng kiểm tra lại.';
    -- Hiển thị lỗi chi tiết
    THROW;
END CATCH;

Select *from SanPhams
Select *from LoTonKhos