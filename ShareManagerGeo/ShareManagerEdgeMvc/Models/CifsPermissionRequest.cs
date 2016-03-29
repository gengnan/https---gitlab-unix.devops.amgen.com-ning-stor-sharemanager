using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;

namespace ShareManagerEdgeMvc.Models
{
    public enum PermissionType
    {
        RO = 0,
        RW = 1,
        NC = 2,
        GK = 3
    }

    public static class PermissionTypeExt
    {
        public static string AsDisplayString(this PermissionType status)
        {
            switch (status)
            {
                case PermissionType.RO: return Resources.Resources.RO;
                case PermissionType.RW: return Resources.Resources.RW;
                case PermissionType.NC: return Resources.Resources.NC;
                case PermissionType.GK: return Resources.Resources.GK;

                default: return Resources.Resources.UnknownItem;
            }
        }
    }

    public class PermissionTypeModel
    {
        public PermissionTypeModel()
        {
            PermissionTypeList = new List<SelectListItem>();
        }
        [Display(Name = "PermissionType", ResourceType = typeof(Resources.Resources))]
        public int PermissionTypeId { get; set; }
        public IEnumerable<SelectListItem> PermissionTypeList { get; set; }

    }

    public enum RequestType
    {
        Add = 0,
        Remove = 1
    }

    public static class RequestTypeExt
    {
        public static string AsDisplayString(this RequestType type)
        {
            switch (type)
            {
                case RequestType.Add: return Resources.Resources.Add;
                case RequestType.Remove: return Resources.Resources.Remove;
                default: return Resources.Resources.UnknownItem;
            }
        }
    }

    public enum RequestStatus
    {
        Open = 0,
        Completed = 1,
        Failed = 2
    }

    public static class RequestStatusExt
    {
        public static string AsDisplayString(this RequestStatus status)
        {
            switch (status)
            {
                case RequestStatus.Open: return Resources.Resources.Open;
                case RequestStatus.Completed: return Resources.Resources.Completed;
                case RequestStatus.Failed: return Resources.Resources.Failed;
                default: return Resources.Resources.UnknownItem;
            }
        }

    }

    public enum RequestAdPrincipalType
    {
        User = 0,
        SecurityGroup = 1
    }

    public static class RequestAdPrincipalTypeExt
    {
        public static string AsDisplayString(this RequestAdPrincipalType type)
        {
            switch (type)
            {
                case RequestAdPrincipalType.User: return Resources.Resources.User;
                case RequestAdPrincipalType.SecurityGroup: return Resources.Resources.SecurityGroup;
                default: return Resources.Resources.UnknownItem;
            }
        }
    }

    public enum RequestApprovalStatus
    {
        Approved,
        Denied,
        Cancelled
    }

    public static class RequestApprovalStatusExt
    {
        public static string AsDisplayString(this RequestApprovalStatus status)
        {
            switch (status)
            {

                case RequestApprovalStatus.Approved: return Resources.Resources.ApproveRequest;
                case RequestApprovalStatus.Denied: return Resources.Resources.DenyRequest;
                case RequestApprovalStatus.Cancelled: return Resources.Resources.CancelRequest;
                default: return Resources.Resources.UnknownItem;
            }
        }
    }

    public static class RequestApprovalStatusExt2
    {
        public static string AsDisplayString(this RequestApprovalStatus status)
        {
            switch (status)
            {

                case RequestApprovalStatus.Approved: return Resources.Resources.RequestApproved;
                case RequestApprovalStatus.Denied: return Resources.Resources.RequestDenied;
                case RequestApprovalStatus.Cancelled: return Resources.Resources.Cancelled;
                default: return Resources.Resources.UnknownItem;
            }
        }
    }

    public class CifsPermissionRequest : IDescribableEntity
    {
        [Key]
        public int CifsPermissionRequestID { get; set; }

        [Required()]
        public int CifsShareID { get; set; }

        [Display(Name = "PermissionType", ResourceType = typeof(Resources.Resources))]
        public PermissionType? PermissionType { get; set; }

        [Display(Name = "RequestType", ResourceType = typeof(Resources.Resources))]
        public RequestType? RequestType { get; set; }

        [Required()]
        [Display(Name = "RequestJustification", ResourceType = typeof(Resources.Resources))]
        [StringLength(1024, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "TenTwentyFourLength")]
        public string RequestJustification { get; set; }

