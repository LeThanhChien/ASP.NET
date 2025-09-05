using LeThanhChien_2122110282.Context;
using LeThanhChien_2122110282.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LeThanhChien_2122110282.Controllers
{
    public class ListingGridController : Controller
    {
        CSDLASPEntities2 objCSDLASPEntities2 = new CSDLASPEntities2();
        // GET: ListingGrid
        public ActionResult AllListingGrid()
        {
            var model = new HomeModel
            {
                ListCategory = objCSDLASPEntities2.Categories.ToList(),
                ListProduct = objCSDLASPEntities2.Products.ToList()
            };

            // Calculate the top 8 discounted products
            var discountedProductIds = objCSDLASPEntities2.Products
                .Where(p => p.PriceDiscount.HasValue && p.PriceDiscount < p.Price)
                .OrderByDescending(p => (p.Price - p.PriceDiscount) / p.Price)
                .Take(8)
                .Select(p => p.Id)
                .ToList();

            ViewBag.DiscountedProductIds = discountedProductIds;

            return View(model);
        }
    }
}