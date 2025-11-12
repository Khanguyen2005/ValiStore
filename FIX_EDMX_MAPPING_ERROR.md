## ?? LÀM TH? CÔNG - Update Entity Framework Model

File `.edmx` không th? t? ??ng s?a b?ng script. B?n c?n làm theo các b??c sau:

### Cách 1: Update Model from Database (KHUY?N NGH?)

1. **Ch?y SQL Migration tr??c:**
   - Execute file `Database/Migration_ChangeMoneyColumnsToInt.sql` trong SSMS
   - ??m b?o t?t c? columns ?ã ??i sang INT/BIGINT

2. **Trong Visual Studio:**
   - M? file `Models/EF/ValiModernModel.edmx` (double-click)
   - Click chu?t ph?i vào vùng tr?ng trong EF Designer
   - Ch?n **"Update Model from Database..."**
   - Tab **Refresh**: Check t?t c? tables có liên quan:
     - ? Products
   - ? Orders
     - ? Order_Details
     - ? Payments
   - Click **Finish**

3. **Save và Build:**
   - Save file `.edmx` (Ctrl+S)
   - Build l?i project (Ctrl+Shift+B)

---

### Cách 2: S?a th? công file .edmx (KHÔNG KHUY?N NGH?)

N?u "Update Model from Database" không ho?t ??ng, b?n có th? edit file XML tr?c ti?p, nh?ng **r?t d? l?i**.

Tìm và thay th? trong file `ValiModernModel.edmx`:

#### A. Trong `<edmx:StorageModels>`:
```xml
<!-- Tìm: -->
<Property Name="price" Type="decimal" Precision="12" Scale="2" Nullable="false" />
<Property Name="original_price" Type="decimal" Precision="12" Scale="2" Nullable="false" />

<!-- ??i thành: -->
<Property Name="price" Type="int" Nullable="false" />
<Property Name="original_price" Type="int" Nullable="false" />
```

```xml
<!-- Tìm: -->
<Property Name="total_amount" Type="decimal" Precision="12" Scale="2" Nullable="false" />

<!-- ??i thành: -->
<Property Name="total_amount" Type="bigint" Nullable="false" />
```

```xml
<!-- Order_Details.price -->
<Property Name="price" Type="decimal" Precision="12" Scale="2" Nullable="false" />
<!-- ??i: -->
<Property Name="price" Type="int" Nullable="false" />
```

```xml
<!-- Payments.amount -->
<Property Name="amount" Type="decimal" Precision="12" Scale="2" Nullable="false" />
<!-- ??i: -->
<Property Name="amount" Type="bigint" Nullable="false" />
```

#### B. Trong `<edmx:ConceptualModels>`:
```xml
<!-- Tìm: -->
<Property Name="original_price" Type="Decimal" Precision="12" Scale="2" Nullable="false" />
<Property Name="price" Type="Decimal" Precision="12" Scale="2" Nullable="false" />

<!-- ??i thành: -->
<Property Name="original_price" Type="Int32" Nullable="false" />
<Property Name="price" Type="Int32" Nullable="false" />
```

```xml
<!-- Orders.total_amount -->
<Property Name="total_amount" Type="Decimal" Precision="12" Scale="2" Nullable="false" />
<!-- ??i: -->
<Property Name="total_amount" Type="Int64" Nullable="false" />
```

```xml
<!-- Order_Details.price -->
<Property Name="price" Type="Decimal" Precision="12" Scale="2" Nullable="false" />
<!-- ??i: -->
<Property Name="price" Type="Int32" Nullable="false" />
```

```xml
<!-- Payments.amount -->
<Property Name="amount" Type="Decimal" Precision="12" Scale="2" Nullable="false" />
<!-- ??i: -->
<Property Name="amount" Type="Int64" Nullable="false" />
```

---

### ? Kh?c ph?c nhanh (n?u không mu?n s?a .edmx):

**T?m th?i revert code v? DECIMAL:**

N?u b?n không mu?n ??ng vào `.edmx` ngay bây gi?, có th? t?m revert các class entity v? `decimal` cho kh?p v?i `.edmx` hi?n t?i, sau ?ó fix sau:

```csharp
// Product.cs
public decimal original_price { get; set; }
public decimal price { get; set; }

// Order.cs
public decimal total_amount { get; set; }

// Order_Details.cs
public decimal price { get; set; }

// Payment.cs
public decimal amount { get; set; }
```

Nh?ng ?i?u này s? m?t ?i vi?c chuy?n sang INT, nên **KHÔNG khuy?n ngh?**.

---

### ? Gi?i pháp t?t nh?t:

**Ch?y "Update Model from Database"** sau khi execute SQL migration script. ?ây là cách an toàn và t? ??ng nh?t.
