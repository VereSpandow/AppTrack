using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using AppTrack.SharedModels;
using System.Web.WebPages.Html;
using JQueryUIHelpers;
namespace AppTrack.ViewModels
{
    public class VendorProfileProgram
    {
        public string ProgramName { get; set; }

        public int? MemberParticipationRequired { get; set; }

        public decimal? MemberRebate { get; set; }

        public decimal? CorporateRebate { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public Nullable<System.DateTime> StartDate { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public Nullable<System.DateTime> EndDate { get; set; }

        public string Status { get; set; }

        public string C_ProgramName { get; set; }

        public string RequirementType { get; set; }

        public string RequirementName { get; set; }

        public string DocumentName { get; set; }

    }

    public class VendorRebateSummary
    {
        public int CommissionID { get; set; }
        public string CommissionName { get; set; }
        public int PeriodID { get; set; }
        public string PeriodName { get; set; }
        [DataType(DataType.Currency)]
        public decimal RebateTotal { get; set; }

    }
    public class VendorProfileViewModel
    {
        public Vendor VendorRecord { get; set; }

        public List<VendorProfileProgram> VendorProgramList { get; set; }

        public List<VendorContact> VendorContactList { get; set; }

        public List<CustomerNote> VendorTaskList { get; set; }

        public List<VendorRequirement> VendorRequirementList { get; set; }

    }
}