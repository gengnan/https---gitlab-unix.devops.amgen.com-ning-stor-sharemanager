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
using ShareManagerEdgeMvc.DAL;
using ShareManagerEdgeMvc.Helpers;

namespace ShareManagerEdgeMvc.Areas.user.Controllers
{
    public class SmbRequestController : ShareManagerEdgeMvc.Controllers.BaseController
    {
        private ShareContext db = new ShareContext();

        // GET: /user/SmbRequest/
        public ActionResult Index()
        {
            var user = HttpContext.User.Identity.Name.Substring(3);

            //get all requests where status is RequestStatus.Open
            var cifspermissionrequests = db.CifsPermissionRequests.Where(c=> (c.RequestedByUserAlias.ToUpper().Equals(user.ToUpper()) ||
                c.RequestedForUserAlias.ToUpper().Equals(user.ToUpper()))
                && c.RequestStatus == RequestStatus.Open).Include(c => c.CifsShare).OrderByDescending(c => c.RequestOpenedOnDateTime);

            //get all requests where status isn't RequestStatus.Open
            var completedRequests = db.CifsPermissionRequests.Include(c => c.CifsShare);

            completedRequests = completedRequests.Where(c => (c.RequestedByUserAlias.ToUpper().Equals(user.ToUpper()) ||
                c.RequestedForUserAlias.ToUpper().Equals(user.ToUpper()))
                && c.RequestStatus != RequestStatus.Open);

            completedRequests = completedRequests.OrderByDescending(c => c.RequestClosedOnDateTime);

            // add completed requests as ViewData
            ViewData["CompletedRequests"] = completedRequests.ToList();

            return View(cifspermissionrequests.ToList());
        }

        // GET: /user/SmbRequest/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CifsPermissionRequest cifspermissionrequest = await db.CifsPermissionRequests.FindAsync(id);
            if (cifspermissionrequest == null)
            {
                return HttpNotFound();
            }
            ViewBag.AdType = EnumHelper.GetSelectList<RequestAdPrincipalType>();
            return View(cifspermissionrequest);
        }

        // GET: /user/SmbRequest/User/1
        // creates a request to add or remove a user from share permissions
        [HttpGet, ActionName("UserRequest")]
        public ActionResult UserRequest(int id)
        {
           
            ViewBag.CifsShareID = id;
            ViewData["IsDropDown"] = false;
            ViewData["UserAlreadyHasAccess"] = false;
            ViewData["CifsShareData"] = db.CifsShares.Find(id) as ShareManagerEdgeMvc.Models.CifsShare;
            List<SelectListItem> permlist = Helpers.EnumHelper.GetSelectList<PermissionType>();
            
            ViewBag.RequestType = EnumHelper.GetSelectList<RequestType>();
            ViewBag.RequestedForUserAlias = HttpContext.User.Identity.Name.Substring(3);
            ViewBag.CifsShare = db.CifsShares.Find(id);


            var auth = new AuthenticationHelper();
            var user = AdHelper.GetAdUser(HttpContext.User.Identity.Name.Substring(3));

            bool isShareAdmin= auth.IsShareAdmin(user.UserAlias);
            bool isSiteAdmin = auth.IsSiteAdmin(user.UserAlias);
            ViewData["IsShareAdmin"] = auth.IsShareAdmin(user.UserAlias);
            ViewData["IsSiteAdmin"] = auth.IsSiteAdmin(user.UserAlias);


            ViewData["ncgroupname"] = ViewBag.CifsShare.NoChangeGroup;
            // sometimes need a retry
            var owners = AdHelper.GetAdGroupMembers(ViewBag.CifsShare.OwnerGroup, ViewBag.CifsShare.Ou.Domain.ToString()); //hit
            if (owners == null)
            {
                owners = AdHelper.GetAdGroupMembers(ViewBag.CifsShare.OwnerGroup, ViewBag.CifsShare.Ou.Domain.ToString()); //hit
            }
            ViewData["Owners"] = owners;

            if (!isShareAdmin && !isSiteAdmin)
            {
                permlist.Remove(permlist.First(e => (e.Value.Equals("3"))));   
            }

            if (!string.IsNullOrEmpty(ViewBag.CifsShare.ReadOnlyGroup))
            {
                var readers = AdHelper.GetAllAdGroupMembers(ViewBag.CifsShare.ReadOnlyGroup, ViewBag.CifsShare.Ou.Domain.ToString());
                if (readers == null)
                {
                    readers = AdHelper.GetAllAdGroupMembers(ViewBag.CifsShare.ReadOnlyGroup, ViewBag.CifsShare.Ou.Domain.ToString());
                }
                ViewData["Readers"] = readers;
            }
            else
            { 
                permlist.Remove(permlist.First(e=> (e.Value.Equals("0"))));
                //permlist.RemoveAt(((int)Enum.Parse(typeof(PermissionType), PermissionType.RO.ToString()))); 
            }
            // get list of writers with retry
            if (!string.IsNullOrEmpty(ViewBag.CifsShare.ReadWriteGroup))
            {
                var writers = AdHelper.GetAllAdGroupMembers(ViewBag.CifsShare.ReadWriteGroup, ViewBag.CifsShare.Ou.Domain.ToString());
                if (writers == null)
                {
                    writers = AdHelper.GetAllAdGroupMembers(ViewBag.CifsShare.ReadWriteGroup, ViewBag.CifsShare.Ou.Domain.ToString());
                }
                ViewData["Writers"] = writers;
            }
            else
            {
                permlist.Remove(permlist.First(e => (e.Value.Equals("1"))));
                //permlist.RemoveAt(((int)Enum.Parse(typeof(PermissionType), PermissionType.RW.ToString()))); 
            }
            // not required so only do the work if it's not null
            if (!string.IsNullOrEmpty(ViewBag.CifsShare.NoChangeGroup))
            {
                var nochangers = AdHelper.GetAllAdGroupMembers(ViewBag.CifsShare.NoChangeGroup, ViewBag.CifsShare.Ou.Domain.ToString());
                if (nochangers == null)
                {
                    nochangers = AdHelper.GetAllAdGroupMembers(ViewBag.CifsShare.NoChangeGroup, ViewBag.CifsShare.Ou.Domain.ToString());
                }
                ViewData["NoChangers"] = nochangers;
            }
            else
            {
                // remove the NC from selection process
                permlist.Remove(permlist.First(e => (e.Value.Equals("2"))));
                //permlist.RemoveAt(((int)Enum.Parse(typeof(PermissionType), PermissionType.NC.ToString())));
            }
            ViewBag.PermissionType = permlist;
            ViewData["PermTypeCount"] = permlist.Count;
            return View();
        }

