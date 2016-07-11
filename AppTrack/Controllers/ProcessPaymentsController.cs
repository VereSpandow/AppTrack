


using AuthorizeNet;
using PayPal.Payments.DataObjects;
using PayPal.Payments.Transactions;
using AppTrack.Helpers;
using AppTrack.Models;
using AppTrack.SharedModels;
using AppTrack.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;

namespace AppTrack.Controllers
{
    [AuthorizeAdminRedirect(Roles = "Accounting,Finance")]
    public class ProcessPaymentsController : BaseController
    {
        private DevProvidentIDOCEntities db = new DevProvidentIDOCEntities();

        private DataHelpers DataHelper = new DataHelpers();


        // GET: OrderList
        public ActionResult Index(int BatchID = 0)
        {

            DateTime now = DateTime.Now;
            DateTime searchStartDate = DateTime.Now;
            searchStartDate = searchStartDate.AddMonths(-2);
            DateTime searchEndDate = DateTime.Now;
            decimal searchStartBalance = 0.00M;
            decimal searchEndBalance = 9999.99M;
            int searchCustID = 0;
            string searchStatus = "Open";
            int searchItemID = 0;
            int searchOrderID = 0;

            var orderListViewModel = new PaymentOrderListViewModel();

            var thisOrderHeaderList = db.Database.SqlQuery<TmpOrderHeaderForPayment>("exec dbo.[LB_GetOrderHeaderListForPayment] @StartDate, @EndDate, @StartBalance, @EndBalance, @CustID, @Status, @ItemID, @OrderID",
            new SqlParameter("@StartDate", searchStartDate),
            new SqlParameter("@EndDate", searchEndDate),
            new SqlParameter("@StartBalance", searchStartBalance),
            new SqlParameter("@EndBalance", searchEndBalance),
            new SqlParameter("@CustID", searchCustID),
            new SqlParameter("@Status", searchStatus),
            new SqlParameter("@ItemID", searchItemID),
            new SqlParameter("@OrderID", searchOrderID),
            new SqlParameter("@MaxRows", Constants.maxPaymentBatchSize)
            ).ToList();

            orderListViewModel.OrderHeaderList = thisOrderHeaderList;

            orderListViewModel.SearchStartDate = searchStartDate;
            orderListViewModel.SearchEndDate = searchEndDate;
            orderListViewModel.SearchStartBalance = searchStartBalance;
            orderListViewModel.SearchEndBalance = searchEndBalance;
            orderListViewModel.SearchCustID = 0;
            orderListViewModel.SearchStatus = "Open";
            orderListViewModel.SearchItemID = 0;

            IEnumerable<System.Web.Mvc.SelectListItem> searchItemList = DataHelper.GetAllMembershipItemsSelectList(false, true);

            orderListViewModel.SearchItemList = searchItemList;

            IEnumerable<System.Web.Mvc.SelectListItem> searchStatusList = DataHelper.GetStatusSelectList(Constants.autoShipStatusLookupGroupID, false, true);

            orderListViewModel.SearchStatusList = searchStatusList;

            return View(orderListViewModel);


        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index([Bind(Include = "SearchStartDate, SearchEndDate, SearchStartBalance, SearchEndBalance, SearchCustID, SearchStatus, SearchItemID, SearchOrderID, SubmitAction")] 
            PaymentOrderListViewModel model)
        {
            if (ModelState.IsValid)
            {
                DateTime now = DateTime.Now;
                DateTime searchStartDate = model.SearchStartDate ?? DateTime.Now;
                DateTime searchEndDate = model.SearchEndDate ?? DateTime.Now;
                decimal searchStartBalance = model.SearchStartBalance;
                decimal searchEndBalance = model.SearchEndBalance;
                int searchCustID = model.SearchCustID ?? 0;
                string searchStatus = model.SearchStatus ?? "Open";
                int searchItemID = model.SearchItemID ?? 0;
                int searchOrderID = model.SearchOrderID ?? 0;

                if (model.SubmitAction == "Process Payments")
                {
                    searchStatus = "Open";
                    if (searchStartBalance <= 0.00M)
                    {
                        searchStartBalance = 0.00M;
                    }
                }


                if (searchStartDate > searchEndDate)
                {
                    ModelState.AddModelError("", "Start Date cannot be after End Date");
                    // Return below using empty model
                    model.OrderHeaderList = new List<TmpOrderHeaderForPayment>();
                }
                else
                {
                    if (searchStartBalance > searchEndBalance)
                    {
                        ModelState.AddModelError("", "Starting Balance cannot be greater than Ending Balance");
                        // Return below using empty model
                        model.OrderHeaderList = new List<TmpOrderHeaderForPayment>();
                    }
                    else
                    {
                        if (model.SubmitAction == "Process Payments")
                        {
                            var thisPaymentList = db.Database.SqlQuery<TmpOrderHeaderForPayment>("exec dbo.[LB_GetOrderHeaderListForPaymentTmp]").ToList();

                            foreach (var item in thisPaymentList)
                            {
                                int OrderID = item.OrderID;
                                string chargeIT = AuthorizeAndCapture(OrderID);
                            }

                            db.LB_UpdateOrderHeaderListForPaymentTMP();

                            //BatchEmailFailedPayments();
  
                            var thisOrderHeaderList = db.Database.SqlQuery<TmpOrderHeaderForPayment>("exec dbo.[LB_GetPaymentsJustProcessed]").ToList();
                            model.OrderHeaderList = thisOrderHeaderList;

                            //Set SearchStartBalance = 0 so the Process Payments button is not visible when this view is returned.
                            model.SearchStartBalance = 0;
                        
                        }
                        else
                        {
                            var thisOrderHeaderList = db.Database.SqlQuery<TmpOrderHeaderForPayment>("exec dbo.[LB_GetOrderHeaderListForPayment] @StartDate, @EndDate, @StartBalance, @EndBalance, @CustID, @Status, @ItemID, @OrderID",
                                new SqlParameter("@StartDate", searchStartDate),
                                new SqlParameter("@EndDate", searchEndDate),
                                new SqlParameter("@StartBalance", searchStartBalance),
                                new SqlParameter("@EndBalance", searchEndBalance),
                                new SqlParameter("@CustID", searchCustID),
                                new SqlParameter("@Status", searchStatus),
                                new SqlParameter("@ItemID", searchItemID),
                                new SqlParameter("@OrderID", searchOrderID),
                                new SqlParameter("@MaxRows", Constants.maxPaymentBatchSize)
                                ).ToList();
                        
                                model.OrderHeaderList = thisOrderHeaderList;
                        }

 
                    }
                } // Search Start and End Date valid
            } // ModelState.IsValid

            IEnumerable<System.Web.Mvc.SelectListItem> searchItemList = DataHelper.GetAllMembershipItemsSelectList(false, true);
            model.SearchItemList = searchItemList;
            IEnumerable<System.Web.Mvc.SelectListItem> searchStatusList = DataHelper.GetStatusSelectList(Constants.autoShipStatusLookupGroupID, false, true);
            model.SearchStatusList = searchStatusList;

            return View(model);
        }


        [HttpGet]
        public String AuthorizeAndCapture(int OrderID = 0)
        {
            var paymentAttemptRows = db.Database.SqlQuery<PaymentAttempt>("exec dbo.[LB_GetPaymentMethodCCByOrderID]  @OrderID",
             new SqlParameter("@OrderID", OrderID)
             ).FirstOrDefault();

            if (paymentAttemptRows == null)
            {
                return "Failed - No Payment Method";
            }
            int? CustID = paymentAttemptRows.CustID;
            int PaymentMethodID = paymentAttemptRows.PMID;
            string CustomerProfileID = paymentAttemptRows.CustomerProfileID;
            string PaymentProfileID = paymentAttemptRows.PaymentProfileID;
            string shippingProfileID = paymentAttemptRows.ShippingProfileID;
            decimal BalanceDue = paymentAttemptRows.BalanceDue;
            decimal Amount = paymentAttemptRows.BalanceDue;            
            string customerEmail = paymentAttemptRows.Email;
            string customerDescription = paymentAttemptRows.PName;
            string Description = "AppTrack Payment Profile ID: " + PaymentProfileID;
            int responseCode = 1;
            string responseStatus = "Success";
            string paymentResult = "";
            
            string responseString = "";
            int returnStatusID = 1;
            /*
              Create Payment History Record, to be updated  with result later
            */
            ObjectParameter PaymentHistoryID = new ObjectParameter("PaymentHistoryID", typeof(int));
            db.LB_InsertPaymentHistory(CustID,
                OrderID,
                PaymentMethodID,
                Description,
                BalanceDue,
                PaymentHistoryID);

            var scalarPaymentHistoryID = (int)PaymentHistoryID.Value;

            //
            // **AUTHORIZE.NET
            //

            if (paymentAttemptRows.PaymentType == "CIM")
            {
                ServiceMode serviceMode = ServiceMode.Test;
                string apiLogin = Constants.apiLogin;
                string transactionKey = Constants.transactionKey;
                if (Constants.AuthorizeNetStatus == "Live")
                {
                    serviceMode = ServiceMode.Live;
                    apiLogin = Constants.apiLoginLive;
                    transactionKey = Constants.transactionKeyLive;
                }

                // Authoirize gateway return variables
                decimal authorizeAmount = 0.00M;
                bool authorizeApproved;
                string authorizeAuthorizationCode = "";
                string authorizeInvoiceNumber = "";
                string authorizeMessage = "";
                string authorizeResponseCode = "";
                string authorizeResponseReasonCode = "";
                string authorizePaymentTransactionID = "";


                CustomerGateway customerGateway = new CustomerGateway(apiLogin, transactionKey, serviceMode);

                try
                {
                    Order order = new Order(CustomerProfileID, PaymentProfileID, "");
                    order.Amount = Amount;
                    order.ShippingAddressProfileID = shippingProfileID;
                    order.InvoiceNumber = "R" + OrderID.ToString();
                    order.Description = customerDescription;
                    order.CustomerProfileID = CustomerProfileID;
                    order.PaymentProfileID = PaymentProfileID;

                    //                IGatewayResponse authorize = customerGateway.Authorize(CustomerProfileID, PaymentProfileID, Amount);
                    IGatewayResponse authorize = customerGateway.AuthorizeAndCapture(order);
                    authorizeAmount = authorize.Amount;
                    authorizeApproved = authorize.Approved;
                    authorizeAuthorizationCode = authorize.AuthorizationCode;
                    authorizeInvoiceNumber = authorize.InvoiceNumber;
                    authorizeMessage = authorize.Message;
                    authorizeResponseCode = authorize.ResponseCode;
                    authorizeResponseReasonCode = authorize.ResponseReasonCode;
                    authorizePaymentTransactionID = authorize.TransactionID;
                    ViewBag.Message = "Awesome";

                    if (authorizeApproved == true)
                    {
                        returnStatusID = 1;
                        responseCode = 1;
                        responseStatus = "Success";
                    }
                    else
                    {
                        returnStatusID = 3;
                        responseCode = -1;
                        responseStatus = "Declined";

                    }
                    responseString = responseStatus + "|" + authorizeAuthorizationCode + "|" + authorizeMessage + "|" + authorizeResponseCode + "|" + authorizeResponseReasonCode + "|" + authorizeInvoiceNumber;

                    db.LB_UpdatePaymentHistory(scalarPaymentHistoryID,
                    responseCode,
                    responseString,
                    responseStatus,
                    authorizePaymentTransactionID,
                    returnStatusID,
                    AdminID);

                    return responseString;
                }
                catch (Exception e)
                {
                    string s = e.Message;
                    authorizePaymentTransactionID = "1";
                    ViewBag.Message = "Ooops";
                    return "Failure";
                }
            }
            //
            // **PAYPAL
            //

            if (paymentAttemptRows.PaymentType == "Paypal")
            {
                // Populate with your Payflow credentials
                string payflowUser = "apiuser";
                string payflowVendor = "primaeyegroup";
                string payflowPartner = "PayPal";
                string payflowPassword = "Testing2@";

                // For live transactions, change this to payflowpro.paypal.com.
                PayflowConnectionData connect = new PayflowConnectionData(Constants.payflowConnectURL);

                // Set our Payflow credentials -- change this to your credentials
                UserInfo userInfo = new UserInfo(payflowUser, payflowVendor, payflowPartner, payflowPassword);

                try
                {
                    Currency amt = new Currency(Amount);

                    Invoice refInv = new Invoice();
                    refInv.Amt = amt;

                    /* We still have to pass a BaseTender object (or one derived from it).
                     * Since we're running a reference transaction against a credit card, the
                     * most appropriate thing to do would be to pass a CardTender object.
                     * And since a CardTender object requires a CreditCard object, we'll just
                     * construct a blank one to satisfy that requirement.
                     */
                    CreditCard refCard = new CreditCard("", "");
                    CardTender refTender = new CardTender(refCard);

                    /* Construct the ReferenceTransaction object.  Note that we're passing
                     * in the PNREF from the response from the previous transaction
                     * (response.TransactionResponse.Pnref).  Also note that the first parameter
                     * is the transaction type -- "S" for "sale", or "A" for "authorization".
                     */
                    ReferenceTransaction refTrx = new ReferenceTransaction("S", PaymentProfileID,
                        userInfo, connect, refInv, refTender, Guid.NewGuid().ToString());

                    ExtendData extData = new ExtendData("BUTTONSOURCE", "Your_BN_Code_Here");
                    refTrx.AddToExtendData(extData);

                    Response refResp = refTrx.SubmitTransaction();

                    //            Console.WriteLine("Request: " + refResp.RequestString);
                    //            Console.WriteLine("Response: " + refResp.ResponseString);

                    string returnTransactionID = "";
                    string paypalAuthorizationCode = "";

                    if (refResp.TransactionResponse.Result == 0)
                    {
                        returnStatusID = 1;
                        responseCode = 1;
                        responseStatus = "Success";

                        paypalAuthorizationCode = refResp.TransactionResponse.AuthCode;
                        returnTransactionID = refResp.TransactionResponse.Pnref;
                        responseString = refResp.TransactionResponse.RespMsg;

                        // Update C_PaymentMethod with new Transaction ID to use in future transaction
                        db.LB_UpdatePaymentMethodPaymentProfileID(PaymentMethodID, returnTransactionID);

                    }
                    else
                    {
                        returnStatusID = 3;
                        responseCode = -1;
                        responseStatus = "Declined";

                        responseString = refResp.TransactionResponse.RespMsg;
                        ModelState.AddModelError("", "Transaction failed: " + refResp.TransactionResponse.Result.ToString() + ": " + refResp.TransactionResponse.RespMsg);
                    }

                    responseString = responseStatus + "|" + returnTransactionID + "|" + paypalAuthorizationCode + "|" + responseString;

                    db.LB_UpdatePaymentHistory(scalarPaymentHistoryID,
                    responseCode,
                    responseString,
                    responseStatus,
                    returnTransactionID,
                    returnStatusID,
                    AdminID);

                    return responseString;
                }
                catch (Exception e)
                {
                    string s = e.Message;
                    ViewBag.Message = "Error encountered: " + s;
                    return "Failure";
                }
            }
            return "Invalid payment type";
        }

        [HttpGet]
        public string Refund(int id = 0, decimal refundAmount = 0)
        {
            int PaymentID = id;


            ServiceMode serviceMode = ServiceMode.Test;
            string apiLogin = Constants.apiLogin;
            string transactionKey = Constants.transactionKey;
            if (Constants.AuthorizeNetStatus == "Live")
            {
                serviceMode = ServiceMode.Live;
                apiLogin = Constants.apiLoginLive;
                transactionKey = Constants.transactionKeyLive;
            }

            var refundAttemptRows = db.Database.SqlQuery<PaymentAttempt>("exec dbo.[LB_GetPaymentAttemptCIMByPaymentID]  @PaymentID",
                 new SqlParameter("@PaymentID", PaymentID)
                 ).FirstOrDefault();

            int?     CustID = refundAttemptRows.CustID;
            int     PaymentMethodID = refundAttemptRows.PMID;
            string CustomerProfileID = refundAttemptRows.CustomerProfileID;
            string  PaymentProfileID = refundAttemptRows.PaymentProfileID;
            string  paymentTransactionID = refundAttemptRows.TransactionID;
            if (refundAmount == 0)
            {
                decimal Amount = refundAttemptRows.Amount;
                refundAmount = Amount;
            }
            string  customerEmail = refundAttemptRows.Email;
            string  customerDescription = refundAttemptRows.PName;
            string  Description = customerDescription ;
            int     responseCode = 1;
            string  responseStatus = "Success";
            string  paymentResult = "";
            string  responseString = "";
            int     returnStatusID = 1;

            if (refundAttemptRows.PaymentType == "CIM")
            {
                CustomerGateway customerGateway = new CustomerGateway(apiLogin, transactionKey, serviceMode);

                IGatewayResponse authorize = customerGateway.Refund(CustomerProfileID, PaymentProfileID, paymentTransactionID, refundAmount);

                paymentTransactionID = authorize.TransactionID;
                ViewBag.Message = "Awesome";
                // Authoirize gateway return variables
                decimal authorizeAmount = 0.00M;
                bool authorizeApproved;
                string authorizeAuthorizationCode = "";
                string authorizeInvoiceNumber = "";
                string authorizeMessage = "";
                string authorizeResponseCode = "";
                string authorizeResponseReasonCode = "";
                string authorizePaymentTransactionID = "";

                authorizeAmount = authorize.Amount;
                authorizeApproved = authorize.Approved;
                authorizeAuthorizationCode = authorize.AuthorizationCode;
                authorizeInvoiceNumber = authorize.InvoiceNumber;
                authorizeMessage = authorize.Message;
                authorizeResponseCode = authorize.ResponseCode;
                authorizeResponseReasonCode = authorize.ResponseReasonCode;
                authorizePaymentTransactionID = authorize.TransactionID;
                ViewBag.Message = "Awesome";

                if (authorizeApproved == true)
                {
                    returnStatusID = 1;
                    responseCode = 1;
                    responseStatus = "Success";

                }
                else
                {
                    returnStatusID = 3;
                    responseCode = -1;
                    responseStatus = "Failure";

                }
                responseString = responseStatus + "|" + authorizeAuthorizationCode + "|" + authorizeMessage + "|" + authorizeResponseCode + "|" + authorizeResponseReasonCode + "|" + authorizeInvoiceNumber;
                return responseString;

            }

            if (refundAttemptRows.PaymentType == "Paypal")
            {
                // Populate with your Payflow credentials
                string payflowUser = "apiuser";
                string payflowVendor = "primaeyegroup";
                string payflowPartner = "PayPal";
                string payflowPassword = "Testing2@";

                // For live transactions, change this to payflowpro.paypal.com.
                PayflowConnectionData connect = new PayflowConnectionData(Constants.payflowConnectURL);

                // Set our Payflow credentials -- change this to your credentials
                UserInfo userInfo = new UserInfo(payflowUser, payflowVendor, payflowPartner, payflowPassword);

                try
                {
                    Currency amt = new Currency(refundAmount);

                    Invoice refInv = new Invoice();
                    refInv.Amt = amt;

                    /* We still have to pass a BaseTender object (or one derived from it).
                        * Since we're running a reference transaction against a credit card, the
                        * most appropriate thing to do would be to pass a CardTender object.
                        * And since a CardTender object requires a CreditCard object, we'll just
                        * construct a blank one to satisfy that requirement.
                        */
                    CreditCard refCard = new CreditCard("", "");
                    CardTender refTender = new CardTender(refCard);

                    /* Construct the ReferenceTransaction object.  Note that we're passing
                        * in the PNREF from the response from the previous transaction
                        * (response.TransactionResponse.Pnref).  Also note that the first parameter
                        * is the transaction type -- "S" for "sale", or "A" for "authorization".
                        */
                    ReferenceTransaction refTrx = new ReferenceTransaction("C", paymentTransactionID,
                        userInfo, connect, refInv, refTender, Guid.NewGuid().ToString());

                    ExtendData extData = new ExtendData("BUTTONSOURCE", "Your_BN_Code_Here");
                    refTrx.AddToExtendData(extData);

                    Response refResp = refTrx.SubmitTransaction();

                    //            Console.WriteLine("Request: " + refResp.RequestString);
                    //            Console.WriteLine("Response: " + refResp.ResponseString);

                    string returnTransactionID = "";
                    string paypalAuthorizationCode = "";

                    if (refResp.TransactionResponse.Result == 0)
                    {
                        returnStatusID = 1;
                        responseCode = 1;
                        responseStatus = "Success";

                        paypalAuthorizationCode = refResp.TransactionResponse.AuthCode;
                        returnTransactionID = refResp.TransactionResponse.Pnref;
                        responseString = refResp.TransactionResponse.RespMsg;
                    }
                    else
                    {
                        returnStatusID = 3;
                        responseCode = -1;
                        responseStatus = "Declined";

                        responseString = refResp.TransactionResponse.RespMsg;
                        ModelState.AddModelError("", "Transaction failed: " + refResp.TransactionResponse.Result.ToString() + ": " + refResp.TransactionResponse.RespMsg);
                    }

                    responseString = responseStatus + "|" + returnTransactionID + "|" + paypalAuthorizationCode + "|" + responseString;

                    return responseString;
                }
                catch (Exception e)
                {
                    string s = e.Message;
                    ViewBag.Message = "Error encountered: " + s;
                    return "Failure";
                }
            }
            return "Invalid payment type";
        }


        [HttpGet]
        public ActionResult Void(int id = 0)
        {

            ServiceMode serviceMode = ServiceMode.Test;
            string apiLogin = Constants.apiLogin;
            string transactionKey = Constants.transactionKey;
            if (Constants.AuthorizeNetStatus == "Live")
            {
                serviceMode = ServiceMode.Live;
                apiLogin = Constants.apiLoginLive;
                transactionKey = Constants.transactionKeyLive;
            }

            string CustomerProfile = "36422314";
            string PaymentProfile = "32948822";
            string paymentTransactionID = "2238299945";
            string CustID = "148778";
            decimal Amount = 1.75M;

            CustomerGateway customerGateway = new CustomerGateway(apiLogin, transactionKey, serviceMode);
            try
            {
                IGatewayResponse authorize = customerGateway.Void(CustomerProfile, PaymentProfile, paymentTransactionID);
                paymentTransactionID = authorize.TransactionID;
                ViewBag.Message = "Awesome";
            }
            catch (Exception e)
            {
                string s = e.Message;
                paymentTransactionID = "1";
                ViewBag.Message = "Ooops";
            }

            ViewBag.CustomerProfile = CustomerProfile;
            ViewBag.PaymentProfile = PaymentProfile;

            return RedirectToAction("Index");
        }

        public void BatchEmailFailedPayments()
        {

            int emailId = Constants.emailIDFailedPayment;
            var emailRow = db.Database.SqlQuery<C_EmailTemplates>("exec dbo.[LB_GetEmailTemplate]  @ID",
            new SqlParameter("@ID", emailId)
            ).FirstOrDefault();
            string subject = emailRow.Subject;
            string body = emailRow.Content;
            string thisbody = emailRow.Content;

            var failedPaymentAttemptRows = db.Database.SqlQuery<FailedPaymentAttempt>("exec dbo.[LB_GetPaymentAttemptsCIMFailed]  @EmailFlag",
             new SqlParameter("@EmailFlag", 1 )
            ).ToList();

            foreach (var item in failedPaymentAttemptRows)
            {
                    string thisPCardNumber = item.CardNumber;
                    string thisEmail = item.Email;
                    thisbody = body.Replace("#PCARDNUMBER#", thisPCardNumber);
                    SmtpClient client = new SmtpClient();
                    MailMessage mailMessage = new MailMessage();
                    MailAddress FromAddress = new MailAddress("MemberServices@AppTrack.net", "AppTrack Member Services");
                    mailMessage.From = FromAddress;
                    mailMessage.To.Add("scott@motiongrid.com");
                    mailMessage.Subject = subject;
                    mailMessage.Body = thisbody;
                    mailMessage.IsBodyHtml = true;
                    client.Send(mailMessage);
                    //await client.SendMailAsync(mailMessage);
                
            }

            db.LB_UpdatePaymentHistoryEmailsSent();

            
            }

    }
}