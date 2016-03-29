using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Collections;
using ShareManagerApi.Models;



namespace ShareManagerApi.Helper
{
    public class AdUser
    {
        public string UserName { get; set; }
        public string UserAlias { get; set; }
    }

    public class AdGroupMember
    {
        public string UserName { get; set; }
        public string UserAlias { get; set; }
        public string GroupName { get; set; }
    }

    public class AdGroup
    {
        public string GroupName { get; set; }
    }

    public class AdRequestStatus
    {
        public bool Successfull { get; set; }
        public string UserMessage { get; set; }
        public string ErrorMessage { get; set; }
    }

    public static class AdHelper
    {

        public static List<String> getdomains()
        {
            var db = new ShareContext();
            var ous = db.Ous.ToList();

            var domains = new List<String>();
            foreach (Ou ou in ous)
            {
                string domainName = ou.Domain.ToString() + ".corp.amgen.com";
                domains.Add(domainName);
            }

            return domains;
        }

        public static AdUser GetAdUser(string userAlias)
        {

            AdUser adUser = new AdUser();
            var domains = getdomains();
            foreach (string domain in domains)
            {
                PrincipalContext ctx = new PrincipalContext(ContextType.Domain, domain);
                try
                {
                    // get user principal for user as a Principal object
                    var user = UserPrincipal.FindByIdentity(ctx, userAlias) as Principal;
                    // get the directory entry objects for the Principal
                    var dEntry = user.GetUnderlyingObject() as DirectoryEntry;
                    // get the fullname property from the underlying objects
                    adUser.UserName = dEntry.Properties["DisplayName"].Value.ToString();
                    adUser.UserAlias = user.Name;
                    return adUser;

                }
                catch (Exception)
                {

                    //continue to next domain
                }

            }

            return null;


        }

        public static List<AdUser> GetAdGroupMembers(string groupName, string domain)
        {
            var members = new List<AdUser>();
            //var domains = getdomains();
            try
            {

                //foreach (string domain in domains)
                //{
                    PrincipalContext ctx = new PrincipalContext(ContextType.Domain, domain);
                    GroupPrincipal group = GroupPrincipal.FindByIdentity(ctx, groupName); // doesn't throw exception if not found
                    if (group != null)
                    {

                        // iterate over members (users are Principal class)
                        foreach (Principal p in group.GetMembers())
                        {
                            AdUser user = new AdUser();
                            // display name (Kevin Klein), samaccountname == email alias
                            user.UserName = p.DisplayName;
                            user.UserAlias = p.SamAccountName;
                            members.Add(user);
                        }
                        return members.OrderBy(o => o.UserName).ToList();

                    }
                //}

            }
            catch (Exception)
            {
                return null;
            }

            return null;

        }

        public static List<AdGroupMember> GetAllAdGroupMembers(string groupName, string domain)
        {
            var members = new List<AdGroupMember>();
            //var domains = getdomains();
            try
            {

                //foreach (string domain in domains)
                //{
                    PrincipalContext ctx = new PrincipalContext(ContextType.Domain, domain);
                    GroupPrincipal group = GroupPrincipal.FindByIdentity(ctx, groupName); // doesn't throw exception if not found
                    if (group != null)
                    {

                        // iterate over members (users are Principal class)
                        foreach (Principal p in group.GetMembers(false).Reverse()) // reverse so groups are last in list
                        {
                            // if it's a User, then it's our Group so leave GroupName null
                            if (p.GetType() == typeof(UserPrincipal))
                            {
                                AdGroupMember user = new AdGroupMember();
                                // display name (Kevin Klein), samaccountname == email alias
                                user.UserName = p.DisplayName;
                                user.UserAlias = p.SamAccountName;
                                user.GroupName = null;
                                members.Add(user);

                                // Klein, Kevin (keklein)
                            }
                            else // Principal is a Group
                            {
                                AdGroupMember GroupName = new AdGroupMember();
                                // Displays Group name as follows : test-sharemgmt_nested (Security Group)
                                GroupName.UserName = p.Name + " (" + @Resources.Resources.SecurityGroup + ")";
                                GroupName.UserAlias = null;
                                GroupName.GroupName = null;
                                members.Add(GroupName);

                                // get Group and recursively get memebers recursively (it's their responsibility to understand their own nesting)
                                //GroupPrincipal nestedGrp = GroupPrincipal.FindByIdentity(ctx, IdentityType.Name, p.Name);
                                //foreach (Principal q in nestedGrp.GetMembers(true))
                                //{
                                //    AdGroupMember user = new AdGroupMember();
                                //    // display name (Kevin Klein), samaccountname == email alias
                                //    user.UserName = q.DisplayName;
                                //    user.UserAlias = q.SamAccountName;
                                //    user.GroupName = p.Name; // p.Name is name of nested Group
                                //    members.Add(user);

                                //}
                            }
                        }

                        return members.OrderBy(o => o.UserName).ToList();
                    }

                //}
            }
            catch (Exception)
            {
                return null;
            }
            return null;


        }

