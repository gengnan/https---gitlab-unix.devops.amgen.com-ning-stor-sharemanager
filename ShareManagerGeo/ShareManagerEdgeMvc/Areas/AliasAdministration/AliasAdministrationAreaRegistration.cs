using System.Web.Mvc;

namespace ShareManagerEdgeMvc.Areas.AliasAdministration
{
    public class AliasAdministrationAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "AliasAdministration";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "AliasAdministration_default",
                "AliasAdministration/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}