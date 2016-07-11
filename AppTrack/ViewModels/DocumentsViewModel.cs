using AppTrack.SharedModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppTrack.ViewModels
{
    public class DocumentsViewModel
    {
    public List<Document> DocumentList { get; set; }

    public Document document { get; set; }

    }
}