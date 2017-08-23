using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Collections;

namespace ClaimsLoad.Library
{
    class EmailHandler
    {

        public static void SendEmail(string Message, string fileName)
        {
            //Logo
            string URL = "http://uwbase.oldrepublicaerospace.com/images/Blue_MarketingLogos_Aerospace.jpg";
            //Send Email to Application Developer Team
            SmtpClient myclient = new SmtpClient(System.Configuration.ConfigurationManager.AppSettings["mailServer"].ToString());
            myclient.Port = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["mailPort"].ToString());
            myclient.UseDefaultCredentials = false;
            //myclient.Credentials = new System.Net.NetworkCredential(System.Configuration.ConfigurationManager.AppSettings["mailUsername"].ToString(), System.Configuration.ConfigurationManager.AppSettings["mailPassword"].ToString());
            myclient.DeliveryMethod = SmtpDeliveryMethod.Network;
            MailMessage mymes = new MailMessage();
            mymes.To.Add(System.Configuration.ConfigurationManager.AppSettings["mailTo"].ToString());
            mymes.Priority = MailPriority.Normal;
            mymes.IsBodyHtml = true;
            mymes.From = new MailAddress(System.Configuration.ConfigurationManager.AppSettings["mailFrom"].ToString());
            mymes.Subject = "New Workers Comp Claims file " + fileName + " was Received!";
            mymes.Body = "<html><head></head><body>";
            mymes.Body += "<img src=\"" + URL + "\" />" + "<br/><br/>";
            mymes.Body += "<b>Hello Support,</b><br/><br/>";
            mymes.Body += "<b>The following information is from the run of the Claims Upload process :</b><br/><br/>";
            mymes.Body += "---------------------------------------------------------------------------------------<br/><br/>";
            mymes.Body += "<b>Summary:</b>" + " " + Message + "<br/><br/>";
            mymes.Body += "---------------------------------------------------------------------------------------<br/><br/>";
            mymes.Body += "<b>If there are any errors reported review the error log.</b>";
            myclient.Send(mymes);
            mymes.Dispose();
        }

    }
}
