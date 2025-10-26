-- Xóa dữ liệu cũ nếu cần (CẨN THẬN KHI CHẠY LỆNH DELETE!)
-- DELETE FROM LoaiSanPhams;
-- =============================================
--          DU LIEU MAU TINH/THANH PHO

-- =============================================
--         INSERT DATA FOR TinhThanhPho
-- =============================================

INSERT INTO TinhThanhPhos (MaTinh, TenTinh) VALUES
('T01', N'Thành phố Hà Nội'),
('T02', N'Thành phố Hồ Chí Minh'),
('T03', N'Thành phố Đà Nẵng'),
('T04', N'Tỉnh Bình Dương'),
('T05', N'Thành phố Cần Thơ');
GO

-- =============================================
--         INSERT DATA FOR QuanHuyen
-- =============================================

-- Quận/Huyện cho Hà Nội (T01)
INSERT INTO QuanHuyens (MaQuan, TenQuan, MaTinh) VALUES
('Q0101', N'Quận Hoàn Kiếm', 'T01'),
('Q0102', N'Quận Ba Đình', 'T01'),
('Q0103', N'Quận Đống Đa', 'T01'),
('Q0104', N'Quận Hai Bà Trưng', 'T01'),
('Q0105', N'Quận Cầu Giấy', 'T01');
GO

-- Quận/Huyện cho TP. Hồ Chí Minh (T02)
INSERT INTO QuanHuyens (MaQuan, TenQuan, MaTinh) VALUES
('Q0201', N'Quận 1', 'T02'),
('Q0202', N'Quận 3', 'T02'),
('Q0203', N'Quận Bình Thạnh', 'T02'),
('Q0204', N'Quận Tân Bình', 'T02'),
('Q0205', N'Thành phố Thủ Đức', 'T02'); -- TP Thủ Đức được coi như một quận/huyện trong ngữ cảnh này
GO

-- Quận/Huyện cho Đà Nẵng (T03)
INSERT INTO QuanHuyens (MaQuan, TenQuan, MaTinh) VALUES
('Q0301', N'Quận Hải Châu', 'T03'),
('Q0302', N'Quận Thanh Khê', 'T03'),
('Q0303', N'Quận Sơn Trà', 'T03'),
('Q0304', N'Quận Ngũ Hành Sơn', 'T03'),
('Q0305', N'Quận Liên Chiểu', 'T03');
GO

-- Quận/Huyện cho Bình Dương (T04)
INSERT INTO QuanHuyens (MaQuan, TenQuan, MaTinh) VALUES
('Q0401', N'Thành phố Thủ Dầu Một', 'T04'),
('Q0402', N'Thành phố Dĩ An', 'T04'),
('Q0403', N'Thành phố Thuận An', 'T04'),
('Q0404', N'Thị xã Bến Cát', 'T04'),
('Q0405', N'Huyện Bắc Tân Uyên', 'T04');
GO

-- Quận/Huyện cho Cần Thơ (T05)
INSERT INTO QuanHuyens (MaQuan, TenQuan, MaTinh) VALUES
('Q0501', N'Quận Ninh Kiều', 'T05'),
('Q0502', N'Quận Bình Thuỷ', 'T05'),
('Q0503', N'Quận Cái Răng', 'T05'),
('Q0504', N'Quận Ô Môn', 'T05'),
('Q0505', N'Huyện Phong Điền', 'T05');
GO

-- =============================================
--         INSERT DATA FOR XaPhuong
-- =============================================

-- Xã/Phường cho Quận Hoàn Kiếm (Q0101), Hà Nội
INSERT INTO XaPhuongs (MaXa, TenXa, MaQuan) VALUES
('X010101', N'Phường Hàng Trống', 'Q0101'),
('X010102', N'Phường Lý Thái Tổ', 'Q0101'),
('X010103', N'Phường Trần Hưng Đạo', 'Q0101'),
('X010104', N'Phường Tràng Tiền', 'Q0101'),
('X010105', N'Phường Hàng Buồm', 'Q0101');
GO

