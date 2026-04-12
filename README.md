# ValiStore - E-commerce Website for Luggage

ValiStore là đồ án xây dựng nền tảng bán vali trực tuyến theo mô hình nhiều vai trò: **Admin**, **Customer** và **Shipper**.  
Hệ thống tập trung vào trải nghiệm mua hàng thực tế, quản trị vận hành và xử lý đơn giao hàng xuyên suốt.

## Tổng quan chức năng

- Quản lý sản phẩm, danh mục, thương hiệu và banner ở trang quản trị.
- Mua hàng với giỏ hàng và quy trình checkout đầy đủ.
- Tích hợp thanh toán trực tuyến qua **VNPay** và **PayPal**.
- Quản lý đơn hàng, trạng thái giao nhận và cập nhật theo từng vai trò.
- Chat realtime giữa **Customer** và **Shipper** bằng **SignalR**.

## Công nghệ sử dụng

- **ASP.NET MVC 5** (.NET Framework 4.8.1)
- **Entity Framework 6**
- **SQL Server**
- **SignalR**
- **OWIN**
- **Bootstrap 5**
- **jQuery / jQuery Validation**

## ERD (Entity Relationship Diagram)

<img src="https://github.com/user-attachments/assets/2f354c61-72e8-484c-9d73-4cd4527037aa" alt="ValiStore ERD" width="100%" />

## Phân công vai trò thành viên

### Nguyễn Đàm Khá
- Thành viên đóng góp chính và nhiều nhất của dự án.
- Thiết kế **Database** và mô hình dữ liệu.
- Phát triển phần **Admin**.
- Tích hợp thanh toán **VNPay** và **PayPal** cho phần **Customer**.
- Xây dựng chat realtime bằng **SignalR** giữa **Shipper** và **Customer**.
- Tổng quan: các tính năng khó và phức tạp chủ yếu do Khá đảm nhiệm.

### Đinh Lê Thảo Vy
- Phụ trách phần giao diện (**UI**) của **Customer**.

### Trần Phan Khải
- Phụ trách **Backend Customer**.
- Phụ trách toàn bộ phần **Shipper**.

## Triển khai

- Dự án đã từng được triển khai lên **AWS** trong giai đoạn vận hành.
- Hiện tại môi trường AWS cũ không còn được duy trì.
