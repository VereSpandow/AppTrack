using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using AppTrack.Models;

// Caution if we include System.Web.Mvc for use with SelectListItem, 
// the Compare attribute becomes ambiguous as it is in 2 namespaces.
// So, just specify System.Web.Mvc for SelectListItem

namespace AppTrack.ViewModels
{
    public class ItemBOMListViewModel : ItemBOMViewModel
    {
        public List<C_ItemBOM> ItemBOMList { get; set; }        
    }

    public class ItemBOMViewModel : C_ItemBOM
    {
        public ItemBOMViewModel()
        {
            ParentID = 1; // Parent Item ID
            ItemID = 1; // Child Item ID
            Quantity = 0;            
            StatusID = 1;
            PostDate = DateTime.Now;           
        }

        public int ParentID { get; set; }

        public int ItemID { get; set; }      

        [Display(Name = "Quantity")]
        public int Quantity { get; set; }

        [Display(Name = "Status")]
        public int StatusID { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public Nullable<System.DateTime> PostDate { get; set; }
    }
}