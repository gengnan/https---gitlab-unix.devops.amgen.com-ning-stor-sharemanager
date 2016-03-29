using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ShareManagerEdgeMvc.DAL;
using ShareManagerEdgeMvc.Models;


namespace ShareManagerEdgeMvc.Helpers
{
    public class AuthenticationHelper
    {
        private ShareContext db = new ShareContext();

        public bool IsShareAdmin(string alias)
        {
            var admins = db.Administrators.Where(s => s.UserAlias.ToUpper().Equals(alias.ToUpper()) && s.AdminType == AdminType.Share);
            if (admins == null)
            {
                return false;
            }
            else
            {
                if (admins.Count() > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public bool IsSiteAdmin(string alias)
        {
            var admins = db.Administrators.Where(s => s.UserAlias.ToUpper().Equals(alias.ToUpper()) && s.AdminType == AdminType.Site);
            if (admins == null)
            {
                return false;
            }
            else
            {
                if (admins.Count() > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public bool IsShareOwner(string alias, CifsShare share)
        {
            return false;
        }
    }
}