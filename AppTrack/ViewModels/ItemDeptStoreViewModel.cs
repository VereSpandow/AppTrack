using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using AppTrack.Models;

// Caution if we include System.Web.Mvc for use with SelectListItem, 
// the Compare attribute becomes ambiguous as it is in 2 namespaces.
// So, just specify System.Web.Mvc for SelectListItem

namespace AppTrack.ViewModels
{

    public class ItemDeptStoreListViewModel : ItemDeptStoreViewModel
    {
        public List<C_ItemDeptStore> ItemDeptStoreList { get; set; }        
    }

    public class ItemDeptStoreViewModel : C_ItemDeptStore
    {
        public ItemDeptStoreViewModel()
        {
            ItemID = null;
            DeptID = null;
            StoreID = null;
            SeqNo = null;
            StatusID = null;
            PostDate = null; //DateTime.Now;            
        }
                                
        public Nullable<int> ItemID { get; set; }

        public Nullable<int> DeptID { get; set; }

        public Nullable<int> StoreID { get; set; }

        public Nullable<int> SeqNo { get; set; }

        [Display(Name = "Status")]
        public Nullable<int> StatusID { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public Nullable<System.DateTime> PostDate { get; set; }
    }
}