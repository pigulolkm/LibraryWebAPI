using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using LibraryWebAPI.Models;

namespace LibraryWebAPI.Controllers
{
    [RoutePrefix("api/values")]
    public class ValuesController : ApiController
    {
        /*
        // GET api/values
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        public void Delete(int id)
        {
        }
         */

        // Get api/values
        [Route("")]
        public IEnumerable<LibraryUser> Get()
        {
            LibraryEntities dbcontext = new LibraryEntities();
            var query = from l in dbcontext.LibraryUsers
                        select l;
            IEnumerable<LibraryUser> lu = query.ToList();

            return lu;
        }
        
        // Get api/values/id
        [Route("{id:int}")]
        public LibraryUser GetlibraryUser(int id)
        {
            LibraryEntities dbcontext = new LibraryEntities();
            var query = from l in dbcontext.LibraryUsers
                        where l.L_id == id
                        select l;

            return (LibraryUser)query.FirstOrDefault();
        }
    }
}