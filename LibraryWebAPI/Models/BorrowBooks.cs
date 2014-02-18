using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LibraryWebAPI.Models
{
    public class BorrowBooks
    {
        public String title { get; set; }
        public String author { get; set; }
        public String publisher { get; set; }
        public String publicationDate { get; set; }
        public System.DateTime shouldReturnedDate { get; set;  }
    }
}