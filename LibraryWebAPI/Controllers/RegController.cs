using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LibraryWebAPI.Models;
using System.Web.Http;

namespace LibraryWebAPI.Controllers
{
    [RoutePrefix("api/reg")]
    public class RegController : ApiController
    {
        // Get api/reg/firstName/lastName
        [Route("{firstname}/{lastname}")]
        public bool PostRegUser(string firstName, string lastName)
        {
            return false;
        }

    }
}
