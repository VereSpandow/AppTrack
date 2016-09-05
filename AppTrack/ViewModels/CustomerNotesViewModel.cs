using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using AppTrack.SharedModels;
using System.Web.WebPages.Html;
using JQueryUIHelpers;

namespace AppTrack.ViewModels
{
    public class CustomerNoteListViewModel
    {

            public List<CustomerNote>CustomerNoteList { get; set; }

            public int SearchActivityID { get; set; }

            [Display(Name = "Member ID")]
            public int SearchCustID { get; set; }

            [Display(Name = "Vendor ID")]
            public int SearchVendorID { get; set; }

            [Display(Name = "Assigned To")]
            public int SearchAssignedToID { get; set; }
            public IEnumerable<System.Web.Mvc.SelectListItem> SearchAssignedToList { get; set; }

            [Display(Name = "Start Date")]
            [DataType(DataType.Date)]
            public Nullable<System.DateTime> SearchStartDate { get; set; }

            [Display(Name = "End Date")]
            [DataType(DataType.Date)]
            public Nullable<System.DateTime> SearchEndDate { get; set; }

            [Display(Name = "Scheduled from")]
            [DataType(DataType.Date)]
            public Nullable<System.DateTime> SearchScheduledStartDate { get; set; }

            [Display(Name = "Scheduled to")]
            [DataType(DataType.Date)]
            public Nullable<System.DateTime> SearchScheduledEndDate { get; set; }

            [Display(Name = "Completed from")]
            [DataType(DataType.Date)]
            public Nullable<System.DateTime> SearchCompletedStartDate { get; set; }

            [Display(Name = "Completed to")]
            [DataType(DataType.Date)]
            public Nullable<System.DateTime> SearchCompletedEndDate { get; set; }

            [Display(Name = "Type")]
            public string SearchNoteType { get; set; }
            public IEnumerable<System.Web.Mvc.SelectListItem> SearchNoteTypeList { get; set; }

            [Display(Name = "Contact Type")]
            public string SearchCommType { get; set; }
            public IEnumerable<System.Web.Mvc.SelectListItem> SearchCommTypeList { get; set; }

            [Display(Name = "Contact Dir")]
            public string SearchCommDirection { get; set; }
            public IEnumerable<System.Web.Mvc.SelectListItem> SearchCommDirectionList { get; set; }

            [Display(Name = "Search Phrase")]
            [MaxLength(20)]
            public string SearchPhrase { get; set; }

            [Display(Name = "Status")]
            public string SearchNoteStatus { get; set; }
            public IEnumerable<System.Web.Mvc.SelectListItem> SearchNoteStatusList { get; set; }

    }

    public class CustomerNoteUpdateViewModel : CustomerNote
    {

        public int SearchActivityID { get; set; }

        public int SearchCustID { get; set; }

        public int SearchVendorID { get; set; }

        public int SearchAssignedToID { get; set; }

        [DataType(DataType.Date)]
        public Nullable<System.DateTime> SearchStartDate { get; set; }

        [DataType(DataType.Date)]
        public Nullable<System.DateTime> SearchEndDate { get; set; }

        [DataType(DataType.Date)]
        public Nullable<System.DateTime> SearchScheduledStartDate { get; set; }

        [DataType(DataType.Date)]
        public Nullable<System.DateTime> SearchScheduledEndDate { get; set; }

        [DataType(DataType.Date)]
        public Nullable<System.DateTime> SearchCompletedStartDate { get; set; }

        [DataType(DataType.Date)]
        public Nullable<System.DateTime> SearchCompletedEndDate { get; set; }

        public string SearchNoteType { get; set; }

        public string SearchCommType { get; set; }

        public string SearchCommDirection { get; set; }

        public string SearchPhrase { get; set; }

        public string SearchNoteStatus { get; set; }

        public IEnumerable<System.Web.Mvc.SelectListItem> EditNoteTypeList { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> EditCommTypeList { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> EditCommDirectionList { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> EditNoteStatusList { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> EditAssignedToList { get; set; }
    }

}