using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace AppTrack
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            // This next line allows pages to render in an iFrame, otherwise they are blocked
            // Removed this because it seems like it is ok to load in iFrame if from same origin (domain)
            // which is how we would always use this
            //AntiForgeryConfig.SuppressXFrameOptionsHeader = true;

        }
    }

    public static class Constants
    {
        // LIVE SETTINGS
        public const bool isLive = true;
        public const string siteDomain = "AppTrack.LoyaltyBenefits.com"; // Live
        public const string siteURL = "http://AppTrack.LoyaltyBenefits.com"; // Live
        public const string secureSiteURL = "http://AppTrack.LoyaltyBenefits.com"; // Live

        public const string notificationEmailTo = "vere@loyaltybenefits.com"; // Live
        public const string referralEmailTo = "vere@loyaltybenefits.com"; // Live
        public const string contactEmailTo = "vere@loyaltybenefits.com"; // Live
        public const string salesEmailTo = "vere@loyaltybenefits.com"; // Live

        public const string AuthorizeNetStatus = "Live";
        public const string secureProtocol = "http";
        public const string docuSignAccountEmail = "docusign@AppTrack.com";
        public const string docuSignAccountPassword = "Testing3#";

        public const string payflowConnectURL = "payflowpro.paypal.com";
        /*        
        // TEST SETTINGS
        public const bool isLive = false;
        public const string siteDomain = "idocbeta.loyaltybenefits.com"; 
        public const string siteURL = "http://idocbeta.loyaltybenefits.com";
        public const string secureSiteURL = "https://idocbeta.loyaltybenefits.com";

        public const string notificationEmailTo = "vere@loyaltybenefits.com";
        public const string referralEmailTo = "vere@loyaltybenefits.com";
        public const string contactEmailTo = "vere@loyaltybenefits.com"; // Live
        public const string salesEmailTo = "vere@loyaltybenefits.com"; // Live

        public const string AuthorizeNetStatus = "Test";
        public const string secureProtocol = "http";

        public const string docuSignAccountEmail = "vere@loyaltybenefits.com";
        public const string docuSignAccountPassword = "Testing2@";

        public const string payflowConnectURL = "pilot-payflowpro.paypal.com";
        */


        // BOTH LIVE AND TEST SETTINGS
        public const string companyName = "AppTrack";
        public const int pageSize = 25;

        public const string adminEmailFrom = "notifications@AppTrack.net"; // Live
        public const string memberEmailFrom = "memberservices@AppTrack.net"; // Live
        public const string vendorEmailFrom = "memberupdate@AppTrack.net"; // Live

        public const int emailIDNewMember = 1;
        public const int emailIDFailedPayment = 2;
        public const int emailIDNewMemberNotification = 3;
        public const int emailIDNewMemberNoPassword = 14;
        public const int emailIDContactMeReply = 4;
        public const int emailIDNewSelectMemberNotification = 5;
        public const int emailIDMeetingRegistrationNotification = 13;
        public const int emailIDContactMeNotification = 15;
        public const int emailIDMemberReferralNotification = 16;
        public const int emailIDMeetingUpdateNotification = 19;
        public const int emailIDPromotionalOfferNotification = 20;
        public const int emailIDMembershipAddNotification = 21;
        
        public const int emailSwitch = 1;  // 1 = Yes

        public const string adminRoles = "Accounting,Admin,Executive,Finance,Marketing,MemberServices,MemberServicesManager,SalesManager,SalesRep,VendorAdmin,Website";
        public const int orphanMeetingID = 10;
        public const int orphanSalesRepID = 30;
        public const int orphanIMDID = 40;
        public const int salesRepCustomerType = 3;
        public const int memberDirectorCustomerType = 4;
        public const int memberCustomerType = 6;
        public const int memberContactCustomerType = 61;
        public const int locationCustomerType = 66;
        public const int vendorCustomerType = 7;
        public const int vendorContactCustomerType = 71;
        public const int meetingAttendeeCustomerType = 98;
        public const int contractProviderCustomerType = 10;
        public const int statusLookupGroupID = 3;
        public const int noteStatusLookupGroupID = 4;
        public const int meetingStatusLookupGroupID = 5;
        public const int commissionStatusLookupGroupID = 6;
        public const int autoShipStatusLookupGroupID = 7;
        public const int vendorStatusLookupGroupID = 10;
        public const int vendorRebateLookupGroupID = 8;
        public const int SalesRepCommissionLookupGroupID = 11;
        public const int IMDCommissionLookupGroupID = 12;
        public const int fileStatusLookupGroupID = 13;
        public const int contractStatusLookupGroupID = 14;
        public const int contractTypeLookupGroupID = 15;
        public const int contractDetailTypeLookupGroupID = 16;
        public const int providerTypeLookupGroupID = 17;
        public const int activityStatusLookupGroupID = 18;
        public const int activityOnBoardingStatusLookupGroupID = 22;
        public const int activityTypeLookupGroupID = 19;
        public const int noteTypeLookupGroupID = 20;
        public const int commDirectionLookupGroupID = 21;
        public const int activityCategoryGroupID = 9;
        public const int activityPrimaOnBoardingCategoryID = 38;
        public const int activityAppTrackOnBoardingCategoryID = 42;
        public const int IMDPricingLevel = 3;
        public const int memberRebateCommissionID = 70;
        public const int corporateRebateCommissionID = 60;
        public const int IMDMemberEnrollmentCommissionID = 40;
        public const int IMDSixMonthMemberCommissionID = 41;
        public const int IMDMeetingCommissionID = 45;
        public const int rebateCommissionID = 70;
        public const int rebateCommissionPeriodType = 4;
        public const int memberRebateVolumeType = 4;
        public const int rebateVolumeType = 4;

        public const int stateRegionTypeID = 200;
        public const int memberStoreID = 10;
        public const int AppTrackStoreID = 10;
        public const int PRIMAStoreID = 15;
        public const int PRIMAItemID = 24;
        public const int fatalErrorCode = -1;
        public const string DocumentFolderPath = "~/Documents/Vendor/";
        public const string ContractDocumentFolderPath = "~/App_Data/Contract/";
        public const int categoryStudyGroupMeeting = 10;
        public const int categoryRegionalConference = 9;
        public const int categoryNationalConference = 8;
        public const int IMDEnrollmentPromotionID = 7;

        public const int weeklyPeriodTypeID = 1;
        public const int monthlyPeriodTypeID = 2;
        public const int quarterlyPeriodTypeID = 4;
        public const int yearlyPeriodTypeID = 12;
        public const int maxPaymentBatchSize = 10;

        public const string isShippingProfileRequired = "No"; 
        public const string isAuthorizeOn = "No";
        public const string apiLogin = "3eXFLw56zB";
        public const string transactionKey = "737ZM3MRg4B45zjm";
        public const string apiLoginLiveMG = "Motion972";
        public const string transactionKeyLiveMG = "5jKS4X5j6DDQ9x8d";
        public const string apiLoginLive = "2nRA67m596";
        public const string transactionKeyLive = "755V7JywYZt548Uv";
        public const string emailTemplate = @"<!doctype html><html><head><meta charset=utf-8><title>Email notice from AppTrack</title></head><body style=""margin:0;"">
        <table width=100% height:100% border=0 cellspacing=0 cellpadding=0>
        <tr><td>
        <table width=600 border=0 cellspacing=0 cellpadding=0 align=center style=""font-family:Helvetica, Arial, sans-serif;color:#333;"">
        <tr><td colspan=3><img src=""#SITEURL#/Images/Emailheader.jpg"" style=""vertical-align: bottom;""></td></tr>
        <tr><td width=20></td><td><p>#BODYCOPY#</p></td><td width=20></td></tr>
        <tr><td colspan=3><img src=""#SITEURL#/Images/Emailfooter.jpg"" style=""vertical-align: bottom;""></td></tr></table>
        </td></tr>
        </table>
        </body></html>";
    }
}
