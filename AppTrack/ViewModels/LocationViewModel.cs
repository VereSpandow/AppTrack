using AppTrack.SharedModels;
using AppTrack.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace AppTrack.ViewModels
{
    public class LocationListViewModel
    {
        public List<Location> LocationList { get; set; }

        [Display(Name = "Last Name")]
        [MaxLength(10)]
        public string SearchLastName { get; set; }

        [Display(Name = "Status")]
        public string SelectedStatus { get; set; }

        public IEnumerable<System.Web.Mvc.SelectListItem> StatusList { get; set; }

    }


    public class LocationViewModel : Location
    {
        public IEnumerable<System.Web.Mvc.SelectListItem> StateList { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> NameTitleList { get; set; }

        public string AddLocation { get; set; }

    }

    public class LocationProfileViewModel
    {
        public Location LocationRecord { get; set; }

        public List<CommissionHeader> CommissionHeaderList { get; set; }

        public IEnumerable<System.Web.Mvc.SelectListItem> statusList { get; set; }

        public int CustID { get; set; }

        public int ParentID { get; set; }

        [MaxLength(50)]
        public string cancelReasonCode { get; set; }

        public string Comments { get; set; }

        public int StatusID { get; set; }

        public string Status { get; set; }

        public IEnumerable<System.Web.Mvc.SelectListItem> CancelReasonCodesList { get; set; }

    }

}