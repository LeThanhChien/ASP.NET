﻿using LeThanhChien_2122110282.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LeThanhChien_2122110282.Controllers
{
    public class CategoryController : Controller
    {
        CSDLASPEntities2 objCSDLASPEntities2 = new CSDLASPEntities2();
        // GET: Category
        public ActionResult AllCategory()
        {
            var lstCategory = objCSDLASPEntities2.Categories.ToList();
            return View(lstCategory);
        }
    }
}