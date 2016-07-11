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
    public class MemberDirectorListViewModel
    {
        public List<MemberDirector> MemberDirectorList { get; set; }

        [Display(Name = "TIN Name")]
        [MaxLength(10)]
        public string SearchCompanyName { get; set; }

        [Display(Name = "Display Name")]
        [MaxLength(10)]
        public string SearchDisplayName { get; set; }

        [Display(Name = "Status")]

        public string SelectedStatus { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> StatusList { get; set; }
    }

    public class MemberDirectorViewModel : MemberDirector
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

        public IEnumerable<System.Web.Mvc.SelectListItem> StateList { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> NameTitleList { get; set; }
        
    }

    public class MemberDirectorProfileViewModel
    {
        public MemberDirector MemberDirectorRecord { get; set; }

        public List<MeetingEvent> MeetingEventList { get; set; }

        public List<CommissionHeader> CommissionHeaderList { get; set; }

        public List<CustomerBasic> MemberList { get; set; }

    }

    public class MemberDirectorMemberListViewModel
    {
        public int CustID { get; set; }

        public List<Member> MemberList { get; set; }

        [Display(Name = "Practice Name")]
        [MaxLength(10)]
        public string SearchDisplayName { get; set; }

        [Display(Name = "Last Name")]
        [MaxLength(10)]
        public string SearchLastName { get; set; }

        [Display(Name = "Status")]

        public string SelectedStatus { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> StatusList { get; set; }
    }

}