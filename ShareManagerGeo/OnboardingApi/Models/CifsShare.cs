using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using OnboardingApi.Helpers;

using System.Web.Mvc;

namespace OnboardingApi.Models
{

    public enum Status
    {
        InService = 0,
        OutOfService = 1,
        Offline = 2,
        Retired = 3
    }

    public enum IsFsrShare
    {
        Yes = 0,
        No = 1
    }
    
    public class CifsShare
    {
        public int CifsShareID { get; set; }

        [Required()]
        public int OuID { get; set; }

        public string Name { get; set; }

        public string CmdbCi { get; set; }

        public string UncPath { get; set; }

        public System.Nullable<int> ParentShareId { get; set; }

        public bool IsFsrShare { get; set; }

        public string ShareOwnerFunctionalArea { get; set; }

        public string ShareOwnerCostCenter { get; set; }

        public string OwnerGroup { get; set; }

        public string ReadOnlyGroup { get; set; }

        public string ReadWriteGroup { get; set; }

        public string NoChangeGroup { get; set; }

        public Status? Status { get; set; }

        public DateTime? CreatedOnDateTime { get; set; }

        public string CreatedBy { get; set; }

        public DateTime? ModifiedOnDateTime { get; set; }

        public string ModifiedBy { get; set; }

        // Key relationship plumbing
        public virtual Ou Ou { get; set; }

        
    }
}