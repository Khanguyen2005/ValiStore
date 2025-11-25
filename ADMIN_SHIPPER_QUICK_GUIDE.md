# Admin Shipper Management - Quick Guide

## How to Access Shipper Features in Admin Panel

### ? Method 1: Shippers Management Page (NEW - RECOMMENDED)

1. **Navigate to Shippers**
   - Click **"Shippers"** in the admin sidebar (truck icon ??)
   - URL: `/Admin/ShipperManagement/Index`

2. **What You'll See**
   - **Summary Cards**: Total shippers, completed deliveries, pending, assigned
   - **Shippers Table**: All shippers with performance metrics
   - **Performance Rate**: Visual progress bar showing completion rate
   - **Quick Actions**: View deliveries, Edit shipper

3. **View Individual Shipper Deliveries**
   - Click **"Deliveries"** button on any shipper row
   - Takes you to detailed delivery history for that shipper

### Method 2: Through Users Page

1. **Navigate to Users**
   - Click "Users" in the sidebar
   - URL: `/Admin/User/Index`

2. **Find Shippers**
   - Look for users with **blue "Shipper" badge** (has truck icon ??)
   - You'll see a **truck button** next to shipper users

3. **View Deliveries**
   - Click the **truck icon button** (??) next to any shipper
   - This will take you to: `/Admin/Order/ShipperDeliveries/{shipperId}`

### Method 3: Through Order Details

1. **Navigate to Orders**
   - Click "Orders" in sidebar
   - URL: `/Admin/Order/Index`

2. **Open an Order**
   - Click "View" on any order

3. **If Order Has Shipper Assigned**
   - Scroll to "Shipper Information" section
   - Click **"View All Deliveries"** button
   - This shows all orders for that shipper

## New Features in Shippers Management Page

### Summary Dashboard
Shows 4 key metrics:
- ?? **Total Shippers** - Number of delivery personnel
- ? **Total Completed** - Sum of all completed deliveries
- ?? **Pending Deliveries** - Orders awaiting delivery
- ?? **Total Assigned** - All assigned orders across shippers

### Shippers Performance Table
Each shipper row shows:
- **Shipper Info**: Name, ID, avatar
- **Contact**: Email, phone
- **Total Assigned**: All orders ever assigned
- **Pending**: Orders not yet delivered
- **Delivered (Awaiting)**: Delivered but not confirmed
- **Completed**: Successfully completed orders
- **Success Rate**: Visual progress bar (green ?80%, yellow ?50%, red <50%)
- **Actions**: 
  - ?? **Deliveries** button - View all deliveries
  - ?? **Edit** button - Edit shipper info

## What You'll See on Shipper Deliveries Page

### Shipper Information Card
```
Name: Nguy?n V?n Giao
Email: shipper1@valimodern.com
Phone: 0901234567
Shipper ID: #123
```

### Statistics Dashboard (4 Cards)
1. **Total Assigned** (Blue) - All orders assigned to this shipper
2. **Pending Delivery** (Yellow) - Orders not yet delivered
3. **Delivered (Awaiting Confirm)** (Light Blue) - Delivered but not completed
4. **Completed** (Green) - Fully completed orders

### Delivery List Table
Shows all orders with:
- Order ID & Customer info
- Order Date & Assigned Date
- Delivered Date (if applicable)
- **Delivery Duration** (with fast delivery badges ?)
- Amount & Status
- Quick "View" button to see order details

### Example Navigation Flow

```
Admin Dashboard
    ??> Shippers (NEW! ?)
    ?   ??> Summary Cards
    ?   ??> All Shippers Table
    ?   ??> Click "Deliveries"
    ?       ??> Shipper Deliveries Page
    ?
    ??> Users
    ?   ??> Find Shipper (has truck icon)
    ?       ??> Click Truck Button ??
    ?           ??> Shipper Deliveries Page
    ?
    ??> Orders
        ??> Order Details
            ??> "View All Deliveries" button
                ??> Shipper Deliveries Page

Shipper Deliveries Page
    ??> Statistics Cards
    ??> Delivery List
    ??> Click "View" on any order
        ??> Order Details
            ??> "View All Deliveries" button
                ??> Back to Shipper Deliveries
```