        // GET: /user/SmbRequest/Group/1
        // creates a request to add or remove a security group from share permissions
        [HttpGet, ActionName("GroupRequest")]
        public ActionResult Group(int id)
        {

            ViewBag.CifsShareID = id;
            ViewData["IsDropDown"] = false;
            ViewData["UserAlreadyHasAccess"] = false;
            ViewData["CifsShareData"] = db.CifsShares.Find(id) as ShareManagerEdgeMvc.Models.CifsShare;
            List<SelectListItem> permlist = Helpers.EnumHelper.GetSelectList<PermissionType>();
            // remove the Owner/Gatekeeper from selection process
            permlist.RemoveAt(((int)Enum.Parse(typeof(PermissionType), PermissionType.GK.ToString())));
            
            
            ViewBag.RequestType = EnumHelper.GetSelectList<RequestType>();
            ViewBag.RequestedForUserAlias = HttpContext.User.Identity.Name.Substring(3);
            ViewBag.CifsShare = db.CifsShares.Find(id);
            ViewData["ncgroupname"] = ViewBag.CifsShare.NoChangeGroup;

            var auth = new AuthenticationHelper();
            var user = AdHelper.GetAdUser(HttpContext.User.Identity.Name.Substring(3));

            bool isShareAdmin = auth.IsShareAdmin(user.UserAlias);
            bool isSiteAdmin = auth.IsSiteAdmin(user.UserAlias);
            ViewData["IsShareAdmin"] = auth.IsShareAdmin(user.UserAlias);
            ViewData["IsSiteAdmin"] = auth.IsSiteAdmin(user.UserAlias);

            // sometimes need a retry
            var owners = AdHelper.GetAdGroupMembers(ViewBag.CifsShare.OwnerGroup, ViewBag.CifsShare.Ou.Domain.ToString()); //hit
            if (owners == null)
            {
                owners = AdHelper.GetAdGroupMembers(ViewBag.CifsShare.OwnerGroup, ViewBag.CifsShare.Ou.Domain.ToString());
            }
            ViewData["Owners"] = owners;

            if (!string.IsNullOrEmpty(ViewBag.CifsShare.ReadOnlyGroup))
            {
                var readers = AdHelper.GetAllAdGroupMembers(ViewBag.CifsShare.ReadOnlyGroup, ViewBag.CifsShare.Ou.Domain.ToString());
                if (readers == null)
                {
                    readers = AdHelper.GetAllAdGroupMembers(ViewBag.CifsShare.ReadOnlyGroup, ViewBag.CifsShare.Ou.Domain.ToString());
                }
                ViewData["Readers"] = readers;
            }
            else { permlist.Remove(permlist.First(e => (e.Value.Equals("0")))); }
            // get list of writers with retry
            if (!string.IsNullOrEmpty(ViewBag.CifsShare.ReadWriteGroup))
            {
                var writers = AdHelper.GetAllAdGroupMembers(ViewBag.CifsShare.ReadWriteGroup, ViewBag.CifsShare.Ou.Domain.ToString());
                if (writers == null)
                {
                    writers = AdHelper.GetAllAdGroupMembers(ViewBag.CifsShare.ReadWriteGroup, ViewBag.CifsShare.Ou.Domain.ToString());
                }
                ViewData["Writers"] = writers;
            }
            else
            { permlist.Remove(permlist.First(e => (e.Value.Equals("1")))); }
            // not required so only do the work if it's not null
            if (!string.IsNullOrEmpty(ViewBag.CifsShare.NoChangeGroup))
            {
                var nochangers = AdHelper.GetAllAdGroupMembers(ViewBag.CifsShare.NoChangeGroup, ViewBag.CifsShare.Ou.Domain.ToString());
                if (nochangers == null)
                {
                    nochangers = AdHelper.GetAllAdGroupMembers(ViewBag.CifsShare.NoChangeGroup, ViewBag.CifsShare.Ou.Domain.ToString());
                }
                ViewData["NoChangers"] = nochangers;
            }
            else
            {
                // remove the NC from selection process
                permlist.Remove(permlist.First(e => (e.Value.Equals("2"))));
                //permlist.RemoveAt(((int)Enum.Parse(typeof(PermissionType), PermissionType.NC.ToString())));
            }
            ViewBag.PermissionType = permlist;
            return View();
        }


