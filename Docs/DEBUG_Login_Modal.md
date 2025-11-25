# ?? Login Modal Debug Guide

## ?? Issue
Login modal shows "Please fill in all fields correctly" even when filled.

## ? Fix Applied

### **1. Added Server Logging:**
- `LoginAjax` now logs to Visual Studio Output window
- Check: **View ? Output ? Show output from: Debug**

### **2. Added Client Logging:**
- Browser console shows Ajax request/response
- Check: **F12 ? Console tab**

### **3. Improved Error Messages:**
- Server returns specific ModelState errors
- Client displays actual error message

---

## ?? How to Test & Debug

### **Step 1: Open Browser Console**
1. Press **F12** (or right-click ? Inspect)
2. Click **Console** tab
3. Keep it open while testing

### **Step 2: Try Login**
1. Click **"Login"** button in navbar
2. Enter credentials:
   - **Email:** `thanhcho@gmail.com`
   - **Password:** `123456`
3. Click **"Login"** button in modal
4. **Watch the Console**

### **Step 3: Check Console Output**

#### **Should see:**
```
[Login] Form submitted
[Login] Form data: Email=thanhcho%40gmail.com&Password=123456&RememberMe=on&__RequestVerificationToken=...
[Login] Response: {success: true, redirectUrl: "/Home/Index"}
[Login] Success! Redirecting to: /Home/Index
```

#### **If error, you'll see:**
```
[Login] Response: {success: false, message: "Incorrect email or password."}
[Login] Failed: Incorrect email or password.
```

### **Step 4: Check Visual Studio Output**

**In Visual Studio:**
1. Go to **View ? Output**
2. Select **"Show output from: Debug"**
3. Look for:
```
[LoginAjax] Attempting login for: thanhcho@gmail.com
[LoginAjax] ModelState.IsValid: True
[LoginAjax] Looking for user: thanhcho@gmail.com
[LoginAjax] User found: 123, is_admin: False, role: NULL
[LoginAjax] Login successful, redirecting to: /Home/Index
```

---

## ?? Common Issues & Solutions

### **Issue 1: "Please fill in all fields correctly"**

**Check Console:**
```
[Login] Response: {success: false, message: "Please fill in all fields correctly."}
```

**Cause:** ModelState validation failed

**Debug:**
1. Check VS Output for: `[LoginAjax] Validation errors: ...`
2. Specific error will be shown

**Common causes:**
- Missing anti-forgery token
- Email format invalid
- Password empty

### **Issue 2: "Incorrect email or password"**

**Check Console:**
```
[Login] Response: {success: false, message: "Incorrect email or password."}
```

**Solutions:**
1. **Verify email exists in database:**
   ```sql
   SELECT * FROM Users WHERE email = 'thanhcho@gmail.com'
   ```
2. **Verify password matches:**
   - Password is stored in **plain text** (no hashing)
   - Check exact match (case-sensitive)

3. **Create test user if not exists:**
   ```sql
   INSERT INTO Users (username, email, password, is_admin, created_at, updated_at)
   VALUES ('thanh', 'thanhcho@gmail.com', '123456', 0, GETDATE(), GETDATE())
   ```

### **Issue 3: Ajax Error**

**Check Console:**
```
[Login] Ajax error: error Internal Server Error
[Login] Response: <HTML error page>
```

**Cause:** Server exception

**Debug:**
1. Check VS Output for: `[LoginAjax] Exception: ...`
2. Check exception message and stack trace

**Common causes:**
- Database connection error
- Missing columns in Users table
- Anti-forgery token mismatch

---

## ?? Test Accounts

### **Test with these accounts:**

| Email | Password | Role | Expected Result |
|-------|----------|------|-----------------|
| `thanhcho@gmail.com` | `123456` | Customer | ? Login ? Home |
| `admin@vali.com` | `admin123` | Admin | ? Login ? Admin Dashboard |
| `shipper1@vali.com` | `123456` | Shipper | ? Login ? Shipper Dashboard |
| `wrong@email.com` | `123456` | - | ? Error: "Incorrect email or password" |
| `thanhcho@gmail.com` | `wrongpass` | - | ? Error: "Incorrect email or password" |

---

## ?? Manual SQL Check

### **1. Verify user exists:**
```sql
SELECT id, email, password, is_admin, role
FROM Users
WHERE email = 'thanhcho@gmail.com'
```

**Expected:**
```
id | email                 | password | is_admin | role
---|-----------------------|----------|----------|------
1  | thanhcho@gmail.com    | 123456   | 0        | NULL
```

### **2. If user doesn't exist, create:**
```sql
INSERT INTO Users (username, email, password, phone, is_admin, address, created_at, updated_at)
VALUES ('thanhcho', 'thanhcho@gmail.com', '123456', '', 0, '', GETDATE(), GETDATE())
```

### **3. Verify password is correct:**
```sql
SELECT *
FROM Users
WHERE email = 'thanhcho@gmail.com'
  AND password = '123456'
```

**Should return 1 row.**

---

## ? Quick Checklist

When testing, verify:

- [ ] Browser console open (F12)
- [ ] VS Output window open (View ? Output ? Debug)
- [ ] Modal opens when click "Login"
- [ ] Email and password fields filled
- [ ] Click "Login" button
- [ ] **Check console for logs**
- [ ] **Check VS Output for logs**
- [ ] Error message shown (if any)
- [ ] User exists in database
- [ ] Password matches exactly

---

## ?? Expected Flow

### **Successful Login:**
```
1. User clicks "Login" in navbar
   ? Modal opens

2. User enters email + password
   ? Form fields populated

3. User clicks "Login" button
   ? Console: "[Login] Form submitted"
   ? Button: "Logging in..." (with spinner)

4. Ajax POST to /Account/LoginAjax
   ? VS Output: "[LoginAjax] Attempting login..."
   ? VS Output: "[LoginAjax] User found..."

5. Server returns success
   ? Console: "[Login] Success! Redirecting..."
   ? Browser redirects to home/dashboard

6. User is logged in
   ? Navbar shows user menu
```

### **Failed Login:**
```
1-3. Same as above

4. Ajax POST to /Account/LoginAjax
   ? VS Output: "[LoginAjax] User not found or password incorrect"

5. Server returns error
   ? Console: "[Login] Failed: Incorrect email or password."
   ? Error alert shows in modal
   ? Button: "Login" (enabled again)

6. User can try again
```

---

## ?? Next Steps

### **1. Run App:**
```
Press F5
```

### **2. Test Login:**
1. Open browser console (F12)
2. Click "Login" button
3. Enter: `thanhcho@gmail.com` / `123456`
4. Click "Login"
5. **Watch console & VS Output**

### **3. Share Logs:**

If still error, share:
- **Browser console output** (copy/paste)
- **VS Output window** (Debug section)
- **Screenshot of error**

---

**Debug Tools Ready!** ??

Bây gi? run app (F5) và test login. Console s? cho b?n bi?t chính xác v?n ?? là gì! ??
