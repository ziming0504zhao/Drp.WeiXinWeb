using System;
using System.Web.Helpers;
using System.Web.Mvc;
using Drp.Common;
using Drp.Entity.Sys;
using Drp.Model.Sys;
using Drp.WeiXinWeb.Helpers;
using M2SA.AppGenome.Logging;

namespace Drp.WeiXinWeb.Areas.Common.Controllers
{
    /// <summary>
    /// 短信服务
    /// </summary>
    public class SmsController : Controller
    {
        [HttpPost]
        public JsonResult SmsPost(string mobile,string code)
        {
            try
            {
                //var code = StrHelper.GenerateRandomNumber(4);
                var result = SmsHelper.SmsSend(mobile, "43894",
                    "#code#=" + code);

                return Json("{\"code\":\"" + code + "\",\"result\":\"" + result + "\"}");
            }
            catch (Exception ex)
            {
                LogManager.GetLogger().Error(ex);
                return Json("{\"code\":\"0\",\"result\":\"" + ex.Message + "\"}");
            }
        }
    }
}