        // POST: /user/SmbRequest/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost, ActionName("UserRequest")]
        [ValidateAntiForgeryToken]
        public ActionResult UserRequest([Bind(Include = "CifsPermissionRequestID,PermissionType,RequestType,RequestJustification,RequestAdPrincipalType,RequestedForUserAlias,RequestedForUserName,RequestedByUserAlias,RequestedByUserName,RequestStatus,RequestStatusMsg,RequestOpenedOnDateTime,RequestClosedOnDateTime")] CifsPermissionRequest cifspermissionrequest, int id)
        {
            List<SelectListItem> permlist = Helpers.EnumHelper.GetSelectList<PermissionType>();
            CifsShare cifsshare = null;
            cifsshare = db.CifsShares.Find(id);
            cifspermissionrequest.CifsShareID = id;
            ViewBag.RequestType = EnumHelper.GetSelectList<RequestType>();



            var towners = AdHelper.GetAdGroupMembers(cifsshare.OwnerGroup,cifsshare.Ou.Domain.ToString()); //hit
            // this tests the state of the Model to ensure it's correct.

            if (ModelState.IsValid)
            {
                cifspermissionrequest.RequestAdPrincipalType = RequestAdPrincipalType.User;
                // create ad user for current site user and another with same value
                AdUser requestedBy = AdHelper.GetAdUser(HttpContext.User.Identity.Name.Substring(3));
                AdUser requestedFor = requestedBy;
                ViewData["UserAlreadyHasAccess"] = false;
                // fix up requested by
                if (!string.IsNullOrEmpty(cifspermissionrequest.RequestedForUserAlias) && towners.Count != 0)
                {
                    cifspermissionrequest.RequestedForUserAlias = cifspermissionrequest.RequestedForUserAlias.Trim();
                    if (cifspermissionrequest.RequestedForUserAlias.Contains("\\"))
                    {
                        cifspermissionrequest.RequestedForUserAlias = cifspermissionrequest.RequestedForUserAlias.Substring(cifspermissionrequest.RequestedForUserAlias.IndexOf("\\") + 1).Trim();
                    }
                    if (AdHelper.IsUserAliasValid(cifspermissionrequest.RequestedForUserAlias))
                    {
                        // if requested by and for are identical no need to overwrite requestedFor AdUser
                        if (cifspermissionrequest.RequestedForUserAlias.ToUpper() != HttpContext.User.Identity.Name.Substring(3).ToUpper())
                        {
                            requestedFor = AdHelper.GetAdUser(cifspermissionrequest.RequestedForUserAlias);
                        }

                        // is there a request for this share for this user alias with the same permission
                        var exists = db.CifsPermissionRequests.Where(c => c.PermissionType.Value == cifspermissionrequest.PermissionType &&
                            c.RequestType.Value == cifspermissionrequest.RequestType &&
                            c.RequestAdPrincipalType.Value == RequestAdPrincipalType.User &&
                            c.RequestedForUserAlias == cifspermissionrequest.RequestedForUserAlias &&
                            c.RequestStatus.Value == RequestStatus.Open &&
                            c.CifsShareID == id);

                        cifsshare = db.CifsShares.Find(cifspermissionrequest.CifsShareID);

                        // only add a new request if one doesn't exist for the user.
                        if (exists == null || exists.Count() == 0)
                        {

                            // set remaining values
                            cifspermissionrequest.RequestedForUserName = requestedFor.UserName;
                            cifspermissionrequest.RequestedByUserAlias = requestedBy.UserAlias;
                            cifspermissionrequest.RequestedByUserName = requestedBy.UserName;
                            cifspermissionrequest.RequestStatus = RequestStatus.Open;
                            cifspermissionrequest.RequestOpenedOnDateTime = System.DateTime.Now;
                            cifspermissionrequest.CifsShareID = id;


                            // check if the request is valid for the Security Group
                            if (IsPermissionRequestValidForSecurityGroup(cifsshare, cifspermissionrequest))
                            {

                                if (!(cifspermissionrequest.RequestType == RequestType.Remove && cifspermissionrequest.PermissionType == PermissionType.GK && towners.Count == 1))
                                {
                                    // Add new permission request
                                    db.CifsPermissionRequests.Add(cifspermissionrequest);
                                    var userId = HttpContext.User.Identity.Name.Substring(3);
                                    db.SaveChanges(userId);

                                    // get share information for use by Emailer
                                    cifsshare = db.CifsShares.Find(id);
                                    // notify user
                                    bool email = EmailHelper.SendNewRequestNotificationMessage(cifspermissionrequest, cifsshare);
                                    if (email)
                                    {
                                        // update request with notification time stamp
                                        cifspermissionrequest.RequestApprovalNotificationTimeStamp = System.DateTime.Now;
                                        db.Entry(cifspermissionrequest).State = EntityState.Modified;
                                        //var userId = HttpContext.User.Identity.Name.Substring(3);
                                        db.SaveChanges(userId);
                                    }

                                    // If you got here, you passed all checks and added new request to db.
                                    // now redirect user to the Details page for that Share.
                                    return RedirectToAction("Details", "SmbShares", new { id = cifspermissionrequest.CifsShareID });
                                }
                            } // end if permission request 

                        } // end if exists
                    }//end if user valid
                }//end if null or empty


            } // model state is not valid
            ViewData["UserAlreadyHasAccess"] = true;

            // if you got here it's because either the model state wasn't valid OR
            // a duplicate request already exists OR
            // the permission request wasn't valid for the security group

            // get necessary items for redisplay on request form page
            ViewBag.CifsShareID = new SelectList(db.CifsShares, "CifsShareID", "Name", cifspermissionrequest.CifsShareID);

            ViewData["ncgroupname"] = cifsshare.NoChangeGroup;

            var auth = new AuthenticationHelper();
            var user = AdHelper.GetAdUser(HttpContext.User.Identity.Name.Substring(3));

            bool isShareAdmin = auth.IsShareAdmin(user.UserAlias);
            bool isSiteAdmin = auth.IsSiteAdmin(user.UserAlias);
            ViewData["IsShareAdmin"] = auth.IsShareAdmin(user.UserAlias);
            ViewData["IsSiteAdmin"] = auth.IsSiteAdmin(user.UserAlias);



            // sometimes need a retry
           // var towners = AdHelper.GetAdGroupMembers(cifsshare.OwnerGroup);
            if (towners == null)
            {
                towners = AdHelper.GetAdGroupMembers(cifsshare.OwnerGroup,cifsshare.Ou.Domain.ToString()); //hit
            }
            ViewData["Owners"] = towners;

            if (!isShareAdmin && !isSiteAdmin)
            {
                permlist.Remove(permlist.First(e => (e.Value.Equals("3"))));
            }


            if (!string.IsNullOrEmpty(cifsshare.ReadOnlyGroup))
            {
                var treaders = AdHelper.GetAllAdGroupMembers(cifsshare.ReadOnlyGroup, cifsshare.Ou.Domain.ToString());
                if (treaders == null)
                {
                    treaders = AdHelper.GetAllAdGroupMembers(cifsshare.ReadOnlyGroup, cifsshare.Ou.Domain.ToString());
                }
                ViewData["Readers"] = treaders;
            }
            else
            { permlist.Remove(permlist.First(e => (e.Value.Equals("0")))); }
            // get list of writers with retry
            if (!string.IsNullOrEmpty(cifsshare.ReadWriteGroup))
            {
                var twriters = AdHelper.GetAllAdGroupMembers(cifsshare.ReadWriteGroup, cifsshare.Ou.Domain.ToString());
                if (twriters == null)
                {
                    twriters = AdHelper.GetAllAdGroupMembers(cifsshare.ReadWriteGroup, cifsshare.Ou.Domain.ToString());
                }
                ViewData["Writers"] = twriters;
            }
            else
            { permlist.Remove(permlist.First(e => (e.Value.Equals("1")))); }
            // not required so only do the work if it's not null
            if (!string.IsNullOrEmpty(cifsshare.NoChangeGroup))
            {
                var nochangers = AdHelper.GetAllAdGroupMembers(cifsshare.NoChangeGroup, cifsshare.Ou.Domain.ToString());
                if (nochangers == null)
                {
                    nochangers = AdHelper.GetAllAdGroupMembers(cifsshare.NoChangeGroup, cifsshare.Ou.Domain.ToString());
                }
                ViewData["NoChangers"] = nochangers;
            }
            else
            {
                // remove the NC from selection process
                //permlist.RemoveAt(((int)Enum.Parse(typeof(PermissionType), PermissionType.NC.ToString())));
                permlist.Remove(permlist.First(e => (e.Value.Equals("2"))));
            }
            ViewData["CifsShareData"] = cifsshare;
            ViewBag.PermissionType = permlist;

            ViewData["PermTypeCount"] = permlist.Count;

            ViewData["CurrentRequestFor"] = cifspermissionrequest.RequestedForUserAlias;

            return View(cifspermissionrequest);
        }


