# My Orders Feature - User Guide

## Overview
Feature "My Orders" cho phép ng??i dùng xem và qu?n lý các ??n hàng c?a h?.

## Files Created

### Controllers
- `Controllers/OrderController.cs` - User order controller v?i các actions:
  - `Index` - Hi?n th? danh sách ??n hàng
  - `Details` - Xem chi ti?t ??n hàng
  - `Cancel` - H?y ??n hàng (ch? cho ??n Pending)

### View Models
- `Models/ViewModels/UserOrderViewModels.cs` - Ch?a các ViewModels:
  - `UserOrdersVM` - Danh sách ??n hàng
  - `UserOrderItemVM` - Item trong list
  - `UserOrderDetailsVM` - Chi ti?t ??n hàng
  - `UserOrderItemDetailVM` - Chi ti?t s?n ph?m
  - `UserOrderPaymentVM` - Thông tin thanh toán

### Views
- `Views/Order/Index.cshtml` - Trang danh sách ??n hàng
- `Views/Order/Details.cshtml` - Trang chi ti?t ??n hàng

## Features

### 1. Order List (`/Order/Index`)
- **Filter by Status**: L?c ??n hàng theo tr?ng thái
  - All (t?t c?)
  - Pending (?ang ch?)
  - Confirmed (?ã xác nh?n)
  - Shipped (?ang giao)
  - Completed (hoàn thành)
  - Cancelled (?ã h?y)

- **Display Information**:
  - Order code
  - Order date
  - Status badge v?i màu phù h?p
  - S? l??ng items
  - Payment method
  - Total amount
  - Action button ?? xem details

- **Pagination**: Hi?n th? 10 ??n hàng/trang

- **Empty State**: Hi?n th? message thân thi?n khi không có ??n hàng

### 2. Order Details (`/Order/Details/{id}`)
- **Order Status Timeline**: Visual timeline hi?n th? ti?n trình ??n hàng
  - Order Placed
  - Order Confirmed
  - Order Shipped
  - Order Completed

- **Order Items**: B?ng hi?n th? chi ti?t s?n ph?m
  - Product image
  - Product name (link ??n product details)
  - Color & Size variants
  - Price, Quantity, Subtotal
  - Total amount

- **Shipping Information**:
  - Phone number
  - Delivery address

- **Payment Information**:
  - Payment method
  - Payment status (badge)
  - Transaction ID (n?u có)
  - Payment date & amount

- **Cancel Order Button**: Ch? hi?n th? cho ??n hàng Pending
  - Modal confirmation
  - Restore product stock khi cancel
  - Update payment status

### 3. Order Cancellation
- **Conditions**: Ch? cho phép cancel ??n hàng Pending
- **Effects**:
  - Update order status thành "Cancelled"
  - Restore product stock (+quantity)
  - Gi?m sold count (-quantity)
  - Update payment status thành "Failed" (n?u pending)
- **UI**: Modal confirmation v?i warning message

## Security & Authorization
- T?t c? routes yêu c?u authentication (`[Authorize]`)
- Ch? user có th? xem orders c?a chính mình
- Validation user_id khi truy c?p order details
- CSRF protection v?i AntiForgeryToken

## Database Integration
- S? d?ng Entity Framework
- Include related entities (Order_Details, Product, Color, Size, Payments)
- Optimized queries v?i pagination
- Transaction handling khi cancel order

## UI/UX Features

### Styling
- Bootstrap 5 responsive design
- Custom CSS cho timeline
- Status badges v?i màu phù h?p
- Hover effects trên order cards
- Icons t? Bootstrap Icons

### Status Colors
- **Pending**: Warning (yellow)
- **Confirmed**: Info (cyan)
- **Shipped**: Primary (blue)
- **Completed**: Success (green)
- **Cancelled**: Danger (red)

### Responsive Design
- Mobile-friendly layout
- Responsive table cho order items
- Collapsible filters trên mobile
- Touch-friendly buttons

## Integration v?i Layout

Menu item ?ã ???c thêm vào `Views/Shared/_Layout.cshtml`:
```html
<li>
    <a href="@Url.Action("Index", "Order")" class="dropdown-item">
        <i class="bi bi-bag-check me-2"></i>My Orders
    </a>
</li>
```

## Usage Examples

### Xem t?t c? ??n hàng
```
GET /Order/Index
```

### L?c ??n hàng theo status
```
GET /Order/Index?status=Pending
GET /Order/Index?status=Completed
```

### Xem chi ti?t ??n hàng
```
GET /Order/Details/123
```

### H?y ??n hàng
```
POST /Order/Cancel/123
(Requires AntiForgeryToken)
```

## Testing Checklist

- [ ] Verify authentication required
- [ ] Test filter by all statuses
- [ ] Test pagination
- [ ] Test order details display
- [ ] Test cancel order functionality
- [ ] Test stock restoration on cancel
- [ ] Test user can only see their orders
- [ ] Test responsive design
- [ ] Test empty state display
- [ ] Test payment information display

## Future Enhancements

Possible improvements:
1. **Order Tracking**: Real-time order tracking integration
2. **Reorder**: Quick reorder functionality
3. **Order Search**: Search orders by product name or order code
4. **Export**: Download order history as PDF/Excel
5. **Reviews**: Leave product reviews after order completion
6. **Notifications**: Email/SMS notifications on status changes
7. **Order Filtering**: Advanced filters (date range, price range)
8. **Order Sorting**: Sort by date, amount, status

## Notes
- ??m b?o ?ã ch?y SQL migration ?? thêm PayPal payment method
- Stock ???c restore t? ??ng khi cancel order
- Payment status ???c update t? ??ng
- Order timeline ch? hi?n th? cho các status h?p l?
