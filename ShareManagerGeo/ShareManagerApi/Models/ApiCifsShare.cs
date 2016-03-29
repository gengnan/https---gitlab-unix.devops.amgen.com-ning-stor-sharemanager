using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
//using ShareManagerEdgeMvc.Helpers;
using Resources;
using System.Web.Mvc;

namespace ShareManagerApi.Models
{
    public enum Status
    {
        InService = 0,
        OutOfService = 1,
        Offline = 2,
        Retired = 3
    }

    // extension class for the Status Enum
    public static class StatusExt
    {
        // Returns the Resouces string for the Enumeraion
        public static string AsDisplayString(this Status status)
        {
            switch (status)
            {
                case Status.InService: return Resources.Resources.InService;
                case Status.Offline: return Resources.Resources.Offline;
                case Status.OutOfService: return Resources.Resources.OutOfService;
                case Status.Retired: return Resources.Resources.Retired;

                default: return Resources.Resources.UnknownItem;
            }
        }
    }

    public enum IsFsrShare
    {
        Yes = 0,
        No = 1
    }

    public static class IsFsrShareExt
    {
        public static string AsDisplayString(this IsFsrShare isfsrshare)
        {
            switch (isfsrshare)
            {
                case IsFsrShare.Yes: return Resources.Resources.Yes;
                case IsFsrShare.No: return Resources.Resources.No;

                default: return Resources.Resources.UnknownItem;
            }
        }
    }

    public class CifsShare : IDescribableEntity
    {
        ///<summary>
        ///ID
        ///</summary>
        [Key]
        public int CifsShareID { get; set; }

        ///<summary>
        ///ID of domain and resolver group for the share
        ///</summary>
        [Required()]
        public int OuID { get; set; }

        ///<summary>
        ///share name
        ///</summary>
        [Display(Name = "ShareName", ResourceType = typeof(Resources.Resources))]
        [Required(ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "ShareNameRequired")]
        [StringLength(75, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "SeventyFiveLength")]
        public string Name { get; set; }

        ///<summary>
        ///CMDB CI; folder CI is the share CI of its parent
        ///</summary>
        [Display(Name = "CmdbCi", ResourceType = typeof(Resources.Resources))]
        [StringLength(75, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "SeventyFiveLength")]
        public string CmdbCi { get; set; }

        ///<summary>
        ///End user access path; support alias
        ///</summary>
        [Display(Name = "UncPath", ResourceType = typeof(Resources.Resources))]
        [Required(ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "ShareUNCPathsRequired")]
        [StringLength(150, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "OneFiftyLength")]
        public string UncPath { get; set; }

        ///<summary>
        ///parent of the share; normally for folders
        ///</summary>
        [Display(Name = "ParentShare", ResourceType = typeof(Resources.Resources))]
        public System.Nullable<int> ParentShareId { get; set; }

        ///<summary>
        ///flag if FSR share; 1 for yes
        ///</summary>
        [Display(Name = "IsFsrShare", ResourceType = typeof(Resources.Resources))]
        public bool IsFsrShare { get; set; }

        ///<summary>
        ///functional area that owns the share
        ///</summary>
        [Display(Name = "ShareOwnerFunctionalArea", ResourceType = typeof(Resources.Resources))]
        [StringLength(25, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "TwentyFiveLength")]
        public string ShareOwnerFunctionalArea { get; set; }

        ///<summary>
        ///cost center that owns the share
        ///</summary>
        [Display(Name = "ShareOwnerCostCenter", ResourceType = typeof(Resources.Resources))]
        [StringLength(25, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "TwentyFiveLength")]
        public string ShareOwnerCostCenter { get; set; }

        ///<summary>
        ///GK group name
        ///</summary>
        [Display(Name = "OwnerGroup", ResourceType = typeof(Resources.Resources))]
        [Required(ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "ShareOwnerGroupRequired")]
        [StringLength(64, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "SixtyFourLength")]
        public string OwnerGroup { get; set; }

        ///<summary>
        ///RO group name
        ///</summary>
        [Display(Name = "ReadOnlyGroup", ResourceType = typeof(Resources.Resources))]
        [StringLength(64, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "SixtyFourLength")]
        public string ReadOnlyGroup { get; set; }

        ///<summary>
        ///RW group name: read+write+modify+delete
        ///</summary>
        [Display(Name = "ReadWriteGroup", ResourceType = typeof(Resources.Resources))]
        [StringLength(64, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "SixtyFourLength")]
        public string ReadWriteGroup { get; set; }

        ///<summary>
        ///No CHange group name: read+write; normally for windows
        ///</summary>
        [Display(Name = "NoChangeGroup", ResourceType = typeof(Resources.Resources))]
        [StringLength(64, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "SixtyFourLength")]
        public string NoChangeGroup { get; set; }

        ///<summary>
        ///share status; share is visible to user when in service
        ///</summary>
        [Required()]
        public Status? Status { get; set; }
        ///<summary>
        ///share entry inserted time
        ///</summary>
        [Display(Name = "CreatedOnDateTime", ResourceType = typeof(Resources.Resources))]
        public DateTime? CreatedOnDateTime { get; set; }
        ///<summary>
        ///sshare entry inserted by
        ///</summary>
        [Display(Name = "CreatedBy", ResourceType = typeof(Resources.Resources))]
        [StringLength(25)]
        public string CreatedBy { get; set; }
        ///<summary>
        ///sshare entry updated time
        ///</summary>
        [Display(Name = "ModifiedOnDateTime", ResourceType = typeof(Resources.Resources))]
        public DateTime? ModifiedOnDateTime { get; set; }
        ///<summary>
        ///sshare entry updated by
        ///</summary>
        [Display(Name = "ModifiedBy", ResourceType = typeof(Resources.Resources))]
        [StringLength(25)]
        public string ModifiedBy { get; set; }


        // Key relationship plumbing
        //public virtual Ou Ou { get; set; }

        //public virtual CifsShare ParentShare { get; set; }

        //public virtual ICollection<CifsPermissionRequest> CifsPermissionRequests { get; set; }



        string IDescribableEntity.Describe()
        {
            return "{ CifsShareID : \"" + CifsShareID +
                "\", OuID : \"" + OuID +
                "\", ShareName : \"" + Name +
                "\", CmdbCi : \"" + CmdbCi +
                "\", UncPath : \"" + UncPath +
                "\", ParentShareId : \"" + ParentShareId +
                "\", IsFsrShare : \"" + IsFsrShare +
                "\", ShareOwnerFunctionalArea : \"" + ShareOwnerFunctionalArea +
                "\", ShareOwnerCostCenter : \"" + ShareOwnerCostCenter +
                "\", OwnerGroup : \"" + OwnerGroup +
                "\", ReadOnlyGroup : \"" + ReadOnlyGroup +
                "\", ReadWriteGroup : \"" + ReadWriteGroup +
                "\", NoChangeGroup : \"" + NoChangeGroup +
                "\", Status : \"" + Status +
                "\", CreatedOnDateTime : \"" + CreatedOnDateTime +
                "\", CreatedBy : \"" + CreatedBy +
                "\", ModifiedOnDateTime : \"" + ModifiedOnDateTime +
                "\", ModifiedBy : \"" + ModifiedBy +
                "\"}";

        }
    }
}