﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CmsShopingCart.Models.ViewModels.Account
{
    public class OrdersForUserVM
    {
        public int OrderNumber { get; set; }
        public decimal Total { get; set; }
        public Dictionary<string, int> ProductAndQty { get; set; }
        public DateTime CreateAt { get; set; }
    }
}