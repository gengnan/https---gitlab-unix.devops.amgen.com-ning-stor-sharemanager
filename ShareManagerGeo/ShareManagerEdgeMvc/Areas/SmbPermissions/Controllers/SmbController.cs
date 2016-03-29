using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ShareManagerEdgeMvc.DAL;
using ShareManagerEdgeMvc.Models;
using PagedList;

namespace ShareManagerEdgeMvc.Areas.SmbPermissions.Controllers
{
    public class SmbController : ShareManagerEdgeMvc.Controllers.BaseController
    {
        private ShareContext db = new ShareContext();
        //
        // GET: /SmbPermissions/Smb/
        public ActionResult Index(string sortOrder, string currentFilter, string searchString, int? page)
        {
            // viewmodel definition
            var viewModel = new CifsShareViewModel();

            // get shares
            viewModel.CifsShares = from s in db.CifsShares select s;

            var user = HttpContext.User.Identity.Name.Substring(3);
            ViewData["user"] = user;
            // Adding sorting functionalisty
            ViewBag.CurrentSort = sortOrder;
            ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewBag.DateSortParm = sortOrder == "Date" ? "date_desc" : "Date";

            // adding pluming for pagedList
            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewBag.CurrentFilter = searchString;

           

            // if searchString isn't null then filter list based on searchString
            if (!String.IsNullOrEmpty(searchString))
            {
                viewModel.CifsShares = viewModel.CifsShares.Where(s => s.Name.ToUpper().Contains(searchString.ToUpper())
                    || s.UncPath.ToUpper().Contains(searchString.ToUpper())
                    || s.CmdbCi.ToUpper().Contains(searchString.ToUpper()));
            }

            // gets the request count for the current user
            viewModel.CifsPermissionRequests = db.CifsPermissionRequests.Where(s => (s.RequestedByUserAlias.ToUpper().Equals(user.ToUpper())
                || s.RequestedForUserAlias.ToUpper().Equals(user.ToUpper()))
                && s.RequestStatus == RequestStatus.Open)
                .OrderBy(s => s.RequestOpenedOnDateTime);

                        

            // orders the shares based on the sortOrder parameter
            switch (sortOrder)
            {
                case "name":
                    viewModel.CifsShares = viewModel.CifsShares.OrderByDescending(s => s.Name);
                    break;
                default:
                    viewModel.CifsShares = viewModel.CifsShares.OrderBy(s => s.CreatedOnDateTime);
                    break;
            }

            int pageSize = 10;
            int pageNumber = (page ?? 1);
            viewModel.SearchResults = viewModel.CifsShares.ToPagedList(pageNumber, pageSize);
            return View(viewModel);
            //return View(viewModel);
        }

        //
        // GET: /SmbPermissions/Smb/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        //
        // GET: /SmbPermissions/Smb/RequestDetails/5
        public ActionResult RequestDetails(int id)
        {
            return View();
        }

        //
        // GET: /SmbPermissions/Smb/Create
        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /SmbPermissions/Smb/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        //
        // GET: /SmbPermissions/Smb/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        //
        // POST: /SmbPermissions/Smb/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        //
        // GET: /SmbPermissions/Smb/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        //
        // POST: /SmbPermissions/Smb/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}
