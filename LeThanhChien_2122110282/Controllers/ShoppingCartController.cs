using LeThanhChien_2122110282.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LeThanhChien_2122110282.Models;

namespace LeThanhChien_2122110282.Controllers
{
    public class ShoppingCartController : Controller
    {
        CSDLASPEntities2 objCSDLASPEntities2 = new CSDLASPEntities2();

        // GET: ShoppingCart
        public ActionResult AllShoppingCart()
        {
            var cart = Session["cart"] as List<CartModel> ?? new List<CartModel>();

            // Calculate the top 8 discounted products
            using (var db = new CSDLASPEntities2())
            {
                var discountedProductIds = db.Products
                    .Where(p => p.PriceDiscount.HasValue && p.PriceDiscount < p.Price)
                    .OrderByDescending(p => (p.Price - p.PriceDiscount) / p.Price)
                    .Take(8)
                    .Select(p => p.Id)
                    .ToList();

                ViewBag.DiscountedProductIds = discountedProductIds;
            }

            return View(cart);
        }

        [HttpPost]
        public ActionResult AddToCart(int Id, int Quantity)
        {
            try
            {
                var cart = Session["cart"] as List<CartModel> ?? new List<CartModel>();
                var product = objCSDLASPEntities2.Products.FirstOrDefault(p => p.Id == Id);
                if (product == null)
                {
                    return Json(new { success = false, message = "Sản phẩm không tồn tại." }, JsonRequestBehavior.AllowGet);
                }

                // Calculate the top 8 discounted products
                var discountedProducts = objCSDLASPEntities2.Products
                    .Where(p => p.PriceDiscount.HasValue && p.PriceDiscount < p.Price)
                    .OrderByDescending(p => (p.Price - p.PriceDiscount) / p.Price)
                    .Take(8)
                    .Select(p => p.Id)
                    .ToList();

                // Determine the price to use
                double priceToUse = discountedProducts.Contains(product.Id) && product.PriceDiscount.HasValue && product.PriceDiscount < product.Price
                    ? product.PriceDiscount.Value
                    : product.Price ?? 0;

                var cartItem = cart.FirstOrDefault(x => x.Product.Id == Id);
                if (cartItem != null)
                {
                    cartItem.Quantity += Quantity;
                }
                else
                {
                    cart.Add(new CartModel
                    {
                        Product = product,
                        Quantity = Quantity,
                        Price = priceToUse,
                    });
                }

                Session["cart"] = cart;
                Session["count"] = Convert.ToInt32(Session["count"] ?? 0) + Quantity;

                return Json(new { success = true, count = Convert.ToInt32(Session["count"]) }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        private int isExist(int id)
        {
            List<CartModel> cart = Session["cart"] as List<CartModel> ?? new List<CartModel>();
            for (int i = 0; i < cart.Count; i++)
            {
                if (cart[i].Product.Id.Equals(id))
                    return i;
            }
            return -1;
        }

        public ActionResult Cart()
        {
            var cart = Session["cart"] as List<CartModel> ?? new List<CartModel>();

            // Calculate the top 8 discounted products
            using (var db = new CSDLASPEntities2())
            {
                var discountedProductIds = db.Products
                    .Where(p => p.PriceDiscount.HasValue && p.PriceDiscount < p.Price)
                    .OrderByDescending(p => (p.Price - p.PriceDiscount) / p.Price)
                    .Take(8)
                    .Select(p => p.Id)
                    .ToList();

                ViewBag.DiscountedProductIds = discountedProductIds;
            }

            return View(cart);
        }

        [HttpPost]
        public ActionResult Remove(int id)
        {
            try
            {
                var cart = Session["cart"] as List<CartModel> ?? new List<CartModel>();
                var itemToRemove = cart.FirstOrDefault(x => x.Product.Id == id);
                if (itemToRemove == null)
                {
                    return Json(new { success = false, message = "Sản phẩm không tồn tại trong giỏ hàng." });
                }

                cart.Remove(itemToRemove);
                Session["cart"] = cart;

                var currentCount = Convert.ToInt32(Session["count"] ?? 0);
                var newCount = currentCount - itemToRemove.Quantity;
                Session["count"] = newCount > 0 ? newCount : 0;

                // Calculate the new subtotal using CartModel.Price
                var subtotal = cart.Sum(item => item.Price * item.Quantity);

                return Json(new { success = true, subtotal = subtotal * 1000 }); // Multiply by 1000 to match the display format
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }

        public class RemoveItemsModel
        {
            public int[] Ids { get; set; }
        }

        [HttpPost]
        public ActionResult RemoveMultiple(RemoveItemsModel model)
        {
            if (model == null || model.Ids == null || !model.Ids.Any())
            {
                return Json(new { success = false, message = "Không có sản phẩm nào được chọn để xóa." });
            }

            List<CartModel> cart = Session["cart"] as List<CartModel> ?? new List<CartModel>();
            int initialCount = Convert.ToInt32(Session["count"] ?? 0);

            int removedItems = cart.RemoveAll(item => model.Ids.Contains(item.Product.Id));

            Session["cart"] = cart;
            Session["count"] = initialCount - removedItems;

            return Json(new { success = true, count = Convert.ToInt32(Session["count"]) });
        }

        [HttpGet]
        public ActionResult OrderTracking()
        {
            // Check if user is logged in
            if (Session["idUser"] == null)
            {
                return RedirectToAction("Login", "Home");
            }

            int userId = (int)Session["idUser"];
            var user = objCSDLASPEntities2.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
            {
                Session.Clear();
                return RedirectToAction("Login", "Home");
            }

            // Fetch all orders
            var orders = objCSDLASPEntities2.Orders.ToList();

            // Filter orders by the logged-in user's name
            string userFullName = $"{user.FirstName} {user.LastName}";
            var userOrders = orders
                .Where(o => o.Name.StartsWith(userFullName))
                .ToList();

            // Count unique order groups
            if (!userOrders.Any())
            {
                ViewBag.Message = "Không có đơn hàng nào để hiển thị.";
                Session["orderCount"] = 0;
            }
            else
            {
                var orderGroups = userOrders
                    .Select(o => o.Name.Split(new[] { " | OrderID: " }, StringSplitOptions.None)[1])
                    .Distinct()
                    .ToList();
                Session["orderCount"] = orderGroups.Count;
            }

            // Fetch all products to build productDict
            var products = objCSDLASPEntities2.Products.ToList();
            var productDict = products.ToDictionary(p => p.Id, p => p);

            // Identify discounted products using the same logic as the homepage
            var discountedProducts = products
                .Where(p => p.PriceDiscount.HasValue && p.PriceDiscount < p.Price)
                .OrderByDescending(p => (p.Price - p.PriceDiscount) / p.Price)
                .Take(8)
                .ToList();
            var discountedProductIds = discountedProducts.Select(p => p.Id).ToList();

            // Pass data to the view via ViewBag
            ViewBag.DiscountedProductIds = discountedProductIds;
            ViewBag.Products = productDict;

            // Debug logs
            System.Diagnostics.Debug.WriteLine($"Total orders in database: {orders.Count}");
            System.Diagnostics.Debug.WriteLine($"User orders: {userOrders.Count}");
            System.Diagnostics.Debug.WriteLine($"User order groups: {Session["orderCount"]}");
            System.Diagnostics.Debug.WriteLine($"Discounted Product IDs: {string.Join(", ", discountedProductIds)}");
            foreach (var product in products.Where(p => p.PriceDiscount.HasValue))
            {
                System.Diagnostics.Debug.WriteLine($"Product ID: {product.Id}, Price: {product.Price}, PriceDiscount: {product.PriceDiscount}");
            }

            return View(userOrders);
        }

        public ActionResult OrderDetails(int id)
        {
            var order = objCSDLASPEntities2.Orders.FirstOrDefault(o => o.Id == id);
            if (order == null)
            {
                TempData["ErrorMessage"] = "Đơn hàng không tồn tại.";
                return RedirectToAction("OrderTracking");
            }

            // Extract OrderGroupId from Name
            var parts = order.Name.Split(new[] { " | OrderID: " }, StringSplitOptions.None);
            if (parts.Length < 2)
            {
                TempData["ErrorMessage"] = "Định dạng đơn hàng không hợp lệ.";
                return RedirectToAction("OrderTracking");
            }

            // Compute the search string outside the LINQ query
            var searchString = "OrderID: " + parts[1];
            var groupOrders = objCSDLASPEntities2.Orders
                .Where(o => o.Name.Contains(searchString))
                .ToList();

            return View(groupOrders);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CancelOrderGroup(string orderGroupId)
        {
            try
            {
                if (string.IsNullOrEmpty(orderGroupId))
                {
                    TempData["ErrorMessage"] = "Mã đơn hàng không hợp lệ.";
                    return RedirectToAction("OrderTracking");
                }

                // Verify the user is logged in
                if (Session["idUser"] == null)
                {
                    return RedirectToAction("Login", "Home");
                }
                int userId = (int)Session["idUser"];
                var user = objCSDLASPEntities2.Users.FirstOrDefault(u => u.Id == userId);
                if (user == null)
                {
                    Session.Clear();
                    return RedirectToAction("Login", "Home");
                }
                string userFullName = $"{user.FirstName} {user.LastName}";

                var searchString = "OrderID: " + orderGroupId;
                var orders = objCSDLASPEntities2.Orders
                    .Where(o => o.Name.Contains(searchString))
                    .ToList();

                if (!orders.Any())
                {
                    TempData["ErrorMessage"] = "Đơn hàng không tồn tại.";
                    return RedirectToAction("OrderTracking");
                }

                // Verify the order group belongs to the user
                if (!orders.All(o => o.Name.StartsWith(userFullName)))
                {
                    TempData["ErrorMessage"] = "Bạn không có quyền hủy đơn hàng này.";
                    return RedirectToAction("OrderTracking");
                }

                foreach (var order in orders)
                {
                    objCSDLASPEntities2.Orders.Remove(order);
                }

                objCSDLASPEntities2.SaveChanges();

                // Recalculate the number of order groups for the user
                var remainingOrders = objCSDLASPEntities2.Orders.ToList();
                var userOrders = remainingOrders
                    .Where(o => o.Name.StartsWith(userFullName))
                    .ToList();

                if (!userOrders.Any())
                {
                    Session["orderCount"] = 0;
                }
                else
                {
                    var orderGroups = userOrders
                        .Select(o => o.Name.Split(new[] { " | OrderID: " }, StringSplitOptions.None)[1])
                        .Distinct()
                        .ToList();
                    Session["orderCount"] = orderGroups.Count;
                }

                TempData["SuccessMessage"] = "Hủy đơn hàng thành công.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Có lỗi xảy ra khi hủy đơn hàng: {ex.Message}";
            }
            return RedirectToAction("OrderTracking");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CancelOrder(int id)
        {
            try
            {
                var order = objCSDLASPEntities2.Orders.Find(id);
                if (order == null)
                {
                    TempData["ErrorMessage"] = "Đơn hàng không tồn tại.";
                    return RedirectToAction("OrderTracking");
                }

                // Verify the order belongs to the logged-in user
                if (Session["idUser"] == null)
                {
                    return RedirectToAction("Login", "Home");
                }
                int userId = (int)Session["idUser"];
                var user = objCSDLASPEntities2.Users.FirstOrDefault(u => u.Id == userId);
                if (user == null)
                {
                    Session.Clear();
                    return RedirectToAction("Login", "Home");
                }
                string userFullName = $"{user.FirstName} {user.LastName}";
                if (!order.Name.StartsWith(userFullName))
                {
                    TempData["ErrorMessage"] = "Bạn không có quyền hủy đơn hàng này.";
                    return RedirectToAction("OrderTracking");
                }

                objCSDLASPEntities2.Orders.Remove(order);
                objCSDLASPEntities2.SaveChanges();

                // Recalculate the number of order groups for the user
                var remainingOrders = objCSDLASPEntities2.Orders.ToList();
                var userOrders = remainingOrders
                    .Where(o => o.Name.StartsWith(userFullName))
                    .ToList();

                if (!userOrders.Any())
                {
                    Session["orderCount"] = 0;
                }
                else
                {
                    var orderGroups = userOrders
                        .Select(o => o.Name.Split(new[] { " | OrderID: " }, StringSplitOptions.None)[1])
                        .Distinct()
                        .ToList();
                    Session["orderCount"] = orderGroups.Count;
                }

                TempData["SuccessMessage"] = "Hủy đơn hàng thành công.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Có lỗi xảy ra khi hủy đơn hàng: {ex.Message}";
            }
            return RedirectToAction("OrderTracking");
        }

        public ActionResult Wishlist()
        {
            var wishlist = Session["Wishlist"] as List<CartModel> ?? new List<CartModel>();
            wishlist.RemoveAll(item => item.Product == null || objCSDLASPEntities2.Products.Find(item.Product.Id) == null);
            Session["Wishlist"] = wishlist;
            Session["wishlistCount"] = wishlist.Sum(x => x.Quantity);

            // Calculate the top 8 discounted products
            using (var db = new CSDLASPEntities2())
            {
                var discountedProductIds = db.Products
                    .Where(p => p.PriceDiscount.HasValue && p.PriceDiscount < p.Price)
                    .OrderByDescending(p => (p.Price - p.PriceDiscount) / p.Price)
                    .Take(8)
                    .Select(p => p.Id)
                    .ToList();

                ViewBag.DiscountedProductIds = discountedProductIds;
            }

            return View(wishlist);
        }

        [HttpPost]
        public ActionResult AddToWishlist(int Id, int Quantity)
        {
            if (Id <= 0)
            {
                return Json(new { success = false, message = "ID sản phẩm không hợp lệ." });
            }
            try
            {
                var wishlist = Session["Wishlist"] as List<CartModel> ?? new List<CartModel>();
                var product = objCSDLASPEntities2.Products.FirstOrDefault(p => p.Id == Id);
                if (product == null)
                {
                    return Json(new { success = false, message = "Sản phẩm không tồn tại." });
                }

                // Calculate the top 8 discounted products
                var discountedProducts = objCSDLASPEntities2.Products
                    .Where(p => p.PriceDiscount.HasValue && p.PriceDiscount < p.Price)
                    .OrderByDescending(p => (p.Price - p.PriceDiscount) / p.Price)
                    .Take(8)
                    .Select(p => p.Id)
                    .ToList();

                // Determine the price to use
                double priceToUse = discountedProducts.Contains(product.Id) && product.PriceDiscount.HasValue && product.PriceDiscount < product.Price
                    ? product.PriceDiscount.Value
                    : product.Price ?? 0;

                var item = wishlist.FirstOrDefault(x => x.Product.Id == Id);
                if (item == null)
                {
                    wishlist.Add(new CartModel
                    {
                        Product = product,
                        Quantity = Quantity,
                        Price = priceToUse
                    });
                }
                else
                {
                    item.Quantity += Quantity;
                }

                Session["Wishlist"] = wishlist;
                Session["wishlistCount"] = wishlist.Sum(x => x.Quantity);

                return Json(new { success = true, wishlistCount = Session["wishlistCount"] });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public ActionResult RemoveFromWishlist(int id)
        {
            try
            {
                var wishlist = Session["Wishlist"] as List<CartModel> ?? new List<CartModel>();
                var item = wishlist.FirstOrDefault(x => x.Product.Id == id);
                if (item != null)
                {
                    wishlist.Remove(item);
                    Session["Wishlist"] = wishlist;
                    Session["wishlistCount"] = wishlist.Sum(x => x.Quantity);
                    return Json(new { success = true, wishlistCount = Session["wishlistCount"] });
                }
                return Json(new { success = false, message = "Sản phẩm không có trong danh sách yêu thích." });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                return Json(new { success = false, message = ex.Message });
            }
        }
        [HttpGet]
        public ActionResult GetCartItems()
        {
            try
            {
                var cart = Session["cart"] as List<CartModel> ?? new List<CartModel>();
                if (!cart.Any())
                {
                    return Json(new { success = true, items = new List<object>(), total = 0 }, JsonRequestBehavior.AllowGet);
                }

                var items = cart.Select(item => new
                {
                    ProductId = item.Product.Id,
                    Name = item.Product.Name ?? "Sản phẩm không xác định",
                    Price = item.Product.Price ?? 0,
                    PriceDiscount = item.Product.PriceDiscount,
                    Avatar = item.Product.Avatar ?? "default-image.jpg",
                    Quantity = item.Quantity,
                    CartPrice = item.Price // Use the price stored in the cart item
                }).ToList();

                var total = cart.Sum(item => item.Price * item.Quantity);

                return Json(new { success = true, items, total }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetCartItems: {ex.Message}");
                return Json(new { success = false, message = "Lỗi máy chủ khi tải giỏ hàng." }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult CheckWishlistStatus(int productId)
        {
            try
            {
                var wishlist = Session["Wishlist"] as List<CartModel> ?? new List<CartModel>();
                var isInWishlist = wishlist.Any(item => item.Product.Id == productId);
                return Json(new { isInWishlist }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in CheckWishlistStatus: {ex.Message}");
                return Json(new { isInWishlist = false }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}