using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Collections;
using OnboardingApi.Models;

namespace OnboardingApi.Helpers
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
        public static AdUser GetAdUser(string userAlias)
        {
            AdUser adUser = new AdUser();
            PrincipalContext ctx = new PrincipalContext(ContextType.Domain);
            try
            {
                // get user principal for user as a Principal object
                var user = UserPrincipal.FindByIdentity(ctx, userAlias) as Principal;
                // get the directory entry objects for the Principal
                var dEntry = user.GetUnderlyingObject() as DirectoryEntry;
                // get the fullname property from the underlying objects
                adUser.UserName = dEntry.Properties["DisplayName"].Value.ToString();
                adUser.UserAlias = user.Name;
                
            }
            catch (Exception)
            {
                return null;
            }

            return adUser;
        }

        public static List<AdUser> GetAdGroupMembers(string groupName)
        {
            var members = new List<AdUser>();
            try 
            { 
                PrincipalContext ctx = new PrincipalContext(ContextType.Domain);
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

                    
                }
                else
                {
                    return null;
                }
            }
            catch(Exception)
            {
                return null;
            }
            return members;
        }

        public static List<AdGroupMember> GetAllAdGroupMembers(string groupName)
        {
            var members = new List<AdGroupMember>();
            try
            {
                PrincipalContext ctx = new PrincipalContext(ContextType.Domain);
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
                        }
                        else // Principal is a Group
                        {
                            //AdGroupMember GroupName = new AdGroupMember();
                            //// Displays Group name as follows : test-sharemgmt_nested (Security Group)
                            //GroupName.UserName = p.Name + " (" + @Resources.Resources.SecurityGroup + ")";
                            //GroupName.UserAlias = null;
                            //GroupName.GroupName = null;
                            //members.Add(GroupName);

                            // get Group and recursively get memebers recursively (it's their responsibility to understand their own nesting)
                            GroupPrincipal nestedGrp = GroupPrincipal.FindByIdentity(ctx, IdentityType.Name, p.Name);
                            foreach (Principal q in nestedGrp.GetMembers(true))
                            {
                                AdGroupMember user = new AdGroupMember();
                                // display name (Kevin Klein), samaccountname == email alias
                                user.UserName = q.DisplayName;
                                user.UserAlias = q.SamAccountName;
                                user.GroupName = p.Name; // p.Name is name of nested Group
                                members.Add(user);

                            }
                        }
                    }
                }
                else
                {
                    return null;
                }
            }
            catch (Exception)
            {
                return null;
            }
            return members;
        }

        public static string GetAdUserOrgUnit(string userAlias)
        {
            PrincipalContext ctx = new PrincipalContext(ContextType.Domain);

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
                return "ERROR   Unable to find AD Entry for " + userAlias;
            }
        }

        public static List<AdGroup> GetGroupsUserIsMemberOf(String userAlias)
        {
            var groups = new List<AdGroup>();
            try
            {
                PrincipalContext ctx = new PrincipalContext(ContextType.Domain);
                var user = UserPrincipal.FindByIdentity(ctx, userAlias) as Principal;
                foreach(var item in user.GetGroups())
                {
                    var adGroup = new AdGroup();
                    adGroup.GroupName = item.SamAccountName.ToString();
                    groups.Add(adGroup);
                }
            }
            catch (Exception)
            {
                return null;
            }

            return groups;
        }

        public static bool IsUserMemberOfGroup(string groupName, string groupDomain, string groupOu, string userAlias)
        {
            
            try
            {
                var userContext = new PrincipalContext(ContextType.Domain);
                var groupContext = new PrincipalContext(ContextType.Domain, groupDomain, groupOu);
                var user = UserPrincipal.FindByIdentity(userContext, IdentityType.SamAccountName, userAlias); // create user Principal Object
                var group = GroupPrincipal.FindByIdentity(groupContext, groupName); // create group
                return user.IsMemberOf(group);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool IsGroupMemberOfGroup(string groupName, string groupDomain, string groupOu, string nestedGroup)
        {

            try
            {
                var userContext = new PrincipalContext(ContextType.Domain);
                var groupContext = new PrincipalContext(ContextType.Domain, groupDomain, groupOu);
                var group = GroupPrincipal.FindByIdentity(groupContext, groupName); // create group
                foreach(Principal p in group.GetMembers(false))
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

        public static AdRequestStatus AddUserToGroup(string userAlias, string groupName, string groupDomain, string groupOu)
        {
            AdRequestStatus msg = new AdRequestStatus();

            // validate user is valid, if not return error message
            if (!AdHelper.IsUserAliasValid(userAlias))
            {
                msg.Successfull = false;
                msg.UserMessage = "ERROR   " + userAlias + " could not be found in Active Directory.";
                msg.ErrorMessage = System.DateTime.Now + " Call to AdHelper.IsUserAliasValid(userAlias) failed.";
                return msg;
            }

            // validate user isn't already a member of the group
            if (AdHelper.IsUserMemberOfGroup(groupName, groupDomain, groupOu, userAlias))
            {
                msg.Successfull = true;
                msg.UserMessage = "INFO   " + userAlias + " is already a member of " + groupName + ". ";
                return msg;
            }

            try
            {
                // create group and user context, then find each by identity and then add the member and save group
                using (var groupContext = new PrincipalContext(ContextType.Domain, groupDomain, groupOu))
                using (var userContext = new PrincipalContext(ContextType.Domain))
                using (var group = GroupPrincipal.FindByIdentity(groupContext, IdentityType.Name, groupName))
                using (var user = UserPrincipal.FindByIdentity(userContext, IdentityType.Name, userAlias))
                {
                    group.Members.Add(user);
                    group.Save(); // if you don't do this the membership add call won't be saved
                }

                 // return success message
                msg.Successfull = true;
                msg.UserMessage = userAlias + " successfully added to " + groupName + ". ";
                msg.ErrorMessage = null;
            }
            catch (Exception ex)
            {
                // there there's an error, return error message
                msg.Successfull = false;
                msg.UserMessage = "Unable to add user to group.";
                msg.ErrorMessage = System.DateTime.Now + ex.ToString();
                
            }
            
            return msg;
            
            
        }

        public static AdRequestStatus AddGroupToGroup(string nestedGroup, string groupName, string groupDomain, string groupOu)
        {
            AdRequestStatus msg = new AdRequestStatus();

            // validate user is valid, if not return error message
            if (!AdHelper.IsGroupNameValid(nestedGroup))
            {
                msg.Successfull = false;
                msg.UserMessage = "ERROR   " + nestedGroup + " could not be found in Active Directory.";
                msg.ErrorMessage = System.DateTime.Now + " Call to AdHelper.IsGroupNameValid(userAlias) failed.";
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
                using (var subGroupContext = new PrincipalContext(ContextType.Domain))
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
            }
            catch (Exception ex)
            {
                // there there's an error, return error message
                msg.Successfull = false;
                msg.UserMessage = "Unable to add user to group.";
                msg.ErrorMessage = System.DateTime.Now + ex.ToString();

            }

            return msg;


        }


        public static AdRequestStatus RemoveGroupFromGroup(string nestedGroup, string groupName, string groupDomain, string groupOu)
        {
            AdRequestStatus msg = new AdRequestStatus();
            nestedGroup = nestedGroup.Trim();
            // validate user is valid, if not return error message
            if (!AdHelper.IsGroupNameValid(nestedGroup))
            {
                msg.Successfull = false;
                msg.UserMessage = "ERROR   " + nestedGroup + " could not be found in Active Directory.";
                msg.ErrorMessage = System.DateTime.Now + " Call to AdHelper.IsGroupNameValid(userAlias) failed.";
                return msg;
            }

            // validate group isn't already a member of the group
            if (!AdHelper.IsGroupMemberOfGroup(groupName, groupDomain, groupOu, nestedGroup))
            {
                msg.Successfull = true;
                msg.UserMessage = "INFO   " + nestedGroup + " is NOT a member of " + groupName + ". ";
                return msg;
            }

            try
            {
                // create group and user context, then find each by identity and then add the member and save group
                using (var groupContext = new PrincipalContext(ContextType.Domain, groupDomain, groupOu))
                using (var subGroupContext = new PrincipalContext(ContextType.Domain))
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
            }
            catch (Exception ex)
            {
                // there there's an error, return error message
                msg.Successfull = false;
                msg.UserMessage = string.Format("Unable to remove {0} from {1}.",nestedGroup, groupName);
                msg.ErrorMessage = System.DateTime.Now + ex.ToString();

            }

            return msg;


        }


        public static AdRequestStatus RemoveUserFromGroup(string userAlias, string groupName, string groupDomain, string groupOu)
        {
            AdRequestStatus msg = new AdRequestStatus();

            // validate user is valid, if not return error message
            if (!AdHelper.IsUserAliasValid(userAlias))
            {
                msg.Successfull = false;
                msg.UserMessage = "ERROR   " + userAlias + " could not be found in Active Directory.";
                msg.ErrorMessage = System.DateTime.Now + " Call to AdHelper.IsUserAliasValid(userAlias) failed.";
                return msg;
            }

            // validate user isn't already a member of the group
            if (!AdHelper.IsUserMemberOfGroup(groupName, groupDomain, groupOu, userAlias))
            {
                msg.Successfull = true;
                msg.UserMessage = "INFO   " + userAlias + " is NOT a member of " + groupName + ". ";
                return msg;
            }

            try
            {
                // create group and user context, then find each by identity and then add the member and save group
                using (var groupContext = new PrincipalContext(ContextType.Domain, groupDomain, groupOu))
                using (var userContext = new PrincipalContext(ContextType.Domain))
                using (var group = GroupPrincipal.FindByIdentity(groupContext, IdentityType.Name, groupName))
                using (var user = UserPrincipal.FindByIdentity(userContext, IdentityType.Name, userAlias))
                {
                    group.Members.Remove(user);
                    group.Save(); // if you don't do this the membership add call won't be saved
                }

                 // return success message
                msg.Successfull = true;
                msg.UserMessage = userAlias + " successfully removed from " + groupName + ". ";
                msg.ErrorMessage = null;
            }
            catch (Exception ex)
            {
                // there there's an error, return error message
                msg.Successfull = false;
                msg.UserMessage = "Unable to remove user from group " + groupName  + ".";
                msg.ErrorMessage = System.DateTime.Now + ex.ToString();
                
            }
            
            return msg;
            
            
        }

        // private method for validating if a user alias is valid
        private static bool IsUserAliasValid(string userAlias)
        {

            // create context
            var userContext = new PrincipalContext(ContextType.Domain);

            try
            {
                //attempt to find user. if no exception then user was found.
                var user = UserPrincipal.FindByIdentity(userContext, IdentityType.SamAccountName, userAlias);
                // user was found, return true
                if (user != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                //user wasn't found return false.
                return false;
            }


        }

        // private method for validating if a user alias is valid
        private static bool IsGroupNameValid(string groupName)
        {

            // create context
            var groupContext = new PrincipalContext(ContextType.Domain);

            try
            {
                //attempt to find user. if no exception then user was found.
                var group = GroupPrincipal.FindByIdentity(groupContext, IdentityType.Name, groupName);
                // user was found, return true
                if (group != null)
                {
                    
                        return true;
                }
                else
                {
                    group = GroupPrincipal.FindByIdentity(groupContext, IdentityType.Name, groupName);
                    if (group != null)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                        
                }
            }
            catch (Exception)
            {
                //user wasn't found return false.
                return false;
            }


        }

    }

   

}