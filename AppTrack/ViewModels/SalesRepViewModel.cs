using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Security;
using AppTrack.SharedModels;
using System;
// Caution if we include System.Web.Mvc for use with SelectListItem, the Compare attribute becomes ambiguous as it is in 2 namespaces.
// So, just specify System.Web.Mvc for SelectListItem

namespace AppTrack.ViewModels
{
    public class SalesRepListViewModel
    {
        public List<SalesRep> SalesRepList { get; set; }
        
        [Display(Name = "Last Name")]
        [MaxLength(50)]
        public string SearchLastName { get; set; }

        [Display(Name = "Status")]

        public string SelectedStatus { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> StatusList { get; set; }
    }
    
    // Used to Create and Edit Sales Reps
    public class SalesRepViewModel
    {
        [Display(Name = "Sales RepID")]
        public int CustID { get; set; }

        [Display(Name = "SalesForceID")]
        [MaxLength(18)]
        public string SalesForceID { get; set; }

        [Display(Name = "Tax ID")]
        [MinLength(9)]
        [MaxLength(11)]
        [RegularExpression(@"^\d{9}|\d{3}-\d{2}-\d{4}|\d{2}-\d{7}$", ErrorMessage = "Invalid Tax ID Number")]
        public string TaxID { get; set; }

        [Required, Display(Name = "First Name")]
        [MaxLength(50)]
        public string FirstName { get; set; }

        [Required, Display(Name = "Last Name")]
        [MaxLength(50)]
        public string LastName { get; set; }

        [Display(Name = "Display Name")]
        [MaxLength(50)]
        public string DisplayName { get; set; }

        [Required,Display(Name = "Email*")]
        [MaxLength(100)]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Display(Name = "Status")]
        [MaxLength(50)]
        public string Status { get; set; }

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

        [Display(Name = "StatusID")]
        public int StatusID { get; set; }

        [Display(Name = "AdminID")]
        public int  AdminID { get; set; }
    }
    public class SalesRepMember : Member
    {
        [Display(Name = "Sales RepID")]
        public int SalesRepID { get; set; }

        [Required, Display(Name = "First Name")]
        [MaxLength(50)]
        public string SalesRepFirstName { get; set; }

        [Required, Display(Name = "Last Name")]
        [MaxLength(50)]
        public string SalesRepLastName { get; set; }

        public string MemberStatus { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public Nullable<System.DateTime> MemberStatusDate { get; set; }

        public string PeriodName { get; set; }

    }

    public class SalesRepMemberActivityViewModel
    {
        [Display(Name = "Start Period")]
        public int StartPeriodID { get; set; }

        [Display(Name = "End Period")]
        public int EndPeriodID { get; set; }

        public IEnumerable<System.Web.Mvc.SelectListItem> PeriodList { get; set; }

        [Display(Name = "Sales Rep ID")]
        public int SelectedSalesRepID { get; set; }

        public IEnumerable<System.Web.Mvc.SelectListItem> SalesRepList { get; set; }

        public List<SalesRepMember> SalesRepMemberList { get; set; }
    }

}