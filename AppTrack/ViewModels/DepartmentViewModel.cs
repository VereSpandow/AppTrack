using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using AppTrack.Models;

// Caution if we include System.Web.Mvc for use with SelectListItem, 
// the Compare attribute becomes ambiguous as it is in 2 namespaces.
// So, just specify System.Web.Mvc for SelectListItem

namespace AppTrack.ViewModels
{
    public class DepartmentListViewModel : DepartmentViewModel
    {
        public List<C_Departments> DepartmentList { get; set; }        
    }

    public class DepartmentViewModel : C_Departments
    {
        public DepartmentViewModel()
        {
            DeptID = 1;
            DepartmentTitle = null; 
            DepartmentName = null; 
            DepartmentDescription = null; 
            PostDate = DateTime.Now; 
            SeqNo = 1; 
            isMutEx = "Yes";                         
        }
                                
        public int DeptID { get; set; }

        [Display(Name = "Title")]
        [MaxLength(50)]
        public string DepartmentTitle { get; set; }

        [Display(Name = "Name")]
        [MaxLength(50)]
        public string DepartmentName { get; set; }
        
        [Display(Name = "Description")]
        [DataType(DataType.MultilineText)]
        [MaxLength(500)]
        public string DepartmentDescription { get; set; }

        /*
        [Display(Name = "Email")]
        [MaxLength(50)]
        public string DeptSupportEmail { get; set; }

        [Display(Name = "Phone")]
        [MaxLength(50)]
        public string DeptSupportPhone { get; set; }
        */

        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public System.DateTime PostDate { get; set; }

        [Display(Name = "Sequence Number")]
        public int SeqNo { get; set; }
                
        [MaxLength(3)]
        public string isMutEx { get; set; }
    }
}