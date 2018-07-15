using CmsShopingCart.Models.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CmsShopingCart.Models.ViewModels.Shop
{
    public class CategoryVM
    {
        public CategoryVM()
        {

        }
        public CategoryVM(CategoryDTO row)
        {
            Id = row.Id;
            Name = row.Name;
            Slug = row.Slug;
            Sorting = row.Sorting;
        }




        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        public string Slug { get; set; }

        public int Sorting { get; set; }
    }
}