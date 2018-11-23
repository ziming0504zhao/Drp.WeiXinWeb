using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web.Mvc;
using Drp.Common;
using Drp.Model;
using Drp.Model.Sys;
using Drp.Model.WeiXin;
using M2SA.AppGenome.Logging;
using Newtonsoft.Json;
using YiYouLun.Weixin.MP.AdvancedAPIs;
using YiYouLun.Weixin.MP.Entities;
using YiYouLun.Weixin.MP.Helpers;

namespace Drp.WeiXinWeb.Controllers
{
    public class BaseController : Controller
    {
        protected readonly string appId = ConstValue.WeiXinConfig.AppId;
        protected readonly string secret = ConstValue.WeiXinConfig.AppSecret;

        /// <summary>  
        /// 在控制器执行方法之前执行     
        /// </summary>  
        /// <param name="filterContext"></param>  
        protected override void OnActionExecuted(ActionExecutedContext filterContext) //protected 只能被子类访问  
        {
            base.OnActionExecuted(filterContext);
            var url = filterContext.HttpContext.Request.Url.OriginalString.Replace(":80", "");
            LogManager.GetLogger().Error("url:" + url);
            if (Session["oauth"] == null)
            {
                //filterContext.Result =
                //    new RedirectResult(
                //        YiYouLun.Weixin.MP.AdvancedAPIs.OAuth.GetAuthorizeUrl(
                //            ConstValue.WeiXinConfig.AppId, "http://qudao.bjseetheworld.com/oauth2/BaseCallback?reurl=" +
                //                                           url,
                //            "JeffreySu",
                //            YiYouLun.Weixin.MP.AdvancedAPIs.OAuthScope.snsapi_base));

                filterContext.Result =
                        new RedirectResult(
                            YiYouLun.Weixin.MP.AdvancedAPIs.OAuth.GetAuthorizeUrl(
                                ConstValue.WeiXinConfig.AppId, "http://qudao.bjseetheworld.com/oauth2/BaseCallback?reurl=" +
                                                               url,
                                "JeffreySu",
                                YiYouLun.Weixin.MP.AdvancedAPIs.OAuthScope.snsapi_userinfo));
            }
            Session["JsSdkUiPackage"] = GetJsSdkUiPackage(url);
            //Session["ShareUrl"] = "http://" + Request.Url.Authority + "/card/home/index?s=" +
            //                      Dencryptor.AESEncrypt(AccountId().ToString());
        }

        protected override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            var mpFileVersionInfo = FileVersionInfo.GetVersionInfo(Server.MapPath("~/bin/YiYouLun.Weixin.MP.dll"));
            var extensionFileVersionInfo =
                FileVersionInfo.GetVersionInfo(Server.MapPath("~/bin/YiYouLun.Weixin.MP.MvcExtension.dll"));
            TempData["MpVersion"] = string.Format("{0}.{1}", mpFileVersionInfo.FileMajorPart,
                mpFileVersionInfo.FileMinorPart); //Regex.Match(fileVersionInfo.FileVersion, @"\d+\.\d+");
            TempData["ExtensionVersion"] = string.Format("{0}.{1}", extensionFileVersionInfo.FileMajorPart,
                extensionFileVersionInfo.FileMinorPart); //Regex.Match(fileVersionInfo.FileVersion, @"\d+\.\d+");

            

            base.OnResultExecuting(filterContext);
        }

        /// <summary>
        /// 客户Id
        /// </summary>
        /// <returns></returns>
        public int CustomerId()
        {
            try
            {
                var weixinUsers = WeixinUser.FindByList(OpenId());
                return null != weixinUsers && weixinUsers.Any() ? weixinUsers.First().CustomerId : 0;
            }
            catch (Exception ex)
            {
                LogManager.GetLogger().Error(ex);
                return 0;
            }
        }

        /// <summary>
        /// 客户姓名
        /// </summary>
        /// <returns></returns>
        public string CustomerName()
        {
            try
            {
                var weixinUsers = WeixinUser.FindByList(OpenId());
                var accountId = null != weixinUsers && weixinUsers.Any() ? weixinUsers.First().CustomerId : 0;
                var account = Account.FindById(accountId);
                return null != account ? account.Name : string.Empty;
            }
            catch (Exception ex)
            {
                LogManager.GetLogger().Error(ex);
                return "";
            }
        }


        public WeixinUser WeiXinUser()
        {
            try
            {
                var weixinUsers = WeixinUser.FindByList(OpenId());
                return null != weixinUsers && weixinUsers.Any() ? weixinUsers.First() : null;
            }
            catch (Exception ex)
            {
                LogManager.GetLogger().Error(ex);
                return null;
            }
        }

        #region 微信公众号

