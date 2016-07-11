using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Security;
using AppTrack.SharedModels;
using AppTrack.Models;
// Caution if we include System.Web.Mvc for use with SelectListItem, the Compare attribute becomes ambiguous as it is in 2 namespaces.
// So, just specify System.Web.Mvc for SelectListItem

namespace AppTrack.ViewModels
{
    public class MemberListViewModel
    {
        public List<Member> MemberList { get; set; }

        [Display(Name = "Member ID")]
        [MaxLength(10)]
        public string SearchCustID { get; set; }

        [Display(Name = "Last Name")]
        [MaxLength(25)]
        public string SearchLastName { get; set; }

        [Display(Name = "Practice Name")]
        [MaxLength(50)]
        public string SearchDisplayName { get; set; }

        [Display(Name = "TIN Name")]
        [MaxLength(50)]
        public string SearchCompany { get; set; }

        [Display(Name = "Address Line 1")]
        [MaxLength(50)]
        public string SearchAddress1 { get; set; }

        [Display(Name = "City")]
        [MaxLength(50)]
        public string SearchCity { get; set; }

        [Display(Name = "State")]
        [MaxLength(50)]
        public string SearchState { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> StateList { get; set; }

        [Display(Name = "Zip Code")]
        [DataType(DataType.PostalCode)]
        public string SearchPostalCode { get; set; }

        [Display(Name = "Phone")]
        [DataType(DataType.PhoneNumber)]
        public string SearchPhone { get; set; }

        [Display(Name = "Email")]
        [MaxLength(100)]
        public string SearchEmail { get; set; }

        [Display(Name = "Account Manager")]
        public int SearchAccountManagerID { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> AccountManagerList { get; set; }

        [Display(Name = "Status")]

        public string SelectedStatus { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> StatusList { get; set; }
    }

    public class MemberViewModel : Member
    {

        public IEnumerable<System.Web.Mvc.SelectListItem> StateList { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> NameTitleList { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> StatusList { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> BoardingStatusList { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> PracticeSizeList { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> CancelReasonCodesList { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> PracticeManagementSoftwareList { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> RepList { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> IMDList { get; set; }
        [Display(Name = "Account Manager")]
        public int UplineID { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> AccountManagerList { get; set; }

    }

    public class MemberProfileViewModel 
    {
        public Member MemberRecord { get; set; }

        public AutoshipBasic AutoshipBasicRecord { get; set; }

        public List<AutoshipBasic> AutoshipBasicList { get; set; }

        public List<CommissionHeader> CommissionHeaderList { get; set; }

        public List<OrderBasic> OrderBasicList { get; set; }

        [Display(Name = "Comments")]
        public String Comments { get; set; }

        [MaxLength(50)]
        public string cancelReasonCode { get; set; }

        public IEnumerable<System.Web.Mvc.SelectListItem> CancelReasonCodesList { get; set; }

        public IEnumerable<System.Web.Mvc.SelectListItem> statusList { get; set; }

        [MaxLength(50)]
        public string Status { get; set; }

        [MaxLength(50)]
        public string OriginaStatus { get; set; }

        public int? CustID { get; set; }
        public int AdminID { get; set; }

    }

    public class MemberStatusViewModel
    {

        [Display(Name = "Comments")]
        public String Comments { get; set; }

        [MaxLength(50)]
        public string cancelReasonCode { get; set; }

        [MaxLength(50)]
        public string Status { get; set; }

        [MaxLength(50)]
        public string OriginaStatus { get; set; }

        public int StatusID { get; set; }
        public int CustID { get; set; }
        public int AdminID { get; set; }

    }

    public class MemberContactViewModel 
    {
        public Contact contactRecord { get; set; }

        public IEnumerable<System.Web.Mvc.SelectListItem> ContactTypeList { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> NameTitleList { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> StateList { get; set; }

    }

    public partial class MemberPaymentMethodViewModel : PaymentMethod
    {
        [Display(Name = "CVV Code")]
        [MinLength(2)]
        [MaxLength(4)]
        public string CardCode { get; set; }
        public PaymentMethod currentPaymentMethod { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> CardTypeList { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> CardExpMonthList { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> CardExpYearList { get; set; }

    }

    public class MemberVendorViewModel
    {
        public List<MemberVendor> MemberVendorList { get; set; }
        public int? CustID { get; set; }
    }

    public class MemberVendorRequirement
    {
        public int? ID { get; set; }
        public int? RequirementID { get; set; }
        public string RequirementType { get; set; }
        public int? CustID { get; set; }
        public int? Vendor { get; set; }
        public string RequirementName { get; set; }
        public string Status { get; set; }
        public Nullable<System.DateTime> StatusDate { get; set; }
        public int? DocumentID { get; set; }
        public string Path { get; set; }
        public string FileName { get; set; }
        public string DocumentName { get; set; }
        public string TemplateID { get; set; }
    }

    public class MemberVendorRequirementViewModel
    {
        public List<MemberVendorRequirement> MemberVendorRequirementList { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> MemberVendorRequirementStatusList { get; set; }
        public int? CustID { get; set; }
        public int? VendorID { get; set; }
    }


    public partial class MemberRebatePayeeViewModel: RebatePayee 
    {
        public IEnumerable<System.Web.Mvc.SelectListItem> StateList { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> NameTitleList { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> VendorPayeeList { get; set; }
     }

    public class MemberProgramItemPromotion
    {
        [Display(Name = "Member ID")]
        public int CustID { get; set; }
        [Display(Name = "ID")]
        public int ID { get; set; }
        [Display(Name = "Item ID")]
        public int ItemID { get; set; }
        [Display(Name = "Promotion ID")]
        public int PromotionID { get; set; }
        [Display(Name = "Promotion Title")]
        public string PromotionTitle { get; set; }
        public Nullable<int> StatusID { get; set; }
    }


    public partial class MemberProgramViewModel: MemberProgramBasic
    {
        public IEnumerable<System.Web.Mvc.SelectListItem> ProgramList { get; set; }
        public int? PromotionID { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> PromotionList { get; set; }
        public int? AdminID { get; set; }
        public int? IsIdoc { get; set; }
        public int? IsPrima { get; set; }
    }

    public partial class MemberReviewViewModel :  MemberReview
    {
        public IEnumerable<System.Web.Mvc.SelectListItem> ReviewReasonList { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> ReviewStatusList { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> ReviewOutcomeList { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> ReviewOutcomeReasonList { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> ChangeAllianceList { get; set; }

    }

    public class MembershipActivityList : C_MembershipActivity
    {
        public string StoreName { get; set; }
        public string DepartmentName { get; set; }
    }

    public class MembershipActivityViewModel
    {
        public List<MembershipActivityList> MembershipActivityList { get; set; }
    }
}