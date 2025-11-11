# ValiModern - Payment System Documentation

## H? Th?ng Thanh Toán

H? th?ng h? tr? **2 ph??ng th?c thanh toán**:

### 1. COD (Cash on Delivery)
- Thanh toán khi nh?n hàng
- ??n hàng ???c t?o v?i tr?ng thái "Pending"
- Khách hàng chu?n b? ti?n m?t khi nh?n hàng

### 2. VNPay
- Thanh toán tr?c tuy?n qua c?ng VNPay
- Tích h?p sandbox VNPay (dùng ?? test)
- Sau khi thanh toán thành công, ??n hàng chuy?n sang "Processing"

---

## C?u Hình VNPay ? (?Ã C?P NH?T)

### Thông Tin VNPay Sandbox ?ã ??ng Ký:
```
TmnCode: 3Q31F2ZM
HashSecret: JRU3EZ2CRN0RN9ZN7HWUKBNQV1SI3EAE
BaseUrl: https://sandbox.vnpayment.vn/paymentv2/vpcpay.html
ReturnUrl: http://localhost:44300/Checkout/VnPayReturn
```

?? **L?u ý quan tr?ng:**
- Port m?c ??nh trong config là **44300**
- N?u project ch?y port khác, update `vnp_ReturnUrl` trong `Web.config`
- Check port: Right-click project ? Properties ? Web ? Project Url

### Test VNPay Sandbox
VNPay Sandbox cung c?p **th? test mi?n phí**:

#### Th? N?i ??a (ATM)
- **S? th?:** 9704198526191432198
- **Tên ch? th?:** NGUYEN VAN A
- **Ngày phát hành:** 07/15
- **M?t kh?u OTP:** 123456

#### Th? Qu?c T? (VISA/Master)
- **S? th?:** 4111111111111111 (VISA) ho?c 5111111111111118 (MasterCard)
- **Tên:** TEST USER
- **CVV:** 123
- **Exp:** 12/25

---

## Cách S? D?ng

### Flow Mua Hàng
1. **Duy?t s?n ph?m** ? Ch?n màu/size
2. **Add to Cart** ? Thêm vào gi? hàng
3. **View Cart** (`/Cart`) ? Xem và ?i?u ch?nh gi? hàng
4. **Checkout** (`/Checkout`) ? Nh?p thông tin giao hàng (ph?i login)
5. **Ch?n ph??ng th?c thanh toán:**
   - **COD**: Hoàn t?t ??n hàng ? Confirmation
   - **VNPay**: Redirect ??n VNPay ? Nh?p th? test ? Return ? Confirmation

### Ki?m Tra Port c?a Project
1. M? Visual Studio
2. Right-click vào project **ValiModern**
3. Properties ? Web ? Project Url
4. N?u port khác **44300**, update Web.config:

```xml
<add key="vnp_ReturnUrl" value="http://localhost:YOUR_PORT/Checkout/VnPayReturn" />
```

### Test Payment Flow

#### Test COD:
1. Login vào h? th?ng
2. Add s?n ph?m vào cart
3. Checkout ? Ch?n **COD**
4. Submit ? Xem confirmation page
5. Check database: Orders.status = "Pending"

#### Test VNPay:
1. Login vào h? th?ng
2. Add s?n ph?m vào cart
3. Checkout ? Ch?n **VNPay**
4. Submit ? Redirect sang VNPay sandbox
5. Ch?n **Thanh toán qua th? ATM n?i ??a**
6. Ch?n ngân hàng: **NCB - Ngân hàng TMCP Qu?c Dân**
7. Nh?p thông tin th? test (xem trên)
8. Nh?p OTP: **123456**
9. Xác nh?n ? Return v? `/Checkout/VnPayReturn`
10. Check database: Orders.status = "Processing", Payments.status = "Completed"

---

## API Endpoints

#### Cart
- `GET /Cart` - Xem gi? hàng
- `POST /Cart/AddToCart` - Thêm s?n ph?m (productId, quantity, colorId?, sizeId?)
- `POST /Cart/UpdateQuantity` - C?p nh?t s? l??ng
- `POST /Cart/RemoveFromCart` - Xóa s?n ph?m
- `POST /Cart/ClearCart` - Xóa toàn b? gi?

#### Checkout
- `GET /Checkout` - Trang checkout (yêu c?u ??ng nh?p)
- `POST /Checkout/ProcessOrder` - X? lý ??n hàng
- `GET /Checkout/VnPayPayment?orderId=X` - Redirect ??n VNPay
- `GET /Checkout/VnPayReturn` - Callback t? VNPay (VNPay t? ??ng g?i)
- `GET /Checkout/Confirmation?orderId=X` - Xác nh?n ??n hàng

---

## Database Schema

### Table: Orders
```sql
- id (int, PK)
- user_id (int, FK)
- order_date (datetime)
- status (nvarchar) -- Pending, Processing, Completed, Cancelled
- total_amount (decimal)
- phone (nvarchar)
- shipping_address (nvarchar)
- created_at, updated_at
```

### Table: Order_Details
```sql
- id (int, PK)
- order_id (int, FK)
- product_id (int, FK)
- quantity (int)
- price (decimal)
- color_id (int?, FK)
- size_id (int?, FK)
- created_at
```

