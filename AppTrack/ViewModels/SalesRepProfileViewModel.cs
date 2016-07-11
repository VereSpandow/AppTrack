using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using AppTrack.SharedModels;
using System.Web.WebPages.Html;
using JQueryUIHelpers;
namespace AppTrack.ViewModels
{
    public class SalesRepProfileViewModel
    {
        public SalesRep SalesRepRecord { get; set; }

        public Nullable<System.DateTime> StartDate { get; set; }
    }
}