        // POST: /user/SmbRequest/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost, ActionName("GroupRequest")]
        [ValidateAntiForgeryToken]
        public ActionResult GroupRequest([Bind(Include = "CifsPermissionRequestID,CifsShareID,PermissionType,RequestType,RequestJustification,RequestAdPrincipalType,RequestedForUserAlias,RequestedForUserName,RequestedByUserAlias,RequestedByUserName,RequestStatus,RequestStatusMsg,RequestOpenedOnDateTime,RequestClosedOnDateTime")] CifsPermissionRequest cifspermissionrequest, int id)
        {
            List<SelectListItem> permlist = Helpers.EnumHelper.GetSelectList<PermissionType>();
            CifsShare cifsshare = null;
            cifsshare = db.CifsShares.Find(id);
            cifspermissionrequest.CifsShareID = id;
            ViewBag.RequestType = EnumHelper.GetSelectList<RequestType>();
            var towners = AdHelper.GetAdGroupMembers(cifsshare.OwnerGroup,cifsshare.Ou.Domain.ToString());

            if (ModelState.IsValid)
            {
                cifspermissionrequest.RequestAdPrincipalType = RequestAdPrincipalType.SecurityGroup;
                // create ad user for current site user and another with same value
                AdUser requestedBy = AdHelper.GetAdUser(HttpContext.User.Identity.Name.Substring(3));
                //AdUser requestedFor = requestedBy;
                ViewData["UserAlreadyHasAccess"] = false;
                // fix up requested by

                if (!string.IsNullOrEmpty(cifspermissionrequest.RequestedForUserAlias) && towners.Count != 0)
                { 
                    cifspermissionrequest.RequestedForUserAlias = cifspermissionrequest.RequestedForUserAlias.Trim();
                    if (cifspermissionrequest.RequestedForUserAlias.Contains("\\"))
                    {
                        cifspermissionrequest.RequestedForUserAlias = cifspermissionrequest.RequestedForUserAlias.Substring(cifspermissionrequest.RequestedForUserAlias.IndexOf("\\") + 1).Trim();
                    }

                    if (AdHelper.IsGroupNameValid(cifspermissionrequest.RequestedForUserAlias)&&AdHelper.IsGroupInDomain(cifspermissionrequest.RequestedForUserAlias,cifsshare.Ou.Domain.ToString()))
                    {
                        // is there a request for this share for this user alias with the same permission
                        var exists = db.CifsPermissionRequests.Where(c => c.PermissionType.Value == cifspermissionrequest.PermissionType &&
                            c.RequestType.Value == cifspermissionrequest.RequestType &&
                            c.RequestAdPrincipalType.Value == RequestAdPrincipalType.SecurityGroup &&
                            c.RequestedForUserAlias == cifspermissionrequest.RequestedForUserAlias &&
                            c.RequestStatus.Value == RequestStatus.Open &&
                            c.CifsShareID == id);

                        cifsshare = db.CifsShares.Find(cifspermissionrequest.CifsShareID); //Yue Added 10/30

                        // only add a new request if one doesn't exist for the user.
                        if (exists == null || exists.Count() == 0)
                        {

                            // set remaining values
                            cifspermissionrequest.RequestedForUserName = cifspermissionrequest.RequestedForUserAlias + " (" + Resources.Resources.SecurityGroup + ")";
                            cifspermissionrequest.RequestedByUserAlias = requestedBy.UserAlias;
                            cifspermissionrequest.RequestedByUserName = requestedBy.UserName;
                            cifspermissionrequest.RequestStatus = RequestStatus.Open;
                            cifspermissionrequest.RequestOpenedOnDateTime = System.DateTime.Now;
                            cifspermissionrequest.CifsShareID = id;

                            // get share infor for email helper
                            if (IsPermissionRequestValidForSecurityGroup(cifsshare, cifspermissionrequest))
                            {
                                if (!(cifspermissionrequest.RequestType == RequestType.Remove && cifspermissionrequest.PermissionType == PermissionType.GK && towners.Count == 1))
                                {
                                    cifsshare = db.CifsShares.Find(id);

                                    // add request to db
                                    db.CifsPermissionRequests.Add(cifspermissionrequest);
                                    var userId = HttpContext.User.Identity.Name.Substring(3);
                                    db.SaveChanges(userId);

                                    // notify user
                                    bool email = EmailHelper.SendNewRequestNotificationMessage(cifspermissionrequest, cifsshare);
                                    if (email)
                                    {
                                        cifspermissionrequest.RequestApprovalNotificationTimeStamp = System.DateTime.Now;
                                        db.Entry(cifspermissionrequest).State = EntityState.Modified;
                                        //var userId = HttpContext.User.Identity.Name.Substring(3);
                                        db.SaveChanges(userId);
                                    }
                                    else
                                    {
                                        // retry
                                        email = EmailHelper.SendNewRequestNotificationMessage(cifspermissionrequest, cifsshare);
                                        if (email)
                                        {
                                            cifspermissionrequest.RequestApprovalNotificationTimeStamp = System.DateTime.Now;
                                            db.Entry(cifspermissionrequest).State = EntityState.Modified;
                                            //var userId = HttpContext.User.Identity.Name.Substring(3);
                                            db.SaveChanges(userId);
                                        }

                                    }
                                    // If you got here, you passed all checks and added new request to db.
                                    // now redirect user to the Details page for that Share.
                                    return RedirectToAction("Details", "SmbShares", new { id = cifspermissionrequest.CifsShareID });
                                }
                            }// end permvalid for group                        

                        } // end if exists
                    
                    }

                } // end !string.IsNullOrEmpty(cifspermissionrequest.RequestedForUserAlias)

            } // end if model state

            ViewData["UserAlreadyHasAccess"] = true; //Yue added 10/30
            // if you've gotten here, it's becuase 1. the model  state is not valid
            //      2. the requested for user alias was empty or null
            //      3. there already exists a similar request.

            // get details for displaying all content on page.
            ViewBag.CifsShareID = new SelectList(db.CifsShares, "CifsShareID", "Name", cifspermissionrequest.CifsShareID);

            ViewData["ncgroupname"] = cifsshare.NoChangeGroup;

            // sometimes need a retry
            //var towners = AdHelper.GetAdGroupMembers(cifsshare.OwnerGroup);
            if (towners == null)
            {
                towners = AdHelper.GetAdGroupMembers(cifsshare.OwnerGroup,cifsshare.Ou.Domain.ToString());
            }
            ViewData["Owners"] = towners;
            permlist.RemoveAt(((int)Enum.Parse(typeof(PermissionType), PermissionType.GK.ToString())));

            if (!string.IsNullOrEmpty(cifsshare.ReadOnlyGroup))
            {
                var treaders = AdHelper.GetAllAdGroupMembers(cifsshare.ReadOnlyGroup, cifsshare.Ou.Domain.ToString());
                if (treaders == null)
                {
                    treaders = AdHelper.GetAllAdGroupMembers(cifsshare.ReadOnlyGroup, ViewBag.CifsShare.Ou.Domain.ToString());
                }
                ViewData["Readers"] = treaders;
            }
            else
            { permlist.Remove(permlist.First(e => (e.Value.Equals("0")))); }
            // get list of writers with retry
            if (!string.IsNullOrEmpty(cifsshare.ReadWriteGroup))
            {
                var twriters = AdHelper.GetAllAdGroupMembers(cifsshare.ReadWriteGroup, cifsshare.Ou.Domain.ToString());
                if (twriters == null)
                {
                    twriters = AdHelper.GetAllAdGroupMembers(cifsshare.ReadWriteGroup, cifsshare.Ou.Domain.ToString());
                }
                ViewData["Writers"] = twriters;
            }
            else
            { permlist.Remove(permlist.First(e => (e.Value.Equals("1")))); }
            // not required so only do the work if it's not null
            if (!string.IsNullOrEmpty(cifsshare.NoChangeGroup))
            {
                var nochangers = AdHelper.GetAllAdGroupMembers(cifsshare.NoChangeGroup, cifsshare.Ou.Domain.ToString());
                if (nochangers == null)
                {
                    nochangers = AdHelper.GetAllAdGroupMembers(cifsshare.NoChangeGroup, cifsshare.Ou.Domain.ToString());
                }
                ViewData["NoChangers"] = nochangers;
            }
            else
            {
                // remove the NC from selection process
                //permlist.RemoveAt(((int)Enum.Parse(typeof(PermissionType), PermissionType.NC.ToString())));
                permlist.Remove(permlist.First(e => (e.Value.Equals("2"))));
            }
            ViewBag.PermissionType = permlist;
            ViewData["CifsShareData"] = cifsshare;
            return View(cifspermissionrequest);
        }

