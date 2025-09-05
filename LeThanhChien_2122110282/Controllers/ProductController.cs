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
        public ActionResult Detail(int id)
        {
            var product = objCSDLASPEntities2.Products.FirstOrDefault(p => p.Id == id);
            if (product == null)
            {
                return HttpNotFound();
            }

            // Calculate the top 8 discounted products
            var discountedProductIds = objCSDLASPEntities2.Products
                .Where(p => p.PriceDiscount.HasValue && p.PriceDiscount < p.Price)
                .OrderByDescending(p => (p.Price - p.PriceDiscount) / p.Price)
                .Take(8)
                .Select(p => p.Id)
                .ToList();

            ViewBag.DiscountedProductIds = discountedProductIds;

            return View(product);
        }
        public JsonResult GetProductDetails(int id)
        {
            var product = objCSDLASPEntities2.Products
                .Where(p => p.Id == id)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Avatar,
                    p.Price,
                    p.PriceDiscount,
                    p.FullDescription
                })
                .FirstOrDefault();

            if (product == null)
            {
                return Json(new { success = false, message = "Product not found" }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { success = true, data = product }, JsonRequestBehavior.AllowGet);
        }
    }
}