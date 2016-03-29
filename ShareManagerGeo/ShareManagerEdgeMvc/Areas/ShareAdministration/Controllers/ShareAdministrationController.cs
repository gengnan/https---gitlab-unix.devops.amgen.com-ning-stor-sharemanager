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
    public class ShareAdministrationController : ShareManagerEdgeMvc.Controllers.BaseController
    {
        private ShareContext db = new ShareContext();

        // GET: /ShareAdministration/ShareAdministration/
        // sortOrder is for doing naming sorting
        // searchString is for implementing filtering for searches
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
            var cifsshares = db.CifsShares.Include(c => c.Ou);

            //if search string isn't empty, search cifsshares for the items
            if (!String.IsNullOrEmpty(searchString))
            {
                cifsshares = cifsshares.Where(s => s.Name.ToUpper().Contains(searchString.ToUpper())
                || s.UncPath.ToUpper().Contains(searchString.ToUpper())
                || s.ShareOwnerFunctionalArea.ToUpper().Contains(searchString.ToUpper())
                || s.ShareOwnerCostCenter.ToUpper().Contains(searchString.ToUpper())
                || s.CmdbCi.ToUpper().Contains(searchString.ToUpper())
                || s.OwnerGroup.ToUpper().Contains(searchString.ToUpper())
                || s.ReadOnlyGroup.ToUpper().Contains(searchString.ToUpper())
                || s.ReadWriteGroup.ToUpper().Contains(searchString.ToUpper())
                || s.NoChangeGroup.ToUpper().Contains(searchString.ToUpper()));
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

        // GET: /ShareAdministration/ShareAdministration/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CifsShare cifsshare = db.CifsShares.Find(id);

            // Get's the OU information for the CifsShare that's identified by the parameter
            ViewBag.SelectedOu = from s in db.Ous.ToList()
                                     where s.ID == cifsshare.OuID
                                     select s;
                                     
            

            if (cifsshare == null)
            {
                return HttpNotFound();
            }
            return View(cifsshare);
        }

        // GET: /ShareAdministration/ShareAdministration/Create
        public ActionResult Create()
        {
            var Statuses = EnumHelper.GetSelectList<Status>();
            ViewBag.Status = Statuses;
            //ViewBag.OuID = new SelectList(db.Ous, "ID", "OrganizationalUnit");
            ViewBag.OuID = new SelectList((from s in db.Ous.ToList() select new {
                OuID = s.ID,
                OrganizationalUnit = s.ResolverGroup + " - " + s.Domain
            }), "OuID", "OrganizationalUnit", null);
            return View();
        }

        // POST: /ShareAdministration/ShareAdministration/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include="CifsShareID,OuID,Name,CmdbCi,UncPath,ShareOwnerFunctionalArea,ShareOwnerCostCenter,OwnerGroup,ReadOnlyGroup,ReadWriteGroup,NoChangeGroup,Status,CreateOnDateTime,CreatedBy,ModifiedOnDateTime,ModifiedBy")] CifsShare cifsshare)
        {
            if (ModelState.IsValid)
            {
                //TODO trim all parameters

                //Set Create and Modified Informations
                var timestamp = System.DateTime.Now;
                var userAlias = HttpContext.User.Identity.Name;
                cifsshare.CreatedBy = userAlias;
                cifsshare.ModifiedBy = userAlias;
                cifsshare.CreatedOnDateTime = timestamp;
                cifsshare.ModifiedOnDateTime = timestamp;
                db.CifsShares.Add(cifsshare);
                var userId = HttpContext.User.Identity.Name.Substring(3);
                db.SaveChanges(userId);
                return RedirectToAction("Index");
            }
            
            
            return View(cifsshare);
        }

        // GET: /ShareAdministration/ShareAdministration/Edit/5
        public ActionResult Edit(int? id)
        {
            CifsShareViewModel ViewModel = new CifsShareViewModel();
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CifsShare cifsshare = db.CifsShares.Find(id);
            if (cifsshare == null)
            {
                return HttpNotFound();
            }

            
            //REMOVED - DEFAULT ITEM ViewBag.OuID = new SelectList(db.Ous, "ID", "OrganizationalUnit", cifsshare.OuID);
            
            // get viewbag for OU drop down list that uses Resolver Group and Domain to identify the OU to use for a Share
            ViewBag.OuID = new SelectList((from s in db.Ous.ToList() 
                                           select new
                                           {
                                               OuID = s.ID,
                                               OrganizationalUnit = s.ResolverGroup + " - " + s.Domain
                                           }), "OuID", "OrganizationalUnit", cifsshare.OuID);


            
            //var StatusList = EnumHelper.GetSelectList<Status>(cifsshare.Status.ToString());
            //ViewBag.Status = StatusList;
            return View(cifsshare);
        }

        // POST: /ShareAdministration/ShareAdministration/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include="CifsShareID,OuID,Name,CmdbCi,UncPath,ShareOwnerFunctionalArea,ShareOwnerCostCenter,OwnerGroup,ReadOnlyGroup,ReadWriteGroup,NoChangeGroup,Status")] CifsShare cifsshare)
        {
            if (ModelState.IsValid)
            {
                //TODO trim all parameters
                var userAlias = HttpContext.User.Identity.Name;
                cifsshare.ModifiedBy = userAlias;
                cifsshare.ModifiedOnDateTime = DateTime.Now;

                
                db.Entry(cifsshare).State = EntityState.Modified;
                var userId = HttpContext.User.Identity.Name.Substring(3);
                db.SaveChanges(userId);
                return RedirectToAction("Index");
            }
            ViewBag.OuID = new SelectList((from s in db.Ous.ToList()
                                           select new
                                           {
                                               OuID = s.ID,
                                               OrganizationalUnit = s.ResolverGroup + " - " + s.Domain
                                           }), "OuID", "OrganizationalUnit", cifsshare.OuID);
           

            return View(cifsshare);
        }

        // GET: /ShareAdministration/ShareAdministration/Delete/5
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

        // POST: /ShareAdministration/ShareAdministration/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            // Turning off implementation at this time
            //CifsShare cifsshare = db.CifsShares.Find(id);
            //db.CifsShares.Remove(cifsshare);
            //db.SaveChanges();
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
