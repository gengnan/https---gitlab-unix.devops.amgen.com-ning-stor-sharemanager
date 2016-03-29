using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using ShareManagerApi.Models;
//using System.Enum;
using ShareManagerApi.Helper;
using System.Configuration;



namespace ShareManagerApi.Controllers
{
    ///<summary>
    ///Handles operations for Access Requests
    ///</summary>
    public class CifsPermissionRequestsController : ApiController
    {
        private ShareContext db = new ShareContext();

        ///<summary>
        ///Return all existing access requests in app DB
        ///</summary>
        // GET: api/CifsPermissionRequests
        public IQueryable<CifsPermissionRequest> GetCifsPermissionRequests()
        {
            return db.CifsPermissionRequests;
        }


        ///<summary>
        ///Return one access request with specific ID
        ///</summary>
        // GET: api/CifsPermissionRequests/5
        [ResponseType(typeof(CifsPermissionRequest))]
        public async Task<IHttpActionResult> GetCifsPermissionRequest(int id)
        {
            CifsPermissionRequest cifsPermissionRequest = await db.CifsPermissionRequests.FindAsync(id);
            if (cifsPermissionRequest == null)
            {
                return NotFound();
            }

            return Ok(cifsPermissionRequest);
        }


        //// PUT: api/CifsPermissionRequests/5
        ////[ResponseType(typeof(void))]
        //public async Task<IHttpActionResult> PutCifsPermissionRequest(int id, CifsPermissionRequest cifsPermissionRequest)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    if (id != cifsPermissionRequest.CifsPermissionRequestID)
        //    {
        //        return BadRequest();
        //    }

        //    db.Entry(cifsPermissionRequest).State = EntityState.Modified;

        //    try
        //    {
        //        await db.SaveChangesAsync();
        //    }
        //    catch (DbUpdateConcurrencyException)
        //    {
        //        if (!CifsPermissionRequestExists(id))
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


