using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Diagnostics;
using System.Net.Sockets;
using System.Security;
using System.Net.NetworkInformation;
using System.Xml;
using System.Xml.XPath;
using System.Threading;
using System.Security.Principal;
using System.Web;
using System.Collections;
using System.Collections.Specialized;

namespace JureHR
{
    /// <summary>
    /// Class Network
    /// </summary>
    public class Network
    {
        /// <summary>
        /// Get Host name by IP
        /// </summary>
        /// <param name="IPAdress"></param>
        /// <returns>Host name</returns>
        public static string GetHostbyIP(string IPAdress)
        {
            IPHostEntry IPHostEntryObject = Dns.GetHostEntry(IPAdress);

            return IPHostEntryObject.HostName;
        }

        /// <summary>
        /// returns HTTP source to a string
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public string GetHttpSource(string url)
        {
            try
            {
                StringBuilder sb = new StringBuilder();

                // used on each read operation
                byte[] buf = new byte[8192];

                // prepare the web page we will be asking for
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                // we will read data via the response stream
                Stream resStream = response.GetResponseStream();

                string tempString = null;
                int count = 0;

                do
                {
                    // fill the buffer with data
                    count = resStream.Read(buf, 0, buf.Length);

                    // make sure we read some data
                    if (count != 0)
                    {
                        // translate from bytes to ASCII text
                        tempString = Encoding.ASCII.GetString(buf, 0, count);

                        // continue building the string
                        sb.Append(tempString);
                    }
                }
                while (count > 0); // any more data to read?

                // print out page source
                return sb.ToString();
            }
            catch (Exception ex)
            {
                Mailer.ErrNotify(ex, "Network GetHttpSource");              
                return null;
            }
        }

