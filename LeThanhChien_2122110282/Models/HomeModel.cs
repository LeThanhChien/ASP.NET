using LeThanhChien_2122110282.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LeThanhChien_2122110282.Models
{
    public class HomeModel
    {
        public List<Product> ListProduct { get; set; }
        public List<Category> ListCategory { get; set; }

        public List<Brand> ListBrand { get; set; }
        public int ProductCount { get; set; }
        public int OrderCount { get; set; }
        public int MemberCount { get; set; }
    }
}