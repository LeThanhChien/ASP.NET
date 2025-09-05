using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Security;
using LeThanhChien_2122110282.Context;
using LeThanhChien_2122110282.Models;
using Newtonsoft.Json;

namespace LeThanhChien_2122110282.Controllers
{
    public class HomeController : Controller
    {
        CSDLASPEntities2 objCSDLASPEntities2 = new CSDLASPEntities2();

        public ActionResult Index()
        {
            HomeModel objHomeModel = new HomeModel();
            objHomeModel.ListCategory = objCSDLASPEntities2.Categories.ToList();
            objHomeModel.ListProduct = objCSDLASPEntities2.Products.ToList();
            return View(objHomeModel);
        }

        public ActionResult Search(string search)
        {
            var products = objCSDLASPEntities2.Products
                .Where(p => p.Name.Contains(search) || p.FullDescription.Contains(search))
                .ToList();
            return View(products);
        }



        [HttpGet]
        public ActionResult Login()
        {
            if (Session["idUser"] != null)
            {
                return RedirectToAction("Profile", "Home");
            }
            return View(new User());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string email, string password)
        {
            if (ModelState.IsValid)
            {
                var f_password = GetMD5(password);
                var data = objCSDLASPEntities2.Users
                    .Where(s => s.Email.Equals(email) && s.Password.Equals(f_password))
                    .ToList();

                if (data.Count() > 0)
                {
                    Session["FullName"] = data.FirstOrDefault().FirstName + " " + data.FirstOrDefault().LastName;
                    Session["Email"] = data.FirstOrDefault().Email;
                    Session["idUser"] = data.FirstOrDefault().Id;
                    return RedirectToAction("Profile", "Home");
                }
                else
                {
                    ViewBag.error = "Login failed";
                    return View();
                }
            }
            return View();
        }

        public ActionResult Logout()
        {
            Session.Clear();
            Session.Abandon();
            FormsAuthentication.SignOut();
            return RedirectToAction("Login");
        }

        public static string GetMD5(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return string.Empty;
            }

