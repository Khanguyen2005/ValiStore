using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using ValiModern.Models.EF;
using ValiModern.Models.ViewModels;

namespace ValiModern.Controllers
{
    public class CartController : Controller
    {
        private readonly ValiModernDBEntities _db = new ValiModernDBEntities();

        private const string CartSessionKey = "ShoppingCart";

        // Helper để lấy/set cart từ session
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

        private void SaveCart(List<CartItem> cart)
        {
            Session[CartSessionKey] = cart;
        }

        // GET: Cart
        public ActionResult Index()
        {
            var cart = GetCart();
            var vm = new CartVM { Items = cart };
            return View(vm);
        }

        // POST: Cart/AddToCart
        [HttpPost]
        public ActionResult AddToCart(int productId, int quantity = 1, int? colorId = null, int? sizeId = null)
        {
            // OPTIMIZE: Use AsNoTracking and select only needed fields
            var product = _db.Products
                .AsNoTracking()
                .Where(p => p.id == productId)
                .Select(p => new { p.id, p.name, p.image_url, p.price })
                .FirstOrDefault();
                
            if (product == null)
            {
                TempData["Error"] = "Product not found.";
                return RedirectToAction("Index", "Home");
            }

            var cart = GetCart();

            // Find if item already exists with same product/color/size
            var existingItem = cart.FirstOrDefault(i =>
                i.ProductId == productId &&
                i.ColorId == colorId &&
                i.SizeId == sizeId);

            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                // OPTIMIZE: Load color and size with AsNoTracking
                var color = colorId.HasValue 
                    ? _db.Colors
                        .AsNoTracking()
                        .Where(c => c.id == colorId.Value)
                        .Select(c => new { c.name, c.color_code })
                        .FirstOrDefault()
                    : null;
                    
                var size = sizeId.HasValue 
                    ? _db.Sizes
                        .AsNoTracking()
                        .Where(s => s.id == sizeId.Value)
                        .Select(s => new { s.name })
                        .FirstOrDefault()
                    : null;

                cart.Add(new CartItem
                {
                    ProductId = productId,
                    ProductName = product.name,
                    ProductImageUrl = product.image_url,
                    Price = product.price,
                    Quantity = quantity,
                    ColorId = colorId,
                    ColorName = color?.name,
                    ColorCode = color?.color_code,
                    SizeId = sizeId,
                    SizeName = size?.name
                });
            }

            SaveCart(cart);
            TempData["Success"] = "Product added to cart!";
            return RedirectToAction("Index");
        }

        // POST: Cart/UpdateQuantity
        [HttpPost]
        public ActionResult UpdateQuantity(int productId, int? colorId, int? sizeId, int quantity)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(i =>
                i.ProductId == productId &&
                i.ColorId == colorId &&
                i.SizeId == sizeId);

            if (item != null)
            {
                if (quantity <= 0)
                {
                    cart.Remove(item);
                }
                else
                {
                    item.Quantity = quantity;
                }
                SaveCart(cart);
            }

            return RedirectToAction("Index");
        }

        // POST: Cart/RemoveFromCart
        [HttpPost]
        public ActionResult RemoveFromCart(int productId, int? colorId, int? sizeId)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(i =>
                i.ProductId == productId &&
                i.ColorId == colorId &&
                i.SizeId == sizeId);

            if (item != null)
            {
                cart.Remove(item);
                SaveCart(cart);
                TempData["Success"] = "Item removed from cart.";
            }

            return RedirectToAction("Index");
        }

        // POST: Cart/ClearCart
        [HttpPost]
        public ActionResult ClearCart()
        {
            Session.Remove(CartSessionKey);
            TempData["Success"] = "Cart cleared.";
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _db.Dispose();
            base.Dispose(disposing);
        }
    }
}
