using LeThanhChien_2122110282.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LeThanhChien_2122110282.Models
{
    public class CartModel
    {
        public Product Product { get; set; }
        public int Quantity { get; set; }
        public double Price { get; set; }

    }
    public class WishlistItem
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}