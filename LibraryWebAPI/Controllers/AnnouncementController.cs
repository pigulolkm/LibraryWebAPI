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
using Newtonsoft.Json;
using System.Text;
using System.IO;

namespace LibraryWebAPI.Controllers
{
    public class AnnouncementController : ApiController
    {
        private LibraryEntities db = new LibraryEntities();

        // GET api/Announcement
        public IEnumerable<Announcement> GetAnnouncements()
        {
            return db.Announcements.AsEnumerable();
        }

        // GET api/Announcement
        public IEnumerable<Announcement> GetLastFiveAnnouncements()
        {
            var lastFiveAnnouncement = (from a in db.Announcements
                                        orderby a.A_datetime descending
                                        select a).Take(5);
            return lastFiveAnnouncement.AsEnumerable();
        }

        // GET api/Announcement/5
        public Announcement GetAnnouncement(int id)
        {
            Announcement announcement = db.Announcements.Find(id);
            if (announcement == null)
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
            }

            return announcement;
        }

        // PUT api/Announcement/5
        public HttpResponseMessage PutAnnouncement(int id, Announcement announcement)
        {
            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }

            if (id != announcement.A_id)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }

            db.Entry(announcement).State = EntityState.Modified;

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

        // POST api/Announcement
        //public HttpResponseMessage PostAnnouncement(Announcement announcement)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        db.Announcements.Add(announcement);
        //        db.SaveChanges();

        //        HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Created, announcement);
        //        response.Headers.Location = new Uri(Url.Link("DefaultApi", new { id = announcement.A_id }));
        //        return response;
        //    }
        //    else
        //    {
        //        return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
        //    }
        //}

        public String PostAnnouncement(NotificationMessage notificationMessage)
        {
            string applicationID = "AIzaSyBRGAevWCGpSXntIC9v7KZrfesCf21PQc0";
            string senderID = "1007963483160";

            // Add to db
            Announcement ann = new Announcement
            {
                A_content = notificationMessage.msg,
                A_datetime = DateTime.Now                
            };
            db.Announcements.Add(ann);
            db.SaveChanges();

            // Push notification creation
            WebRequest request = WebRequest.Create("https://android.googleapis.com/gcm/send");
            request.Method = "post";
            request.ContentType = "application/json;charset=UTF-8";
            request.Headers.Add(string.Format("Authorization: key={0}", applicationID));
            request.Headers.Add(string.Format("Sender: id={0}", senderID));

            String[] regIDs = db.GCMs.Select(g => g.Gcm_regID).ToArray();

            object json = new
            {
                delay_while_idle = false,
                data = new
                {
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

        // DELETE api/Announcement/5
        public HttpResponseMessage DeleteAnnouncement(int id)
        {
            Announcement announcement = db.Announcements.Find(id);
            if (announcement == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            db.Announcements.Remove(announcement);

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex);
            }

            return Request.CreateResponse(HttpStatusCode.OK, announcement);
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}