using LeThanhChien_2122110282.Context;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LeThanhChien_2122110282.Areas.Admin.Controllers
{
    public class CategoryController : Controller
    {
        CSDLASPEntities2 objCSDLASPEntities2 = new CSDLASPEntities2();
        // GET: Admin/Category
        public ActionResult CategoryList()
        {
            var category = objCSDLASPEntities2.Categories.ToList();
            return View(category);
        }
        public ActionResult Details(int Id)
        {
            var objCategory = objCSDLASPEntities2.Categories.Where(n => n.Id == Id).FirstOrDefault();
            return View(objCategory);
        }
        [HttpGet]
        public ActionResult Delete(int id)
        {
            var objCategorydelete = objCSDLASPEntities2.Categories.Where(n => n.Id == id).FirstOrDefault();
            return View(objCategorydelete);
        }
        [HttpPost]
        public ActionResult Delete(Category obj)
        {
            var objCategorydelete = objCSDLASPEntities2.Categories.Where(n => n.Id == obj.Id).FirstOrDefault();
            objCSDLASPEntities2.Categories.Remove(objCategorydelete);
            objCSDLASPEntities2.SaveChanges();
            return RedirectToAction("CategoryList", "Category");
        }
        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Create(Category objCategory)
        {
            try
            {
                if (objCategory.ImageUpload != null)
                {
                    string fileName = Path.GetFileNameWithoutExtension(objCategory.ImageUpload.FileName);
                    string extension = Path.GetExtension(objCategory.ImageUpload.FileName);
                    fileName = fileName + extension + "_" + long.Parse(DateTime.Now.ToString("yyyyMMddhhmmss"));
                    objCategory.Avatar = fileName;
                    objCategory.ImageUpload.SaveAs(Path.Combine(Server.MapPath("~/Content/images/"), fileName));
                }
                objCategory.CreatedOnUtc = DateTime.UtcNow; 
                objCategory.UpdatedOnUtc = DateTime.UtcNow;
                objCSDLASPEntities2.Categories.Add(objCategory);
                objCSDLASPEntities2.SaveChanges();
                return RedirectToAction("CategoryList", "Category");
            }
            catch (Exception)
            {
                return View();
            }
        }
        [HttpGet]
        public ActionResult Edit(int id)
        {
            var objCategoryEdit = objCSDLASPEntities2.Categories.Where(n => n.Id == id).FirstOrDefault();
            return View(objCategoryEdit);
        }
        [HttpPost]
        public ActionResult Edit(int id, Category objEdit)
        {
            if (objEdit.ImageUpload != null)
            {
                string fileName = Path.GetFileNameWithoutExtension(objEdit.ImageUpload.FileName);
                string extension = Path.GetExtension(objEdit.ImageUpload.FileName);
                fileName = fileName + extension + "_" + long.Parse(DateTime.Now.ToString("yyyyMMddhhmmss"));
                objEdit.Avatar = fileName;
                objEdit.ImageUpload.SaveAs(Path.Combine(Server.MapPath("~/Content/images/"), fileName));
            }
            objEdit.UpdatedOnUtc = DateTime.UtcNow;
            objCSDLASPEntities2.Entry(objEdit).State = System.Data.Entity.EntityState.Modified;
            objCSDLASPEntities2.SaveChanges();
            return RedirectToAction("CategoryList", "Category");
        }
    }
}