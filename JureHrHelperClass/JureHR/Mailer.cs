using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Mail;
using System.Net;
using System.IO;
using System.Net.NetworkInformation;
using System.DirectoryServices;
using System.Security.Principal;
using System.Web;
using System.Net.Configuration;
using System.Configuration;
using System.Web.Configuration;
using System.Xml;
using System.Net.Mime;

namespace JureHR
{
    /// <summary>
    /// mailHelper is a helper class that takes the System.Net.Mail library
    /// and provides a simple library for anyone to use.
    /// </summary>
    public class Mailer
    {
        #region Properties And Constructors

        /// <summary>
        /// Set Server Name
        /// </summary>
        public static string ServerName { get; set; }
        /// <summary>
        /// Set Port
        /// </summary>
        public static int Port { get; set; }
        /// <summary>
        /// Set User Name
        /// </summary>
        public static string AuthName { get; set; }
        /// <summary>
        /// Set Password
        /// </summary>
        public static string AuthPassword { get; set; }

        /// <summary>
        /// Gets the SmtpClient
        /// </summary>
        private SmtpClient MailClient { get; set; }
        /// <summary>
        /// Gets the MailMessage
        /// </summary>
        private MailMessage Message { get; set; }
        /// <summary>
        /// Gets the from MailAddress
        /// </summary>
        private MailAddress MailFromAddress { get; set; }

        /// <summary>
        /// Gets or Sets the address the email is from
        /// </summary>
        private string FromAddress { get; set; }
        /// <summary>
        /// Gets or Sets the subject of the email
        /// </summary>
        private string Subject { get; set; }
        /// <summary>
        /// Gets or Sets the body of the email
        /// </summary>
        private string Body { get; set; }
        /// <summary>
        /// Gets or Sets if this is a html email.  True for html false for text
        /// </summary>
        private bool IsHtmlEmail { get; set; }

        /// <summary>
        /// Mailer Constructor
        /// </summary>
        public Mailer()
        {
        }
        /// <summary>
        /// Mailer Constructor With Server Name
        /// </summary>
        /// <param name="serverName">Set Diferrent Server Name from Web.Config</param>
        public Mailer(string serverName)
        {
            ServerName = serverName;
        }
        /// <summary>
        /// Mailer Constructor With Server Name And Port Number
        /// </summary>
        /// <param name="serverName">Set Diferrent Server Name from Web.Config</param>
        /// <param name="port">Set Diferrent Port Number from Web.Config</param>
        public Mailer(string serverName, int port)
        {
            ServerName = serverName;
            Port = port;
        }
        /// <summary>
        /// Mailer Constructor With Server Name, Port Number And Authentication
        /// </summary>
        /// <param name="serverName">Set Diferrent Server Name from Web.Config</param>
        /// <param name="port">Set Diferrent Port Number from Web.Config</param>
        /// <param name="authName">Set Diferrent UserName from Web.Config</param>
        /// <param name="authPassword">Set Diferrent Password from Web.Config</param>
        public Mailer(string serverName, int port, string authName, string authPassword)
        {
            ServerName = serverName;
            Port = port;
            AuthName = authName;
            AuthPassword = authPassword;
        }

        #endregion

        #region Public Members

        /// <summary>
        /// The status of the mail being sent using the async method
        /// </summary>
        public static string mailStatus = String.Empty;

        /// <summary>
        /// Notifies the calling application of the email status
        /// </summary>
        public static event EventHandler notifyCaller;

        #endregion

        #region Private Read Methods

        /// <summary>
        /// system.net/mailSettings
        /// </summary>
        /// <returns></returns>
        private static MailSettingsSectionGroup GetSystemMailSettings()
        {
            Configuration config = WebConfigurationManager.OpenWebConfiguration(HttpContext.Current.Request.ApplicationPath);
            MailSettingsSectionGroup settings = (MailSettingsSectionGroup)config.GetSectionGroup("system.net/mailSettings");
            return settings;
        }

