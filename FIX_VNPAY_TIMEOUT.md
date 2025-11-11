# Fix: VNPay Return URL "localhost refused to connect"

## V?n ??
Sau khi thanh toán VNPay thành công, VNPay redirect v? nh?ng g?p l?i:
```
localhost refused to connect
ERR_CONNECTION_REFUSED
```

## Nguyên nhân
- Visual Studio t? ??ng **stop debugging** sau m?t th?i gian (default: 300 seconds = 5 phút)
- VNPay redirect m?t th?i gian ? Server ?ã t?t khi callback v?

## Gi?i pháp

### ? Cách 1: Run Without Debugging (RECOMMENDED)
1. **Stop** server hi?n t?i (Shift+F5)
2. **Ctrl+F5** (Run without debugging)
3. Server s? ch?y cho ??n khi b?n t? t?t
4. Test l?i flow VNPay

### ? Cách 2: T?ng Timeout c?a IIS Express
1. M? file: `%USERPROFILE%\Documents\IISExpress\config\applicationhost.config`
2. Tìm section `<sites>`
3. Thêm/s?a timeout:
```xml
<site name="ValiModern" id="1">
    <application path="/">
        <virtualDirectory path="/" physicalPath="..." />
    </application>
    <bindings>
    <binding protocol="http" bindingInformation="*:44300:localhost" />
    </bindings>
    <!-- Thêm dòng này -->
    <limits connectionTimeout="00:10:00" />
</site>
```

### ? Cách 3: Test nhanh h?n
Làm các b??c VNPay **nhanh trong 2-3 phút**:
1. Checkout ? Ch?n VNPay ? Submit
2. Ch?n bank NCB ? Nh?p th? test ? **Không ch?n ch?**
3. Nh?p OTP ? Xác nh?n ngay

### ? Cách 4: Copy URL ?? test sau (Workaround)

Khi VNPay redirect v? (tr??c khi l?i), **copy toàn b? URL** t? address bar:
```
http://localhost:44300/Checkout/VnPayReturn?vnp_Amount=50000000&vnp_BankCode=NCB&vnp_BankTranNo=VNP19254613&vnp_CardType=ATM&vnp_OrderInfo=Thanh+toan+don+hang...&vnp_ResponseCode=00&vnp_SecureHash=...
```

Sau ?ó:
1. **F5** ho?c **Ctrl+F5** ?? start l?i server
2. **Paste URL** vào browser
3. Enter ? Code s? x? lý nh? bình th??ng

## Test Flow v?i Ctrl+F5

```
1. Ctrl+F5 (Run without debugging)
2. Login ? Add to cart ? Checkout
3. Ch?n VNPay ? Submit
4. T?i VNPay:
   - Ch?n bank NCB
   - S? th?: 9704198526191432198
   - Tên: NGUYEN VAN A
   - Ngày: 07/15
 - OTP: 123456
5. Xác nh?n ? ? Redirect v? thành công
6. Xem Confirmation page
```

## Verify thành công

Sau khi VNPay return v?, check database:

```sql
-- Check order
SELECT TOP 1 * FROM Orders ORDER BY id DESC

-- Check payment
SELECT TOP 1 * FROM Payments ORDER BY id DESC
```

K?t qu? mong ??i:
- Order.status = "Processing"
- Payment.status = "Completed"
- Payment.transaction_id = VNP transaction number

## Advanced: S? d?ng ngrok (Optional)

N?u c?n test v?i URL public (không b? timeout):

```bash
# 1. Install ngrok
choco install ngrok

# 2. Run ngrok
ngrok http 44300

# 3. Copy HTTPS URL (ví d?: https://abc123.ngrok.io)

# 4. Update Web.config
<add key="vnp_ReturnUrl" value="https://abc123.ngrok.io/Checkout/VnPayReturn" />

# 5. Test VNPay ? URL s? luôn available
```

## Tóm t?t

**QUICK FIX:**
1. **Stop** server (Shift+F5)
2. **Ctrl+F5** (không ph?i F5!)
3. Test l?i t? ??u

Server s? ch?y mãi cho ??n khi b?n t? t?t!

---

**Updated:** 2024
**Status:** ? Tested & Working