        /// <summary>
        /// check if host is alive
        /// </summary>
        /// <param name="svr"></param>
        /// <returns></returns>
        public bool IsAlive(string svr)
        {
            // create the ping process
            Process ping = new Process();
            ping.StartInfo.FileName = "ping.exe";
            ping.StartInfo.CreateNoWindow = true;
            ping.StartInfo.UseShellExecute = false;
            ping.StartInfo.RedirectStandardError = true;
            ping.StartInfo.RedirectStandardOutput = true;

            // add arguments to the ping process
            ping.StartInfo.Arguments = "-n 1 -w 1500 " + svr;

            ping.Start();
            ping.WaitForExit();

            // store ping's sdtOut
            string tmp = ping.StandardOutput.ReadToEnd();
            ping.Dispose();

            // check DNS
            if (tmp.Contains("could not find"))
            {
                return false;
            }

            // check to see if host is alive
            if (tmp.Contains("TTL="))
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        /// <summary>
        /// check TCP Port
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public bool IsAlive(string ipAddress, int port)
        {
            try
            {
                //create the socket instance...
                Socket m_socClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                // get the remote IP address...
                IPAddress ip = IPAddress.Parse(ipAddress);
                int iPortNo = port;

                //create the end point 
                IPEndPoint ipEnd = new IPEndPoint(ip, iPortNo);

                //connect to the remote host...
                m_socClient.Connect(ipEnd);

                m_socClient.Close();
                return true;
            }
            catch (SocketException se)
            {

                Mailer.ErrNotify(se, "Network IsAlive"); 
                return false;
            } 
        } 

        /// <summary>
        /// shutdown host
        /// </summary>
        /// <param name="svr"></param>
        /// <returns></returns>
        public bool KillHost(string svr)
        {
            // create the shutdown process
            Process shutdown = new Process();
            shutdown.StartInfo.FileName = "shutdown.exe";
            shutdown.StartInfo.CreateNoWindow = true;
            shutdown.StartInfo.UseShellExecute = false;
            shutdown.StartInfo.RedirectStandardOutput = true;
            shutdown.StartInfo.RedirectStandardError = true;

            // Add arguments to the shutdown command
            shutdown.StartInfo.Arguments = "-f -s -t 0 /m " + svr;

            try
            {
                shutdown.Start();
                shutdown.WaitForExit();

                // store the stdOut from shutdown
                string tmp = shutdown.StandardError.ReadToEnd();

                if (tmp != "")
                {
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Mailer.ErrNotify(ex, "Network KillHost");
                return false;
            }
        }

        /// <summary>
        /// shutdown host using password
        /// </summary>
        /// <param name="svr"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public bool KillHost(string svr, string username, string password)
        {
            // create the shutdown process
            Process shutdown = new Process();
            shutdown.StartInfo.FileName = "shutdown.exe";
            shutdown.StartInfo.UserName = username;
            SecureString pww = new SecureString();

            for (int i = 0; i < password.Length; i++)
            {
                pww.AppendChar(password[i]);
            }
            shutdown.StartInfo.Password = pww;
            shutdown.StartInfo.CreateNoWindow = true;
            shutdown.StartInfo.UseShellExecute = false;
            shutdown.StartInfo.RedirectStandardOutput = true;
            shutdown.StartInfo.RedirectStandardError = true;

            // Add arguments to the shutdown command
            shutdown.StartInfo.Arguments = "-f -s -t 0 /m " + svr;

            try
            {
                shutdown.Start();
                shutdown.WaitForExit();

                // store the stdOut from shutdown
                string tmp = shutdown.StandardError.ReadToEnd();

                if (tmp != "")
                {
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                Mailer.ErrNotify(ex, "Network KillHost");
                return false;
            }
        } 

        /// <summary>
        /// map network drive
        /// </summary>
        /// <param name="letter"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public bool MapDrive(char letter, string path)
        {
            Process net = new Process();
            net.StartInfo.FileName = "net.exe";
            net.StartInfo.Arguments = "use " + letter + ": " + path;
            net.StartInfo.CreateNoWindow = true;
            net.StartInfo.UseShellExecute = false;
            net.StartInfo.RedirectStandardError = true;
            net.Start();
            net.WaitForExit();
            string res = net.StandardError.ReadToEnd();

            if (res != "")
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Get Dns Server Name
        /// </summary>
        /// <returns></returns>
        public static string GetDnsServerName()
        {
            string result = "";
            try
            {
                result = ((!string.IsNullOrEmpty(HttpContext.Current.Request.ServerVariables["HTTP_HOST"].ToString())) ? HttpContext.Current.Request.ServerVariables["HTTP_HOST"].ToString() : "");
            }
            catch
            {
            }
            return result;
        }

        /// <summary>
        /// loop over them to find one which has an OperationalStatus of Up, then get the IPInterfaceProperties of the active NetworkInterface, then get the DNS Addresses from the IPInterfaceProperties and then return the first value.
        /// </summary>
        /// <returns>ip adderss</returns>
        public static IPAddress GetDnsAdress()
        {
            NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

            foreach (NetworkInterface networkInterface in networkInterfaces)
            {
                if (networkInterface.OperationalStatus == OperationalStatus.Up)
                {
                    IPInterfaceProperties ipProperties = networkInterface.GetIPProperties();
                    IPAddressCollection dnsAddresses = ipProperties.DnsAddresses;

                    foreach (IPAddress dnsAdress in dnsAddresses)
                    {
                        return dnsAdress;
                    }
                }
            }
            Mailer.ErrNotify("Unable to find DNS Address", "Network KillHost");
            return null;
        }

        /// <summary>
        /// Get user IP Address
        /// </summary>
        /// <returns></returns>
        public static string UserIPAddress()
        {
            string text = "";
            try
            {
                text = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                if (text == null || text == "" || text.ToLower() == "unknown")
                {
                    text = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
                }
            }
            catch { }
            return text;
        }

        /// <summary>
        /// get the all the Urls from an XML Sitemap using XPath. 
        /// </summary>
        /// <param name="url"></param>
        /// <returns>list of avalible url's</returns>
        private IEnumerable<string> GetUrls(string url)
        {
            List<string> urls = new List<string>();
            XmlReader xmlReader = new XmlTextReader(string.Format("{0}sitemap.xml", url));
            XPathDocument document = new XPathDocument(xmlReader);
            XPathNavigator navigator = document.CreateNavigator();

            XmlNamespaceManager resolver = new XmlNamespaceManager(xmlReader.NameTable);
            resolver.AddNamespace("sitemap", "http://www.google.com/schemas/sitemap/0.9");

            XPathNodeIterator iterator = navigator.Select("/sitemap:urlset/sitemap:url/sitemap:loc", resolver);

            while (iterator.MoveNext())
            {
                if (iterator.Current == null)
                    continue;

                urls.Add(iterator.Current.Value);
            }

            return urls;
        }

        /// <summary>
        /// Get All Posible Http Info
        /// </summary>
        /// <returns></returns>
        public static XmlDocument HttpInfoGET()
        {
            XmlDocument xmlDocument = new XmlDocument();
            XmlElement xmlElement = xmlDocument.CreateElement("root");
            XmlElement newChild = xmlDocument.CreateElement("System.Threading.Thread.CurrentPrincipal.Identity");
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild, "AuthenticationType", Thread.CurrentPrincipal.Identity.AuthenticationType);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild, "IsAuthenticated", Thread.CurrentPrincipal.Identity.IsAuthenticated);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild, "Name", Thread.CurrentPrincipal.Identity.Name);
            xmlElement.AppendChild(newChild);
            XmlElement newChild2 = xmlDocument.CreateElement("System.Security.Principal.WindowsIdentity.GetCurrent");
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild2, "AuthenticationType", WindowsIdentity.GetCurrent().AuthenticationType);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild2, "ImpersonationLevel", WindowsIdentity.GetCurrent().ImpersonationLevel);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild2, "IsAnonymous", WindowsIdentity.GetCurrent().IsAnonymous);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild2, "IsAuthenticated", WindowsIdentity.GetCurrent().IsAuthenticated);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild2, "IsGuest", WindowsIdentity.GetCurrent().IsGuest);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild2, "IsSystem", WindowsIdentity.GetCurrent().IsSystem);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild2, "Name", WindowsIdentity.GetCurrent().Name);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild2, "Owner", WindowsIdentity.GetCurrent().Owner);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild2, "Token", WindowsIdentity.GetCurrent().Token);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild2, "User", WindowsIdentity.GetCurrent().User);
            xmlElement.AppendChild(newChild2);
            XmlElement newChild3 = xmlDocument.CreateElement("System.Web.HttpContext.Current.Request");
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild3, "AnonymousID", HttpContext.Current.Request.AnonymousID);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild3, "ApplicationPath", HttpContext.Current.Request.ApplicationPath);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild3, "ContentEncoding", HttpContext.Current.Request.ContentEncoding);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild3, "FilePath", HttpContext.Current.Request.FilePath);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild3, "HttpMethod", HttpContext.Current.Request.HttpMethod);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild3, "IsAuthenticated", HttpContext.Current.Request.IsAuthenticated);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild3, "IsLocal", HttpContext.Current.Request.IsLocal);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild3, "IsSecureConnection", HttpContext.Current.Request.IsSecureConnection);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild3, "LogonUserIdentity", HttpContext.Current.Request.LogonUserIdentity);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild3, "Path", HttpContext.Current.Request.Path);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild3, "PathInfo", HttpContext.Current.Request.PathInfo);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild3, "PhysicalApplicationPath", HttpContext.Current.Request.PhysicalApplicationPath);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild3, "PhysicalPath", HttpContext.Current.Request.PhysicalPath);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild3, "RawUrl", HttpContext.Current.Request.RawUrl);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild3, "RequestType", HttpContext.Current.Request.RequestType);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild3, "Url", HttpContext.Current.Request.Url);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild3, "UrlReferrer", HttpContext.Current.Request.UrlReferrer);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild3, "UserAgent", HttpContext.Current.Request.UserAgent);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild3, "UserHostAddress", HttpContext.Current.Request.UserHostAddress);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild3, "UserHostName", HttpContext.Current.Request.UserHostName);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild3, "UserLanguages", HttpContext.Current.Request.UserLanguages[0]);
            xmlElement.AppendChild(newChild3);
            XmlElement newChild4 = xmlDocument.CreateElement("System.Web.HttpContext.Current.Request.ServerVariables");
            NameValueCollection serverVariables = HttpContext.Current.Request.ServerVariables;
            string[] allKeys = serverVariables.AllKeys;
            for (int i = 0; i < allKeys.Length; i++)
            {
                string name = allKeys[i];
                XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild4, name, serverVariables[name]);
            }
            xmlElement.AppendChild(newChild4);
            XmlElement newChild5 = xmlDocument.CreateElement("System.Environment");
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild5, "CommandLine", Environment.CommandLine);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild5, "CurrentDirectory", Environment.CurrentDirectory);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild5, "MachineName", Environment.MachineName);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild5, "NewLine", Environment.NewLine);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild5, "OSVersion", Environment.OSVersion);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild5, "ProcessorCount", Environment.ProcessorCount);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild5, "SystemDirectory", Environment.SystemDirectory);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild5, "TickCount", Environment.TickCount);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild5, "UserDomainName", Environment.UserDomainName);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild5, "UserName", Environment.UserName);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild5, "WorkingSet", Environment.WorkingSet);
            xmlElement.AppendChild(newChild5);
            XmlElement newChild6 = xmlDocument.CreateElement("System.Environment.GetEnvironmentVariables");
            IDictionary environmentVariables = Environment.GetEnvironmentVariables();
            foreach (DictionaryEntry dictionaryEntry in environmentVariables)
            {
                XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild6, dictionaryEntry.Key.ToString(), dictionaryEntry.Value);
            }
            xmlElement.AppendChild(newChild6);
            XmlElement newChild7 = xmlDocument.CreateElement("System.Web.HttpContext.Current.Request.Browser");
            HttpBrowserCapabilities browser = HttpContext.Current.Request.Browser;
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild7, "ActiveXControls", browser.ActiveXControls);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild7, "Adapters", browser.Adapters);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild7, "AOL", browser.AOL);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild7, "BackgroundSounds", browser.BackgroundSounds);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild7, "Beta", browser.Beta);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild7, "Browser", browser.Browser);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild7, "Browsers", browser.Browsers);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild7, "CanCombineFormsInDeck", browser.CanCombineFormsInDeck);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild7, "CanInitiateVoiceCall", browser.CanInitiateVoiceCall);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild7, "CanRenderAfterInputOrSelectElement", browser.CanRenderAfterInputOrSelectElement);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild7, "CanRenderEmptySelects", browser.CanRenderEmptySelects);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild7, "CanRenderInputAndSelectElementsTogether", browser.CanRenderInputAndSelectElementsTogether);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild7, "CanRenderMixedSelects", browser.CanRenderMixedSelects);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild7, "CanRenderOneventAndPrevElementsTogether", browser.CanRenderOneventAndPrevElementsTogether);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild7, "CanRenderPostBackCards", browser.CanRenderPostBackCards);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild7, "CanRenderSetvarZeroWithMultiSelectionList", browser.CanRenderSetvarZeroWithMultiSelectionList);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild7, "CanSendMail", browser.CanSendMail);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild7, "Capabilities", browser.Capabilities);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild7, "CDF", browser.CDF);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild7, "ClrVersion", browser.ClrVersion);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild7, "Cookies", browser.Cookies);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild7, "Crawler", browser.Crawler);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild7, "DefaultSubmitButtonLimit", browser.DefaultSubmitButtonLimit);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild7, "EcmaScriptVersion", browser.EcmaScriptVersion);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild7, "Frames", browser.Frames);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild7, "GatewayMajorVersion", browser.GatewayMajorVersion);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild7, "GatewayMinorVersion", browser.GatewayMinorVersion);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild7, "GatewayVersion", browser.GatewayVersion);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild7, "HasBackButton", browser.HasBackButton);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild7, "HidesRightAlignedMultiselectScrollbars", browser.HidesRightAlignedMultiselectScrollbars);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild7, "HtmlTextWriter", browser.HtmlTextWriter);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild7, "Id", browser.Id);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild7, "InputType", browser.InputType);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild7, "IsColor", browser.IsColor);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild7, "IsMobileDevice", browser.IsMobileDevice);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild7, "JavaApplets", browser.JavaApplets);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild7, "JScriptVersion", browser.JScriptVersion);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild7, "MajorVersion", browser.MajorVersion);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild7, "MaximumHrefLength", browser.MaximumHrefLength);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild7, "MaximumRenderedPageSize", browser.MaximumRenderedPageSize);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild7, "MaximumSoftkeyLabelLength", browser.MaximumSoftkeyLabelLength);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild7, "MinorVersion", browser.MinorVersion);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild7, "MinorVersionString", browser.MinorVersionString);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild7, "MobileDeviceManufacturer", browser.MobileDeviceManufacturer);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild7, "MobileDeviceModel", browser.MobileDeviceModel);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild7, "MSDomVersion", browser.MSDomVersion);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild7, "NumberOfSoftkeys", browser.NumberOfSoftkeys);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild7, "Platform", browser.Platform);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild7, "PreferredImageMime", browser.PreferredImageMime);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild7, "PreferredRenderingMime", browser.PreferredRenderingMime);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild7, "PreferredRenderingType", browser.PreferredRenderingType);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild7, "PreferredRequestEncoding", browser.PreferredRequestEncoding);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild7, "PreferredResponseEncoding", browser.PreferredResponseEncoding);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild7, "RendersBreakBeforeWmlSelectAndInput", browser.RendersBreakBeforeWmlSelectAndInput);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild7, "RendersBreaksAfterHtmlLists", browser.RendersBreaksAfterHtmlLists);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild7, "RendersBreaksAfterWmlAnchor", browser.RendersBreaksAfterWmlAnchor);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild7, "RendersBreaksAfterWmlInput", browser.RendersBreaksAfterWmlInput);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild7, "RendersWmlDoAcceptsInline", browser.RendersWmlDoAcceptsInline);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild7, "RendersWmlSelectsAsMenuCards", browser.RendersWmlSelectsAsMenuCards);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild7, "RequiredMetaTagNameValue", browser.RequiredMetaTagNameValue);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild7, "RequiresAttributeColonSubstitution", browser.RequiresAttributeColonSubstitution);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild7, "RequiresContentTypeMetaTag", browser.RequiresContentTypeMetaTag);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild7, "RequiresControlStateInSession", browser.RequiresControlStateInSession);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild7, "RequiresDBCSCharacter", browser.RequiresDBCSCharacter);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild7, "RequiresHtmlAdaptiveErrorReporting", browser.RequiresHtmlAdaptiveErrorReporting);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild7, "RequiresLeadingPageBreak", browser.RequiresLeadingPageBreak);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild7, "RequiresNoBreakInFormatting", browser.RequiresNoBreakInFormatting);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild7, "RequiresOutputOptimization", browser.RequiresOutputOptimization);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild7, "RequiresPhoneNumbersAsPlainText", browser.RequiresPhoneNumbersAsPlainText);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild7, "RequiresSpecialViewStateEncoding", browser.RequiresSpecialViewStateEncoding);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild7, "RequiresUniqueFilePathSuffix", browser.RequiresUniqueFilePathSuffix);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild7, "RequiresUniqueHtmlCheckboxNames", browser.RequiresUniqueHtmlCheckboxNames);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild7, "RequiresUniqueHtmlInputNames", browser.RequiresUniqueHtmlInputNames);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild7, "RequiresUrlEncodedPostfieldValues", browser.RequiresUrlEncodedPostfieldValues);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild7, "ScreenBitDepth", browser.ScreenBitDepth);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild7, "ScreenCharactersHeight", browser.ScreenCharactersHeight);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild7, "ScreenCharactersWidth", browser.ScreenCharactersWidth);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild7, "ScreenPixelsHeight", browser.ScreenPixelsHeight);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild7, "ScreenPixelsWidth", browser.ScreenPixelsWidth);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild7, "SupportsAccesskeyAttribute", browser.SupportsAccesskeyAttribute);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild7, "SupportsBodyColor", browser.SupportsBodyColor);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild7, "SupportsBold", browser.SupportsBold);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild7, "SupportsCacheControlMetaTag", browser.SupportsCacheControlMetaTag);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild7, "SupportsCallback", browser.SupportsCallback);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild7, "SupportsCss", browser.SupportsCss);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild7, "SupportsDivAlign", browser.SupportsDivAlign);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild7, "SupportsDivNoWrap", browser.SupportsDivNoWrap);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild7, "SupportsEmptyStringInCookieValue", browser.SupportsEmptyStringInCookieValue);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild7, "SupportsFontColor", browser.SupportsFontColor);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild7, "SupportsFontName", browser.SupportsFontName);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild7, "SupportsFontSize", browser.SupportsFontSize);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild7, "SupportsImageSubmit", browser.SupportsImageSubmit);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild7, "SupportsIModeSymbols", browser.SupportsIModeSymbols);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild7, "SupportsInputIStyle", browser.SupportsInputIStyle);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild7, "SupportsInputMode", browser.SupportsInputMode);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild7, "SupportsItalic", browser.SupportsItalic);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild7, "SupportsJPhoneMultiMediaAttributes", browser.SupportsJPhoneMultiMediaAttributes);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild7, "SupportsJPhoneSymbols", browser.SupportsJPhoneSymbols);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild7, "SupportsQueryStringInFormAction", browser.SupportsQueryStringInFormAction);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild7, "SupportsRedirectWithCookie", browser.SupportsRedirectWithCookie);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild7, "SupportsSelectMultiple", browser.SupportsSelectMultiple);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild7, "SupportsUncheck", browser.SupportsUncheck);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild7, "SupportsXmlHttp", browser.SupportsXmlHttp);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild7, "Tables", browser.Tables);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild7, "TagWriter", browser.TagWriter);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild7, "Type", browser.Type);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild7, "UseOptimizedCacheKey", browser.UseOptimizedCacheKey);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild7, "VBScript", browser.VBScript);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild7, "Version", browser.Version);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild7, "W3CDomVersion", browser.W3CDomVersion);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild7, "Win16", browser.Win16);
            XMLDoc.XmlElement.AddXmlElement(xmlDocument, ref newChild7, "Win32", browser.Win32);
            xmlElement.AppendChild(newChild7);
            xmlDocument.AppendChild(xmlElement);
            return xmlDocument;
        }
    }
}
