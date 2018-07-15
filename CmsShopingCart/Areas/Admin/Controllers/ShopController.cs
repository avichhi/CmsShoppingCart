using CmsShopingCart.Models.Data;
using CmsShopingCart.Models.ViewModels.Shop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using PagedList;
namespace CmsShopingCart.Areas.Admin.Controllers
{
    public class ShopController : Controller
    {
        private readonly Db db;
        public ShopController()
        {
            db = new Db();
        }
        // GET: Admin/Shop/Categories
        public ActionResult Categories()
        {
            List<CategoryVM> categoryList;
            categoryList = db.Categories.ToArray().OrderBy(x => x.Sorting).Select(x => new CategoryVM(x)).ToList();
            return View(categoryList);
        }


        [HttpPost]
        public string AddNewCategory(string catname)
        {
           
            if (db.Categories.Any(x => x.Name == catname))
                return "titletaken";
            var dto = new CategoryDTO();
            dto.Name = catname;
            dto.Slug = catname.Replace(" ", "-").ToLower();
            dto.Sorting = 100;
            db.Categories.Add(dto);
            db.SaveChanges();
            return dto.Id.ToString(); 

        }

        [HttpPost]
       
        public async Task<ActionResult> RecordCategories(int[] id)
        {
            var count = 1;
            CategoryDTO pagedto;
            foreach (var pageid in id)
            {
                pagedto = await db.Categories.FindAsync(pageid);
                pagedto.Sorting = count;
                await db.SaveChangesAsync();
                count++;
            }



            return RedirectToAction("Categories");
        }
        
        public async Task<ActionResult> DeleteCategory(int id)
        {

            CategoryDTO catdto = await db.Categories.FindAsync(id);
            if (catdto.Slug == "home")
            {
                TempData["DM"] = "The Slug of the home is not Deleted";
                return RedirectToAction("Index");
            }
            db.Categories.Remove(catdto);
            await db.SaveChangesAsync();

            return RedirectToAction("Categories");
        }


