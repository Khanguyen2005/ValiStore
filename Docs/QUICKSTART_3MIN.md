# ? 3-MINUTE QUICKSTART

## ?? Packages ?ã có, CH? C?N restore references!

### **Step 1: M? Package Manager Console**
```
Tools ? NuGet Package Manager ? Package Manager Console
```

### **Step 2: Copy/Paste (t?ng dòng):**

```powershell
Update-Package Microsoft.AspNet.SignalR -Reinstall -IgnoreDependencies
Update-Package Microsoft.Owin -Reinstall -IgnoreDependencies
Update-Package Microsoft.Owin.Host.SystemWeb -Reinstall -IgnoreDependencies
```

?? ??i 30 giây...

### **Step 3: Rebuild**
```
Ctrl + Shift + B
```

### **Step 4: Run**
```
F5
```

### **Step 5: Test Chat**
1. Login as **Shipper**
2. Go to any order ? Click **"Chat Now"**
3. Send a message
4. **Expect:** Message appears **INSTANTLY** ?

---

## ? Done!

Chat is now **REALTIME** - no more lag! ??

---

## ?? Need Help?

- **Fix references:** `Docs/QUICKFIX_SignalR_References.md`
- **Full guide:** `Docs/SignalRSetupGuide.md`
- **Architecture:** `Docs/SignalRRealtimeChatUpgrade.md`

---

**?? If build still fails after reinstall:**

1. Close Visual Studio
2. Delete `bin` and `obj` folders
3. Reopen Visual Studio
4. Rebuild (Ctrl+Shift+B)
