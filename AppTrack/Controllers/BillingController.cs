using AppTrack.Helpers;
using AppTrack.Models;
using AppTrack.SharedModels;
using AppTrack.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;
using PayPal.Payments.DataObjects;
using PayPal.Payments.Transactions;

namespace AppTrack.Controllers
{
    public class ControllerHelpers
    {
        private DevProvidentIDOCEntities db = new DevProvidentIDOCEntities();

        public OrderUpdateViewModel PopulateModelLists(OrderUpdateViewModel model, int OrderID, string Status)
        {

            model.PaymentMethod = db.Database.SqlQuery<C_PaymentMethod>("exec dbo.[LB_GetPaymentMethodByOrderID] @OrderID",
            new SqlParameter("@OrderID", OrderID)
             ).FirstOrDefault();

            model.OrderHeader = db.Database.SqlQuery<C_OrderHeader>("exec dbo.[LB_GetOrderHeaderByOrderID] @OrderID",
            new SqlParameter("@OrderID", OrderID)
             ).FirstOrDefault();

            model.OrderDetailList = db.Database.SqlQuery<C_OrderDetail>("exec dbo.[LB_GetOrderDetailByOrderID] @OrderID",
            new SqlParameter("@OrderID", OrderID)
             ).ToList();

            model.PaymentHistoryList = db.Database.SqlQuery<C_PaymentHistory>("exec dbo.[LB_GetPaymentHistoryByOrderID] @OrderID, @Status",
             new SqlParameter("@OrderID", OrderID),
             new SqlParameter("@Status", Status)
             ).ToList();

            model.ReturnPaymentList = db.Database.SqlQuery<C_PaymentHistory>("exec dbo.[LB_GetPaymentHistoryToReturnByOrderID] @OrderID",
             new SqlParameter("@OrderID", OrderID)
             ).ToList();

            model.RefundPaymentList = db.Database.SqlQuery<C_PaymentHistory>("exec dbo.[LB_GetPaymentHistoryToRefundByOrderID] @OrderID",
             new SqlParameter("@OrderID", OrderID)
             ).ToList();

            model.DiscountList = db.Database.SqlQuery<C_OrderHistory>("exec dbo.[LB_GetOrderHistoryByOrderID] @OrderID, @TransactionType",
             new SqlParameter("@OrderID", OrderID),
             new SqlParameter("@TransactionType", "DISCOUNT")
             ).ToList();

            return model;
        }
    }

    [AuthorizeAdminRedirect(Roles = "Accounting,Finance,MemberServices,MemberServicesManager")]
    public class BillingController : BaseController
    {
        private DevProvidentIDOCEntities db = new DevProvidentIDOCEntities();

        private DataHelpers DataHelper = new DataHelpers();
        private ControllerHelpers ControllerHelper = new ControllerHelpers();

        // ProfileList : Get
        [AuthorizeAdminRedirect(Roles = "Accounting,Finance")]
        [HttpGet]
        public ActionResult ProfileList(string id = "")
        {
            int intCustID = 0;

            if (id != "")
            {
                if (!Int32.TryParse(id, out intCustID))
                    intCustID = 0;

            }
            // Get first and last date of current month
            DateTime now = DateTime.Now;
            var searchStartDate = new DateTime(now.Year, now.Month, 1);
            var searchEndDate = searchStartDate.AddMonths(1).AddDays(-1);

            string searchStatus = " ";

            var autoShipHeaderList = db.Database.SqlQuery<AutoShipHeader>("exec dbo.[LB_GetAutoShipHeader] @StartDate, @EndDate, @CustID, @Status",
            new SqlParameter("@StartDate", searchStartDate),
            new SqlParameter("@EndDate", searchEndDate),
             new SqlParameter("@CustID", intCustID),
             new SqlParameter("@Status", searchStatus)
             ).ToList();

            IEnumerable<System.Web.Mvc.SelectListItem> searchStatusList = DataHelper.GetStatusSelectList(Constants.autoShipStatusLookupGroupID, false, true);

            var autoShipHeaderListViewModel = new AutoShipHeaderListViewModel
            {
                AutoShipHeaderList = autoShipHeaderList,
                SearchStartDate = searchStartDate,
                SearchEndDate = searchEndDate,
                SearchCustID = id,
                SearchStatus = searchStatus,
                SearchStatusList = searchStatusList
            };

            return View(autoShipHeaderListViewModel);
        }

