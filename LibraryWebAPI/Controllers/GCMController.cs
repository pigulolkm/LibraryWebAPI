using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using LibraryWebAPI.Models;
using System.Text;
using System.IO;
using Newtonsoft.Json;

namespace LibraryWebAPI.Controllers
{
    public class GCMController : ApiController
    {
        private LibraryEntities db = new LibraryEntities();

        // GET api/GCM
        public IEnumerable<GCM> GetGCMs()
        {
            return db.GCMs.AsEnumerable();
        }

        // GET api/GCM/5
        public GCM GetGCM(int id)
        {
            GCM gcm = db.GCMs.Find(id);
            if (gcm == null)
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
            }

            return gcm;
        }

        // PUT api/GCM/5
        public HttpResponseMessage PutGCM(int id, GCM gcm)
        {
            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }

            if (id != gcm.Gcm_id)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }

            db.Entry(gcm).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex);
            }

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        // POST api/GCM
        public HttpResponseMessage PostGCM(GCM gcm)
        {
            if (ModelState.IsValid)
            {
                gcm.Gcm_created_datetime = DateTime.Now;

                db.GCMs.Add(gcm);
                db.SaveChanges();

                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Created, gcm.Gcm_id);
                response.Headers.Location = new Uri(Url.Link("DefaultApi", new { id = gcm.Gcm_id }));
                return response;
            }
            else
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }
        }

        // DELETE api/GCM/5
        public HttpResponseMessage DeleteGCM(int id)
        {
            GCM gcm = db.GCMs.Find(id);
            if (gcm == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            db.GCMs.Remove(gcm);

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex);
            }

            return Request.CreateResponse(HttpStatusCode.OK, gcm.Gcm_id);
        }

        public String PostAnnouncement(NotificationMessage notificationMessage)
        {
            string applicationID = "AIzaSyBRGAevWCGpSXntIC9v7KZrfesCf21PQc0";
            string senderID = "1007963483160";

            WebRequest request = WebRequest.Create("https://android.googleapis.com/gcm/send");
            request.Method = "post";
            request.ContentType = "application/json";
            request.Headers.Add(string.Format("Authorization: key={0}", applicationID));
            request.Headers.Add(string.Format("Sender: id={0}", senderID));

            String[] regIDs = db.GCMs.Select(g => g.Gcm_regID).ToArray();

            object json =   new { 
                                    delay_while_idle = false,
                                    data =  new {
                                                    message = notificationMessage.msg 
                                                }, 
                                    registration_ids = regIDs
                                };

            string postData = JsonConvert.SerializeObject(json);

            System.Diagnostics.Debug.Write(postData);

            Byte[] byteArray = Encoding.UTF8.GetBytes(postData);

            request.ContentLength = byteArray.Length;

            Stream dataStream = request.GetRequestStream();

            dataStream.Write(byteArray, 0, byteArray.Length);

            dataStream.Close();

            WebResponse tResponse = request.GetResponse();

            dataStream = tResponse.GetResponseStream();

            StreamReader tReader = new StreamReader(dataStream);

            String sResponseFromServer = tReader.ReadToEnd();   //Get response from GCM server.

            tReader.Close();

            dataStream.Close();
            tResponse.Close();

            return sResponseFromServer;
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}