        private static void MailSettings()
        {
            MailSettingsSectionGroup systemSettings = GetSystemMailSettings();

            if (String.IsNullOrEmpty(ServerName))
                ServerName = systemSettings.Smtp.Network.Host;
            if (String.IsNullOrEmpty(ServerName))
                ServerName = "127.0.0.1";
            if (Port == 0)
                Port = systemSettings.Smtp.Network.Port;
            if (Port == 0)
                Port = 25;
            if (String.IsNullOrEmpty(AuthName) && String.IsNullOrEmpty(AuthPassword))
            {
                AuthName = systemSettings.Smtp.Network.UserName;
                AuthPassword = systemSettings.Smtp.Network.Password;
            }
        }

        /// <summary>
        /// Notifies the calling application about the status of the email being sent
        /// </summary>
        protected static void OnNotifyCaller()
        {
            if (notifyCaller != null)
            {
                notifyCaller(mailStatus, EventArgs.Empty);
            }
        }

        #endregion

        #region Public Email Methods

        /// <summary>
        /// Using the assigned properties generates an email message object
        /// and sends it using the assigned smtp server.  To use this method
        /// use createMailObjects, set the properties for the email information
        /// and use the AddToAddress, AddCCAddress, AddBCCAddress, AddAttachments 
        /// and AuthenticateToServer methods to finish the setup of the mail objects
        /// </summary>
        public void SendEmail(Boolean asyncEmail)
        {
            try
            {
                if (asyncEmail == true)
                {
                    MailClient.SendCompleted += new SendCompletedEventHandler(MailClient_SendCompleted);
                    MailClient.SendAsync(Message, Message.To.ToString());
                }
                else
                {
                    MailClient.Send(Message);
                }
            }
            catch (SmtpException ex)
            {
                ErrNotify(ex, "Mailer SendEmail 1");
            }
        }

        /// <summary>
        /// Generates an email and sends it, requires no additional properties to be set
        /// prior to this call
        /// </summary>
        /// <param name="asyncEmail">Used it you want a response from the SMTP server</param>
        /// <param name="from">The address the email is from</param>
        /// <param name="to">The address the email is to</param>
        /// <param name="subject">The subject of the email</param>
        /// <param name="body">The body of the email</param>
        /// <param name="htmlEmail">Is this an html based email</param>
        /// <param name="separate">Messages are sending per email separetly</param>
        public void SendEmail(Boolean asyncEmail, String from, String to, String subject, String body, Boolean htmlEmail, Boolean separate)
        {
            try
            {
                SendSeperate(asyncEmail, from, to, null, null, subject, body, htmlEmail, null, null, separate);
            }
            catch (SmtpException ex)
            {
                ErrNotify(ex, "Mailer SendEmail 2");
            }
        }

        /// <summary>
        /// Generates an email and sends it, requires no additional properties to be set
        /// prior to this call
        /// </summary>
        /// <param name="asyncEmail">Used it you want a response from the SMTP server</param>
        /// <param name="from">The address the email is from</param>
        /// <param name="to">The address the email is to</param>
        /// <param name="subject">The subject of the email</param>
        /// <param name="body">The body of the email</param>
        /// <param name="htmlEmail">Is this an html based email</param>
        /// <param name="separate">Messages are sending per email separetly</param>
        /// <param name="filePath">The path to the attachment</param>
        public void SendEmail(Boolean asyncEmail, String from, String to, String subject, String body, Boolean htmlEmail, String filePath, Boolean separate)
        {
            try
            {
                SendSeperate(asyncEmail, from, to, null, null, subject, body, htmlEmail, filePath, null, separate);
            }
            catch (SmtpException ex)
            {
                ErrNotify(ex, "Mailer SendEmail 3");
            }
        }

        /// <summary>
        /// Generates an email and sends it, requires no additional properties to be set
        /// prior to this call
        /// </summary>
        /// <param name="asyncEmail">Used it you want a response from the SMTP server</param>
        /// <param name="from">The address the email is from</param>
        /// <param name="to">The address the email is to</param>
        /// <param name="subject">The subject of the email</param>
        /// <param name="body">The body of the email</param>
        /// <param name="htmlEmail">Is this an html based email</param>
        /// <param name="separate">Messages are sending per email separetly</param>
        /// <param name="attachData">An attachment created from a file resource</param>
        public void SendEmail(Boolean asyncEmail, String from, String to, String subject, String body, Boolean htmlEmail, Attachment attachData, Boolean separate)
        {
            try
            {
                SendSeperate(asyncEmail, from, to, null, null, subject, body, htmlEmail, null, attachData, separate);
            }
            catch (SmtpException ex)
            {
                ErrNotify(ex, "Mailer SendEmail 4");
            }
        }

