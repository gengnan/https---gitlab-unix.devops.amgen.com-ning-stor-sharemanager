using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace ShareManagerEdgeMvc.Models
{
    public enum Domain
    {
        AM, EU, AP
    }



    public class Ou : IDescribableEntity
    {
        [Key]
        public int ID { get; set; }

        [Required()]
        public Domain? Domain { get; set; }

        [Display(Name = "OrganizationalUnit", ResourceType = typeof(Resources.Resources))]
        [Required(ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "OrganizationalUnitRequired")]
        [StringLength(250, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "TwoFiftyLength")]
        public string OrganizationalUnit { get; set; }

        [Display(Name = "ResolverGroup", ResourceType = typeof(Resources.Resources))]
        [Required(ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "ResolverGroupRequired")]
        [StringLength(75, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "SeventyFiveLength")]
        public string ResolverGroup { get; set; }

        string IDescribableEntity.Describe()
        {
            return "{ Domain : \"" + Domain + "\", OrgUnit : \"" + OrganizationalUnit + "\", ResolverGrp : \"" + ResolverGroup + "\", ID : \"" + ID + "\"}";       
        
        }
    }
}