using CmsShopingCart.Models.Data;
using CmsShopingCart.Models.ViewModels.Shop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CmsShopingCart.Controllers
{
    public class ShopController : Controller
    {
        private readonly Db db;


        public ShopController()
        {
            db = new Db();
        }
        // GET: Shop
        public ActionResult Index()
        {
            return View("Index","Pages");
        }
        public ActionResult CategoryMenuPartial()
        {
            var categoryVMList = db.Categories.ToArray().OrderBy(x => x.Sorting).Select(x => new CategoryVM(x)).ToList();
            return PartialView(categoryVMList);
        }

        public ActionResult Category(string name)
        {
            var categoryDTO = db.Categories.Where(x => x.Slug == name).FirstOrDefault();
            int catId = categoryDTO.Id;

            var productVMList = db.Products.ToArray().Where(x => x.CategoryId == catId).Select(x => new ProductVM(x)).ToList();

            var productCat = db.Products.Where(x => x.CategoryId == catId).FirstOrDefault();
            ViewBag.CategoryName = productCat.CategoryName;
            return View(productVMList);
        }

        [ActionName("product-details")]
        public ActionResult ProductDetails(string name)
        {
            int id = 0;
            if(!db.Products.Any(x=>x.Slug.Equals(name)))
            {
                return RedirectToAction("Index", "Pages");
            }
            var dto = db.Products.Where(x => x.Slug == name).FirstOrDefault();
            id = dto.Id;
            var model = new ProductVM(dto);
            model.GallaryImages = Directory.EnumerateFiles(Server.MapPath("~/Images/Uploads/Products/" + id + "/Gallary/Thumbs"))
                                            .Select(fn => Path.GetFileName(fn));
            return View("ProductDetails",model);
        }
    }
}