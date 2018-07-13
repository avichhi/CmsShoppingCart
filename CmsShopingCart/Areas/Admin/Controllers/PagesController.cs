using CmsShopingCart.Models.Data;
using CmsShopingCart.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CmsShopingCart.Areas.Admin.Controllers
{
    public class PagesController : Controller
    {
        private readonly Db db;
        public PagesController()
        {
            db = new Db();
        }
        // GET: Admin/Pages
        public ActionResult Index()
        {
            List<PagesVM> pagesList;

            
            
                pagesList = db.Pages.ToArray().OrderBy(x => x.Sorting).Select(x => new PagesVM(x)).ToList();
            

            return View(pagesList);
        }

        [HttpGet]
        public ActionResult AddPage()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddPage(PagesVM model)
        {
            if (!ModelState.IsValid)
                return View(model);

            string slug;


            PagesDTO page = new PagesDTO();
            
            
            if (string.IsNullOrWhiteSpace(model.Slug))
            {
                slug = model.Title.Replace(" ", "-").ToLower();
            }
            else
            {
                slug = model.Slug.Replace(" ", "-").ToLower();
            }


            if (db.Pages.Any(x => x.Title == model.Title) || db.Pages.Any(x => x.Slug == model.Slug))
            {
                ModelState.AddModelError("", "The Title or Slug is already exists");
                return View(model);
            }


            page.Title = model.Title;
            page.Slug = slug;
            page.Body = model.Body;
            page.HasSidebar = model.HasSidebar;
            page.Sorting = 100;

            db.Pages.Add(page);
            await db.SaveChangesAsync();

            TempData["SM"] = "The Data is Stored Successfully";

            return RedirectToAction("Index");
           
        }

        [HttpGet]
        public ActionResult EditPage(int id)
        {
            PagesVM model ;
            PagesDTO pagesdto = db.Pages.Find(id);
            if(pagesdto == null)
            {
                return Content("Page Does Not Exist !");
            }
            model = new PagesVM(pagesdto);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditPage(PagesVM model)
        {
            if (!ModelState.IsValid)
                return View(model);

            int id = model.Id;
            string slug = "home";


            PagesDTO page = await db.Pages.FindAsync(id);


           if(model.Slug != "home")
            {
                if (string.IsNullOrWhiteSpace(model.Slug))
                {
                    slug = model.Title.Replace(" ", "-").ToLower();
                }
                else
                {
                    slug = model.Slug.Replace(" ", "-").ToLower();
                }

            }

            if (db.Pages.Where(x => x.Id != id).Any(x => x.Title == model.Title) || db.Pages.Where(x => x.Id != id).Any(x => x.Slug == model.Slug))
            {
                ModelState.AddModelError("", "The Title or Slug is already exists");
                return View(model);
            }


            page.Title = model.Title;
            page.Slug = slug;
            page.Body = model.Body;
            page.HasSidebar = model.HasSidebar;
          

            
            await db.SaveChangesAsync();

            TempData["SM"] = "The Data is Edited Successfully";

            return RedirectToAction("Index");

        }


        public ActionResult DetailsPage(int id)
        {
            PagesVM model;
            PagesDTO pagedto = db.Pages.Find(id);
            if(pagedto == null)
            {
                return Content("There is No page");
            }
            model = new PagesVM(pagedto);
            return View(model);
        }

        
        public async Task<ActionResult> DeletePage(int id)
        {
           
            PagesDTO pagedto = await db.Pages.FindAsync(id);
            if (pagedto.Slug == "home")
            {
                TempData["DM"] = "The Slug of the home is not Deleted";
                return RedirectToAction("Index");
            }
            db.Pages.Remove(pagedto);
            await db.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]        
        public async Task<ActionResult> RecordPages(int[] id)
        {
            var count = 1;
            PagesDTO pagedto;
            foreach (var pageid in id)
            {
                pagedto = await db.Pages.FindAsync(pageid);
                pagedto.Sorting = count;
                await db.SaveChangesAsync();
                count++;
            }
            
            

            return RedirectToAction("Index");
        }
        [HttpGet]
        public ActionResult EditSidebar()
        {
            SidebarVM model;
            SidebarDTO sidebarDTO = db.Sidebars.Find(1);
            model = new SidebarVM(sidebarDTO); 
            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult>  EditSidebar(SidebarVM model)
        {
            
            SidebarDTO sidebarDTO = await db.Sidebars.FindAsync(1);
            sidebarDTO.Body = model.Body;
            await db.SaveChangesAsync();
            TempData["SM"] = "Sidebar edited successfully.";
            return RedirectToAction("EditSidebar");
        }
    }
}