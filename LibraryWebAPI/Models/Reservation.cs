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
    
    public partial class Reservation
    {
        public int R_id { get; set; }
        public Nullable<int> B_id { get; set; }
        public Nullable<int> L_id { get; set; }
        public Nullable<System.DateTime> R_datetime { get; set; }
        public Nullable<bool> R_isActivated { get; set; }
        public Nullable<System.DateTime> R_finishDatetime { get; set; }
        public Nullable<System.DateTime> R_getBookDate { get; set; }
    }
}
