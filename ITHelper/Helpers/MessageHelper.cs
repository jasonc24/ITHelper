using Microsoft.Extensions.Configuration;
using System;
using System.Net.Mail;

namespace ITHelper.Helpers
{
    public static class MessageHelper
    {
        /// <summary>
        /// Log4Net logging module
        /// </summary>
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        /// <summary>
        /// Retrieves the SMTP mail client for the application.  If one does not exist, it creates it, then returns it while storing it for future use.
        /// </summary>
        /// <returns></returns>
        public static SmtpClient GetSmtpClient(IConfiguration Configuration)
        {
            // House everything in a try block for safety
            try
            {
                // Create a new client object
                var client = new SmtpClient(Configuration["MailSettings:EMailRelay"],
                    int.Parse(Configuration["MailSettings:EMailPort"]));

                // Determine the type of credentials to use
                client.UseDefaultCredentials = bool.Parse(Configuration["MailSettings:AnonymousAuthentication"]);
                if (!client.UseDefaultCredentials)
                {
                    client.Credentials = new System.Net.NetworkCredential(
                                       Configuration["MailSettings:EMailAddress"],
                                       Configuration["MailSettings:EMailPassword"]);
                }

                // Set additional client settings
                client.EnableSsl = bool.Parse(Configuration["MailSettings:TSL"]);
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.Timeout = int.Parse(Configuration["MailSettings:SMTPTimeout"]) * 1000;

                // Hook-up the callback mechanism
                //client.SendCompleted += new SendCompletedEventHandler(SendCompletedCallbackAsync);

                // Return the client to the caller
                return (client);
            }
            catch (Exception e)
            { LogError(e); }

            // Return a null if there was an exception
            return (null);

        }

        /// <summary>
        /// Logs an error into the database asynchronously
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public static void LogError(Exception e)
        {
            log.Error(string.Format("{0}\n{1}",
                e.Message,
                e.StackTrace.Replace(" at ", " at\n")));
        }

        /// <summary>
        /// Logs an error into the database asynchronously
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        //public static async Task LogErrorAsync(Exception e)
        //{
        //    using (var db = new DataContext())
        //    {
        //        var error = new Error()
        //        {
        //            Message = e.Message,
        //            StackTrace = e.StackTrace.Replace("\n", " "),
        //            Source = e.Source,
        //            TargetSite = e.TargetSite.ToString(),
        //            DateCreated = DateTimeOffset.Now
        //        };
        //        db.Errors.Add(error);
        //        await db.SaveChangesAsync();
        //    }
        //}
    }
}