        /// <summary>
        /// Generates an email and sends it, requires no additional properties to be set
        /// prior to this call
        /// </summary>
        /// <param name="asyncEmail">Used it you want a response from the SMTP server</param>
        /// <param name="from">The address the email is from</param>
        /// <param name="to">The address the email is to</param>
        /// <param name="cc">The addess the email is being copied to</param>
        /// <param name="subject">The subject of the email</param>
        /// <param name="body">The body of the email</param>
        /// <param name="htmlEmail">Is this an html based email</param>
        /// <param name="separate">Messages are sending per email separetly</param>
        public void SendEmail(Boolean asyncEmail, String from, String to, String cc, String subject, String body, Boolean htmlEmail, Boolean separate)
        {
            try
            {
                SendSeperate(asyncEmail, from, to, cc, null, subject, body, htmlEmail, null, null, separate);
            }
            catch (SmtpException ex)
            {
                ErrNotify(ex, "Mailer SendEmail 5");
            }
        }

        /// <summary>
        /// Generates an email and sends it, requires no additional properties to be set
        /// prior to this call
        /// </summary>
        /// <param name="asyncEmail">Used it you want a response from the SMTP server</param>
        /// <param name="from">The address the email is from</param>
        /// <param name="to">The address the email is to</param>
        /// <param name="cc">The addess the email is being copied to</param>
        /// <param name="subject">The subject of the email</param>
        /// <param name="body">The body of the email</param>
        /// <param name="htmlEmail">Is this an html based email</param>
        /// <param name="separate">Messages are sending per email separetly</param>
        /// <param name="filePath">The path to the attachment</param>
        public void SendEmail(Boolean asyncEmail, String from, String to, String cc, String subject, String body, Boolean htmlEmail, String filePath, Boolean separate)
        {
            try
            {
                SendSeperate(asyncEmail, from, to, cc, null, subject, body, htmlEmail, filePath, null, separate);
            }
            catch (SmtpException ex)
            {
                ErrNotify(ex, "Mailer SendEmail 6");
            }
        }

        /// <summary>
        /// Generates an email and sends it, requires no additional properties to be set
        /// prior to this call
        /// </summary>
        /// <param name="asyncEmail">Used it you want a response from the SMTP server</param>
        /// <param name="from">The address the email is from</param>
        /// <param name="to">The address the email is to</param>
        /// <param name="cc">The addess the email is being copied to</param>
        /// <param name="subject">The subject of the email</param>
        /// <param name="body">The body of the email</param>
        /// <param name="htmlEmail">Is this an html based email</param>
        /// <param name="separate">Messages are sending per email separetly</param>
        /// <param name="attachData">An attachment created from a file resource</param>
        public void SendEmail(Boolean asyncEmail, String from, String to, String cc, String subject, String body, Boolean htmlEmail, Attachment attachData, Boolean separate)
        {
            try
            {
                SendSeperate(asyncEmail, from, to, cc, null, subject, body, htmlEmail, null, attachData, separate);
            }
            catch (SmtpException ex)
            {
                ErrNotify(ex, "Mailer SendEmail 7");
            }
        }

        /// <summary>
        /// Generates an email and sends it, requires no additional properties to be set
        /// prior to this call
        /// </summary>
        /// <param name="asyncEmail">Used it you want a response from the SMTP server</param>
        /// <param name="from">The address the email is from</param>
        /// <param name="to">The address the email is to</param>
        /// <param name="cc">The addess the email is being copied to</param>
        /// <param name="bcc">The addess the email is being blind copied to</param>
        /// <param name="subject">The subject of the email</param>
        /// <param name="body">The body of the email</param>
        /// <param name="htmlEmail">Is this an html based email</param>
        /// <param name="separate">Messages are sending per email separetly</param>
        public void SendEmail(Boolean asyncEmail, String from, String to, String cc, String bcc, String subject, String body, Boolean htmlEmail, Boolean separate)
        {
            try
            {
                SendSeperate(asyncEmail, from, to, cc, bcc, subject, body, htmlEmail, null, null, separate);
            }
            catch (SmtpException ex)
            {
                ErrNotify(ex, "Mailer SendEmail 8");
            }
        }

