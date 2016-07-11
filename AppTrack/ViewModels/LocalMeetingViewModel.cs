using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AppTrack.Models;
using AppTrack.SharedModels;
using System.ComponentModel.DataAnnotations;

namespace AppTrack.ViewModels
{




//Deprecated the use of this after consolidating this with MeetingViewModel.cs
    




    public partial class LocalMeeting
        {
            public int ID { get; set; }
            
            public Nullable<int> CustID { get; set; }

            [Display(Name = "Sponsor Name")]
            [MaxLength(50)]
            public string SponsorName { get; set; }

            [Display(Name = "Host Name")]
            [MaxLength(50)]
            public string HostName { get; set; }

            [Display(Name = "Location Title")]
            [MaxLength(50)]
            public string LocationTitle { get; set; }

            [Display(Name = "Event Name")]
            [MaxLength(50)]
            public string EventTitle { get; set; }

            [Display(Name = "Topic")]
            [MaxLength(50)]
            public string EventDescription { get; set; }

            [Display(Name = "Start Date/Time")]
            [DataType(DataType.DateTime)]
            public System.DateTime EventStartDate { get; set; }

            [Display(Name = "End Date/Time")]
            [DataType(DataType.DateTime)]
            public Nullable<System.DateTime> EventEndDate { get; set; }
            
            public string EventDateTimeString { get; set; }

            [Display(Name = "Max Capacity")]
            public Nullable<int> Capacity { get; set; }
            
            public Nullable<int> Reserved { get; set; }
            
            public Nullable<int> Available { get; set; }

            [Display(Name = "Address Line 1")]
            [MaxLength(50)]
            public string Address1 { get; set; }
            
            public string Address2 { get; set; }

            [Display(Name = "City")]
            [MaxLength(50)]
            public string City { get; set; }

            [Display(Name = "State")]
            [MaxLength(50)]
            public string State { get; set; }

            [Display(Name = "Zip Code")]
            [DataType(DataType.PostalCode)]
            public string PostalCode { get; set; }

            [MaxLength(30)]
            public string Status { get; set; }

            [DataType(DataType.DateTime)]
            public System.DateTime StatusDate { get; set; }
            
            public int StatusID { get; set; }
            
            public int AdminID { get; set; }
        }

}