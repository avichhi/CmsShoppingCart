﻿using CmsShopingCart.Models.Data;
using CmsShopingCart.Models.ViewModels.Cart;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;

namespace CmsShopingCart.Controllers
{
    public class CartController : Controller
    {
        private readonly Db db;
        public CartController()
        {
            db = new Db();
        }
        // GET: Cart
        public ActionResult Index()
        {
            var cart = Session["cart"] as List<CartVM> ?? new List<CartVM>();
            if (cart.Count == 0 || Session["cart"] == null)
            {
                ViewBag.Message = "Your Cart is Empty";
                return View();
            }
            decimal total = 0m;
            foreach (var item in cart)
            {
                total += item.Total;
            }
            ViewBag.GrandTotal = total;

            return View(cart);
        }
        public ActionResult CartPartial()
        {
            var model = new CartVM();
            int qty = 0;
            decimal price = 0m;
            if (Session["cart"] != null)
            {
                var list = (List<CartVM>)Session["Cart"];
                foreach (var item in list)
                {
                    qty += item.Quantity;
                    price += item.Price * item.Quantity;
                }
                model.Quantity = qty;
                model.Price = price;
            }
            else
            {
                model.Quantity = 0;
                model.Price = 0m;
            }
            return PartialView(model);
        }
        public ActionResult AddToCartPartial(int id)
        {
            var cart = Session["cart"] as List<CartVM> ?? new List<CartVM>();
            var model = new CartVM();
            var product = db.Products.Find(id);
            var productInCart = cart.FirstOrDefault(x => x.ProductId == id);
            if (productInCart == null)
            {
                cart.Add(new CartVM()
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    Quantity = 1,
                    Price = product.Price,
                    Image = product.ImageName
                });
            }
            else
            {
                productInCart.Quantity++;
            }
            int qty = 0;
            decimal price = 0m;
            foreach (var item in cart)
            {
                qty += item.Quantity;
                price += item.Price * item.Quantity;

            }
            model.Quantity = qty;
            model.Price = price;

            Session["cart"] = cart;
            return PartialView(model);
        }
        public JsonResult IncrementProduct(int productId)
        {
            var cart = Session["cart"] as List<CartVM> ?? new List<CartVM>();
            var model = cart.FirstOrDefault(x => x.ProductId == productId);
            model.Quantity++;
            var result = new { qty = model.Quantity , price = model.Price};
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        
        public JsonResult DecrementProduct(int productId)
        {
            var cart = Session["cart"] as List<CartVM> ?? new List<CartVM>();
            var model = cart.FirstOrDefault(x => x.ProductId == productId);
            if(model.Quantity > 1)
            {
                model.Quantity--;
            }
            else
            {
                model.Quantity = 0;
                cart.Remove(model); 
            }
            var result = new { qty = model.Quantity, price = model.Price };
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public void RemoveProduct(int productId)
        {
            var cart = Session["cart"] as List<CartVM> ?? new List<CartVM>();
            var model = cart.FirstOrDefault(x => x.ProductId == productId);
            cart.Remove(model);
        }
        public ActionResult PaypalPartial()
        {
            var cart = Session["cart"] as List<CartVM> ?? new List<CartVM>();

            return PartialView(cart);
        }
        [HttpPost]
        public void PlaceOrder()
        {
            var cart = Session["cart"] as List<CartVM> ?? new List<CartVM>();
            string username = User.Identity.Name;
            
            var orderDTO = new OrderDTO();
            var q = db.Users.FirstOrDefault(x => x.Username == username);
            int userId = q.Id;
            orderDTO.UserId = userId;
            orderDTO.CreateAt = DateTime.Now;
            db.Orders.Add(orderDTO);
            db.SaveChanges();
            var orderdId = orderDTO.OrderId;
            var orderDetailsDTO = new OrderDetailsDTO();
            foreach (var item in cart)
            {
                orderDetailsDTO.OrderId = orderdId;
                orderDetailsDTO.UserId = userId;
                orderDetailsDTO.ProductId = item.ProductId;
                orderDetailsDTO.Quantity = item.Quantity;
                db.OrderDetails.Add(orderDetailsDTO);
                db.SaveChanges();
            }

            try
            {
                MailMessage mail = new MailMessage();
                SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");
                //Email
                mail.From = new MailAddress("Email");
                mail.To.Add(q.EmailAddress);
                mail.Subject = "CMS Shooping Cart  Order  Initiated";
                mail.Body = "Your Order is Initiated";

                SmtpServer.Port = 587;
                //Email & Password
                SmtpServer.Credentials = new NetworkCredential("Email", "Password");
                SmtpServer.EnableSsl = true;

                SmtpServer.Send(mail);
            }
            catch (Exception)
            {
                RedirectToAction("Index", "Pages");
            }
            Session["cart"] = null;
        }
    }
    
}