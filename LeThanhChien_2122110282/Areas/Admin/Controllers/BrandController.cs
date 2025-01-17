using LeThanhChien_2122110282.Context;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LeThanhChien_2122110282.Areas.Admin.Controllers
{
    public class BrandController : Controller
    {
        CSDLASPEntities2 objCSDLASPEntities2 = new CSDLASPEntities2();
        public ActionResult BrandList(string currentFilter, string SearchString, int? page)
        {
            var lstBrand = new List<Brand>();
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
                lstBrand = objCSDLASPEntities2.Brands.Where(n => n.Name.Contains(SearchString)).ToList();
            }
            else
            {
                //lấy all sản phẩm trong bảng Brand
                lstBrand = objCSDLASPEntities2.Brands.ToList();
            }
            ViewBag.CurrentFilter = SearchString;
            //số lượng item của 1 trang = 4
            int pageSize = 4;
            int pageNumber = (page ?? 1);
            //sắp xếp theo id sản phẩm, sp mới đưa lên đầu
            lstBrand = lstBrand.OrderByDescending(n => n.Id).ToList();
            return View(lstBrand);
        }

        public ActionResult Details(int Id)
        {
            var objBrand = objCSDLASPEntities2.Brands.Where(n => n.Id == Id).FirstOrDefault();
            return View(objBrand);
        }
        [HttpGet]
        public ActionResult Delete(int id)
        {
            var objBranddelete = objCSDLASPEntities2.Brands.Where(n => n.Id == id).FirstOrDefault();
            return View(objBranddelete);
        }
        [HttpPost]
        public ActionResult Delete(Brand obj)
        {
            var objBranddelete = objCSDLASPEntities2.Brands.Where(n => n.Id == obj.Id).FirstOrDefault();
            objCSDLASPEntities2.Brands.Remove(objBranddelete);
            objCSDLASPEntities2.SaveChanges();
            return RedirectToAction("BrandList", "Brand");
        }
        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Create(Brand objBrand)
        {
            try
            {
                if (objBrand.ImageUpload != null)
                {
                    string fileName = Path.GetFileNameWithoutExtension(objBrand.ImageUpload.FileName);
                    string extension = Path.GetExtension(objBrand.ImageUpload.FileName);
                    fileName = fileName + extension + "_" + long.Parse(DateTime.Now.ToString("yyyyMMddhhmmss"));
                    objBrand.Avatar = fileName;
                    objBrand.ImageUpload.SaveAs(Path.Combine(Server.MapPath("~/Content/images/items/"), fileName));
                }
                objBrand.CreatedOnUtc = DateTime.UtcNow;
                objBrand.UpdatedOnUtc = DateTime.UtcNow;
                objCSDLASPEntities2.Brands.Add(objBrand);
                objCSDLASPEntities2.SaveChanges();
                return RedirectToAction("BrandList", "Brand");
            }
            catch (Exception)
            {
                return View();
            }
        }
        [HttpGet]
        public ActionResult Edit(int id)
        {
            var objBrandEdit = objCSDLASPEntities2.Brands.Where(n => n.Id == id).FirstOrDefault();
            return View(objBrandEdit);
        }
        [HttpPost]
        public ActionResult Edit(int id, Brand objEdit)
        {
            if (objEdit.ImageUpload != null)
            {
                string fileName = Path.GetFileNameWithoutExtension(objEdit.ImageUpload.FileName);
                string extension = Path.GetExtension(objEdit.ImageUpload.FileName);
                fileName = fileName + extension + "_" + long.Parse(DateTime.Now.ToString("yyyyMMddhhmmss"));
                objEdit.Avatar = fileName;
                objEdit.ImageUpload.SaveAs(Path.Combine(Server.MapPath("~/Content/images/items/"), fileName));
            }
            objEdit.UpdatedOnUtc = DateTime.UtcNow;
            objCSDLASPEntities2.Entry(objEdit).State = System.Data.Entity.EntityState.Modified;
            objCSDLASPEntities2.SaveChanges();
            return RedirectToAction("BrandList", "Brand");
        }
    }
}