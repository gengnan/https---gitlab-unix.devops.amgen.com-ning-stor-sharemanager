using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace OnboardingApi.Models
{
    public enum Domain
    {
        AM, EU, AP
    }



    public class Ou
    {
        public int ID { get; set; }

        [Required()]
        public Domain? Domain { get; set; }

        public string OrganizationalUnit { get; set; }

        public string ResolverGroup { get; set; }
    }
}