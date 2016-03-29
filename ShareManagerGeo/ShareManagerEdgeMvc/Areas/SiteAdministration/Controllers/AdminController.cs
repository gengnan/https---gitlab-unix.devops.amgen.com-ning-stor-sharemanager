using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ShareManagerEdgeMvc.Models;
using ShareManagerEdgeMvc.DAL;
using ShareManagerEdgeMvc.Helpers;

namespace ShareManagerEdgeMvc.Areas.SiteAdministration.Controllers
{
    public class AdminController : ShareManagerEdgeMvc.Controllers.BaseController
    {
        private ShareContext db = new ShareContext();

        // GET: /SiteAdministration/Admin/
        public ActionResult Index()
        {
            var admins = db.Administrators.ToList();
            
            return View(admins);
        }

        // GET: /SiteAdministration/Admin/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Administrator administrator = db.Administrators.Find(id);
            if (administrator == null)
            {
                return HttpNotFound();
            }
            return View(administrator);
        }

        // GET: /SiteAdministration/Admin/Create
        public ActionResult Create()
        {
            ViewBag.AdminType = EnumHelper.GetSelectList<AdminType>();
            return View();
        }

        // POST: /SiteAdministration/Admin/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include="ID,UserName,UserAlias,AdminType")] Administrator administrator)
        {
            if (ModelState.IsValid)
            {
                db.Administrators.Add(administrator);
                var userId = HttpContext.User.Identity.Name.Substring(3);
                db.SaveChanges(userId);
                return RedirectToAction("Index");
            }

            return View(administrator);
        }

        // GET: /SiteAdministration/Admin/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Administrator administrator = db.Administrators.Find(id);
            var Statuses = EnumHelper.GetSelectList<AdminType>();
            ViewBag.AdminType = Statuses;

            if (administrator == null)
            {
                return HttpNotFound();
            }
            return View(administrator);
        }

        // POST: /SiteAdministration/Admin/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include="ID,UserName,UserAlias,AdminType")] Administrator administrator)
        {
            

            if (ModelState.IsValid)
            {
                db.Entry(administrator).State = EntityState.Modified;
                var userId = HttpContext.User.Identity.Name.Substring(3);
                db.SaveChanges(userId);
                return RedirectToAction("Index");
            }
            return View(administrator);
        }

        // GET: /SiteAdministration/Admin/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Administrator administrator = db.Administrators.Find(id);
            if (administrator == null)
            {
                return HttpNotFound();
            }
            return View(administrator);
        }

        // POST: /SiteAdministration/Admin/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Administrator administrator = db.Administrators.Find(id);
            db.Administrators.Remove(administrator);
            var userId = HttpContext.User.Identity.Name.Substring(3);
            db.SaveChanges(userId);
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
