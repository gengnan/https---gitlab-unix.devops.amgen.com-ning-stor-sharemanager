using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net.Mail;
using ShareManagerEdgeMvc.Models;
using Resources;
using System.Configuration;

namespace ShareManagerEdgeMvc.Helpers
{

    public static class EmailHelper
    {
        //public static string smtphost = "mailhost-i.amgen.com";
        //public static string from = "do-not-reply@amgen.com";

        //public static string baseUrl = "http://uswa-ssvc-stor01.amgen.com";

        public static bool SendNewRequestNotificationMessage(CifsPermissionRequest request, CifsShare share)
        {

            var groupName = share.OwnerGroup;
            string baseUrl = ConfigurationManager.AppSettings["baseurl"].ToString();

            List<AdUser> users = AdHelper.GetAdGroupMembers(groupName,share.Ou.Domain.ToString()); //hit

            if (users == null) return false;

            List<string> aliases = new List<string>();

            foreach (var alias in users)
            {
                string userAlias = alias.UserAlias + "@amgen.com";
                aliases.Add(userAlias);
            }

            List<string> ccs = new List<string>();
            if (request.RequestAdPrincipalType != RequestAdPrincipalType.SecurityGroup)
            {

                ccs.Add(request.RequestedForUserAlias + "@amgen.com");
            }
           
            ccs.Add(request.RequestedByUserAlias + "@amgen.com");
            string subject = Resources.Resources.ActionRequired + " " + Resources.Resources.RequestPending + " " + share.UncPath + " - " + Resources.Resources.ApplicationName;
            string body = "<html><head><style type=\"text/css\">body{font-family: Arial, Helvetica, sans-serif;   font-size:10pt;   background-color:#ffffff;}" +
            "tr,td{font-family: Arial, Helvetica, sans-serif;" +
               "font-size:10pt;" +
               "background-color:#ffffff;}" +
            "span.sig{font-style:italic;" +
                   "font-weight:bold;" +
                   "color:#811919;}" +
            "</style></head>" +
            "<body><p>" + Resources.Resources.ApplicationName + " - " + Resources.Resources.RequestPending + " " + share.UncPath + "</p>" +
            "<p>" + Resources.Resources.ImmediateActionRequired + "</p>" +
            "<p><b><u>" + Resources.Resources.WhatToKnow + "</u></b></p>" +
            "<p>" + Resources.Resources.RequestMade + "</p>" +
            "<p><TABLE><TR><TD style=\"padding-right:2px;text-align:right;\"><b>" + Resources.Resources.ShareName + "</b>:&nbsp;</TD>" +
            "<td>" + share.Name + "</td>" +
            "<TR><TD style=\"padding-right:2px;text-align:right;\"><b>" + Resources.Resources.UncPath + "</b>:&nbsp;</TD>" +
            "<TD>" + share.UncPath + "</TD></TR>" +
            "<TR><TD style=\"padding-right:2px;text-align:right;\"><b>" + Resources.Resources.RequestType + "</b>:&nbsp;</TD>" +
            "<TD>" + RequestTypeExt.AsDisplayString(request.RequestType.Value) + "</TD></TR>" +
            "<TR><TD style=\"padding-right:2px;text-align:right;\"><b>" + Resources.Resources.PermissionType + "</b>:&nbsp;</TD>" +
            "<TD>" + PermissionTypeExt.AsDisplayString(request.PermissionType.Value) + "</TD></TR>" +
            "<TR><TD style=\"padding-right:2px;text-align:right;\"><b>" + Resources.Resources.RequestedForUserName + "</b>:&nbsp;</TD>" +
            "<TD>" + request.RequestedForUserName + "</TD></TR>" +
            "<TR><TD style=\"padding-right:2px;text-align:right;\"><b>" + Resources.Resources.RequestedByUserName + "</b>:&nbsp;</TD>" +
            "<TD>" + request.RequestedByUserName + "</TD></TR>" +
            "<TR><TD style=\"padding-right:2px;text-align:right;\"><b>" + Resources.Resources.OwnersMembers + "</b>:&nbsp;</TD>" +
            "<TD>"; 
             var groupmembers = AdHelper.GetAdGroupMembers(share.OwnerGroup,share.Ou.Domain.ToString()); //hit
                body += "</BR><UL>";
                if (groupmembers != null)
                {
                    foreach (var user in groupmembers)
                    {
                        body += "<LI>" + user.UserName + "</LI>";
                    }
                }
                body += "</UL></TD></TR>";
            

            body += "<TR><TD style=\"padding-right:2px;text-align:right;\"><b>" + Resources.Resources.RequestJustification + "</b>:&nbsp;</TD>" +
            "<TD>" + request.RequestJustification + "</TD></TR>" +
            "<TR><TD style=\"padding-right:2px;text-align:right;\"><b>" + Resources.Resources.RequestOpenedOnDateTime + "</b>:&nbsp;</TD>" +
            "<TD>" + request.RequestOpenedOnDateTime + "</TD></TR>" +
            "<TR><TD style=\"padding-right:2px;text-align:right;\"><b>" + Resources.Resources.RequestStatus + "</b>:&nbsp;</TD>" +
            "<TD>" + RequestStatusExt.AsDisplayString(request.RequestStatus.Value) + "</TD></TR>" +
            "</TABLE></p><p>" + 
            "<p><b><u>" + Resources.Resources.WhatToDo + "</u></b></p>" + 
            "<p>" + Resources.Resources.PleaseGoTo + " <a href=\"" + baseUrl +"/owner/smbrequest/details/" + request.CifsPermissionRequestID +
            "\">" + baseUrl + "/owner/smbrequest/details/" + request.CifsPermissionRequestID + "</a> " + Resources.Resources.AndApproveOrDenyRequest +
            "<p><b>" +Resources.Resources.MoreInfo + "</b></P>" +
            "<p>"+ Resources.Resources.MoreInfoContent + "</p>" +
            "<p>" + Resources.Resources.ThankYou + "</p></body></html>";

            return SendSmtpMessage(aliases, ccs, subject, body);
        }

