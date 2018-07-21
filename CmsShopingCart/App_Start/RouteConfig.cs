using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace CmsShopingCart
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute("Account", "Account/{action}/{id}", new { controller = "Account", action = "Index",id = UrlParameter.Optional }, new[] { "CmsShopingCart.Controllers" });


            routes.MapRoute("Cart", "Cart/{action}/{id}", new { controller = "Cart", action = "Index",id = UrlParameter.Optional }, new[] { "CmsShopingCart.Controllers" });


            routes.MapRoute("Shop", "Shop/{action}/{name}", new { controller = "Shop", action = "Index",name = UrlParameter.Optional }, new[] { "CmsShopingCart.Controllers" });


            routes.MapRoute("SidebarPartial", "pages/SidebarPartial", new { controller = "Pages", action = "SidebarPartial" }, new[] { "CmsShopingCart.Controllers" });
            routes.MapRoute("PagesMenuPartial", "pages/PagesMenuPartial", new { controller = "Pages", action = "PagesMenuPartial" }, new[] { "CmsShopingCart.Controllers" });
            routes.MapRoute("Pages", "{page}", new { controller = "Pages", action = "Index" }, new[] { "CmsShopingCart.Controllers" });
            routes.MapRoute("Default", "", new { controller = "Pages", action = "Index" }, new[] { "CmsShopingCart.Controllers" });

            //routes.MapRoute(
            //    name: "Default",
            //    url: "{controller}/{action}/{id}",
            //    defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            //);
        }
    }
}
