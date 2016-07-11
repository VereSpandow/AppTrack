using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using AppTrack.SharedModels;
using System.Web.WebPages.Html;
using JQueryUIHelpers;
using AppTrack.Models;

namespace AppTrack.ViewModels
{
    public class MemberActivity : C_ActivityTracking
    {
        public string PracticeName { get; set; }
        public string CategoryName { get; set; }
        public string VendorName { get; set; }
        public string AdminName { get; set; }

    }

    public class MemberActivityListViewModel
    {

        public List<MemberActivity> MemberActivityList { get; set; }

        public int SearchActivityID { get; set; }
        public int SearchCustID { get; set; }
        public int SearchOwnerID { get; set; }
        public int SearchAssignedToID { get; set; }
        public int SearchVendorID { get; set; }
        [Display(Name = "Activity Type")]
        public int SearchCategoryID { get; set; }

        public int SearchStoreID { get; set; }

        public string ScheduledDateOption { get; set; }
        public string CompletedDateOption { get; set; }

        public string SearchActivityTitle { get; set; }

        [Display(Name = "Added from")]
        [DataType(DataType.Date)]
        public Nullable<System.DateTime> SearchStartDate { get; set; }

        [Display(Name = "Added to")]
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

        [Display(Name = "Status")]
        public string SearchActivityStatus { get; set; }

        public IEnumerable<System.Web.Mvc.SelectListItem> SearchCategoryList { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> SearchActivityTypeList { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> SearchActivityVendorList { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> SearchActivityStatusList { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> SearchAssignedToList { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> SearchStoreList { get; set; }

    }

    public class MemberActivityDetail
    {
        public MemberActivity MemberActivity { get; set; }

        public List<CustomerNote> MemberActivityNoteList { get; set; }
    }

    class MemberActivityUpdateViewModelMetaData
    {
        [Required]
        public string Title { get; set; }
    }

    [MetadataType(typeof(MemberActivityUpdateViewModelMetaData))]
    public class MemberActivityUpdateViewModel : C_ActivityTracking
    {
        public int SearchActivityID { get; set; }
        public int SearchCustID { get; set; }
        public int SearchOwnerID { get; set; }
        public int SearchAssignedToID { get; set; }
        public int SearchVendorID { get; set; }
        public int SearchCategoryID { get; set; }

        [DataType(DataType.Date)]
        public Nullable<System.DateTime> SearchScheduledStartDate { get; set; }

        [DataType(DataType.Date)]
        public Nullable<System.DateTime> SearchScheduledEndDate { get; set; }

        [DataType(DataType.Date)]
        public Nullable<System.DateTime> SearchCompletedStartDate { get; set; }

        [DataType(DataType.Date)]
        public Nullable<System.DateTime> SearchCompletedEndDate { get; set; }

        public string SearchActivityStatus { get; set; }

        public IEnumerable<System.Web.Mvc.SelectListItem> EditCategoryList { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> EditActivityTypeList { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> EditActivityVendorList { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> EditActivityStatusList { get; set; }
    }

    public class MemberActivitySummary
    {
        public string AdminName { get; set; }

        public int ActivityCount { get; set; }
    }

    public class MemberActivitySummaryViewModel
    {

        public List<MemberActivitySummary> MemberActivitySummaryList { get; set; }

        public int SearchCategoryID { get; set; }

        [DataType(DataType.Date)]
        public Nullable<System.DateTime> SearchScheduledStartDate { get; set; }

        [DataType(DataType.Date)]
        public Nullable<System.DateTime> SearchScheduledEndDate { get; set; }

        [DataType(DataType.Date)]
        public Nullable<System.DateTime> SearchCompletedStartDate { get; set; }

        [DataType(DataType.Date)]
        public Nullable<System.DateTime> SearchCompletedEndDate { get; set; }

        public string SearchActivityStatus { get; set; }

        public IEnumerable<System.Web.Mvc.SelectListItem> EditCategoryList { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> SearchActivityStatusList { get; set; }
    }

}