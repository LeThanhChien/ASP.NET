using LeThanhChien_2122110282.Context;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LeThanhChien_2122110282.Models;

namespace LeThanhChien_2122110282.Areas.Admin.Controllers
{
    public class HomeController : Controller
    {
        CSDLASPEntities2 objCSDLASPEntities2 = new CSDLASPEntities2();
        // GET: Admin/Home
        public ActionResult Index()
        {
            int productcount = objCSDLASPEntities2.Products.Count();
            int memberCount = objCSDLASPEntities2.Users.Count();
            int orderCount = objCSDLASPEntities2.Orders.Count();

            var HomeModel = new HomeModel
            {
                ProductCount = productcount,  
                MemberCount = memberCount,
                OrderCount = orderCount
            }; 
            return View(HomeModel);
        }
    }
}