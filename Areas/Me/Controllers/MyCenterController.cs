using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Drp.Common;
using Drp.Entity.Customer;
using Drp.Model;
using Drp.Model.Customer;
using Drp.Model.Order;
using Drp.Model.Sys;
using Drp.WeiXinWeb.Controllers;
using M2SA.AppGenome.Logging;
using System.Transactions;
using Drp.Model.Card;
using Drp.Model.WeiXin;
using YiYouLun.Weixin.MP.AdvancedAPIs;

//using System.Text.RegularExpressions;

namespace Drp.WeiXinWeb.Areas.Me.Controllers
{
    //如果将此特性加在Controller上，那么访问这个Controller里面的方法都需要验证用户登录状态
    public partial class MyCenterController : BaseController
    {
        public ActionResult Index()
        {
            try
            {
                var customerId = CustomerId();
                if (customerId == 0)
                {
                    return Redirect("/Me/JoinUs/Index");
                }
                ViewData["CustomerName"] = CustomerName();
                ViewData["WeixinUser"] = WeiXinUser();
                LogManager.GetLogger().Info("customerId:" + customerId);

                ViewData["CustoemrBase"] = CustomerBase.FindById(customerId);

                var customerCards = CustomerCard.FindByList(customerId: customerId);
                ViewData["CustomerCards"] = customerCards;

                var orderBases = OrderBaseInfo.FindByList(customerId: customerId);
                ViewData["OrderBases"] = orderBases;

                ViewData["CustomerBases"] = CustomerBase.FindByList(parentId: customerId);
            }
            catch (Exception ex)
            {
                LogManager.GetLogger().Error(ex);
            }
            return View();
        }

        public ActionResult PersonalInformation(int customerId)
        {
            try
            {
                ViewData["CustomerBase"] = CustomerBase.FindById(customerId);
                ViewData["Industries"] = DictionaryBase.FindByList(ConstValue.Catalog.IndustryId, true);
            }
            catch (Exception ex)
            {
                LogManager.GetLogger().Error(ex);
            }
            return View();
        }

        public ActionResult PersonalInformation1(int customerId)
        {
            try
            {
                ViewData["CustomerBase"] = CustomerBase.FindById(customerId);
            }
            catch (Exception ex)
            {
                LogManager.GetLogger().Error(ex);
            }
            return View();
        }

        public ActionResult WithDrawalSuccess()
        {
            try
            {

            }
            catch (Exception ex)
            {
                LogManager.GetLogger().Error(ex);
            }

            return View();

        }

        /// <summary>
        /// 我的提成
        /// </summary>
        /// <returns></returns>
        public ActionResult SalesCommissions()
        {
            try
            {
                var customerId = CustomerId();
                ViewData["CustomerName"] = CustomerName();
                ViewData["WeixinUser"] = WeiXinUser();

                ViewData["CustomerBase"] = CustomerBase.FindById(customerId);

                var customerBases = CustomerBase.FindByList(parentId: customerId);

                var orderBases = OrderBaseInfo.FindByList();
                orderBases = orderBases.Where(p => customerBases.Any(c => c.Id == p.CustomerId)).ToList();

                ViewData["OrderBases"] = orderBases;

                ViewData["CustomerOrderCommissions"] = CustomerOrderCommission.FindByList(customerId: customerId);

                ViewData["CustomerWithdrawalRecords"] = CustomerWithdrawalRecord.FindByList(customerId: customerId);
            }
            catch (Exception ex)
            {
                LogManager.GetLogger().Error(ex);
            }
            return View();
        }
        
        /// <summary>
        /// 会员卡
        /// </summary>
        /// <returns></returns>
        public ActionResult MemberCard()
        {
            try
            {
                var customerId = CustomerId();
                ViewData["CustomerName"] = CustomerName();
                ViewData["WeixinUser"] = WeiXinUser();

                ViewData["CustomerCards"] = CustomerCard.FindByList(customerId: customerId);
                ViewData["CardEquities"] = CardEquity.FindByList();
            }
            catch (Exception ex)
            {
                LogManager.GetLogger().Error(ex);
            }
            return View();
        }

