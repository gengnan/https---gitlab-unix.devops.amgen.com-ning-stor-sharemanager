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

namespace ShareManagerEdgeMvc.Areas.SmbPermissions.Controllers
{
    public class RequestsController : ShareManagerEdgeMvc.Controllers.BaseController
    {
        private ShareContext db = new ShareContext();

        // GET: /SmbPermissions/Requests/
        public ActionResult Index()
        {
            var cifspermissionrequests = db.CifsPermissionRequests.Include(c => c.CifsShare);
            return View(cifspermissionrequests.ToList());
        }

        // GET: /SmbPermissions/Requests/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CifsPermissionRequest cifspermissionrequest = db.CifsPermissionRequests.Find(id);
            if (cifspermissionrequest == null)
            {
                return HttpNotFound();
            }
            return View(cifspermissionrequest);
        }

        // GET: /SmbPermissions/Requests/Create
        public ActionResult Create()
        {
            ViewBag.CifsShareID = new SelectList(db.CifsShares, "CifsShareID", "Name");
            ViewBag.PermissionType = EnumHelper.GetSelectList<PermissionType>();
            ViewBag.RequestType = EnumHelper.GetSelectList<RequestType>();
            ViewBag.RequestedForUserAlias = HttpContext.User.Identity.Name.Substring(3);
            return View();
        }

        // POST: /SmbPermissions/Requests/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include="CifsPermissionRequestID,CifsShareID,PermissionType,RequestType,RequestJustification,RequestedForUserAlias,RequestedForUserName,RequestedByUserAlias,RequestedByUserName,RequestStatus,RequestStatusMsg,RequestOpenedOnDateTime,RequestClosedOnDateTime")] CifsPermissionRequest cifspermissionrequest)
        {
            if (ModelState.IsValid)
            {
                // create ad user for current site user and another with same value
                AdUser requestedBy = AdHelper.GetAdUser(HttpContext.User.Identity.Name.Substring(3));
                AdUser requestedFor = requestedBy;

                // fix up requested by
                if (cifspermissionrequest.RequestedForUserAlias.Contains("\\"))
                {
                    cifspermissionrequest.RequestedForUserAlias = cifspermissionrequest.RequestedForUserAlias.Substring(cifspermissionrequest.RequestedForUserAlias.IndexOf('\\') + 1).Trim();
                }

                // if requested by and for are identical no need to overwrite requestedFor AdUser
                if (cifspermissionrequest.RequestedForUserAlias.ToUpper() != HttpContext.User.Identity.Name.Substring(3).ToUpper())
                {
                    requestedFor = AdHelper.GetAdUser(cifspermissionrequest.RequestedForUserAlias);
                }

                // set remaining values
                cifspermissionrequest.RequestedForUserName = requestedFor.UserName;
                cifspermissionrequest.RequestedByUserAlias = requestedBy.UserAlias;
                cifspermissionrequest.RequestedByUserName = requestedBy.UserName;
                cifspermissionrequest.RequestStatus = RequestStatus.Open;
                cifspermissionrequest.RequestOpenedOnDateTime = System.DateTime.Now;
                //cifspermissionrequest.RequestType = cifspermissionrequest.RequestType.Value;
                //cifspermissionrequest.PermissionType = cifspermissionrequest.PermissionType.Value;
                
                db.CifsPermissionRequests.Add(cifspermissionrequest);
                var userId = HttpContext.User.Identity.Name.Substring(3);
                db.SaveChanges(userId);
                return RedirectToAction("Index");
            }

            ViewBag.CifsShareID = new SelectList(db.CifsShares, "CifsShareID", "Name", cifspermissionrequest.CifsShareID);
            return View(cifspermissionrequest);
        }

        // GET: /SmbPermissions/Requests/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CifsPermissionRequest cifspermissionrequest = db.CifsPermissionRequests.Find(id);
            if (cifspermissionrequest == null)
            {
                return HttpNotFound();
            }
            ViewBag.CifsShareID = new SelectList(db.CifsShares, "CifsShareID", "Name", cifspermissionrequest.CifsShareID);
            ViewBag.RequestStatus = EnumHelper.GetSelectList<RequestStatus>(cifspermissionrequest.RequestStatus.ToString());
            return View(cifspermissionrequest);
        }

        // POST: /SmbPermissions/Requests/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include="CifsPermissionRequestID,CifsShareID,PermissionType,RequestType,RequestJustification,RequestedForUserAlias,RequestedForUserName,RequestedByUserAlias,RequestedByUserName,RequestStatus,RequestStatusMsg,RequestOpenedOnDateTime,RequestClosedOnDateTime")] CifsPermissionRequest cifspermissionrequest)
        {
            if (ModelState.IsValid)
            {
                db.Entry(cifspermissionrequest).State = EntityState.Modified;
                var userId = HttpContext.User.Identity.Name.Substring(3);
                db.SaveChanges(userId);
                return RedirectToAction("Index");
            }
            ViewBag.CifsShareID = new SelectList(db.CifsShares, "CifsShareID", "Name", cifspermissionrequest.CifsShareID);
            return View(cifspermissionrequest);
        }

        // GET: /SmbPermissions/Requests/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CifsPermissionRequest cifspermissionrequest = db.CifsPermissionRequests.Find(id);
            if (cifspermissionrequest == null)
            {
                return HttpNotFound();
            }
            return View(cifspermissionrequest);
        }

        // POST: /SmbPermissions/Requests/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            CifsPermissionRequest cifspermissionrequest = db.CifsPermissionRequests.Find(id);
            db.CifsPermissionRequests.Remove(cifspermissionrequest);
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
