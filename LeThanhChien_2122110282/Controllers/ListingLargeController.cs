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
        public ActionResult AllListingLarge(int? Id)
        {
            List<Product> products;

            if (Id.HasValue)
            {
                // Filter products by category if Id is provided
                products = objCSDLASPEntities2.Products
                    .Where(p => p.CategoryId == Id)
                    .ToList();
            }
            else
            {
                // Get all products if no category Id is provided
                products = objCSDLASPEntities2.Products.ToList();
            }

            // Calculate the top 8 discounted products
            var discountedProductIds = objCSDLASPEntities2.Products
                .Where(p => p.PriceDiscount.HasValue && p.PriceDiscount < p.Price)
                .OrderByDescending(p => (p.Price - p.PriceDiscount) / p.Price)
                .Take(8)
                .Select(p => p.Id)
                .ToList();

            ViewBag.DiscountedProductIds = discountedProductIds;

            return View(products);
        }
    }
}