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

namespace ShareManagerEdgeMvc.Areas.SiteAdministration.Controllers
{
    public class OuController : ShareManagerEdgeMvc.Controllers.BaseController
    {
        private ShareContext db = new ShareContext();

        // GET: /SiteAdministration/Ou/
        public ActionResult Index()
        {
            return View(db.Ous.ToList());
        }

        // GET: /SiteAdministration/Ou/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ou ou = db.Ous.Find(id);
            if (ou == null)
            {
                return HttpNotFound();
            }
            return View(ou);
        }

        // GET: /SiteAdministration/Ou/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: /SiteAdministration/Ou/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include="ID,Domain,OrganizationalUnit,ResolverGroup")] Ou ou)
        {
            if (ModelState.IsValid)
            {
                db.Ous.Add(ou);
                var userId = HttpContext.User.Identity.Name.Substring(3);
                db.SaveChanges(userId);
                return RedirectToAction("Index");
            }

            return View(ou);
        }

        // GET: /SiteAdministration/Ou/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ou ou = db.Ous.Find(id);
            if (ou == null)
            {
                return HttpNotFound();
            }
            return View(ou);
        }

        // POST: /SiteAdministration/Ou/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include="ID,Domain,OrganizationalUnit,ResolverGroup")] Ou ou)
        {
            if (ModelState.IsValid)
            {
                db.Entry(ou).State = EntityState.Modified;
                var userId = HttpContext.User.Identity.Name.Substring(3);
                db.SaveChanges(userId);
                return RedirectToAction("Index");
            }
            return View(ou);
        }

        // GET: /SiteAdministration/Ou/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ou ou = db.Ous.Find(id);
            if (ou == null)
            {
                return HttpNotFound();
            }
            return View(ou);
        }

        // POST: /SiteAdministration/Ou/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Ou ou = db.Ous.Find(id);
            db.Ous.Remove(ou);
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
