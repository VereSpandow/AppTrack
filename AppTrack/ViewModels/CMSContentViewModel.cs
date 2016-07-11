using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AppTrack.Models;

namespace AppTrack.ViewModels
{
    public partial class CMSContentView: CMSContent
    {

            [AllowHtml]
            public string ContentValue { get; set; }
    }
}