        /// <summary>
        /// 获取公众号二维码地址
        /// </summary>
        public string ShowQrCodeUrl(int lineId)
        {
            try
            {
                //微信用户信息
                if (!YiYouLun.Weixin.MP.CommonAPIs.AccessTokenContainer.CheckRegistered(appId))
                {
                    YiYouLun.Weixin.MP.CommonAPIs.AccessTokenContainer.Register(appId, secret);
                }
                var tokenResult = YiYouLun.Weixin.MP.CommonAPIs.AccessTokenContainer.GetTokenResult(appId);
                var createQrCodeResult = QrCode.Create(tokenResult.access_token, 3600, lineId);
                var showQrCodeUrl = QrCode.GetShowQrCodeUrl(createQrCodeResult.ticket);
                LoggerHelper.ToLog("CreateQrCodeResult:" + JsonConvert.SerializeObject(createQrCodeResult));
                LoggerHelper.ToLog("ShowQrCodeUrl:" + showQrCodeUrl);

                return showQrCodeUrl;
            }
            catch (Exception ex)
            {
                LogManager.GetLogger().Error(ex);
            }

            return "";
        }

        /// <summary>
        /// 发送微信信息
        /// </summary>
        public void SendWeiXinMsg(string msg)
        {
            try
            {
                //微信用户信息
                if (!YiYouLun.Weixin.MP.CommonAPIs.AccessTokenContainer.CheckRegistered(appId))
                {
                    YiYouLun.Weixin.MP.CommonAPIs.AccessTokenContainer.Register(appId, secret);
                }
                var tokenResult = YiYouLun.Weixin.MP.CommonAPIs.AccessTokenContainer.GetTokenResult(appId);
                if (!string.IsNullOrEmpty(OpenId()))
                {
                    YiYouLun.Weixin.MP.AdvancedAPIs.Custom.SendText(tokenResult.access_token, OpenId(), msg);
                }
            }
            catch (Exception ex)
            {
                LogManager.GetLogger().Error(ex);
            }
        }

        /// <summary>
        /// 发送微信信息
        /// </summary>
        public void SendWeiXinMsg(string msg, string openId)
        {
            try
            {
                //微信用户信息
                if (!YiYouLun.Weixin.MP.CommonAPIs.AccessTokenContainer.CheckRegistered(appId))
                {
                    YiYouLun.Weixin.MP.CommonAPIs.AccessTokenContainer.Register(appId, secret);
                }
                var tokenResult = YiYouLun.Weixin.MP.CommonAPIs.AccessTokenContainer.GetTokenResult(appId);
                if (!string.IsNullOrEmpty(openId))
                {
                    YiYouLun.Weixin.MP.AdvancedAPIs.Custom.SendText(tokenResult.access_token, openId, msg);
                }
            }
            catch (Exception ex)
            {
                LogManager.GetLogger().Error(ex);
            }
        }

        /// <summary>
        /// 发送图文消息
        /// </summary>
        public void SendWeiXinNews(string openId, List<Article> articles)
        {
            try
            {
                //微信用户信息
                if (!YiYouLun.Weixin.MP.CommonAPIs.AccessTokenContainer.CheckRegistered(appId))
                {
                    YiYouLun.Weixin.MP.CommonAPIs.AccessTokenContainer.Register(appId, secret);
                }
                var tokenResult = YiYouLun.Weixin.MP.CommonAPIs.AccessTokenContainer.GetTokenResult(appId);
                if (!string.IsNullOrEmpty(openId))
                {
                    var result = Custom.SendNews(tokenResult.access_token, openId, articles);
                    LoggerHelper.ToLog("SendWeiXinNews:" + JsonConvert.SerializeObject(result));
                }
            }
            catch (Exception ex)
            {
                LogManager.GetLogger().Error(ex);
            }
        }

        /// <summary>
        /// 发送微信模板消息
        /// </summary>
        /// <param name="openId"></param>
        /// <param name="templateId"></param>
        /// <param name="url"></param>
        /// <param name="weixinTemplate"></param>
        public void SendWeiXinTempMsg(string openId, string templateId, string url, object weixinTemplate)
        {
            try
            {
                //微信用户信息
                if (!YiYouLun.Weixin.MP.CommonAPIs.AccessTokenContainer.CheckRegistered(appId))
                {
                    YiYouLun.Weixin.MP.CommonAPIs.AccessTokenContainer.Register(appId, secret);
                }
                var tokenResult = YiYouLun.Weixin.MP.CommonAPIs.AccessTokenContainer.GetTokenResult(appId);
                if (!string.IsNullOrEmpty(openId))
                {
                    YiYouLun.Weixin.MP.AdvancedAPIs.Template.SendTemplateMessage(tokenResult.access_token, openId,
                        templateId, "#FF0000", url, weixinTemplate);
                }
            }
            catch (Exception ex)
            {
                LogManager.GetLogger().Error(ex);
            }
        }

        /// <summary>
        /// 获取微信OpenId
        /// </summary>
        public string OpenId()
        {
            return Session["OpenId"]?.ToString() ?? string.Empty;
        }

        /// <summary>
        /// JSSDK Helper
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public JsSdkUiPackage GetJsSdkUiPackage(string url)
        {
            return JSSDKHelper.GetJsSdkUiPackage(appId, secret, url);
        }

        #endregion

    }
}
