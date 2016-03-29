using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShareManagerEdgeMvc.Models
{
    public class CifsShareAlias : IDescribableEntity
    {
        [Key]
        public int CifsShareAliasPairID { get; set; }

        [Required()]
        [Display(Name = "PrimaryCifsShare", ResourceType = typeof(Resources.Resources))]
        public int PrimaryCifsShareID { get; set; }

        [Required()]
        [Display(Name = "SecondaryCifsShare", ResourceType = typeof(Resources.Resources))]
        public int SecondaryCifsShareID { get; set; }

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

        //public virtual CifsShare CifsShareId { get; set; }

        [ForeignKey("PrimaryCifsShareID")]
        public virtual CifsShare PrimaryCifsShare { get; set; }
        [ForeignKey("SecondaryCifsShareID")]
        public virtual CifsShare SecondaryCifsShare { get; set; }

        string IDescribableEntity.Describe()
        {
            return "{ CifsShareAliasPairID : \"" + CifsShareAliasPairID +
                "\", PrimaryCifsShareID : \"" + PrimaryCifsShareID +
                "\", SecondaryCifsShareID : \"" + SecondaryCifsShareID +
                "\", CreatedOnDateTime : \"" + CreatedOnDateTime +
                "\", CreatedBy : \"" + CreatedBy +
                "\", ModifiedOnDateTime : \"" + ModifiedOnDateTime +
                "\", ModifiedBy : \"" + ModifiedBy +
                "\"}";

        }

    }
}