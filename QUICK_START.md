# Quick Start - Test Payment System

## B??c 1: Ki?m tra Port
1. Right-click project ? Properties ? Web
2. Xem **Project Url**: `http://localhost:XXXXX`
3. N?u port khác **44300**, update Web.config:
   ```xml
   <add key="vnp_ReturnUrl" value="http://localhost:YOUR_PORT/Checkout/VnPayReturn" />
   ```

## B??c 2: Build & Run
```bash
# Trong Visual Studio
Press F5 ho?c Ctrl+F5
```

## B??c 3: Test COD Payment
1. M? browser: `http://localhost:44300`
2. Click **Register** ? T?o account m?i
3. Login vào h? th?ng
4. Vào trang **Shop** ho?c **Home**
5. Click vào 1 s?n ph?m
6. Click **Add to Cart**
7. Click icon gi? hàng (góc ph?i navbar)
8. Click **Proceed to Checkout**
9. Nh?p thông tin:
   - Full Name: Test User
 - Phone: 0901234567
   - Address: 123 Test Street
10. Ch?n **Cash on Delivery (COD)**
11. Click **Place Order**
12. ? Success! Xem confirmation page

## B??c 4: Test VNPay Payment
1. Làm l?i b??c 3-8
2. Ch?n **VNPay**
3. Click **Place Order**
4. Redirect sang VNPay Sandbox
5. Ch?n **Thanh toán qua th? ATM n?i ??a**
6. Ch?n ngân hàng: **NCB**
7. Nh?p:
   - S? th?: `9704198526191432198`
   - Tên: `NGUYEN VAN A`
   - Ngày phát hành: `07/15`
8. Click **Ti?p t?c**
9. Nh?p OTP: `123456`
10. Click **Xác nh?n thanh toán**
11. ? Redirect v? confirmation page!

## B??c 5: Ki?m tra Database
```sql
-- Check order
SELECT TOP 10 * FROM Orders ORDER BY id DESC

-- Check payment
SELECT TOP 10 * FROM Payments ORDER BY id DESC

-- Check order details
SELECT od.*, p.name as product_name 
FROM Order_Details od
JOIN Products p ON od.product_id = p.id
ORDER BY od.id DESC
```

## Common Issues

### Port không ?úng
```
Error: "The requested URL was not found"
Solution: Check Properties ? Web ? Project Url, update Web.config
```

### VNPay timeout
```
Error: "Payment timeout"
Solution: T?o order m?i, làm nhanh h?n (< 15 phút)
```

### Signature invalid
```
Error: "Invalid payment signature"
Solution: Check vnp_HashSecret trong Web.config không có space th?a
```

## Xem Log
```csharp
// Trong CheckoutController.VnPayReturn
// Uncomment ?? debug:
foreach (string key in Request.QueryString.AllKeys)
{
    System.Diagnostics.Debug.WriteLine($"{key}: {Request.QueryString[key]}");
}
```

## Next Steps
- ??c `PAYMENT_GUIDE.md` ?? hi?u chi ti?t
- Test error cases (cancel payment, insufficient balance)
- Add validation rules
- Customize UI

## Success Criteria
- ? COD order created with status "Pending"
- ? VNPay redirect works
- ? Payment success ? Order status "Processing"
- ? Payment failed ? Order status "Cancelled"
- ? Cart badge shows correct count
- ? Confirmation page displays order info

Happy coding! ??
