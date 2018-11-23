using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Drp.Model;
using M2SA.AppGenome;
using YiYouLun.Weixin.MP.CommonAPIs;

namespace Drp.WeiXinWeb
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            AccessTokenContainer.Register(ConstValue.WeiXinConfig.AppId, ConstValue.WeiXinConfig.AppSecret);
            ApplicationHost.GetInstance().Start();
        }

        protected void Application_End(object sender, EventArgs e)
        {
            ApplicationHost.GetInstance().Stop();
        }
    }
}
