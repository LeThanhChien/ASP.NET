using LeThanhChien_2122110282.Context;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace LeThanhChien_2122110282.Controllers
{
    public class UserRegisterController : Controller
    {
        CSDLASPEntities2 objCSDLASPEntities2 = new CSDLASPEntities2();
        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }

        //POST: Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(User _user)
        {
            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(_user.Email) || string.IsNullOrEmpty(_user.Password))
                {
                    ViewBag.error = "Email and Password cannot be null";
                    return View();
                }

                var check = objCSDLASPEntities2.Users.FirstOrDefault(s => s.Email == _user.Email);
                if (check == null)
                {
                    _user.Password = GetMD5(_user.Password);
                    objCSDLASPEntities2.Configuration.ValidateOnSaveEnabled = false;
                    objCSDLASPEntities2.Users.Add(_user);
                    objCSDLASPEntities2.SaveChanges();
                    TempData["SuccessMessage"] = "Registration successful! Please log in.";
                    return RedirectToAction("Login", "Home");
                }
                else
                {
                    ViewBag.error = "Email already exists";
                    return View();
                }
            }
            return View();
        }

        //create a string MD5
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


    }
}