            using (MD5 md5 = new MD5CryptoServiceProvider())
            {
                byte[] fromData = Encoding.UTF8.GetBytes(str);
                byte[] targetData = md5.ComputeHash(fromData);
                StringBuilder byte2String = new StringBuilder();

                for (int i = 0; i < targetData.Length; i++)
                {
                    byte2String.Append(targetData[i].ToString("x2"));
                }
                return byte2String.ToString();
            }
        }
        public ActionResult GetProducts(int page = 1, int pageSize = 12)
        {
            try
            {
                var products = objCSDLASPEntities2.Products.ToList();
                var totalProducts = products.Count;
                var totalPages = (int)Math.Ceiling((double)totalProducts / pageSize);
                var pagedProducts = products
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(p => new
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Price = p.Price ?? 0,
                        PriceDiscount = p.PriceDiscount,
                        Avatar = p.Avatar ?? "default-image.jpg",
                        ShortDes = p.ShortDes ?? "",
                        CategoryId = p.CategoryId
                    })
                    .ToList();

                // Calculate the top 8 discounted products
                var discountedProductIds = objCSDLASPEntities2.Products
                    .Where(p => p.PriceDiscount.HasValue && p.PriceDiscount < p.Price)
                    .OrderByDescending(p => (p.Price - p.PriceDiscount) / p.Price)
                    .Take(8)
                    .Select(p => p.Id)
                    .ToList();

                return Json(new
                {
                    success = true,
                    Products = pagedProducts,
                    TotalPages = totalPages,
                    CurrentPage = page,
                    DiscountedProductIds = discountedProductIds // Add discountedProductIds to the response
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult Blog()
        {
            HomeModel objHomeModel = new HomeModel();
            objHomeModel.ListCategory = objCSDLASPEntities2.Categories.ToList();
            objHomeModel.ListProduct = objCSDLASPEntities2.Products.ToList();
            return View(objHomeModel);
        }
        public ActionResult About()
        {
            return View();
        }
        public ActionResult Contact()
        {
            return View();
        }

        public ActionResult Profile()
        {
            System.Diagnostics.Debug.WriteLine($"Profile action: idUser={Session["idUser"]}, Email={Session["Email"]}, FullName={Session["FullName"]}");

            // Check if the user is logged in using session
            if (Session["idUser"] == null)
            {
                System.Diagnostics.Debug.WriteLine("Session[idUser] is null, redirecting to Login");
                return RedirectToAction("Login");
            }

            int userId = (int)Session["idUser"];
            var user = objCSDLASPEntities2.Users.FirstOrDefault(u => u.Id == userId);

            if (user == null)
            {
                System.Diagnostics.Debug.WriteLine("User not found in database, clearing session and redirecting to Login");
                Session.Clear();
                return RedirectToAction("Login");
            }

            return View(user);
        }
        [Authorize]
        public ActionResult EditProfile()
        {
            int userId = (int)Session["idUser"];
            var user = objCSDLASPEntities2.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
            {
                return HttpNotFound("User not found");
            }
            return View(user);
        }

        [HttpGet]
        public ActionResult Checkout()
        {
            if (Session["idUser"] == null)
            {
                return RedirectToAction("Login", "Home");
            }
            else { 
            var discountedProductIds = objCSDLASPEntities2.Products
                .Where(p => p.PriceDiscount.HasValue && p.PriceDiscount < p.Price)
                .OrderByDescending(p => (p.Price - p.PriceDiscount) / p.Price)
                .Take(8)
                .Select(p => p.Id)
                .ToList();

            ViewBag.DiscountedProductIds = discountedProductIds;

            return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Checkout(string fullName, string address, string email, string paymentMethod)
        {
            if (Session["idUser"] == null)
            {
                return RedirectToAction("Login", "Home");
            }
            try
            {
                if (string.IsNullOrWhiteSpace(fullName))
                    ModelState.AddModelError("fullName", "Full name is required");
                if (string.IsNullOrWhiteSpace(address))
                    ModelState.AddModelError("address", "Address is required");
                if (string.IsNullOrWhiteSpace(email))
                    ModelState.AddModelError("email", "Email is required");
                if (string.IsNullOrWhiteSpace(paymentMethod))
                    ModelState.AddModelError("paymentMethod", "Payment method is required");

                if (!ModelState.IsValid)
                {
                    return View();
                }

                if (Session["cart"] == null)
                {
                    TempData["ErrorMessage"] = "Your cart is empty. Please add more products.";
                    return RedirectToAction("Cart", "ShoppingCart");
                }

                if (Session["idUser"] == null)
                {
                    return RedirectToAction("Login");
                }
                int userId = (int)Session["idUser"];
                var user = objCSDLASPEntities2.Users.FirstOrDefault(u => u.Id == userId);
                if (user == null)
                {
                    Session.Clear();
                    return RedirectToAction("Login");
                }
                string userFullName = $"{user.FirstName} {user.LastName}";

                using (var transaction = objCSDLASPEntities2.Database.BeginTransaction())
                {
                    try
                    {
                        var cart = (List<CartModel>)Session["cart"];
                        string orderGroupId = Guid.NewGuid().ToString();

                        foreach (var item in cart)
                        {
                            var order = new Order
                            {
                                Name = $"{userFullName} | OrderID: {orderGroupId}", // Use the user's actual name
                                ProductId = item.Product.Id,
                                Price = item.Product.Price * item.Quantity,
                                Status = paymentMethod == "Cash on Delivery" ? 1 : 3,
                                PaymentMethod = paymentMethod,
                                CreatedOnUtc = DateTime.UtcNow
                            };
                            objCSDLASPEntities2.Orders.Add(order);
                        }

                        objCSDLASPEntities2.SaveChanges();
                        transaction.Commit();

                        Session["cart"] = null;
                        TempData["SuccessMessage"] = "Đơn hàng đã được đặt thành công!";
                        return RedirectToAction("OrderConfirmation");
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        System.Diagnostics.Debug.WriteLine($"Checkout error: {ex}");
                        TempData["ErrorMessage"] = $"An error occurred: {ex.Message}";
                        return RedirectToAction("Checkout");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Checkout error: {ex}");
                TempData["ErrorMessage"] = "An unexpected error occurred. Please try again.";
                return RedirectToAction("Checkout");
            }
        }

        [HttpGet]
        public ActionResult MoMoCheckout(string FullName, string Address, string Email)
        {

            if (Session["cart"] == null)
            {
                TempData["ErrorMessage"] = "Your cart is empty. Please add more products.";
                return RedirectToAction("Cart", "ShoppingCart");
            }

            var cart = (List<CartModel>)Session["cart"];
                decimal subtotal = cart.Sum(item => (decimal)item.Product.Price * item.Quantity);

                ViewBag.FullName = string.IsNullOrEmpty(FullName) ? "No Name Provided" : FullName;
                ViewBag.Address = string.IsNullOrEmpty(Address) ? "No Address Provided" : Address;
                ViewBag.Email = string.IsNullOrEmpty(Email) ? "No Email Provided" : Email;
                ViewBag.TotalAmount = subtotal;

                return View();
            
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MoMoCheckout(MoMoCheckoutModel model)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(model.FullName) || string.IsNullOrWhiteSpace(model.Address) || string.IsNullOrWhiteSpace(model.Email))
                {
                    return Json(new { success = false, error = "Please fill in all required fields." });
                }

                if (Session["cart"] == null)
                {
                    return Json(new { success = false, error = "Your cart session has expired. Please reload the cart." });
                }

                if (Session["idUser"] == null)
                {
                    return Json(new { success = false, error = "Please log in to place an order." });
                }
                int userId = (int)Session["idUser"];
                var user = objCSDLASPEntities2.Users.FirstOrDefault(u => u.Id == userId);
                if (user == null)
                {
                    Session.Clear();
                    return Json(new { success = false, error = "User not found. Please log in again." });
                }
                string userFullName = $"{user.FirstName} {user.LastName}";

                var cart = (List<CartModel>)Session["cart"];
                string orderGroupId = Guid.NewGuid().ToString();

                foreach (var item in cart)
                {
                    if (item == null || item.Product == null)
                    {
                        return Json(new { success = false, error = "Invalid cart item detected." });
                    }

                    objCSDLASPEntities2.Orders.Add(new Order
                    {
                        Name = $"{userFullName} | OrderID: {orderGroupId}",
                        ProductId = item.Product.Id,
                        Price = item.Product.Price * item.Quantity,
                        Status = 2, // MoMo status
                        PaymentMethod = model.PaymentMethod,
                        CreatedOnUtc = DateTime.UtcNow
                    });
                }

                objCSDLASPEntities2.SaveChanges();
                Session["cart"] = null;
                return RedirectToAction("MoMoConfirmation");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"MoMoCheckout POST error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                return Json(new { success = false, error = $"An error occurred while processing your payment: {ex.Message}" });
            }
        }

        public ActionResult OrderConfirmation()
        {
            return View();
        }

        public ActionResult MoMoConfirmation()
        {
            if (TempData["SuccessMessage"] == null)
            {

                return RedirectToAction("Checkout");
            }
            return View();
        }
        [HttpGet]
        public ActionResult PayPalCheckout(string FullName, string Address, string Email)
        {
            if (Session["cart"] == null)
            {
                TempData["ErrorMessage"] = "Your cart is empty. Please add more products.";
                return RedirectToAction("Cart", "ShoppingCart");
            }

            var cart = (List<CartModel>)Session["cart"];
            decimal subtotal = cart.Sum(item => (decimal)item.Product.Price * item.Quantity);

            ViewBag.FullName = string.IsNullOrEmpty(FullName) ? "No Name Provided" : FullName;
            ViewBag.Address = string.IsNullOrEmpty(Address) ? "No Address Provided" : Address;
            ViewBag.Email = string.IsNullOrEmpty(Email) ? "No Email Provided" : Email;
            ViewBag.TotalAmount = subtotal;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreatePayPalOrder(string fullName, string address, string email)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(address) || string.IsNullOrWhiteSpace(email))
                {
                    return Json(new { success = false, error = "Please fill in all required fields." });
                }

                if (Session["cart"] == null)
                {
                    return Json(new { success = false, error = "Your cart session has expired. Please reload the cart." });
                }

                if (Session["idUser"] == null)
                {
                    return Json(new { success = false, error = "Please log in to place an order." });
                }

                var cart = (List<CartModel>)Session["cart"];
                decimal total = cart.Sum(item => (decimal)item.Product.Price * item.Quantity);

                Session["PayPalOrderDetails"] = new
                {
                    FullName = fullName,
                    Address = address,
                    Email = email,
                    Total = total
                };

                return Json(new { success = true, total = total.ToString("F2") });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CreatePayPalOrder error: {ex.Message}");
                return Json(new { success = false, error = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CapturePayPalOrder(string orderID)
        {
            try
            {
                if (string.IsNullOrEmpty(orderID))
                {
                    return Json(new { success = false, error = "Invalid order ID." });
                }

                if (Session["cart"] == null || Session["PayPalOrderDetails"] == null)
                {
                    return Json(new { success = false, error = "Session expired or invalid cart data." });
                }

                if (Session["idUser"] == null)
                {
                    return Json(new { success = false, error = "Please log in to place an order." });
                }
                int userId = (int)Session["idUser"];
                var user = objCSDLASPEntities2.Users.FirstOrDefault(u => u.Id == userId);
                if (user == null)
                {
                    Session.Clear();
                    return Json(new { success = false, error = "User not found. Please log in again." });
                }
                string userFullName = $"{user.FirstName} {user.LastName}";

                var cart = (List<CartModel>)Session["cart"];
                var orderDetails = (dynamic)Session["PayPalOrderDetails"];

                using (var transaction = objCSDLASPEntities2.Database.BeginTransaction())
                {
                    try
                    {
                        string orderGroupId = Guid.NewGuid().ToString();

                        foreach (var item in cart)
                        {
                            if (item == null || item.Product == null)
                            {
                                return Json(new { success = false, error = "Invalid cart item detected." });
                            }

                            objCSDLASPEntities2.Orders.Add(new Order
                            {
                                Name = $"{userFullName} | OrderID: {orderGroupId}",
                                ProductId = item.Product.Id,
                                Price = item.Product.Price * item.Quantity,
                                Status = 3, // Paid status
                                PaymentMethod = "PayPal",
                                CreatedOnUtc = DateTime.UtcNow
                            });
                        }

                        objCSDLASPEntities2.SaveChanges();
                        transaction.Commit();

                        Session["cart"] = null;
                        Session["PayPalOrderDetails"] = null;
                        TempData["SuccessMessage"] = "Payment successful! Order placed.";
                        return Json(new { success = true, redirectUrl = Url.Action("PayPalConfirmation") });
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        System.Diagnostics.Debug.WriteLine($"CapturePayPalOrder error: {ex.Message}");
                        return Json(new { success = false, error = $"An error occurred while processing your payment: {ex.Message}" });
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CapturePayPalOrder error: {ex.Message}");
                return Json(new { success = false, error = $"An error occurred: {ex.Message}" });
            }
        }

        public ActionResult PayPalConfirmation()
        {
            if (TempData["SuccessMessage"] == null)
            {
                return RedirectToAction("Checkout");
            }
            return View();
        }
        [HttpPost]
        public ActionResult GoogleLogin(string id, string name, string email, string imageUrl)
        {
            try
            {
                // Verify the Google ID token (recommended for security)
                // You can use Google.Apis.Auth to validate the token server-side

                // Check if the user exists
                var existingUser = objCSDLASPEntities2.Users.FirstOrDefault(u => u.Email == email);
                if (existingUser != null)
                {
                    // Log the user in (e.g., set authentication cookie)
                    FormsAuthentication.SetAuthCookie(existingUser.Email, false);
                    return Json(new { success = true, redirectUrl = Url.Action("Index", "Home") });
                }

                // If user doesn't exist, you might redirect to registration or create a new user
                return Json(new { success = false, message = "Tài khoản không tồn tại. Vui lòng đăng ký." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
