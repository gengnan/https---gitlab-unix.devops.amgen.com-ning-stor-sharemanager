using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using ShareManagerEdgeMvc.Helpers;
using Resources;
using System.Web.Mvc;

namespace ShareManagerEdgeMvc.Models
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
            switch(isfsrshare)
            {
                case IsFsrShare.Yes : return Resources.Resources.Yes;
                case IsFsrShare.No: return Resources.Resources.No;

                default: return Resources.Resources.UnknownItem;
            }
        }
    }

    public class CifsShare : IDescribableEntity
    {
        [Key]
        public int CifsShareID { get; set; }

        [Required()]
        public int OuID { get; set; }

        [Display(Name = "ShareName", ResourceType = typeof(Resources.Resources))]
        [Required(ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "ShareNameRequired")]
        [StringLength(75, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "SeventyFiveLength")]
        public string Name { get; set; }

        [Display(Name = "CmdbCi", ResourceType = typeof(Resources.Resources))]
        [StringLength(75, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "SeventyFiveLength")]
        public string CmdbCi { get; set; }

        [Display(Name = "UncPath", ResourceType = typeof(Resources.Resources))]
        [Required(ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "ShareUNCPathsRequired")]
        [StringLength(150, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "OneFiftyLength")]
        public string UncPath { get; set; }

        [Display(Name = "ParentShare", ResourceType = typeof(Resources.Resources))]
        public System.Nullable<int> ParentShareId { get; set; }

        [Display(Name = "IsFsrShare", ResourceType = typeof(Resources.Resources))]
        public bool IsFsrShare { get; set; }

        [Display(Name = "ShareOwnerFunctionalArea", ResourceType = typeof(Resources.Resources))]
        [StringLength(25, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "TwentyFiveLength")]
        public string ShareOwnerFunctionalArea { get; set; }

        [Display(Name = "ShareOwnerCostCenter", ResourceType = typeof(Resources.Resources))]
                [StringLength(25, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "TwentyFiveLength")]
        public string ShareOwnerCostCenter { get; set; }

        [Display(Name = "OwnerGroup", ResourceType = typeof(Resources.Resources))]
        [Required(ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "ShareOwnerGroupRequired")]
        [StringLength(64, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "SixtyFourLength")]
        public string OwnerGroup { get; set; }

        [Display(Name = "ReadOnlyGroup", ResourceType = typeof(Resources.Resources))]
        [StringLength(64, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "SixtyFourLength")]
        public string ReadOnlyGroup { get; set; }

        [Display(Name = "ReadWriteGroup", ResourceType = typeof(Resources.Resources))]
        [StringLength(64, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "SixtyFourLength")]
        public string ReadWriteGroup { get; set; }

        [Display(Name = "NoChangeGroup", ResourceType = typeof(Resources.Resources))]
        [StringLength(64, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "SixtyFourLength")]
        public string NoChangeGroup { get; set; }

        [Required()]
        public Status? Status { get; set; }

        [Display(Name = "CreatedOnDateTime", ResourceType = typeof(Resources.Resources))]
        public DateTime? CreatedOnDateTime { get; set; }

        [Display(Name = "CreatedBy", ResourceType = typeof(Resources.Resources))]
        [StringLength(25)]
        public string CreatedBy { get; set; }

        [Display(Name = "ModifiedOnDateTime", ResourceType = typeof(Resources.Resources))]
        public DateTime? ModifiedOnDateTime { get; set; }

        [Display(Name = "ModifiedBy", ResourceType = typeof(Resources.Resources))]
        [StringLength(25)]
        public string ModifiedBy { get; set; }


        // Key relationship plumbing
        public virtual Ou Ou { get; set; }

        //public virtual CifsShare ParentShare { get; set; }

        public virtual ICollection<CifsPermissionRequest> CifsPermissionRequests { get; set; }



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