        /// <summary>
        /// Generates an email and sends it, requires no additional properties to be set
        /// prior to this call
        /// </summary>
        /// <param name="asyncEmail">Used it you want a response from the SMTP server</param>
        /// <param name="from">The address the email is from</param>
        /// <param name="to">The address the email is to</param>
        /// <param name="cc">The addess the email is being copied to</param>
        /// <param name="bcc">The addess the email is being blind copied to</param>
        /// <param name="subject">The subject of the email</param>
        /// <param name="body">The body of the email</param>
        /// <param name="htmlEmail">Is this an html based email</param>
        /// <param name="separate">Messages are sending per email separetly</param>
        /// <param name="filePath">The path to the attachment</param>
        public void SendEmail(Boolean asyncEmail, String from, String to, String cc, String bcc, String subject, String body, Boolean htmlEmail, String filePath, Boolean separate)
        {
            try
            {
                SendSeperate(asyncEmail, from, to, cc, bcc, subject, body, htmlEmail, filePath, null, separate);
            }
            catch (SmtpException ex)
            {
                ErrNotify(ex, "Mailer SendEmail 9");
            }
        }

        /// <summary>
        /// Generates an email and sends it, requires no additional properties to be set
        /// prior to this call
        /// </summary>
        /// <param name="asyncEmail">Used it you want a response from the SMTP server</param>
        /// <param name="from">The address the email is from</param>
        /// <param name="to">The address the email is to</param>
        /// <param name="cc">The addess the email is being copied to</param>
        /// <param name="bcc">The addess the email is being blind copied to</param>
        /// <param name="subject">The subject of the email</param>
        /// <param name="body">The body of the email</param>
        /// <param name="htmlEmail">Is this an html based email</param>
        /// <param name="separate">Messages are sending per email separetly</param>
        /// <param name="attachData">An attachment created from a file resource</param>
        public void SendEmail(Boolean asyncEmail, String from, String to, String cc, String bcc, String subject, String body, Boolean htmlEmail, Attachment attachData, Boolean separate)
        {
            try
            {
                SendSeperate(asyncEmail, from, to, cc, bcc, subject, body, htmlEmail, null, attachData, separate);
            }
            catch (SmtpException ex)
            {
                ErrNotify(ex, "Mailer SendEmail 10");
            }
        }

        #endregion

        #region Private Email Methods

        /// <summary>
        /// Creates the base mail objects needed to generate an SMTP based email
        /// </summary>
        private void CreateMailObjects()
        {
            try
            {
                MailSettings();

                MailClient = new SmtpClient();
                Message = new MailMessage();
                MailFromAddress = new MailAddress(FromAddress);

                if (Port != 0)
                {
                    MailClient.Port = Port;
                }
                MailClient.Host = ServerName;

                if (MailFromAddress != null)
                {
                    Message.From = MailFromAddress;
                }

                Message.Subject = Subject;
                Message.Body = Body;
                Message.IsBodyHtml = IsHtmlEmail;
            }
            catch (SmtpException ex)
            {
                ErrNotify(ex, "Mailer CreateMailObjects");
            }
        }

        private void SendSeperate(Boolean asyncEmail, String from, String to, String cc, String bcc, String subject, String body, Boolean htmlEmail, String filePath, Attachment attachData, Boolean separate)
        {
            if (separate)
            {
                string[] arr = { to };
                string sendTo = string.Join(" ", arr);
                foreach (string _sendTo in EmailSeperatorSendMessage(sendTo))
                {
                    Send(asyncEmail, from, _sendTo, cc, bcc, subject, body, htmlEmail, filePath, attachData, separate);
                }
            }
            else
            {
                Send(asyncEmail, from, to, cc, bcc, subject, body, htmlEmail, filePath, attachData, separate);
            }
        }

