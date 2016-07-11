using AppTrack.SharedModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;


namespace AppTrack.ViewModels
{
    public class ContactListViewModel
    {
        public int CustID { get; set; }
        public List<Contact> ContactList { get; set; }

    }


    public class ContactViewModel : Contact
    {
        public IEnumerable<System.Web.Mvc.SelectListItem> NameTitleList { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> ContactTypeList { get; set; }


    }

}