        private bool IsPermissionRequestValidForSecurityGroup(CifsShare share, CifsPermissionRequest request)
        {
            var reqtype = request.RequestType;
            var permtype = request.PermissionType;
            var printype = request.RequestAdPrincipalType;

            // if request is add then ...
            if(reqtype == RequestType.Add)
            {
                // based on permission type, call AdHelper.IsUserMemberOfGroup with right SG and if true then return false because 
                // request is NOT valid. User is already a member so you shouldn't add them.
                if (printype == RequestAdPrincipalType.SecurityGroup)
                {
                    switch (permtype)
                    {
                        case PermissionType.RO:
                            return !AdHelper.IsGroupMemberOfGroup(share.ReadOnlyGroup, share.Ou.Domain.ToString(), share.Ou.OrganizationalUnit, request.RequestedForUserAlias);
                        case PermissionType.RW:
                            return !AdHelper.IsGroupMemberOfGroup(share.ReadWriteGroup, share.Ou.Domain.ToString(), share.Ou.OrganizationalUnit, request.RequestedForUserAlias);
                        case PermissionType.NC:
                            return !AdHelper.IsGroupMemberOfGroup(share.NoChangeGroup, share.Ou.Domain.ToString(), share.Ou.OrganizationalUnit, request.RequestedForUserAlias);
                        case PermissionType.GK:
                            return !AdHelper.IsGroupMemberOfGroup(share.OwnerGroup, share.Ou.Domain.ToString(), share.Ou.OrganizationalUnit, request.RequestedForUserAlias);
                        default:
                            return true;
                    }
                }
                else
                {
                    switch (permtype)
                    {
                        case PermissionType.RO:
                            return !AdHelper.IsUserMemberOfGroup(share.ReadOnlyGroup, share.Ou.Domain.ToString(), share.Ou.OrganizationalUnit, request.RequestedForUserAlias);
                        case PermissionType.RW:
                            return !AdHelper.IsUserMemberOfGroup(share.ReadWriteGroup, share.Ou.Domain.ToString(), share.Ou.OrganizationalUnit, request.RequestedForUserAlias);
                        case PermissionType.NC:
                            return !AdHelper.IsUserMemberOfGroup(share.NoChangeGroup, share.Ou.Domain.ToString(), share.Ou.OrganizationalUnit, request.RequestedForUserAlias);
                        case PermissionType.GK:
                            return !AdHelper.IsUserMemberOfGroup(share.OwnerGroup, share.Ou.Domain.ToString(), share.Ou.OrganizationalUnit, request.RequestedForUserAlias);
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
                            return AdHelper.IsGroupMemberOfGroup(share.ReadOnlyGroup, share.Ou.Domain.ToString(), share.Ou.OrganizationalUnit, request.RequestedForUserAlias);
                        case PermissionType.RW:
                            return AdHelper.IsGroupMemberOfGroup(share.ReadWriteGroup, share.Ou.Domain.ToString(), share.Ou.OrganizationalUnit, request.RequestedForUserAlias);
                        case PermissionType.NC:
                            return AdHelper.IsGroupMemberOfGroup(share.NoChangeGroup, share.Ou.Domain.ToString(), share.Ou.OrganizationalUnit, request.RequestedForUserAlias);
                        case PermissionType.GK:
                            return AdHelper.IsGroupMemberOfGroup(share.OwnerGroup, share.Ou.Domain.ToString(), share.Ou.OrganizationalUnit, request.RequestedForUserAlias);
                        default:
                            return true;
                    }
                }
                else
                {
                    switch (permtype)
                    {
                        case PermissionType.RO:
                            return AdHelper.IsUserMemberOfGroup(share.ReadOnlyGroup, share.Ou.Domain.ToString(), share.Ou.OrganizationalUnit, request.RequestedForUserAlias);
                        case PermissionType.RW:
                            return AdHelper.IsUserMemberOfGroup(share.ReadWriteGroup, share.Ou.Domain.ToString(), share.Ou.OrganizationalUnit, request.RequestedForUserAlias);
                        case PermissionType.NC:
                            return AdHelper.IsUserMemberOfGroup(share.NoChangeGroup, share.Ou.Domain.ToString(), share.Ou.OrganizationalUnit, request.RequestedForUserAlias);
                        case PermissionType.GK:
                            return AdHelper.IsUserMemberOfGroup(share.OwnerGroup, share.Ou.Domain.ToString(), share.Ou.OrganizationalUnit, request.RequestedForUserAlias);
                        default:
                            return true;
                    }
                }
            }
        }

