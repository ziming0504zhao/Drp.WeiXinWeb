using System;
using System.Linq;
using System.Web.Mvc;
using Drp.Common;
using Drp.Entity.Customer;
using Drp.Model;
using Drp.Model.Customer;
using Drp.Model.Sys;
using Drp.Model.WeiXin;
using Drp.WeiXinWeb.Controllers;
using M2SA.AppGenome.Cache;
using M2SA.AppGenome.Logging;

//using System.Text.RegularExpressions;

namespace Drp.WeiXinWeb.Areas.Me.Controllers
{
    //如果将此特性加在Controller上，那么访问这个Controller里面的方法都需要验证用户登录状态
    public partial class JoinUsController : BaseController
    {
        public ActionResult Index()
        {
            try
            {
                ViewData["CustomerName"] = CustomerName();
                ViewData["WeixinUser"] = WeiXinUser();
                var customerId = CustomerId().ToString();
                LogManager.GetLogger().Info("customerId:" + customerId);

                var cities =
                    DestinationBase.FindByList(destinationTypeIds: ConstValue.Catalog.DestinationType.City.ToString());
                var uId = CacheHelper.Get(OpenId());
                if (null != cities && cities.Any())
                {
                    cities =
                        cities.Where(
                            p =>
                                (null != p.Parent && p.Parent.Name.Equals("中国")) ||
                                (null != p.Parent && null != p.Parent.Parent && p.Parent.Parent.Name.Equals("中国")))
                            .ToList();
                }
                ViewData["Cities"] = cities;
                LogManager.GetLogger().Info("JoinUs-uId:" + uId);
            }
            catch (Exception ex)
            {
                LogManager.GetLogger().Error(ex);
            }
            return View();
        }

        public ActionResult AccountBind()
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

        public ActionResult JoinUsSuccess(int customerId)
        {
            ViewData["CustomerId"] = customerId;
            return View();
        }

    }

    public partial class JoinUsController
    {
        [HttpPost]
        public JsonResult JoinUsSubmit(FormCollection values)
        {
            var resultInfo = new ResultInfo<object>(null, true, "");
            try
            {
                var mobile = values["Mobile"];
                LogManager.GetLogger().Error("Mobile:" + mobile);

                var name = values["Name"];
                LogManager.GetLogger().Error("Name:" + name);

                var cityName = values["CityName"];
                LogManager.GetLogger().Error("CityName:" + cityName);

                if (CustomerBase.FindByList(mobile: mobile).Any())
                {
                    resultInfo.IsSuccess = false;
                    resultInfo.Message = "手机号码己存在!";
                    return Json(resultInfo);
                }
                var uId = CacheHelper.Get(OpenId());
                var parentId = uId?.ToString() ?? "0";
                var customer= CustomerBase.FindById(int.Parse(parentId));

                var customerBaseEntity = new CustomerBaseEntity();
                customerBaseEntity.Mobile = mobile;
                customerBaseEntity.Name = name;
                customerBaseEntity.CityName = cityName;
                customerBaseEntity.TypeId = 2;
                if (null != customer)
                {
                    customerBaseEntity.ParentId = customer.Id;
                    customerBaseEntity.ParentName = customer.Name;
                }
                var flag = CustomerBase.Save(customerBaseEntity);
                if (flag)
                {
                    var weiXinUser = WeiXinUser();
                    if (null!= weiXinUser)
                    {
                        weiXinUser.CustomerId = customerBaseEntity.Id;
                        WeixinUser.Save(weiXinUser);
                    }
                }
                resultInfo.Message = customerBaseEntity.Id.ToString();
            }
            catch (Exception ex)
            {
                LogManager.GetLogger().Error(ex);
                resultInfo.IsSuccess = false;
                resultInfo.Message = ex.Message;
            }

            return Json(resultInfo);
        }

        [HttpPost]
        public JsonResult AccountBindSubmit(FormCollection values)
        {
            var resultInfo = new ResultInfo<object>(null, true, "");
            try
            {
                var mobile = values["Mobile"];
                LogManager.GetLogger().Error("Mobile:" + mobile);
                var customerBases = CustomerBase.FindByList(mobile: mobile);

                if (null != customerBases && customerBases.Any())
                {
                    var weiXinUser = WeiXinUser();
                    if (null != weiXinUser)
                    {
                        weiXinUser.CustomerId = customerBases.First().Id;
                        WeixinUser.Save(weiXinUser);

                        resultInfo.Message = weiXinUser.CustomerId.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.GetLogger().Error(ex);
                resultInfo.IsSuccess = false;
                resultInfo.Message = ex.Message;
            }

            return Json(resultInfo);
        }
    }
}