-- Xã/Phường cho Quận Ba Đình (Q0102), Hà Nội
INSERT INTO XaPhuongs (MaXa, TenXa, MaQuan) VALUES
('X010201', N'Phường Phúc Xá', 'Q0102'),
('X010202', N'Phường Trúc Bạch', 'Q0102'),
('X010203', N'Phường Vĩnh Phúc', 'Q0102'),
('X010204', N'Phường Cống Vị', 'Q0102'),
('X010205', N'Phường Liễu Giai', 'Q0102');
GO

-- Xã/Phường cho Quận Đống Đa (Q0103), Hà Nội
INSERT INTO XaPhuongs (MaXa, TenXa, MaQuan) VALUES
('X010301', N'Phường Cát Linh', 'Q0103'),
('X010302', N'Phường Hàng Bột', 'Q0103'),
('X010303', N'Phường Láng Hạ', 'Q0103'),
('X010304', N'Phường Láng Thượng', 'Q0103'),
('X010305', N'Phường Khâm Thiên', 'Q0103');
GO

-- Xã/Phường cho Quận Hai Bà Trưng (Q0104), Hà Nội
INSERT INTO XaPhuongs (MaXa, TenXa, MaQuan) VALUES
('X010401', N'Phường Nguyễn Du', 'Q0104'),
('X010402', N'Phường Lê Đại Hành', 'Q0104'),
('X010403', N'Phường Bùi Thị Xuân', 'Q0104'),
('X010404', N'Phường Phố Huế', 'Q0104'),
('X010405', N'Phường Đống Mác', 'Q0104');
GO

-- Xã/Phường cho Quận Cầu Giấy (Q0105), Hà Nội
INSERT INTO XaPhuongs (MaXa, TenXa, MaQuan) VALUES
('X010501', N'Phường Nghĩa Đô', 'Q0105'),
('X010502', N'Phường Nghĩa Tân', 'Q0105'),
('X010503', N'Phường Mai Dịch', 'Q0105'),
('X010504', N'Phường Dịch Vọng', 'Q0105'),
('X010505', N'Phường Yên Hoà', 'Q0105');
GO

-- Xã/Phường cho Quận 1 (Q0201), TP. HCM
INSERT INTO XaPhuongs (MaXa, TenXa, MaQuan) VALUES
('X020101', N'Phường Tân Định', 'Q0201'),
('X020102', N'Phường Đa Kao', 'Q0201'),
('X020103', N'Phường Bến Nghé', 'Q0201'),
('X020104', N'Phường Bến Thành', 'Q0201'),
('X020105', N'Phường Nguyễn Thái Bình', 'Q0201');
GO

-- Xã/Phường cho Quận 3 (Q0202), TP. HCM
INSERT INTO XaPhuongs (MaXa, TenXa, MaQuan) VALUES
('X020201', N'Phường Võ Thị Sáu', 'Q0202'),
('X020202', N'Phường 1', 'Q0202'),
('X020203', N'Phường 2', 'Q0202'),
('X020204', N'Phường 4', 'Q0202'),
('X020205', N'Phường 5', 'Q0202');
GO

-- Xã/Phường cho Quận Bình Thạnh (Q0203), TP. HCM
INSERT INTO XaPhuongs (MaXa, TenXa, MaQuan) VALUES
('X020301', N'Phường 1', 'Q0203'),
('X020302', N'Phường 2', 'Q0203'),
('X020303', N'Phường 3', 'Q0203'),
('X020304', N'Phường 5', 'Q0203'),
('X020305', N'Phường 6', 'Q0203');
GO

-- Xã/Phường cho Quận Tân Bình (Q0204), TP. HCM
INSERT INTO XaPhuongs (MaXa, TenXa, MaQuan) VALUES
('X020401', N'Phường 1', 'Q0204'),
('X020402', N'Phường 2', 'Q0204'),
('X020403', N'Phường 3', 'Q0204'),
('X020404', N'Phường 4', 'Q0204'),
('X020405', N'Phường 5', 'Q0204');
GO

