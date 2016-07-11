using AppTrack.Helpers;
using AppTrack.Models;
using AppTrack.SharedModels;
using AppTrack.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AppTrack.Controllers
{

    [AuthorizeAdminRedirect(Roles = "ContractAdmin, ContractUser")]
    public class ContractController : BaseController
    {
        private DevProvidentIDOCEntities db = new DevProvidentIDOCEntities();

        [HttpGet]
        public ActionResult Index(string Company = "")
        {
            string searchCompany = Company;
            string selectedStatus = "Active";
            DateTime startExpDate = DateTime.Now.AddDays(-90);
            DateTime endExpDate = DateTime.Now.AddDays(90);

            var contractRows = db.Database.SqlQuery<Contract>("exec dbo.[LB_GetContractList]  @Company, @Status, @StartExpDate, @EndExpDate",
             new SqlParameter("@Company", searchCompany),
             new SqlParameter("@Status", selectedStatus),
             new SqlParameter("@StartExpDate", startExpDate),
             new SqlParameter("@EndExpDate", endExpDate)
             ).ToList();

            var DataHelper = new DataHelpers();

            IEnumerable<System.Web.Mvc.SelectListItem> thisList = DataHelper.GetStatusSelectList(Constants.contractStatusLookupGroupID, true);

            var contractListViewModel = new ContractListViewModel
            {
                ContractList = contractRows,
                SearchCompany = searchCompany,
                SelectedStatus = selectedStatus,
                StatusList = thisList,
                SearchStartDate = startExpDate,
                SearchEndDate = endExpDate
            };

            return View(contractListViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index([Bind(Include = "SearchCompany, SelectedStatus, SearchStartDate, SearchEndDate")] ContractListViewModel contractListViewModel)
        {
            string searchCompany = contractListViewModel.SearchCompany ?? "";
            string selectedStatus = contractListViewModel.SelectedStatus ?? "Active";

            DateTime now = DateTime.Now;
            if (contractListViewModel.SearchStartDate > contractListViewModel.SearchEndDate)
            {
                ModelState.AddModelError("", "Start Date cannot be after End Date");
                // Return below using empty model
                contractListViewModel.ContractList = new List<Contract>();
            }
            else 
            {
                var contractRows = db.Database.SqlQuery<Contract>("exec dbo.[LB_GetContractList]  @Company, @Status, @StartExpDate, @EndExpDate",
                 new SqlParameter("@Company", searchCompany),
                 new SqlParameter("@Status", selectedStatus),
                 new SqlParameter("@StartExpDate", contractListViewModel.SearchStartDate),
                 new SqlParameter("@EndExpDate", contractListViewModel.SearchEndDate)
                 ).ToList();

                contractListViewModel.ContractList = contractRows;
            }

            var DataHelper = new DataHelpers();

            IEnumerable<System.Web.Mvc.SelectListItem> thisList = DataHelper.GetStatusSelectList(Constants.contractStatusLookupGroupID, true);

            contractListViewModel.StatusList = thisList;

            return View(contractListViewModel);
        }

        public ActionResult ContractProfile(int id = 0)
        {
            int ContractID = id;
            ViewBag.ContractID = ContractID;

                ViewBag.errorCode = 0;

                var contractProfileViewModel = new ContractProfileViewModel();

                // Contract Record
                contractProfileViewModel.ContractRecord = db.Database.SqlQuery<Contract>("exec dbo.[LB_GetContractByID] @ContractID",
                new SqlParameter("@ContractID", ContractID)
                ).First();

                // Program Summary List
                contractProfileViewModel.ContractDetailList = db.Database.SqlQuery<ContractDetail>("exec dbo.[LB_GetContractDetailByContractID] @ContractID",
                new SqlParameter("@ContractID", ContractID)
                ).ToList();

                return View(contractProfileViewModel);
        }

        [HttpGet]
        public ActionResult UpdateContract(int ContractID = 0, string pageLayout = "")
        {
            ViewBag.ContractID = ContractID;
            ViewBag.PageLayout = pageLayout;

            var contractViewModel = new ContractViewModel();

            if (ContractID != 0)
            {
                contractViewModel = db.Database.SqlQuery<ContractViewModel>("exec dbo.[LB_GetContractByID] @ContractID",
                new SqlParameter("@ContractID", ContractID)
                ).First();

                if (contractViewModel == null)
                {
                    TempData["ErrorCode"] = Constants.fatalErrorCode;
                    TempData["ErrorMessage"] = "Invalid Contract ID supplied";
                    return RedirectToAction("Error", "Admin");
                }
            }

            var DataHelper = new DataHelpers();

            IEnumerable<System.Web.Mvc.SelectListItem> statusList = DataHelper.GetStatusSelectList(Constants.contractStatusLookupGroupID, true, false);
            contractViewModel.StatusList = statusList;

            IEnumerable<System.Web.Mvc.SelectListItem> contractTypeList = DataHelper.GetStatusSelectList(Constants.contractTypeLookupGroupID, true, false);
            contractViewModel.ContractTypeList = contractTypeList;

            IEnumerable<System.Web.Mvc.SelectListItem> providerTypeList = DataHelper.GetStatusSelectList(Constants.providerTypeLookupGroupID, true, false);
            contractViewModel.ProviderTypeList = providerTypeList;

            IEnumerable<System.Web.Mvc.SelectListItem> providerList = DataHelper.GetContractProviderList(true, false);
            contractViewModel.ProviderList = providerList;

            if (pageLayout == "I")
            {
                return View("UpdateContract", "~/Views/Shared/_IFrameLayout.cshtml", contractViewModel);
            }
            else
            {
                return View(contractViewModel);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateContract([Bind(Include = "ID, CustID, ContractType, ContractTitle, ContractDescription, EffectiveDate, ExpirationDate, SignatureDate, ExclusivityFlag, ExclusivityDescription, SpecialTerms, AdminOnly, Status")] ContractViewModel contractViewModel, string pageLayout = "")
        {
            var DataHelper = new DataHelpers();

            int ContractID = contractViewModel.ID;

            ViewBag.PageLayout = pageLayout;

            int scalarID = 0;
            string errorMessage = "";

            // Update Contract

            ObjectParameter returnID = new ObjectParameter("returnID", typeof(int));
            ObjectParameter returnMessage = new ObjectParameter("returnMessage", typeof(string));

            db.LB_UpdateContractHeader(contractViewModel.ID, contractViewModel.CustID, contractViewModel.ContractType, contractViewModel.ContractTitle, contractViewModel.ContractDescription, contractViewModel.EffectiveDate, contractViewModel.ExpirationDate, contractViewModel.SignatureDate, contractViewModel.ExclusivityFlag, contractViewModel.ExclusivityDescription, contractViewModel.SpecialTerms, contractViewModel.AdminOnly, AdminID, returnID, returnMessage);

            scalarID = (int)returnID.Value;
            errorMessage = (string)returnMessage.Value ?? "";

            if (scalarID == -1)
            {
                ModelState.AddModelError("", errorMessage);
            }
            else
            {
                ContractID = scalarID;
            }

            IEnumerable<System.Web.Mvc.SelectListItem> statusList = DataHelper.GetStatusSelectList(Constants.contractStatusLookupGroupID, true, false);
            contractViewModel.StatusList = statusList;

            IEnumerable<System.Web.Mvc.SelectListItem> contractTypeList = DataHelper.GetStatusSelectList(Constants.contractTypeLookupGroupID, true, false);
            contractViewModel.ContractTypeList = contractTypeList;

            IEnumerable<System.Web.Mvc.SelectListItem> providerTypeList = DataHelper.GetStatusSelectList(Constants.providerTypeLookupGroupID, true, false);
            contractViewModel.ProviderTypeList = providerTypeList;

            IEnumerable<System.Web.Mvc.SelectListItem> providerList = DataHelper.GetContractProviderList(true, false);
            contractViewModel.ProviderList = providerList;

            if (ModelState.IsValid)
            {
                if (pageLayout == "I")
                {
                    ModelState.AddModelError("", "Contract updated successfully");
                    return View("UpdateContract", "~/Views/Shared/_IFrameLayout.cshtml", contractViewModel);
                }
                else
                {
                    return RedirectToAction("ContractProfile", new { id = ContractID });
                }
            }
            else
            {
                if (pageLayout == "I")
                {
                    return View("UpdateContract", "~/Views/Shared/_IFrameLayout.cshtml", contractViewModel);
                }
                else
                {
                    return View(contractViewModel);
                }
            }
        }

        //
        // DETAILS SECTION
        //
        [HttpGet]
        public ActionResult ContractDetails(int ContractID = 0)
        {
            ViewBag.ContractID = ContractID;

            return View();
        }

        [HttpGet]
        public ActionResult ContractDetailList(int ContractID = 0, int ContractDetailID = 0, string ActionType = "")
        {
            var DataHelper = new DataHelpers();

            var contractDetail = new ContractDetail()
            {
                ContractID = ContractID,
                ID = ContractDetailID
            };

            if (ContractDetailID > 0)
            {
                // Check to see if Action is Delete
                if (ActionType == "D")
                {
                    db.LB_DeleteContractDetail(ContractDetailID, AdminID);
                }
                else
                {
                    // Check Program
                    contractDetail = db.Database.SqlQuery<ContractDetail>("exec dbo.[LB_GetContractDetailByID] @ContractDetailID",
                    new SqlParameter("@ContractDetailID", ContractDetailID)
                    ).FirstOrDefault();

                    if (contractDetail == null)
                    {
                        // Program not found so error
                        ModelState.AddModelError("", "Contract Detail was not found and could not be retrieved");
                    }
                }
            }

            // Initalize the View Model

            var contractDetailViewModel = new ContractDetailViewModel();

            contractDetailViewModel.ContractDetail = contractDetail;

            var contractDetailList = db.Database.SqlQuery<ContractDetail>("exec dbo.[LB_GetContractDetailByContractID] @ContractID",
            new SqlParameter("@ContractID", ContractID)
            ).ToList();

            contractDetailViewModel.ContractDetailList = contractDetailList;

            IEnumerable<System.Web.Mvc.SelectListItem> contractDetailTypeList = DataHelper.GetStatusSelectList(Constants.contractDetailTypeLookupGroupID, true, false);
            contractDetailViewModel.ContractDetailTypeList = contractDetailTypeList;

            return PartialView("_ContractDetailList", contractDetailViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ContractDetailList([Bind(Include = "ContractDetail")] ContractDetailViewModel contractDetailViewModel)
        {
            var DataHelper = new DataHelpers();

            int ContractID = new int { };
            int ContractDetailID = new int { };

            if (ModelState.IsValid)
            {
                ContractID = contractDetailViewModel.ContractDetail.ContractID;
                ContractDetailID = contractDetailViewModel.ContractDetail.ID;

                if (ContractDetailID == 0)
                {
                    // Add Program 
                    ObjectParameter returnID = new ObjectParameter("returnID", typeof(int));
                    ObjectParameter returnMessage = new ObjectParameter("returnMessage", typeof(string));

                    db.LB_InsertContractDetail(ContractID,
                        contractDetailViewModel.ContractDetail.DetailType ,
                        contractDetailViewModel.ContractDetail.DetailDescription,
                        contractDetailViewModel.ContractDetail.ContractAmount,
                        contractDetailViewModel.ContractDetail.ContractPercent,
                        contractDetailViewModel.ContractDetail.ProjectedAmount,
                        AdminID,
                        returnID, returnMessage);

                    var scalarContractID = (int)returnID.Value;
                    var errorMessage = (string)returnMessage.Value ?? "";

                    if (scalarContractID == -1)
                    {
                        ModelState.AddModelError("", errorMessage);
                    }
                }
                else
                {
                    // Update Program
                    ObjectParameter returnID = new ObjectParameter("returnID", typeof(int));
                    ObjectParameter returnMessage = new ObjectParameter("returnMessage", typeof(string));

                    db.LB_UpdateContractDetail(
                        contractDetailViewModel.ContractDetail.ID, 
                        contractDetailViewModel.ContractDetail.DetailDescription,
                        contractDetailViewModel.ContractDetail.ContractAmount,
                        contractDetailViewModel.ContractDetail.ContractPercent,
                        contractDetailViewModel.ContractDetail.ProjectedAmount,
                        AdminID,
                        returnID, returnMessage);

                    var scalarContractID = (int)returnID.Value;
                    var errorMessage = (string)returnMessage.Value ?? "";

                    if (scalarContractID == -1)
                    {
                        ModelState.AddModelError("", errorMessage);
                    }
                }
            }

            var contractDetailList = db.Database.SqlQuery<ContractDetail>("exec dbo.[LB_GetContractDetailByContractID] @ContractID",
            new SqlParameter("@ContractID", ContractID)
            ).ToList();
            
            contractDetailViewModel.ContractDetailList = contractDetailList;

            if (ModelState.IsValid)
            {
                // Add empty items to View Model and clear out Model State
                // Need to populate the CustID property because the View form needs this
                var emptyContractDetail = new ContractDetail()
                {
                    ContractID = ContractID
                };

                contractDetailViewModel.ContractDetail = emptyContractDetail;
                ModelState.Clear();

                ViewBag.ErrorCode = 0;
            }
            else
            {
                ViewBag.ErrorCode = 1;
            }

            IEnumerable<System.Web.Mvc.SelectListItem> contractDetailTypeList = DataHelper.GetStatusSelectList(Constants.contractDetailTypeLookupGroupID, true, false);
            contractDetailViewModel.ContractDetailTypeList = contractDetailTypeList;

            return PartialView("_ContractDetailList", contractDetailViewModel);
        }

        //
        // DOCUMENT SECTION
        //
        [HttpGet]
        public ActionResult ContractDocuments(int ContractID = 0, int DocumentID = 0, string ActionType = "", string pageLayout = "")
        {
            var DataHelper = new DataHelpers();

            ViewBag.PageLayout = pageLayout;

            var contractDocument = new ContractDocument()
            {
                ContractID = ContractID
            };

            if (DocumentID > 0)
            {
                // Check Document
                contractDocument = db.Database.SqlQuery<ContractDocument>("exec dbo.[LB_GetContractDocumentByID] @DocumentID",
                new SqlParameter("@DocumentID", DocumentID)
                ).First();

                if (contractDocument == null)
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

                        db.LB_DeleteContractDocument(DocumentID, AdminID);

                        // Get the filename and try to delete it

                        string folderName = Server.MapPath(Constants.ContractDocumentFolderPath) + ContractID.ToString();
                        string fullPath = Server.MapPath(Constants.DocumentFolderPath + ContractID.ToString()) + "\\" + contractDocument.FileName;

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
                        // Reset the contractDocument object to null
                        contractDocument = new ContractDocument();
                    }
                }
            }

            // Initalize the View Model

            var contractDocumentViewModel = new ContractDocumentViewModel();

            contractDocumentViewModel.ContractDocument = contractDocument;

            var thisDocumentList = db.Database.SqlQuery<ContractDocument>("exec dbo.[LB_GetContractDocuments] @ContractID",
            new SqlParameter("@ContractID", ContractID)
            ).ToList();

            contractDocumentViewModel.ContractDocumentList = thisDocumentList;

            if (pageLayout == "I")
            {
                return View("ContractDocuments", "~/Views/Shared/_IFrameLayout.cshtml", contractDocumentViewModel);
            }
            else
            {
                return View(contractDocumentViewModel);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ContractDocuments(HttpPostedFileBase documentFile, [Bind(Include = "contractDocument")]ContractDocumentViewModel contractDocumentViewModel, string pageLayout = "")
        {
            var DataHelper = new DataHelpers();

            int ContractID = new int { };
            int DocumentID = new int { };

            ViewBag.ContractDisplayName = "";
            ViewBag.PageLayout = pageLayout;

            ContractID = contractDocumentViewModel.ContractDocument.ContractID;

            if (ModelState.IsValid)
            {
                DocumentID = contractDocumentViewModel.ContractDocument.DocumentID;

                if (DocumentID == 0)
                {
                    if (documentFile != null && documentFile.ContentLength > 0)
                    {
                        string folderName = Server.MapPath(Constants.ContractDocumentFolderPath) + ContractID.ToString();
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
                                ModelState.AddModelError("", "A file with this name already exists on the server for this Contract");
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

                        string documentPath = Constants.ContractDocumentFolderPath + ContractID.ToString();

                        db.LB_InsertContractDocument(ContractID, 
                             Path.GetFileName(documentFile.FileName),
                            documentPath,
                            "Contract", "",
                            contractDocumentViewModel.ContractDocument.DocumentName,
                            contractDocumentViewModel.ContractDocument.DocumentDescription,
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

                    db.LB_UpdateContractDocument(ContractID, DocumentID, 
                        "Contract", "",
                        contractDocumentViewModel.ContractDocument.DocumentName,
                        contractDocumentViewModel.ContractDocument.DocumentDescription,
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

            var thisDocumentList = db.Database.SqlQuery<ContractDocument>("exec dbo.[LB_GetContractDocuments] @ContractID",
            new SqlParameter("@ContractID", ContractID)
            ).ToList();

            contractDocumentViewModel.ContractDocumentList = thisDocumentList;

            if (ModelState.IsValid)
            {
                var emptyContractDocument = new ContractDocument() { ContractID = ContractID };
                contractDocumentViewModel.ContractDocument = emptyContractDocument;
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
                return View("ContractDocuments", "~/Views/Shared/_IFrameLayout.cshtml", contractDocumentViewModel);
            }
            else
            {
                ViewBag.NextAction = "Requirements";
                return View(contractDocumentViewModel);
            }
        }
       
        [HttpGet]
        public ActionResult DownloadFile(int DocumentID)
        {

            // Check Document
            var contractDocument = db.Database.SqlQuery<ContractDocument>("exec dbo.[LB_GetContractDocumentByID] @DocumentID",
            new SqlParameter("@DocumentID", DocumentID)
            ).First();

            if (contractDocument == null)
            {
                // document not found so error
                return HttpNotFound();
            }
            else
            {

                string fileFolder = contractDocument.Path;
                string fileName = contractDocument.FileName;

                string fullPath = Server.MapPath(String.Format(fileFolder + "/{0}", fileName));
                if (System.IO.File.Exists(fullPath))
                {
                    byte[] filedata = System.IO.File.ReadAllBytes(fullPath);
                    string contentType = MimeMapping.GetMimeMapping(fullPath);

                    System.Net.Mime.ContentDisposition cd = new System.Net.Mime.ContentDisposition
                    {
                        FileName = fileName,
                        Inline = true,
                    };

                    Response.AppendHeader("Content-Disposition", cd.ToString());

                    return File(filedata, contentType);
                }
                return HttpNotFound();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateStatus(int ContractID, string Status)
        {

                ObjectParameter returnID = new ObjectParameter("returnID", typeof(int));
                ObjectParameter returnMessage = new ObjectParameter("returnMessage", typeof(string));

                db.LB_UpdateContractStatus(ContractID, Status, AdminID, returnID, returnMessage);

                var scalarID = (int)returnID.Value;
                var errorMessage = (string)returnMessage.Value ?? "";

                if (scalarID == -1)
                {
                    ModelState.AddModelError("", errorMessage);
                }

                var contractProfileViewModel = new ContractProfileViewModel();

                // Contract Record
                contractProfileViewModel.ContractRecord = db.Database.SqlQuery<Contract>("exec dbo.[LB_GetContractByID] @ContractID",
                new SqlParameter("@ContractID", ContractID)
                ).First();

                // Program Summary List
                contractProfileViewModel.ContractDetailList = db.Database.SqlQuery<ContractDetail>("exec dbo.[LB_GetContractDetailByContractID] @ContractID",
                new SqlParameter("@ContractID", ContractID)
                ).ToList();

                return View("ContractProfile", contractProfileViewModel);

        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }



    }
}