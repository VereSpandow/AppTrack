using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using AppTrack.Models;

// Caution if we include System.Web.Mvc for use with SelectListItem, 
// the Compare attribute becomes ambiguous as it is in 2 namespaces.
// So, just specify System.Web.Mvc for SelectListItem

namespace AppTrack.ViewModels
{

    public class StoreListViewModel : StoreViewModel
    {
        public List<C_Stores> StoreList { get; set; }        
    }

    public class StoreViewModel : C_Stores
    {
        public StoreViewModel()
        {
            CompanyID = 1;
            StoreID = 0;
            StoreName = null;
            Title = null;
            Description = null;
            StartDate = DateTime.Now;
            EndDate = DateTime.Now.AddYears(100);            
            Status = "Active";
            StatusID = 1;
            PostDate = DateTime.Now;            
        }
        
        [Display(Name = "Company ID")]
        public Nullable<int> CompanyID { get; set; }

        [Display(Name = "Store ID")]
        public int StoreID { get; set; }
        
        [Display(Name = "Store Name")]
        [MaxLength(50)]
        public string StoreName { get; set; }
        
        [Display(Name = "Title")]
        [MaxLength(50)]
        public string Title { get; set; }
        
        [Display(Name = "Description")]
        [DataType(DataType.MultilineText)]
        [MaxLength(4000)]
        public string Description { get; set; }                       

        [Required(ErrorMessage = "Required"), Display(Name = "Start Date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public Nullable<System.DateTime> StartDate { get; set; }
                
        [Required(ErrorMessage = "Required"), Display(Name = "End Date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public Nullable<System.DateTime> EndDate { get; set; }

        [Display(Name = "Status")]
        [MaxLength(50)]
        public string Status { get; set; }

        [Display(Name = "Status")]
        public Nullable<int> StatusID { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public System.DateTime PostDate { get; set; }
    }
}