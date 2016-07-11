using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AppTrack.Models;
using System.Web.Mvc;

namespace AppTrack.ViewModels
{
    public class CustomerViewModel:C_Info
    {
    
        public C_Info C_Info { get; set; }
        public StatesDictionary States { get; set; }

        public CustomerViewModel(C_Info c_info)
        {
            //C_Info = c_info;
            States = new StatesDictionary();
        }
    }

public class StatesDictionary
{
    public static SelectList StateSelectList
    {
        get { return new SelectList(StateDictionary, "Value", "Key"); }
    } 
    
    public static readonly IDictionary<string, string> 
        StateDictionary = new Dictionary<string, string> { 
      {"Choose...",""}
    , { "Alabama", "AL" }
    , { "Alaska", "AK" }
    , { "Arizona", "AZ" }
    , { "Arkansas", "AR" }
    , { "California", "CA" }
    // code continues to add states...
    }; 
}


}