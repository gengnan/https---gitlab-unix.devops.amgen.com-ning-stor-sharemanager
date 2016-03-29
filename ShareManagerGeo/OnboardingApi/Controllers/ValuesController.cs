using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using OnboardingApi.Helpers;
using OnboardingApi.Models;
using OnboardingApi.DAL;
using System.Web.Configuration;
using System.Data.Entity;


namespace OnboardingApi.Controllers
{
    [Authorize]
    public class ValuesController : ApiController
    {
        // static AppSetting variables from the config file
        private static string groupname =  WebConfigurationManager.AppSettings["GroupName"].ToString();
        private static string groupdomain =  WebConfigurationManager.AppSettings["GroupDomain"].ToString();
        private static string groupou = WebConfigurationManager.AppSettings["GroupOu"].ToString();

        public ShareContext db = new ShareContext();

        // GET api/values
        public HttpResponseMessage Get()
        {
            // get api user identify (windows credential)
            var identity = this.User.Identity;
            // authorize user
            if (AuthorizeUser(identity.Name))
            {
                // get list of Ous where resolver group is NOT IS-STORAGE
                var ous = db.Ous.Where(c => !c.ResolverGroup.ToUpper().Equals("IS-STORAGE"));
                // return response with list of Ou objects in JSON format
                return Request.CreateResponse(HttpStatusCode.OK, ous, GlobalConfiguration.Configuration.Formatters.JsonFormatter);
            }
            else
            {
                // user not authorized
                return Request.CreateResponse(HttpStatusCode.BadRequest, "User Not Authorized");
            }
            
        }

        

        // Create/Add
        // POST api/values
        public HttpResponseMessage Post([FromBody]CifsShare share)
        {
            // get the identiry of the user making the call.
            var identity = this.User.Identity;
            var alias = identity.Name.Substring(3);

            // if user is authorized to acccess this API...
            if (AuthorizeUser(alias))
            {
                // validate input. if any of these are null or empty, fail out with error message.
                if(string.IsNullOrEmpty(share.Name) || string.IsNullOrEmpty(share.UncPath) ||
                        string.IsNullOrEmpty(share.CmdbCi) || string.IsNullOrEmpty(share.OwnerGroup) ||
                        string.IsNullOrEmpty(share.ReadOnlyGroup) || string.IsNullOrEmpty(share.ReadWriteGroup))
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Name, UncPath, CmdbCi, OwnerGroup, ReadOnlyGroup, and ReadWriteGroup must all have a value specified");
                }

                // clean up values incase extra spaces were added by accident.
                share.Name = share.Name.Trim();
                share.UncPath = share.UncPath.Trim();
                share.CmdbCi = share.CmdbCi.Trim();
                share.OwnerGroup = share.OwnerGroup.Trim();
                share.ReadOnlyGroup = share.ReadOnlyGroup.Trim();
                share.ReadWriteGroup = share.ReadWriteGroup.Trim();
                if (!string.IsNullOrEmpty(share.NoChangeGroup))
                {
                    share.NoChangeGroup = share.NoChangeGroup.Trim();
                }

                // check if there exists a share with identical path, and cmdb and ou
                var exists = db.CifsShares.Where(c => c.UncPath.ToUpper().Equals(share.UncPath.ToUpper()) &&
                    c.CmdbCi.ToUpper().Equals(share.CmdbCi.ToUpper()) &&
                    c.OuID == share.OuID);

                // if there isn't an existing share
                if (exists == null || exists.Count() < 1)
                {
                    // add created by and modified by values
                    share.CreatedBy = alias;
                    share.ModifiedBy = alias;
                    var now = System.DateTime.Now;
                    share.ModifiedOnDateTime = now;
                    share.CreatedOnDateTime = now;
                    db.CifsShares.Add(share);
                    db.SaveChanges();

                    // return a response with share data
                    return Request.CreateResponse(HttpStatusCode.Created, share, GlobalConfiguration.Configuration.Formatters.JsonFormatter);
                    
                }
                else
                {
                    // return a response letting the user know that a share already exists with that UncPath, Cmdb Ci, and OU
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "There already exists a share with that UncPath, CMDB CI, for that OU");
                }

                
            }
            else // User is not authorized
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "User Not Authorized");
            }
        }

       

        private bool AuthorizeUser(string alias)
        {

            if (alias.Contains("\\"))
            {
                alias = alias.Substring(3);
            }
                   
            return AdHelper.IsUserMemberOfGroup(groupname, groupdomain, groupou, alias);

        }
    }
}