        public static bool SendRequestCompletedEmailMessage(CifsPermissionRequest request, CifsShare share)
        {
            string baseUrl = ConfigurationManager.AppSettings["baseurl"].ToString();
            List<string> aliases = null;
            aliases = new List<string>() { request.RequestedByUserAlias + "@amgen.com" };
            List<string> ccs = null;
            if (request.RequestAdPrincipalType != RequestAdPrincipalType.SecurityGroup)
            {
                
                ccs = new List<string>() { request.RequestedForUserAlias + "@amgen.com" };
            }
            //if (request.RequestAdPrincipalType == RequestAdPrincipalType.SecurityGroup)
            //{                
            //    aliases = new List<string>() { request.RequestedByUserAlias + "@amgen.com" };
            //}
            //else
            //{
            //    aliases = new List<string>() { request.RequestedForUserAlias + "@amgen.com", request.RequestedByUserAlias + "@amgen.com" };
            //}
            
            

            string requestapprovalstatus = RequestApprovalStatusExt2.AsDisplayString(request.RequestApprovalStatus.Value);
            string subject = Resources.Resources.ApplicationName + " " + requestapprovalstatus + " " + Resources.Resources.ForLowerCase + " " + share.UncPath;
            
            string body = "<html><head><style type=\"text/css\">body{font-family: Arial, Helvetica, sans-serif;   font-size:10pt;   background-color:#ffffff;}" +
            "tr,td{font-family: Arial, Helvetica, sans-serif;" +
               "font-size:10pt;" +
               "background-color:#ffffff;}" +
            "span.sig{font-style:italic;" +
                   "font-weight:bold;" +
                   "color:#811919;}" +
            "</style></head>" +
            "<body><p><b>" + Resources.Resources.ApplicationName + " " + requestapprovalstatus + "</b></p>" +
            "<p>" + Resources.Resources.RequestCompletedEmail + "</p>" +
            "<p><TABLE><TR><TD style=\"padding-right:2px;text-align:right;\"><b>" + Resources.Resources.ShareName + "</b>:&nbsp;</TD>" +
            "<td>" + share.Name + "</td>" +
            "<TR><TD style=\"padding-right:2px;text-align:right;\"><b>" + Resources.Resources.UncPath + "</b>:&nbsp;</TD>" +
            "<TD>" + share.UncPath + "</TD></TR>" +
            "<TR><TD style=\"padding-right:2px;text-align:right;\"><b>" + Resources.Resources.PermissionType + "</b>:&nbsp;</TD>" +
            "<TD>" + PermissionTypeExt.AsDisplayString(request.PermissionType.Value) + "</TD></TR>" +
            "<TR><TD style=\"padding-right:2px;text-align:right;\"><b>" + Resources.Resources.RequestType + "</b>:&nbsp;</TD>" +
            "<TD>" + RequestTypeExt.AsDisplayString(request.RequestType.Value) + "</TD></TR>" +
            "<TR><TD style=\"padding-right:2px;text-align:right;\"><b>" + Resources.Resources.RequestJustification + "</b>:&nbsp;</TD>" +
            "<TD>" + request.RequestJustification + "</TD></TR>" +
            "<TR><TD style=\"padding-right:2px;text-align:right;\"><b>" + Resources.Resources.RequestedForUserName + "</b>:&nbsp;</TD>" +
            "<TD>" + request.RequestedForUserName;
            //if (request.RequestAdPrincipalType == RequestAdPrincipalType.SecurityGroup)
            //{
            //    var groupmembers = AdHelper.GetAdGroupMembers(request.RequestedForUserAlias);
            //    body += "</BR><UL>";
            //    foreach (var user in groupmembers)
            //    {
            //        body += "<LI>" + user.UserName + "</LI>";
            //    }
            //    body += "</UL></TD></TR>";
            //}
            //else
            //{
                body += "</TD></TR>";
            //}
            body += "<TR><TD style=\"padding-right:2px;text-align:right;\"><b>" + Resources.Resources.RequestedByUserName + "</b>:&nbsp;</TD>" +
            "<TD>" + request.RequestedByUserName + "</TD></TR>" +
            "<TR><TD style=\"padding-right:2px;text-align:right;\"><b>" + Resources.Resources.RequestOpenedOnDateTime + "</b>:&nbsp;</TD>" +
            "<TD>" + request.RequestOpenedOnDateTime + "</TD></TR>" +
            "<TR><TD style=\"padding-right:2px;text-align:right;\"><b>" + Resources.Resources.RequestStatus + "</b>:&nbsp;</TD>" +
            "<TD>" + RequestStatusExt.AsDisplayString(request.RequestStatus.Value) + "</TD></TR>" +
            "<TR><TD style=\"padding-right:2px;text-align:right;\"><b>" + Resources.Resources.RequestApprovalStatus + "</b>:&nbsp;</TD>" +
            "<TD>" + RequestApprovalStatusExt2.AsDisplayString(request.RequestApprovalStatus.Value) + "</TD></TR>";

            if (request.RequestApprovalStatus == RequestApprovalStatus.Denied || request.RequestApprovalStatus == RequestApprovalStatus.Cancelled)
            {
                body = body + "<TR><TD style=\"padding-right:2px;text-align:right;\"><b>" + Resources.Resources.RequestStatusMsg + "</b>:&nbsp;</TD>" +
            "<TD>" + request.RequestStatusMsg + "</TD></TR>";
            }

            body = body + "<TR><TD style=\"padding-right:2px;text-align:right;\"><b>" + Resources.Resources.ClosedBy + "</b>:&nbsp;</TD>" +
            "<TD>" + request.ClosedByUserName + "</TD></TR>" +
            "<TR><TD style=\"padding-right:2px;text-align:right;\"><b>" + Resources.Resources.RequestClosedOnDateTime + "</b>:&nbsp;</TD>" +
            "<TD>" + request.RequestClosedOnDateTime + "</TD></TR>" +
            "</TABLE></p><p>";

            if (!(request.RequestApprovalStatus == RequestApprovalStatus.Denied || request.RequestApprovalStatus == RequestApprovalStatus.Cancelled))
            {
                body = body + "<b>" + Resources.Resources.EmailClosingRestart + "</b> ";
            }
             body = body + Resources.Resources.EmailClosingP1 + " " + Resources.Resources.PleaseGoTo +  
            " <a href=\"" + baseUrl + "/user/smbshares/details/" + request.CifsShareID +
            "\"" + ">" + baseUrl + "/owner/smbshares/details/" + request.CifsShareID + "</a></p>" +
            "<p>" + Resources.Resources.EmailClosingP2 + " " + share.Ou.ResolverGroup + " " + Resources.Resources.EmailClosingP3 + "</p>";


            return SendSmtpMessage(aliases, ccs , subject, body);
        }

