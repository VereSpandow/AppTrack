﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace AppTrack.ViewModels
{
    public class MemberBudgetReportViewModel
    {
        public List<MemberBudgetReport> MemberBudgetReportList { get; set; }
        public List<MemberBudgetReportXTab> MemberBudgetReportListXTab { get; set; }

        [Display(Name = "Member Type")]
        [MaxLength(10)]
        public string MemberType { get; set; }

        [Display(Name = "Program Type")]
        public int PrgramType { get; set; }

        [Display(Name = "Period")]
        public int searchPeriodID { get; set; }

        public IEnumerable<System.Web.Mvc.SelectListItem> SearchPeriodList { get; set; }

    }

    public class MemberBudgetReport
    {
        public int Seqno{ get; set; }
        public int DataGroupID { get; set; }
        public int Flag1 { get; set; }
        public int PeriodID { get; set; }
        public string PeriodName { get; set; }
        public string Title { get; set; }
        public int DepartmentID { get; set; }
        public string DepartmentName { get; set; }
        public int StoreID { get; set; }
        public string StoreName { get; set; }
        public int MemberCount { get; set; }
    }

    public class MemberBudgetReportXTab
    {
        public int Seqno { get; set; }
        public int DataGroupID { get; set; }
        public int Flag1 { get; set; }
        public int PeriodID { get; set; }
        public string PeriodName { get; set; }
        public string Title { get; set; }
        public int DepartmentID { get; set; }
        public string DepartmentName { get; set; }
        public int StoreID { get; set; }
        public string StoreName { get; set; }
        public int MemberCount { get; set; }
        public int PeriodID1 { get; set; }
        public int PeriodID2 { get; set; }
        public int PeriodID3 { get; set; }
        public int PeriodID4 { get; set; }
        public int PeriodID5 { get; set; }
        public int PeriodID6 { get; set; }
        public int PeriodID7 { get; set; }
        public int PeriodID8 { get; set; }
        public int PeriodID9 { get; set; }
        public int PeriodID10 { get; set; }
        public int PeriodID11 { get; set; }
        public int PeriodID12 { get; set; }
        public int PeriodID13 { get; set; }
        public string PeriodLabel1 { get; set; }
        public string PeriodLabel2 { get; set; }
        public string PeriodLabel3 { get; set; }
        public string PeriodLabel4 { get; set; }
        public string PeriodLabel5 { get; set; }
        public string PeriodLabel6 { get; set; }
        public string PeriodLabel7 { get; set; }
        public string PeriodLabel8 { get; set; }
        public string PeriodLabel9 { get; set; }
        public string PeriodLabel10 { get; set; }
        public string PeriodLabel11 { get; set; }
        public string PeriodLabel12 { get; set; }
        public string PeriodLabel13 { get; set; }
        public int PeriodData1 { get; set; }
        public int PeriodData2 { get; set; }
        public int PeriodData3 { get; set; }
        public int PeriodData4 { get; set; }
        public int PeriodData5 { get; set; }
        public int PeriodData6 { get; set; }
        public int PeriodData7 { get; set; }
        public int PeriodData8 { get; set; }
        public int PeriodData9 { get; set; }
        public int PeriodData10 { get; set; }
        public int PeriodData11 { get; set; }
        public int PeriodData12 { get; set; }
        public int PeriodData13 { get; set; }
    }
}