        [Display(Name= "UserOrGroup", ResourceType = typeof(Resources.Resources))]
        public RequestAdPrincipalType? RequestAdPrincipalType { get; set; }

        [Required()]
        [Display(Name = "RequestedForUserAlias", ResourceType = typeof(Resources.Resources))]
        [StringLength(25, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "TwentyFiveLength")]
        public string RequestedForUserAlias { get; set; }

        [Display(Name = "RequestedForUserName", ResourceType = typeof(Resources.Resources))]
        [StringLength(75, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "SeventyFiveLength")]
        public string RequestedForUserName { get; set; }

        [Display(Name = "RequestedByUserAlias", ResourceType = typeof(Resources.Resources))]
        [StringLength(25, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "TwentyFiveLength")]
        public string RequestedByUserAlias { get; set; }

        [Display(Name = "RequestedByUserName", ResourceType = typeof(Resources.Resources))]
        [StringLength(75, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "SeventyFiveLength")]
        public string RequestedByUserName { get; set; }

        [Display(Name = "RequestStatus", ResourceType = typeof(Resources.Resources))]
        public RequestStatus? RequestStatus { get; set; }

        //[Required()]
        [Display(Name = "RequestStatusMsg", ResourceType = typeof(Resources.Resources))]
        public string RequestStatusMsg { get; set; }

        [Display(Name = "RequestApprovalNotificationTimeStamp", ResourceType = typeof(Resources.Resources))]
        public DateTime? RequestApprovalNotificationTimeStamp { get; set; }

        [Display(Name = "RequestApprovalStatus", ResourceType = typeof(Resources.Resources))]
        public RequestApprovalStatus? RequestApprovalStatus { get; set; }

        [Display(Name = "RequestOpenedOnDateTime", ResourceType = typeof(Resources.Resources))]
        public DateTime? RequestOpenedOnDateTime { get; set; }

        [Display(Name = "RequestClosedOnDateTime", ResourceType = typeof(Resources.Resources))]
        public DateTime? RequestClosedOnDateTime { get; set; }

        [Display(Name = "RequestClosedNotification", ResourceType = typeof(Resources.Resources))]
        public bool? RequestClosedNotification { get; set; }

        [Display(Name = "ClosedBy", ResourceType = typeof(Resources.Resources))]
        [StringLength(25, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "TwentyFiveLength")]
        public string ClosedByUserAlias { get; set; }

        [Display(Name = "ClosedBy", ResourceType = typeof(Resources.Resources))]
        [StringLength(75, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "SeventyFiveLength")]
        public string ClosedByUserName { get; set; }

        [Display(Name = "AdHelperErrorMsg", ResourceType = typeof(Resources.Resources))]
        public string AdHelperErrorMsg { get; set; }

        // Foreign Key Plumbing
        public virtual CifsShare CifsShare { get; set; }

        string IDescribableEntity.Describe()
        {
            return "{ CifsPermissionRequestID : \"" + CifsPermissionRequestID + 
                "\", CifsShareID : \"" + CifsShareID +
                "\", PermissionType : \"" + PermissionType +
                "\", RequestType : \"" + RequestType +
                "\", RequestJustification : \"" + RequestJustification +
                "\", RequestAdPrincipalType : \"" + RequestAdPrincipalType +
                "\", RequestedForUserAlias : \"" + RequestedForUserAlias +
                "\", RequestedForUserName : \"" + RequestedForUserName +
                "\", RequestedByUserAlia : \"" + RequestedByUserAlias +
                "\", RequestedByUserName : \"" + RequestedByUserName +
                "\", RequestStatus : \"" + RequestStatus +
                "\", RequestStatusMsg : \"" + RequestStatusMsg +
                "\", RequestApprovalNotificationTimeStamp : \"" + RequestApprovalNotificationTimeStamp +
                "\", RequestApprovalStatus : \"" + RequestApprovalStatus +
                "\", RequestOpenedOnDateTime : \"" + RequestOpenedOnDateTime +
                "\", RequestClosedOnDateTime : \"" + RequestClosedOnDateTime +
                "\", RequestClosedNotification : \"" + RequestClosedNotification +
                "\", ClosedByUserAlias : \"" + ClosedByUserAlias +
                "\", ClosedByUserName : \"" + ClosedByUserName +
                "\", AdHelperErrorMsg : \"" + AdHelperErrorMsg + 
                "\"}";       
        
        }
    }
}