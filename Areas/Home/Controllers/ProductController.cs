using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Drp.Common;
using Drp.Entity.Customer;
using Drp.Model;
using Drp.Model.Customer;
using Drp.Model.Product;
using Drp.Model.Sys;
using Drp.WeiXinWeb.Controllers;
using M2SA.AppGenome.Cache;
using M2SA.AppGenome.Logging;
using M2SA.AppGenome.Reflection;
using Newtonsoft.Json;
using ThoughtWorks.QRCode.Codec;
using YiYouLun.Weixin.MP.AdvancedAPIs;
using YiYouLun.Weixin.MP.Entities;

//using System.Text.RegularExpressions;

namespace Drp.WeiXinWeb.Areas.Home.Controllers
{
    //如果将此特性加在Controller上，那么访问这个Controller里面的方法都需要验证用户登录状态
    public partial class ProductController : BaseController
    {
        public ActionResult Index(int lineId, int? uId = 0)
        {
            try
            {
                ViewData["CustomerName"] = CustomerName();
                var weiXinUser = WeiXinUser();
                ViewData["WeixinUser"] = weiXinUser;
                var customerId = CustomerId().ToString();
                LogManager.GetLogger().Info("customerId:" + customerId);

                var productLine = ProductLine.FindById(lineId);
                ViewData["ProductLine"] = productLine;
                ViewData["CustomerId"] = customerId;
                var customerLineCollects = CustomerLineCollect.FindByList(lineId: lineId,
                    customerId: int.Parse(customerId));
                var isLineCollects = null != customerLineCollects && customerLineCollects.Any() ? "1" : "0";
                ViewData["IsLineCollects"] = isLineCollects;
                var uid = uId ?? 0;
                if (!string.IsNullOrEmpty(OpenId()))
                {
                    CacheHelper.Set(OpenId(), uid);
                }
                LogManager.GetLogger().Info("uId：" + uid);
                LogManager.GetLogger().Info("uId1：" + CacheHelper.Get(OpenId()));
                ViewData["ShowQrCodeUrl"] = ShowQrCodeUrl(lineId);

                #region 发送消息给客服,有客户在浏览某个产品

                LogManager.GetLogger().Info("发送消息给客服,有客户在浏览某个产品");

                try
                {
                    if (null != weiXinUser && !string.IsNullOrEmpty(weiXinUser.NickName))
                    {
                        var imgUrl = "";
                        if (!string.IsNullOrEmpty(productLine.ImgUrl))
                        {
                            imgUrl = productLine.ImgUrl.Split(',')[0];
                        }

                        //var articles = new List<Article>();
                        //articles.Add(new Article()
                        //{
                        //    Title = "【" + weiXinUser.NickName + "】正在访问当前产品",
                        //    Description = productLine.LineName,
                        //    Url = ConstValue.SysConfig.ImgUrl + "/home/Product/index?lineId=" + productLine.Id,
                        //    PicUrl = ConstValue.SysConfig.BossImgUrl + "/" + imgUrl
                        //});

                        var destNames = "";
                        if (null!=productLine.LineDests && productLine.LineDests.Any())
                        {
                            destNames = productLine.LineDests.Aggregate(destNames, (current, dest) => current + (dest.Name + "-"));
                            destNames = !string.IsNullOrEmpty(destNames) && destNames.EndsWith("-")
                                ? destNames.TrimEnd('-')
                                : destNames;
                        }

                        var productLinePrices = ProductLinePrice.FindByList(lineId: productLine.Id);
                        productLinePrices = productLinePrices.Where(p => DateTime.Parse(p.StartDate) > DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"))).OrderBy(p => p.SalePrice).ToList();
                        var data = new
                        {
                            //使用TemplateDataItem简单创建数据。
                            first =
                                new TemplateDataItem("【" + weiXinUser.NickName + "】正在访问【" + productLine.LineName + "】"),
                            keyword1 = new TemplateDataItem(productLine.LineName),
                            keyword2 =
                                new TemplateDataItem(productLinePrices.Any()
                                    ? productLinePrices.First().StartDate
                                    : string.Empty),
                            keyword3 = new TemplateDataItem(destNames),
                            remark = new TemplateDataItem("请及时与客户联系", "#173177"),
                        };
                        var url = ConstValue.SysConfig.ImgUrl + "/home/Product/index?lineId=" + productLine.Id;
                        if (!string.IsNullOrEmpty(ConstValue.WeiXinConfig.CustomerServiceOpenIds))
                        {
                            foreach (var openId in ConstValue.WeiXinConfig.CustomerServiceOpenIds.Split(','))
                            {
                                if (!string.IsNullOrEmpty(openId))
                                {
                                    SendWeiXinTempMsg(openId, ConstValue.WeiXinConfig.Template.BrowseId, url, data);
                                }
                            }
                        }

                        

                    }
                }
                catch (Exception ex)
                {
                    LogManager.GetLogger().Error(ex);
                }

                #endregion
            }
            catch (Exception ex)
            {
                LogManager.GetLogger().Error(ex);
            }
            return View();
        }

        public ActionResult LineList(int? suitableTypeId = null, int? productCategoryId = null, int? destId = null)
        {
            try
            {
                ViewData["CustomerName"] = CustomerName();
                ViewData["WeixinUser"] = WeiXinUser();
                var customerId = CustomerId().ToString();
                LogManager.GetLogger().Info("customerId:" + customerId);

                var productLines = ProductLine.FindByList(suitableTypeId: suitableTypeId,
                    productCategoryId: productCategoryId);
                if (null != productLines && null != destId)
                {
                    productLines =
                        productLines.Where(p => null != p.LineDests && p.LineDests.Any(l => l.Id == destId.Value))
                            .ToList();
                }

                ViewData["ProductLines"] = productLines;
                ViewData["CustomerId"] = customerId;

                ViewData["CustomerBase"] = CustomerBase.FindById(int.Parse(customerId));

                ViewData["SuitableTypeId"] = suitableTypeId ?? 0;
                ViewData["ProductCategoryId"] = productCategoryId ?? 0;
                ViewData["DestId"] = destId ?? 0;

                ViewData["ProductCategories"] = DictionaryBase.FindByList(ConstValue.Catalog.ProductCategoryId, true);
                ViewData["DestinationBases"] =
                    DestinationBase.FindByList(
                        destinationTypeIds: ConstValue.Catalog.DestinationType.Continent.ToString());
            }
            catch (Exception ex)
            {
                LogManager.GetLogger().Error(ex);
            }
            return View();
        }

        public ActionResult DownLoadPosters(int id)
        {
            try
            {
                ViewData["CustomerName"] = CustomerName();
                ViewData["WeixinUser"] = WeiXinUser();
                var customerId = CustomerId().ToString();
                LogManager.GetLogger().Info("customerId:" + customerId);

                var productLine = ProductLine.FindById(id);

                ViewData["ProductLine"] = productLine;

                var imgUrl1 = (!string.IsNullOrEmpty(productLine.WeChatAdUrl)
                    ? (ConstValue.SysConfig.BossImgUrl + productLine.WeChatAdUrl.Split(',')[0])
                    : "");
                if (!string.IsNullOrEmpty(imgUrl1))
                {
                    System.Net.WebClient myWebClient = new System.Net.WebClient();
                    //将头像保存到服务器
                    var virPath = "/Uploads/temp/";
                    var fileName = Guid.NewGuid().ToString("N") + ".png";
                    myWebClient.DownloadFile(imgUrl1, System.Web.HttpContext.Current.Request.PhysicalApplicationPath + virPath + fileName);
                    ViewData["ImgUrl1"] = virPath + fileName;
                }
                else
                {
                    ViewData["ImgUrl1"] = "";
                }

                var url = ConstValue.SysConfig.ImgUrl + "/Home/Product/Index?lineId="+ id + "&uId=" + CustomerId();
                LogManager.GetLogger().Info("DownLoadPosters-Url:" + url);

                #region 生成二维码

                var content = url;
                var size =
                    Convert.ToInt32(string.IsNullOrEmpty(Request.QueryString["size"])
                        ? "200"
                        : Request.QueryString["size"]);
                var border =
                    Convert.ToInt32(string.IsNullOrEmpty(Request.QueryString["border"])
                        ? "10"
                        : Request.QueryString["border"]);

                var image = QRCode.CreateQRCode(content,
                    QRCodeEncoder.ENCODE_MODE.BYTE,
                    QRCodeEncoder.ERROR_CORRECTION.M,
                    0,
                    5,
                    size,
                    border);
                //将图片输出到页面
                var ms = new System.IO.MemoryStream();
                image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);

                var imgUrl = "";
                var strPath = Server.MapPath("~/Uploads");
                //保存图片到目录  
                if (Directory.Exists(strPath))
                {
                    //文件名称  
                    var guid = Guid.NewGuid().ToString().Replace("-", "") + ".png";
                    image.Save(strPath + "/" + guid, System.Drawing.Imaging.ImageFormat.Png);

                    imgUrl = "/Uploads/" + guid;
                }
                else
                {
                    //当前目录不存在，则创建  
                    Directory.CreateDirectory(strPath);
                }
                ms.Close();
                ms = null;
                image.Dispose();
                image = null;

                ViewData["ImgUrl"] = imgUrl;

                #endregion
            }
            catch (Exception ex)
            {
                LogManager.GetLogger().Error(ex);
            }

            return View();
        }
    }

