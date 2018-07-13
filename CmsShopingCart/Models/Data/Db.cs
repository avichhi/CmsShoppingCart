using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace CmsShopingCart.Models.Data
{
    public class Db:DbContext
    {
        public Db()
           : base("Db")
        {   
        }
        public DbSet<PagesDTO> Pages { get; set; }
        public DbSet<SidebarDTO> Sidebars { get; set; }
    }
}