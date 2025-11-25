# ?? SignalR Setup & Installation Guide

## ?? Cài ??t NuGet Packages

### **Cách 1: Package Manager Console (Khuy?n ngh?)**

M? **Tools ? NuGet Package Manager ? Package Manager Console** và ch?y:

```powershell
# Restore all packages
Update-Package -reinstall

# Or install specific SignalR packages
Install-Package Microsoft.AspNet.SignalR -Version 2.4.3
Install-Package Microsoft.Owin.Host.SystemWeb -Version 4.2.2
```

### **Cách 2: Visual Studio GUI**

1. Right-click d? án ? **Manage NuGet Packages**
2. Click **Restore** (n?u có warning)
3. Search và install:
   - `Microsoft.AspNet.SignalR` (2.4.3)
   - `Microsoft.Owin.Host.SystemWeb` (4.2.2)

### **Cách 3: Build Solution**

Visual Studio s? t? ??ng restore packages khi build:
- **Build ? Rebuild Solution** (Ctrl+Shift+B)

---

## ? Ki?m Tra Packages ?ã Cài

Sau khi restore, ki?m tra các file sau:

### **1. References trong Solution Explorer**

```
References/
??? Microsoft.AspNet.SignalR.Core
??? Microsoft.AspNet.SignalR.SystemWeb
??? Microsoft.Owin
??? Microsoft.Owin.Host.SystemWeb
??? Microsoft.Owin.Security
??? Owin
```

### **2. Scripts folder**

```
Scripts/
??? jquery.signalR-2.4.3.js
??? jquery.signalR-2.4.3.min.js
??? ... (other scripts)
```

### **3. packages folder**

```
packages/
??? Microsoft.AspNet.SignalR.2.4.3/
??? Microsoft.AspNet.SignalR.Core.2.4.3/
??? Microsoft.AspNet.SignalR.JS.2.4.3/
??? Microsoft.AspNet.SignalR.SystemWeb.2.4.3/
??? Microsoft.Owin.4.2.2/
??? Microsoft.Owin.Host.SystemWeb.4.2.2/
??? Microsoft.Owin.Security.4.2.2/
??? Owin.1.0/
```

---

## ?? Ch?y ?ng D?ng

### **1. Build Solution**

```
Build ? Rebuild Solution (Ctrl+Shift+B)
```

Ki?m tra **Error List** - ph?i không có l?i!

### **2. Start Debugging**

```
Debug ? Start Debugging (F5)
```

ho?c

```
Debug ? Start Without Debugging (Ctrl+F5)
```

---

## ?? Test SignalR Chat

### **Test 1: Shipper Chat**

1. Login as **Shipper** (role: shipper)
2. Go to **Shipper Dashboard** (`/Shipper`)
3. Click on any order in "Assigned Orders"
4. Click **"Chat Now"** button
5. M? **Browser Console** (F12)
6. Check for:
   ```
   [SignalR] Connected to ChatHub
   [SignalR] Joined order chat: X
   ```
7. Type a message and send
8. Message should appear **instantly** without refresh

### **Test 2: Customer Chat**

1. Login as **Customer** (normal user)
2. Go to **My Orders** (`/Order`)
3. Click on an order with status **"Shipped"**
4. Click **"Chat with Shipper"** button
5. Check console for SignalR connection
6. Send a message
7. Message should appear instantly

### **Test 3: Realtime Messaging**

1. Open **2 browser windows/tabs**:
   - Window 1: Customer viewing order
   - Window 2: Shipper viewing same order
2. Both open chat modal
3. Customer sends message ? Shipper receives **instantly**
4. Shipper sends message ? Customer receives **instantly**
5. No delay, no refresh needed!

### **Test 4: Auto-Reconnect**

1. Open chat
2. Disable internet connection for 10 seconds
3. Re-enable connection
4. Check console:
   ```
   [SignalR] Disconnected. Attempting to reconnect...
   [SignalR] Reconnected
   [SignalR] Joined order chat: X
   ```
5. Send message - should work again

---

## ?? Troubleshooting

### **Problem: Build Errors "CS0234: The type or namespace name 'Owin' does not exist"**

**Solution:**
```powershell
# Package Manager Console
Update-Package -reinstall -ProjectName ValiModern
```

### **Problem: SignalR Hub script not found (`/signalr/hubs`)**

**Solution:**
1. Check `Startup.cs` exists with `OwinStartup` attribute
2. Make sure `app.MapSignalR()` is called
3. Rebuild solution
4. Clear browser cache (Ctrl+F5)

### **Problem: SignalR not connecting in browser**

**Check Console:**
```javascript
// Should see:
[SignalR] Connected to ChatHub
Connection ID: xxxxx
```

**If not:**
1. Check user is logged in (`USER_ID` defined)
2. Verify SignalR scripts loaded:
   ```html
   <script src="/Scripts/jquery.signalR-2.4.3.min.js"></script>
   <script src="/signalr/hubs"></script>
   ```
3. Check browser console for errors
4. Verify `web.config` has:
   ```xml
   <modules runAllManagedModulesForAllRequests="true"/>
   ```

