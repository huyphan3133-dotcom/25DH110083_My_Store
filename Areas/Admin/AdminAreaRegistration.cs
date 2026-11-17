using System.Web.Mvc;

namespace _25DH110083_My_Store.Areas.Admin
{
    public class AdminAreaRegistration : AreaRegistration 
    {
        public override string AreaName => "Admin";

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
            "Admin_default",
            "Admin/{controller}/{action}/{id}",
            new { action = "Index", id = UrlParameter.Optional },
            new[] { "_25DH110083_My_Store.Areas.Admin.Controllers" }
            );
        }
    }
}