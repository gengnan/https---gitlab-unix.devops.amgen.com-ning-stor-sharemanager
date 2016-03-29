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
using System.Data.Entity.Validation;
using System.Diagnostics;

namespace ShareManagerEdgeMvc.Areas.owner.Controllers
{
	public class SmbRequestController : ShareManagerEdgeMvc.Controllers.BaseController
	{
		private ShareContext db = new ShareContext();

		// private method for authenticating whether a user is a member of the 
		// owner group
		private bool AuthenticateUser(CifsPermissionRequest request, string userAlias)
		{

			var user = AdHelper.GetAdUser(HttpContext.User.Identity.Name.Substring(3));
			if (user == null) // if unable to determine user return false
			{
				return false;
			}
			if (request == null) // if request is empty return false
			{
				return false; 
			}

			CifsShare cifsshare = db.CifsShares.Find(request.CifsShareID);
			
			if (AdHelper.IsUserMemberOfGroup(cifsshare.OwnerGroup, cifsshare.Ou.Domain.ToString(), cifsshare.Ou.OrganizationalUnit, user.UserAlias))
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		// GET: /owner/SmbRequest/
		// DETAILS about a request. Merely provides details about a request that is already completed.
		public ActionResult Index(int? id)
		{
			// if ID is null then this is a bad request
			if (id == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}

			// find request by id
			var cifspermissionrequest = db.CifsPermissionRequests.Find(id);
			// load share info
			cifspermissionrequest.CifsShare = db.CifsShares.Find(cifspermissionrequest.CifsShareID);

			// authenticate user
			if (AuthenticateUser(cifspermissionrequest, HttpContext.User.Identity.Name.Substring(3)))
			{
				// if security group based request, get all SG users to display on screen.
				ViewBag.RequestForMembers = null;
				if (cifspermissionrequest.RequestAdPrincipalType == RequestAdPrincipalType.SecurityGroup)
				{
					ViewBag.RequestForMembers = AdHelper.GetAdGroupMembers(cifspermissionrequest.RequestedForUserAlias,cifspermissionrequest.CifsShare.Ou.Domain.ToString()) as List<AdUser>;
				}
				return View(cifspermissionrequest);
			}
			else
			{
				return View("Error");
			}
		}

		// GET: /owner/SmbRequest/Details/5
		// Get's the data about an open request that will be approved or denied.
		public ActionResult Details(int? id)
		{
			// if ID is null then this is a bad request
			if (id == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}

			// using the ID parameter, get's the request information and using the request, get's the share details.
			CifsPermissionRequest cifspermissionrequest = db.CifsPermissionRequests.Find(id);
			CifsShare cifsshare = db.CifsShares.Find(cifspermissionrequest.CifsShareID);

			if(cifsshare.Status != Status.InService)
			{
				return View("OutOfServiceError");
			}

			// if the request has already been approved, redirect the user to the Index page which is actually
			// provides the details for requests.
			if (cifspermissionrequest.RequestApprovalStatus != null)
			{
				return RedirectToAction("Index", new { id = cifspermissionrequest.CifsPermissionRequestID });
			}

			// convert Request Approval Status enum into a Select List. (Approve or deny request)
			ViewBag.RequestApprovalStatus = EnumHelper.GetSelectList<RequestApprovalStatus>();

			ViewBag.RequestForMembers = null;

			// if this request is for a security group, get a list of that security groups members.
			if (cifspermissionrequest.RequestAdPrincipalType == RequestAdPrincipalType.SecurityGroup)
			{
				ViewBag.RequestForMembers = AdHelper.GetAdGroupMembers(cifspermissionrequest.RequestedForUserAlias,cifspermissionrequest.CifsShare.Ou.Domain.ToString()) as List<AdUser>;
			}

			// authenticate the user (are they a member of the Owners Group?
			if (AuthenticateUser(cifspermissionrequest, HttpContext.User.Identity.Name.Substring(3)))
			{
				return View(cifspermissionrequest);
			}
			else
			{
				return View("AuthError");
			}
		}

		// POST: /owner/SmbRequest/Details/5
		// Used to submit approval or denial of the request
		[HttpPost]
        public ActionResult Details([Bind(Include = "CifsPermissionRequestID,CifsShare,CifsShareID,PermissionType,RequestType,RequestJustification,RequestAdPrincipalType,RequestedForUserAlias,RequestedForUserName,RequestedByUserAlias,RequestedByUserName,,RequestApprovalNotificationTimeStamp,RequestStatus,RequestStatusMsg,RequestApprovalStatus,RequestOpenedOnDateTime")] CifsPermissionRequest cifspermissionrequest)
		{
			// get share information from posted back (BIND) content.
            CifsShare CifsShare = db.CifsShares.Find(cifspermissionrequest.CifsShareID);
            //CifsShare cifsshare = db.CifsShares.Find(cifspermissionrequest.CifsShareID);
            CifsPermissionRequest requestInDbs = db.CifsPermissionRequests.Find(cifspermissionrequest.CifsPermissionRequestID);

            //CifsPermissionRequest cifspermissionrequest = db.CifsPermissionRequests.Find(id);
            //CifsShare cifsshare = db.CifsShares.Find(cifspermissionrequest.CifsShareID);

			// create new request status object (used for reporting details from AD helper)
			AdRequestStatus reqStatus = new AdRequestStatus();

            

			// return an error if their isn't a valid permission request 
			if (cifspermissionrequest == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}

			// get current user's ad information (name, alias)
			var user = AdHelper.GetAdUser(HttpContext.User.Identity.Name.Substring(3));

			// authenticate user
			if (!AuthenticateUser(cifspermissionrequest, HttpContext.User.Identity.Name.Substring(3)))
			{
				return View("AuthError");
			}

            if(requestInDbs.RequestApprovalStatus != null)
            {
                return RedirectToAction("Index", new { id = cifspermissionrequest.CifsPermissionRequestID });
            }

            ModelState.Remove("CifsShare");

            if(cifspermissionrequest.RequestStatusMsg == null)
            {
            ModelState.AddModelError("RequestStatusMsg", "Value is required");
            }

            if (cifspermissionrequest.RequestApprovalStatus == null)
            {
                ModelState.AddModelError("RequestApprovalStatus", "Please provide your decision");
            }

            if (!(ModelState.IsValid))
            {


                cifspermissionrequest.CifsShare = CifsShare;
                
                ViewBag.RequestApprovalStatus = EnumHelper.GetSelectList<RequestApprovalStatus>();

                ViewBag.RequestForMembers = null;

                // if this request is for a security group, get a list of that security groups members.
                if (cifspermissionrequest.RequestAdPrincipalType == RequestAdPrincipalType.SecurityGroup)
                {
                    ViewBag.RequestForMembers = AdHelper.GetAdGroupMembers(cifspermissionrequest.RequestedForUserAlias,cifspermissionrequest.CifsShare.Ou.Domain.ToString()) as List<AdUser>;
                }

                // authenticate the user (are they a member of the Owners Group?
                if (AuthenticateUser(cifspermissionrequest, HttpContext.User.Identity.Name.Substring(3)))
                {
                    return View(cifspermissionrequest);
                }
                else
                {
                    return View("AuthError");
                }
            }

            if (cifspermissionrequest.RequestApprovalStatus == null)
            {
                //cifspermissionrequest.RequestStatusMsg;
               
                List<SelectListItem> apprstatus = Helpers.EnumHelper.GetSelectList<RequestApprovalStatus>();
                cifspermissionrequest.CifsShare = CifsShare;
                ViewBag.RequestApprovalStatus = apprstatus;
                ViewData["OwnerMadeDeicison"] = false;
                return View(cifspermissionrequest);
            }
			// if status is approved, do the following...
			else if (cifspermissionrequest.RequestApprovalStatus == RequestApprovalStatus.Approved)
			{
				// if request type is add, call AddUserToGroupMethod 
				if (cifspermissionrequest.RequestType == RequestType.Add)
				{
					//determin the correct group based on PermissionType
					if(cifspermissionrequest.PermissionType == PermissionType.RO)
					{
						// if user user AddUserToGroup call otherwise, use AddGroupToGroup call
						if(cifspermissionrequest.RequestAdPrincipalType == RequestAdPrincipalType.User)
						{ 
							reqStatus = AdHelper.AddUserToGroup(cifspermissionrequest.RequestedForUserAlias, CifsShare.ReadOnlyGroup, 
								CifsShare.Ou.Domain.ToString(), CifsShare.Ou.OrganizationalUnit,CifsShare,cifspermissionrequest);
						}
						else
						{
							reqStatus = AdHelper.AddGroupToGroup(cifspermissionrequest.RequestedForUserAlias, CifsShare.ReadOnlyGroup,
                                CifsShare.Ou.Domain.ToString(), CifsShare.Ou.OrganizationalUnit, CifsShare, cifspermissionrequest);
						}
					}
					else if (cifspermissionrequest.PermissionType == PermissionType.RW)
					{
						// if user user AddUserToGroup call otherwise, use AddGroupToGroup call
						if (cifspermissionrequest.RequestAdPrincipalType == RequestAdPrincipalType.User)
						{
							reqStatus = AdHelper.AddUserToGroup(cifspermissionrequest.RequestedForUserAlias, CifsShare.ReadWriteGroup,
                                CifsShare.Ou.Domain.ToString(), CifsShare.Ou.OrganizationalUnit, CifsShare, cifspermissionrequest);
						}
						else
						{
							reqStatus = AdHelper.AddGroupToGroup(cifspermissionrequest.RequestedForUserAlias, CifsShare.ReadWriteGroup,
                                CifsShare.Ou.Domain.ToString(), CifsShare.Ou.OrganizationalUnit, CifsShare, cifspermissionrequest);
						}
					}
					else if (cifspermissionrequest.PermissionType == PermissionType.NC)
					{
						// if user user AddUserToGroup call otherwise, use AddGroupToGroup call
						if (cifspermissionrequest.RequestAdPrincipalType == RequestAdPrincipalType.User)
						{
							reqStatus = AdHelper.AddUserToGroup(cifspermissionrequest.RequestedForUserAlias, CifsShare.NoChangeGroup,
                                CifsShare.Ou.Domain.ToString(), CifsShare.Ou.OrganizationalUnit, CifsShare, cifspermissionrequest);
						}
						else
						{
							reqStatus = AdHelper.AddGroupToGroup(cifspermissionrequest.RequestedForUserAlias, CifsShare.NoChangeGroup,
                                CifsShare.Ou.Domain.ToString(), CifsShare.Ou.OrganizationalUnit, CifsShare, cifspermissionrequest);
						}
					}
					else if (cifspermissionrequest.PermissionType == PermissionType.GK)
					{
						// if user user AddUserToGroup call otherwise, use AddGroupToGroup call
						if (cifspermissionrequest.RequestAdPrincipalType == RequestAdPrincipalType.User)
						{
							reqStatus = AdHelper.AddUserToGroup(cifspermissionrequest.RequestedForUserAlias, CifsShare.OwnerGroup,
                                CifsShare.Ou.Domain.ToString(), CifsShare.Ou.OrganizationalUnit, CifsShare, cifspermissionrequest);
						}
						else
						{
							reqStatus = AdHelper.AddGroupToGroup(cifspermissionrequest.RequestedForUserAlias, CifsShare.OwnerGroup,
                                CifsShare.Ou.Domain.ToString(), CifsShare.Ou.OrganizationalUnit, CifsShare, cifspermissionrequest);
						}
					}

					// set request status to completed if reqStatus not null and reqStatus.Successfull is true
					if(reqStatus != null && reqStatus.Successfull == true)
					{
						cifspermissionrequest.RequestStatus = RequestStatus.Completed;
					}
					else
					{
						cifspermissionrequest.RequestStatus = RequestStatus.Failed;	         
					}

				}
				else if (cifspermissionrequest.RequestType == RequestType.Remove)
				{
					//determin the correct group based on PermissionType
					if(cifspermissionrequest.PermissionType == PermissionType.RO)
					{
						if (cifspermissionrequest.RequestAdPrincipalType == RequestAdPrincipalType.User)
						{
							reqStatus = AdHelper.RemoveUserFromGroup(cifspermissionrequest.RequestedForUserAlias, CifsShare.ReadOnlyGroup,
                                CifsShare.Ou.Domain.ToString(), CifsShare.Ou.OrganizationalUnit, CifsShare, cifspermissionrequest);
						}
						else
						{
							reqStatus = AdHelper.RemoveGroupFromGroup(cifspermissionrequest.RequestedForUserAlias, CifsShare.ReadOnlyGroup,
                                CifsShare.Ou.Domain.ToString(), CifsShare.Ou.OrganizationalUnit, CifsShare, cifspermissionrequest);
						}
					}
					else if (cifspermissionrequest.PermissionType == PermissionType.RW)
					{
						if (cifspermissionrequest.RequestAdPrincipalType == RequestAdPrincipalType.User)
						{
							reqStatus = AdHelper.RemoveUserFromGroup(cifspermissionrequest.RequestedForUserAlias, CifsShare.ReadWriteGroup,
                                CifsShare.Ou.Domain.ToString(), CifsShare.Ou.OrganizationalUnit, CifsShare, cifspermissionrequest);
						}
						else
						{
							reqStatus = AdHelper.RemoveGroupFromGroup(cifspermissionrequest.RequestedForUserAlias, CifsShare.ReadWriteGroup,
                                CifsShare.Ou.Domain.ToString(), CifsShare.Ou.OrganizationalUnit, CifsShare, cifspermissionrequest);
						}
					}
					else if (cifspermissionrequest.PermissionType == PermissionType.NC)
					{
						if (cifspermissionrequest.RequestAdPrincipalType == RequestAdPrincipalType.User)
						{
							reqStatus = AdHelper.RemoveUserFromGroup(cifspermissionrequest.RequestedForUserAlias, CifsShare.NoChangeGroup,
                                CifsShare.Ou.Domain.ToString(), CifsShare.Ou.OrganizationalUnit, CifsShare, cifspermissionrequest);
						}
						else
						{
							reqStatus = AdHelper.RemoveGroupFromGroup(cifspermissionrequest.RequestedForUserAlias, CifsShare.NoChangeGroup,
                                CifsShare.Ou.Domain.ToString(), CifsShare.Ou.OrganizationalUnit, CifsShare, cifspermissionrequest);
						}
					}
					else if (cifspermissionrequest.PermissionType == PermissionType.GK)
					{
						if (cifspermissionrequest.RequestAdPrincipalType == RequestAdPrincipalType.User)
						{
							reqStatus = AdHelper.RemoveUserFromGroup(cifspermissionrequest.RequestedForUserAlias, CifsShare.OwnerGroup,
                                CifsShare.Ou.Domain.ToString(), CifsShare.Ou.OrganizationalUnit, CifsShare, cifspermissionrequest);
						}
						else
						{
							reqStatus = AdHelper.RemoveGroupFromGroup(cifspermissionrequest.RequestedForUserAlias, CifsShare.OwnerGroup,
                                CifsShare.Ou.Domain.ToString(), CifsShare.Ou.OrganizationalUnit, CifsShare, cifspermissionrequest);
						}
					}

					// set request status to completed if reqStatus not null and reqStatus.Successfull is true
					if(reqStatus != null && reqStatus.Successfull == true)
					{
						cifspermissionrequest.RequestStatus = RequestStatus.Completed;
					}
					else
					{
						cifspermissionrequest.RequestStatus = RequestStatus.Failed;	         
					}
				}
			}
			else
			{
				// must be completed.
				cifspermissionrequest.RequestStatus = RequestStatus.Completed;
			}

			// if request status item is null then there was an issue.
			if (reqStatus == null)
			{
				reqStatus.Successfull = false;
				reqStatus.UserMessage = "Unable to determine Request.";
				reqStatus.ErrorMessage = "Failed to determine PermissionType or RequestType in Logic.";

			}

			// set values 
			cifspermissionrequest.ClosedByUserAlias = user.UserAlias;
			cifspermissionrequest.ClosedByUserName = user.UserName;
			cifspermissionrequest.RequestClosedOnDateTime = System.DateTime.Now;
			cifspermissionrequest.RequestStatusMsg += " " + reqStatus.UserMessage;

            cifspermissionrequest.CifsShare = CifsShare ;


			// if value of error message isn't null, set the value
			if(!string.IsNullOrEmpty(reqStatus.ErrorMessage))
			{
				cifspermissionrequest.AdHelperErrorMsg = reqStatus.ErrorMessage;
			}

			//// Email user, double check that value isn't false and try again if yes.
			bool email = false;
			email = EmailHelper.SendRequestCompletedEmailMessage(cifspermissionrequest, CifsShare);
			if (!email)
			{
				email = EmailHelper.SendRequestCompletedEmailMessage(cifspermissionrequest, CifsShare);
			}

			cifspermissionrequest.RequestClosedNotification = email;

			// quickly get another copy of this request.
			var checkrequest = db.CifsPermissionRequests.Find(cifspermissionrequest.CifsPermissionRequestID);

			// double check that the request wasn't already approved.
			if (checkrequest.RequestApprovalStatus == null)
			{
				// if not already approve, submit changes.
				// because I got another copy of the request, I had to update the last pull of the data
				// with the results from the post value.
                //try
                //{
                    db.Entry(checkrequest).CurrentValues.SetValues(cifspermissionrequest);
                    // db.Entry().State = EntityState.Modified; REMVOED for CONcurrency
                    var userId = HttpContext.User.Identity.Name.Substring(3);
                    db.SaveChanges(userId);

                //}
                //catch (DbEntityValidationException dbEx)
                //{
                //    foreach (var validationErrors in dbEx.EntityValidationErrors)
                //    {
                //        foreach (var validationError in validationErrors.ValidationErrors)
                //        {
                //            Trace.TraceInformation("Property: {0} Error: {1}",
                //                                    validationError.PropertyName,
                //                                    validationError.ErrorMessage);
                //        }
                //    }
                //}
                    //UpdateRequest(cifspermissionrequest);
                    return RedirectToAction("Index", new { id = cifspermissionrequest.CifsPermissionRequestID });

			}
			else
			{
				// if alreayd approved, redirect user to Index page to see approval details.
				return RedirectToAction("Index", new { id = cifspermissionrequest.CifsPermissionRequestID }); 
			}
		}

		

		// GET: /owner/SmbRequest/Create
		public ActionResult Create()
		{
			ViewBag.CifsShareID = new SelectList(db.CifsShares, "CifsShareID", "Name");
			return View();
		}

		// POST: /owner/SmbRequest/Create
		// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
		// more details see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Create([Bind(Include="CifsPermissionRequestID,CifsShareID,PermissionType,RequestType,RequestJustification,RequestedForUserAlias,RequestedForUserName,RequestedByUserAlias,RequestedByUserName,RequestStatus,RequestStatusMsg,RequestOpenedOnDateTime,RequestClosedOnDateTime")] CifsPermissionRequest cifspermissionrequest)
		{

			
			if (ModelState.IsValid)
			{
				db.CifsPermissionRequests.Add(cifspermissionrequest);
                var userId = HttpContext.User.Identity.Name.Substring(3);
                db.SaveChanges(userId);
				return RedirectToAction("Index");
			}

			ViewBag.CifsShareID = new SelectList(db.CifsShares, "CifsShareID", "Name", cifspermissionrequest.CifsShareID);
			return View(cifspermissionrequest);
		}

		//// GET: /owner/SmbRequest/Edit/5
		//public ActionResult Edit(int? id)
		//{
		//    if (id == null)
		//    {
		//        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
		//    }
		//    CifsPermissionRequest cifspermissionrequest = db.CifsPermissionRequests.Find(id);
		//    if (cifspermissionrequest == null)
		//    {
		//        return HttpNotFound();
		//    }
		//    ViewBag.CifsShareID = new SelectList(db.CifsShares, "CifsShareID", "Name", cifspermissionrequest.CifsShareID);
		//    return View(cifspermissionrequest);
		//}

		//// POST: /owner/SmbRequest/Edit/5
		//// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
		//// more details see http://go.microsoft.com/fwlink/?LinkId=317598.
		//[HttpPost]
		//[ValidateAntiForgeryToken]
		//public ActionResult Edit([Bind(Include="CifsPermissionRequestID,CifsShareID,PermissionType,RequestType,RequestJustification,RequestedForUserAlias,RequestedForUserName,RequestedByUserAlias,RequestedByUserName,RequestStatus,RequestStatusMsg,RequestOpenedOnDateTime,RequestClosedOnDateTime")] CifsPermissionRequest cifspermissionrequest)
		//{
		//    if (ModelState.IsValid)
		//    {
		//        db.Entry(cifspermissionrequest).State = EntityState.Modified;
		//        db.SaveChanges();
		//        return RedirectToAction("Index");
		//    }
		//    ViewBag.CifsShareID = new SelectList(db.CifsShares, "CifsShareID", "Name", cifspermissionrequest.CifsShareID);
		//    return View(cifspermissionrequest);
		//}

		// GET: /owner/SmbRequest/Delete/5
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

		// POST: /owner/SmbRequest/Delete/5
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
