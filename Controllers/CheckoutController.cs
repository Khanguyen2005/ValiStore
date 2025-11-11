using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ValiModern.Helpers;
using ValiModern.Models.EF;
using ValiModern.Models.ViewModels;

namespace ValiModern.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly ValiModernDBEntities _db = new ValiModernDBEntities();
        private const string CartSessionKey = "ShoppingCart";

        private List<CartItem> GetCart()
        {
            var cart = Session[CartSessionKey] as List<CartItem>;
            if (cart == null)
            {
                cart = new List<CartItem>();
                Session[CartSessionKey] = cart;
            }
            return cart;
        }

        // GET: Checkout
        [Authorize]
        public ActionResult Index()
        {
            var cart = GetCart();
            if (cart == null || !cart.Any())
            {
                TempData["Error"] = "Your cart is empty.";
                return RedirectToAction("Index", "Cart");
            }

            var vm = new CheckoutVM
            {
                Items = cart
            };

            // Pre-fill user info if logged in
            if (User.Identity.IsAuthenticated)
            {
                var email = User.Identity.Name;
                var user = _db.Users.FirstOrDefault(u => u.email == email);
                if (user != null)
                {
                    vm.FullName = user.username;
                    vm.Phone = user.phone;
                    vm.Address = user.address;
                    vm.Email = user.email;
                }
            }

            return View(vm);
        }

        // POST: Checkout/ProcessOrder
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult ProcessOrder(CheckoutVM model)
        {
            var cart = GetCart();
            if (cart == null || !cart.Any())
            {
                TempData["Error"] = "Your cart is empty.";
                return RedirectToAction("Index", "Cart");
            }

            // Validate model
            if (string.IsNullOrWhiteSpace(model.FullName))
                ModelState.AddModelError("FullName", "Full name is required.");
            if (string.IsNullOrWhiteSpace(model.Phone))
                ModelState.AddModelError("Phone", "Phone is required.");
            if (string.IsNullOrWhiteSpace(model.Address))
                ModelState.AddModelError("Address", "Address is required.");
            if (string.IsNullOrWhiteSpace(model.PaymentMethod))
                ModelState.AddModelError("PaymentMethod", "Please select a payment method.");

            if (!ModelState.IsValid)
            {
                model.Items = cart;
                return View("Index", model);
            }

            // Get user
            var email = User.Identity.Name;
            var user = _db.Users.FirstOrDefault(u => u.email == email);
            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction("Index");
            }

            // Calculate total amount
            decimal totalAmount = cart.Sum(i => i.Subtotal);
       
   // Validate total amount
        if (totalAmount <= 0)
            {
                TempData["Error"] = "Invalid order amount. Please check your cart.";
  return RedirectToAction("Index", "Cart");
            }