        // GET: /user/SmbRequest/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            int Maxid = db.CifsPermissionRequests.OrderByDescending(u => u.CifsPermissionRequestID).FirstOrDefault().CifsPermissionRequestID;
            if (id > Maxid)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CifsPermissionRequest cifspermissionrequest = db.CifsPermissionRequests.Find(id);
            var cifsshare = db.CifsShares.Find(cifspermissionrequest.CifsShareID);
            if (cifspermissionrequest == null)
            {
                return HttpNotFound();
            }
            ViewBag.CifsShareID = new SelectList(db.CifsShares, "CifsShareID", "Name", cifspermissionrequest.CifsShareID);
            ViewBag.CifsShare = cifsshare as CifsShare;
            ViewBag.RequestType = EnumHelper.GetSelectList<RequestType>(cifspermissionrequest.RequestType.ToString());
            //ViewBag.PermissionType = EnumHelper.GetSelectList<PermissionType>(cifspermissionrequest.PermissionType.ToString());

            List<SelectListItem> permlist = Helpers.EnumHelper.GetSelectList<PermissionType>();
            permlist.Remove(permlist.First(e => (e.Value.Equals("3"))));  

            if (string.IsNullOrEmpty(ViewBag.CifsShare.ReadOnlyGroup))
            {
                permlist.Remove(permlist.First(e => (e.Value.Equals("0"))));
                //permlist.RemoveAt(((int)Enum.Parse(typeof(PermissionType), PermissionType.RO.ToString()))); 
            }

