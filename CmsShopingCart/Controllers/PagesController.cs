using CmsShopingCart.Models.Data;
using CmsShopingCart.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CmsShopingCart.Controllers
{
    public class PagesController : Controller
    {
        private readonly Db db;


        public PagesController()
        {
            db = new Db();
        }
        // GET: Pages
        public ActionResult Index(string page ="")
        {
            if (page == "")
                page = "home";
            if (!db.Pages.Any(x => x.Slug.Equals(page)))
            {
                return RedirectToAction("Index" , new { page = ""} );
            }

            var dto = db.Pages.Where(x => x.Slug == page).FirstOrDefault();
            ViewBag.PageTitle = dto.Title;
            ViewBag.HasSidebar = dto.HasSidebar;
            
            
            var model = new PagesVM(dto);

            return View(model);
        }

        public ActionResult PagesMenuPartial()
        {
            var pageVMList = db.Pages.ToArray().Where(x => x.Slug != "home").Select(x => new PagesVM(x)).ToList();

            return PartialView(pageVMList);
        }
        public ActionResult SidebarPartial()
        {
            var dto = db.Sidebars.Find(1);
            var model = new SidebarVM(dto);
            return PartialView(model);
        }
    }
}