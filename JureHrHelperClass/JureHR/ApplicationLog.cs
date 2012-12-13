using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Web;

namespace JureHR
{
    /// <summary>
    ///     Logging class to provide tracing and logging support. 
    ///     <remarks>
    ///         There are four different logging levels (error, warning, info, trace) 
    ///         that produce output to either the system event log or a tracing 
    ///         file as specified in the current ApplicationConfiguration settings.
    ///     </remarks>
    /// </summary>
    public class ApplicationLog
    {
        #region Public Methods

        /// <summary>
        ///     Write at the Error level to the event log and/or tracing file.
        ///     <param name="source">The source of error in event log.</param>
        ///     <param name="message">The text to write to the log file or event log.</param>
        /// </summary>
        public static void WriteError(string source, object message)
        {
            //Defer to the helper function to log the message.
            WriteLog(source, TraceLevel.Error, message);
        }

        /// <summary>
        ///     Write at the Warning level to the event log and/or tracing file.
        ///     <param name="source">The source of error in event log.</param>
        ///     <param name="message">The text to write to the log file or event log.</param>
        /// </summary>
        public static void WriteWarning(string source, object message)
        {
            //Defer to the helper function to log the message.
            WriteLog(source, TraceLevel.Warning, message);
        }

        /// <summary>
        ///     Write at the Info level to the event log and/or tracing file.
        ///     <param name="source">The source of error in event log.</param>
        ///     <param name="message">The text to write to the log file or event log.</param>
        /// </summary>
        public static void WriteInfo(string source, object message)
        {
            //Defer to the helper function to log the message.
            WriteLog(source, TraceLevel.Info, message);
        }

        /// <summary>
        ///     Write at the Verbose level to the event log and/or tracing file.
        ///     <param name="source">The source of error in event log.</param>
        ///     <param name="message">The text to write to the log file or event log.</param>
        /// </summary>
        public static void WriteTrace(string source, object message)
        {
            //Defer to the helper function to log the message.
            WriteLog(source, TraceLevel.Verbose, message);
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Determine where a string needs to be written based on the
        ///     configuration settings and the error level.
        ///     <param name="source">The source of error in event log.</param>
        ///     <param name="level">The severity of the information to be logged.</param>
        ///     <param name="message">The string to be logged.</param>
        /// </summary>
        private static void WriteLog(string source, TraceLevel level, object message)
        {
            try
            {
                // Map the trace level to the corresponding event log attribute.
                EventLogEntryType LogEntryType;
                switch (level)
                {
                    case TraceLevel.Error:
                        LogEntryType = EventLogEntryType.Error;
                        break;
                    case TraceLevel.Warning:
                        LogEntryType = EventLogEntryType.Warning;
                        break;
                    case TraceLevel.Info:
                        LogEntryType = EventLogEntryType.Information;
                        break;
                    case TraceLevel.Verbose:
                        LogEntryType = EventLogEntryType.SuccessAudit;
                        break;
                    default:
                        LogEntryType = EventLogEntryType.SuccessAudit;
                        break;
                }
                
                if (!EventLog.SourceExists(GetLogName()))
                {
                    //An event log source should not be created and immediately used.
                    //There is a latency time to enable the source, it should be created
                    //prior to executing the application that uses the source.
                    //Execute this sample a second time to use the new source.
                    EventLog.CreateEventSource(source, GetLogName());
                    // The source is created.  Exit the application to allow it to be registered.
                    //return;
                }
                EventLog eventLog = new EventLog();
                eventLog.Source = source;
                eventLog.Log = GetLogName();//System.Reflection.Assembly.GetExecutingAssembly().FullName

                string info = string.Empty;
                if (message is Exception)
                {
                    info = GetStackTrace() + AditionalInfo() + message;
                }
                else
                {
                    info = GetStackTrace() + ExceptionToText((Exception)message);
                }

                eventLog.WriteEntry(info, LogEntryType);
            }
            catch //(Exception ex)
            {
                //Mailer.ErrNotify(ex, "ApplicationLog WriteLog");
            }
        }

        /// <summary>
        /// Exception Info
        /// </summary>
        /// <param name="exc"></param>
        /// <returns></returns>
        private static string ExceptionToText(Exception exc)
        {
            StringBuilder str = new StringBuilder();
            str.Append(AditionalInfo());
            str.Append("Exception InnerException ToString: " + ((exc.InnerException == null) ? "" : exc.InnerException.ToString()) + Environment.NewLine);
            str.Append("Exception InnerException Message: " + ((exc.InnerException == null) ? "" : exc.InnerException.Message) + Environment.NewLine);
            str.Append("Exception InnerException Source: " + ((exc.InnerException == null) ? "" : exc.InnerException.Source) + Environment.NewLine);
            str.Append("Exception ToString: " + ((exc == null) ? "" : exc.ToString()) + Environment.NewLine);
            str.Append("Exception Source: " + ((exc == null) ? "" : exc.Source) + Environment.NewLine);
            str.Append("Exception Message: " + ((exc == null) ? "" : exc.Message));
            str.Append("TargetSite: " + ((exc == null) ? "" : exc.TargetSite.ToString()) + Environment.NewLine);
            str.Append("Exception StackTrace: " + ((exc == null) ? "" : exc.StackTrace) + Environment.NewLine);
            return str.ToString();
        }

        /// <summary>
        /// Gets information about origin of error
        /// </summary>
        /// <returns></returns>
        private static string GetStackTrace()
        {
            StackTrace st = new StackTrace(new StackFrame(3, true));
            StackFrame sf = st.GetFrame(0);
            StringBuilder str = new StringBuilder();
            str.Append("File: " + sf.GetFileName() + Environment.NewLine);
            str.Append("Method: " + sf.GetMethod().Name + Environment.NewLine);
            str.Append("Line Number: " + sf.GetFileLineNumber() + Environment.NewLine);
            str.Append("Column Number:" + sf.GetFileColumnNumber() + Environment.NewLine + Environment.NewLine);

            return str.ToString();
        }

        /// <summary>
        /// User and server information
        /// </summary>
        /// <returns></returns>
        private static string AditionalInfo()
        {
            StringBuilder str = new StringBuilder();
            str.Append("DateTimeNow: " + DateTime.Now.ToString("MM/dd/yyyy h:mm:ss tt") + Environment.NewLine);
            str.Append("UserIPAddress: " + Network.UserIPAddress() + Environment.NewLine);
            str.Append("WebServerName: " + Network.GetDnsServerName() + Environment.NewLine);
            str.Append("Url: " + HttpContext.Current.Request.Url.ToString() + Environment.NewLine);

            return str.ToString();
        }

        /// <summary>
        /// Get name from web.config
        /// </summary>
        /// <returns></returns>
        private static string GetLogName()
        {
            string name = string.Empty;
            if (ConfigurationManager.AppSettings["Log_Name"] != null)
            {
                name = ConfigurationManager.AppSettings["Log_Name"];
            }
            else
            {
                name = "JureHR Library";
            }
            return name;
        }

        #endregion
    }
}
