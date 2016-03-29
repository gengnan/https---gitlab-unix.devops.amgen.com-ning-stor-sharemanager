using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web;
using System.Web.Http.Description;
using ShareManagerEdgeMvc.DAL;
using ShareManagerEdgeMvc.Models;

namespace ShareManagerEdgeMvc.Areas.api.Controllers
{
    public class SharesController : ApiController
    {
        private ShareContext db = new ShareContext();

        // GET: api/Shares
        public HttpResponseMessage Get()
        {
            var shares = db.CifsShares.ToList<CifsShare>();
            return Request.CreateResponse(HttpStatusCode.OK, shares, GlobalConfiguration.Configuration.Formatters.JsonFormatter);
        }

        // GET: api/Shares/5
        [ResponseType(typeof(CifsShare))]
        public HttpResponseMessage Get(int id)
        {
            CifsShare cifsShare = db.CifsShares.Find(id);
            if (cifsShare == null)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }

            return Request.CreateResponse(HttpStatusCode.OK, cifsShare);
        }

        // PUT: api/Shares/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutCifsShare(int id, CifsShare cifsShare)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != cifsShare.CifsShareID)
            {
                return BadRequest();
            }

            db.Entry(cifsShare).State = EntityState.Modified;

            try
            {
                var userId = HttpContext.Current.User.Identity.Name.Substring(3);
                db.SaveChanges(userId);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CifsShareExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/Shares
        [ResponseType(typeof(CifsShare))]
        public IHttpActionResult PostCifsShare(CifsShare cifsShare)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.CifsShares.Add(cifsShare);
            var userId = HttpContext.Current.User.Identity.Name.Substring(3);
            db.SaveChanges(userId);

            return CreatedAtRoute("DefaultApi", new { id = cifsShare.CifsShareID }, cifsShare);
        }

        // DELETE: api/Shares/5
        [ResponseType(typeof(CifsShare))]
        public IHttpActionResult DeleteCifsShare(int id)
        {
            CifsShare cifsShare = db.CifsShares.Find(id);
            if (cifsShare == null)
            {
                return NotFound();
            }

            db.CifsShares.Remove(cifsShare);
            var userId = HttpContext.Current.User.Identity.Name.Substring(3);
            db.SaveChanges(userId);

            return Ok(cifsShare);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool CifsShareExists(int id)
        {
            return db.CifsShares.Count(e => e.CifsShareID == id) > 0;
        }
    }
}