        public static string GetAdUserOrgUnit(string userAlias)
        {
            var domains = getdomains();
            foreach (string domain in domains)
            {
                PrincipalContext ctx = new PrincipalContext(ContextType.Domain, domain);
                try
                {
                    // get user principal for user as a Principal object
                    var user = UserPrincipal.FindByIdentity(ctx, userAlias) as Principal;
                    // get the directory entry objects for the Principal
                    var dEntry = user.GetUnderlyingObject() as DirectoryEntry;
                    // return the amgen-comOrgUnitName property from the underlying objects
                    return dEntry.Properties["amgen-comOrgUnitName"].Value.ToString();

                }
                catch (Exception)
                {
                    //continue;
                    //continue to next domain
                }

            }
            return "ERROR   Unable to find AD Entry for " + userAlias;


        }

        public static List<AdGroup> GetGroupsUserIsMemberOf(String userAlias) //testing needed
        {
            var groups = new List<AdGroup>();
            var domains = getdomains();
            foreach (string domain in domains)
            {
                PrincipalContext ctx = new PrincipalContext(ContextType.Domain, domain);
                try
                {

                    var user = UserPrincipal.FindByIdentity(ctx, userAlias) as Principal;
                    if (user == null)
                        continue;
                    foreach (var item in user.GetGroups())
                    {
                        var adGroup = new AdGroup();
                        adGroup.GroupName = item.SamAccountName.ToString();
                        groups.Add(adGroup);
                    }
                }
                catch (Exception)
                {
                    //return null;
                    //continue to next domain
                }
            }
            return groups;
        }

        public static bool IsUserMemberOfGroup(string groupName, string groupDomain, string groupOu, string userAlias) //is group domain decided by selection?//testing  needed
        {

            var domains = getdomains();
            foreach (string domain in domains)
            {
                var userContext = new PrincipalContext(ContextType.Domain, domain);
                try
                {
                    var user = UserPrincipal.FindByIdentity(userContext, IdentityType.SamAccountName, userAlias); // create user Principal Object
                    if (user == null)
                        continue;
                    var groupContext = new PrincipalContext(ContextType.Domain, groupDomain); //   , groupOu);
                    var group = GroupPrincipal.FindByIdentity(groupContext, groupName); // create group
                    return user.IsMemberOf(group);// user context covers all domain?
                }
                catch (Exception)
                {
                    //continue to next domain
                }
            }
            return false;

        }

