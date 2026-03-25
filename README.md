# ValiStore – E-Commerce Web Application

**Dự án cá nhân** | ASP.NET MVC · SQL Server · SignalR · PayPal · VNPay

## Mô tả

Nền tảng thương mại điện tử bán lẻ thời trang được xây dựng trên ASP.NET MVC 5, hỗ trợ ba vai trò: **Khách hàng**, **Shipper** và **Admin**. Ứng dụng cho phép mua sắm trực tuyến, thanh toán đa phương thức, theo dõi đơn hàng và nhắn tin thời gian thực giữa khách hàng và shipper thông qua SignalR.

---

## Công nghệ sử dụng

| Loại | Công nghệ |
|------|-----------|
| **Backend** | ASP.NET MVC 5, .NET Framework 4.8.1 |
| **ORM** | Entity Framework 6 (Database-First) |
| **Database** | Microsoft SQL Server (AWS RDS) |
| **Real-time** | SignalR 2.4.3 |
| **Frontend** | Bootstrap 5, jQuery 3.7, Razor View |
| **Validation** | jQuery Validation, Unobtrusive JavaScript |
| **Authentication** | Forms Authentication, Claims-based Identity |
| **Payment** | PayPal REST API (OAuth 2.0), VNPay (HMAC-SHA512) |
| **Serialization** | Newtonsoft.Json |
| **Middleware** | OWIN |

---

## Tính năng đã thực hiện

### 🛍️ Khách hàng
- Xem danh mục sản phẩm, lọc theo danh mục và thương hiệu
- Giỏ hàng dựa trên Session (thêm, xóa, cập nhật số lượng)
- Thanh toán qua **PayPal** (OAuth 2.0) và **VNPay** (HMAC-SHA512)
- Quản lý đơn hàng: xem lịch sử, trạng thái, chi tiết đơn
- Nhắn tin thời gian thực với shipper qua **SignalR**
- Đăng ký, đăng nhập, quản lý thông tin tài khoản

### 🚚 Shipper
- Dashboard theo dõi đơn hàng được giao (lọc theo trạng thái)
- Cập nhật trạng thái giao hàng và ghi chú giao hàng
- Nhắn tin thời gian thực với khách hàng theo từng đơn hàng

### ⚙️ Admin
- Quản lý sản phẩm (CRUD): hình ảnh, giá, tồn kho, màu sắc, kích cỡ
- Quản lý danh mục, thương hiệu, banner quảng cáo
- Quản lý đơn hàng, trạng thái thanh toán, phân công shipper
- Quản lý tài khoản người dùng và phân quyền (Admin / Shipper / Customer)
- Dashboard thống kê tổng quan

---

## Kiến trúc hệ thống

- **Monolithic MVC với Areas** – phân tách rõ khu vực Admin và khách hàng
- **Layered Architecture**: Presentation (Controllers + Views) → Business Logic (Services) → Data Access (EF DbContext)
- **Database-First EF**: Sinh model tự động từ schema SQL Server
- **SignalR Hub**: Phòng chat theo đơn hàng (`order_{orderId}`), lưu lịch sử tin nhắn vào database
- **Role-based Authorization**: Ba vai trò phân quyền rõ ràng qua Claims Identity
