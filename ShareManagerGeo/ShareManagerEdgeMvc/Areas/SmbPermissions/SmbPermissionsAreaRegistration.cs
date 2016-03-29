using System.Web.Mvc;

namespace ShareManagerEdgeMvc.Areas.SmbPermissions
{
    public class SmbPermissionsAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "SmbPermissions";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "SmbPermissions_default",
                "SmbPermissions/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}