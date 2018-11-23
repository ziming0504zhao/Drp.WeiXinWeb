using System;
using System.Linq;
using System.Web.Mvc;
using Drp.Common;
using Drp.Entity.WeiXin;
using Drp.Model.WeiXin;
using Newtonsoft.Json;
using YiYouLun.Weixin;
using YiYouLun.Weixin.Exceptions;
using YiYouLun.Weixin.MP.AdvancedAPIs;

namespace Drp.WeiXinWeb.Controllers
{
    public class OAuth2Controller : Controller
    {
        //下面换成账号对应的信息，也可以放入web.config等地方方便配置和更换
        private string appId = System.Configuration.ConfigurationManager.AppSettings["AppID"];
        private string secret = System.Configuration.ConfigurationManager.AppSettings["Appsecret"];

        public ActionResult Index()
        {
            //此页面引导用户点击授权
            //ViewData["UrlUserInfo"] = OAuth.GetAuthorizeUrl(appId, "http://qudao.bjseetheworld.com/oauth2/UserInfoCallback", "JeffreySu", OAuthScope.snsapi_userinfo);
            //var urlBaseUrl = OAuth.GetAuthorizeUrl(appId, "http://qudao.bjseetheworld.com/oauth2/BaseCallback", "JeffreySu",
            //    OAuthScope.snsapi_base);
            var urlBaseUrl = OAuth.GetAuthorizeUrl(appId, "http://qudao.bjseetheworld.com/oauth2/BaseCallback", "JeffreySu",
                OAuthScope.snsapi_userinfo);
            LoggerHelper.ToLog("urlBaseUrl:" + urlBaseUrl);
            ViewData["UrlBase"] = urlBaseUrl;
            return View();
        }

        public ActionResult UserInfoCallback(string code, string state)
        {
            LoggerHelper.ToLog("到这了1");
            if (string.IsNullOrEmpty(code))
            {
                return Content("您拒绝了授权！");
            }

            if (state != "JeffreySu")
            {
                //这里的state其实是会暴露给客户端的，验证能力很弱，这里只是演示一下
                //实际上可以存任何想传递的数据，比如用户ID，并且需要结合例如下面的Session["OAuthAccessToken"]进行验证
                return Content("验证失败！请从正规途径进入！");
            }

            //通过，用code换取access_token
            var result = OAuth.GetAccessToken(appId, secret, code);
            if (result.errcode != ReturnCode.请求成功)
            {
                return Content("错误：" + result.errmsg);
            }

            //下面2个数据也可以自己封装成一个类，储存在数据库中（建议结合缓存）
            //如果可以确保安全，可以将access_token存入用户的cookie中，每一个人的access_token是不一样的
            Session["OAuthAccessTokenStartTime"] = DateTime.Now;
            Session["OAuthAccessToken"] = result;

            //因为第一步选择的是OAuthScope.snsapi_userinfo，这里可以进一步获取用户详细信息
            try
            {
                var userInfo = OAuth.GetUserInfo(result.access_token, result.openid);
                return View(userInfo);
            }
            catch (ErrorJsonResultException ex)
            {
                return Content(ex.Message);
            }
        }

        public ActionResult BaseCallback(string code, string state)
        {
            LoggerHelper.ToLog("code:" + code);
            LoggerHelper.ToLog("state:" + state);
            Session["oauth"] = "true";
            ViewData["reurl"] = Request["reurl"];
            LoggerHelper.ToLog("reurl:" + Request["reurl"]);
            if (string.IsNullOrEmpty(code))
            {
                return Content("您拒绝了授权！");
            }

            if (state != "JeffreySu")
            {
                //这里的state其实是会暴露给客户端的，验证能力很弱，这里只是演示一下
                //实际上可以存任何想传递的数据，比如用户ID，并且需要结合例如下面的Session["OAuthAccessToken"]进行验证
                return Content("验证失败！请从正规途径进入！");
            }

            //通过，用code换取access_token
            var result = OAuth.GetAccessToken(appId, secret, code);
            LoggerHelper.ToLog("result:" + JsonConvert.SerializeObject(result));
            Session["OpenId"] = result.openid;
            {
                var weixinUsers = WeixinUser.FindByList(openId: result.openid);
                if (null== weixinUsers || !weixinUsers.Any())
                {
                    var weixinUser = new WeixinUserEntity();
                    weixinUser.NickName = result.openid;
                    weixinUser.Privilege = "";
                    weixinUser.Province = "";
                    weixinUser.OpenId = result.openid;
                    WeixinUser.Save(weixinUser);
                }
            }
            //LogManager.GetLogger().Error("openid:" + result.openid);
            if (result.errcode != ReturnCode.请求成功)
            {
                return Content("错误：" + result.errmsg);
            }

            //下面2个数据也可以自己封装成一个类，储存在数据库中（建议结合缓存）
            //如果可以确保安全，可以将access_token存入用户的cookie中，每一个人的access_token是不一样的
            Session["OAuthAccessTokenStartTime"] = DateTime.Now;
            Session["OAuthAccessToken"] = result;

            
            //因为这里还不确定用户是否关注本微信，所以只能试探性地获取一下
            OAuthUserInfo userInfo = null;
            try
            {
                LoggerHelper.ToLog("已关注，可以得到详细信息。。。。");
                //已关注，可以得到详细信息
                userInfo = OAuth.GetUserInfo(result.access_token, result.openid);

                LoggerHelper.ToLog("userInfo:" + JsonConvert.SerializeObject(userInfo));

                ViewData["ByBase"] = true;

                if (null != userInfo && !string.IsNullOrEmpty(userInfo.openid))
                {
                    var weixinUsers = WeixinUser.FindByList(openId: userInfo.openid);
                    var weixinUser = null != weixinUsers && weixinUsers.Any() ? weixinUsers.First() : null;
                    if (null != weixinUser)
                    {

                        weixinUser.City = userInfo.city;
                        weixinUser.Country = userInfo.country;
                        weixinUser.HeaderImage = userInfo.headimgurl;
                        weixinUser.NickName = userInfo.nickname;
                        weixinUser.Privilege = userInfo.province;
                        weixinUser.Province = "";

                        WeixinUser.Save(weixinUser);

                    }
                    else
                    {
                        var weixinUser1 = new WeixinUserEntity();
                        weixinUser1.City = userInfo.city;
                        weixinUser1.Country = userInfo.country;
                        weixinUser1.HeaderImage = userInfo.headimgurl;
                        weixinUser1.NickName = userInfo.nickname;
                        weixinUser.Privilege = userInfo.province;
                        weixinUser1.Province = "";
                        weixinUser1.OpenId = userInfo.openid;

                        WeixinUser.Save(weixinUser1);

                    }
                }


                var weixinUsers1 = WeixinUser.FindByList(openId: result.openid);
                Session["wUserInfo"] = null != weixinUsers1 && weixinUsers1.Any()
                    ? weixinUsers1.First()
                    : new WeixinUser();
                return View("UserInfoCallback", userInfo);
            }
            catch (ErrorJsonResultException ex)
            {
                //未关注，只能授权，无法得到详细信息
                //这里的 ex.JsonResult 可能为："{\"errcode\":40003,\"errmsg\":\"invalid openid\"}"
                LoggerHelper.ToLog("BaseCallback:" + ex);
                //return Content("用户已授权，授权Token：" + result);
                return View("UserInfoCallback", userInfo);
            }
        }
    }
}