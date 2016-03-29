using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ShareManagerEdgeMvc.Models;
using ShareManagerEdgeMvc.Helpers;
using ShareManagerEdgeMvc.DAL;
using PagedList;

namespace ShareManagerEdgeMvc.Areas.user.Controllers
{
    public class SmbSharesController : ShareManagerEdgeMvc.Controllers.BaseController
    {
        private ShareContext db = new ShareContext();

        // GET: /user/SmbShares/
        public ActionResult Index(string sortOrder, string currentFilter, string searchString, int? page)
        {
            ViewBag.CurrentSort = sortOrder;
            ViewBag.NameSortParm = sortOrder == "name" ? "name_desc" : "name";
            ViewBag.UncSortParm = sortOrder == "unc" ? "unc_desc" : "unc";
            ViewBag.FuncSortParm = sortOrder == "func" ? "func_desc" : "func";
            ViewBag.CCSortParm = sortOrder == "cc" ? "cc_desc" : "cc";
            ViewBag.DomainSortParm = sortOrder == "domain" ? "domain_desc" : "domain";

            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewBag.CurrentFilter = searchString;

            var cifsshares = db.CifsShares.Where(c => c.Status == Status.InService).Include(c => c.Ou);

            if(!String.IsNullOrEmpty(searchString))
            {
                cifsshares = cifsshares.Where(s => s.Name.ToUpper().Contains(searchString.ToUpper())
                    || s.UncPath.ToUpper().Contains(searchString.ToUpper())
                    || s.ShareOwnerFunctionalArea.ToUpper().Contains(searchString.ToUpper())
                    || s.ShareOwnerCostCenter.ToUpper().Contains(searchString.ToUpper()));
            }

            switch(sortOrder)
            {
                case "name_desc":
                    cifsshares = cifsshares.OrderByDescending(c => c.Name);
                    break;
                case "unc_desc":
                    cifsshares = cifsshares.OrderByDescending(c => c.UncPath);
                    break;
                case "func_desc":
                    cifsshares = cifsshares.OrderByDescending(c => c.ShareOwnerFunctionalArea);
                    break;
                case "cc_desc":
                    cifsshares = cifsshares.OrderByDescending(c => c.ShareOwnerCostCenter);
                    break;
                case "domain_desc":
                    cifsshares = cifsshares.OrderByDescending(c => c.Ou.Domain);
                    break;
                case "name":
                    cifsshares = cifsshares.OrderBy(c => c.Name);
                    break;
                case "unc":
                    cifsshares = cifsshares.OrderBy(c => c.UncPath);
                    break;
                case "func":
                    cifsshares = cifsshares.OrderBy(c => c.ShareOwnerFunctionalArea);
                    break;
                case "cc":
                    cifsshares = cifsshares.OrderBy(c => c.ShareOwnerCostCenter);
                    break;
                case "domain":
                    cifsshares = cifsshares.OrderBy(c => c.Ou.Domain);
                    break;
                default:
                    cifsshares = cifsshares.OrderBy(c => c.CreatedOnDateTime);
                    break;
            }

            int pageSize = 10;
            int pageNumber = (page ?? 1);
            return View(cifsshares.ToPagedList(pageNumber, pageSize));
        }

