using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ShareManagerEdgeMvc.DAL;
using ShareManagerEdgeMvc.Models;
using ShareManagerEdgeMvc.Helpers;

namespace ShareManagerEdgeMvc.Controllers
{
    public class HomeController : BaseController
    {
        private ShareContext db = new ShareContext();
        
        

        public ActionResult Index()
        {
            var user = HttpContext.User.Identity.Name.Substring(3);
            var auth = new AuthenticationHelper();

            ViewData["IsShareAdmin"] = auth.IsShareAdmin(user);
            ViewData["IsSiteAdmin"] = auth.IsSiteAdmin(user);

            // gets the request count for the current user
            var requestCount = db.CifsPermissionRequests.Where(s => (s.RequestedByUserAlias.ToUpper().Equals(user.ToUpper()) 
                || s.RequestedForUserAlias.ToUpper().Equals(user.ToUpper())) 
                && s.RequestStatus == RequestStatus.Open)
                .Count();

            if (requestCount == 0)
            {
                ViewData["OpenRequestCount"] = 0;
            }
            else
            {
                ViewData["OpenRequestCount"] = requestCount;
            }

            return View();
        }
        

        public ActionResult SetCulture(string culture)
        {
            // Validate input
            culture = CultureHelper.GetImplementedCulture(culture);
            // Save culture in a cookie
            HttpCookie cookie = Request.Cookies["_culture"];
            if (cookie != null)
                cookie.Value = culture;   // update cookie value
            else
            {
                cookie = new HttpCookie("_culture");
                cookie.Value = culture;
                cookie.Expires = DateTime.Now.AddYears(1);
            }
            Response.Cookies.Add(cookie);
            return RedirectToAction("Index");
        }  

        public ActionResult About()
        {
            //ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            //ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult Use()
        {
            //ViewBag.Message = "How to use page.";

            return View();
        }
    }
}