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
using PagedList;

namespace ShareManagerEdgeMvc.Areas.ShareAdministration.Controllers
{
    public class ShareController : ShareManagerEdgeMvc.Controllers.BaseController
    {
        private ShareContext db = new ShareContext();

        // GET: /ShareAdministration/Share/
        public ActionResult Index(string sortOrder, string currentFilter, string searchString, int? page)
        {
            // get current user
            var user = HttpContext.User.Identity.Name.Substring(3);

            // protect from bad search string that causes issues
            if (searchString == "Throw") { searchString = null; }
            //Adding support for column sort ordering
            ViewBag.CurrentSort = sortOrder;
            ViewBag.NameSortParam = String.IsNullOrEmpty(sortOrder) ? "name" : "";

            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewBag.CurrentFileter = searchString;

            // default list of shares
            var cifsshares = db.CifsShares.Include(c => c.Ou).Where(s => (s.Status != Status.Retired));
            //cifsshares = cifsshares.Where(s => s.Status.Equals(Status.Retired));
            //if search string isn't empty, search cifsshares for the items
            if (!String.IsNullOrEmpty(searchString))
            {
                cifsshares = cifsshares.Where(s => s.Name.ToUpper().Contains(searchString.ToUpper()) ||
                    s.UncPath.ToUpper().Contains(searchString.ToUpper()));
            }

            // default to sorting by created on date 
            // else use descending name sort
            switch (sortOrder)
            {
                case "name":
                    cifsshares = cifsshares.OrderByDescending(s => s.Name);
                    break;
                default:
                    cifsshares = cifsshares.OrderBy(s => s.CreatedOnDateTime);
                    break;
            }

            //Adding paging support
            int pageSize = 15;
            int pageNumber = (page ?? 1);
            return View(cifsshares.ToPagedList(pageNumber, pageSize));

            //REMOVED to implement paging return View(cifsshares.ToList());
        }

        // GET: /ShareAdministration/Share/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CifsShare cifsshare = db.CifsShares.Find(id);
            if (cifsshare == null)
            {
                return HttpNotFound();
            }
            return View(cifsshare);
        }

        // GET: /ShareAdministration/Share/Create
        public ActionResult Create()
        {
            ViewBag.Status = EnumHelper.GetSelectList<Status>();
            ViewBag.OuID = new SelectList((from s in db.Ous.ToList()
                                           select new
                                           {
                                               OuID = s.ID,
                                               OrganizationalUnit = s.ResolverGroup + " - " + s.Domain
                                           }), "OuID", "OrganizationalUnit");


            ViewBag.ParentShareId = new SelectList((from s in db.CifsShares//.Where(j => j.ParentShareId == null)
                                                  select new
                                                  {
                                                      ParentShareId = s.CifsShareID,
                                                      UncPath = s.UncPath
                                                  }), "ParentShareId", "UncPath");

            return View();
        }