        /// <summary>
        /// 销售排名
        /// </summary>
        /// <returns></returns>
        public ActionResult SalesRanking()
        {
            try
            {
                var customerId = CustomerId();
                ViewData["CustomerName"] = CustomerName();
                ViewData["WeixinUser"] = WeiXinUser();
                var orderBases = OrderBaseInfo.FindByList(bookStatusId: 2);
                ViewData["OrderBases"] = orderBases;
                var orderCityDic = new Dictionary<string, decimal>();
                var orderCustomerDic = new Dictionary<int, decimal>();
                if (null != orderBases && orderBases.Any())
                {
                    foreach (var orderBase in orderBases)
                    {
                        if (orderCityDic.ContainsKey(orderBase.CityName))
                        {
                            orderCityDic[orderBase.CityName] = orderCityDic[orderBase.CityName] + orderBase.TotalAmount;
                        }
                        else
                        {
                            orderCityDic.Add(orderBase.CityName, orderBase.TotalAmount);
                        }

                        if (orderCustomerDic.ContainsKey(orderBase.CustomerId))
                        {
                            orderCustomerDic[orderBase.CustomerId] = orderCustomerDic[orderBase.CustomerId] +
                                                                     orderBase.TotalAmount;
                        }
                        else
                        {
                            orderCustomerDic.Add(orderBase.CustomerId, orderBase.TotalAmount);
                        }
                    }
                }

                var dicSort = from objDic in orderCityDic orderby objDic.Value descending select objDic;
                ViewData["OrderCiteDic"] = dicSort;

                var orderCustomerDicSort = from objDic in orderCustomerDic orderby objDic.Value descending select objDic;
                ViewData["OrderCustomerDic"] = orderCustomerDicSort;
            }
            catch (Exception ex)
            {
                LogManager.GetLogger().Error(ex);
            }
            return View();
        }

        /// <summary>
        /// 我的粉丝
        /// </summary>
        /// <returns></returns>
        public ActionResult Fans()
        {
            try
            {
                var customerId = CustomerId();
                LogManager.GetLogger().Info("customerId:" + customerId);

                ViewData["CustomerId"] = customerId;
                ViewData["CustomerName"] = CustomerName();
                ViewData["WeixinUser"] = WeiXinUser();

                ViewData["WeiXinUsers"] = WeixinUser.FindByList();
                var customerBases = CustomerBase.FindByList();
                var fanCount = customerBases.Count(p => p.ParentId == customerId);
                ViewData["FanCount"] = fanCount;

                ViewData["CustomerBases"] = customerBases;
                var customerFanDic = new Dictionary<int, int>();
                if (null != customerBases && customerBases.Any())
                {
                    foreach (var customerBase in customerBases)
                    {
                        customerFanDic[customerBase.Id] = customerBases.Count(p => p.ParentId == customerBase.Id);
                    }
                }
                var dicSort = from objDic in customerFanDic orderby objDic.Value descending select objDic;
                ViewData["CustomerFanDic"] = dicSort;

                var index = 1;
                foreach (KeyValuePair<int, int> kvp in dicSort)
                {
                    if (kvp.Key== customerId)
                    {
                        break;
                    }
                    index++;
                }
                ViewData["Ranking"] = index;
            }
            catch (Exception ex)
            {
                LogManager.GetLogger().Error(ex);
            }
            return View();
        }

        /// <summary>
        /// 银行账号
        /// </summary>
        /// <returns></returns>
        public ActionResult BankAccount()
        {
            try
            {
                var customerId = CustomerId();
                ViewData["CustomerName"] = CustomerName();
                ViewData["WeixinUser"] = WeiXinUser();
                ViewData["CustomerBankAccounts"] = CustomerBankAccount.FindByList(customerId: customerId);

                ViewData["AccountTypes"] = DictionaryBase.FindByList(ancestorId: ConstValue.Catalog.AccountTypeId,
                    isParent: true);
                ViewData["Banks"] = DictionaryBase.FindByList(ancestorId: ConstValue.Catalog.BankId,
                    isParent: true);
            }
            catch (Exception ex)
            {
                LogManager.GetLogger().Error(ex);
            }
            return View();
        }

        /// <summary>
        /// 我的收藏
        /// </summary>
        /// <returns></returns>
        public ActionResult Collection()
        {
            try
            {
                var customerId = CustomerId();
                ViewData["CustomerName"] = CustomerName();
                ViewData["WeixinUser"] = WeiXinUser();
                ViewData["CustomerLineCollects"] = CustomerLineCollect.FindByList(customerId: customerId);
            }
            catch (Exception ex)
            {
                LogManager.GetLogger().Error(ex);
            }
            return View();
        }

