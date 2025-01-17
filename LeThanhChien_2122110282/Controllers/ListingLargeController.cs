using LeThanhChien_2122110282.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LeThanhChien_2122110282.Controllers
{
    public class ListingLargeController : Controller
    {
        CSDLASPEntities2 objCSDLASPEntities2 = new CSDLASPEntities2();
        // GET: ListingLarge
        public ActionResult AllListingLarge(int Id)
        {
            var LstingLarge = objCSDLASPEntities2.Products.Where(n => n.CategoryId == Id).ToList();
            return View(LstingLarge);
        }
    }
}