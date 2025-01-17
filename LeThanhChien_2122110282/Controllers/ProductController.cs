using LeThanhChien_2122110282.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LeThanhChien_2122110282.Controllers
{
    public class ProductController : Controller
    {
        CSDLASPEntities2 objCSDLASPEntities2 = new CSDLASPEntities2();
        // GET: Product
        public ActionResult Detail(int Id)
        {
            var objProduct = objCSDLASPEntities2.Products.Where(n=>n.Id == Id).FirstOrDefault();
            return View(objProduct);
        }
    }
}