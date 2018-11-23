using System;
using System.Web.Mvc;
using Drp.Common;
using Drp.Model.Customer;
using Drp.Model.Sys;
using Drp.WeiXinWeb.Controllers;
using M2SA.AppGenome.Logging;
using Newtonsoft.Json;

//using System.Text.RegularExpressions;

namespace Drp.WeiXinWeb.Areas.Home.Controllers
{
    //如果将此特性加在Controller上，那么访问这个Controller里面的方法都需要验证用户登录状态
    public class TrainController : BaseController
    {
        public ActionResult Index()
        {
            try
            {
                ViewData["CustomerName"] = CustomerName();
                ViewData["WeixinUser"] = WeiXinUser();
                var customerId = CustomerId();
                ViewData["CustomerBase"] = CustomerBase.FindById(customerId);
                LogManager.GetLogger().Info("customerId:" + customerId);

                ViewData["Trains"] = Train.FindByList();
            }
            catch (Exception ex)
            {
                LogManager.GetLogger().Error(ex);
            }
            return View();
        }

        public ActionResult TrainText(int id)
        {
            try
            {
                var train = Train.FindById(id);
                if (null!= train)
                {
                    train.ViewCount++;
                    Train.Save(train);
                }

                ViewData["Train"] = train;
            }
            catch (Exception ex)
            {
                LogManager.GetLogger().Error(ex);
            }
            return View();
        }

        public ActionResult TrainVideo(int id)
        {
            try
            {
                var train = Train.FindById(id);
                if (null != train)
                {
                    train.ViewCount++;
                    Train.Save(train);
                }

                ViewData["Train"] = train;
            }
            catch (Exception ex)
            {
                LogManager.GetLogger().Error(ex);
            }
            return View();
        }

    }
}