        public ActionResult Withdrawal()
        {
            try
            {
                var customerId = CustomerId();
                ViewData["CustomerName"] = CustomerName();
                ViewData["WeixinUser"] = WeiXinUser();
                ViewData["CustomerBase"] = CustomerBase.FindById(customerId);
            }
            catch (Exception ex)
            {
                LogManager.GetLogger().Error(ex);
            }
            return View();
        }

    }

    public partial class MyCenterController
    {
        [HttpPost]
        public JsonResult PersonInfoSubmit(FormCollection values)
        {
            var resultInfo = new ResultInfo<object>(null, true, "");
            try
            {
                var customerId = int.Parse(values["CustomerId"]);
                if (null == Request.Files || Request.Files.Count < 3)
                {
                    resultInfo.IsSuccess = false;
                    resultInfo.Message = "请上传相关证件!";
                    return Json(resultInfo);
                }
                var idCardImgUrl1 = "";
                var idCardImgUrl2 = "";
                var myPhotoUrl = "";
                #region

                var pathForSaving = Server.MapPath("~/Uploads");
                //if (FileHelper.CreateFolderIfNeeded(pathForSaving))
                //{
                //    foreach (string file in Request.Files)
                //    {
                //        HttpPostedFileBase uploadFile = Request.Files[file] as HttpPostedFileBase;
                //        if (uploadFile != null && uploadFile.ContentLength > 0)
                //        {
                //            var path = Path.Combine(pathForSaving, uploadFile.FileName);
                //            uploadFile.SaveAs(path);
                //            idCardImgUrl1 += "/Uploads/" + uploadFile.FileName + "^";
                //        }
                //    }
                //    idCardImgUrl1 = !string.IsNullOrEmpty(idCardImgUrl1) && idCardImgUrl1.EndsWith("^")
                //        ? idCardImgUrl1.TrimEnd('^')
                //        : idCardImgUrl1;
                //}

                HttpPostedFileBase uploadFile = Request.Files["uploadInput1"] as HttpPostedFileBase;
                if (uploadFile != null && uploadFile.ContentLength > 0)
                {
                    var aLastName = uploadFile.FileName.Substring(uploadFile.FileName.LastIndexOf(".", StringComparison.Ordinal) + 1, (uploadFile.FileName.Length - uploadFile.FileName.LastIndexOf(".", StringComparison.Ordinal) - 1));
                    var v = Guid.NewGuid().ToString("N") + "." + aLastName;
                    //var path = Path.Combine(pathForSaving, uploadFile.FileName);
                    var path = Path.Combine(pathForSaving, v);
                    uploadFile.SaveAs(path);

                    idCardImgUrl1 += "/Uploads/" + v;
                }

                uploadFile = Request.Files["uploadInput2"] as HttpPostedFileBase;
                if (uploadFile != null && uploadFile.ContentLength > 0)
                {
                    var aLastName = uploadFile.FileName.Substring(uploadFile.FileName.LastIndexOf(".", StringComparison.Ordinal) + 1, (uploadFile.FileName.Length - uploadFile.FileName.LastIndexOf(".", StringComparison.Ordinal) - 1));
                    var v = Guid.NewGuid().ToString("N") + "." + aLastName;
                    //var path = Path.Combine(pathForSaving, uploadFile.FileName);
                    var path = Path.Combine(pathForSaving, v);
                    uploadFile.SaveAs(path);
                    idCardImgUrl2 += "/Uploads/" + v;
                }
                uploadFile = Request.Files["uploadInput3"] as HttpPostedFileBase;
                if (uploadFile != null && uploadFile.ContentLength > 0)
                {
                    var aLastName = uploadFile.FileName.Substring(uploadFile.FileName.LastIndexOf(".", StringComparison.Ordinal) + 1, (uploadFile.FileName.Length - uploadFile.FileName.LastIndexOf(".", StringComparison.Ordinal) - 1));
                    var v = Guid.NewGuid().ToString("N") + "." + aLastName;
                    //var path = Path.Combine(pathForSaving, uploadFile.FileName);
                    var path = Path.Combine(pathForSaving, v);
                    uploadFile.SaveAs(path);
                    myPhotoUrl += "/Uploads/" + v;
                }

                #endregion

                var customerBase = CustomerBase.FindById(customerId);
                if (!string.IsNullOrEmpty(idCardImgUrl1))
                {
                    customerBase.IdCardImgUrl1 = idCardImgUrl1;
                }
                if (!string.IsNullOrEmpty(idCardImgUrl2))
                {
                    customerBase.IdCardImgUrl2 = idCardImgUrl2;
                }
                if (!string.IsNullOrEmpty(myPhotoUrl))
                {
                    customerBase.MyPhotoUrl = myPhotoUrl;
                }

                LogManager.GetLogger().Error("selectIndustry:" + values["selectIndustry"]);
                customerBase.IndustryId = int.Parse(values["selectIndustry"]);
                CustomerBase.Save(customerBase);
            }
            catch (Exception ex)
            {
                LogManager.GetLogger().Error(ex);
            }

            return Json(resultInfo);
        }

