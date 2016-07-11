using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AppTrack.SharedModels;
using AppTrack.Models;
using System.ComponentModel.DataAnnotations;
// Caution if we include System.Web.Mvc for use with SelectListItem, the Compare attribute becomes ambiguous as it is in 2 namespaces.
// So, just specify System.Web.Mvc for SelectListItem

namespace AppTrack.ViewModels
{
    public class MeetingViewModel
    {
        public List<MeetingEvent> MeetingEventList { get; set; }

        public MeetingEvent meetingEvent { get; set; }

        public IEnumerable<System.Web.Mvc.SelectListItem> StateList { get; set; }

        public IEnumerable<System.Web.Mvc.SelectListItem> StatusList { get; set; }

        [Required, Display(Name = "Time")]
        public int eventStartHour { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> TimeHourList { get; set; }

        [Required]
        public int eventStartMinute { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> TimeMinuteList { get; set; }

        public string eventStartAMPM { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> TimeAMPMList { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        public Nullable<System.DateTime> SearchStartDate { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        public Nullable<System.DateTime> SearchEndDate { get; set; }

        [Display(Name = "Member Director")]
        public int SearchCustID { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> SearchMemberDirectorList { get; set; }

        [Display(Name = "Topic")]
        [MaxLength(100)]
        public string SearchPhrase { get; set; }

        [Display(Name = "Status")]
        public string SearchStatus { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> SearchStatusList { get; set; }

        [Display(Name = "State")]
        public string SearchState { get; set; }

    }

    public partial class MeetingRegistration
    {
        public int EventID { get; set; }

        public int CustID { get; set; }

        [Display(Name = "Guest Of (Member ID)")]
        public int SponsorID { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        public Nullable<System.DateTime> RegistrationDate { get; set; }

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
        [Display(Name = "Position/Title")]
        [MaxLength(40)]
        public string JobTitle { get; set; }

        [Required]
        [Display(Name = "Email")]
        [MaxLength(100)]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Display(Name = "Practice Name")]
        [MaxLength(100)]
        public string SponsorName { get; set; }

        [Display(Name = "Phone")]
        [DataType(DataType.PhoneNumber)]
        public string Phone { get; set; }

        [Display(Name = "Are you an AppTrack a Member?")]
        public int Flag1 { get; set; }

        public int CustomerType { get; set; }

        public string Status { get; set; }

    }

    public partial class MeetingRegistrationViewModel
    {
        public MeetingRegistration meetingRegistration { get; set; }

        public MeetingEvent meetingEvent { get; set; }

        public IEnumerable<System.Web.Mvc.SelectListItem> NameTitleList { get; set; }

    }
    public class MeetingRegistrationList
    {
        public List<MeetingRegistration> meetingRegistrationList { get; set; }
    }

    public class MeetingRegistrationListViewModel
    {
        public List<MeetingRegistration> MeetingRegistrationList { get; set; }

        public MeetingEvent meetingEvent { get; set; }

        public MeetingRegistration meetingRegistration { get; set; }

        public IEnumerable<System.Web.Mvc.SelectListItem> NameTitleList { get; set; }
    }

    public partial class LocalMeetingList
    {
        public List<MeetingEvent> localMeetings { get; set; }

        public string SearchLastName { get; set; }
        public string SearchStatus { get; set; }
    }

    public partial class LocalMeetingListFromData
    {
        public List<C_EventHeader> C_EventHeaderList { get; set; }

        public string SearchLastName { get; set; }
        public string SearchStatus { get; set; }
    }



}