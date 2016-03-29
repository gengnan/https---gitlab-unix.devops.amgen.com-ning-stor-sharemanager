using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;


namespace ShareManagerEdgeMvc.Models
{
    public enum FunctionalArea
    {
        RDI, GCOIS, OIS, EISTS, LAW, HR, IMA, ENTSYS, CFBS, ISC
    }

    public class CifsProvisioningRequest : IDescribableEntity
    {
        [Key]
        public int ID { get; set; }

        [Display(Name = "ShareName", ResourceType = typeof(Resources.Resources))]
        [Required(ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "ShareNameRequired")]
        [StringLength(75, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "SeventyFiveLength")]
        public string ShareName { get; set; }

        [Display(Name = "ShareSizeGb", ResourceType = typeof(Resources.Resources))]
        public int ShareSizeGb { get; set; }

        [Display(Name = "UncPath", ResourceType = typeof(Resources.Resources))]
        [Required(ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "UncPathRequired")]
        [StringLength(150, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "OneFiftyLength")]
        public string ShareUncPath { get; set; }

        [Display(Name = "ShareOwnerFunctionalArea", ResourceType = typeof(Resources.Resources))]
        [Required(ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "ShareOwnerFunctionalAreaRequired")]
        [StringLength(25, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "TwentyFiveLength")]
        public string ShareOwnerFunctionalArea { get; set; }

        [Display(Name = "ShareOwnerCostCenter", ResourceType = typeof(Resources.Resources))]
        [Required(ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "ShareOwnerCostCenterRequired")]
        [StringLength(25, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "TwentyFiveLength")]
        public string ShareOwnerCostCenter { get; set; }

        [Display(Name = "RequestedByUserAlias", ResourceType = typeof(Resources.Resources))]
        [Required(ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "RequestedByUserAliasRequired")]
        [StringLength(25, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "TwentyFiveLength")]
        public string RequestedByUserAlias { get; set; }

        [Display(Name = "RequestedByUserName", ResourceType = typeof(Resources.Resources))]
        [Required(ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "RequestedByUserNameRequired")]
        [StringLength(25, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "TwentyFiveLength")]
        public string RequestedByUserName { get; set; }

        [Display(Name = "RequestOpenedOnDateTime", ResourceType = typeof(Resources.Resources))]
        public DateTime RequestOpenedOnDateTime { get; set; }

        [Display(Name = "RequestClosedOnDateTime", ResourceType = typeof(Resources.Resources))]
        public DateTime RequestClosedOnDateTime { get; set; }

        [Display(Name = "RequestStatus", ResourceType = typeof(Resources.Resources))]
        public RequestStatus? RequestStatus { get; set; }

        [Display(Name = "RequestJustification", ResourceType = typeof(Resources.Resources))]
        [Required(ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "RequestJustificationRequired")]
        [StringLength(1024, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "TenTwentyFourLength")]
        public string RequestJustification { get; set; }

        [Display(Name = "OwnerMemberAliases", ResourceType = typeof(Resources.Resources))]
        [Required(ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "OwnerMemberAliasesRequired")]
        [StringLength(150, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "OneFiftyLength")]
        public string OwnerMemberAliases { get; set; }

        [Display(Name = "ReadOnlyMemberAliases", ResourceType = typeof(Resources.Resources))]
        [StringLength(150, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "OneFiftyLength")]
        public string ReadOnlyMemberAliases { get; set; }

        [Display(Name = "ReadWriteMemberAliases", ResourceType = typeof(Resources.Resources))]
        [StringLength(150, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "OneFiftyLength")]
        public string ReadWriteMemberAliases { get; set; }

        [Display(Name = "NoChangeMemberAliases", ResourceType = typeof(Resources.Resources))]
        [StringLength(150, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "OneFiftyLength")]
        public string NoChangeMemberAliases { get; set; }

        public int WfaJobID { get; set; }

        [Display(Name = "AggregateName", ResourceType = typeof(Resources.Resources))]
        [StringLength(100, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "OneHundredLength")]
        public string AggregateName { get; set; }

        [Display(Name = "Comments", ResourceType = typeof(Resources.Resources))]
        public string Comments { get; set; }

        string IDescribableEntity.Describe()
        {
            throw new NotImplementedException();
        }
    }
}