using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
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
                    product.stock -= item.Quantity;  // Decrease stock
                    product.sold += item.Quantity;   // Increase sold count
                }
            }

            // Create payment record
            var payment = new Payment
            {
                order_id = order.id,
                amount = (long)totalAmount,  // Use validated totalAmount
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
                System.Diagnostics.Debug.WriteLine($"Error saving order: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Order Amount: {totalAmount}");
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
                // VNPay Configuration from Web.config (except ReturnUrl - build dynamically to avoid wrong port)
                string vnp_Url = ConfigurationManager.AppSettings["vnp_Url"];
                string vnp_TmnCode = ConfigurationManager.AppSettings["vnp_TmnCode"];
                string vnp_HashSecret = ConfigurationManager.AppSettings["vnp_HashSecret"];

                // Build dynamic return URL (current host + action)
                var baseUrl = Request.Url.GetLeftPart(UriPartial.Authority); // http(s)://localhost:PORT
                // If you want to enforce http(s) scheme based on config, adjust here
                var forceHttps = (ConfigurationManager.AppSettings["ForceHttpsReturnUrl"] ?? "false").Equals("true", StringComparison.OrdinalIgnoreCase);
                if (forceHttps && baseUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
                {
                    baseUrl = baseUrl.Replace("http://", "https://");
                }
                string dynamicReturnUrl = baseUrl + Url.Action("VnPayReturn", "Checkout");

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
                vnpay.AddRequestData("vnp_ReturnUrl", dynamicReturnUrl); // dynamic instead of hardcoded
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
        // Note: No [Authorize] - this is a PUBLIC callback from VNPay
        public ActionResult VnPayReturn()
        {
            if (Request.QueryString.Count > 0)
            {
                string vnp_HashSecret = ConfigurationManager.AppSettings["vnp_HashSecret"];
                var vnpayData = Request.QueryString;
                VnPayLibrary vnpay = new VnPayLibrary();

                foreach (string s in vnpayData)
                {
                    //get all querystring data
                    if (!string.IsNullOrEmpty(s) && s.StartsWith("vnp_"))
                    {
                        vnpay.AddResponseData(s, vnpayData[s]);
                    }
                }

                string orderCode = Convert.ToString(vnpay.GetResponseData("vnp_TxnRef"));
                long vnpayTranId = Convert.ToInt64(vnpay.GetResponseData("vnp_TransactionNo"));
                string vnp_ResponseCode = vnpay.GetResponseData("vnp_ResponseCode");
                string vnp_TransactionStatus = vnpay.GetResponseData("vnp_TransactionStatus");
                String vnp_SecureHash = Request.QueryString["vnp_SecureHash"];

                bool checkSignature = vnpay.ValidateSignature(vnp_SecureHash, vnp_HashSecret);
                if (checkSignature)
                {
                    if (vnp_ResponseCode == "00" && vnp_TransactionStatus == "00")
                    {
                        var itemOrder = _db.Orders.FirstOrDefault(x => x.id.ToString() == orderCode);
                        if (itemOrder != null)
                        {
                            // Update order status - use 'Confirmed' which is allowed by CHECK constraint
                            itemOrder.status = "Confirmed";  // Changed from "Processing" to "Confirmed"
                            itemOrder.updated_at = DateTime.Now;

                            // Update payment
                            var payment = _db.Payments.FirstOrDefault(p => p.order_id == itemOrder.id);
                            if (payment != null)
                            {
                                payment.status = "Completed";
                                payment.transaction_id = vnpayTranId.ToString();
                                payment.payment_date = DateTime.Now;
                            }

                            _db.SaveChanges();
                        }
                        //Thanh toan thanh cong
                        TempData["Success"] = "Payment successful!";
                        return RedirectToAction("Confirmation", new { orderId = itemOrder.id });
                    }
                    else
                    {
                        //Thanh toan khong thanh cong
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

        protected override void Dispose(bool disposing)
        {
            if (disposing) _db.Dispose();
            base.Dispose(disposing);
        }
    }
}
