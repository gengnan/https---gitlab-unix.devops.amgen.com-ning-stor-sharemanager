using System.Web.Mvc;

namespace ShareManagerEdgeMvc.Areas.owner
{
    public class ownerAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "owner";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "owner_default",
                "owner/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}