## Visual Indicators

### In Admin Sidebar
- ?? **Shippers** - NEW dedicated page for shipper management

### In Shippers Management Page
- ?? **Green Progress** - Success rate ?80%
- ?? **Yellow Progress** - Success rate 50-79%
- ?? **Red Progress** - Success rate <50%

### In Users List
- ? **Admin**: Blue badge with shield icon
- ? **Shipper**: Light blue badge with truck icon + **Truck Button** ??
- ? **Customer**: Gray badge with person icon

### In Shipper Deliveries
- ? **Fast Delivery**: Green badge for <24h deliveries
- ?? **Completed**: Green status badge
- ?? **Shipped**: Blue status badge
- ?? **Pending**: Yellow/warning badge

## Quick Access URLs

Assuming shipper ID is `5`:

```
Shippers Management (NEW):
/Admin/ShipperManagement/Index

View Shipper Deliveries:
/Admin/Order/ShipperDeliveries/5

Users Page:
/Admin/User/Index

Orders Page:
/Admin/Order/Index
```

## Testing the Feature

1. **View All Shippers** (NEW)
   - Go to Shippers in sidebar
   - See all shippers with performance metrics
   - Click "Deliveries" to view individual shipper

2. **Create a Shipper User** (if you don't have one)
   - Click "Add New Shipper" button on Shippers page
   - OR: Go to Users ? Create New
   - Set role to "shipper"

3. **Assign Order to Shipper**
   - Go to Orders ? Select an order with status "Confirmed"
   - In Order Details, use "Assign Shipper" dropdown
   - Select the shipper and click "Assign Shipper"

4. **View Shipper's Deliveries**
   - **Option A**: Go to Shippers ? Click "Deliveries" button
   - **Option B**: Go to Users ? Click truck button ??
   - You should see the assigned order in the list

## Features Comparison

| Feature | Shippers Page | Users Page | Order Details |
|---------|--------------|------------|---------------|
| See all shippers | ? | ? | ? |
| Performance metrics | ? | ? | ? |
| Success rate | ? | ? | ? |
| Quick access to deliveries | ? | ? | ? |
| Edit shipper | ? | ? | ? |
| Add new shipper | ? | ? | ? |

## Features in Shippers Management Page

? **Summary Dashboard** - Quick overview of all shippers  
? **Performance Metrics** - Completion rate, pending, completed  
? **Visual Progress Bars** - Easy-to-read success rate  
? **Quick Actions** - View deliveries, edit info  
? **Sortable Stats** - See top performers  
? **One-Click Access** - Direct link to deliveries  

## Features in Shipper Deliveries Page

? **Shipper Info Display** - Name, email, phone, ID  
? **Performance Statistics** - 4 metric cards  
? **Paginated List** - 20 orders per page  
? **Delivery Duration** - Calculates time from assigned to delivered  
? **Fast Delivery Badges** - Highlights quick deliveries (<24h)  
? **Status Filters** - See orders by status  
? **Navigation** - Links back to order details  

## Troubleshooting

**Q: I don't see "Shippers" in sidebar**  
A: Make sure you rebuild the project. The link should appear between "Orders" and "Users".

**Q: I don't see the truck button in Users page**  
A: Make sure the user has `role = "shipper"` in the database.

**Q: Shipper Deliveries page shows "No orders assigned"**  
A: Assign some orders to this shipper first from Order Details page.

**Q: Can't access ShipperManagement or ShipperDeliveries**  
A: Make sure you're logged in as Admin.

**Q: Performance rate shows 0%**  
A: This is normal if the shipper hasn't completed any deliveries yet.

---

## Related Documentation
- `DOCS_SHIPPER_DELIVERY_HISTORY.md` - Full feature documentation
- `DOCS_SHIPPER_FEATURE.md` - Original shipper feature docs
- `SUMMARY_SHIPPER_DELIVERY_HISTORY.md` - Quick summary
