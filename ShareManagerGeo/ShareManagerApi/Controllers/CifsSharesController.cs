using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using ShareManagerApi.Models;

namespace ShareManagerApi.Controllers
{
    ///<summary>
    ///Handles operations for Cifs Shares DB entries
    ///</summary>
    public class CifsSharesController : ApiController
    {
        private ShareContext db = new ShareContext();

        // GET: api/CifsShares
        ///<summary>
        ///Return list of all share records in app DB
        ///</summary>
        public IQueryable<CifsShare> GetCifsShares()
        {
            return db.CifsShares;
        }

        ///<summary>
        ///Return one share record with specific id
        ///</summary>
        // GET: api/CifsShares/5
        [ResponseType(typeof(CifsShare))]
        public async Task<IHttpActionResult> GetCifsShare(int id)
        {
            CifsShare cifsShare = await db.CifsShares.FindAsync(id);
            if (cifsShare == null)
            {
                return NotFound();
            }

            return Ok(cifsShare);
        }

        ///<summary>
        ///Return list of all share records on one storage system (server)
        ///</summary>
        public IQueryable<CifsShare> GetCifsShare(string server)
        {
            string path1 = "\\\\" + server ;

            var cifsShare = db.CifsShares.Where(p => p.Status == Status.InService && p.UncPath.Contains(path1));

            return cifsShare;



        }

        ///<summary>
        ///Return one share record with path \\server\share; if multiple are found, return the first one
        ///</summary>
        public int GetCifsShare(string server, string share)
        {
            string path1 = "\\\\" + server + "\\" + share ;
            string path2 = "\\\\" + server + "\\" + share + "\\";
            
            var cifsShare = db.CifsShares.Where(p => p.Status == Status.InService &&p.UncPath == path1);
            if (!cifsShare.Any())
            {
                cifsShare = db.CifsShares.Where(p => p.Status == Status.InService && p.UncPath == path2);
            }
            if(!cifsShare.Any())
            {
                return 0;
            }
            else
            {
                return cifsShare.First().CifsShareID;
            }


        }

        ///<summary>
        ///Return one share record with path \\server\share\folder; if multiple are found, return the first one
        ///</summary>
        public int GetCifsShare(string server, string share, string folder)
        {
            string path1 = "\\\\" + server + "\\" + share + "\\" + folder;
            string path2 = "\\\\" + server + "\\" + share + "\\" + folder + "\\";

            var cifsShare = db.CifsShares.Where(p => p.Status == Status.InService && p.UncPath == path1);
            if (!cifsShare.Any())
            {
                cifsShare = db.CifsShares.Where(p => p.Status == Status.InService && p.UncPath == path2);
            }
            if (!cifsShare.Any())
            {
                return 0;
            }
            else
            {
                return cifsShare.First().CifsShareID;
            }


        }

        // PUT: api/CifsShares/5
        //[ResponseType(typeof(void))]
        //public async Task<IHttpActionResult> PutCifsShare(int id, CifsShare cifsShare)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    if (id != cifsShare.CifsShareID)
        //    {
        //        return BadRequest();
        //    }

        //    db.Entry(cifsShare).State = EntityState.Modified;

        //    try
        //    {
        //        await db.SaveChangesAsync();
        //    }
        //    catch (DbUpdateConcurrencyException)
        //    {
        //        if (!CifsShareExists(id))
        //        {
        //            return NotFound();
        //        }
        //        else
        //        {
        //            throw;
        //        }
        //    }

        //    return StatusCode(HttpStatusCode.NoContent);
        //}

        // POST: api/CifsShares

        //Example: body:
            //{
            //'OuID':1,
            //'Name':'APITest',
            //'UncPath':'\\\\API\\Test',
            //'OwnerGroup':'flr-apitest_gk',
            //'Status':0
            //}
        // Content-Type: application/JSON
        //[ResponseType(typeof(CifsShare))]
        //public async Task<IHttpActionResult> PostCifsShare(CifsShare cifsShare)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    db.CifsShares.Add(cifsShare);
        //    await db.SaveChangesAsync();

        //    return CreatedAtRoute("DefaultApi", new { id = cifsShare.CifsShareID }, cifsShare);
        //}

        // DELETE: api/CifsShares/5
        //[ResponseType(typeof(CifsShare))]
        //public async Task<IHttpActionResult> DeleteCifsShare(int id)
        //{
        //    CifsShare cifsShare = await db.CifsShares.FindAsync(id);
        //    if (cifsShare == null)
        //    {
        //        return NotFound();
        //    }

        //    db.CifsShares.Remove(cifsShare);
        //    await db.SaveChangesAsync();

        //    return Ok(cifsShare);
        //}

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