// Create order
   var order = new Order
  {
          user_id = user.id,
    order_date = DateTime.Now,
       status = model.PaymentMethod == "COD" ? "Pending" : "Pending",
   total_amount = totalAmount,
        phone = model.Phone,
           shipping_address = model.Address,
          created_at = DateTime.Now,
     updated_at = DateTime.Now
    };

          _db.Orders.Add(order);
    _db.SaveChanges();

     // Create order details
      foreach (var item in cart)
            {
      var orderDetail = new Order_Details
     {
       order_id = order.id,
       product_id = item.ProductId,
          quantity = item.Quantity,
  price = item.Price,
     color_id = item.ColorId,
           size_id = item.SizeId,
          created_at = DateTime.Now
        };
    _db.Order_Details.Add(orderDetail);
            }

            // Create payment record
        var payment = new Payment
       {
        order_id = order.id,
             amount = totalAmount,  // Use validated totalAmount
        payment_method = model.PaymentMethod,
     status = model.PaymentMethod == "COD" ? "Pending" : "Pending",
       transaction_id = "",
payment_date = DateTime.Now
            };
            _db.Payments.Add(payment);
      
            try
            {
             _db.SaveChanges();
            }
     catch (Exception ex)
      {
   // Log the error
           System.Diagnostics.Debug.WriteLine($"Error saving payment: {ex.Message}");
    System.Diagnostics.Debug.WriteLine($"Order Amount: {totalAmount}");
        TempData["Error"] = "Error processing payment. Please try again.";
      return RedirectToAction("Index");
        }

   // Clear cart
            Session.Remove(CartSessionKey);

            // Handle payment method
    if (model.PaymentMethod == "COD")
          {
      return RedirectToAction("Confirmation", new { orderId = order.id });
            }
            else if (model.PaymentMethod == "VNPay")
     {
   return RedirectToAction("VnPayPayment", new { orderId = order.id });
     }

            TempData["Error"] = "Invalid payment method.";
       return RedirectToAction("Index");
        }

        // GET: Checkout/VnPayPayment
        [Authorize]
        public ActionResult VnPayPayment(int orderId)
        {
            var order = _db.Orders.Find(orderId);
            if (order == null)
            {
                TempData["Error"] = "Order not found.";
                return RedirectToAction("Index", "Home");
            }

            try
            {
                // VNPay Configuration from Web.config
                string vnp_Returnurl = ConfigurationManager.AppSettings["vnp_ReturnUrl"];
                string vnp_Url = ConfigurationManager.AppSettings["vnp_Url"];
                string vnp_TmnCode = ConfigurationManager.AppSettings["vnp_TmnCode"];
                string vnp_HashSecret = ConfigurationManager.AppSettings["vnp_HashSecret"];

                // Validate config
                if (string.IsNullOrEmpty(vnp_TmnCode) || string.IsNullOrEmpty(vnp_HashSecret))
                {
                    TempData["Error"] = "VNPay configuration is missing. Please contact administrator.";
                    return RedirectToAction("Index", "Home");
                }

                // Build VNPay request
                var vnpay = new VnPayLibrary();

                // Required fields
                vnpay.AddRequestData("vnp_Version", ConfigurationManager.AppSettings["vnp_Version"] ?? "2.1.0");
                vnpay.AddRequestData("vnp_Command", ConfigurationManager.AppSettings["vnp_Command"] ?? "pay");
                vnpay.AddRequestData("vnp_TmnCode", vnp_TmnCode);

                // Amount (must multiply by 100 - VNPay uses smallest currency unit)
                long amount = (long)(order.total_amount * 100);
                vnpay.AddRequestData("vnp_Amount", amount.ToString());

                vnpay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
                vnpay.AddRequestData("vnp_CurrCode", ConfigurationManager.AppSettings["vnp_CurrCode"] ?? "VND");
                vnpay.AddRequestData("vnp_IpAddr", VnPayLibrary.GetIpAddress(Request));
                vnpay.AddRequestData("vnp_Locale", ConfigurationManager.AppSettings["vnp_Locale"] ?? "vn");
                vnpay.AddRequestData("vnp_OrderInfo", "Thanh toan don hang #" + order.id);
                vnpay.AddRequestData("vnp_OrderType", "other");
                vnpay.AddRequestData("vnp_ReturnUrl", vnp_Returnurl);
                vnpay.AddRequestData("vnp_TxnRef", order.id.ToString());

                // Optional: Add bank code if you want direct payment
                // vnpay.AddRequestData("vnp_BankCode", "VNBANK");

                string paymentUrl = vnpay.CreateRequestUrl(vnp_Url, vnp_HashSecret);

                return Redirect(paymentUrl);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error creating VNPay payment: " + ex.Message;
                return RedirectToAction("Index", "Home");
            }
        }

        // GET: Checkout/VnPayReturn
        // Note: No parameters - get everything from QueryString manually to avoid model binding errors
    public ActionResult VnPayReturn()
  {
       System.Diagnostics.Debug.WriteLine("=== VNPay Return Callback START ===");
    
            // Get query string manually (avoid model binding)
var queryString = Request.QueryString;
  
if (queryString.Count == 0 || string.IsNullOrEmpty(queryString["vnp_TxnRef"]))
    {
       System.Diagnostics.Debug.WriteLine("ERROR: No query string or missing vnp_TxnRef!");
    TempData["Error"] = "Invalid payment response from VNPay.";
 return RedirectToAction("Index", "Home");
            }

   try
    {
        string vnp_HashSecret = ConfigurationManager.AppSettings["vnp_HashSecret"];
       
                if (string.IsNullOrEmpty(vnp_HashSecret))
      {
   System.Diagnostics.Debug.WriteLine("ERROR: vnp_HashSecret is missing!");
      TempData["Error"] = "VNPay configuration error.";
   return RedirectToAction("Index", "Home");
    }

            var vnpay = new VnPayLibrary();

    // Add all vnp_ parameters to library
      foreach (string key in queryString.AllKeys)
     {
        if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
       {
         vnpay.AddResponseData(key, queryString[key]);
   System.Diagnostics.Debug.WriteLine($"{key}: {queryString[key]}");
  }
   }

    string orderId = vnpay.GetResponseData("vnp_TxnRef");
      string vnpayTranId = vnpay.GetResponseData("vnp_TransactionNo");
           string vnp_ResponseCode = vnpay.GetResponseData("vnp_ResponseCode");
   string vnp_TransactionStatus = vnpay.GetResponseData("vnp_TransactionStatus");
       string vnp_SecureHash = queryString["vnp_SecureHash"];

   System.Diagnostics.Debug.WriteLine($"Order ID: {orderId}");
  System.Diagnostics.Debug.WriteLine($"Transaction ID: {vnpayTranId}");
       System.Diagnostics.Debug.WriteLine($"Response Code: {vnp_ResponseCode}");
    System.Diagnostics.Debug.WriteLine($"Transaction Status: {vnp_TransactionStatus}");

     // Validate signature
bool checkSignature = vnpay.ValidateSignature(vnp_SecureHash, vnp_HashSecret);
  System.Diagnostics.Debug.WriteLine($"Signature Valid: {checkSignature}");

         if (!checkSignature)
      {
    System.Diagnostics.Debug.WriteLine("ERROR: Invalid signature!");
     TempData["Error"] = "Invalid payment signature. Please contact support.";
return RedirectToAction("Index", "Home");
    }

    // Parse order ID
   int orderIdInt;
   if (!int.TryParse(orderId, out orderIdInt))
    {
   System.Diagnostics.Debug.WriteLine($"ERROR: Invalid order ID: {orderId}");
   TempData["Error"] = "Invalid order reference.";
         return RedirectToAction("Index", "Home");
  }

     // Get order from database
 var order = _db.Orders.Find(orderIdInt);
 if (order == null)
   {
System.Diagnostics.Debug.WriteLine($"ERROR: Order {orderIdInt} not found!");
 TempData["Error"] = "Order not found.";
    return RedirectToAction("Index", "Home");
            }

    System.Diagnostics.Debug.WriteLine($"Order found: ID={order.id}, Amount={order.total_amount}");

          // Check payment result
         if (vnp_ResponseCode == "00" && vnp_TransactionStatus == "00")
     {
      System.Diagnostics.Debug.WriteLine("Payment SUCCESS - Updating database...");
         
    // Update order
   order.status = "Processing";
        order.updated_at = DateTime.Now;

            // Update payment
       var payment = _db.Payments.FirstOrDefault(p => p.order_id == order.id);
  if (payment != null)
    {
        payment.status = "Completed";
payment.transaction_id = vnpayTranId ?? "";
               payment.payment_date = DateTime.Now;
 System.Diagnostics.Debug.WriteLine($"Payment record updated: ID={payment.id}");
   }
   else
           {
      System.Diagnostics.Debug.WriteLine("WARNING: Payment record not found!");
              }

       _db.SaveChanges();
      System.Diagnostics.Debug.WriteLine("Database updated successfully!");

                    TempData["Success"] = "Payment successful! Your order is being processed.";
  return RedirectToAction("Confirmation", new { orderId = order.id });
        }
        else
       {
  System.Diagnostics.Debug.WriteLine($"Payment FAILED - Code: {vnp_ResponseCode}");
    
     // Update order as cancelled
             order.status = "Cancelled";
         order.updated_at = DateTime.Now;

       var payment = _db.Payments.FirstOrDefault(p => p.order_id == order.id);
       if (payment != null)
    {
      payment.status = "Failed";
     payment.transaction_id = vnpayTranId ?? "";
          }

      _db.SaveChanges();

     string errorMessage = GetVnPayResponseMessage(vnp_ResponseCode);
    TempData["Error"] = $"Payment failed: {errorMessage}";
    return RedirectToAction("Index", "Home");
 }
        }
         catch (Exception ex)
  {
           System.Diagnostics.Debug.WriteLine($"EXCEPTION in VnPayReturn: {ex.Message}");
  System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
   if (ex.InnerException != null)
     {
     System.Diagnostics.Debug.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }
         
    TempData["Error"] = "Error processing payment response. Please contact support.";
         return RedirectToAction("Index", "Home");
       }
        }
        // Helper method to get VNPay response message
        private string GetVnPayResponseMessage(string responseCode)
        {
            switch (responseCode)
            {
                case "00": return "Transaction successful";
                case "07": return "Suspected fraud";
                case "09": return "Customer's card not registered for Internet Banking";
                case "10": return "Customer entered incorrect card information more than 3 times";
                case "11": return "Payment timeout";
                case "12": return "Card locked";
                case "13": return "Incorrect OTP";
                case "24": return "Customer cancelled transaction";
                case "51": return "Insufficient account balance";
                case "65": return "Daily transaction limit exceeded";
                case "75": return "Payment bank under maintenance";
                case "79": return "Transaction amount exceeded limit";
                default: return $"Payment error (Code: {responseCode})";
            }
        }

        // GET: Checkout/Confirmation
        [Authorize]
        public ActionResult Confirmation(int orderId)
        {
            var order = _db.Orders.Find(orderId);
            if (order == null)
            {
                TempData["Error"] = "Order not found.";
                return RedirectToAction("Index", "Home");
            }

            var payment = _db.Payments.FirstOrDefault(p => p.order_id == orderId);

            var vm = new OrderConfirmationVM
            {
                OrderId = order.id,
                OrderCode = "#" + order.id.ToString("D6"),
                TotalAmount = order.total_amount,
                PaymentMethod = payment?.payment_method ?? "N/A",
                Status = order.status,
                OrderDate = order.order_date
            };

            return View(vm);
        }

        // Helper method to get IP address
        private string GetIpAddress()
        {
            string ipAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

            if (string.IsNullOrEmpty(ipAddress))
            {
                ipAddress = Request.ServerVariables["REMOTE_ADDR"];
            }

            return ipAddress;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _db.Dispose();
            base.Dispose(disposing);
        }
    }
}