        [HttpPost]
        public string RenameCategory(string newCatName, int id)
        {

            if (db.Categories.Any(x => x.Name == newCatName))
                return "titletaken";
            var dto = db.Categories.Find(id);
            dto.Name = newCatName;
            dto.Slug = newCatName.Replace(" ", "-").ToLower();
            db.SaveChanges();
            return "okay";


            
        }
        public ActionResult AddProduct()
        {
            var model = new ProductVM();
            model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
            return View(model);
        }
        [HttpPost]
        public ActionResult AddProduct(ProductVM model,HttpPostedFileBase file)
        {
            if (!ModelState.Any())
            {
                model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                return View(model);
            }
            if(db.Products.Any(x => x.Name == model.Name))
            {
                model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                ModelState.AddModelError("","The Product Name is already exisits");
                return View(model);
            }

            

            var product = new ProductDTO();

            product.Name = model.Name;
            product.Slug = model.Name.Replace(" ", "-").ToLower();
            product.Description = model.Description;
            product.Price = model.Price;
            product.CategoryId = model.CategoryId;
            var catDTO = db.Categories.FirstOrDefault(x=>x.Id == model.CategoryId);
            product.CategoryName = catDTO.Name;

            db.Products.Add(product);
            db.SaveChanges();

            //FileUpload
            var id = product.Id;
            TempData["SM"] = "You have Added a Product Successfully";

            var originalDirectory = new DirectoryInfo(string.Format("{0}Images\\Uploads", Server.MapPath(@"\")));
            var pathString1 = Path.Combine(originalDirectory.ToString(), "Products"); 
            var pathString2 = Path.Combine(originalDirectory.ToString(), "Products\\"+ id.ToString());
            var pathString3 = Path.Combine(originalDirectory.ToString(), "Products\\"+ id.ToString() + "\\Thumbs");
            var pathString4 = Path.Combine(originalDirectory.ToString(), "Products\\"+ id.ToString() + "\\Gallary");
            var pathString5 = Path.Combine(originalDirectory.ToString(), "Products\\"+ id.ToString() + "\\Gallary\\Thumbs");

            if (!Directory.Exists(pathString1))
                Directory.CreateDirectory(pathString1);
            if (!Directory.Exists(pathString2))
                Directory.CreateDirectory(pathString2);
            if (!Directory.Exists(pathString3))
                Directory.CreateDirectory(pathString3);
            if (!Directory.Exists(pathString4))
                Directory.CreateDirectory(pathString4);
            if (!Directory.Exists(pathString5))
                Directory.CreateDirectory(pathString5);

            if(file != null && file.ContentLength > 0)
            {
                var ext = file.ContentType.ToLower();
                
                if(ext != "image/jpg" && ext != "image/jpeg" && ext != "image/pjpeg" 
                   && ext != "image/gif" && ext != "image/x-png" && ext != "image/png")
                {

                    model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                    ModelState.AddModelError("", "The image extension not supported upload jpeg,gif,png");
                    return View(model);
                }
                var imageName = file.FileName;
                var dto = db.Products.Find(id);
                dto.ImageName = imageName;
                db.SaveChanges();

                var path = string.Format("{0}\\{1}", pathString2, imageName);
                var path2 = string.Format("{0}\\{1}", pathString3, imageName);
                file.SaveAs(path);
                var img = new WebImage(file.InputStream);
                img.Resize(200, 200);
                img.Save(path2);


            }
            return RedirectToAction("AddProduct");
        }
        public ActionResult Products(int? page,int? catId)
        {
            var pageNumber = page ?? 1;
            var listOfProductVM = db.Products.ToArray()
                                    .Where(x => catId == null || catId == 0 || x.CategoryId == catId)
                                    .Select(x => new ProductVM(x))
                                    .ToList();
            ViewBag.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");

            ViewBag.SelectedCat = catId.ToString();
            var onePageOfProducts = listOfProductVM.ToPagedList(pageNumber, 3); // will only contain 25 products max because of the pageSize
            ViewBag.OnePageOfProducts = onePageOfProducts;



            return View();
        }
        [HttpGet]
        public ActionResult EditProduct(int id)
        {
            var dto = db.Products.Find(id);
            if(dto == null)
            {
                return Content(@"The Product doesn't exixts");
            }
            var model = new ProductVM(dto);
            model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
            model.GallaryImages = Directory.EnumerateFiles(Server.MapPath("~/Images/Uploads/Products/" + id + "/Gallary/Thumbs"))
                                            .Select(fn => Path.GetFileName(fn));
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
         public ActionResult EditProduct(ProductVM model,HttpPostedFileBase file)
         {
            var id = model.Id;
            model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
            model.GallaryImages = Directory.EnumerateFiles(Server.MapPath("~/Images/Uploads/Products/" + id + "/Gallary/Thumbs"))
                                           .Select(fn => Path.GetFileName(fn));

            if (!ModelState.IsValid)
            {
                return View(model);
            }
            if(db.Products.Where(x => x.Id != id ).Any(x => x.Name == model.Name))
            {
                ModelState.AddModelError("", "The Product Name is already there please change it");
                return View(model);
            }
            var dto = db.Products.Find(id);
            dto.Name = model.Name;
            dto.Slug = model.Name.Replace(" ", "-").ToLower();
            dto.Description = model.Description;
            dto.Price = model.Price;
            dto.CategoryId = model.CategoryId;
            dto.ImageName = model.ImageName;

            var catDTO = db.Categories.FirstOrDefault(x => x.Id == model.CategoryId);
            dto.CategoryName = catDTO.Name;
            db.SaveChanges();
            TempData["SM"] = "The Product is edited successfully";


            if (file != null && file.ContentLength > 0)
            {
                var ext = file.ContentType.ToLower();

                if (ext != "image/jpg" && ext != "image/jpeg" && ext != "image/pjpeg"
                   && ext != "image/gif" && ext != "image/x-png" && ext != "image/png")
                {

                    ModelState.AddModelError("", "The image extension not supported upload jpeg,gif,png");
                    return View(model);
                }
            }

            var originalDirectory = new DirectoryInfo(string.Format("{0}Images\\Uploads", Server.MapPath(@"\")));
            var pathString1 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString());
            var pathString2 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Thumbs");

            var di1 = new DirectoryInfo(pathString1);
            var di2 = new DirectoryInfo(pathString2);

            foreach (var file1 in di1.GetFiles())
                file1.Delete();
            foreach (var file2 in di2.GetFiles())
                file2.Delete();

            string imageName = file.FileName;
            var productDTO = db.Products.Find(id);
            productDTO.ImageName = imageName;
            db.SaveChanges();

            var path = string.Format("{0}\\{1}", pathString1, imageName);
            var path2 = string.Format("{0}\\{1}", pathString2, imageName);
            file.SaveAs(path);
            var img = new WebImage(file.InputStream);
            img.Resize(200, 200);
            img.Save(path2);

            return RedirectToAction("EditProduct");
        }

        public ActionResult DeleteProduct(int id)
        {
            var dto = db.Products.Find(id);
            db.Products.Remove(dto);
            db.SaveChanges();

            var originalDirectory = new DirectoryInfo(string.Format("{0}Images\\Uploads", Server.MapPath(@"\")));
            var pathString1 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString());

            if (Directory.Exists(pathString1))
                Directory.Delete(pathString1,true);

            return RedirectToAction("Products");
        }

        [HttpPost]
        public void SaveGallaryImages(int id)
        {
            
            foreach (string fileName in Request.Files)
            {
                var file = Request.Files[fileName];
                if (file != null && file.ContentLength > 0)
                {
                    var originalDirectory = new DirectoryInfo(string.Format("{0}Images\\Uploads", Server.MapPath(@"\")));
                    var pathString1 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallary");
                    var pathString2 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallary\\Thumbs");
                    var path = string.Format("{0}\\{1}", pathString1, file.FileName);
                    var path2 = string.Format("{0}\\{1}", pathString2, file.FileName);
                    file.SaveAs(path);
                    var img = new WebImage(file.InputStream);
                    img.Resize(200, 200);
                    img.Save(path2);


                }
            }
            
        }
        [HttpPost]
        public void DeleteImage(int id, string imageName)
        {
            var fullPath1 = Request.MapPath(@"~/Images/Uploads/Products/" + id.ToString() + "/Gallary/"+ imageName);
            var fullPath2 = Request.MapPath(@"~/Images/Uploads/Products/" + id.ToString() + "/Gallary/Thumbs/"+ imageName);
            if (System.IO.File.Exists(fullPath1))
                System.IO.File.Delete(fullPath1);
            if (System.IO.File.Exists(fullPath2))
                System.IO.File.Delete(fullPath2);
        }

    }

}