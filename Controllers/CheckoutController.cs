using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        private PayPalClient _paypalClient;

        public CheckoutController()
        {
            var clientId = ConfigurationManager.AppSettings["PayPal:ClientId"];
            var secret = ConfigurationManager.AppSettings["PayPal:Secret"];
            var mode = ConfigurationManager.AppSettings["PayPal:Mode"];
            _paypalClient = new PayPalClient(clientId, secret, mode);
        }

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

            ViewBag.PayPalClientId = _paypalClient.ClientId;
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

            // Validate stock availability before creating order
            foreach (var item in cart)
            {
                var product = _db.Products.Find(item.ProductId);
                if (product == null)
                {
                    TempData["Error"] = $"Product {item.ProductName} not found.";
                    model.Items = cart;
                    return View("Index", model);
                }
                if (product.stock < item.Quantity)
                {
                    TempData["Error"] = $"Insufficient stock for {product.name}. Available: {product.stock}, Requested: {item.Quantity}";
                    model.Items = cart;
                    return View("Index", model);
                }
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
                total_amount = (long)totalAmount,
                phone = model.Phone,
                shipping_address = model.Address,
                created_at = DateTime.Now,
                updated_at = DateTime.Now
            };

            _db.Orders.Add(order);
            _db.SaveChanges();

            // Create order details AND update product stock/sold
            foreach (var item in cart)
            {
                var orderDetail = new Order_Details
                {
                    order_id = order.id,
                    product_id = item.ProductId,
                    quantity = item.Quantity,
                    price = (int)item.Price,
                    color_id = item.ColorId,
                    size_id = item.SizeId,
                    created_at = DateTime.Now
                };
                _db.Order_Details.Add(orderDetail);

                // Update product stock and sold count
                var product = _db.Products.Find(item.ProductId);
                if (product != null)
                {
                    product.stock -= item.Quantity;
                    product.sold += item.Quantity;
                }
            }

            // Create payment record
            var payment = new Payment
            {
                order_id = order.id,
                amount = (long)totalAmount,
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
                System.Diagnostics.Debug.WriteLine($"Error saving order: {ex.Message}");
                TempData["Error"] = "Error processing order. Please try again.";
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
                string vnp_Url = ConfigurationManager.AppSettings["vnp_Url"];
                string vnp_TmnCode = ConfigurationManager.AppSettings["vnp_TmnCode"];
                string vnp_HashSecret = ConfigurationManager.AppSettings["vnp_HashSecret"];
                string vnp_ReturnUrlFromConfig = ConfigurationManager.AppSettings["vnp_ReturnUrl"]; // Prefer config like sample code

                if (string.IsNullOrEmpty(vnp_TmnCode) || string.IsNullOrEmpty(vnp_HashSecret) || string.IsNullOrEmpty(vnp_Url))
                {
                    TempData["Error"] = "VNPay configuration is missing. Please contact administrator.";
                    return RedirectToAction("Index", "Home");
                }

                // Determine return URL - prefer config value (sample approach), else build dynamically
                string returnUrl;
                if (!string.IsNullOrWhiteSpace(vnp_ReturnUrlFromConfig))
                {
                    returnUrl = vnp_ReturnUrlFromConfig.Trim();
                }
                else
                {
                    var req = Request.Url;
                    var baseUrl = $"{req.Scheme}://{req.Authority}";
                    returnUrl = baseUrl + Url.Action("VnPayReturn", "Checkout");
                }

                // Build VNPay request (align to sample usage)
                var vnpay = new VnPayLibrary();
                vnpay.AddRequestData("vnp_Version", VnPayLibrary.VERSION);
                vnpay.AddRequestData("vnp_Command", "pay");
                vnpay.AddRequestData("vnp_TmnCode", vnp_TmnCode);

                long amount = order.total_amount * 100; // smallest unit
                vnpay.AddRequestData("vnp_Amount", amount.ToString());

                vnpay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
                vnpay.AddRequestData("vnp_CurrCode", "VND");
                vnpay.AddRequestData("vnp_IpAddr", VnPayLibrary.GetIpAddress(Request));
                vnpay.AddRequestData("vnp_Locale", "vn");
                vnpay.AddRequestData("vnp_OrderInfo", "Thanh toan don hang :" + order.id);
                vnpay.AddRequestData("vnp_OrderType", "other");
                vnpay.AddRequestData("vnp_ReturnUrl", returnUrl);
                vnpay.AddRequestData("vnp_TxnRef", order.id.ToString());

                string paymentUrl = vnpay.CreateRequestUrl(vnp_Url, vnp_HashSecret);
                return Redirect(paymentUrl);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[VNPAY ERROR] {ex.Message}");
                TempData["Error"] = "Error creating VNPay payment: " + ex.Message;
                return RedirectToAction("Index", "Home");
            }
        }

        private string BuildSimulatedVnPayReturnUrl(Order order, string tmnCode, string secret, string baseUrl)
        {
            // Build a sorted list like VNPay would send on return
            var data = new SortedList<string, string>(new VnPayCompare())
            {
                {"vnp_Amount", (order.total_amount * 100).ToString()},
                {"vnp_Command", "pay"},
                {"vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss")},
                {"vnp_CurrCode", "VND"},
                {"vnp_IpAddr", "127.0.0.1"},
                {"vnp_Locale", "vn"},
                {"vnp_OrderInfo", $"Thanh toan don hang #{order.id}"},
                {"vnp_OrderType", "other"},
                {"vnp_TmnCode", tmnCode},
                {"vnp_TxnRef", order.id.ToString()},
                {"vnp_TransactionNo", new Random().Next(10000000, 99999999).ToString()},
                {"vnp_ResponseCode", "00"},
                {"vnp_TransactionStatus", "00"}
            };

            // Build raw string like VnPayLibrary.GetResponseData
            var sb = new StringBuilder();
            foreach (var kv in data)
            {
                if (!string.IsNullOrEmpty(kv.Value))
                {
                    sb.Append(Uri.EscapeDataString(kv.Key));
                    sb.Append("=");
                    sb.Append(Uri.EscapeDataString(kv.Value));
                    sb.Append("&");
                }
            }
            if (sb.Length > 0) sb.Length -= 1; // trim &
            var raw = sb.ToString();

            // Compute HMAC SHA512
            var hash = ComputeHmacSha512(secret, raw);

            var returnUrl = baseUrl + Url.Action("VnPayReturn", "Checkout");
            var signedUrl = returnUrl + "?" + raw + "&vnp_SecureHash=" + hash;
            System.Diagnostics.Debug.WriteLine("[VNPAY SIM] Redirecting to: " + signedUrl);
            return signedUrl;
        }

        private static string ComputeHmacSha512(string key, string data)
        {
            var keyBytes = Encoding.UTF8.GetBytes(key);
            var dataBytes = Encoding.UTF8.GetBytes(data);
            using (var hmac = new System.Security.Cryptography.HMACSHA512(keyBytes))
            {
                var hashBytes = hmac.ComputeHash(dataBytes);
                var sb = new StringBuilder(hashBytes.Length * 2);
                foreach (var b in hashBytes)
                {
                    sb.Append(b.ToString("x2"));
                }
                return sb.ToString();
            }
        }

        // GET: Checkout/VnPayReturn
        public ActionResult VnPayReturn()
        {
            System.Diagnostics.Debug.WriteLine("[VNPAY] VnPayReturn called");
            
            if (Request.QueryString.Count > 0)
            {
                string vnp_HashSecret = ConfigurationManager.AppSettings["vnp_HashSecret"];
                var vnpayData = Request.QueryString;
                VnPayLibrary vnpay = new VnPayLibrary();

                foreach (string s in vnpayData)
                {
                    if (!string.IsNullOrEmpty(s) && s.StartsWith("vnp_"))
                    {
                        vnpay.AddResponseData(s, vnpayData[s]);
                        System.Diagnostics.Debug.WriteLine($"[VNPAY] {s} = {vnpayData[s]}");
                    }
                }

                string orderCode = Convert.ToString(vnpay.GetResponseData("vnp_TxnRef"));
                long vnpayTranId = Convert.ToInt64(vnpay.GetResponseData("vnp_TransactionNo"));
                string vnp_ResponseCode = vnpay.GetResponseData("vnp_ResponseCode");
                string vnp_TransactionStatus = vnpay.GetResponseData("vnp_TransactionStatus");
                String vnp_SecureHash = Request.QueryString["vnp_SecureHash"];

                bool checkSignature = vnpay.ValidateSignature(vnp_SecureHash, vnp_HashSecret);
                
                System.Diagnostics.Debug.WriteLine($"[VNPAY] Signature valid: {checkSignature}");
                System.Diagnostics.Debug.WriteLine($"[VNPAY] Response code: {vnp_ResponseCode}");
                
                if (checkSignature)
                {
                    if (vnp_ResponseCode == "00" && vnp_TransactionStatus == "00")
                    {
                        var itemOrder = _db.Orders.FirstOrDefault(x => x.id.ToString() == orderCode);
                        if (itemOrder != null)
                        {
                            itemOrder.status = "Confirmed";
                            itemOrder.updated_at = DateTime.Now;

                            var payment = _db.Payments.FirstOrDefault(p => p.order_id == itemOrder.id);
                            if (payment != null)
                            {
                                payment.status = "Completed";
                                payment.transaction_id = vnpayTranId.ToString();
                                payment.payment_date = DateTime.Now;
                            }

                            _db.SaveChanges();
                        }
                        TempData["Success"] = "Payment successful!";
                        return RedirectToAction("Confirmation", new { orderId = itemOrder.id });
                    }
                    else
                    {
                        TempData["Error"] = "Payment failed. Error code: " + vnp_ResponseCode;
                        return RedirectToAction("Index", "Home");
                    }
                }
                else
                {
                    TempData["Error"] = "Invalid payment signature";
                    return RedirectToAction("Index", "Home");
                }
            }

            return RedirectToAction("Index", "Home");
        }

        // GET: Checkout/Confirmation
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

        #region PayPal Integration

        [Authorize]
        [HttpPost]
        public async Task<JsonResult> CreatePayPalOrder()
        {
            try
            {
                var cart = GetCart();
                if (cart == null || !cart.Any())
                {
                    return Json(new { error = "Cart is empty" });
                }

                var totalAmount = cart.Sum(p => p.Subtotal);
                var tongTien = (totalAmount / 23000).ToString("F2", System.Globalization.CultureInfo.InvariantCulture); // Use dot separator
                var donViTienTe = "USD";
                var maDonHangThamChieu = "DH" + DateTime.Now.Ticks.ToString();

                System.Diagnostics.Debug.WriteLine($"[PayPal] Creating order: Amount={tongTien} {donViTienTe}, Ref={maDonHangThamChieu}");

                var response = await _paypalClient.CreateOrder(tongTien, donViTienTe, maDonHangThamChieu);

                System.Diagnostics.Debug.WriteLine($"[PayPal] Order created: ID={response?.id}, Status={response?.status}");

                return Json(response);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[PayPal ERROR] CreateOrder: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[PayPal ERROR] Stack: {ex.StackTrace}");
                return Json(new { error = ex.Message });
            }
        }

        [Authorize]
        [HttpPost]
        public async Task<JsonResult> CapturePayPalOrder(string orderId)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[PayPal] Capturing order: {orderId}");

                var response = await _paypalClient.CaptureOrder(orderId);

                System.Diagnostics.Debug.WriteLine($"[PayPal] Capture response: Status={response?.status}");

                // Get cart BEFORE database operations
                var cart = GetCart();
                if (cart == null || !cart.Any())
                {
                    return Json(new { error = "Cart is empty" });
                }

                var email = User.Identity.Name;
                var user = _db.Users.FirstOrDefault(u => u.email == email);

                if (user == null)
                {
                    return Json(new { error = "User not found" });
                }

                decimal totalAmount = cart.Sum(i => i.Subtotal);

                var order = new Order
                {
                    user_id = user.id,
                    order_date = DateTime.Now,
                    status = "Confirmed",
                    total_amount = (long)totalAmount,
                    phone = user.phone ?? "N/A",
                    shipping_address = user.address ?? "N/A",
                    created_at = DateTime.Now,
                    updated_at = DateTime.Now
                };

                _db.Orders.Add(order);
                _db.SaveChanges();

                System.Diagnostics.Debug.WriteLine($"[PayPal] Order created in DB: OrderId={order.id}");

                foreach (var item in cart)
                {
                    System.Diagnostics.Debug.WriteLine($"[PayPal] Processing item: ProductId={item.ProductId}, ColorId={item.ColorId}, SizeId={item.SizeId}");

                    var orderDetail = new Order_Details
                    {
                        order_id = order.id,
                        product_id = item.ProductId,
                        quantity = item.Quantity,
                        price = (int)item.Price,
                        color_id = item.ColorId.HasValue && item.ColorId.Value > 0 ? item.ColorId : null,
                        size_id = item.SizeId.HasValue && item.SizeId.Value > 0 ? item.SizeId : null,
                        created_at = DateTime.Now
                    };
                    _db.Order_Details.Add(orderDetail);

                    var product = _db.Products.Find(item.ProductId);
                    if (product != null)
                    {
                        product.stock -= item.Quantity;
                        product.sold += item.Quantity;
                    }
                }

                System.Diagnostics.Debug.WriteLine($"[PayPal] Order details added, creating payment record");

                var payment = new Payment
                {
                    order_id = order.id,
                    amount = (long)totalAmount,
                    payment_method = "PayPal",
                    status = "Completed",
                    transaction_id = response.id,
                    payment_date = DateTime.Now
                };
                _db.Payments.Add(payment);

                try
                {
                    _db.SaveChanges();
                    System.Diagnostics.Debug.WriteLine($"[PayPal] All data saved successfully");
                }
                catch (System.Data.Entity.Validation.DbEntityValidationException ex)
                {
                    foreach (var validationErrors in ex.EntityValidationErrors)
                    {
                        foreach (var validationError in validationErrors.ValidationErrors)
                        {
                            System.Diagnostics.Debug.WriteLine($"[PayPal VALIDATION ERROR] Property: {validationError.PropertyName}, Error: {validationError.ErrorMessage}");
                        }
                    }
                    throw;
                }

                Session.Remove(CartSessionKey);

                return Json(new { success = true, orderId = order.id });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[PayPal ERROR] CaptureOrder: {ex.Message}");
                
                var innerEx = ex.InnerException;
                while (innerEx != null)
                {
                    System.Diagnostics.Debug.WriteLine($"[PayPal ERROR] Inner: {innerEx.Message}");
                    innerEx = innerEx.InnerException;
                }
                
                System.Diagnostics.Debug.WriteLine($"[PayPal ERROR] Stack: {ex.StackTrace}");
                
                return Json(new { error = GetDeepestExceptionMessage(ex) });
            }
        }

        private string GetDeepestExceptionMessage(Exception ex)
        {
            var innermost = ex;
            while (innermost.InnerException != null)
            {
                innermost = innermost.InnerException;
            }
            return innermost.Message;
        }

        [Authorize]
        public ActionResult PaymentSuccess()
        {
            return View("Confirmation");
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing) _db.Dispose();
            base.Dispose(disposing);
        }
    }
}
