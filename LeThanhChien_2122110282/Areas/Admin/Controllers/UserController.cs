using LeThanhChien_2122110282.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LeThanhChien_2122110282.Areas.Admin.Controllers
{
    public class UserController : Controller
    {
        CSDLASPEntities2 objCSDLASPEntities2 = new CSDLASPEntities2();
        // GET: Admin/User
        public ActionResult UserList()
        {
            var objUser = objCSDLASPEntities2.Users.ToList();
            return View(objUser);
        }
        public ActionResult Details(int Id)
        {
            var objUser = objCSDLASPEntities2.Users.Where(n => n.Id == Id).FirstOrDefault();
            return View(objUser);
        }
        [HttpGet]
        public ActionResult Delete(int id)
        {
            var objUserdelete = objCSDLASPEntities2.Users.Where(n => n.Id == id).FirstOrDefault();
            return View(objUserdelete);
        }
        [HttpPost]
        public ActionResult Delete(User obj)
        {
            var objUserdelete = objCSDLASPEntities2.Users.Where(n => n.Id == obj.Id).FirstOrDefault();
            objCSDLASPEntities2.Users.Remove(objUserdelete);
            objCSDLASPEntities2.SaveChanges();
            return RedirectToAction("UserList", "User");
        }
        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Create(User objUser)
        {
            try
            {

                objCSDLASPEntities2.Users.Add(objUser);
                objCSDLASPEntities2.SaveChanges();
                return RedirectToAction("UserList", "User");
            }
            catch (Exception)
            {
                return View();
            }
        }
        [HttpGet]
        public ActionResult Edit(int id)
        {
            var objUserEdit = objCSDLASPEntities2.Users.Where(n => n.Id == id).FirstOrDefault();
            return View(objUserEdit);
        }
        [HttpPost]
        public ActionResult Edit(int id, User objEdit)
        {

            objCSDLASPEntities2.Entry(objEdit).State = System.Data.Entity.EntityState.Modified;
            objCSDLASPEntities2.SaveChanges();
            return RedirectToAction("UserList", "User");
        }

    }
}