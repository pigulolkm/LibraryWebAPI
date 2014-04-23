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
using LibraryWebAPI.Utils;

namespace LibraryWebAPI.Controllers
{
    public class AnnouncementController : ApiController
    {
        private LibraryEntities db = new LibraryEntities();

        // GET api/Announcement
        public IEnumerable<Announcement> GetAnnouncements()
        {
            return db.Announcements.OrderByDescending(a => a.A_datetime).AsEnumerable();
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
            // Add to db
            Announcement ann = new Announcement
            {
                A_content = notificationMessage.msg,
                A_datetime = DateTime.Now                
            };
            db.Announcements.Add(ann);
            db.SaveChanges();

            String[] regIDs = db.GCMs.Select(g => g.Gcm_regID).ToArray();

            String sResponseFromServer = Util.sendNotificationMsg(notificationMessage.msg, regIDs);       

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