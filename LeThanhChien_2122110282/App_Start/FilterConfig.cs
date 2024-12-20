using System.Web;
using System.Web.Mvc;

namespace LeThanhChien_2122110282
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