        // GET: /user/SmbShares/Details/5
        public ActionResult Details(int? id)
        {
            var auth = new AuthenticationHelper();
                        
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CifsShare cifsshare = db.CifsShares.Find(id);

            var user = AdHelper.GetAdUser(HttpContext.User.Identity.Name.Substring(3));
            
            List<CifsPermissionRequest> openrequests = db.CifsPermissionRequests.Where(c => c.CifsShareID == id && c.RequestStatus.Value == RequestStatus.Open).ToList();
            List<CifsPermissionRequest> closedrequests = db.CifsPermissionRequests.Where(c => c.CifsShareID == id && (c.RequestStatus.Value == RequestStatus.Completed ||
                c.RequestStatus.Value == RequestStatus.Failed)).OrderByDescending(c => c.RequestClosedOnDateTime).ToList();

            List<CifsPermissionRequest> openrequestsAlias = db.CifsPermissionRequests.Where(c => c.CifsShareID == id && c.RequestStatus.Value == RequestStatus.Open).ToList();
            List<CifsShareAlias> shareAliases = db.CifsShareAliases.Where(c => c.PrimaryCifsShareID == id).ToList();

            List<CifsPermissionRequest> aliasOpenrequests = new List<CifsPermissionRequest>();
            List<CifsPermissionRequest> aliasClosedrequests = new List<CifsPermissionRequest>();

            foreach (CifsShareAlias alias in shareAliases)
            {
                aliasOpenrequests = db.CifsPermissionRequests.Where(c => c.CifsShareID == alias.SecondaryCifsShareID && c.RequestStatus.Value == RequestStatus.Open).ToList();
                aliasClosedrequests = db.CifsPermissionRequests.Where(c => c.CifsShareID == alias.SecondaryCifsShareID && (c.RequestStatus.Value == RequestStatus.Completed ||
                    c.RequestStatus.Value == RequestStatus.Failed)).OrderByDescending(c => c.RequestClosedOnDateTime).ToList();
            }

            openrequests.AddRange(aliasOpenrequests);
            closedrequests.AddRange(aliasClosedrequests);

            
            //List<CifsPermissionRequest> closedrequestsAlias = db.CifsPermissionRequests.Where(c => c.CifsShareID == id && (c.RequestStatus.Value == RequestStatus.Completed ||
               // c.RequestStatus.Value == RequestStatus.Failed)).OrderByDescending(c => c.RequestClosedOnDateTime).ToList();

            //closedrequests = closedrequests.OrderByOrderByDescending(c=> c.RequestClosedOnDateTime);

            ViewData["IsShareAdmin"] = auth.IsShareAdmin(user.UserAlias);
            ViewData["IsSiteAdmin"] = auth.IsSiteAdmin(user.UserAlias);
            ViewData["OpenRequests"] = openrequests;
            ViewData["ClosedRequests"] = closedrequests;

            ViewData["ncgroupname"] = cifsshare.NoChangeGroup;
            // sometimes need a retry
            var owners = AdHelper.GetAdGroupMembers(cifsshare.OwnerGroup,cifsshare.Ou.Domain.ToString()); //hit
            if (owners == null)
            {
                owners = AdHelper.GetAdGroupMembers(cifsshare.OwnerGroup,cifsshare.Ou.Domain.ToString()); //hit
            }
            ViewData["Owners"] = owners;
            
            ViewData["IsOwner"] = false;
            if(owners != null && owners.Exists(x=> x.UserAlias == user.UserAlias))
            {
                ViewData["IsOwner"] = true;
            }

            var readers = AdHelper.GetAllAdGroupMembers(cifsshare.ReadOnlyGroup, cifsshare.Ou.Domain.ToString());
            if (readers == null)
            {
                readers = AdHelper.GetAllAdGroupMembers(cifsshare.ReadOnlyGroup, cifsshare.Ou.Domain.ToString());
            }
            ViewData["Readers"] = readers;

            // get list of writers with retry
            var writers = AdHelper.GetAllAdGroupMembers(cifsshare.ReadWriteGroup, cifsshare.Ou.Domain.ToString());
            if (writers == null)
            {
                writers = AdHelper.GetAllAdGroupMembers(cifsshare.ReadWriteGroup, cifsshare.Ou.Domain.ToString());
            }
            ViewData["Writers"] = writers;

            // not required so only do the work if it's not null
            if(!string.IsNullOrEmpty(cifsshare.NoChangeGroup))
            {
                var nochangers = AdHelper.GetAllAdGroupMembers(cifsshare.NoChangeGroup, cifsshare.Ou.Domain.ToString());
                if (nochangers == null)
                {
                    nochangers = AdHelper.GetAllAdGroupMembers(cifsshare.NoChangeGroup, cifsshare.Ou.Domain.ToString());
                }
                ViewData["NoChangers"] = nochangers;
            }
            
            

            if (cifsshare == null)
            {
                return HttpNotFound();
            }
            return View(cifsshare);
        }

        // GET: /user/SmbShares/Create
        public ActionResult Create()
        {
            ViewBag.OuID = new SelectList(db.Ous, "ID", "OrganizationalUnit");
            return View();
        }

        // POST: /user/SmbShares/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include="CifsShareID,OuID,Name,CmdbCi,UncPath,ShareOwnerFunctionalArea,ShareOwnerCostCenter,OwnerGroup,ReadOnlyGroup,ReadWriteGroup,NoChangeGroup,Status,CreatedOnDateTime,CreatedBy,ModifiedOnDateTime,ModifiedBy")] CifsShare cifsshare)
        {
            if (ModelState.IsValid)
            {
                db.CifsShares.Add(cifsshare);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.OuID = new SelectList(db.Ous, "ID", "OrganizationalUnit", cifsshare.OuID);
            return View(cifsshare);
        }

        // GET: /user/SmbShares/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CifsShare cifsshare = await db.CifsShares.FindAsync(id);
            if (cifsshare == null)
            {
                return HttpNotFound();
            }
            ViewBag.OuID = new SelectList(db.Ous, "ID", "OrganizationalUnit", cifsshare.OuID);
            return View(cifsshare);
        }

        // POST: /user/SmbShares/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include="CifsShareID,OuID,Name,CmdbCi,UncPath,ShareOwnerFunctionalArea,ShareOwnerCostCenter,OwnerGroup,ReadOnlyGroup,ReadWriteGroup,NoChangeGroup,Status,CreatedOnDateTime,CreatedBy,ModifiedOnDateTime,ModifiedBy")] CifsShare cifsshare)
        {
            if (ModelState.IsValid)
            {
                db.Entry(cifsshare).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.OuID = new SelectList(db.Ous, "ID", "OrganizationalUnit", cifsshare.OuID);
            return View(cifsshare);
        }

        // GET: /user/SmbShares/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CifsShare cifsshare = await db.CifsShares.FindAsync(id);
            if (cifsshare == null)
            {
                return HttpNotFound();
            }
            return View(cifsshare);
        }

        // POST: /user/SmbShares/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            CifsShare cifsshare = await db.CifsShares.FindAsync(id);
            db.CifsShares.Remove(cifsshare);
            await db.SaveChangesAsync();
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
