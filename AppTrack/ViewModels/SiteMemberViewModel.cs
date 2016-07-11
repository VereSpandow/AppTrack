using AppTrack.SharedModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace AppTrack.ViewModels
{
    public class SiteMemberViewModel
    {
    }

    public class SiteMemberEditViewModel: Member
    {
        [Display(Name = "Member/Location ID")]
        [RegularExpression("([1-9][0-9]*)", ErrorMessage = "Please enter a valid Member/Location ID value")]
        public int? CustID { get; set; }

        [Display(Name = "Payee ID")]
        [RegularExpression("([1-9][0-9]*)", ErrorMessage = "Please enter a valid Payee ID value")]
        public int? PayoutCustID { get; set; }

        [Display(Name = "First Name")]
        [MaxLength(50)]
        public string ShipFirstName { get; set; }

        [Display(Name = "Last Name")]
        [MaxLength(50)]
        public string ShipLastName { get; set; }

        [Display(Name = "Payee Name")]
        [MaxLength(50)]
        public string ShipName { get; set; }

        [Required, Display(Name = "Address Line 1")]
        [MaxLength(50)]
        public string ShipAddress1 { get; set; }

        [Display(Name = "Address Line 2")]
        [MaxLength(50)]
        public string ShipAddress2 { get; set; }

        [Required, Display(Name = " Ship City")]
        [MaxLength(50)]
        public string ShipCity { get; set; }

        [Required, Display(Name = "State")]
        [MaxLength(50)]
        public string ShipState { get; set; }

        [Required, Display(Name = "Postal Code")]
        [DataType(DataType.PostalCode)]
        public string ShipPostalCode { get; set; }

        [Display(Name = "Payee Email")]
        [MaxLength(100)]
        public string ShipEmail { get; set; }

        [Display(Name = "Payee Phone")]
        [DataType(DataType.PhoneNumber)]
        public string ShipPhone { get; set; }

        [Display(Name = "AdminID")]
        public int AdminID { get; set; }

        public string FormAction { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> NameTitleList { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> StateList { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> StatusList { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> BoardingStatusList { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> PracticeSizeList { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> CancelReasonCodesList { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> PracticeManagementSoftwareList { get; set; }
    }


    public class VendorDetaiLViewModel: Vendor
    {
            [Display(Name = "Vendor Program ID")]
            public int ProgramID { get; set; }

            [Display(Name = "Vendor ID")]
            public int CustID { get; set; }

            [Display(Name = "Product")]
            public int C_ProgramID { get; set; }

            [Display(Name = "Product Name")]
            [MaxLength(100)]
            public string C_ProgramName { get; set; }

            [Required, Display(Name = "Program Name")]
            [MaxLength(100)]
            public string ProgramName { get; set; }

            [Required, Display(Name = "Program Summary")]
            [MaxLength(500)]
            public string ProgramSummary { get; set; }

            [Required, Display(Name = " Program Description")]
            [MaxLength(1500)]
            public string ProgramDescription { get; set; }

            [Display(Name = "Program Requirements")]
            [MaxLength(2500)]
            public string ProgramRequirements { get; set; }

            [Display(Name = "Directions")]
            [MaxLength(2500)]
            public string ProgramDirections { get; set; }

            [Required, Display(Name = "Member Participation Required")]
            public int MemberParticipationRequired { get; set; }

            [Required, Display(Name = "Member Rebate")]
            [DataType(DataType.Currency)]
            [Range(0.0, 50.00,
                ErrorMessage = "Member Rebate must be a % between 0 and 50")]
            public decimal MemberRebate { get; set; }

            [Required, Display(Name = "Corporate Rebate")]
            [DataType(DataType.Currency)]
            [Range(0.0, 50.00,
                ErrorMessage = "Corporate Rebate must be a % between 0 and 50")]
            public decimal CorporateRebate { get; set; }

            [Display(Name = "Start Date")]
            [DataType(DataType.DateTime)]
            [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
            public Nullable<System.DateTime> StartDate { get; set; }

            [Display(Name = "End Date")]
            [DataType(DataType.DateTime)]
            [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
            public Nullable<System.DateTime> EndDate { get; set; }

            public List<Document> documentList { get; set; }
        
    }

    public class DocumentListViewModel 
    {
        public List<Document> documentList { get; set; }
    }


    public class SiteMemberEditLocationViewModel : Location
    {
        [Display(Name = "First Name")]
        [MaxLength(50)]
        public string ShipFirstName { get; set; }

        [Display(Name = "Last Name")]
        [MaxLength(50)]
        public string ShipLastName { get; set; }

        [Display(Name = "Payee Name")]
        [MaxLength(50)]
        public string ShipName { get; set; }

        [Required, Display(Name = "Address Line 1")]
        [MaxLength(50)]
        public string ShipAddress1 { get; set; }

        [Display(Name = "Address Line 2")]
        [MaxLength(50)]
        public string ShipAddress2 { get; set; }

        [Required, Display(Name = " Ship City")]
        [MaxLength(50)]
        public string ShipCity { get; set; }

        [Required, Display(Name = "State")]
        [MaxLength(50)]
        public string ShipState { get; set; }

        [Required, Display(Name = "Postal Code")]
        [DataType(DataType.PostalCode)]
        public string ShipPostalCode { get; set; }

        [Display(Name = "Payee Email")]
        [MaxLength(100)]
        public string ShipEmail { get; set; }

        [Display(Name = "Payee Phone")]
        [DataType(DataType.PhoneNumber)]
        public string ShipPhone { get; set; }

        [Display(Name = "AdminID")]
        public int AdminID { get; set; }

        public string FormAction { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> NameTitleList { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> StateList { get; set; }

        public string AddLocation { get; set; }

    }


}