        // POST: /ShareAdministration/Share/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "CifsShareID,OuID,Name,CmdbCi,UncPath,ParentShareId,IsFsrShare,ShareOwnerFunctionalArea,ShareOwnerCostCenter,OwnerGroup,ReadOnlyGroup,ReadWriteGroup,NoChangeGroup,Status,CreatedOnDateTime,CreatedBy,ModifiedOnDateTime,ModifiedBy")] CifsShare cifsshare)
        {
            if (ModelState.IsValid)
            {
                var ou = db.Ous.Find(cifsshare.OuID);
                var user = HttpContext.User.Identity.Name.Substring(3);
                var now = System.DateTime.Now;
                cifsshare.CreatedBy = user;
                cifsshare.ModifiedBy = user;
                cifsshare.CreatedOnDateTime = now;
                cifsshare.ModifiedOnDateTime = now;
                if (String.IsNullOrEmpty(cifsshare.UncPath) ||
                    (! (AdHelper.IsGroupInOu(cifsshare.OwnerGroup, ou.Domain.ToString(), ou.OrganizationalUnit)
                     && (cifsshare.ReadOnlyGroup == null || AdHelper.IsGroupInOu(cifsshare.ReadOnlyGroup, ou.Domain.ToString(), ou.OrganizationalUnit))
                    && (cifsshare.ReadWriteGroup == null || AdHelper.IsGroupInOu(cifsshare.ReadWriteGroup, ou.Domain.ToString(), ou.OrganizationalUnit))
                    && (cifsshare.NoChangeGroup == null || AdHelper.IsGroupInOu(cifsshare.NoChangeGroup, ou.Domain.ToString(), ou.OrganizationalUnit))
                    )))
                {
                    cifsshare.Status = Status.OutOfService;
                }
                db.CifsShares.Add(cifsshare);
                var userId = HttpContext.User.Identity.Name.Substring(3);
                db.SaveChanges(userId);
                return RedirectToAction("Index");
            }

            //ViewBag.OuID = new SelectList(db.Ous, "ID", "OrganizationalUnit", cifsshare.OuID);
            ViewBag.Status = EnumHelper.GetSelectList<Status>();
            //ViewBag.OuID = new SelectList((from s in db.Ous.ToList()
            //                               select new
            //                               {
            //                                   OuID = s.ID,
            //                                   OrganizationalUnit = s.ResolverGroup + " - " + s.Domain
            //                               }), "OuID", "OrganizationalUnit");

            //ViewBag.ParentShareId = new SelectList(db.CifsShares, "ParentShareId", "UncPath", cifsshare.ParentShareId);
            //ViewBag.ParentShareList = new SelectList((from s in db.CifsShares.Where(j => j.ParentShareId == null)
            //                                          select new
            //                                          {
            //                                              CifsShareId = s.CifsShareID,
            //                                              UncPath = s.UncPath
            //                                          }), "CifsShareId", "UncPath");

            ViewBag.OuID = new SelectList((from s in db.Ous.ToList()
                                           select new
                                           {
                                               OuID = s.ID,
                                               OrganizationalUnit = s.ResolverGroup + " - " + s.Domain
                                           }), "OuID", "OrganizationalUnit");


            ViewBag.ParentShareId = new SelectList((from s in db.CifsShares//.Where(j => j.ParentShareId == null)
                                                    select new
                                                    {
                                                        ParentShareId = s.CifsShareID,
                                                        UncPath = s.UncPath
                                                    }), "ParentShareId", "UncPath");

            
            return View(cifsshare);


        }

        // GET: /ShareAdministration/Share/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CifsShare cifsshare = db.CifsShares.Find(id);
            if (cifsshare == null)
            {
                return HttpNotFound();
            }

            var user = HttpContext.User.Identity.Name.Substring(3);
            var auth = new AuthenticationHelper();

            ViewBag.isSiteAdmin = auth.IsSiteAdmin(user);

            var uncpath = cifsshare.UncPath;
            ViewData["UncPath"] = uncpath;
            var gkgroup = cifsshare.OwnerGroup;
            ViewData["OwnerGroup"] = gkgroup;
            var rogroup = cifsshare.ReadOnlyGroup;
            ViewData["ReadOnlyGroup"] = rogroup;
            var rwgroup = cifsshare.ReadWriteGroup;
            ViewData["ReadWriteGroup"] = rwgroup;
            var ncgroup = cifsshare.NoChangeGroup;
            ViewData["NoChangeGroup"] = ncgroup;
            ViewBag.Status = EnumHelper.GetSelectList<Status>(cifsshare.Status.ToString());
            //ViewBag.OuID = new SelectList((from s in db.Ous.ToList()
            //                               select new
            //                               {
            //                                   OuID = s.ID,
            //                                   OrganizationalUnit = s.ResolverGroup + " - " + s.Domain
            //                               }), "OuID", "OrganizationalUnit", cifsshare.OuID);

            //ViewBag.ParentShareList = new SelectList((from s in db.CifsShares.Where(j => j.ParentShareId == null)
            //                                select new
            //                                {
            //                                    CifsShareId = s.CifsShareID,
            //                                    UncPath = s.UncPath
            //                                }), "CifsShareId", "UncPath");

            ViewBag.OuID = new SelectList((from s in db.Ous.ToList()
                                           select new
                                           {
                                               OuID = s.ID,
                                               OrganizationalUnit = s.ResolverGroup + " - " + s.Domain
                                           }), "OuID", "OrganizationalUnit", cifsshare.OuID);


            ViewBag.ParentShareId = new SelectList((from s in db.CifsShares//.Where(j => j.ParentShareId == null)
                                                    select new
                                                    {
                                                        ParentShareId = s.CifsShareID,
                                                        UncPath = s.UncPath
                                                    }), "ParentShareId", "UncPath", cifsshare.ParentShareId);
            ViewBag.CurrentStat = cifsshare.Status;

