USE [ZHome];
GO

-- Cập nhật dữ liệu bảng locations chuẩn chuẩn hóa đầy đủ cho Hà Nội & Hòa Lạc
ALTER TABLE [dbo].[locations] NOCHECK CONSTRAINT ALL;
DELETE FROM [dbo].[locations];
ALTER TABLE [dbo].[locations] CHECK CONSTRAINT ALL;
GO

SET IDENTITY_INSERT [dbo].[locations] ON;

INSERT INTO [dbo].[locations] ([id], [name], [type], [parent_id], [level]) VALUES 
(1, N'Hà Nội', N'Thành phố', NULL, 1),
(2, N'Quận Ba Đình', N'Quận', 1, 2),
(3, N'Quận Bắc Từ Liêm', N'Quận', 1, 2),
(4, N'Quận Cầu Giấy', N'Quận', 1, 2),
(5, N'Quận Đống Đa', N'Quận', 1, 2),
(6, N'Quận Hà Đông', N'Quận', 1, 2),
(7, N'Quận Hai Bà Trưng', N'Quận', 1, 2),
(8, N'Quận Hoàn Kiếm', N'Quận', 1, 2),
(9, N'Quận Hoàng Mai', N'Quận', 1, 2),
(10, N'Quận Long Biên', N'Quận', 1, 2),
(11, N'Quận Nam Từ Liêm', N'Quận', 1, 2),
(12, N'Quận Tây Hồ', N'Quận', 1, 2),
(13, N'Quận Thanh Xuân', N'Quận', 1, 2),
(14, N'Thị xã Sơn Tây', N'Thị xã', 1, 2),
(15, N'Huyện Ba Vì', N'Huyện', 1, 2),
(16, N'Huyện Chương Mỹ', N'Huyện', 1, 2),
(17, N'Huyện Đan Phượng', N'Huyện', 1, 2),
(18, N'Huyện Đông Anh', N'Huyện', 1, 2),
(19, N'Huyện Gia Lâm', N'Huyện', 1, 2),
(20, N'Huyện Hoài Đức', N'Huyện', 1, 2),
(21, N'Huyện Mê Linh', N'Huyện', 1, 2),
(22, N'Huyện Mỹ Đức', N'Huyện', 1, 2),
(23, N'Huyện Phú Xuyên', N'Huyện', 1, 2),
(24, N'Huyện Phúc Thọ', N'Huyện', 1, 2),
(25, N'Huyện Quốc Oai', N'Huyện', 1, 2),
(26, N'Huyện Sóc Sơn', N'Huyện', 1, 2),
(27, N'Huyện Thạch Thất', N'Huyện', 1, 2),
(28, N'Huyện Thanh Oai', N'Huyện', 1, 2),
(29, N'Huyện Thanh Trì', N'Huyện', 1, 2),
(30, N'Huyện Thường Tín', N'Huyện', 1, 2),
(31, N'Huyện Ứng Hòa', N'Huyện', 1, 2),
-- Xã / Phường nổi bật
(100, N'Xã Tân Xã', N'Xã', 27, 3),
(101, N'Xã Thạch Hòa', N'Xã', 27, 3),
(102, N'Xã Bình Yên', N'Xã', 27, 3),
(103, N'Xã Hạ Bằng', N'Xã', 27, 3),
(104, N'Xã Cần Kiệm', N'Xã', 27, 3),
(105, N'Xã Đồng Trúc', N'Xã', 27, 3),
(200, N'Phường Dịch Vọng', N'Phường', 4, 3),
(201, N'Phường Dịch Vọng Hậu', N'Phường', 4, 3),
(202, N'Phường Mai Dịch', N'Phường', 4, 3),
(300, N'Phường Mỹ Đình 1', N'Phường', 11, 3),
(301, N'Phường Mỹ Đình 2', N'Phường', 11, 3),
(302, N'Phường Mễ Trì', N'Phường', 11, 3),
(400, N'Phường Khương Đình', N'Phường', 13, 3),
(401, N'Phường Nhân Chính', N'Phường', 13, 3);

SET IDENTITY_INSERT [dbo].[locations] OFF;
GO
