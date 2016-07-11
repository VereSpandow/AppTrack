using System;
using System.IO;
using System.Web;
using System.Web.Mvc;
using AppTrack.Helpers;

namespace AppTrack.Controllers
{
    
    [AuthorizeAdminRedirect(Roles=Constants.adminRoles)]
    public class AdminController : BaseController
    {

        [HttpPost]
        public ActionResult Index(HttpPostedFileBase file)
        {
            var thisMessage = "Invalid File Type ";
            if (file != null && file.ContentLength > 0)
            {
                String fileExtension = Path.GetExtension(file.FileName).ToLower();
                String allowedExtensionsStr = ".gif.png.jpeg.jpg.csv.txt.xlsx.xls.zip";
                int findExt = allowedExtensionsStr.IndexOf(fileExtension);
                if (findExt >= 0)
                {
                    string folderName = Server.MapPath("~/App_Data/AdminUploads");
                    if (!System.IO.Directory.Exists(folderName))
                    {
                        System.IO.Directory.CreateDirectory(folderName);
                    }
                    try
                    {
                        string path = Path.Combine(Server.MapPath("~/App_Data/AdminUploads"), Path.GetFileName(file.FileName));
                        file.SaveAs(path);
                        thisMessage = "File uploaded successfully ";
                    }
                    catch (Exception ex)
                    {
                        thisMessage = "ERROR:" + ex.Message.ToString();
                    }
                }
            }
            else
            {
                thisMessage = "You have not specified a file.";
            }
            ViewBag.Message = thisMessage;
            return View();
            //return RedirectToAction("Index");            
        }

        [AllowAnonymous]
        public ActionResult Error(int id = 0)
        {
            if (id == 99)
            {
                ViewBag.ErrorMessage = "You are not authorized to access this function.";
            }
            else
            {
                if (TempData["ErrorMessage"] != null)
                {
                    ViewBag.ErrorMessage = TempData["ErrorMessage"];
                    TempData["ErrorMessage"] = "";
                }

            }
            return View();
        }
    }
}