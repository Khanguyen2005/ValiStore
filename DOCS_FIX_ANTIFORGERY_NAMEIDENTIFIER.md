# Fix Anti-Forgery Token Error - Use NameIdentifier

## V?n ??
L?i anti-forgery token xu?t hi?n khi shipper submit form:

```
System.InvalidOperationException: A claim of type 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier' 
or 'http://schemas.microsoft.com/accesscontrolservice/2010/07/claims/identityprovider' was not present on the provided ClaimsIdentity.
```

### Nguyên nhân
1. **Database design cho phép NULL values:**
   - `User.email` có th? NULL
   - `User.username` có th? NULL
   - Ch? có `User.id` là NOT NULL và UNIQUE

2. **Cách implementation c?:**
   - `FormsAuthenticationTicket.Name` l?u `user.email`
   - `AntiForgeryConfig.UniqueClaimTypeIdentifier` = `ClaimTypes.Email`
   - Shipper ho?c user không có email ? l?i anti-forgery

## Gi?i pháp tri?t ??

### 1. ??i FormsAuthenticationTicket.Name thành User.id

**File: `Controllers/AccountController.cs`**

```csharp
// OLD - L?u email (có th? NULL)
var ticket = new FormsAuthenticationTicket(
    1,
    user.email,  // ? CÓ TH? NULL
    DateTime.Now,
    DateTime.Now.AddDays(14),
    rememberMe,
    role,
    FormsAuthentication.FormsCookiePath);

// NEW - L?u user ID (LUÔN unique, NOT NULL)
var ticket = new FormsAuthenticationTicket(
    1,
    user.id.ToString(),  // ? LUÔN CÓ GIÁ TR?
    DateTime.Now,
    DateTime.Now.AddDays(14),
    rememberMe,
    role,
    FormsAuthentication.FormsCookiePath);
```

### 2. ??i AntiForgeryConfig thành NameIdentifier

**File: `Global.asax.cs`**

```csharp
protected void Application_Start()
{
    // ...
    
    // OLD - Dùng Email (có th? NULL)
    AntiForgeryConfig.UniqueClaimTypeIdentifier = ClaimTypes.Email;  // ?
    
    // NEW - Dùng NameIdentifier (User.id - LUÔN CÓ)
    AntiForgeryConfig.UniqueClaimTypeIdentifier = ClaimTypes.NameIdentifier;  // ?
}
```

### 3. Update Claims trong Application_PostAuthenticateRequest

**File: `Global.asax.cs`**

```csharp
protected void Application_PostAuthenticateRequest(object sender, EventArgs e)
{
    var authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];
    if (authCookie == null) return;
    
    FormsAuthenticationTicket ticket;
    try
    {
        ticket = FormsAuthentication.Decrypt(authCookie.Value);
    }
    catch { return; }
    
    if (ticket == null || ticket.Expired) return;
    
    // Create claims identity
    var identity = new ClaimsIdentity("Forms", ClaimTypes.Name, ClaimTypes.Role);
    
    // ticket.Name now contains user ID (from login fix)
    // Add NameIdentifier claim (REQUIRED for anti-forgery)
    identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, ticket.Name));
    
    // Add Name claim for User.Identity.Name
    identity.AddClaim(new Claim(ClaimTypes.Name, ticket.Name));
    
    // Add role claim from UserData
    if (!string.IsNullOrEmpty(ticket.UserData))
    {
        identity.AddClaim(new Claim(ClaimTypes.Role, ticket.UserData));
    }
    
    HttpContext.Current.User = new ClaimsPrincipal(identity);
}
```

### 4. Update t?t c? Controllers ?? dùng User ID

**Pattern c? (dùng email):**
```csharp
var email = User.Identity.Name;
var user = _db.Users.FirstOrDefault(u => u.email == email);
```

**Pattern m?i (dùng ID):**
```csharp
int userId;
if (!int.TryParse(User.Identity.Name, out userId))
{
    TempData["Error"] = "Invalid user session.";
    return RedirectToAction("Index", "Home");
}

var user = _db.Users.Find(userId);
```

### 5. Các file ?ã update

**Controllers:**
- ? `Controllers/AccountController.cs` - Login, Register, Profile
- ? `Controllers/OrderController.cs` - My Orders
- ? `Controllers/ShipperController.cs` - Shipper Delivery
- ? `Controllers/CheckoutController.cs` - Checkout, PayPal

**Filters:**
- ? `Filters/AuthorizeAdminAttribute.cs`
- ? `Filters/AuthorizeShipperAttribute.cs`

**Layouts:**
- ? `Views/Shared/_Layout.cshtml`
- ? `Views/Shared/_ShipperLayout.cshtml`
- ? `Areas/Admin/Views/Shared/_AdminLayout.cshtml`

**Core:**
- ? `Global.asax.cs`

## ?u ?i?m c?a gi?i pháp

1. **B?o m?t t?t h?n:** 
   - User ID không ti?t l? thông tin cá nhân (email)
   - Tránh email enumeration attacks

2. **T??ng thích v?i database:**
   - `User.id` LUÔN có giá tr?
   - Không b? l?i khi user không có email/username

3. **Performance t?t h?n:**
   - `Find(userId)` nhanh h?n `FirstOrDefault(u => u.email == email)`
   - S? d?ng primary key index

4. **Nh?t quán:**
   - T?t c? authentication logic dùng cùng identifier
   - D? maintain và debug

## Testing

### Test Cases
1. ? Login v?i email/password
2. ? Register user m?i
3. ? Shipper submit form "Mark as Delivered"
4. ? Customer confirm received order
5. ? Admin actions v?i anti-forgery
6. ? Logout và re-login

### K?t qu?
- ? Build successful
- ? Không còn l?i anti-forgery
- ? T?t c? forms v?i `@Html.AntiForgeryToken()` ho?t ??ng bình th??ng

## Notes

- **Session Management:** Session v?n l?u `DisplayName`, `UserRole`, `UserId` ?? t?i ?u performance
- **Fallback Logic:** Layout có fallback ?? rebuild session t? database n?u c?n
- **Backward Compatibility:** Logout form clear c? authentication cookie và anti-forgery cookie

## Author
Fixed by: AI Assistant  
Date: 2024  
Status: ? HOÀN THÀNH