            if (string.IsNullOrEmpty(ViewBag.CifsShare.ReadWriteGroup))
            { 
                permlist.Remove(permlist.First(e => (e.Value.Equals("1"))));
                //permlist.RemoveAt(((int)Enum.Parse(typeof(PermissionType), PermissionType.RW.ToString()))); 
            }

            if (string.IsNullOrEmpty(ViewBag.CifsShare.NoChangeGroup))
            {
                permlist.Remove(permlist.First(e => (e.Value.Equals("2"))));
                //permlist.RemoveAt(((int)Enum.Parse(typeof(PermissionType), PermissionType.NC.ToString())));
            }

            ViewBag.PermissionType = permlist;
            ViewData["PermTypeCount"] = permlist.Count;



            return View(cifspermissionrequest);
        }

        // POST: /user/SmbRequest/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "CifsPermissionRequestID,CifsShareID,PermissionType,RequestType,RequestAdPrincipalType,RequestJustification,RequestedForUserAlias,RequestedForUserName,RequestedByUserAlias,RequestedByUserName,RequestStatus,RequestStatusMsg,RequestOpenedOnDateTime,RequestClosedOnDateTime")] CifsPermissionRequest cifspermissionrequest, int id)
        {
            var request = cifspermissionrequest;
            var cifsshare = db.CifsShares.Find(cifspermissionrequest.CifsShareID);
            if (ModelState.IsValid)
            {
                //CifsShare cifsshare = null;
                //cifsshare = db.CifsShares.Find(id);
                cifspermissionrequest.RequestedByUserAlias = cifspermissionrequest.RequestedByUserAlias;
                cifspermissionrequest.RequestedByUserName = cifspermissionrequest.RequestedByUserName;
                cifspermissionrequest.RequestedForUserAlias = cifspermissionrequest.RequestedForUserAlias;
                cifspermissionrequest.RequestedForUserName = cifspermissionrequest.RequestedForUserName;
                cifspermissionrequest.RequestOpenedOnDateTime = cifspermissionrequest.RequestOpenedOnDateTime;
                bool email = EmailHelper.SendNewRequestNotificationMessage(cifspermissionrequest, cifsshare);
                if (email)
                {
                    // update request with notification time stamp
                    cifspermissionrequest.RequestApprovalNotificationTimeStamp = System.DateTime.Now;
                    db.Entry(cifspermissionrequest).State = EntityState.Modified;
                    var userId = HttpContext.User.Identity.Name.Substring(3);
                    db.SaveChanges(userId);
                }
                return RedirectToAction("Index");
            }
            ViewBag.CifsShareID = new SelectList(db.CifsShares, "CifsShareID", "Name", cifspermissionrequest.CifsShareID);
            //var cifsshare = db.CifsShares.Find(cifspermissionrequest.CifsShareID);
            ViewBag.CifsShare = cifsshare as CifsShare;
            ViewBag.RequestType = EnumHelper.GetSelectList<RequestType>(cifspermissionrequest.RequestType.ToString());
            //ViewBag.PermissionType = EnumHelper.GetSelectList<PermissionType>(cifspermissionrequest.PermissionType.ToString());

            List<SelectListItem> permlist = Helpers.EnumHelper.GetSelectList<PermissionType>();
            permlist.Remove(permlist.First(e => (e.Value.Equals("3"))));

            if (string.IsNullOrEmpty(ViewBag.CifsShare.ReadOnlyGroup))
            {
                permlist.Remove(permlist.First(e => (e.Value.Equals("0"))));
                //permlist.RemoveAt(((int)Enum.Parse(typeof(PermissionType), PermissionType.RO.ToString()))); 
            }

            if (string.IsNullOrEmpty(ViewBag.CifsShare.ReadWriteGroup))
            {
                permlist.Remove(permlist.First(e => (e.Value.Equals("1"))));
                //permlist.RemoveAt(((int)Enum.Parse(typeof(PermissionType), PermissionType.RW.ToString()))); 
            }

            if (string.IsNullOrEmpty(ViewBag.CifsShare.NoChangeGroup))
            {
                permlist.Remove(permlist.First(e => (e.Value.Equals("2"))));
                //permlist.RemoveAt(((int)Enum.Parse(typeof(PermissionType), PermissionType.NC.ToString())));
            }

            ViewBag.PermissionType = permlist;
            ViewData["PermTypeCount"] = permlist.Count;

            return View(cifspermissionrequest);
        }

        public ActionResult Remind(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            int Maxid = db.CifsPermissionRequests.OrderByDescending(u => u.CifsPermissionRequestID).FirstOrDefault().CifsPermissionRequestID;
            if (id > Maxid)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CifsPermissionRequest cifspermissionrequest = db.CifsPermissionRequests.Find(id);
            var cifsshare = db.CifsShares.Find(cifspermissionrequest.CifsShareID);
            //var request = cifspermissionrequest;

            bool email = EmailHelper.SendNewRequestNotificationMessage(cifspermissionrequest, cifsshare);

            ViewBag.Email = email;
            return View();
        }
        // GET: /user/SmbRequest/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CifsPermissionRequest cifspermissionrequest =  db.CifsPermissionRequests.Find(id);
            var cifsshare = db.CifsShares.Find(cifspermissionrequest.CifsShareID);
            ViewBag.CifsShare = cifsshare as CifsShare;
            if (cifspermissionrequest == null)
            {
                return HttpNotFound();
            }
            return View(cifspermissionrequest);
        }

        // POST: /user/SmbRequest/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            AdUser closedBy = AdHelper.GetAdUser(HttpContext.User.Identity.Name.Substring(3));
            CifsPermissionRequest cifspermissionrequest = db.CifsPermissionRequests.Find(id);
            //db.CifsPermissionRequests.Remove(cifspermissionrequest);
            cifspermissionrequest.RequestStatus = RequestStatus.Completed;
            cifspermissionrequest.RequestApprovalStatus = RequestApprovalStatus.Cancelled;
            cifspermissionrequest.ClosedByUserAlias = closedBy.UserAlias;
            cifspermissionrequest.ClosedByUserName = closedBy.UserName;
            cifspermissionrequest.RequestClosedOnDateTime = System.DateTime.Now;
            cifspermissionrequest.RequestStatusMsg = "Request Cancelled by User";
            db.Entry(cifspermissionrequest).State = EntityState.Modified;
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
