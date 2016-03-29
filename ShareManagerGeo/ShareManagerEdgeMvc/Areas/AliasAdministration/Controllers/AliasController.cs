using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ShareManagerEdgeMvc.DAL;
using ShareManagerEdgeMvc.Models;
using ShareManagerEdgeMvc.Helpers;
using PagedList;

namespace ShareManagerEdgeMvc.Areas.AliasAdministration.Controllers
{
    public class AliasController : Controller
    {
        private ShareContext db = new ShareContext();

        // GET: AliasAdministration/Alias
        public ActionResult Index(string sortOrder, string currentFilter, string searchString, int? page)
        {
            ViewBag.CurrentSort = sortOrder;
            ViewBag.PrimarySortParm = sortOrder == "primary" ? "primary_desc" : "primary";
            ViewBag.AliasSortParm = sortOrder == "alias" ? "alias_desc" : "alias";

            List<CifsShareAlias> cifssharealiases = db.CifsShareAliases.ToList();

            switch (sortOrder)
            {
                case "primary_desc":
                    cifssharealiases = cifssharealiases.OrderByDescending(c => c.PrimaryCifsShareID).ToList();
                    break;
                case "alias_desc":
                    cifssharealiases = cifssharealiases.OrderByDescending(c => c.SecondaryCifsShareID).ToList();
                    break;
                case "primary":
                    cifssharealiases = cifssharealiases.OrderBy(c => c.PrimaryCifsShareID).ToList();
                    break;
                case "alias":
                    cifssharealiases = cifssharealiases.OrderBy(c => c.SecondaryCifsShareID).ToList();
                    break;
                default:
                    cifssharealiases = cifssharealiases.OrderBy(c => c.CifsShareAliasPairID).ToList();
                    break;
            }

            int pageSize = 50;
            int pageNumber = (page ?? 1);
            return View(cifssharealiases.ToPagedList(pageNumber, pageSize));
            

        }

        // GET: AliasAdministration/Alias/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CifsShareAlias cifsShareAlias = db.CifsShareAliases.Find(id);
            if (cifsShareAlias == null)
            {
                return HttpNotFound();
            }
            return View(cifsShareAlias);
        }

        // GET: AliasAdministration/Alias/Create
        public ActionResult Create()
        {
            ViewBag.PrimaryCifsShareID = new SelectList((from s in db.CifsShares//.Where(j => j.ParentShareId == null)
                                                    select new
                                                    {
                                                        CifsShareId = s.CifsShareID,
                                                        UncPath = s.UncPath
                                                    }), "CifsShareId", "UncPath");

            ViewBag.SecondaryCifsShareID = new SelectList((from s in db.CifsShares//.Where(j => j.ParentShareId == null)
                                                         select new
                                                         {
                                                             CifsShareId = s.CifsShareID,
                                                             UncPath = s.UncPath
                                                         }), "CifsShareId", "UncPath");
            
            return View();
        }

        // POST: AliasAdministration/Alias/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "CifsShareAliasPairID,PrimaryCifsShareID,SecondaryCifsShareID,CreatedOnDateTime,CreatedBy,ModifiedOnDateTime,ModifiedBy")] CifsShareAlias cifsShareAlias)
        {
            if (cifsShareAlias.PrimaryCifsShareID == cifsShareAlias.SecondaryCifsShareID)
            {
                ModelState.AddModelError("SecondaryCifsShareID", "Alias cannot be the same as primary share.");
            }
            
            if (ModelState.IsValid)
            {
                var userId = HttpContext.User.Identity.Name.Substring(3);
                var now = System.DateTime.Now;
                cifsShareAlias.CreatedBy = userId;
                cifsShareAlias.ModifiedBy = userId;
                cifsShareAlias.CreatedOnDateTime = now;
                cifsShareAlias.ModifiedOnDateTime = now;
                
                
                db.CifsShareAliases.Add(cifsShareAlias);
                
                db.SaveChanges(userId);
                return RedirectToAction("Index");
            }
            ViewBag.PrimaryCifsShareID = new SelectList((from s in db.CifsShares//.Where(j => j.ParentShareId == null)
                                                         select new
                                                         {
                                                             CifsShareId = s.CifsShareID,
                                                             UncPath = s.UncPath
                                                         }), "CifsShareId", "UncPath");

            ViewBag.SecondaryCifsShareID = new SelectList((from s in db.CifsShares//.Where(j => j.ParentShareId == null)
                                                           select new
                                                           {
                                                               CifsShareId = s.CifsShareID,
                                                               UncPath = s.UncPath
                                                           }), "CifsShareId", "UncPath");
            return View(cifsShareAlias);
        }

