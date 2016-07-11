using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using AppTrack.Models;

// Caution if we include System.Web.Mvc for use with SelectListItem, 
// the Compare attribute becomes ambiguous as it is in 2 namespaces.
// So, just specify System.Web.Mvc for SelectListItem

namespace AppTrack.ViewModels
{

    public class ItemListViewModel : ItemViewModel
    {
        public List<C_Item> ItemList { get; set; }        
    }

    public class ItemViewModel : C_Item
    {
        public ItemViewModel()
        {
            ProgramID = 1;
            ItemImage1 = "~/images/";
            MinQuantity = 1;
            MaxQuantity = 1;
            FromItemID = 0;
            ToItemID = 0;
            StartDate = DateTime.Now;
            EndDate = DateTime.Now.AddYears(100);
            DisplayFlag = 1;
            Status = "Active";
            StatusID = 1;
            PostDate = DateTime.Now;            
        }
                                
        public int ItemID { get; set; }
        
        /*        

        [Display(Name = "Item Parent ID")]
        public Nullable<int> ParentID { get; set; }
        
        */

        [Display(Name = "Program ID")]
        public int ProgramID { get; set; }

        [Display(Name = "Product Code")]
        [MaxLength(50)]
        public string ProductCode { get; set; }

        [Display(Name = "Item Name")]
        [MaxLength(50)]
        public string ItemName { get; set; }

        [Display(Name = "Item Title")]
        [MaxLength(200)]
        public string ItemTitle { get; set; }

        [Display(Name = "Item Description")]
        [DataType(DataType.MultilineText)]
        [MaxLength(2500)]
        public string ItemDescription { get; set; }

        /*
         [Display(Name = "GL Code")]
         [MaxLength(50)]
         public string GLCode { get; set; }
        */

         [Display(Name = "Image1 URL")]
         [MaxLength(50)]
         public string ItemImage1 { get; set; }

        /*
        
         [Display(Name = "Image2 URL")]
         [MaxLength(50)]
         public string ItemImage2 { get; set; }

         [Display(Name = "Detail Page")]
         [MaxLength(50)]
         public string ItemDetailPage { get; set; }

         [Display(Name = "Video URL")]
         [MaxLength(50)]
         public string ItemVideo { get; set; }               

         [Display(Name = "Is Ship")]
         public Nullable<int> IsShip { get; set; }
         
        */

        [Display(Name = "Min Quantity")]
        public int MinQuantity { get; set; }

        [Display(Name = "Max Quantity")]
        public int MaxQuantity { get; set; }
        
        [Display(Name = "To Item ID")]
        public Nullable<int> ToItemID { get; set; }

        [Display(Name = "From Item ID")]
        public Nullable<int> FromItemID { get; set; }
        
        [Display(Name = "Status")]
        [MaxLength(10)]        
        public string Status { get; set; }
        
        [Display(Name = "Is AutoShip")]
        [MaxLength(3)]
        public string isAutoShip { get; set; }

        /*
        [Display(Name = "Is Shipping")]
        public Nullable<byte> isShipping { get; set; }
        */

        [Required(ErrorMessage = "Required"), Display(Name = "Start Date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public Nullable<System.DateTime> StartDate { get; set; }
                
        [Required(ErrorMessage = "Required"), Display(Name = "End Date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public Nullable<System.DateTime> EndDate { get; set; }
        
        /*        

        [Display(Name = "Is Pre Packaged")]
        public Nullable<byte> IsPrePackaged { get; set; }

        */
        
        [Display(Name = "Display")]
        public Nullable<byte> DisplayFlag { get; set; }

        /*
        [Display(Name = "Filter Set1")]        
        [MaxLength(50)]        
        public string FilterSet1 { get; set; }

        [Display(Name = "Filter Set2")]
        [MaxLength(50)]
        public string FilterSet2 { get; set; }

        [Display(Name = "Filter Set3")]
        [MaxLength(50)]
        public string FilterSet3 { get; set; }

        [Display(Name = "Filter Set4")]
        [MaxLength(50)]
        public string FilterSet4 { get; set; }

        [Display(Name = "Filter Set5")]
        [MaxLength(50)]
        public string FilterSet5 { get; set; }

        [Display(Name = "Filter Set6")]
        [MaxLength(50)]
        public string FilterSet6 { get; set; }     
        */

        [Display(Name = "Status")]
        public Nullable<int> StatusID { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public Nullable<System.DateTime> PostDate { get; set; }
    }
}