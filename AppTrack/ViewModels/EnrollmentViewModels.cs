using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using AppTrack.SharedModels;
using AppTrack.Models;
using System.Web.Security;

namespace AppTrack.ViewModels
{
    public class EnrollmentViewModels
    {
    }

    public partial class EnrollmentPrimary
     {
            public int RepID { get; set; }
            public int CustID { get; set; }
            public int EventID { get; set; }
            public int ParentID { get; set; }
            public int SponsorID { get; set; }
            [Display(Name = "Practice Name")]
            [MaxLength(100)]
            public string DisplayName { get; set; }
            [Display(Name = "Title")]
            [MaxLength(5)]
            public string NameTitle { get; set; }
            [Required]
            [Display(Name = "First Name")]
            [MaxLength(40)]
            public string FirstName { get; set; }
            [Required]
            [Display(Name = "Last Name")]
            [MaxLength(40)]
            public string LastName { get; set; }
            [Required]
            [Display(Name = "Email")]
            [MaxLength(100)]
            [DataType(DataType.EmailAddress)]
            public string Email { get; set; }
            [Display(Name = "Street Address")]
            [MaxLength(40)]
            public string Address1 { get; set; }
            [Display(Name = "Address Line 2")]
            [MaxLength(40)]
            public string Address2 { get; set; }
            [Display(Name = "City")]
            [MaxLength(40)]
            public string City { get; set; }
            [Display(Name = "State")]
            [MaxLength(2)]
            public string State { get; set; }
            [RegularExpression(@"(^(?!0{5})(\d{5})(?!-?0{4})(-?\d{4})?$)", ErrorMessage = "Improperly formatted zip code.  It must be entered as nnnnn or nnnnn-nnnnn.")]
            [Display(Name = "Postal Code")]
            [MaxLength(10)]
            [DataType(DataType.PostalCode)]
            public string PostalCode { get; set; }
            [Display(Name = "Phone")]
            [DataType(DataType.PhoneNumber)]
            public string Phone { get; set; }
            [Display(Name = "Fax")]
            [DataType(DataType.PhoneNumber)]
            public string Fax { get; set; }

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

            public string Multilocation { get; set; }

            public IEnumerable<System.Web.Mvc.SelectListItem> StateList { get; set; }
            public IEnumerable<System.Web.Mvc.SelectListItem> NameTitleList { get; set; }

    }

    public partial class EnrollmentLocations
    {
        public int RepID { get; set; }
        public int EventID { get; set; }
        public int ParentID { get; set; }

        [Required]
        [Display(Name = "Practice Name")]
        [MaxLength(100)]
        public string DisplayName { get; set; }
        [Required]
        [Display(Name = "First Name")]
        [MaxLength(40)]
        public string FirstName { get; set; }
        [Required]
        [Display(Name = "Last Name")]
        [MaxLength(40)]
        public string LastName { get; set; }
        [Required]
        [Display(Name = "Email")]
        [MaxLength(100)]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        [Required]
        [Display(Name = "Street Address")]
        [MaxLength(40)]
        public string Address1 { get; set; }
        [Display(Name = "Address Line 2")]
        [MaxLength(40)]
        public string Address2 { get; set; }
        [Required]
        [Display(Name = "City")]
        [MaxLength(40)]
        public string City { get; set; }
        [Required]
        [Display(Name = "State")]
        [MaxLength(2)]
        public string State { get; set; }
        [Required]
        [RegularExpression(@"(^(?!0{5})(\d{5})(?!-?0{4})(-?\d{4})?$)", ErrorMessage = "Improperly formatted zip code.  It must be entered as nnnnn or nnnnn-nnnnn.")]
        [Display(Name = "Postal Code")]
        [MaxLength(10)]
        [DataType(DataType.PostalCode)]
        public string PostalCode { get; set; }
        [Required]
        [Display(Name = "Phone")]
        [DataType(DataType.PhoneNumber)]
        public string Phone { get; set; }
        [Display(Name = "Fax")]
        [DataType(DataType.PhoneNumber)]
        public string Fax { get; set; }
        [Display(Name = "PrimaryEmail")]
        [MaxLength(100)]
        [DataType(DataType.EmailAddress)]
        public string PrimaryEmail { get; set; }
        public string AddLocation { get; set; }

        public IEnumerable<System.Web.Mvc.SelectListItem> StateList { get; set; }
    
    }

    public partial class EnrollmentPaymentMethod
    {
        public int CustID { get; set; }

        [Required]
        [Display(Name = "Card Holder Name")]
        [MaxLength(100)]
        public string PName { get; set; }

        [Required]
        [Display(Name = "Card Type")]
        [MaxLength(10)]
        public string PCardType { get; set; }

        [Required]
        [Display(Name = "Card Number")]
        [MaxLength(16)]
        [DataType(DataType.CreditCard)]
        public string PCCNumber { get; set; }

        [Required]
        [RegularExpression(@"^(1[0-2]|0[1-9])$", ErrorMessage = "Please select the expiration month")]
        [Display(Name = "Exp. Month)")]
        [MaxLength(2)]
        public string PExpMonth { get; set; }

        [Required]
        [RegularExpression(@"^\d{4}", ErrorMessage = "Please select the expiration year")]
        [Range(2015,2050)]
        [Display(Name = "Exp. Year")]
        [MaxLength(4)]
        public string PExpYear { get; set; }

        [Required]
        [RegularExpression(@"^\d{3,4}", ErrorMessage = "Please enter a 3 or 4 digits")]
        [Display(Name = "CVV Code")]
        public string CardCode { get; set; }

        [Required]
        [Display(Name = "Agreement")]
        public bool Agreement { get; set; }

        public IEnumerable<System.Web.Mvc.SelectListItem> CardTypeList { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> CardExpMonthList { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> CardExpYearList { get; set; }

        public string CustomerProfile { get; set; }
        public string PaymentProfile { get; set; }

    }

    public  class EnrollmentVendor
    {
        public int CategoryID { get; set; }
        public string CategoryName { get; set; }
        public int CustID { get; set; }
        public string DisplayName { get; set; }
    }

    public class EnrollmentVendorSelect
    {
        public List<EnrollmentVendor> EnrollmentVendorList  { get; set; }
        public int CustID { get; set; }
    }

    public class EnrollmentFinal
    {
        public int CustID { get; set; }
        public int SponsorID { get; set; }
        [Display(Name = "Referring IMD")]
        public int ParentID { get; set; }
        public String PracticeSize { get; set; }

        [Required]
        [Display(Name = "StudyGroup")]
        public Boolean StudyGroup { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> IMDList { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> PracticeSizeList { get; set; }

    }
}