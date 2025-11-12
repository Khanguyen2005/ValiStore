# Migration: Change Money Columns from DECIMAL to INT

## ?? Objective
Convert all money-related columns from `DECIMAL(12,2)` to `INT` (or `BIGINT` for large totals) to simplify VND currency handling. Display formatted amounts (e.g., `1.000.000 ?`) while storing clean integers.

---

## ?? Changes Made

### 1. Database Migration
**File:** `Database/Migration_ChangeMoneyColumnsToInt.sql`

**Changes:**
- `Products.price`: DECIMAL(12,2) ? **INT**
- `Products.original_price`: DECIMAL(12,2) ? **INT**
- `Orders.total_amount`: DECIMAL(12,2) ? **BIGINT** (for safety with large totals)
- `Order_Details.price`: DECIMAL(12,2) ? **INT**
- `Payments.amount`: DECIMAL(12,2) ? **BIGINT**

**Run this script in SQL Server Management Studio:**
```sql
-- Make sure you're in the correct database
USE [ValiModern];
GO

-- Execute the migration script
-- (content in Database/Migration_ChangeMoneyColumnsToInt.sql)
```

---

### 2. Entity Models Updated
Changed all money properties from `decimal` to `int` or `long`:

#### Models/EF/Product.cs
```csharp
public int original_price { get; set; }  // was decimal
public int price { get; set; }    // was decimal
```

#### Models/EF/Order.cs
```csharp
public long total_amount { get; set; }    // was decimal, now long (BIGINT)
```

#### Models/EF/Order_Details.cs
```csharp
public int price { get; set; }            // was decimal
```

#### Models/EF/Payment.cs
```csharp
public long amount { get; set; }       // was decimal, now long (BIGINT)
```

---

### 3. ViewModels Updated

#### Models/ViewModels/AdminProductViewModels.cs
```csharp
public int OriginalPrice { get; set; }    // was decimal
public int Price { get; set; }       // was decimal
```

---

### 4. Controllers Updated

#### Areas/Admin/Controllers/ProductController.cs
- Cast `TryParseDecimal` results to `int`:
  ```csharp
  if (TryParseDecimal("Price", out var parsedPrice)) { 
   vm.Price = (int)parsedPrice; 
  }
  ```

#### Controllers/CheckoutController.cs
- Cast amounts when creating orders:
  ```csharp
  total_amount = (long)totalAmount,
  price = (int)item.Price,
  amount = (long)totalAmount,
  ```

---

### 5. Views Updated with VND Formatting

#### Areas/Admin/Views/Product/Create.cshtml
#### Areas/Admin/Views/Product/Edit.cshtml

**Features:**
- **Display Input:** Shows formatted VND (e.g., `1.000.000`)
- **Hidden Input:** Stores clean integer value for form submission
- **JavaScript:** Handles format on focus/blur and strips non-digits

**Example:**
```html
<div class="col-md-3">
    @Html.LabelFor(m => m.Price)
 <input type="text" class="form-control" id="Price_display" 
           inputmode="numeric" placeholder="0" 
    value="@Model.Price.ToString("N0", new CultureInfo("vi-VN"))" />
    <input type="hidden" name="Price" id="Price" value="@Model.Price" />
    @Html.ValidationMessageFor(m => m.Price)
</div>
```

**JavaScript:**
```javascript
function bindMoneyInput(displayId, hiddenId) {
    const display = document.getElementById(displayId);
  const hidden = document.getElementById(hiddenId);
    const formatter = new Intl.NumberFormat('vi-VN');
    const stripNonDigits = s => (s || '').replace(/[^\d]/g, '');

    display.addEventListener('focus', function () {
     display.value = hidden.value || '';
        display.select();
  });

    display.addEventListener('input', function () {
        hidden.value = stripNonDigits(display.value);
    });

 display.addEventListener('blur', function () {
 const val = parseInt(hidden.value, 10);
        display.value = val > 0 ? formatter.format(val) : '';
    });
}

bindMoneyInput('Price_display', 'Price');
bindMoneyInput('OriginalPrice_display', 'OriginalPrice');
```

---

### 6. Removed DecimalModelBinder

**File:** `Global.asax.cs`

Removed these lines (no longer needed):
```csharp
// Removed:
// ModelBinders.Binders.Add(typeof(decimal), new DecimalModelBinder());
// ModelBinders.Binders.Add(typeof(decimal?), new DecimalModelBinder());
```

The `Binders/DecimalModelBinder.cs` file can be optionally deleted.

---

## ?? How to Apply

### Step 1: Run Database Migration
1. Open SQL Server Management Studio
2. Connect to your database
3. Open and execute: `Database/Migration_ChangeMoneyColumnsToInt.sql`

### Step 2: Verify Code Changes
All code changes are already applied:
- ? Entity models updated
- ? ViewModels updated
- ? Controllers fixed
- ? Views updated with VND formatting
- ? Build successful

### Step 3: Test the Application
1. **Admin Panel ? Products ? Create**
   - Enter price: `100000` ? displays as `100.000`
   - Submit ? stored as integer `100000`

2. **Admin Panel ? Products ? Edit**
   - Load existing product ? displays formatted (e.g., `1.000.000`)
   - Edit and save ? works correctly

3. **Checkout Flow**
   - Add products to cart
   - Complete checkout ? verify order total is correct
   - Check database ? all amounts stored as integers

---

## ?? Data Type Limits

| Column | Old Type | New Type | Max Value | Notes |
|--------|----------|----------|-----------|-------|
| Products.price | DECIMAL(12,2) | INT | ~2.1 billion | ? 2.1 t? VND |
| Products.original_price | DECIMAL(12,2) | INT | ~2.1 billion | ? 2.1 t? VND |
| Orders.total_amount | DECIMAL(12,2) | BIGINT | ~9 quintillion | Safe for any order |
| Order_Details.price | DECIMAL(12,2) | INT | ~2.1 billion | Per-item price |
| Payments.amount | DECIMAL(12,2) | BIGINT | ~9 quintillion | Safe for any payment |

---

## ? Benefits

1. **Simpler Logic:** No decimal point issues with VND
2. **Better Display:** Format as needed (1.000.000 / 1,000,000) without database constraints
3. **Accurate Calculations:** Integer arithmetic is precise
4. **Flexible Formatting:** Can change display format anytime (dots, commas, currency symbol)

---

## ?? Example Values

| Display (View) | Stored (Database) |
|----------------|-------------------|
| 10.000 ? | 10000 |
| 1.000.000 ? | 1000000 |
| 250.000 ? | 250000 |

---

## ?? Notes

- **No decimal values:** All prices are whole numbers (VND has no cents)
- **Focus behavior:** Clicking input shows raw number for easy editing
- **Blur behavior:** Leaving input auto-formats with thousands separator
- **Form submission:** Only clean integer values sent to server
- **Backward compatible:** Works with existing decimal-parsing logic in controllers

---

## ?? Status

? **COMPLETED**
- All code changes applied
- Build successful
- Ready for database migration
- Ready for testing

**Next Step:** Run the SQL migration script!