### Table: Payments
```sql
- id (int, PK)
- order_id (int, FK)
- amount (decimal)
- payment_method (nvarchar) -- COD, VNPay
- status (nvarchar) -- Pending, Completed, Failed
- transaction_id (nvarchar) -- VNPay transaction ID
- payment_date (datetime)
```

---

## Troubleshooting

### L?i: "Invalid payment signature"
**Nguyên nhân:**
- HashSecret sai ho?c có kho?ng tr?ng th?a
- Version VNPay API không kh?p

**Gi?i pháp:**
1. Ki?m tra `vnp_HashSecret` trong Web.config
2. ??m b?o không có space/newline trong value
3. Verify v?i VNPay Dashboard

### L?i: "Invalid VNPay response" ho?c không redirect v?
**Nguyên nhân:**
- Return URL không ?úng
- Port không kh?p

**Gi?i pháp:**
1. Check port trong Properties ? Web
2. Update `vnp_ReturnUrl` cho ?úng
3. N?u dùng IIS: URL có th? là `http://localhost/ValiModern/Checkout/VnPayReturn`

### L?i: Cart badge không hi?n th?
**Nguyên nhân:**
- Thi?u using statement trong _Layout.cshtml

**Gi?i pháp:**
1. M? `Views/Shared/_Layout.cshtml`
2. Thêm ? ??u file:
```csharp
@using ValiModern.Models.ViewModels
```

### VNPay luôn tr? v? l?i "07" (Suspected fraud)
**Nguyên nhân:**
- Amount quá l?n cho sandbox
- TmnCode không h?p l?

**Gi?i pháp:**
1. Test v?i ??n hàng nh? (< 1,000,000 VND)
2. Verify TmnCode trên VNPay Dashboard

### VNPay báo "Payment timeout"
**Nguyên nhân:**
- Test quá lâu (sandbox timeout ~15 phút)

**Gi?i pháp:**
- Làm nhanh h?n ho?c t?o order m?i

### Debug VNPay Response
Thêm code debug trong `VnPayReturn`:
```csharp
// Log all query string
foreach (string key in Request.QueryString.AllKeys)
{
    System.Diagnostics.Debug.WriteLine($"{key}: {Request.QueryString[key]}");
}
```

---

## VNPay Response Codes

| Code | Meaning |
|------|---------|
| 00 | Giao d?ch thành công |
| 07 | Tr? ti?n thành công, nghi ng? gian l?n |
| 09 | Th? ch?a ??ng ký Internet Banking |
| 10 | Nh?p sai thông tin th? quá 3 l?n |
| 11 | H?t th?i gian thanh toán |
| 12 | Th? b? khóa |
| 13 | Sai OTP |
| 24 | Khách hàng h?y giao d?ch |
| 51 | Tài kho?n không ?? ti?n |
| 65 | V??t quá h?n m?c giao d?ch |
| 75 | Ngân hàng thanh toán ?ang b?o trì |
| 79 | S? ti?n v??t quá h?n m?c |

---

## Security Notes

?? **Quan tr?ng:**
1. **Không commit** `vnp_TmnCode` và `vnp_HashSecret` th?t vào Git
   - S? d?ng `.gitignore` ?? exclude Web.config
   - Ho?c dùng Web.config transform cho production
2. Trên production:
   - Dùng **environment variables** (Azure App Settings)
   - Ho?c **Azure Key Vault**
3. **Validate signature** t? VNPay tr??c khi c?p nh?t order status
4. **Log** m?i VNPay transactions ?? audit
5. Không tin t??ng data t? client, luôn verify trên server

---

## Production Checklist

- [ ] ??i sang VNPay Production URL
- [ ] Update TmnCode và HashSecret t? VNPay Production
- [ ] Set ReturnUrl v? domain th?t (https://yourdomain.com/Checkout/VnPayReturn)
- [ ] Enable HTTPS (b?t bu?c cho production)
- [ ] Move credentials to Azure App Settings
- [ ] Setup logging/monitoring
- [ ] Test v?i th? th?t (s? ti?n nh?)
- [ ] Implement IPN (Instant Payment Notification) URL
- [ ] Add email notification
- [ ] Setup webhook cho order status updates

---

## Next Steps / Improvements

- [ ] ? Tích h?p VNPay Sandbox (DONE)
- [ ] Thêm email notification sau khi ??t hàng
- [ ] Implement order tracking page
- [ ] Thêm shipping fee calculation
- [ ] Coupon/discount code system
- [ ] Multiple payment methods (MoMo, ZaloPay)
- [ ] Admin: Payment reconciliation reports
- [ ] IPN URL handler (VNPay g?i server khi thanh toán xong)
- [ ] Refund functionality
- [ ] Order cancellation flow

---

## Useful Links

- VNPay Sandbox: https://sandbox.vnpayment.vn/
- VNPay API Docs: https://sandbox.vnpayment.vn/apis/docs/
- VNPay Support: https://vnpay.vn/lien-he
- Test Cards: https://sandbox.vnpayment.vn/apis/docs/test-cards/

---

## Contact

**Project:** ValiModern E-commerce  
**Payment Integration:** COD + VNPay  
**Version:** 1.0  
**Last Updated:** 2024

**Developer Notes:**
- ?ã test thành công trên sandbox
- Return URL config: http://localhost:44300/Checkout/VnPayReturn
- S? d?ng th? test NCB - 9704198526191432198