-- Xã/Phường cho TP Thủ Đức (Q0205), TP. HCM
INSERT INTO XaPhuongs (MaXa, TenXa, MaQuan) VALUES
('X020501', N'Phường An Khánh', 'Q0205'),
('X020502', N'Phường An Lợi Đông', 'Q0205'),
('X020503', N'Phường An Phú', 'Q0205'),
('X020504', N'Phường Bình Chiểu', 'Q0205'),
('X020505', N'Phường Bình Thọ', 'Q0205');
GO


-- Xã/Phường cho Quận Hải Châu (Q0301), Đà Nẵng
INSERT INTO XaPhuongs (MaXa, TenXa, MaQuan) VALUES
('X030101', N'Phường Hải Châu I', 'Q0301'),
('X030102', N'Phường Hải Châu II', 'Q0301'),
('X030103', N'Phường Thạch Thang', 'Q0301'),
('X030104', N'Phường Thanh Bình', 'Q0301'),
('X030105', N'Phường Thuận Phước', 'Q0301');
GO

-- Xã/Phường cho Quận Thanh Khê (Q0302), Đà Nẵng
INSERT INTO XaPhuongs (MaXa, TenXa, MaQuan) VALUES
('X030201', N'Phường Tam Thuận', 'Q0302'),
('X030202', N'Phường Thanh Khê Đông', 'Q0302'),
('X030203', N'Phường Thanh Khê Tây', 'Q0302'),
('X030204', N'Phường Xuân Hà', 'Q0302'),
('X030205', N'Phường An Khê', 'Q0302');
GO

-- Xã/Phường cho Quận Sơn Trà (Q0303), Đà Nẵng
INSERT INTO XaPhuongs (MaXa, TenXa, MaQuan) VALUES
('X030301', N'Phường An Hải Bắc', 'Q0303'),
('X030302', N'Phường An Hải Đông', 'Q0303'),
('X030303', N'Phường An Hải Tây', 'Q0303'),
('X030304', N'Phường Mân Thái', 'Q0303'),
('X030305', N'Phường Nại Hiên Đông', 'Q0303');
GO

-- Xã/Phường cho Quận Ngũ Hành Sơn (Q0304), Đà Nẵng
INSERT INTO XaPhuongs (MaXa, TenXa, MaQuan) VALUES
('X030401', N'Phường Mỹ An', 'Q0304'),
('X030402', N'Phường Khuê Mỹ', 'Q0304'),
('X030403', N'Phường Hoà Quý', 'Q0304'),
('X030404', N'Phường Hoà Hải', 'Q0304'),
('X030405', N'Phường A', 'Q0304'); -- Giả định phường thứ 5
GO

-- Xã/Phường cho Quận Liên Chiểu (Q0305), Đà Nẵng
INSERT INTO XaPhuongs (MaXa, TenXa, MaQuan) VALUES
('X030501', N'Phường Hòa Hiệp Bắc', 'Q0305'),
('X030502', N'Phường Hòa Hiệp Nam', 'Q0305'),
('X030503', N'Phường Hòa Khánh Bắc', 'Q0305'),
('X030504', N'Phường Hòa Khánh Nam', 'Q0305'),
('X030505', N'Phường Hòa Minh', 'Q0305');
GO

-- Xã/Phường cho TP Thủ Dầu Một (Q0401), Bình Dương
INSERT INTO XaPhuongs (MaXa, TenXa, MaQuan) VALUES
('X040101', N'Phường Phú Cường', 'Q0401'),
('X040102', N'Phường Hiệp Thành', 'Q0401'),
('X040103', N'Phường Chánh Nghĩa', 'Q0401'),
('X040104', N'Phường Phú Thọ', 'Q0401'),
('X040105', N'Phường Phú Hòa', 'Q0401');
GO

-- Xã/Phường cho TP Dĩ An (Q0402), Bình Dương
INSERT INTO XaPhuongs (MaXa, TenXa, MaQuan) VALUES
('X040201', N'Phường Dĩ An', 'Q0402'),
('X040202', N'Phường Tân Bình', 'Q0402'),
('X040203', N'Phường Tân Đông Hiệp', 'Q0402'),
('X040204', N'Phường Bình An', 'Q0402'),
('X040205', N'Phường Bình Thắng', 'Q0402');
GO

