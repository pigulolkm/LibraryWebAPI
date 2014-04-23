using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;

namespace LibraryWebAPI.Utils
{
    public class Util
    {
        public static String BookStatus_ONTHESHELF = "On the shelf";
	    public static String BookStatus_BORROWED = "Borrowed";
	    public static String BookStatus_RESERVED = "Reserved";
        public static int Reservation_AmountForEachBook = 13;
        public static int Reservation_getBookDays = 7;

        public static String applicationID = "AIzaSyBRGAevWCGpSXntIC9v7KZrfesCf21PQc0";
        public static String senderID = "1007963483160";

        public static String sendNotificationMsg(String msg, String[] regIDs)
        {
            // Push notification creation
            WebRequest request = WebRequest.Create("https://android.googleapis.com/gcm/send");
            request.Method = "post";
            request.ContentType = "application/json;charset=UTF-8";
            request.Headers.Add(string.Format("Authorization: key={0}", Util.applicationID));
            request.Headers.Add(string.Format("Sender: id={0}", Util.senderID));

            object json = new
            {
                delay_while_idle = false,
                data = new
                {
                    message = msg
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

            return null;
        }
    }
}