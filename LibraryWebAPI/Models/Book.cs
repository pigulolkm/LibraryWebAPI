//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace LibraryWebAPI.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Book
    {
        public int B_id { get; set; }
        public string B_author { get; set; }
        public string B_title { get; set; }
        public string B_ISBN { get; set; }
        public string B_publisher { get; set; }
        public string B_publicationDate { get; set; }
        public string B_subject { get; set; }
        public string B_language { get; set; }
        public Nullable<System.DateTime> B_datetime { get; set; }
    }
}
