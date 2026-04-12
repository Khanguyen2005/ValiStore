# ValiStore

ValiStore là đồ án xây dựng hệ thống bán vali trực tuyến, hỗ trợ nhiều vai trò vận hành gồm **Admin**, **Customer** và **Shipper**.

## Mô tả đồ án

Hệ thống tập trung vào quy trình mua hàng thực tế:
- Khách hàng xem sản phẩm, thêm giỏ hàng và đặt hàng.
- Thanh toán trực tuyến qua **VNPay** và **PayPal**.
- Shipper theo dõi và xử lý đơn giao hàng.
- Admin quản lý sản phẩm, đơn hàng và vận hành hệ thống.
- Chat realtime giữa Customer và Shipper để trao đổi trong quá trình giao nhận.

## Công nghệ sử dụng

- **ASP.NET MVC 5** (.NET Framework 4.8.1)
- **Entity Framework 6**
- **SQL Server** (thiết kế và lưu trữ dữ liệu)
- **SignalR** (chat realtime)
- **Bootstrap 5**
- **jQuery / jQuery Validation**
- **OWIN**

## Vai trò từng thành viên

### Nguyễn Đàm Khá
- Là thành viên đóng góp nhiều nhất trong đồ án.
- Thiết kế Database.
- Phát triển phần Admin.
- Tích hợp thanh toán **VNPay** và **PayPal** cho phần Customer.
- Xây dựng tính năng chat realtime bằng **SignalR** giữa Shipper và Customer.
- Tổng quan: các tính năng khó, phức tạp chủ yếu do Khá đảm nhiệm.

### Đinh Lê Thảo Vy
- Phát triển phần giao diện (UI) của Customer.

### Trần Phan Khải
- Phát triển Backend của phần Customer.
- Phát triển toàn bộ phần Shipper.

## Ghi chú

Đây là đồ án nhóm, mỗi thành viên phụ trách mảng riêng; trong đó Nguyễn Đàm Khá chịu trách nhiệm chính ở các phần cốt lõi và tính năng khó.
