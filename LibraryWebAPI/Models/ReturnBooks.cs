using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LibraryWebAPI.Models
{
    public class ReturnBooks
    {
        public String title { get; set; }
        public String author { get; set; }
        public String publisher { get; set; }
        public String publicationDate { get; set; }
        public System.DateTime returnedDate { get; set; }
        public double fine { get; set; }
        public bool isReserved { get; set; }
    }
}