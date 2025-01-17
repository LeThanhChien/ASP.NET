using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using LeThanhChien_2122110282.Context;
using LeThanhChien_2122110282.Models;

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

        [HttpGet]
        public ActionResult Search(string query)
        {
            var products = objCSDLASPEntities2.Products.Where(p => p.Name.Contains(query) || p.FullDescription.Contains(query)).ToList();
            return View(products);
        }

        [HttpGet]
        public ActionResult Login()
        {
            return View();
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
                    return RedirectToAction("Index", "Home");
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
            Session.Clear(); // Remove session
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
        public JsonResult GetProducts(int page = 1, int pageSize = 10)
        {
            var items = objCSDLASPEntities2.Products.OrderBy(p => p.Id).Skip((page - 1) * pageSize).Take(pageSize).ToList();
            var totalItems = objCSDLASPEntities2.Products.Count();

            var result = new
            {
                Products = items,
                TotalItems = totalItems,
                TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize),
                CurrentPage = page
            };

            return Json(result, JsonRequestBehavior.AllowGet);
        }


    }
}