        [HttpPost]
        public JsonResult WithdrawalSubmit(FormCollection values)
        {
            var resultInfo = new ResultInfo<object>(null, true, "");
            try
            {
                LogManager.GetLogger().Error("Amount:" + values["Amount"]);
                var customerId = CustomerId();
                var customerBase = CustomerBase.FindById(customerId);
                var amount = decimal.Parse(values["Amount"]);
                var maxAmount = customerBase.Commission - customerBase.CashWithDrawalAmount;
                if (amount> maxAmount)
                {
                    resultInfo.IsSuccess = false;
                    resultInfo.Message = "提现金额不能超过" + maxAmount + "！";
                    return Json(resultInfo);
                }
               
                using (var ts = new TransactionScope())
                {
                    var customerWithdrawalRecordEntity = new CustomerWithdrawalRecordEntity();
                    customerWithdrawalRecordEntity.Amount = amount;
                    customerWithdrawalRecordEntity.CustomerId = customerId;
                    customerWithdrawalRecordEntity.Status = 0;
                    customerWithdrawalRecordEntity.Reason = string.Empty;
                    CustomerWithdrawalRecord.Save(customerWithdrawalRecordEntity);

                    customerBase.CashWithDrawalAmount += amount;
                    CustomerBase.Save(customerBase);

                    ts.Complete();
                }

                #region 给客服发消息，通知处理
                var data = new
                {
                    //使用TemplateDataItem简单创建数据。
                    first = new TemplateDataItem("【" + customerBase.Name + "】正在申请提现！"),
                    keyword1 = new TemplateDataItem(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
                    keyword2 = new TemplateDataItem(""),
                    keyword3 = new TemplateDataItem("￥" + amount.ToString()),
                    keyword4 = new TemplateDataItem(""),
                    keyword5 = new TemplateDataItem(""),
                    remark = new TemplateDataItem("请及时处理", "#173177"),
                };
                if (!string.IsNullOrEmpty(ConstValue.WeiXinConfig.CustomerServiceOpenIds))
                {
                    foreach (var openId in ConstValue.WeiXinConfig.CustomerServiceOpenIds.Split(','))
                    {
                        if (!string.IsNullOrEmpty(openId))
                        {
                            SendWeiXinTempMsg(openId, ConstValue.WeiXinConfig.Template.WithdrawalInitiationNotificationId, "", data);
                        }
                    }
                }

                #endregion
            }
            catch (Exception ex)
            {
                LogManager.GetLogger().Error(ex);
            }

            return Json(resultInfo);
        }

        [HttpPost]
        public JsonResult BankAccountSubmit(int accountTypeId, string accountUserName,int bankId,string accountNumber)
        {
            var resultInfo = new ResultInfo<object>(null, true, "");
            try
            {
                var customerId = CustomerId();

                var customerBankAccounts = CustomerBankAccount.FindByList(customerId: customerId);
                CustomerBankAccountEntity customerBankAccountEntity = new CustomerBankAccountEntity();
                if (null != customerBankAccounts && customerBankAccounts.Any())
                {
                    customerBankAccountEntity = customerBankAccounts.First();
                }
                customerBankAccountEntity.AccountTypeId = accountTypeId;
                customerBankAccountEntity.AccountUserName = accountUserName;
                customerBankAccountEntity.BankId = bankId;
                customerBankAccountEntity.AccountNumber = accountNumber;
                customerBankAccountEntity.CustomerId = customerId;
                CustomerBankAccount.Save(customerBankAccountEntity);
            }
            catch (Exception ex)
            {
                LogManager.GetLogger().Error(ex);
            }

            return Json(resultInfo);
        }

    }

}