            return View(cifsshare);
        }

        // POST: /ShareAdministration/Share/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include="CifsShareID,OuID,Name,CmdbCi,UncPath,ParentShareId,IsFsrShare,ShareOwnerFunctionalArea,ShareOwnerCostCenter,OwnerGroup,ReadOnlyGroup,ReadWriteGroup,NoChangeGroup,Status,CreatedOnDateTime,CreatedBy,ModifiedOnDateTime,ModifiedBy")] CifsShare cifsshare)
        {
            if (ModelState.IsValid)
            {
                var ou = db.Ous.Find(cifsshare.OuID);
                var userId = HttpContext.User.Identity.Name.Substring(3);
                //var ous = db.Ous.ToList();
                cifsshare.ModifiedBy = HttpContext.User.Identity.Name.Substring(3);
                cifsshare.ModifiedOnDateTime = System.DateTime.Now;
                if(cifsshare.Status == Status.InService)
                {
                    if ((!(String.IsNullOrEmpty(cifsshare.UncPath)))&&
                        AdHelper.IsGroupInOu(cifsshare.OwnerGroup, ou.Domain.ToString(), ou.OrganizationalUnit)
                        && (cifsshare.ReadOnlyGroup == null || AdHelper.IsGroupInOu(cifsshare.ReadOnlyGroup, ou.Domain.ToString(), ou.OrganizationalUnit))
                        && (cifsshare.ReadWriteGroup == null || AdHelper.IsGroupInOu(cifsshare.ReadWriteGroup, ou.Domain.ToString(), ou.OrganizationalUnit))
                        && (cifsshare.NoChangeGroup == null || AdHelper.IsGroupInOu(cifsshare.NoChangeGroup, ou.Domain.ToString(), ou.OrganizationalUnit))
                        )
                    {
                        db.Entry(cifsshare).State = EntityState.Modified;
                        
                        db.SaveChanges(userId);
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        cifsshare.Status = Status.OutOfService;
                    }
                }

                db.Entry(cifsshare).State = EntityState.Modified;
                //var userId = HttpContext.User.Identity.Name.Substring(3);
                db.SaveChanges(userId);
                return RedirectToAction("Index");
            }

            var uncpath = cifsshare.UncPath;
            ViewData["UncPath"] = uncpath;
            var gkgroup = cifsshare.OwnerGroup;
            ViewData["OwnerGroup"] = gkgroup;
            var rogroup = cifsshare.ReadOnlyGroup;
            ViewData["ReadOnlyGroup"] = rogroup;
            var rwgroup = cifsshare.ReadWriteGroup;
            ViewData["ReadWriteGroup"] = rwgroup;
            var ncgroup = cifsshare.NoChangeGroup;
            ViewData["NoChangeGroup"] = ncgroup;

            ViewBag.Status = EnumHelper.GetSelectList<Status>(cifsshare.Status.ToString());
            //ViewBag.OuID = new SelectList((from s in db.Ous.ToList()
            //                               select new
            //                               {
            //                                   OuID = s.ID,
            //                                   OrganizationalUnit = s.ResolverGroup + " - " + s.Domain
            //                               }), "OuID", "OrganizationalUnit", cifsshare.OuID);


            //ViewBag.ParentShareList = new SelectList((from s in db.CifsShares.Where(j => j.ParentShareId == null)
            //                                          select new
            //                                          {
            //                                              CifsShareId = s.CifsShareID,
            //                                              UncPath = s.UncPath
            //                                          }), "CifsShareId", "UncPath");

            ViewBag.OuID = new SelectList((from s in db.Ous.ToList()
                                           select new
                                           {
                                               OuID = s.ID,
                                               OrganizationalUnit = s.ResolverGroup + " - " + s.Domain
                                           }), "OuID", "OrganizationalUnit", cifsshare.OuID);


            ViewBag.ParentShareId = new SelectList((from s in db.CifsShares//.Where(j => j.ParentShareId == null)
                                                    select new
                                                    {
                                                        ParentShareId = s.CifsShareID,
                                                        UncPath = s.UncPath
                                                    }), "ParentShareId");//, "UncPath");
            ViewBag.ParentShareSelect = cifsshare.ParentShareId;

            return View(cifsshare);
        }

        // GET: /ShareAdministration/Share/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CifsShare cifsshare = db.CifsShares.Find(id);
            if (cifsshare == null)
            {
                return HttpNotFound();
            }
            return View(cifsshare);
        }

        // POST: /ShareAdministration/Share/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            CifsShare cifsshare = db.CifsShares.Find(id);
            db.CifsShares.Remove(cifsshare);
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