        [AuthorizeAdminRedirect(Roles = "Accounting,Finance")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ProfileList([Bind(Include = "SearchStartDate, SearchEndDate, SearchCustID, SearchStatus")] AutoShipHeaderListViewModel autoShipHeaderListViewModel, string submitAction)
        {
            if (ModelState.IsValid)
            {
                ViewBag.ShowConfirmation = "False";
                int intCustID = 0;

                if (!Int32.TryParse(autoShipHeaderListViewModel.SearchCustID, out intCustID))
                    intCustID = 0;

                string searchStatus = autoShipHeaderListViewModel.SearchStatus ?? " ";

                DateTime now = DateTime.Now;
                DateTime searchStartDate = autoShipHeaderListViewModel.SearchStartDate ?? DateTime.Now;
                DateTime searchEndDate = autoShipHeaderListViewModel.SearchEndDate ?? DateTime.Now;
                if (searchStartDate > searchEndDate)
                {
                    ModelState.AddModelError("", "Start Date cannot be after End Date");
                    // Return below using empty model
                    autoShipHeaderListViewModel.AutoShipHeaderList = new List<AutoShipHeader>();
                }
                else
                {
                    if (submitAction == "Generate Orders")
                    {
                        if (searchStatus != "Open")
                        {
                            ModelState.AddModelError("", "You must select Status of Open when Generating Orders");
                            // Get the list from search criteria entered
                            autoShipHeaderListViewModel.AutoShipHeaderList = db.Database.SqlQuery<AutoShipHeader>("exec dbo.[LB_GetAutoShipHeader] @StartDate, @EndDate, @CustID, @Status",
                             new SqlParameter("@StartDate", searchStartDate),
                             new SqlParameter("@EndDate", searchEndDate),
                             new SqlParameter("@CustID", intCustID),
                             new SqlParameter("@Status", searchStatus)
                            ).ToList();
                        }
                        else
                        {
                            // Attempt to create tmp table with AutoShipHeader records to use to generate orders

                            ObjectParameter returnID = new ObjectParameter("returnID", typeof(int));
                            ObjectParameter returnMessage = new ObjectParameter("returnMessage", typeof(string));

                            db.LB_InsertTmp_GenerateAutoShipHeader(searchStartDate, searchEndDate, intCustID, searchStatus, AdminID,
                                returnID, returnMessage);

                            var scalarID = (int)returnID.Value;
                            var errorMessage = (string)returnMessage.Value ?? "";

                            if (scalarID == -1)
                            {
                                // Notify user that there is an order generation in progress, and show them the list in progress below
                                ModelState.AddModelError("", errorMessage);
                            }
                            ViewBag.ShowConfirmation = "True";

                            // Get the list from Tmp table
                            autoShipHeaderListViewModel.AutoShipHeaderList = db.Database.SqlQuery<AutoShipHeader>("exec dbo.[LB_GetTmp_GenerateAutoShipHeader]").ToList();
                        }
                    }
                    else
                    {
                        autoShipHeaderListViewModel.AutoShipHeaderList = db.Database.SqlQuery<AutoShipHeader>("exec dbo.[LB_GetAutoShipHeader] @StartDate, @EndDate, @CustID, @Status",
                         new SqlParameter("@StartDate", searchStartDate),
                         new SqlParameter("@EndDate", searchEndDate),
                         new SqlParameter("@CustID", intCustID),
                         new SqlParameter("@Status", searchStatus)
                         ).ToList();
                    } // submitAction = Generate Orders
                } // Search Start and End Date valid
            } // ModelState.IsValid
            IEnumerable<System.Web.Mvc.SelectListItem> searchStatusList = DataHelper.GetStatusSelectList(Constants.autoShipStatusLookupGroupID, false, true);

            autoShipHeaderListViewModel.SearchStatusList = searchStatusList;

            return View(autoShipHeaderListViewModel);
        }


        // ProfileUpdate : Get
        [AuthorizeAdminRedirect(Roles = "Accounting,Finance")]
        [HttpGet]
        public ActionResult ProfileUpdate(int id)
        {
            var autoShipHeaderUpdateViewModel = new AutoShipHeaderUpdateViewModel();

            autoShipHeaderUpdateViewModel.AutoShipHeader = db.Database.SqlQuery<AutoShipHeader>("exec dbo.[LB_GetAutoShipHeaderByID] @AutoShipID",
             new SqlParameter("@AutoShipID", id)
             ).First();

            autoShipHeaderUpdateViewModel.AutoShipDetailList = db.Database.SqlQuery<AutoShipDetail>("exec dbo.[LB_GetAutoShipDetailByAutoShipID] @AutoShipID",
             new SqlParameter("@AutoShipID", id)
             ).ToList();

            IEnumerable<System.Web.Mvc.SelectListItem> editStatusList = DataHelper.GetStatusSelectList(Constants.autoShipStatusLookupGroupID, true, false);

            autoShipHeaderUpdateViewModel.EditStatusList = editStatusList;
            autoShipHeaderUpdateViewModel.EditNextDate = autoShipHeaderUpdateViewModel.AutoShipHeader.NextDate;
            autoShipHeaderUpdateViewModel.EditStatus = autoShipHeaderUpdateViewModel.AutoShipHeader.Status;

            return PartialView("_ProfileUpdate", autoShipHeaderUpdateViewModel);
        }

        [AuthorizeAdminRedirect(Roles = "Accounting,Finance")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ProfileUpdate([Bind(Include = "AutoShipHeader, EditStatus, EditNextDate")] AutoShipHeaderUpdateViewModel autoShipHeaderUpdateViewModel)
        {
            int autoShipID = autoShipHeaderUpdateViewModel.AutoShipHeader.AutoShipID;
            ViewBag.ErrorCode = 0;

            if (ModelState.IsValid)
            {

                if (autoShipHeaderUpdateViewModel.EditNextDate < DateTime.Now)
                {
                    ModelState.AddModelError("", "Next Billing Date cannot be in the past");
                    ViewBag.ErrorCode = 1;
                }
                else
                {
                    autoShipID = autoShipHeaderUpdateViewModel.AutoShipHeader.AutoShipID;

                    // Update Profile
                    ObjectParameter returnID = new ObjectParameter("returnID", typeof(int));
                    ObjectParameter returnMessage = new ObjectParameter("returnMessage", typeof(string));

                    db.LB_UpdateAutoShipHeader(autoShipID,
                        autoShipHeaderUpdateViewModel.EditStatus,
                        autoShipHeaderUpdateViewModel.EditNextDate,
                        AdminID,
                        returnID, returnMessage);

                    var scalarID = (int)returnID.Value;
                    var errorMessage = (string)returnMessage.Value ?? "";

                    if (scalarID == -1)
                    {
                        ModelState.AddModelError("", errorMessage);
                        ViewBag.ErrorCode = 1;
                    }
                }
            }

            autoShipHeaderUpdateViewModel.AutoShipHeader = db.Database.SqlQuery<AutoShipHeader>("exec dbo.[LB_GetAutoShipHeaderByID] @AutoShipID",
             new SqlParameter("@AutoShipID", autoShipID)
             ).First();


            autoShipHeaderUpdateViewModel.AutoShipDetailList = db.Database.SqlQuery<AutoShipDetail>("exec dbo.[LB_GetAutoShipDetailByAutoShipID] @AutoShipID",
             new SqlParameter("@AutoShipID", autoShipID)
             ).ToList();

            IEnumerable<System.Web.Mvc.SelectListItem> editStatusList = DataHelper.GetStatusSelectList(Constants.autoShipStatusLookupGroupID, true, false);

            autoShipHeaderUpdateViewModel.EditStatusList = editStatusList;

            return PartialView("_ProfileUpdate", autoShipHeaderUpdateViewModel);
        }


        [AuthorizeAdminRedirect(Roles = "Accounting,Finance")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GenerateOrders(string submitAction)
        {
            ViewBag.ShowConfirmation = "False";

            if (submitAction == "Generate Orders")
            {
                ObjectParameter returnID = new ObjectParameter("returnID", typeof(int));
                ObjectParameter returnMessage = new ObjectParameter("returnMessage", typeof(string));

                db.LB_GenerateOrdersFromTmp_GenerateAutoShipHeader(returnID, returnMessage);

                var scalarID = (int)returnID.Value;
                var errorMessage = (string)returnMessage.Value ?? "";

                // scalaraID = BatchID stamped on orders or -1 if error
                if (scalarID == -1)
                {
                    // Notify user that there was a fatal error generating orders 
                    // Records exist in C_AutoShipHeader with temporary statusid = 7 either before or after orders were generated
                    ViewBag.ErrorCode = Constants.fatalErrorCode;
                    ViewBag.ErrorMessage = errorMessage;

                    return View("ProfileList");
                }
                else
                {
                    // Get the orders generated and return to Order list view
                    var orderHeaderList = new List<C_OrderHeader>();

                    orderHeaderList = db.Database.SqlQuery<C_OrderHeader>("exec dbo.[LB_GetOrderHeaderByBatchID] @BatchID",
                     new SqlParameter("@BatchID", scalarID)
                     ).ToList();

                    return Redirect("/Billing/OrderList/?BatchID=" + scalarID.ToString());
                }
            }
            else
            {
                // Truncate tmp table to cancel order generation request and return to Profile List with empty Model

                db.LB_TruncateTmp_GenerateAutoShipHeader();
                var autoShipHeaderListViewModel = new AutoShipHeaderListViewModel();

                autoShipHeaderListViewModel.AutoShipHeaderList = new List<AutoShipHeader>();

                IEnumerable<System.Web.Mvc.SelectListItem> searchStatusList = DataHelper.GetStatusSelectList(Constants.autoShipStatusLookupGroupID, false, true);

                autoShipHeaderListViewModel.SearchStatusList = searchStatusList;

                ModelState.AddModelError("", "Previous request to generate orders was cancelled");

                DateTime now = DateTime.Now;
                var searchStartDate = new DateTime(now.Year, now.Month, 1);

                autoShipHeaderListViewModel.SearchStartDate = searchStartDate;
                autoShipHeaderListViewModel.SearchEndDate = searchStartDate.AddMonths(1).AddDays(-1);

                autoShipHeaderListViewModel.SearchCustID = " ";
                autoShipHeaderListViewModel.SearchStatus = " ";

                return View("ProfileList", autoShipHeaderListViewModel);
            }
        }

        [AuthorizeAdminRedirect(Roles = "Accounting,Finance,MemberServicesManager")]
        // GET: OrderList
        public ActionResult OrderList(int BatchID = 0)
        {
            var orderListViewModel = new OrderListViewModel();

            orderListViewModel.OrderHeaderList = db.Database.SqlQuery<C_OrderHeader>("exec dbo.[LB_GetOrderHeaderByBatchID] @BatchID",
            new SqlParameter("@BatchID", BatchID)
             ).ToList();

            orderListViewModel.SearchStartDate = DateTime.Now;
            orderListViewModel.SearchEndDate = DateTime.Now;
            orderListViewModel.SearchStartBalance = -9999;
            orderListViewModel.SearchEndBalance = 9999;
            orderListViewModel.SearchCustID = 0;
            orderListViewModel.SearchStatus = " ";
            orderListViewModel.SearchItemID = 0;

            IEnumerable<System.Web.Mvc.SelectListItem> searchItemList = DataHelper.GetAllMembershipItemsSelectList(false, true);

            orderListViewModel.SearchItemList = searchItemList;

            IEnumerable<System.Web.Mvc.SelectListItem> searchStatusList = DataHelper.GetStatusSelectList(Constants.autoShipStatusLookupGroupID, false, true);

            orderListViewModel.SearchStatusList = searchStatusList;

            return View(orderListViewModel);


        }

        [AuthorizeAdminRedirect(Roles = "Accounting,Finance,MemberServicesManager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult OrderList([Bind(Include = "SearchStartDate, SearchEndDate, SearchStartBalance, SearchEndBalance, SearchCustID, SearchStatus, SearchItemID, SearchOrderID")] 
            OrderListViewModel orderListViewModel)
        {
            if (ModelState.IsValid)
            {
                DateTime now = DateTime.Now;
                DateTime searchStartDate = orderListViewModel.SearchStartDate ?? DateTime.Now;
                DateTime searchEndDate = orderListViewModel.SearchEndDate ?? DateTime.Now;
                decimal searchStartBalance = orderListViewModel.SearchStartBalance;
                decimal searchEndBalance = orderListViewModel.SearchEndBalance;
                int searchCustID = orderListViewModel.SearchCustID ?? 0;
                string searchStatus = orderListViewModel.SearchStatus ?? " ";
                int searchItemID = orderListViewModel.SearchItemID ?? 0;
                int searchOrderID = orderListViewModel.SearchOrderID ?? 0;

                if (searchStartDate > searchEndDate)
                {
                    ModelState.AddModelError("", "Start Date cannot be after End Date");
                    // Return below using empty model
                    orderListViewModel.OrderHeaderList = new List<C_OrderHeader>();
                }
                else
                {
                    if (searchStartBalance > searchEndBalance)
                    {
                        ModelState.AddModelError("", "Starting Balance cannot be greater than Ending Balance");
                        // Return below using empty model
                        orderListViewModel.OrderHeaderList = new List<C_OrderHeader>();
                    }
                    else
                    {
                        orderListViewModel.OrderHeaderList = db.Database.SqlQuery<C_OrderHeader>("exec dbo.[LB_GetOrderHeaderList] @StartDate, @EndDate, @StartBalance, @EndBalance, @CustID, @Status, @ItemID, @OrderID",
                            new SqlParameter("@StartDate", searchStartDate),
                            new SqlParameter("@EndDate", searchEndDate),
                            new SqlParameter("@StartBalance", searchStartBalance),
                            new SqlParameter("@EndBalance", searchEndBalance),
                            new SqlParameter("@CustID", searchCustID),
                            new SqlParameter("@Status", searchStatus),
                            new SqlParameter("@ItemID", searchItemID),
                            new SqlParameter("@OrderID", searchOrderID)
                            ).ToList();

                    }
                } // Search Start and End Date valid
            } // ModelState.IsValid
            IEnumerable<System.Web.Mvc.SelectListItem> searchItemList = DataHelper.GetAllMembershipItemsSelectList(false, true);

            orderListViewModel.SearchItemList = searchItemList;

            IEnumerable<System.Web.Mvc.SelectListItem> searchStatusList = DataHelper.GetStatusSelectList(Constants.autoShipStatusLookupGroupID, false, true);

            orderListViewModel.SearchStatusList = searchStatusList;

            return View(orderListViewModel);
        }

        [AuthorizeAdminRedirect(Roles = "Accounting,Finance")]
        // GET: FailedPaymentList
        public ActionResult FailedPaymentList(int BatchID = 0)
        {
            var model = new FailedPaymentListViewModel();

            DateTime now = DateTime.Now;
            model.SearchStartDate = DateTime.Now.AddMonths(-1);
            model.SearchEndDate = DateTime.Now;
            model.SearchStartBalance = 0.01M;
            model.SearchEndBalance = 999999.00M;
            model.SearchCustID = 0;
            model.SearchStatus = " ";
            model.SearchItemID = 0;
            model.SearchOrderID = 0;

            model.OrderHeaderList = db.Database.SqlQuery<FailedPaymentList>("exec dbo.[LB_GetOrderHeaderFailedPayments] @StartDate, @EndDate, @StartBalance, @EndBalance, @CustID, @Status, @ItemID, @OrderID",
                 new SqlParameter("@StartDate", model.SearchStartDate),
                 new SqlParameter("@EndDate", model.SearchEndDate),
                 new SqlParameter("@StartBalance", model.SearchStartBalance),
                 new SqlParameter("@EndBalance", model.SearchEndBalance),
                 new SqlParameter("@CustID", model.SearchCustID),
                 new SqlParameter("@Status", model.SearchStatus),
                 new SqlParameter("@ItemID", model.SearchItemID),
                 new SqlParameter("@OrderID", model.SearchOrderID)
            ).ToList();

            IEnumerable<System.Web.Mvc.SelectListItem> searchItemList = DataHelper.GetAllMembershipItemsSelectList(false, true);
            model.SearchItemList = searchItemList;
            IEnumerable<System.Web.Mvc.SelectListItem> searchStatusList = DataHelper.GetStatusSelectList(Constants.autoShipStatusLookupGroupID, false, true);
            model.SearchStatusList = searchStatusList;
            return View(model);

        }

        [AuthorizeAdminRedirect(Roles = "Accounting,Finance")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult FailedPaymentList([Bind(Include = "SearchStartDate, SearchEndDate, SearchStartBalance, SearchEndBalance, SearchCustID, SearchStatus, SearchItemID, SearchOrderID")] 
            FailedPaymentListViewModel model)
        {
            if (ModelState.IsValid)
            {
                DateTime now = DateTime.Now;
                DateTime searchStartDate = model.SearchStartDate ?? DateTime.Now;
                DateTime searchEndDate = model.SearchEndDate ?? DateTime.Now;
                decimal searchStartBalance = model.SearchStartBalance;
                decimal searchEndBalance = model.SearchEndBalance;
                int searchCustID = model.SearchCustID ?? 0;
                string searchStatus = model.SearchStatus ?? " ";
                int searchItemID = model.SearchItemID ?? 0;
                int searchOrderID = model.SearchOrderID ?? 0;

                if (searchStartDate > searchEndDate)
                {
                    ModelState.AddModelError("", "Start Date cannot be after End Date");
                    // Return below using empty model
                }
                else
                {
                    if (searchStartBalance > searchEndBalance)
                    {
                        ModelState.AddModelError("", "Starting Balance cannot be greater than Ending Balance");
                        // Return below using empty model
                    }
                    else
                    {
                        model.OrderHeaderList = db.Database.SqlQuery<FailedPaymentList>("exec dbo.[LB_GetOrderHeaderFailedPayments] @StartDate, @EndDate, @StartBalance, @EndBalance, @CustID, @Status, @ItemID, @OrderID",
                            new SqlParameter("@StartDate", searchStartDate),
                            new SqlParameter("@EndDate", searchEndDate),
                            new SqlParameter("@StartBalance", searchStartBalance),
                            new SqlParameter("@EndBalance", searchEndBalance),
                            new SqlParameter("@CustID", searchCustID),
                            new SqlParameter("@Status", searchStatus),
                            new SqlParameter("@ItemID", searchItemID),
                            new SqlParameter("@OrderID", searchOrderID)
                            ).ToList();

                    }
                } // Search Start and End Date valid
            } // ModelState.IsValid
            IEnumerable<System.Web.Mvc.SelectListItem> searchItemList = DataHelper.GetAllMembershipItemsSelectList(false, true);

            model.SearchItemList = searchItemList;

            IEnumerable<System.Web.Mvc.SelectListItem> searchStatusList = DataHelper.GetStatusSelectList(Constants.autoShipStatusLookupGroupID, false, true);

            model.SearchStatusList = searchStatusList;

            return View(model);
        }

        // GET: OrderUpdate
        public ActionResult OrderUpdate(int id = 0)
        {
            int OrderID = id;

            var orderUpdateViewModel = new OrderUpdateViewModel();

            orderUpdateViewModel = ControllerHelper.PopulateModelLists(orderUpdateViewModel, OrderID, "SUCCESS");

            orderUpdateViewModel.PaymentDate = DateTime.Now;
            orderUpdateViewModel.PaymentType = "Check";
            orderUpdateViewModel.PaymentAmount = 0;
            orderUpdateViewModel.CheckNumber = null;
            orderUpdateViewModel.DiscountAmount = 0;
            orderUpdateViewModel.OrderID = OrderID;

            return View(orderUpdateViewModel);
        }

        [AuthorizeAdminRedirect(Roles = "Accounting,Finance,MemberServicesManager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ApplyDiscount([Bind(Include = "OrderID, DiscountAmount, DiscountDescription")] OrderUpdateViewModel orderUpdateViewModel)
        {
            ModelState.Remove("TransactionID");
            ModelState.Remove("PaymentID");
            ModelState.Remove("PaymentDate");
            ModelState.Remove("PaymentAmount");
            ModelState.Remove("PaymentType");
            ModelState.Remove("RefundAmount");
            ModelState.Remove("Description");
            ModelState.Remove("CheckDate");
            ModelState.Remove("CheckNumber");
            ModelState.Remove("CheckAmount");

            if (ModelState.IsValid)
            {

                ObjectParameter returnID = new ObjectParameter("returnID", typeof(int));
                ObjectParameter returnMessage = new ObjectParameter("returnMessage", typeof(string));

                db.LB_UpdateOrderDiscountV2(orderUpdateViewModel.OrderID, orderUpdateViewModel.DiscountAmount, orderUpdateViewModel.DiscountDescription, AdminID, returnID, returnMessage);

                var scalarID = (int)returnID.Value;
                var errorMessage = (string)returnMessage.Value ?? "";

                if (scalarID == -1)
                {
                    ModelState.AddModelError("", errorMessage);
                }
                else
                {
                    ModelState.Clear();
                    return RedirectToAction("OrderUpdate", new { id = orderUpdateViewModel.OrderID });
                }
            }

            orderUpdateViewModel = ControllerHelper.PopulateModelLists(orderUpdateViewModel, orderUpdateViewModel.OrderID, "SUCCESS");

            orderUpdateViewModel.PaymentDate = DateTime.Now;
            orderUpdateViewModel.PaymentType = "Check";
            orderUpdateViewModel.PaymentAmount = 0;
            orderUpdateViewModel.CheckNumber = null;

            return View("OrderUpdate", orderUpdateViewModel);
        }

        [AuthorizeAdminRedirect(Roles = "Accounting,Finance")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ApplyPayment([Bind(Include = "OrderID, PaymentDate, PaymentType, CheckNumber, TransactionID, PaymentAmount")] OrderUpdateViewModel orderUpdateViewModel)
        {
            ModelState.Remove("PaymentID");
            ModelState.Remove("DiscountAmount");
            ModelState.Remove("DiscountDescription");
            ModelState.Remove("RefundAmount");
            ModelState.Remove("Description");
            ModelState.Remove("CheckDate");
            ModelState.Remove("CheckAmount");
            if (orderUpdateViewModel.PaymentType == "Check")
            {
                ModelState.Remove("TransactionID");
            }
            if (orderUpdateViewModel.PaymentType == "FDC")
            {
                ModelState.Remove("CheckNumber");
            }

            if (ModelState.IsValid)
            {

                if (orderUpdateViewModel.PaymentAmount <= 0)
                {
                    ModelState.AddModelError("", "Payment Amount must be > 0");
                }
                if (orderUpdateViewModel.PaymentType == "Check" && orderUpdateViewModel.CheckNumber == null)
                {
                    ModelState.AddModelError("", "Check Number is required for payments by check");
                }
                if (orderUpdateViewModel.PaymentType == "FDC" && orderUpdateViewModel.TransactionID == null)
                {
                    ModelState.AddModelError("", "Transaction ID is required for payments by FDC");
                }

                if (ModelState.IsValid)
                {
                    ObjectParameter returnID = new ObjectParameter("returnID", typeof(int));
                    ObjectParameter returnMessage = new ObjectParameter("returnMessage", typeof(string));

                    db.LB_UpdateOrderPayment(orderUpdateViewModel.OrderID, orderUpdateViewModel.PaymentDate, orderUpdateViewModel.PaymentType, orderUpdateViewModel.CheckNumber, orderUpdateViewModel.TransactionID, orderUpdateViewModel.PaymentAmount, AdminID, returnID, returnMessage);

                    var scalarID = (int)returnID.Value;
                    var errorMessage = (string)returnMessage.Value ?? "";

                    if (scalarID == -1)
                    {
                        ModelState.AddModelError("", errorMessage);
                    }
                    else
                    {
                        ModelState.Clear();
                        return RedirectToAction("OrderUpdate", new { id = orderUpdateViewModel.OrderID });
                    }
                }
            }

            orderUpdateViewModel = ControllerHelper.PopulateModelLists(orderUpdateViewModel, orderUpdateViewModel.OrderID, "SUCCESS");

            return View("OrderUpdate", orderUpdateViewModel);
        }

        [AuthorizeAdminRedirect(Roles = "Accounting,Finance")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ReturnPayment([Bind(Include = "OrderID, PaymentID")] OrderUpdateViewModel orderUpdateViewModel)
        {
            ModelState.Remove("TransactionID");
            ModelState.Remove("PaymentDate");
            ModelState.Remove("PaymentAmount");
            ModelState.Remove("PaymentType");
            ModelState.Remove("DiscountAmount");
            ModelState.Remove("DiscountDescription");
            ModelState.Remove("RefundAmount");
            ModelState.Remove("Description");
            ModelState.Remove("CheckDate");
            ModelState.Remove("CheckNumber");
            ModelState.Remove("CheckAmount");

            if (ModelState.IsValid)
            {

                ObjectParameter returnID = new ObjectParameter("returnID", typeof(int));
                ObjectParameter returnMessage = new ObjectParameter("returnMessage", typeof(string));

                db.LB_UpdateReturnPayment(orderUpdateViewModel.PaymentID, AdminID, returnID, returnMessage);

                var scalarID = (int)returnID.Value;
                var errorMessage = (string)returnMessage.Value ?? "";

                if (scalarID == -1)
                {
                    ModelState.AddModelError("", errorMessage);
                }
                else
                {
                    ModelState.Clear();
                    return RedirectToAction("OrderUpdate", new { id = orderUpdateViewModel.OrderID });
                }
            }

            orderUpdateViewModel = ControllerHelper.PopulateModelLists(orderUpdateViewModel, orderUpdateViewModel.OrderID, "SUCCESS");

            return View("OrderUpdate", orderUpdateViewModel);
        }

        [AuthorizeAdminRedirect(Roles = "Accounting,Finance")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ProcessCreditCardPayment([Bind(Include = "OrderID")] OrderUpdateViewModel orderUpdateViewModel)
        {
            ModelState.Remove("TransactionID");
            ModelState.Remove("PaymentDate");
            ModelState.Remove("PaymentAmount");
            ModelState.Remove("PaymentType");
            ModelState.Remove("DiscountAmount");
            ModelState.Remove("DiscountDescription");
            ModelState.Remove("CheckDate");
            ModelState.Remove("CheckNumber");
            ModelState.Remove("CheckAmount");
            ModelState.Remove("PaymentID");
            ModelState.Remove("RefundAmount");
            ModelState.Remove("Description");
            if (orderUpdateViewModel.OrderID != null)
            {

                // Get 
                var processPaymentController = DependencyResolver.Current.GetService<ProcessPaymentsController>();
                var result = processPaymentController.AuthorizeAndCapture(orderUpdateViewModel.OrderID);
                string strresult = result;
                string isPaymentSuccess = strresult.Substring(0, 7);
                if (isPaymentSuccess != "Success")
                {
                    ModelState.AddModelError("", "The Payment Did Not Post" + strresult);
                    orderUpdateViewModel = ControllerHelper.PopulateModelLists(orderUpdateViewModel, orderUpdateViewModel.OrderID, "SUCCESS");
                    return View("OrderUpdate", orderUpdateViewModel);
                }
                else
                {
                    ModelState.AddModelError("", "The Payment Posted Successfully");
                    return RedirectToAction("OrderUpdate", new { id = orderUpdateViewModel.OrderID });
                }
            }
            else
            {
                ModelState.AddModelError("", "The Payment Did Not Post");
                orderUpdateViewModel = ControllerHelper.PopulateModelLists(orderUpdateViewModel, orderUpdateViewModel.OrderID, "SUCCESS");
                return View("OrderUpdate", orderUpdateViewModel);
            }
        }

        [AuthorizeAdminRedirect(Roles = "Accounting,Finance")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RefundPayment([Bind(Include = "OrderID, PaymentID, RefundAmount, Description")] OrderUpdateViewModel orderUpdateViewModel)
        {
            ModelState.Remove("TransactionID");
            ModelState.Remove("PaymentDate");
            ModelState.Remove("PaymentAmount");
            ModelState.Remove("PaymentType");
            ModelState.Remove("DiscountAmount");
            ModelState.Remove("DiscountDescription");
            ModelState.Remove("CheckDate");
            ModelState.Remove("CheckNumber");
            ModelState.Remove("CheckAmount");

            if (ModelState.IsValid)
            {

                var PaymentHistoryRecord = new C_PaymentHistory();

                PaymentHistoryRecord = db.Database.SqlQuery<C_PaymentHistory>("exec dbo.[LB_GetPaymentHistoryByID] @PaymentID",
                new SqlParameter("@PaymentID", orderUpdateViewModel.PaymentID)
                 ).FirstOrDefault();

                // Check if null then we have a fatal error
                if (PaymentHistoryRecord == null)
                {
                    ModelState.AddModelError("", "Unexpected error encountered: could not find payment to refund");
                }
                else
                {
                    // Otherwise check for payment type and if CIM process refund through Authorize.net
                    string isRefundSuccess = "";
                    string strresult = "";
                    if (PaymentHistoryRecord.PaymentType == "CIM" || PaymentHistoryRecord.PaymentType == "Paypal")
                    {
                        // Process Authorize.net refund
                        var processPaymentController = DependencyResolver.Current.GetService<ProcessPaymentsController>();
                        var result = processPaymentController.Refund(orderUpdateViewModel.PaymentID, orderUpdateViewModel.RefundAmount);
                        strresult = result;
                        isRefundSuccess = strresult.Substring(0, 7);
                    }

                    // If not successful then set ModelState error and continue to populate model to return
                    if (isRefundSuccess != "Success")
                    {
                        ModelState.AddModelError("", "Unexpected error encountered: unable to process refund through Authorize.net");

                        ObjectParameter returnID = new ObjectParameter("returnID", typeof(int));
                        ObjectParameter returnMessage = new ObjectParameter("returnMessage", typeof(string));

                        db.LB_UpdateRefundPaymentFailed(orderUpdateViewModel.PaymentID, orderUpdateViewModel.RefundAmount, orderUpdateViewModel.Description, strresult, AdminID, returnID, returnMessage);

                        var scalarID = (int)returnID.Value;
                        var errorMessage = (string)returnMessage.Value ?? "";

                        if (scalarID == -1)
                        {
                            ModelState.AddModelError("", errorMessage);
                        }
                        else
                        {
                            ModelState.Clear();
                            return RedirectToAction("OrderUpdate", new { id = orderUpdateViewModel.OrderID });
                        }


                    }
                    else
                    {
                        // If Authorize.net successful OR Payment Type was not CC then record refund 

                        ObjectParameter returnID = new ObjectParameter("returnID", typeof(int));
                        ObjectParameter returnMessage = new ObjectParameter("returnMessage", typeof(string));

                        db.LB_UpdateRefundPayment(orderUpdateViewModel.PaymentID, orderUpdateViewModel.RefundAmount, orderUpdateViewModel.Description, AdminID, returnID, returnMessage);

                        var scalarID = (int)returnID.Value;
                        var errorMessage = (string)returnMessage.Value ?? "";

                        if (scalarID == -1)
                        {
                            ModelState.AddModelError("", errorMessage);
                        }
                        else
                        {
                            ModelState.Clear();
                            return RedirectToAction("OrderUpdate", new { id = orderUpdateViewModel.OrderID });
                        }
                    }
                }
            }

            orderUpdateViewModel = ControllerHelper.PopulateModelLists(orderUpdateViewModel, orderUpdateViewModel.OrderID, "SUCCESS");

            return View("OrderUpdate", orderUpdateViewModel);
        }

        [AuthorizeAdminRedirect(Roles = "Accounting,Finance")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RefundByCheck([Bind(Include = "OrderID, CheckDate, CheckNumber, CheckAmount, Description")] OrderUpdateViewModel orderUpdateViewModel)
        {
            ModelState.Remove("TransactionID");
            ModelState.Remove("PaymentID");
            ModelState.Remove("PaymentDate");
            ModelState.Remove("PaymentAmount");
            ModelState.Remove("PaymentType");
            ModelState.Remove("DiscountAmount");
            ModelState.Remove("DiscountDescription");
            ModelState.Remove("RefundAmount");

            if (ModelState.IsValid)
            {

                if (orderUpdateViewModel.CheckAmount <= 0)
                {
                    ModelState.AddModelError("", "Check Amount must be > 0");
                }
                if (orderUpdateViewModel.CheckNumber == null)
                {
                    ModelState.AddModelError("", "Check Number is required for payments by check");
                }
                if (orderUpdateViewModel.CheckDate == null)
                {
                    ModelState.AddModelError("", "Check date is required");
                }

                if (ModelState.IsValid)
                {
                    ObjectParameter returnID = new ObjectParameter("returnID", typeof(int));
                    ObjectParameter returnMessage = new ObjectParameter("returnMessage", typeof(string));

                    db.LB_UpdateRefundByCheck(orderUpdateViewModel.OrderID, orderUpdateViewModel.CheckDate, orderUpdateViewModel.CheckNumber, orderUpdateViewModel.CheckAmount, orderUpdateViewModel.Description, AdminID, returnID, returnMessage);

                    var scalarID = (int)returnID.Value;
                    var errorMessage = (string)returnMessage.Value ?? "";

                    if (scalarID == -1)
                    {
                        ModelState.AddModelError("", errorMessage);
                    }
                    else
                    {
                        ModelState.Clear();
                        return RedirectToAction("OrderUpdate", new { id = orderUpdateViewModel.OrderID });
                    }
                }
            }

            orderUpdateViewModel = ControllerHelper.PopulateModelLists(orderUpdateViewModel, orderUpdateViewModel.OrderID, "SUCCESS");

            return View("OrderUpdate", orderUpdateViewModel);
        }

        [AuthorizeAdminRedirect(Roles = "Accounting,Finance")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateStatus([Bind(Include = "OrderID")] OrderUpdateViewModel orderUpdateViewModel, string Status)
        {
            ModelState.Remove("TransactionID");
            ModelState.Remove("PaymentID");
            ModelState.Remove("PaymentDate");
            ModelState.Remove("PaymentAmount");
            ModelState.Remove("PaymentType");
            ModelState.Remove("DiscountAmount");
            ModelState.Remove("DiscountDescription");
            ModelState.Remove("RefundAmount");
            ModelState.Remove("Description");
            ModelState.Remove("CheckDate");
            ModelState.Remove("CheckNumber");
            ModelState.Remove("CheckAmount");

            if (ModelState.IsValid)
            {

                ObjectParameter returnID = new ObjectParameter("returnID", typeof(int));
                ObjectParameter returnMessage = new ObjectParameter("returnMessage", typeof(string));

                db.LB_UpdateOrderStatus(orderUpdateViewModel.OrderID, Status, AdminID, returnID, returnMessage);

                var scalarID = (int)returnID.Value;
                var errorMessage = (string)returnMessage.Value ?? "";

                if (scalarID == -1)
                {
                    ModelState.AddModelError("", errorMessage);
                }
                else
                {
                    ModelState.Clear();
                    return RedirectToAction("OrderUpdate", new { id = orderUpdateViewModel.OrderID });
                }
            }

            orderUpdateViewModel = ControllerHelper.PopulateModelLists(orderUpdateViewModel, orderUpdateViewModel.OrderID, "SUCCESS");

            return View("OrderUpdate", orderUpdateViewModel);
        }

        [AuthorizeAdminRedirect(Roles = "Accounting,Finance")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CancelOrder([Bind(Include = "OrderID")] OrderUpdateViewModel orderUpdateViewModel)
        {
            ModelState.Remove("TransactionID");
            ModelState.Remove("PaymentID");
            ModelState.Remove("PaymentDate");
            ModelState.Remove("PaymentAmount");
            ModelState.Remove("PaymentType");
            ModelState.Remove("DiscountAmount");
            ModelState.Remove("DiscountDescription");
            ModelState.Remove("RefundAmount");
            ModelState.Remove("Description");
            ModelState.Remove("CheckDate");
            ModelState.Remove("CheckNumber");
            ModelState.Remove("CheckAmount");

            if (ModelState.IsValid)
            {

                ObjectParameter returnID = new ObjectParameter("returnID", typeof(int));
                ObjectParameter returnMessage = new ObjectParameter("returnMessage", typeof(string));

                decimal cancelAmount = 0;

                db.LB_UpdateOrderCancel(orderUpdateViewModel.OrderID, cancelAmount, AdminID, returnID, returnMessage);

                var scalarID = (int)returnID.Value;
                var errorMessage = (string)returnMessage.Value ?? "";

                if (scalarID == -1)
                {
                    ModelState.AddModelError("", errorMessage);
                }
                else
                {
                    ModelState.Clear();
                    return RedirectToAction("OrderUpdate", new { id = orderUpdateViewModel.OrderID });
                }
            }

            orderUpdateViewModel = ControllerHelper.PopulateModelLists(orderUpdateViewModel, orderUpdateViewModel.OrderID, "SUCCESS");

            return View("OrderUpdate", orderUpdateViewModel);
        }

    }
}