using CmsShopingCart.Models.Data;
using CmsShopingCart.Models.ViewModels.Account;
using CmsShopingCart.Models.ViewModels.Shop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace CmsShopingCart.Controllers
{
   
    public class AccountController : Controller
    {
        private readonly Db db;

        public AccountController()
        {
            db = new Db();
        }
        // GET: Account
        public ActionResult Index()
        {
            return Redirect("~/account/login");
        }

        [HttpGet]
        public ActionResult Login()
        {
            var username = User.Identity.Name;
            if (!string.IsNullOrEmpty(username))
                return RedirectToAction("user-profile");
            return View();
        }

        [HttpPost]
        public ActionResult Login(LoginUserVM model)
        {
            if (!ModelState.IsValid)
                return View(model);
            var isValid = false;
            if(db.Users.Any(x=>x.Username.Equals(model.Username) && x.Password.Equals(model.Password)))
            {
                isValid = true;
            }
            if (!isValid)
            {
                ModelState.AddModelError("", "Username or Password is Invalid check again.");
                return View( model);

            }
            else
            {
                FormsAuthentication.SetAuthCookie(model.Username, model.RememberMe);
                return Redirect(FormsAuthentication.GetRedirectUrl(model.Username, model.RememberMe));
            }
        }

        [HttpGet]
        [ActionName("create-account")]
        public ActionResult CreateAccount()
        {
            return View("CreateAccount");
        }


        [ActionName("create-account")]
        [HttpPost]
        public ActionResult CreateAccount(UserVM model)
        {
            if (!ModelState.IsValid)
                return View("CreateAccount", model);
            if (!model.Password.Equals(model.ConfirmPassword))
            {
                ModelState.AddModelError("", "Password donot match");
                return View("CreateAccount", model);
            }
            if (db.Users.Any(x => x.Username.Equals(model.Username)))
            {
                ModelState.AddModelError("", "Username " + model.Username + " Exist");
                model.Username = "";
                return View("CreateAccount", model);
            }
            if (db.Users.Any(x => x.EmailAddress.Equals(model.EmailAddress)))
            {
                ModelState.AddModelError("", "Email " + model.EmailAddress + " Exist");
                model.EmailAddress   = "";
                return View("CreateAccount", model);
            }
            var userDTO = new UserDTO()
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                EmailAddress = model.EmailAddress,
                Username = model.Username,
                Password = model.Password
            };
            db.Users.Add(userDTO);
            db.SaveChanges();
            int id = userDTO.Id;
            var userRoles = new UserRoleDTO() {
                UserId = id,
                RoleId = 2
            };
            db.UserRoles.Add(userRoles);
            db.SaveChanges();
            TempData["SM"] = "You are now register and can login";

            return Redirect("~/account/login"); 
        }
        [Authorize]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return Redirect("~/account/login");
        }

        [Authorize]
        public ActionResult UserNavPartial()
        {
            var username = User.Identity.Name;
            var dto = db.Users.FirstOrDefault(x => x.Username  == username);
            var model = new UserNavPartialVM() {

                FirstName = dto.FirstName,
                LastName = dto.LastName
            };
            return PartialView(model);
        }


        [ActionName("user-profile")]
        [HttpGet]
        [Authorize]
        public ActionResult UserProfile()
        {
            var username = User.Identity.Name;
            var dto = db.Users.FirstOrDefault(x => x.Username == username);
            var model = new UserProfileVM(dto); 
            return View("UserProfile",model);
        }


        [ActionName("user-profile")]
        [HttpPost]
        [Authorize]
        public ActionResult UserProfile(UserProfileVM model)
        {
            if (!ModelState.IsValid)
                return View("UserProfile", model);
            var username = User.Identity.Name;
            if (!db.Users.Where(x=>x.Id==model.Id).Any(x => x.Username == model.Username))
            {
                ModelState.AddModelError("", "Username " + model.Username + " Exist");
                model.Username = "";
                return View("UserProfile", model);
            }
            if (!db.Users.Where(x => x.Id == model.Id).Any(x => x.EmailAddress == model.EmailAddress))
            {
                ModelState.AddModelError("", "Email " + model.EmailAddress + " Exist");
                model.EmailAddress = "";
                return View("UserProfile", model);
            }
            var userDTO = db.Users.Find(model.Id);

            userDTO.FirstName = model.FirstName;
            userDTO.LastName = model.LastName;
            userDTO.EmailAddress = model.EmailAddress;
            userDTO.Username = model.Username;
            if(!string.IsNullOrWhiteSpace(model.Password))
                userDTO.Password = model.Password;

            
            db.SaveChanges();
            TempData["SM"] = "You have edited your profile successfully";

            return Redirect("~/account/user-profile");

        }


        [Authorize(Roles = "User")]
        public ActionResult Orders()
        {
            var ordersForUser = new List<OrdersForUserVM>();
            var user = db.Users.Where(x => x.Username == User.Identity.Name).FirstOrDefault();
            var userId = user.Id;
            var orders = db.Orders.Where(x => x.UserId == userId).ToArray().Select(x => new OrderVM(x)).ToList();
            foreach (var order in orders)
            {

                var productsAndQty = new Dictionary<string, int>();
                var total = 0m;
                var orderDetailsList = db.OrderDetails.Where(x => x.OrderId == order.OrderId).ToList();

                foreach (var orderDetails in orderDetailsList)
                {
                    var product = db.Products.Where(x => x.Id == orderDetails.ProductId).FirstOrDefault();
                    var productname = product.Name;
                    var price = product.Price;
                    productsAndQty.Add(productname, orderDetails.Quantity);
                    total += orderDetails.Quantity * price;
                }
                ordersForUser.Add(new OrdersForUserVM()
                {
                    OrderNumber = order.OrderId,
                    Total = total,
                    ProductAndQty = productsAndQty,
                    CreateAt = order.CreateAt
                });

            }
            return View(ordersForUser);
        }
    }
}