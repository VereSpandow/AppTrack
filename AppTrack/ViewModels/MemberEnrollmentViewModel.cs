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
    public class MemberEnrollmentViewModels
    {
    }

    public class MemberEnrollmentPrep
    {
        [Display(Name = "Member ID")]
        public int? CustID { get; set; }

        [Display(Name = "Event ID")]
        public int? EventID { get; set; }

        [Display(Name = "Sales Rep ID")]
        public int? SponsorID { get; set; }

        [Display(Name = "ItemID")]
        public int? ItemID { get; set; }

        [Display(Name = "Promotion")]
        [Required]
        public int? PromotionID { get; set; }

        [Display(Name = "IMD ID")]
        public int? SecSponsorID { get; set; }

        [Display(Name = "Is Member an IMD?")]
        public string IMDMember { get; set; }

        [Display(Name = "Source Meeting ID")]
        public int? SourceID { get; set; }

        [Display(Name = "Meeting Code")]
        public string SourceCode { get; set; }

        public IEnumerable<System.Web.Mvc.SelectListItem> PromotionList { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> ItemList { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> RepList { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> IMDList { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> EnrollmentMeetingList { get; set; }

    }
        
 
    public class MemberEnrollmentPrimary : Member
    {
        [Required, Display(Name = "Password")]
        [MaxLength(20)]
        [DataType(DataType.Password)]
        [MembershipPassword(ErrorMessage = "Password is not valid, must contain at least 6 characters and at least 1 uppercase letter, lowercase letter, number and a special character")]
        public string Password { get; set; }

        [Required, Display(Name = "Confirm Password")]
        [MaxLength(20)]
        [DataType(DataType.Password)]
        [Compare("Password")]
        public string ConfirmPassword { get; set; }


        [Display(Name = "Enrollment Item")]
        public int? ItemID { get; set; }

        [Display(Name = "Promotion")]
        public int? PromotionID { get; set; }

        public string PayeeSameAsPrimary { get; set; }
        [Required]
        public string Multilocation { get; set; }

        public IEnumerable<System.Web.Mvc.SelectListItem> StateList { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> NameTitleList { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> RepList { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> IMDList { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> EnrollmentMeetingList { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> promotionList { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> ItemList { get; set; }
        

    }

    public partial class MemberEnrollmentLocations : Location
    {

        [Display(Name = "PrimaryEmail")]
        [MaxLength(100)]
        [DataType(DataType.EmailAddress)]
        public string PrimaryEmail { get; set; }

        public string AddLocation { get; set; }
        public string PayeeSameAsLocation { get; set; }

        public IEnumerable<System.Web.Mvc.SelectListItem> StateList { get; set; }
    }

    public partial class MemberEnrollmentPaymentMethod : PaymentMethod
    {
        [Display(Name = "CVV Code")]
        [MinLength(2)]
        [MaxLength(4)]
        public string CardCode { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> CardTypeList { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> CardExpMonthList { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> CardExpYearList { get; set; }

    }

    public class MemberEnrollmentVendor
    {
        public int CategoryID { get; set; }
        public string CategoryName { get; set; }
        public int CustID { get; set; }
        public string DisplayName { get; set; }
    }

    public class MemberEnrollmentVendorSelect
    {
        public List<MemberEnrollmentVendor> MemberEnrollmentVendorList { get; set; }
        public int? CustID { get; set; }
    }

    public class MemberEnrollmentThankYou
    {
        public int? CustID { get; set; }
        public string DisplayName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string NameTitle { get; set; }
        public string Password { get; set; }
        public string DayPhone { get; set; }
        public string Email { get; set; }
        public string ThankYouMessage { get; set; }
        public decimal Volume1 { get; set; }
        public decimal Locations { get; set; }
        public int? CustomerQualificationLevel { get; set; }
    }

}