        // GET: AliasAdministration/Alias/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CifsShareAlias cifsShareAlias = db.CifsShareAliases.Find(id);
            if (cifsShareAlias == null)
            {
                return HttpNotFound();
            }

            ViewBag.PrimaryCifsShareID = new SelectList((from s in db.CifsShares//.Where(j => j.ParentShareId == null)
                                                         select new
                                                         {
                                                             CifsShareId = s.CifsShareID,
                                                             UncPath = s.UncPath
                                                         }), "CifsShareId", "UncPath", cifsShareAlias.PrimaryCifsShareID);

            ViewBag.SecondaryCifsShareID = new SelectList((from s in db.CifsShares//.Where(j => j.ParentShareId == null)
                                                           select new
                                                           {
                                                               CifsShareId = s.CifsShareID,
                                                               UncPath = s.UncPath
                                                           }), "CifsShareId", "UncPath", cifsShareAlias.SecondaryCifsShareID);
            return View(cifsShareAlias);
        }

        // POST: AliasAdministration/Alias/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "CifsShareAliasPairID,PrimaryCifsShareID,SecondaryCifsShareID,CreatedOnDateTime,CreatedBy,ModifiedOnDateTime,ModifiedBy")] CifsShareAlias cifsShareAlias)
        {
            if (cifsShareAlias.PrimaryCifsShareID == cifsShareAlias.SecondaryCifsShareID)
            {
                ModelState.AddModelError("SecondaryCifsShareID", "Alias cannot be the same as primary share.");
            }
            
            if (ModelState.IsValid)
            {
                var userId = HttpContext.User.Identity.Name.Substring(3);
                var now = System.DateTime.Now;
                cifsShareAlias.ModifiedBy = userId;
                cifsShareAlias.ModifiedOnDateTime = now;
                
                db.Entry(cifsShareAlias).State = EntityState.Modified;

                db.SaveChanges(userId);
                return RedirectToAction("Index");
            }
            ViewBag.PrimaryCifsShareID = new SelectList((from s in db.CifsShares//.Where(j => j.ParentShareId == null)
                                                         select new
                                                         {
                                                             CifsShareId = s.CifsShareID,
                                                             UncPath = s.UncPath
                                                         }), "CifsShareId", "UncPath", cifsShareAlias.PrimaryCifsShareID);

            ViewBag.SecondaryCifsShareID = new SelectList((from s in db.CifsShares//.Where(j => j.ParentShareId == null)
                                                           select new
                                                           {
                                                               CifsShareId = s.CifsShareID,
                                                               UncPath = s.UncPath
                                                           }), "CifsShareId", "UncPath", cifsShareAlias.SecondaryCifsShareID);
 
            return View(cifsShareAlias);
        }

        // GET: AliasAdministration/Alias/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CifsShareAlias cifsShareAlias = db.CifsShareAliases.Find(id);
            if (cifsShareAlias == null)
            {
                return HttpNotFound();
            }
            return View(cifsShareAlias);
        }

        // POST: AliasAdministration/Alias/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            CifsShareAlias cifsShareAlias = db.CifsShareAliases.Find(id);
            db.CifsShareAliases.Remove(cifsShareAlias);
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
