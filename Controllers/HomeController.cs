using System.Web.Mvc;

//using System.Text.RegularExpressions;

namespace Drp.WeiXinWeb.Controllers
{
    //如果将此特性加在Controller上，那么访问这个Controller里面的方法都需要验证用户登录状态
    public class HomeController : BaseController
    {
        public ActionResult Index()
        {
            return View();
        }

    }
}
