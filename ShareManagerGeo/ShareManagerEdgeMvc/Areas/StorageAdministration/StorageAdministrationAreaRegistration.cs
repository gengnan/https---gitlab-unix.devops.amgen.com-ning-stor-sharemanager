using System.Web.Mvc;

namespace ShareManagerEdgeMvc.Areas.StorageAdministration
{
    public class StorageAdministrationAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "StorageAdministration";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "StorageAdministration_default",
                "StorageAdministration/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}