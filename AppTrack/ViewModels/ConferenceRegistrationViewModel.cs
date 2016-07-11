using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using AppTrack.Models;

namespace AppTrack.ViewModels
{
    public class ConferenceRegistrationViewModel
    {
            [Display(Name = "Event ID")]
            public int? EventID { get; set; }

            [Display(Name = "Member Director ID")]
            public int? CustID { get; set; }

            [Display(Name = "Member ID")]
            [RegularExpression("([1-9][0-9]*)", ErrorMessage = "Please enter a valid Member ID value")]
            public int? MemberID { get; set; }

            [Display(Name = "Title")]
            [MaxLength(10)]
            public string NameTitle { get; set; }

            [Required, Display(Name = "First Name")]
            [MaxLength(50)]
            public string FirstName { get; set; }

            [Required, Display(Name = "Last Name")]
            [MaxLength(50)]
            public string LastName { get; set; }

            [Display(Name = "Practice Name")]
            [MaxLength(50)]
            public string DisplayName { get; set; }

            [Required, Display(Name = "Address Line 1")]
            [MaxLength(50)]
            public string Address1 { get; set; }

            [Display(Name = "Address Line 2")]
            [MaxLength(50)]
            public string Address2 { get; set; }

            [Required]
            [MaxLength(50)]
            public string City { get; set; }

            [Required]
            [MaxLength(50)]
            public string State { get; set; }

            [Required, Display(Name = "Zip Code")]
            [DataType(DataType.PostalCode)]
            public string PostalCode { get; set; }

            [Required, Display(Name = "Email")]
            [MaxLength(100)]
            public string Email { get; set; }

            [Required, Display(Name = "Confirm Email")]
            [MaxLength(100)]
            [Compare("Email")]
            public string EmailConfirm { get; set; }

            [Display(Name = "Contact Phone")]
            [DataType(DataType.PhoneNumber)]
            public string DayPhone { get; set; }

            [Display(Name = "How many staff are attending?")]
            public string StaffAttendees { get; set; }

            [Display(Name = "Which Days are you attending?")]
            public int DaysAttending { get; set; }

            [Display(Name = "Status")]
            [MaxLength(50)]
            public string Status { get; set; }

            public Nullable<System.DateTime> StatusDate { get; set; }

            public Nullable<System.DateTime> StartDate { get; set; }

            [Display(Name = "StatusID")]
            public int StatusID { get; set; }

            [Display(Name = "AdminID")]
            public int AdminID { get; set; }

            public IEnumerable<System.Web.Mvc.SelectListItem> StaffAttendeeList { get; set; }
        
    }
}