        public static bool SendRequestErrorNotifyEmailMessage(CifsPermissionRequest request, CifsShare share, AdRequestStatus msg)
        {
            string baseUrl = ConfigurationManager.AppSettings["baseurl"].ToString();
            List<string> aliases = null;
            aliases = new List<string>() { "dl-IS-Storage-ShareManager@amgen.com" };
            List<string> ccs = null;
            //if (request.RequestAdPrincipalType != RequestAdPrincipalType.SecurityGroup)
            //{

            //    ccs = new List<string>() { request.RequestedForUserAlias + "@amgen.com" };
            //}
            //if (request.RequestAdPrincipalType == RequestAdPrincipalType.SecurityGroup)
            //{                
            //    aliases = new List<string>() { request.RequestedByUserAlias + "@amgen.com" };
            //}
            //else
            //{
            //    aliases = new List<string>() { request.RequestedForUserAlias + "@amgen.com", request.RequestedByUserAlias + "@amgen.com" };
            //}



            string requestapprovalstatus = RequestApprovalStatusExt2.AsDisplayString(request.RequestApprovalStatus.Value);
            string subject = Resources.Resources.ApplicationName + " " + Resources.Resources.ADModifyFailure + " " + Resources.Resources.ForLowerCase + " " + share.UncPath;

            string body = "<html><head><style type=\"text/css\">body{font-family: Arial, Helvetica, sans-serif;   font-size:10pt;   background-color:#ffffff;}" +
            "tr,td{font-family: Arial, Helvetica, sans-serif;" +
               "font-size:10pt;" +
               "background-color:#ffffff;}" +
            "span.sig{font-style:italic;" +
                   "font-weight:bold;" +
                   "color:#811919;}" +
            "</style></head>" +
            "<body><p><b>" + Resources.Resources.ApplicationName + " " + requestapprovalstatus + "</b></p>" +
            "<p>" + Resources.Resources.RequestCompletedEmail + "</p>" +
            "<p><TABLE><TR><TD style=\"padding-right:2px;text-align:right;\"><b>" + Resources.Resources.ShareName + "</b>:&nbsp;</TD>" +
            "<td>" + share.Name + "</td>" +
            "<TR><TD style=\"padding-right:2px;text-align:right;\"><b>" + Resources.Resources.UncPath + "</b>:&nbsp;</TD>" +
            "<TD>" + share.UncPath + "</TD></TR>" +
            "<TR><TD style=\"padding-right:2px;text-align:right;\"><b>" + Resources.Resources.PermissionType + "</b>:&nbsp;</TD>" +
            "<TD>" + PermissionTypeExt.AsDisplayString(request.PermissionType.Value) + "</TD></TR>" +
            "<TR><TD style=\"padding-right:2px;text-align:right;\"><b>" + Resources.Resources.RequestType + "</b>:&nbsp;</TD>" +
            "<TD>" + RequestTypeExt.AsDisplayString(request.RequestType.Value) + "</TD></TR>" +
            "<TR><TD style=\"padding-right:2px;text-align:right;\"><b>" + Resources.Resources.RequestJustification + "</b>:&nbsp;</TD>" +
            "<TD>" + request.RequestJustification + "</TD></TR>" +
            "<TR><TD style=\"padding-right:2px;text-align:right;\"><b>" + Resources.Resources.RequestedForUserName + "</b>:&nbsp;</TD>" +
            "<TD>" + request.RequestedForUserName;
            //if (request.RequestAdPrincipalType == RequestAdPrincipalType.SecurityGroup)
            //{
            //    var groupmembers = AdHelper.GetAdGroupMembers(request.RequestedForUserAlias);
            //    body += "</BR><UL>";
            //    foreach (var user in groupmembers)
            //    {
            //        body += "<LI>" + user.UserName + "</LI>";
            //    }
            //    body += "</UL></TD></TR>";
            //}
            //else
            //{
            body += "</TD></TR>";
            //}
            body += "<TR><TD style=\"padding-right:2px;text-align:right;\"><b>" + Resources.Resources.RequestedByUserName + "</b>:&nbsp;</TD>" +
            "<TD>" + request.RequestedByUserName + "</TD></TR>" +
            "<TR><TD style=\"padding-right:2px;text-align:right;\"><b>" + Resources.Resources.RequestOpenedOnDateTime + "</b>:&nbsp;</TD>" +
            "<TD>" + request.RequestOpenedOnDateTime + "</TD></TR>" +
            "<TR><TD style=\"padding-right:2px;text-align:right;\"><b>" + Resources.Resources.RequestStatus + "</b>:&nbsp;</TD>" +
            "<TD>" + RequestStatusExt.AsDisplayString(request.RequestStatus.Value) + "</TD></TR>" +
            "<TR><TD style=\"padding-right:2px;text-align:right;\"><b>" + Resources.Resources.RequestApprovalStatus + "</b>:&nbsp;</TD>" +
            "<TD>" + RequestApprovalStatusExt2.AsDisplayString(request.RequestApprovalStatus.Value) + "</TD></TR>";

            if (request.RequestApprovalStatus == RequestApprovalStatus.Denied || request.RequestApprovalStatus == RequestApprovalStatus.Cancelled)
            {
                body = body + "<TR><TD style=\"padding-right:2px;text-align:right;\"><b>" + Resources.Resources.RequestStatusMsg + "</b>:&nbsp;</TD>" +
            "<TD>" + request.RequestStatusMsg + "</TD></TR>";
            }

            body = body + 
            "<TR><TD style=\"padding-right:2px;text-align:right;\"><b>" + Resources.Resources.ClosedBy + "</b>:&nbsp;</TD>" +
            "<TD>" + request.ClosedByUserName + "</TD></TR>" +
            "<TR><TD style=\"padding-right:2px;text-align:right;\"><b>" + Resources.Resources.RequestClosedOnDateTime + "</b>:&nbsp;</TD>" +
            "<TD>" + request.RequestClosedOnDateTime + "</TD></TR>" +
             "<TR><TD style=\"padding-right:2px;text-align:right;\"><b>" + Resources.Resources.AdHelperErrorMsg + "</b>:&nbsp;</TD>" +
            "<TD>" + msg.ErrorMessage + "</TD></TR>" +
            "</TABLE></p><p><b>" + Resources.Resources.EmailClosingRestart + "</b> " + Resources.Resources.EmailClosingP1 + " " + Resources.Resources.PleaseGoTo +
            " <a href=\"" + baseUrl + "/user/smbshares/details/" + request.CifsShareID +
            "\"" + ">" + baseUrl + "/owner/smbshares/details/" + request.CifsShareID + "</a></p>" +
            "<p>" + Resources.Resources.EmailClosingP2 + " " + share.Ou.ResolverGroup + " " + Resources.Resources.EmailClosingP3 + "</p>";


            return SendSmtpMessage(aliases, ccs, subject, body);
        }


        private static bool SendSmtpMessage(List<string> to, List<string> cc,string subject, string body)
        {
            string smtphost = ConfigurationManager.AppSettings["mailhost"].ToString();
            string from = ConfigurationManager.AppSettings["emailfrom"].ToString();

            var message = new MailMessage();
            if (to != null)
            {
                foreach (var emailalias in to)
                {
                    message.To.Add(emailalias);
                }
            }
            else
            { message.To.Add(""); }
            if (cc != null)
            {
                foreach (var emailalias in cc)
                {
                    message.CC.Add(emailalias);
                    //message.To.Add(emailalias);
                }
            }
            //message.To.Add(to);
            message.Subject = subject;
            message.From = new MailAddress(from);
            message.Body = body;
            message.IsBodyHtml = true;

            SmtpClient client = new SmtpClient(smtphost);

            try
            {
                client.Send(message);
                return true;
            }
            catch(Exception)
            {
                return false;
            }
        }
    }
}