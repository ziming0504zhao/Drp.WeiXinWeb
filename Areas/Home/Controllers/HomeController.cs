using System;
using System.Web.Mvc;
using Drp.Common;
using Drp.WeiXinWeb.Controllers;
using M2SA.AppGenome.Logging;
using Newtonsoft.Json;

//using System.Text.RegularExpressions;

namespace Drp.WeiXinWeb.Areas.Home.Controllers
{
    //如果将此特性加在Controller上，那么访问这个Controller里面的方法都需要验证用户登录状态
    public class HomeController : BaseController
    {
        public ActionResult Index()
        {
            try
            {
                ViewData["CustomerName"] = CustomerName();
                ViewData["WeixinUser"] = WeiXinUser();
                var customerId = CustomerId().ToString();
                LogManager.GetLogger().Info("customerId:" + customerId);
            }
            catch (Exception ex)
            {
                LogManager.GetLogger().Error(ex);
            }
            return View();
        }

    }
}