-- Xã/Phường cho TP Thuận An (Q0403), Bình Dương
INSERT INTO XaPhuongs (MaXa, TenXa, MaQuan) VALUES
('X040301', N'Phường An Thạnh', 'Q0403'),
('X040302', N'Phường Lái Thiêu', 'Q0403'),
('X040303', N'Phường Bình Chuẩn', 'Q0403'),
('X040304', N'Phường Thuận Giao', 'Q0403'),
('X040305', N'Phường An Phú', 'Q0403');
GO

-- Xã/Phường cho Thị xã Bến Cát (Q0404), Bình Dương
INSERT INTO XaPhuongs (MaXa, TenXa, MaQuan) VALUES
('X040401', N'Phường Mỹ Phước', 'Q0404'),
('X040402', N'Phường Chánh Phú Hòa', 'Q0404'),
('X040403', N'Phường Thới Hòa', 'Q0404'),
('X040404', N'Xã An Điền', 'Q0404'),
('X040405', N'Xã An Tây', 'Q0404');
GO

-- Xã/Phường cho Huyện Bắc Tân Uyên (Q0405), Bình Dương
INSERT INTO XaPhuongs (MaXa, TenXa, MaQuan) VALUES
('X040501', N'Thị trấn Tân Thành', 'Q0405'),
('X040502', N'Xã Tân Bình', 'Q0405'),
('X040503', N'Xã Bình Mỹ', 'Q0405'),
('X040504', N'Xã Tân Lập', 'Q0405'),
('X040505', N'Xã Đất Cuốc', 'Q0405');
GO


-- Xã/Phường cho Quận Ninh Kiều (Q0501), Cần Thơ
INSERT INTO XaPhuongs (MaXa, TenXa, MaQuan) VALUES
('X050101', N'Phường Cái Khế', 'Q0501'),
('X050102', N'Phường An Hòa', 'Q0501'),
('X050103', N'Phường Thới Bình', 'Q0501'),
('X050104', N'Phường An Nghiệp', 'Q0501'),
('X050105', N'Phường An Cư', 'Q0501');
GO

-- Xã/Phường cho Quận Bình Thuỷ (Q0502), Cần Thơ
INSERT INTO XaPhuongs (MaXa, TenXa, MaQuan) VALUES
('X050201', N'Phường Bình Thuỷ', 'Q0502'),
('X050202', N'Phường Trà An', 'Q0502'),
('X050203', N'Phường Trà Nóc', 'Q0502'),
('X050204', N'Phường Thới An Đông', 'Q0502'),
('X050205', N'Phường An Thới', 'Q0502');
GO

-- Xã/Phường cho Quận Cái Răng (Q0503), Cần Thơ
INSERT INTO XaPhuongs (MaXa, TenXa, MaQuan) VALUES
('X050301', N'Phường Lê Bình', 'Q0503'),
('X050302', N'Phường Hưng Phú', 'Q0503'),
('X050303', N'Phường Hưng Thạnh', 'Q0503'),
('X050304', N'Phường Ba Láng', 'Q0503'),
('X050305', N'Phường Thường Thạnh', 'Q0503');
GO

-- Xã/Phường cho Quận Ô Môn (Q0504), Cần Thơ
INSERT INTO XaPhuongs (MaXa, TenXa, MaQuan) VALUES
('X050401', N'Phường Châu Văn Liêm', 'Q0504'),
('X050402', N'Phường Thới Hòa', 'Q0504'),
('X050403', N'Phường Thới Long', 'Q0504'),
('X050404', N'Phường Long Hưng', 'Q0504'),
('X050405', N'Phường Thới An', 'Q0504');
GO

