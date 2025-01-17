using LeThanhChien_2122110282.Context;
using LeThanhChien_2122110282.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;


namespace LeThanhChien_2122110282.Areas.Admin.Controllers
{
    public class ProductController : Controller
    {

        CSDLASPEntities2 objCSDLASPEntities2 = new CSDLASPEntities2();
        public ActionResult ProductList(string currentFilter, string SearchString, int? page)
        {
            var lstProduct = new List<Product>();
            if (SearchString != null)
            {
                page = 1;
            }
            else
            {
                SearchString = currentFilter;
            }
            if (!string.IsNullOrEmpty(SearchString))
            {
                //lấy ds sản phẩm theo từ khóa tìm kiếm
                lstProduct = objCSDLASPEntities2.Products.Where(n => n.Name.Contains(SearchString)).ToList();
            }
            else
            {
                //lấy all sản phẩm trong bảng product
                lstProduct = objCSDLASPEntities2.Products.ToList();
            }
            ViewBag.CurrentFilter = SearchString;
            //số lượng item của 1 trang = 4
            int pageSize = 4;
            int pageNumber = (page ?? 1);
            //sắp xếp theo id sản phẩm, sp mới đưa lên đầu
            lstProduct = lstProduct.OrderByDescending(n => n.Id).ToList();
            return View(lstProduct);
        }

        public ActionResult Details(int Id)
        {
            var objProduct = objCSDLASPEntities2.Products.Where(n => n.Id == Id).FirstOrDefault();
            return View(objProduct);
        }
        [HttpGet]
        public ActionResult Delete(int id)
        {
            var objProductdelete = objCSDLASPEntities2.Products.Where(n => n.Id == id).FirstOrDefault();
            return View(objProductdelete);
        }
        [HttpPost]
        public ActionResult Delete(Product obj)
        {
            var objProductdelete = objCSDLASPEntities2.Products.Where(n => n.Id == obj.Id).FirstOrDefault();
            objCSDLASPEntities2.Products.Remove(objProductdelete);
            objCSDLASPEntities2.SaveChanges();
            return RedirectToAction("ProductList","Product");
        }
        [HttpGet]
        public ActionResult Create()
        {
            return View();

        }
        [HttpPost]
        public ActionResult Create(Product objProduct)
        {
            try
            {
                if(objProduct.ImageUpload != null)
                {
                    string fileName = Path.GetFileNameWithoutExtension(objProduct.ImageUpload.FileName);
                    string extension = Path.GetExtension(objProduct.ImageUpload.FileName);
                    fileName = fileName + extension + "_" + long.Parse(DateTime.Now.ToString("yyyyMMddhhmmss"));
                    objProduct.Avatar = fileName;
                    objProduct.ImageUpload.SaveAs(Path.Combine(Server.MapPath("~/Content/images/items/"), fileName));
                }
                objCSDLASPEntities2.Products.Add(objProduct);
                objCSDLASPEntities2.SaveChanges();
                return RedirectToAction("ProductList", "Product");
            }
            catch (Exception)
            {
                return View();
            }
        }
        [HttpGet]
        public ActionResult Edit(int id)
        {
            var objProductEdit = objCSDLASPEntities2.Products.Where(n => n.Id == id).FirstOrDefault();
            return View(objProductEdit);
        }
        [HttpPost]
        public ActionResult Edit(int id, Product objEdit)
        {
            if (objEdit.ImageUpload != null)
            {
                string fileName = Path.GetFileNameWithoutExtension(objEdit.ImageUpload.FileName);
                string extension = Path.GetExtension(objEdit.ImageUpload.FileName);
                fileName = fileName + extension + "_" + long.Parse(DateTime.Now.ToString("yyyyMMddhhmmss"));
                objEdit.Avatar = fileName;
                objEdit.ImageUpload.SaveAs(Path.Combine(Server.MapPath("~/Content/images/items/"), fileName));
            }
            objCSDLASPEntities2.Entry(objEdit).State = System.Data.Entity.EntityState.Modified;
            objCSDLASPEntities2.SaveChanges();
            return RedirectToAction("ProductList", "Product");
        }

        public JsonResult GetProducts(string searchString, int page = 1, int pageSize = 10)
        {
            var productsQuery = objCSDLASPEntities2.Products.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                productsQuery = productsQuery.Where(p => p.Name.Contains(searchString));
            }

            var items = productsQuery
                            .OrderBy(p => p.Id)
                            .Skip((page - 1) * pageSize)
                            .Take(pageSize)
                            .ToList();

            var totalItems = productsQuery.Count();

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