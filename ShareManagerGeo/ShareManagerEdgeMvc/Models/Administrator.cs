using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace ShareManagerEdgeMvc.Models
{
    public enum AdminType
    {
        [Display(Name = "ShareAdmin", ResourceType = typeof(Resources.Resources))]
        Share = 0,
        [Display(Name = "StorageAdmin", ResourceType = typeof(Resources.Resources))]
        Storage = 1,
        [Display(Name = "BothAdmin", ResourceType = typeof(Resources.Resources))]
        Both = 2,
        [Display(Name = "SiteAdmin", ResourceType = typeof(Resources.Resources))]
        Site = 3
    }

    public class Administrator : IDescribableEntity
    {
        [Key]
        public int ID { get; set; }

        [Display(Name = "UserName", ResourceType = typeof(Resources.Resources))]
        [Required(ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "UserNameRequired")]
        [StringLength(75, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "SeventyFiveLength")]
        public string UserName { get; set; }

        [Display(Name = "UserAlias", ResourceType = typeof(Resources.Resources))]
        [Required(ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "UserAliasRequired")]
        [StringLength(25, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "TwentyFiveLength")]
        public string UserAlias { get; set; }

        public AdminType? AdminType { get; set; }

        string IDescribableEntity.Describe()
        {
            return "{ UserName : \"" + UserName + "\", UserAlias : \"" + UserAlias + "\", AdminType : \"" + AdminType + "\", ID : \"" + ID + "\"}";       
        }
    }
}