-- Xã/Phường cho Huyện Phong Điền (Q0505), Cần Thơ
INSERT INTO XaPhuongs (MaXa, TenXa, MaQuan) VALUES
('X050501', N'Thị trấn Phong Điền', 'Q0505'),
('X050502', N'Xã Nhơn Ái', 'Q0505'),
('X050503', N'Xã Giai Xuân', 'Q0505'),
('X050504', N'Xã Tân Thới', 'Q0505'),
('X050505', N'Xã Trường Long', 'Q0505');
GO

-- === NGUYÊN LIỆU THÔ ===
INSERT INTO LoaiSanPhams (M_LoaiSP, TenLoai) VALUES
('NLT01', N'Rơm rạ'),
('NLT02', N'Vỏ trấu'),
('NLT03', N'Bã mía'),
('NLT04', N'Lõi ngô / Cùi bắp'),
('NLT05', N'Thân cây nông nghiệp'),
('NLT06', N'Vỏ cà phê / điều'),
('NLT07', N'Xơ dừa');

-- === ĐÃ QUA XỬ LÝ ===
INSERT INTO LoaiSanPhams (M_LoaiSP, TenLoai) VALUES
('DQX01', N'Nghiền / Xay'),
('DQX02', N'Ép khối / Băm nhỏ'),
('DQX03', N'Viên nén năng lượng (Đã xử lý)'), -- Phân biệt với NLSH
('DQX04', N'Phơi khô / Sấy'),
('DQX05', N'Ủ chua / Lên men');

-- === THỨC ĂN CHĂN NUÔI ===
INSERT INTO LoaiSanPhams (M_LoaiSP, TenLoai) VALUES
('TACN01', N'Thức ăn gia súc'),
('TACN02', N'Thức ăn gia cầm'),
('TACN03', N'Thức ăn thủy sản'),
('TACN04', N'Nguyên liệu TĂCN'),
('TACN05', N'Phụ gia TĂCN');

-- === PHÂN BÓN & GIÁ THỂ ===
INSERT INTO LoaiSanPhams (M_LoaiSP, TenLoai) VALUES
('PBGT01', N'Phân compost hữu cơ'),
('PBGT02', N'Phân trùn quế'),
('PBGT03', N'Giá thể trồng nấm'),
('PBGT04', N'Giá thể trồng cây'),
('PBGT05', N'Vi sinh cải tạo đất');

-- === NĂNG LƯỢNG SINH KHỐI ===
INSERT INTO LoaiSanPhams (M_LoaiSP, TenLoai) VALUES
('NLSH01', N'Viên nén (Trấu, Gỗ,...)'),
('NLSH02', N'Củi ép / Briket'),
('NLSH03', N'Dăm gỗ'),
('NLSH04', N'Khí sinh học (BIOGAS)'),
('NLSH05', N'Dầu sinh học');

-- === DỊCH VỤ ===
INSERT INTO LoaiSanPhams (M_LoaiSP, TenLoai) VALUES
('DV01', N'Thu gom tại nguồn'),
('DV02', N'Vận chuyển'),
('DV03', N'Xử lý theo yêu cầu'),
('DV04', N'Tư vấn giải pháp');

-- === THIẾT BỊ ===
INSERT INTO LoaiSanPhams (M_LoaiSP, TenLoai) VALUES
('TB01', N'Máy nghiền / Băm'),
('TB02', N'Máy ép viên nén'),
('TB03', N'Máy ép khối'),
('TB04', N'Máy sấy nông sản');

-- === VẬT TƯ KHÁC ===
INSERT INTO LoaiSanPhams (M_LoaiSP, TenLoai) VALUES
('VTK01', N'Bao bì đóng gói'),
('VTK02', N'Chế phẩm sinh học');

-- === ĐƠN VỊ TÍNH (VÍ DỤ) ===
-- Giả sử bạn cũng muốn thêm vào bảng DonViTinhs
INSERT INTO DonViTinhs (M_DonViTinh, TenLoaiTinh) VALUES
('KG', N'Kilogram'),
('Tan', N'Tấn'),
('Bao', N'Bao'),
('MetKhoi', N'Mét khối'),
('Chuyen', N'Chuyến'),
('Cai', N'Cái');



delete from LoaiSanPhams