        private void Send(Boolean asyncEmail, String from, String to, String cc, String bcc, String subject, String body, Boolean htmlEmail, String filePath, Attachment attachData, Boolean separate)
        {
            try
            {
                Body = body;
                Subject = subject;
                IsHtmlEmail = htmlEmail;

                if (!String.IsNullOrEmpty(from))
                {
                    FromAddress = from;
                }

                CreateMailObjects();

                if (!String.IsNullOrEmpty(to))
                {
                    AddToAddress(to);
                }

                if (!String.IsNullOrEmpty(cc))
                {
                    AddCCAddress(cc);
                }

                if (!String.IsNullOrEmpty(bcc))
                {
                    AddBCCAddress(bcc);
                }

                if (!String.IsNullOrEmpty(AuthName) && !String.IsNullOrEmpty(AuthPassword))
                {
                    AuthenticateToServer();
                }

                if (!String.IsNullOrEmpty(filePath))
                {
                    AddAttachments(filePath);
                }

                if (attachData != null)
                {
                    AddAttachments(attachData);
                }

                if (asyncEmail == true)
                {
                    MailClient.SendCompleted += new SendCompletedEventHandler(MailClient_SendCompleted);
                    MailClient.SendAsync(Message, Message.To.ToString());
                }
                else
                {
                    MailClient.Send(Message);
                }
            }
            catch (SmtpException ex)
            {
                ErrNotify(ex, "Mailer Send");
            }
        }

        static void MailClient_SendCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            String token = (string)e.UserState;

            if (e.Cancelled)
            {
                mailStatus = token + " Send canceled.";
            }
            if (e.Error != null)
            {
                mailStatus = "Error on " + token + ": " + e.Error.ToString();
            }
            else
            {
                mailStatus = token + " mail sent.";
            }

            OnNotifyCaller();
        }

        private List<string> EmailSeperatorSendMessage(string toCcBcc)
        {
            try
            {
                List<string> _emails = Useful.VariableManipulation.ExtractEmails(toCcBcc);
                List<string> emails = new List<string>();
                foreach (string email in _emails)
                {
                    if (Validation.Standard.IsValid(email, Validation.Standard.Type.EMAIL))
                    {
                        emails.Add(email);
                    }
                }
                return emails;
            }
            catch (Exception ex)
            {
                ErrNotify(ex, "Mailer EmailSeperatorSendMessage");
            }
            return null;
        }

        #endregion

        #region Authentication

        /// <summary>
        /// Creates authentication credentials that are passed to the mail client
        /// to access the SMTP server
        /// </summary>
        public void AuthenticateToServer()
        {
            NetworkCredential credentials = new NetworkCredential(AuthName, AuthPassword);
            MailClient.Credentials = credentials;
        }

        #endregion

        #region Attachments

        /// <summary>
        /// Creates an attachment and adds it to the mail message
        /// </summary>
        /// <param name="path">The path to the attachment</param>
        public void AddAttachments(string path)
        {
            Attachment attachData = new Attachment(path);
            Message.Attachments.Add(attachData);
        }

        /// <summary>
        /// Adds an attachment to the mail message
        /// </summary>
        /// <param name="attachData">An attachment object</param>
        public void AddAttachments(Attachment attachData)
        {
            Message.Attachments.Add(attachData);
        }

        #endregion

        #region Add Recipient Addresses

        private string EmailSeperator(string to)
        {
            try
            {
                List<string> emails = Useful.VariableManipulation.ExtractEmails(to);
                to = "";
                for (int i = 0; i < emails.Count; i++)
                {
                    if (i == emails.Count - 1)
                    {
                        to += emails[i] + ";";
                    }
                    else
                    {
                        to += emails[i] + "; ";
                    }
                }

            }
            catch (Exception ex)
            {
                ErrNotify(ex, "Mailer EmailSeperator");
            }
            return to;
        }

        /// <summary>
        /// Used to add a to address to the To Address collection
        /// </summary>
        /// <param name="toAddress">The email address to add</param>
        public void AddToAddress(String toAddress)
        {
            Message.To.Add(EmailSeperator(toAddress));
        }

        /// <summary>
        /// Used to add a cc address to the CC Address collection
        /// </summary>
        /// <param name="ccAddress">The email address to add</param>
        public void AddCCAddress(String ccAddress)
        {
            Message.CC.Add(ccAddress);
        }