### **Problem: Messages not appearing**

**Debug Steps:**
1. Open browser console
2. Check SignalR state:
   ```javascript
   ChatSignalR.getState() // Should be 'connected'
   ```
3. Verify joined room:
   ```javascript
   // Should see in console:
   [SignalR] Joined order chat: 123
   ```
4. Check callback registered:
   ```javascript
   window.onChatMessageReceived
   // Should be a function, not undefined
   ```
5. Check server logs (Output window in Visual Studio)

### **Problem: "Cannot read property 'chatHub' of undefined"**

**Solution:**
SignalR scripts not loaded properly. Make sure order is:
```html
<script src="~/Scripts/jquery-3.7.0.js"></script>
<script src="~/Scripts/jquery.signalR-2.4.3.js"></script>
<script src="~/signalr/hubs"></script>
<script src="~/Scripts/chat-signalr.js"></script>
```

---

## ?? Performance Testing

### **Test Latency:**

```javascript
// Customer sends message
var startTime = Date.now();

// Shipper receives message
var endTime = Date.now();
var latency = endTime - startTime;

console.log('Message latency:', latency, 'ms');
// Should be < 100ms on local network
// Should be < 500ms on internet
```

### **Test Multiple Users:**

1. Open 10+ browser tabs (different users)
2. All join same order chat
3. Send messages from multiple tabs
4. All should receive instantly
5. Check server CPU usage (should be low)

### **Load Test:**

```javascript
// Send 100 messages rapidly
for (var i = 0; i < 100; i++) {
    ChatSignalR.sendMessage(orderId, 'Test message ' + i);
}
// Should handle without errors or delays
```

---

## ?? Security Testing

### **Test 1: Unauthorized Access**

1. Login as Customer A
2. Try to join Customer B's order:
   ```javascript
   ChatSignalR.joinOrderChat(otherUsersOrderId);
   ```
3. **Expected:** No error, but no messages received (silently blocked)

### **Test 2: XSS Attack**

1. Send message with script:
   ```
   <script>alert('XSS')</script>
   ```
2. **Expected:** Script displayed as text, not executed

### **Test 3: SQL Injection**

1. Send message with SQL:
   ```
   '; DROP TABLE Messages; --
   ```
2. **Expected:** Message saved as-is, no SQL execution

---

## ?? Checklist

### **Installation:**
- [ ] NuGet packages restored successfully
- [ ] No build errors
- [ ] SignalR scripts present in `/Scripts/`
- [ ] `Startup.cs` registered with `OwinStartup` attribute
- [ ] Solution builds successfully (Ctrl+Shift+B)

### **Configuration:**
- [ ] `BundleConfig.cs` has SignalR bundle
- [ ] `ChatHub.cs` exists in `/Hubs/`
- [ ] `chat-signalr.js` exists in `/Scripts/`
- [ ] `customer-chat.js` exists in `/Scripts/`
- [ ] `shipper_details.js` updated with SignalR

### **Views:**
- [ ] `Views/Shipper/Details.cshtml` includes SignalR scripts
- [ ] `Views/Order/Details.cshtml` includes SignalR scripts
- [ ] `USER_ID` variable defined in both pages

### **Testing:**
- [ ] Customer can send message to shipper
- [ ] Shipper can send message to customer
- [ ] Messages appear instantly (<1 second)
- [ ] Auto-reconnect works after network loss
- [ ] Unauthorized users blocked from other orders
- [ ] XSS protection working
- [ ] No console errors

### **Deployment:**
- [ ] IIS has WebSocket Protocol enabled (for production)
- [ ] `web.config` configured properly
- [ ] SignalR connection works on production
- [ ] Load tested with multiple users

---

## ?? Next Steps

### **1. Learn SignalR:**
- Read official docs: https://learn.microsoft.com/en-us/aspnet/signalr/
- Understand Hub lifecycle
- Learn about Groups and Clients API

### **2. Extend Features:**
- Add typing indicators
- Add read receipts
- Add file/image upload
- Add emoji support
- Add notification sounds

### **3. Monitor Performance:**
- Add logging to ChatHub
- Monitor SignalR connection metrics
- Track message delivery times
- Set up alerts for connection failures

### **4. Scale if Needed:**
- Use Redis backplane for multiple servers
- Enable Application Insights
- Optimize database queries
- Consider Azure SignalR Service for enterprise scale

---

## ?? Support

**Common Issues:**
- Check `Docs/SignalRRealtimeChatUpgrade.md` for detailed architecture
- Review browser console for client-side errors
- Check Visual Studio Output window for server-side logs
- Enable SignalR tracing for detailed debugging

**Still Having Issues?**
1. Clear browser cache (Ctrl+Shift+Del)
2. Restart Visual Studio
3. Clean solution (Build ? Clean Solution)
4. Rebuild solution (Build ? Rebuild Solution)
5. Restart IIS Express

---

**Good Luck! ??**

Your chat is now **realtime** with SignalR - no more lag or delay!

Test it thoroughly and enjoy instant messaging! ???