    public partial class ProductController
    {
        [HttpPost]
        public JsonResult Collect(int lineId, int state)
        {
            var resultInfo = new ResultInfo<object>(null, true, "");
            try
            {
                var customerId = CustomerId();
                var customerLineCollects = CustomerLineCollect.FindByList(lineId: lineId, customerId: customerId,
                    isValid: null);

                LogManager.GetLogger().Error("customerLineCollects:" + customerLineCollects.Count);
                if (state == 1)
                {
                    if (customerLineCollects.Any())
                    {
                        var customerLineCollect = customerLineCollects.First();
                        CustomerLineCollect.SetValid(true, customerLineCollect.Id.ToString());
                    }
                    else
                    {
                        var customerLineCollectEntity = new CustomerLineCollectEntity();
                        customerLineCollectEntity.CustomerId = customerId;
                        customerLineCollectEntity.LineId = lineId;
                        CustomerLineCollect.Save(customerLineCollectEntity);
                    }
                }
                else
                {
                    if (customerLineCollects.Any())
                    {
                        var customerLineCollect = customerLineCollects.First();
                        CustomerLineCollect.SetValid(false, customerLineCollect.Id.ToString());
                    }
                }

            }
            catch (Exception ex)
            {
                LogManager.GetLogger().Error(ex);
            }

            return Json(resultInfo);
        }
    }

}