        /// <summary>
        /// Used to add a bcc address to the Bcc Address collection
        /// </summary>
        /// <param name="bccAddress">The email address to add</param>
        public void AddBCCAddress(String bccAddress)
        {
            Message.Bcc.Add(bccAddress);
        }

        #endregion

        #region Error Mail Suport

        /// <summary>
        /// ErrNotify sends Email message with all avalible user information and error message
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="subject"></param>
        public static void ErrNotify(object ex, string subject)
        {
            string ErrorMessage = "";

            HttpContext ctx = HttpContext.Current;

            string localhost = ctx.Request.ServerVariables["LOCAL_ADDR"];
            string remotehost = ctx.Request.ServerVariables["REMOTE_ADDR"];
            string httpuseragent = ctx.Request.ServerVariables["HTTP_USER_AGENT"];
            string httpreferer = ctx.Request.ServerVariables["HTTP_REFERER"];
            string HTTPXFORWARDEDFOR = ctx.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            string HTTPFORWARDEDFOR = ctx.Request.ServerVariables["HTTP_FORWARDED_FOR"];
            string HTTPXFORWARDED = ctx.Request.ServerVariables["HTTP_X_FORWARDED"];
            string SCRIPT_NAME = ctx.Request.ServerVariables["SCRIPT_NAME"];
            string REMOTE_USER = ctx.Request.ServerVariables["REMOTE_USER"];
            string REMOTE_HOST = ctx.Request.ServerVariables["REMOTE_HOST"];
            string HTTP_CONNECTION = ctx.Request.ServerVariables["HTTP_CONNECTION"];
            string HTTP_COOKIE = ctx.Request.ServerVariables["HTTP_COOKIE"];
            string SERVER_PORT_SECURE = ctx.Request.ServerVariables["SERVER_PORT_SECURE"];
            string SERVER_PROTOCOL = ctx.Request.ServerVariables["SERVER_PROTOCOL"];
            string HTTPS_KEYSIZE = ctx.Request.ServerVariables["HTTPS_KEYSIZE"];
            string GetLastError = ex.ToString();

            ErrorMessage += "<P style='color:red;'>Error Description</P>";

            if (ex is Exception)
            {
                ErrorMessage += "<P>The error description is as follows: <br />";
                ErrorMessage += Useful.VariableManipulation.ExceptionToText((Exception)ex) + "</P>";
            }
            else
            {
                ErrorMessage += "<P>The error message: <br />";
                ErrorMessage += ex.ToString() + "</P>";
            }

            string FullName;
            string Description;
            string UserDomainName;
            string UserName;
            bool Admin;

            try
            {
                DirectoryEntry de = new DirectoryEntry("WinNT://" + Environment.UserDomainName + "/" + Environment.UserName);

                FullName = de.Properties["FullName"].Value.ToString();
                Description = de.Properties["Description"].Value.ToString();
                UserDomainName = Environment.UserDomainName;
                UserName = Environment.UserName;

                NetworkInterface[] niArr = NetworkInterface.GetAllNetworkInterfaces();


                WindowsIdentity identity = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                Admin = principal.IsInRole(WindowsBuiltInRole.Administrator);

                ErrorMessage += "<P></P>";
                ErrorMessage += "<P style='color:red;'>User Info</P>";
                ErrorMessage += "<P>Full Name: " + FullName + "</P>";
                ErrorMessage += "<P>User Name: " + UserName + "</P>";
                ErrorMessage += "<P>User Domain Name: " + UserDomainName + "</P>";
                ErrorMessage += "<P>Administrator: " + Admin + "</P>";


                ErrorMessage += "<P></P>";
                ErrorMessage += "<P style='color:red;'>Network Info</P>";
                ErrorMessage += "<P>Server Name: " + Environment.MachineName;
                ErrorMessage += "<P>Local Address: " + localhost + "</P>";
                ErrorMessage += "<P>Remote Address: " + remotehost + "</P>";
                ErrorMessage += "<P>Browser: : " + httpuseragent + "</P>";
                ErrorMessage += "<P>Remote UserName (Their LAN): " + REMOTE_USER + "</P>";
                ErrorMessage += "<P>Resolved Hostname: " + REMOTE_HOST + "</P>";
                ErrorMessage += "<P>Page: " + SCRIPT_NAME + "</P>";
                ErrorMessage += "<P>Connection Type: " + HTTP_CONNECTION + "</P>";
                ErrorMessage += "<P>Available Cookies: " + HTTP_COOKIE + "</P>";
                ErrorMessage += "<P>Secure(On/OFF): " + SERVER_PORT_SECURE + "</P>";
                ErrorMessage += "<P>HTTP Version: " + SERVER_PROTOCOL + "</P>";
                ErrorMessage += "<P>Encryption Key Size: " + HTTPS_KEYSIZE + "</P>";

                foreach (NetworkInterface tempNetworkInterface in niArr)
                {
                    string NetworkDiscription = tempNetworkInterface.Description;
                    string NetworkID = tempNetworkInterface.Id;
                    string NetworkName = tempNetworkInterface.Name;
                    string NetworkInterfaceType = tempNetworkInterface.NetworkInterfaceType.ToString();
                    string NetworkOperationalStatus = tempNetworkInterface.OperationalStatus.ToString();
                    long NetworkSpped = tempNetworkInterface.Speed;
                    bool SupportMulticast = tempNetworkInterface.SupportsMulticast;

                    ErrorMessage += "<P>Network Discription  :  " + NetworkDiscription + "</P>";
                    ErrorMessage += "<P>Network ID  :  " + NetworkID + "</P>";
                    ErrorMessage += "<P>Network Name  :  " + NetworkName + "</P>";
                    ErrorMessage += "<P>Network interface type  :  " + NetworkInterfaceType + "</P>";
                    ErrorMessage += "<P>Network Operational Status   :   " + NetworkOperationalStatus + "</P>";
                    ErrorMessage += "<P>Network Spped   :   " + NetworkSpped + "</P>";
                    ErrorMessage += "<P>Support Multicast   :   " + SupportMulticast + "</P>";
                }
            }
            catch { }

            try
            {
                MailMessage m = new MailMessage();
                m.IsBodyHtml = true;
                m.From = new MailAddress("lib@JureHr.ca");
                m.To.Add(new MailAddress("jglavinic@tph.ca"));

                m.Subject = subject;
                m.Body = ErrorMessage;

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    byte[] contentAsBytes = Encoding.UTF8.GetBytes(XMLDoc.ToString(Network.HttpInfoGET()));
                    memoryStream.Write(contentAsBytes, 0, contentAsBytes.Length);

                    // Set the position to the beginning of the stream.
                    memoryStream.Seek(0, SeekOrigin.Begin);

                    // Create attachment
                    ContentType contentType = new ContentType();
                    contentType.MediaType = MediaTypeNames.Application.Octet;
                    contentType.Name = "HTTP_Info.xml";
                    Attachment attachment = new Attachment(memoryStream, contentType);

                    MailSettings();

                    // Add the attachment
                    m.Attachments.Add(attachment);

                    // Send Mail via SmtpClient
                    SmtpClient sc = new SmtpClient(ServerName);
                    sc.Send(m);
                }
            }
            catch (Exception Ex)
            {
                if (ex is Exception)
                    ApplicationLog.WriteError(subject, (Exception)ex);
                else
                    ApplicationLog.WriteError(subject, ex.ToString());
                ApplicationLog.WriteError("ErrNotify", Ex);
            }
        }

        #endregion

        #region Notification Email Suport

        public static void SendNotifation(string body, string subject)
        {

            MailMessage m = new MailMessage();
            m.IsBodyHtml = true;
            m.From = new MailAddress("lib@JureHr.ca");
            m.To.Add(new MailAddress("jglavinic@tph.ca"));

            m.Subject = subject;
            m.Body = body;

            MailSettings();

            // Send Mail via SmtpClient
            SmtpClient sc = new SmtpClient(ServerName);
            try
            {
                sc.Send(m);
            }
            catch (Exception ex)
            {
                ApplicationLog.WriteError("SendNotifation", ex);
            }
        }

        #endregion
    }
}