        public static bool IsGroupMemberOfGroup(string groupName, string groupDomain, string groupOu, string nestedGroup) //is group domain decided by selection?
        {

            try
            {
                //var userContext = new PrincipalContext(ContextType.Domain);
                var groupContext = new PrincipalContext(ContextType.Domain, groupDomain, groupOu);
                var group = GroupPrincipal.FindByIdentity(groupContext, groupName); // create group
                foreach (Principal p in group.GetMembers(false))
                {
                    if (p.GetType() == typeof(GroupPrincipal))
                    {
                        if (p.Name.ToLower() == nestedGroup.ToLower())
                            return true;
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }
            return false;
        }

        public static AdRequestStatus AddUserToGroup(string userAlias, string groupName, string groupDomain, string groupOu, CifsShare CifsShare, CifsPermissionRequest cifspermissionrequest)
        {
            AdRequestStatus msg = new AdRequestStatus();
            var domains = getdomains();

            // validate user is valid, if not return error message
            if (!AdHelper.IsUserAliasValid(userAlias))
            {
                msg.Successfull = false;
                msg.UserMessage = "ERROR   " + userAlias + " could not be found in Active Directory.";
                msg.ErrorMessage = System.DateTime.Now + " Call to AdHelper.IsUserAliasValid(userAlias) failed.";
                bool email = false;
                email = EmailHelper.SendRequestErrorNotifyEmailMessage(cifspermissionrequest, CifsShare, msg);
                if (!email)
                {
                    email = EmailHelper.SendRequestErrorNotifyEmailMessage(cifspermissionrequest, CifsShare, msg);
                }
                return msg;
            }

            // validate user isn't already a member of the group
            if (AdHelper.IsUserMemberOfGroup(groupName, groupDomain, groupOu, userAlias))
            {
                msg.Successfull = true;
                msg.UserMessage = "INFO   " + userAlias + " is already a member of " + groupName + ". ";
                return msg;
            }

            foreach (string domain in domains)
            {
                try
                {
                    // create group and user context, then find each by identity and then add the member and save group
                    using (var groupContext = new PrincipalContext(ContextType.Domain, groupDomain, groupOu))
                    using (var userContext = new PrincipalContext(ContextType.Domain, domain))
                    using (var group = GroupPrincipal.FindByIdentity(groupContext, IdentityType.Name, groupName))
                    using (var user = UserPrincipal.FindByIdentity(userContext, IdentityType.Name, userAlias))
                    {
                        if (user != null)
                        {
                            group.Members.Add(user);
                            group.Save(); // if you don't do this the membership add call won't be saved
                        }
                        else
                        {
                            continue;
                        }
                    }

                    // return success message
                    msg.Successfull = true;
                    msg.UserMessage = userAlias + " successfully added to " + groupName + ". ";
                    msg.ErrorMessage = null;
                    return msg;
                }
                catch (Exception ex)
                {
                    // there there's an error, return error message
                    msg.Successfull = false;
                    msg.UserMessage = "Unable to add user to group.";
                    msg.ErrorMessage = System.DateTime.Now + ex.ToString();
                    bool email = false;
                    email = EmailHelper.SendRequestErrorNotifyEmailMessage(cifspermissionrequest, CifsShare, msg);
                    if (!email)
                    {
                        email = EmailHelper.SendRequestErrorNotifyEmailMessage(cifspermissionrequest, CifsShare, msg);
                    }
                    return msg;

                }
            }
            return msg;


        }

        public static AdRequestStatus AddGroupToGroup(string nestedGroup, string groupName, string groupDomain, string groupOu, CifsShare CifsShare, CifsPermissionRequest cifspermissionrequest)
        {
            AdRequestStatus msg = new AdRequestStatus();

            // validate user is valid, if not return error message
            if (!AdHelper.IsGroupNameValid(nestedGroup))
            {
                msg.Successfull = false;
                msg.UserMessage = "ERROR   " + nestedGroup + " could not be found in Active Directory.";
                msg.ErrorMessage = System.DateTime.Now + " Call to AdHelper.IsGroupNameValid(userAlias) failed.";
                bool email = false;
                email = EmailHelper.SendRequestErrorNotifyEmailMessage(cifspermissionrequest, CifsShare, msg);
                if (!email)
                {
                    email = EmailHelper.SendRequestErrorNotifyEmailMessage(cifspermissionrequest, CifsShare, msg);
                }
                return msg;
            }

            // validate user isn't already a member of the group
            if (AdHelper.IsGroupMemberOfGroup(groupName, groupDomain, groupOu, nestedGroup))
            {
                msg.Successfull = true;
                msg.UserMessage = "INFO   " + nestedGroup + " is already a member of " + groupName + ". ";
                return msg;
            }
            try
            {
                // create group and user context, then find each by identity and then add the member and save group
                using (var groupContext = new PrincipalContext(ContextType.Domain, groupDomain, groupOu))
                using (var subGroupContext = new PrincipalContext(ContextType.Domain, groupDomain))
                using (var group = GroupPrincipal.FindByIdentity(groupContext, IdentityType.Name, groupName))
                using (var subGroup = GroupPrincipal.FindByIdentity(subGroupContext, IdentityType.Name, nestedGroup))
                {
                    group.Members.Add(subGroup);
                    group.Save(); // if you don't do this the membership add call won't be saved
                }

                // return success message
                msg.Successfull = true;
                msg.UserMessage = nestedGroup + " successfully added to " + groupName + ". ";
                msg.ErrorMessage = null;
                return msg;
            }
            catch (Exception ex)
            {
                // there there's an error, return error message
                msg.Successfull = false;
                msg.UserMessage = "Unable to add user to group.";
                msg.ErrorMessage = System.DateTime.Now + ex.ToString();
                bool email = false;
                email = EmailHelper.SendRequestErrorNotifyEmailMessage(cifspermissionrequest, CifsShare, msg);
                if (!email)
                {
                    email = EmailHelper.SendRequestErrorNotifyEmailMessage(cifspermissionrequest, CifsShare, msg);
                }
                return msg;

            }

            return msg;


        }


        public static AdRequestStatus RemoveGroupFromGroup(string nestedGroup, string groupName, string groupDomain, string groupOu, CifsShare CifsShare, CifsPermissionRequest cifspermissionrequest)
        {
            AdRequestStatus msg = new AdRequestStatus();
            var domains = getdomains();
            nestedGroup = nestedGroup.Trim();
            // validate user is valid, if not return error message
            if (!AdHelper.IsGroupNameValid(nestedGroup))
            {
                msg.Successfull = false;
                msg.UserMessage = "ERROR   " + nestedGroup + " could not be found in Active Directory.";
                msg.ErrorMessage = System.DateTime.Now + " Call to AdHelper.IsGroupNameValid(userAlias) failed.";
                bool email = false;
                email = EmailHelper.SendRequestErrorNotifyEmailMessage(cifspermissionrequest, CifsShare, msg);
                if (!email)
                {
                    email = EmailHelper.SendRequestErrorNotifyEmailMessage(cifspermissionrequest, CifsShare, msg);
                }
                return msg;
            }

            // validate group isn't already a member of the group
            if (!AdHelper.IsGroupMemberOfGroup(groupName, groupDomain, groupOu, nestedGroup))
            {
                msg.Successfull = true;
                msg.UserMessage = "INFO   " + nestedGroup + " is NOT a member of " + groupName + ". ";
                return msg;
            }
            foreach (string domain in domains)
            {
                try
                {
                    // create group and user context, then find each by identity and then add the member and save group
                    using (var groupContext = new PrincipalContext(ContextType.Domain, groupDomain, groupOu))
                    using (var subGroupContext = new PrincipalContext(ContextType.Domain, domain))
                    using (var group = GroupPrincipal.FindByIdentity(groupContext, IdentityType.Name, groupName))
                    using (var subGroup = GroupPrincipal.FindByIdentity(subGroupContext, IdentityType.Name, nestedGroup))
                    {
                        group.Members.Remove(subGroup);
                        group.Save(); // if you don't do this the membership add call won't be saved
                    }

                    // return success message
                    msg.Successfull = true;
                    msg.UserMessage = nestedGroup + " successfully removed from " + groupName + ". ";
                    msg.ErrorMessage = null;
                    return msg;
                }
                catch (Exception ex)
                {
                    // there there's an error, return error message
                    msg.Successfull = false;
                    msg.UserMessage = string.Format("Unable to remove {0} from {1}.", nestedGroup, groupName);
                    msg.ErrorMessage = System.DateTime.Now + ex.ToString();
                    bool email = false;
                    email = EmailHelper.SendRequestErrorNotifyEmailMessage(cifspermissionrequest, CifsShare, msg);
                    if (!email)
                    {
                        email = EmailHelper.SendRequestErrorNotifyEmailMessage(cifspermissionrequest, CifsShare, msg);
                    }
                    return msg;
                }
            }
            return msg;


        }


        public static AdRequestStatus RemoveUserFromGroup(string userAlias, string groupName, string groupDomain, string groupOu, CifsShare CifsShare, CifsPermissionRequest cifspermissionrequest)
        {
            AdRequestStatus msg = new AdRequestStatus();
            var domains = getdomains();
            // validate user is valid, if not return error message
            if (!AdHelper.IsUserAliasValid(userAlias))
            {
                msg.Successfull = false;
                msg.UserMessage = "ERROR   " + userAlias + " could not be found in Active Directory.";
                msg.ErrorMessage = System.DateTime.Now + " Call to AdHelper.IsUserAliasValid(userAlias) failed.";
                bool email = false;
                email = EmailHelper.SendRequestErrorNotifyEmailMessage(cifspermissionrequest, CifsShare, msg);
                if (!email)
                {
                    email = EmailHelper.SendRequestErrorNotifyEmailMessage(cifspermissionrequest, CifsShare, msg);
                }
                return msg;
            }

            // validate user isn't already a member of the group
            if (!AdHelper.IsUserMemberOfGroup(groupName, groupDomain, groupOu, userAlias))
            {
                msg.Successfull = true;
                msg.UserMessage = "INFO   " + userAlias + " is NOT a member of " + groupName + ". ";
                return msg;
            }
            foreach (string domain in domains)
            {
                try
                {
                    // create group and user context, then find each by identity and then add the member and save group
                    using (var groupContext = new PrincipalContext(ContextType.Domain, groupDomain, groupOu))
                    using (var userContext = new PrincipalContext(ContextType.Domain, domain))
                    using (var group = GroupPrincipal.FindByIdentity(groupContext, IdentityType.Name, groupName))
                    using (var user = UserPrincipal.FindByIdentity(userContext, IdentityType.Name, userAlias))
                    {
                        if (user != null)
                        {
                            group.Members.Remove(user);
                            group.Save(); // if you don't do this the membership add call won't be saved
                        }
                        else
                        { continue; }
                    }

                    // return success message
                    msg.Successfull = true;
                    msg.UserMessage = userAlias + " successfully removed from " + groupName + ". ";
                    msg.ErrorMessage = null;
                    return msg;
                }
                catch (Exception ex)
                {
                    // there there's an error, return error message
                    msg.Successfull = false;
                    msg.UserMessage = "Unable to remove user from group " + groupName + ".";
                    msg.ErrorMessage = System.DateTime.Now + ex.ToString();
                    bool email = false;
                    email = EmailHelper.SendRequestErrorNotifyEmailMessage(cifspermissionrequest, CifsShare, msg);
                    if (!email)
                    {
                        email = EmailHelper.SendRequestErrorNotifyEmailMessage(cifspermissionrequest, CifsShare, msg);
                    }
                    return msg;
                }
            }
            return msg;


        }

        // private method for validating if a user alias is valid
        public static bool IsUserAliasValid(string userAlias)
        {

            var domains = getdomains();
            foreach (string domain in domains)
            {
                // create context
                var userContext = new PrincipalContext(ContextType.Domain, domain);

                try
                {
                    //attempt to find user. if no exception then user was found.
                    var user = UserPrincipal.FindByIdentity(userContext, IdentityType.SamAccountName, userAlias);
                    // user was found, return true
                    if (user != null)
                    {
                        return true;
                    }

                }
                catch (Exception)
                {
                    //user wasn't found return false.
                    //continue to next domain
                }
            }
            return false;

        }

        // private method for validating if a user alias is valid
        public static bool IsGroupNameValid(string groupName)
        {
            var domains = getdomains();
            foreach (string domain in domains)
            {
                // create context

                var groupContext = new PrincipalContext(ContextType.Domain, domain);

                try
                {
                    //attempt to find user. if no exception then user was found.
                    var group = GroupPrincipal.FindByIdentity(groupContext, IdentityType.Name, groupName);

                    // user was found, return true
                    if (group != null)
                    {

                        return true;
                    }

                }
                catch (Exception)
                {
                    //user wasn't found return false.
                    //continue to next domain
                }
            }
            return false;

        }

        public static bool IsGroupInDomain(string groupName, string domain)
        {
            // create context

            var groupContext = new PrincipalContext(ContextType.Domain, domain);

            try
            {
                //attempt to find user. if no exception then user was found.
                var group = GroupPrincipal.FindByIdentity(groupContext, IdentityType.Name, groupName);
                // user was found, return true
                if (group != null)
                {

                    return true;
                }

            }
            catch (Exception)
            {
                return false;
            }

            return false;

        }

        // private method for validating if a user alias is valid
        public static bool IsGroupInOu(string groupName, string groupDomain, string groupOu)
        {

            // create context
            var groupContext = new PrincipalContext(ContextType.Domain, groupDomain, groupOu);

            try
            {
                //attempt to find group but only in OU. if no exception then user was found.
                var group = GroupPrincipal.FindByIdentity(groupContext, IdentityType.Name, groupName);
                // group was not found, return false
                if (group == null)
                {

                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception)
            {
                //group wasn't found return false.
                return false;
            }


        }

        public static bool IsGroupInAnyOu(string groupName, List<Ou> ous)
        {


            //var ous = db.Ou;
            //var ous = new List<Ou>(db.Ous);
            //ous = db.Ous.Find();
            //var ous = db.Ous.Find(1);
            bool findGroup = false;
            foreach (Ou ou in ous)
            {
                // create context
                var groupContext = new PrincipalContext(ContextType.Domain, ou.Domain.ToString(), ou.OrganizationalUnit);
                try
                {
                    //attempt to find group but only in OU. if no exception then user was found.
                    var group = GroupPrincipal.FindByIdentity(groupContext, IdentityType.Name, groupName);
                    // group was not found, return false
                    if (group == null)
                    {

                        findGroup = false;
                    }
                    else
                    {
                        findGroup = true;
                        break;
                    }
                }
                catch (Exception)
                {
                    //group wasn't found return false.
                    findGroup = false;
                }

            }

            return findGroup;

        }

    }

}