        ///<summary>
        ///Create an access request 
        ///</summary>
        // POST: api/CifsPermissionRequests
        [ResponseType(typeof(CifsPermissionRequest))]
        public async Task<IHttpActionResult> PostCifsPermissionRequest(CifsPermissionRequestRaw cifsPermissionRequest)
        {
            //if request is valid

            var cifsShare = await db.CifsShares.FindAsync(cifsPermissionRequest.CifsShareID);
            var domain = db.Ous.Find(cifsShare.OuID).Domain.ToString();
            if (cifsShare == null)
            { 
                ModelState.AddModelError("CifsShareID", "Cifs Share ID is not found."); 
            }

            if(!Enum.IsDefined(typeof(PermissionType),cifsPermissionRequest.PermissionType))
            {
                ModelState.AddModelError("PermissionType", "Permission Type is not valid."); 
            }

            if (!Enum.IsDefined(typeof(RequestType), cifsPermissionRequest.RequestType))
            {
                ModelState.AddModelError("RequestType", "Request Type is not valid.");
            }

            if (!Enum.IsDefined(typeof(RequestAdPrincipalType), cifsPermissionRequest.RequestAdPrincipalType)||cifsPermissionRequest.RequestAdPrincipalType != RequestAdPrincipalType.User)
            {
                ModelState.AddModelError("RequestAdPrincipalType", "RequestAdPrincipalType has to be User and User Only.");
            }

            if (!Enum.IsDefined(typeof(RequestStatus), cifsPermissionRequest.RequestStatus) || cifsPermissionRequest.RequestStatus != RequestStatus.Open)
            {
                ModelState.AddModelError("RequestStatus", "Request Status has to be Open and Open Only.");
            }

            if (cifsPermissionRequest.RequestJustification == null || cifsPermissionRequest.RequestJustification.Length >1024 )
            {
                ModelState.AddModelError("RequestJustification", "Request Justification character should be between 0 and 1024.");
            }

            if (cifsPermissionRequest.RequestedByUserAlias.Contains("\\"))
            {
                cifsPermissionRequest.RequestedByUserAlias = cifsPermissionRequest.RequestedByUserAlias.Substring(cifsPermissionRequest.RequestedByUserAlias.IndexOf("\\") + 1).Trim();
            }

            if (AdHelper.GetAdUser(cifsPermissionRequest.RequestedByUserAlias) == null)
            {
                ModelState.AddModelError("RequestedByUserAlias", "Requested By User Alias is not valid.");
            }

            if (cifsPermissionRequest.RequestedForUserAlias.Contains("\\"))
            {
                cifsPermissionRequest.RequestedForUserAlias = cifsPermissionRequest.RequestedForUserAlias.Substring(cifsPermissionRequest.RequestedForUserAlias.IndexOf("\\") + 1).Trim();
            }

            if (AdHelper.GetAdUser(cifsPermissionRequest.RequestedForUserAlias) == null)
            {
                ModelState.AddModelError("RequestedForUserAlias", "Requested For User Alias is not valid.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            //if request is needed


            var exists = db.CifsPermissionRequests.Where(c => c.PermissionType.Value == cifsPermissionRequest.PermissionType &&
                            c.RequestType.Value == cifsPermissionRequest.RequestType &&
                            c.RequestAdPrincipalType.Value == RequestAdPrincipalType.User &&
                            c.RequestedForUserAlias == cifsPermissionRequest.RequestedForUserAlias &&
                            c.RequestStatus.Value == RequestStatus.Open &&
                            c.CifsShareID == cifsPermissionRequest.CifsShareID);

                        // only add a new request if one doesn't exist for the user.
            if (exists.Count() != 0)
            {
                ModelState.AddModelError("PermissionType", "Request already exists.");
                return BadRequest(ModelState);
            }

            if (!IsPermissionRequestValidForSecurityGroup(cifsShare, cifsPermissionRequest))
            {
                ModelState.AddModelError("PermissionType", "Share is not configurd for User access has been granted already or have been removed already.");
                return BadRequest(ModelState);
            }

            
            var towners = AdHelper.GetAdGroupMembers(cifsShare.OwnerGroup,domain);

            if(towners.Count == 0)
            {
                ModelState.AddModelError("PermissionType", "Share doesn't not have an owner defined, request cannot be submitted.");
                return BadRequest(ModelState);
            }


            if(cifsPermissionRequest.RequestType == RequestType.Remove && cifsPermissionRequest.PermissionType == PermissionType.GK && towners.Count == 1)
            {
                ModelState.AddModelError("PermissionType", "Request will remove last share owner and is not allowed.");
                return BadRequest(ModelState);
            }

          
            //if approver notification is sent successfully

            cifsPermissionRequest.RequestedByUserName = AdHelper.GetAdUser(cifsPermissionRequest.RequestedByUserAlias).UserName;
            cifsPermissionRequest.RequestedForUserName = AdHelper.GetAdUser(cifsPermissionRequest.RequestedForUserAlias).UserName;
            cifsPermissionRequest.RequestOpenedOnDateTime = DateTime.Now;

            //var userId = HttpContext.User.Identity.Name.Substring(3);
            //var userId = "nin";

            string userId = null;
            try
            {
                userId = HttpContext.Current.User.Identity.Name.Substring(3);
            }
            catch
            {
                string serviceNow = ConfigurationManager.AppSettings["serviceNow"].ToString();
                if (cifsPermissionRequest.uID != null && cifsPermissionRequest.uID == serviceNow)
                {
                    userId = "serviceNow";
                }
                else
                {
                    ModelState.AddModelError("uID", "Authorization is required.");
                    return BadRequest(ModelState);
                }
            }

            CifsPermissionRequest cifsPermissionRequestFormatted = new CifsPermissionRequest();
            //format entry input
            cifsPermissionRequestFormatted.CifsShareID = cifsPermissionRequest.CifsShareID;
            cifsPermissionRequestFormatted.PermissionType = cifsPermissionRequest.PermissionType;
            cifsPermissionRequestFormatted.RequestType = cifsPermissionRequest.RequestType;
            cifsPermissionRequestFormatted.RequestStatus = cifsPermissionRequest.RequestStatus;
            cifsPermissionRequestFormatted.RequestJustification = cifsPermissionRequest.RequestJustification;
            cifsPermissionRequestFormatted.RequestAdPrincipalType = cifsPermissionRequest.RequestAdPrincipalType;
            cifsPermissionRequestFormatted.RequestedForUserAlias = cifsPermissionRequest.RequestedForUserAlias;
            cifsPermissionRequestFormatted.RequestedByUserAlias = cifsPermissionRequest.RequestedByUserAlias;
            cifsPermissionRequestFormatted.RequestedByUserName = cifsPermissionRequest.RequestedByUserName;
            cifsPermissionRequestFormatted.RequestedForUserName = cifsPermissionRequest.RequestedForUserName;
            cifsPermissionRequestFormatted.RequestOpenedOnDateTime = cifsPermissionRequest.RequestOpenedOnDateTime;
            //cifsPermissionRequestFormatted.RequestedForUserName = cifsPermissionRequest.RequestedForUserName;

            db.CifsPermissionRequests.Add(cifsPermissionRequestFormatted);
            await db.SaveChangesAsync(userId);

            bool emailResult = EmailHelper.SendNewRequestNotificationMessage(cifsPermissionRequestFormatted, cifsShare);

            if (emailResult)
            {
                cifsPermissionRequestFormatted.RequestApprovalNotificationTimeStamp = DateTime.Now;
                db.Entry(cifsPermissionRequestFormatted).State = EntityState.Modified;
                await db.SaveChangesAsync(userId);

            }


            return CreatedAtRoute("DefaultApi", new { id = cifsPermissionRequestFormatted.CifsPermissionRequestID }, cifsPermissionRequestFormatted);

        }















        ///<summary>
        ///Update database for request reminder and send email notification
        ///</summary>
        // PUT: api/CifsPermissionRequests
        [Route("api/RemindOpenRequest")]
        [HttpPut]
        [ResponseType(typeof(CifsPermissionRequest))]
        public async Task<IHttpActionResult> PutCifsPermissionRequestRemindOpenRequest(int id)
        {
            ////don't pass security key through uri parameter
            //string secKey = ConfigurationManager.AppSettings["securityKey"];

            //if (key != secKey)
            //{
            //    return Unauthorized();
            //}

            CifsPermissionRequest cifsPermissionRequest = await db.CifsPermissionRequests.FindAsync(id);
            if (cifsPermissionRequest == null)
            {
                return NotFound();
            }

            if (cifsPermissionRequest.RequestStatus != ShareManagerApi.Models.RequestStatus.Open)
            {
                return BadRequest();
            }

            var cifsShare = await db.CifsShares.FindAsync(cifsPermissionRequest.CifsShareID);

            if (cifsShare == null)
            {
                ModelState.AddModelError("CifsShareID", "Cifs Share ID is not found.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != cifsPermissionRequest.CifsPermissionRequestID)
            {
                return BadRequest();
            }

            cifsPermissionRequest.RequestApprovalNotificationTimeStamp = DateTime.Now;
            
            bool emailResult = EmailHelper.SendReminderRequestNotificationMessage(cifsPermissionRequest, cifsShare);

            if (emailResult)
            {
                db.Entry(cifsPermissionRequest).State = EntityState.Modified;

                string userId = null;
                try
                {
                    userId = HttpContext.Current.User.Identity.Name.Substring(3);
                }
                catch
                {
                    return BadRequest(ModelState);
                }

                try
                {
                    await db.SaveChangesAsync(userId);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CifsPermissionRequestExists(id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }














        ///<summary>
        ///Update database for request cancellation and send email notification
        ///</summary>
        // PUT: api/CifsPermissionRequests
        [Route("api/CancelOpenRequest")]
        [HttpPut]
        [ResponseType(typeof(CifsPermissionRequest))]
        public async Task<IHttpActionResult> PutCifsPermissionRequestCancelOpenRequest(int id)
        {
            ////don't pass security key through uri parameter
            //string secKey = ConfigurationManager.AppSettings["securityKey"];

            //if (key != secKey)
            //{
            //    return Unauthorized();
            //}

            CifsPermissionRequest cifsPermissionRequest = await db.CifsPermissionRequests.FindAsync(id);
            if (cifsPermissionRequest == null)
            {
                return NotFound();
            }

            if (cifsPermissionRequest.RequestStatus != ShareManagerApi.Models.RequestStatus.Open)
            {
                return BadRequest();
            }

            var cifsShare = await db.CifsShares.FindAsync(cifsPermissionRequest.CifsShareID);

            if (cifsShare == null)
            {
                ModelState.AddModelError("CifsShareID", "Cifs Share ID is not found.");
            } 

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != cifsPermissionRequest.CifsPermissionRequestID)
            {
                return BadRequest();
            }

            

            db.Entry(cifsPermissionRequest).State = EntityState.Modified;

            string userId = null;
            try
            {
                userId = HttpContext.Current.User.Identity.Name.Substring(3);
            }
            catch
            {
                return BadRequest(ModelState);
            }

            cifsPermissionRequest.RequestStatus = ShareManagerApi.Models.RequestStatus.Completed;
            cifsPermissionRequest.RequestApprovalStatus = ShareManagerApi.Models.RequestApprovalStatus.Cancelled;
            cifsPermissionRequest.RequestStatusMsg = "Request cancelled due to inactivity.";
            cifsPermissionRequest.RequestClosedOnDateTime = DateTime.Now;
            cifsPermissionRequest.RequestClosedNotification = true;
            //cifsPermissionRequest.ClosedByUserAlias = null; //not sure if we set a value here
            //cifsPermissionRequest.ClosedByUserName = null; //not sure if we set a value here
            try
            {
                await db.SaveChangesAsync(userId);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CifsPermissionRequestExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            bool emailResult = EmailHelper.SendCancellationNotificationMessage(cifsPermissionRequest, cifsShare);

            if (emailResult == true)
            {
                cifsPermissionRequest.RequestStatus = ShareManagerApi.Models.RequestStatus.Completed;
                cifsPermissionRequest.RequestApprovalStatus = ShareManagerApi.Models.RequestApprovalStatus.Cancelled;
                cifsPermissionRequest.RequestStatusMsg = "Request cancelled due to inactivity.";
                cifsPermissionRequest.RequestClosedOnDateTime = DateTime.Now;
                cifsPermissionRequest.RequestClosedNotification = true;
                //cifsPermissionRequest.ClosedByUserAlias = null; //not sure if we set a value here
                //cifsPermissionRequest.ClosedByUserName = null; //not sure if we set a value here
                try
                {
                    await db.SaveChangesAsync(userId);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CifsPermissionRequestExists(id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }



















        // DELETE: api/CifsPermissionRequests/5
        //[ResponseType(typeof(CifsPermissionRequest))]
        //public async Task<IHttpActionResult> DeleteCifsPermissionRequest(int id)
        //{
        //    CifsPermissionRequest cifsPermissionRequest = await db.CifsPermissionRequests.FindAsync(id);
        //    if (cifsPermissionRequest == null)
        //    {
        //        return NotFound();
        //    }

        //    db.CifsPermissionRequests.Remove(cifsPermissionRequest);
        //    await db.SaveChangesAsync();

        //    return Ok(cifsPermissionRequest);
        //}

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool CifsPermissionRequestExists(int id)
        {
            return db.CifsPermissionRequests.Count(e => e.CifsPermissionRequestID == id) > 0;
        }

        private bool IsPermissionRequestValidForSecurityGroup(CifsShare share, CifsPermissionRequestRaw request)
        {
            var reqtype = request.RequestType;
            var permtype = request.PermissionType;
            var printype = request.RequestAdPrincipalType;
            var ou = db.Ous.Find(share.OuID);

            //check if group is config
            if(request.PermissionType == PermissionType.GK && share.OwnerGroup == null)
            {
                return false;
            }
            if (request.PermissionType == PermissionType.RO && share.ReadOnlyGroup == null)
            {
                return false;
            }
            if (request.PermissionType == PermissionType.RW && share.ReadWriteGroup == null)
            {
                return false;
            }
            if (request.PermissionType == PermissionType.NC && share.NoChangeGroup == null)
            {
                return false;
            }

            // if request is add then ...
            if (reqtype == RequestType.Add)
            {
                // based on permission type, call AdHelper.IsUserMemberOfGroup with right SG and if true then return false because 
                // request is NOT valid. User is already a member so you shouldn't add them.
                if (printype == RequestAdPrincipalType.SecurityGroup)
                {
                    switch (permtype)
                    {
                        case PermissionType.RO:
                            return !AdHelper.IsGroupMemberOfGroup(share.ReadOnlyGroup, ou.Domain.ToString(), ou.OrganizationalUnit, request.RequestedForUserAlias);
                        case PermissionType.RW:
                            return !AdHelper.IsGroupMemberOfGroup(share.ReadWriteGroup, ou.ToString(), ou.OrganizationalUnit, request.RequestedForUserAlias);
                        case PermissionType.NC:
                            return !AdHelper.IsGroupMemberOfGroup(share.NoChangeGroup, ou.Domain.ToString(), ou.OrganizationalUnit, request.RequestedForUserAlias);
                        case PermissionType.GK:
                            return !AdHelper.IsGroupMemberOfGroup(share.OwnerGroup, ou.Domain.ToString(), ou.OrganizationalUnit, request.RequestedForUserAlias);
                        default:
                            return true;
                    }
                }
                else
                {
                    switch (permtype)
                    {
                        case PermissionType.RO:
                            return !AdHelper.IsUserMemberOfGroup(share.ReadOnlyGroup, ou.Domain.ToString(), ou.OrganizationalUnit, request.RequestedForUserAlias);
                        case PermissionType.RW:
                            return !AdHelper.IsUserMemberOfGroup(share.ReadWriteGroup, ou.Domain.ToString(), ou.OrganizationalUnit, request.RequestedForUserAlias);
                        case PermissionType.NC:
                            return !AdHelper.IsUserMemberOfGroup(share.NoChangeGroup, ou.Domain.ToString(), ou.OrganizationalUnit, request.RequestedForUserAlias);
                        case PermissionType.GK:
                            return !AdHelper.IsUserMemberOfGroup(share.OwnerGroup, ou.Domain.ToString(), ou.OrganizationalUnit, request.RequestedForUserAlias);
                        default:
                            return true;
                    }
                }
            }
            else
            {
                // based on permission type, call AdHelper.IsUserMemberOfGroup with right SG and if true then return true because 
                // request is valid. You can remove a user that is a member
                if (printype == RequestAdPrincipalType.SecurityGroup)
                {
                    switch (permtype)
                    {
                        case PermissionType.RO:
                            return AdHelper.IsGroupMemberOfGroup(share.ReadOnlyGroup, ou.Domain.ToString(), ou.OrganizationalUnit, request.RequestedForUserAlias);
                        case PermissionType.RW:
                            return AdHelper.IsGroupMemberOfGroup(share.ReadWriteGroup, ou.Domain.ToString(), ou.OrganizationalUnit, request.RequestedForUserAlias);
                        case PermissionType.NC:
                            return AdHelper.IsGroupMemberOfGroup(share.NoChangeGroup, ou.Domain.ToString(), ou.OrganizationalUnit, request.RequestedForUserAlias);
                        case PermissionType.GK:
                            return AdHelper.IsGroupMemberOfGroup(share.OwnerGroup, ou.Domain.ToString(), ou.OrganizationalUnit, request.RequestedForUserAlias);
                        default:
                            return true;
                    }
                }
                else
                {
                    switch (permtype)
                    {
                        case PermissionType.RO:
                            return AdHelper.IsUserMemberOfGroup(share.ReadOnlyGroup, ou.Domain.ToString(), ou.OrganizationalUnit, request.RequestedForUserAlias);
                        case PermissionType.RW:
                            return AdHelper.IsUserMemberOfGroup(share.ReadWriteGroup, ou.Domain.ToString(), ou.OrganizationalUnit, request.RequestedForUserAlias);
                        case PermissionType.NC:
                            return AdHelper.IsUserMemberOfGroup(share.NoChangeGroup, ou.Domain.ToString(), ou.OrganizationalUnit, request.RequestedForUserAlias);
                        case PermissionType.GK:
                            return AdHelper.IsUserMemberOfGroup(share.OwnerGroup, ou.Domain.ToString(), ou.OrganizationalUnit, request.RequestedForUserAlias);
                        default:
                            return true;
                    }
                }
            }
        }


    }
}