using System.IO;
using System.Web.Mvc;
using YiYouLun.Weixin.MP;

namespace Drp.WeiXinWeb.Controllers
{
    /// <summary>
    /// 这个Controller专为Choiskycn.Weixin.MP.Test/HttpUtility/RequestUtilityTest.cs/HttpPostTest 提供上传测试目标
    /// </summary>
    public class TestUploadMediaFileController : Controller
    {
        [HttpPost]
        public ActionResult Index(string token, UploadMediaFileType type, int contentLength /*, HttpPostedFileBase postedFile*/)
        {
            var inputStream = Request.InputStream;
            if (contentLength != inputStream.Length)
            {
                return Content("ContentLength不正确，可能接收错误！");
            }

            if (token!="TOKEN")
            {
                return Content("TOKEN不正确！");
            }

            if (type!= UploadMediaFileType.image)
            {
                return Content("UploadMediaFileType不正确！");
            }

            //储存文件，对比是否上传成功
            using (FileStream ms =new FileStream(Server.MapPath("~/TestUploadMediaFile.jpg"), FileMode.OpenOrCreate))
            {
                inputStream.CopyTo(ms,256);
            }

            return Content("{\"type\":\"image\",\"media_id\":\"MEDIA_ID\",\"created_at\":123456789}");
        }
    }
}