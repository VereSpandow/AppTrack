using System;
using System.Linq;
using System.Web.Mvc;
using System.Data.SqlClient;
using System.Data.Entity.Core.Objects;

using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;

using System.Collections.Generic;
using System.Web;
using System.Web.UI.DataVisualization.Charting;
using System.Text;
using System.IO;

using AppTrack.Models;
using AppTrack.SharedModels;
using AppTrack.ViewModels;
using AppTrack.Helpers;

namespace AppTrack.Controllers
{
    [AuthorizeAdminRedirect(Roles = Constants.adminRoles)]
    public class DocumentsController : BaseController
    {
        private DevProvidentIDOCEntities db = new DevProvidentIDOCEntities();
        DataHelpers DataHelper = new DataHelpers();

        private ApplicationUserManager _userManager;
        private ApplicationRoleManager _roleManager;

        public DocumentsController()
        {
        }

        public DocumentsController(ApplicationUserManager userManager, ApplicationRoleManager roleManager)
        {
            UserManager = userManager;
            RoleManager = roleManager;
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        public ApplicationRoleManager RoleManager
        {
            get
            {
                return _roleManager ?? HttpContext.GetOwinContext().Get<ApplicationRoleManager>();
            }
            private set
            {
                _roleManager = value;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        
        
        // GET: Documents
        public ActionResult Index()
        {
            return View();
        }

        //
        // DOCUMENT SECTION
        //
        [HttpGet]
        public ActionResult Documents(int MemberID = 0, int DocumentID = 0, string ActionType = "", string pageLayout = "")
        {
            var DataHelper = new DataHelpers();
            CheckCustomerResult checkCustomerResult = DataHelper.CheckCustomer(MemberID, Constants.memberCustomerType);

            if (checkCustomerResult.ErrorCode != 0)
            {
                TempData["ErrorCode"] = Constants.fatalErrorCode;
                TempData["ErrorMessage"] = "Invalid Member ID supplied to Document List";
                return RedirectToAction("Error", "Admin");
            }

            ViewBag.MemberDisplayName = checkCustomerResult.DisplayName;
            ViewBag.PageLayout = pageLayout;
            var thisDocument = new Document()
            {
                CustID = MemberID
            };

            if (DocumentID > 0)
            {
                // Check Document
                thisDocument = db.Database.SqlQuery<Document>("exec dbo.[LB_GetDocumentByID] @DocumentID",
                new SqlParameter("@DocumentID", DocumentID)
                ).First();

                if (thisDocument == null)
                {
                    // document not found so error
                    ModelState.AddModelError("", "Document was not found and could not be retrieved");
                }
                else
                {
                    // Check to see if Action is Delete
                    if (ActionType == "D")
                    {
                        // Try to delete the record in C_Documents first
                        ObjectParameter returnID = new ObjectParameter("returnID", typeof(int));
                        ObjectParameter returnMessage = new ObjectParameter("returnMessage", typeof(string));

                        db.LB_DeleteDocumentByVendor(MemberID, DocumentID, AdminID, returnID, returnMessage);

                        var scalarID = (int)returnID.Value;
                        var errorMessage = (string)returnMessage.Value ?? "";

                        if (scalarID == -1)
                        {
                            ModelState.AddModelError("", errorMessage);
                        }
                        else
                        {
                            // Get the filename and try to delete it

                            string folderName = Server.MapPath(Constants.DocumentFolderPath) + MemberID.ToString();
                            //                        folderName = Constants.DocumentFolderPath;


                            //                        string fullPath = Request.MapPath(folderName + vendorDocument.FileName);
                            string fullPath = Server.MapPath(Constants.DocumentFolderPath + MemberID.ToString()) + "\\" + thisDocument.FileName;

                            if (System.IO.File.Exists(fullPath))
                            {
                                try
                                {
                                    System.IO.File.Delete(fullPath);
                                }
                                catch
                                {
                                    ModelState.AddModelError("", "The Document file could not be deleted on the server");
                                }
                            }
                        }
                        // Reset the  Document object to null
                        thisDocument = new Document();
                    }
                }
            }

            // Initalize the View Model

            var model = new DocumentsViewModel();

            model.document = thisDocument;

            var documentList = db.Database.SqlQuery<Document>("exec dbo.[LB_GetVendorDocuments] @CustID",
            new SqlParameter("@CustID", MemberID)
            ).ToList();

            model.DocumentList = documentList;

            if (pageLayout == "I")
            {
                ViewBag.NextAction = "";
                return View("Documents", "~/Views/Shared/_IFrameLayout.cshtml", model);
            }
            else
            {
                ViewBag.NextAction = "Requirements";
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Documents(HttpPostedFileBase documentFile, [Bind(Include = "document")]DocumentsViewModel model, string pageLayout = "")
        {
            ModelState.Remove("model.TemplateID");

            var DataHelper = new DataHelpers();

            int MemberID = new int { };
            int DocumentID = new int { };

            ViewBag.MemberDisplayName = "";
            ViewBag.PageLayout = pageLayout;

            MemberID = model.document.CustID;

            CheckCustomerResult checkCustomerResult = DataHelper.CheckCustomer(MemberID, Constants.memberCustomerType);

            if (checkCustomerResult.ErrorCode != 0)
            {
                TempData["ErrorCode"] = Constants.fatalErrorCode;
                TempData["ErrorMessage"] = "Invalid Member  ID supplied to Document List";
                return RedirectToAction("Error", "Admin");
            }
            else
            {
                ViewBag.MemberDisplayName = checkCustomerResult.DisplayName;
            }

            if (ModelState.IsValid)
            {
                DocumentID = model.document.DocumentID;

                if (DocumentID == 0)
                {
                    if (documentFile != null && documentFile.ContentLength > 0)
                    {
                        string folderName = Server.MapPath(Constants.DocumentFolderPath) + MemberID.ToString();
                        if (!System.IO.Directory.Exists(folderName))
                        {
                            System.IO.Directory.CreateDirectory(folderName);
                        }
                        String fileExtension = Path.GetExtension(documentFile.FileName).ToLower();
                        String allowedExtensionsStr = ".doc.docx.pdf.xls.xlsx";
                        int findExt = allowedExtensionsStr.IndexOf(fileExtension);
                        if (findExt >= 0)
                        {
                            string fullPath = Path.Combine(folderName, Path.GetFileName(documentFile.FileName));

                            if (System.IO.File.Exists(fullPath))
                            {
                                ModelState.AddModelError("", "A file with this name already exists on the server for this Vendor");
                            }
                            else
                            {
                                try
                                {
                                    documentFile.SaveAs(fullPath);
                                }
                                catch (Exception ex)
                                {
                                    ModelState.AddModelError("", "ERROR:" + ex.Message.ToString());
                                }
                            }
                        }
                        else
                        {
                            ModelState.AddModelError("", "The file type was not valid. Valid file types are .doc, .docx and .pdf");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", "You have not specified a file.");
                    }

                    if (ModelState.IsValid)
                    {

                        ObjectParameter returnID = new ObjectParameter("returnID", typeof(int));
                        ObjectParameter returnMessage = new ObjectParameter("returnMessage", typeof(string));

                        string documentPath = Constants.DocumentFolderPath + MemberID.ToString();

                        db.LB_InsertDocument(MemberID, 0, "", 0,
                             Path.GetFileName(documentFile.FileName),
                            documentPath,
                            model.document.TemplateID,
                            "Member", "",
                            model.document.DocumentName,
                            model.document.DocumentDescription,
                            model.document.HideFlag,
                            AdminID,
                            returnID, returnMessage);

                        var scalarDocumentID = (int)returnID.Value;
                        var errorMessage = (string)returnMessage.Value ?? "";

                        if (scalarDocumentID == -1)
                        {
                            ModelState.AddModelError("", errorMessage);
                        }
                    }
                }
                else
                {
                    //
                    //
                    // Need to decide whether we want to allow edit to upload a new version of the file or not
                    //
                    //

                    // Update Program
                    ObjectParameter returnID = new ObjectParameter("returnID", typeof(int));
                    ObjectParameter returnMessage = new ObjectParameter("returnMessage", typeof(string));

                    db.LB_UpdateDocument(MemberID, DocumentID, "", 0,
                        model.document.TemplateID,
                        "Member", "",
                        model.document.DocumentName,
                        model.document.DocumentDescription,
                        model.document.HideFlag,
                        AdminID,
                        returnID, returnMessage);

                    var scalarDocumentID = (int)returnID.Value;
                    var errorMessage = (string)returnMessage.Value ?? "";

                    if (scalarDocumentID == -1)
                    {
                        ModelState.AddModelError("", errorMessage);
                    }
                }
            }

            var thisDocumentList = db.Database.SqlQuery<Document>("exec dbo.[LB_GetDocumentsByCustID] @CustID",
            new SqlParameter("@CustID", MemberID)
            ).ToList();

            model.DocumentList = thisDocumentList;

            if (ModelState.IsValid)
            {
                var emptyDocument = new Document() { CustID = MemberID };
                model.document = emptyDocument;
                ModelState.Clear();
                ViewBag.ErrorCode = 0;
            }
            else
            {
                ViewBag.ErrorCode = 1;
            }

            if (pageLayout == "I")
            {
                ViewBag.NextAction = "";
                return View("Documents", "~/Views/Shared/_IFrameLayout.cshtml", model);
            }
            else
            {
                ViewBag.NextAction = "Requirements";
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DocuSign([Bind(Include = "document")]DocumentsViewModel model, string pageLayout = "")
        {
            var DataHelper = new DataHelpers();

            int CustID = new int { };
            int DocumentID = new int { };

            ViewBag.MemberDisplayName = "";
            ViewBag.PageLayout = pageLayout;

            int MemberID = model.document.CustID;

            CheckCustomerResult checkCustomerResult = DataHelper.CheckCustomer(MemberID, Constants.memberCustomerType);

            if (checkCustomerResult.ErrorCode != 0)
            {
                TempData["ErrorCode"] = Constants.fatalErrorCode;
                TempData["ErrorMessage"] = "Invalid MemberID supplied to Document List";
                return RedirectToAction("Error", "Admin");
            }
            else
            {
                ViewBag.MemberDisplayName = checkCustomerResult.DisplayName;
            }

            if (ModelState.IsValid)
            {
                DocumentID = model.document.DocumentID;

                if (DocumentID == 0)
                {
                    ObjectParameter returnID = new ObjectParameter("returnID", typeof(int));
                    ObjectParameter returnMessage = new ObjectParameter("returnMessage", typeof(string));

                    string documentPath = Constants.DocumentFolderPath + MemberID.ToString();

                    db.LB_InsertDocument(MemberID, 0, "", 0,
                        "",
                        "",
                        model.document.TemplateID,
                        "Member", "",
                        model.document.DocumentName,
                        model.document.DocumentDescription,
                        model.document.HideFlag,
                        AdminID,
                        returnID, returnMessage);

                    var scalarDocumentID = (int)returnID.Value;
                    var errorMessage = (string)returnMessage.Value ?? "";

                    if (scalarDocumentID == -1)
                    {
                        ModelState.AddModelError("", errorMessage);
                    }
                }
                else
                {
                    // Update Document
                    ObjectParameter returnID = new ObjectParameter("returnID", typeof(int));
                    ObjectParameter returnMessage = new ObjectParameter("returnMessage", typeof(string));

                    db.LB_UpdateDocument(MemberID, DocumentID, "", 0,
                        model.document.TemplateID,
                        "Member", "",
                        model.document.DocumentName,
                        model.document.DocumentDescription,
                        model.document.HideFlag,
                        AdminID,
                        returnID, returnMessage);

                    var scalarDocumentID = (int)returnID.Value;
                    var errorMessage = (string)returnMessage.Value ?? "";

                    if (scalarDocumentID == -1)
                    {
                        ModelState.AddModelError("", errorMessage);
                    }
                }
            }

            var thisDocumentList = db.Database.SqlQuery<Document>("exec dbo.[LB_GetDocumentsByCustID] @CustID",
            new SqlParameter("@CustID", MemberID)
            ).ToList();

            model.DocumentList = thisDocumentList;

            if (ModelState.IsValid)
            {
                var emptyDocument = new Document() { CustID = MemberID };
                model.document = emptyDocument;
                ModelState.Clear();
                ViewBag.ErrorCode = 0;
            }
            else
            {
                ViewBag.ErrorCode = 1;
            }

            if (pageLayout == "I")
            {
                ViewBag.NextAction = "";
                return View("Documents", "~/Views/Shared/_IFrameLayout.cshtml", model);
            }
            else
            {
                ViewBag.NextAction = "Requirements";
                return View("Documents", model);
            }
        }

    }
}