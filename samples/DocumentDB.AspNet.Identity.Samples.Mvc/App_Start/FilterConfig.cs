using System.Web;
using System.Web.Mvc;

namespace DocumentDB.AspNet.Identity.Samples.Mvc
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
