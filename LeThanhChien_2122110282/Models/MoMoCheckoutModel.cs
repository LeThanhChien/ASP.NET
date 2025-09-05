using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LeThanhChien_2122110282.Models
{
    public class MoMoCheckoutModel
    {
        [AllowHtml] // Allow special characters in FullName
        public string FullName { get; set; }

        [AllowHtml] // Allow special characters in Address
        public string Address { get; set; }

        public string Email { get; set; }

        public string PaymentMethod { get; set; }
    }
}