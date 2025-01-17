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
            HomeModel objHomeModel = new HomeModel();
            objHomeModel.ListCategory = objCSDLASPEntities2.Categories.ToList();
            objHomeModel.ListProduct = objCSDLASPEntities2.Products.ToList();
            return View(objHomeModel);
        }
    }
}