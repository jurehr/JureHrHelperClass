using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Text.RegularExpressions;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Net;
using System.Collections.Specialized;
using System.Web;

namespace JureHR
{
    /// <summary>
    /// Class Of Useful Functions
    /// </summary>
    public static class Useful
    {
        /// <summary>
        /// Class Conversions, Time, Temperature, Size
        /// </summary>
        public static class Conversions
        {
            #region Conversions

            /// <summary>
            ///  Date Time To Oracle
            /// </summary>
            /// <param name="dt"></param>
            /// <returns>Formates DateTime to Oracle format</returns>
            public static string DateTimeToOracle(DateTime dt)
            {
                string[] monthes = { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
                string res = "";
                res += "'";
                res += dt.Day;
                res += "-";
                res += monthes[dt.Month - 1];
                res += "-";
                res += dt.Year;
                res += "'";

                return res;
            }

            /// <summary>
            ///  Date Time To String
            /// <para>DateTime to string value</para>
            /// </summary>
            /// <param name="dt"></param>
            /// <returns>String value</returns>
            public static string DateToStr(DateTime dt)
            {
                string res = "";
                if (dt.Day < 10)
                    res += "0";
                res += dt.Day;
                res += "/";
                if (dt.Month < 10)
                    res += "0";
                res += dt.Month;
                res += "/";
                res += dt.Year;

                return res;
            }

            /// <summary>
            /// Oracle Date To Str
            /// <para>Convert Oracle to date string</para>
            /// </summary>
            /// <param name="str"></param>
            /// <returns>Date string</returns>
            public static string OracleDateToStr(string str)
            {
                string temp = str;
                string res = "";

                string monthStr = "";
                string dayStr = "";

                if (temp[1] == '/')
                {
                    monthStr += "0";
                    monthStr += temp.Substring(0, 2);
                    temp = temp.Substring(2);
                }
                else
                {
                    monthStr += temp.Substring(0, 3);
                    temp = temp.Substring(3);
                }

                if (temp[1] == '/')
                {
                    dayStr += "0";
                    dayStr += temp.Substring(0, 2);
                    temp = temp.Substring(2);
                }
                else
                {
                    dayStr += temp.Substring(0, 3);
                    temp = temp.Substring(3);
                }

                res += dayStr;
                res += monthStr;
                res += temp.Substring(0, 4);

                return res;
            }

            /// <summary>
            /// Celsius To Fahrenheit
            /// </summary>
            /// <param name="temperatureCelsius"></param>
            /// <returns></returns>
            public static double CelsiusToFahrenheit(string temperatureCelsius)
            {
                // Convert argument to double for calculations.
                double celsius = System.Double.Parse(temperatureCelsius);

                // Convert Celsius to Fahrenheit.
                double fahrenheit = (celsius * 9 / 5) + 32;

                return fahrenheit;
            }

            /// <summary>
            /// Fahrenheit To Celsius
            /// </summary>
            /// <param name="temperatureFahrenheit"></param>
            /// <returns></returns>
            public static double FahrenheitToCelsius(string temperatureFahrenheit)
            {
                // Convert argument to double for calculations.
                double fahrenheit = System.Double.Parse(temperatureFahrenheit);

                // Convert Fahrenheit to Celsius.
                double celsius = (fahrenheit - 32) * 5 / 9;

                return celsius;
            }

            #endregion
        }

        /// <summary>
        /// Class Random for, numbers, string, color, boll
        /// </summary>
        public static class Random
        {
            #region Random

            private static System.Random randomSeed = new System.Random();

            /// <summary>
            /// Generate Number
            /// <para>Generates random number and returnes as string</para>
            /// </summary>
            /// <param name="numLen">Length of generated number</param>
            /// <returns>Generated number of specific length</returns>
            public static string GenerateNumber(int numLen)
            {
                System.Random r = new System.Random();
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < numLen; i++)
                { sb.Append(r.Next(0, 9)); }
                return (sb.ToString());
            }

            /// <summary>
            /// Generates a random string with the given length
            /// </summary>
            /// <param name="size">Size of the string</param>
            /// <param name="lowerCase">If true, generate lowercase string</param>
            /// <returns>Random string</returns>
            public static string RandomString(int size, bool lowerCase)
            {
                // StringBuilder is faster than using strings (+=)
                StringBuilder RandStr = new StringBuilder(size);

                // Ascii start position (65 = A / 97 = a)
                int Start = (lowerCase) ? 97 : 65;

                // Add random chars
                for (int i = 0; i < size; i++)
                    RandStr.Append((char)(26 * randomSeed.NextDouble() + Start));

                return RandStr.ToString();
            }

            /// <summary>
            /// Returns a random number.
            /// </summary>
            /// <param name="Minimal">Minimal result</param>
            /// <param name="Maximal">Maximal result</param>
            /// <returns>Random number</returns>
            public static int RandomNumber(int Minimal, int Maximal)
            {
                return randomSeed.Next(Minimal, Maximal);
            }

            /// <summary>
            /// Returns a random boolean value
            /// </summary>
            /// <returns>Random boolean value</returns>
            public static bool RandomBool()
            {
                return (randomSeed.NextDouble() > 0.5);
            }

            /// <summary>
            /// Returns a random color
            /// </summary>
            /// <returns></returns>
            public static System.Drawing.Color RandomColor()
            {
                return System.Drawing.Color.FromArgb(
                    randomSeed.Next(256),
                    randomSeed.Next(256),
                    randomSeed.Next(256)
                );
            }

            /// <summary>
            /// Returns a random date
            /// </summary>
            /// <returns></returns>
            public static DateTime RandomDate()
            {
                return RandomDate(new DateTime(1900, 1, 1), DateTime.Now);
            }

            /// <summary>
            /// Returns a random date
            /// </summary>
            /// <param name="from"></param>
            /// <param name="to"></param>
            /// <returns></returns>
            public static DateTime RandomDate(DateTime from, DateTime to)
            {
                TimeSpan range = new TimeSpan(to.Ticks - from.Ticks);
                return from + new TimeSpan((long)(range.Ticks * randomSeed.NextDouble()));
            }

            #endregion
        }

        /// <summary>
        /// Class Variable Manipulation for, conversion, cleaning and formating
        /// </summary>
        public static class VariableManipulation
        {
            #region Varable Manipulation

            #region Integer Manipulation

            /// <summary>
            /// Extract Numbers
            /// <para>Data mining for numeric values done by set of successful matches found by iteratively applying a regular expression pattern to the input string</para>
            /// </summary>
            /// <param name="str"></param>
            /// <returns></returns>
            public static List<string> ExtractNumbers(string str)
            {
                // Find matches
                System.Text.RegularExpressions.MatchCollection matches
                    = System.Text.RegularExpressions.Regex.Matches(str, @"(\d+\.?\d*|\.\d+)");

                List<string> MatchList = new List<string>();

                // add each match
                foreach (System.Text.RegularExpressions.Match match in matches)
                {
                    MatchList.Add(match.ToString());
                }

                return MatchList;
            }

            /// <summary>
            /// Is Numeric
            /// <para>Is Numeric value string, int min, int max</para>
            /// </summary>
            /// <param name="str"></param>
            /// <param name="min"></param>
            /// <param name="max"></param>
            /// <returns>True or False between two values</returns>
            public static bool IsNumeric(string str, int min, int max)
            {
                if (str == null)
                    return false;

                if (min != 0 && str.Length < min)
                    return false;

                if (max != 0 && str.Length > max)
                    return false;


                char[] ca = str.ToCharArray();
                for (int i = 0; i < ca.Length; i++)
                {
                    if (!char.IsNumber(ca[i]))
                        return false;
                }

                return true;
            }

            /// <summary>
            ///  Is Numeric
            /// <para>The TryParse method converts a string in a specified style and culture-specific format to its double-precision floating point number equivalent. The TryParse method does not generate an exception if the conversion fails. If the conversion passes, True is returned. If it does not, False is returned.</para>
            /// </summary>
            /// <param name="expression"></param>
            /// <returns>Returns True or False</returns>
            private static bool IsNumeric(object expression)
            {
                // Variable to collect the Return value of the TryParse method.
                bool isNumber;
                // Define variable to collect out parameter of the TryParse method. If the conversion fails, the out parameter is zero.
                double retNum;

                isNumber = Double.TryParse(Convert.ToString(expression), System.Globalization.NumberStyles.Any, System.Globalization.NumberFormatInfo.InvariantInfo, out retNum);

                return isNumber;
            }

            /// <summary>
            /// Validate given integer value
            /// </summary>
            /// <param name="Data">pass int value to validate</param>
            /// <param name="DefaultVal">default int value</param>
            /// <param name="MinVal">Minimum int value allowed</param>
            /// <param name="MaxVal">Maximum int value allowed</param>
            /// <returns>Validated int value</returns>
            public static int ValidateInt(object Data, int DefaultVal, int MinVal, int MaxVal)
            {
                int val = DefaultVal;

                try
                {
                    if (Data != null)
                    {
                        val = int.Parse(Data.ToString());

                        if (val < MinVal)
                            val = MinVal;
                        else if (val > MaxVal)
                            val = MaxVal;
                    }
                }
                catch (Exception _Exception)
                {
                    // Error occured while trying to validate

                    // set default value if we ran into a error
                    val = DefaultVal;

                    // You can debug for the error here
                    Console.WriteLine("Error : " + _Exception.Message);
                }

                return val;
            }

            /// <summary>
            /// GetNumeric Function
            /// <para>Takes string and try to find int</para>
            /// </summary>
            /// <param name="expression"></param>
            /// <returns>This method will return the digits between the strings, if no digits found it returns 0</returns>
            public static string GetNumeric(string expression)
            {
                bool found = false;
                string returnValue = "";
                foreach (char c in expression)
                {
                    if (Char.IsNumber(c))
                    {
                        returnValue += c;
                        found = true;
                    }
                }
                if (found == false)
                {
                    returnValue = "0";
                }
                return returnValue;
            }

            /// <summary>
            ///  Align Number
            /// </summary>
            /// <param name="num"></param>
            /// <param name="length"></param>
            /// <returns>Returns number with specific length</returns>
            public static string AlignNumber(int num, int length)
            {
                return AlignNumber(num + "", length);
            }

            /// <summary>
            ///  Align Number
            /// </summary>
            /// <param name="str"></param>
            /// <param name="length"></param>
            /// <returns>Returns number with specific length</returns>
            public static string AlignNumber(string str, int length)
            {
                string buf = "";
                for (int i = 0; i < length - str.Length; i++)
                {
                    buf += "0";
                }
                buf += str;
                return buf;
            }

            /// <summary>
            /// Clean Up Phone Number
            /// <para>Cleans up characteres that people usually type on their phone numbers... the idea is to keep numbers only</para>
            /// </summary>
            /// <param name="myPhoneString"></param>
            /// <returns>Phone number without spaces, dashes...</returns>
            public static string CleanUpPhoneNumber(string myPhoneString)
            {
                myPhoneString = myPhoneString.Trim().Replace(" ", "").Replace(".", "").Replace("-", "").Replace("(", "").Replace(")", "");
                return myPhoneString;
            }

            #endregion

            #region String Manipulation

            /// <summary>
            /// Extract Emails
            /// <para>Data mining for Emails done by set of successful matches found by iteratively applying a regular expression pattern to the input string</para>
            /// </summary>
            /// <param name="str"></param>
            /// <returns></returns>
            public static List<string> ExtractEmails(string str)
            {
                string RegexPattern = @"\b[A-Z0-9._-]+@[A-Z0-9][A-Z0-9.-]{0,61}[A-Z0-9]\.[A-Z.]{2,6}\b";

                // Find matches
                MatchCollection matches = Regex.Matches(str, RegexPattern, RegexOptions.IgnoreCase);

                List<string> MatchList = new List<string>();

                // add each match
                foreach (Match match in matches)
                {
                    MatchList.Add(match.ToString());
                }

                return MatchList;
            }

            /// <summary>
            /// Extract URLs
            /// <para> Data mining for URLs done by set of successful matches found by iteratively applying a regular expression pattern to the input string.</para>
            /// </summary>
            /// <param name="str"></param>
            /// <returns></returns>
            public static List<string> ExtractURLs(string str)
            {
                // match.Groups["name"].Value - URL Name
                // match.Groups["url"].Value - URI
                string RegexPattern = @"<a.*?href=[""'](?<url>.*?)[""'].*?>(?<name>.*?)</a>";

                // Find matches.
                MatchCollection matches = Regex.Matches(str, RegexPattern, RegexOptions.IgnoreCase);

                List<string> MatchList = new List<string>();

                // Report on each match.
                foreach (Match match in matches)
                {
                    MatchList.Add(match.Groups["url"].Value);
                }

                return MatchList;
            }

            /// <summary>
            /// method to convert a string to proper case 
            /// </summary>
            /// <param name="str">value we want converted</param>
            /// <returns></returns>
            public static string FormatProperCase(string str)
            {
                StringBuilder sb = new StringBuilder(str.Length);
                // if not value is provided throw an exception
                if (string.IsNullOrEmpty(str))
                    throw new ArgumentException("A null value cannot be converted", str);

                //if the stirng is less than 2 characters return it upper case
                if (str.Length < 2)
                    return str.ToUpper();

                //let's start with the first character (make upper case)
                sb.Append(str.Substring(0, 1).Trim().ToUpper());

                //now loop through the rest of the string converting where needed
                for (int i = 1; i < str.Length; i++)
                {
                    if (Char.IsUpper(str[i]))
                        sb.Append(" ");
                    else
                        sb.Append(str[i]);
                }

                //return the formatted string
                return sb.ToString();
            }

            /// <summary>
            ///  Align String
            /// </summary>
            /// <param name="str"></param>
            /// <param name="length"></param>
            /// <returns>Returns string with specific length</returns>
            public static string AlignString(string str, int length)
            {
                string buf = "";
                if (str.Length >= length)
                {
                    buf = str.Substring(length);
                    return buf;
                }
                buf = str;
                for (int i = 0; i < length - str.Length; i++)
                {
                    buf += " ";
                }
                return buf;
            }

            /// <summary>
            /// Clean Up SQL String
            /// <para>Cleans up SQL command in order to avoid "SQL Injection" attacks.</para>
            /// </summary>
            /// <param name="mySQLString"></param>
            /// <returns>Returns clean SQL string</returns>
            public static string CleanUpSQLString(string mySQLString)
            {
                mySQLString = mySQLString.Replace("%", "").Replace("'", "").Replace("--", "").Replace(";", "").Replace(">", "").Replace("<", "").Trim();
                return mySQLString;
            }

            /// <summary>
            /// Trim White Space
            /// </summary>
            /// <param name="str"></param>
            /// <returns></returns>
            public static string TrimWhiteSpace(string str)
            {
                return Regex.Replace(str, @"^\s+", string.Empty);
            }

            /// <summary>
            /// Strip HTML
            /// </summary>
            /// <param name="str"></param>
            /// <returns></returns>
            public static string StripHTML(string str)
            {
                return Regex.Replace(str, @"<(.|\n)*?>", string.Empty);
            }

            /// <summary>
            /// Count Words is counting the number of words
            /// </summary>
            /// <param name="word"></param>
            /// <returns> return the number of words</returns>
            public static int CountWords(string word)
            {
                char[] split = word.ToCharArray(); // split the word into character array
                int count = 0; // the count of words to return
                foreach (char space in split) // for each character in the array split...
                {
                    if (space.Equals(' ')) // if the character 'space' is equal to..a space...
                        count++; // add one to count
                }
                count += 1; // add one to count so it will count the last words
                return count; // return the number of words
            }

            /// <summary>
            /// Checks for any space in string
            /// </summary>
            /// <param name="s"></param>
            /// <returns></returns>
            public static bool HaveBlankChar(string s)
            {
                char[] cs = s.ToCharArray();
                foreach (char c in cs)
                {
                    if (c == ' ')
                    {
                        return true;
                    }
                }
                return false;

            }

            /// <summary>
            /// Sorts strings by their length
            /// </summary>
            /// <param name="stringList"></param>
            /// <returns></returns>
            private static IList<string> SortStringLength(IList<string> stringList)
            {
                string[] strs = stringList.ToArray<string>();
                Array.Sort(strs, new Comparison<string>(delegate(string str1, string str2)
                {
                    if (str1 == null && str2 == null)
                        return 0; //both empty
                    else if (str1 == null)
                        return -1; //empty string before non-empty string
                    else if (str2 == null)
                        return 1; //non-empty string after empty string
                    else
                    {
                        if (str1.Length < str2.Length)
                            return -1; //shorter string before longer string
                        else if (str1.Length > str2.Length)
                            return 1; //longer string after shorter string
                        else
                            return str1.CompareTo(str2); //alphabetical order
                    }
                }));

                return strs;
            }

            /// <summary>
            /// Break string in specific width
            /// </summary>
            /// <param name="input"></param>
            /// <param name="width"></param>
            /// <returns></returns>
            public static List<string> BreakStringScanWidth(string input, int width)
            {
                List<string> ret = new List<string>();
                for (int c = 0; c <= (input.Length - width); c++)
                {
                    ret.Add(input.Substring(c, width));
                }
                return ret;
            }

            /// <summary>
            /// Convert Digits To Words
            /// </summary>
            /// <param name="input"></param>
            /// <returns></returns>
            public static string ConvertDigitsToWords(string input)
            {
                string ret = input;
                return ret.Replace("0", "zero").Replace("1", "one").Replace("2", "two").Replace("3", "three").Replace("4", "four").Replace("5", "five").Replace("6", "six").Replace("7", "seven").Replace("8", "eight").Replace("9", "nine");
            }

            /// <summary>
            /// Single Exception Info
            /// </summary>
            /// <param name="exc"></param>
            /// <returns></returns>
            public static string ExceptionToText(Exception exc)
            {
                string str = "";
                str = str + "<br /> \n\n DateTimeNow: " + DateTime.Now.ToString("MM/dd/yyyy h:mm:ss tt");
                str = str + "<br /> \n\n UserIPAddress: " + Network.UserIPAddress();
                str = str + "<br />  \n\n WebServerName: " + Network.GetDnsServerName();
                str = str + "<br />  \n\n Url: " + HttpContext.Current.Request.Url.ToString();
                str = str + "<br />  \n\n Exception InnerException ToString: " + ((exc.InnerException == null) ? "" : exc.InnerException.ToString());
                str = str + "<br />  \n\n Exception InnerException Message: " + ((exc.InnerException == null) ? "" : exc.InnerException.Message);
                str = str + "<br />  \n\n Exception InnerException Source: " + ((exc.InnerException == null) ? "" : exc.InnerException.Source);
                str = str + "<br />  \n\n Exception ToString: " + ((exc == null) ? "" : exc.ToString());
                str = str + "<br />  \n\n Exception Source: " + ((exc == null) ? "" : exc.Source);
                str = str + "<br />  \n\n Exception Message: " + ((exc == null) ? "" : exc.Message);
                str = str + "<br />  \n\n TargetSite: " + ((exc == null) ? "" : exc.TargetSite.ToString());
                return str + "<br />  \n\n Exception StackTrace: " + ((exc == null) ? "" : exc.StackTrace);
            }

            /// <summary>
            /// Single Exception Info
            /// </summary>
            /// <param name="ex"></param>
            /// <returns></returns>
            private static string SingleExceptionInfo(Exception ex)
            {
                object a = (((("" + "--------- Exception Data ---------" + Environment.NewLine) + "Message: " + ex.Message + Environment.NewLine) + "Exception Type: " + ex.GetType().FullName + Environment.NewLine) + "Source: " + ex.Source + Environment.NewLine) + "StackTrace: " + ex.StackTrace + Environment.NewLine;
                return string.Concat(new object[] { a, "TargetSite: ", ex.TargetSite, Environment.NewLine });
            }

            /// <summary>
            /// Elaborate Exception
            /// </summary>
            /// <param name="ex"></param>
            /// <returns></returns>
            public static string ElaborateException(Exception ex)
            {
                string myReturn = "";
                if (ex != null)
                {
                    myReturn = myReturn + SingleExceptionInfo(ex);
                    if (ex.InnerException != null)
                    {
                        myReturn = myReturn + SingleExceptionInfo(ex.InnerException);
                        if (ex.InnerException.InnerException != null)
                        {
                            myReturn = myReturn + SingleExceptionInfo(ex.InnerException.InnerException);
                            if (ex.InnerException.InnerException.InnerException != null)
                            {
                                myReturn = myReturn + SingleExceptionInfo(ex.InnerException.InnerException.InnerException);
                            }
                        }
                    }
                }
                return myReturn;
            }

            /// <summary>
            /// Short Exception
            /// </summary>
            /// <param name="ex"></param>
            /// <returns></returns>
            public static string ShortException(Exception ex)
            {
                string msg = ex.ToString();
                if (msg.StartsWith("System."))
                {
                    msg = msg.Substring("System.".Length);
                }
                string firstLine = null;
                StringReader sr = new StringReader(msg);
                for (string line = sr.ReadLine(); line != null; line = sr.ReadLine())
                {
                    if (firstLine == null)
                    {
                        firstLine = line;
                        if (firstLine.EndsWith("."))
                        {
                            firstLine = firstLine.Substring(0, firstLine.Length - 1);
                        }
                    }
                    if (line.IndexOf(") in ") > -1)
                    {
                        string slm = line.Split(new char[] { '(' })[0] + line.Split(new char[] { ')' })[1];
                        return (firstLine + " " + slm.Trim().Replace(":line ", ":"));
                    }
                }
                sr.Close();
                sr.Dispose();
                return firstLine;
            }

            /// <summary>
            /// TitleCase
            /// </summary>
            /// <param name="input"></param>
            /// <returns></returns>
            public static string TitleCase(string input)
            {
                if (string.IsNullOrEmpty(input))
                {
                    return input;
                }
                string ret;
                string[] bits = input.Split(new char[] { ' ' });
                if (bits.Length == 1)
                {
                    ret = input.Substring(0, 1).ToUpper();
                    if (input.Length > 1)
                    {
                        ret = ret + input.Substring(1).ToLower();
                    }
                    return ret;
                }
                ret = "";
                foreach (string bit in bits)
                {
                    if (ret.Length > 0)
                    {
                        ret = ret + " ";
                    }
                    ret = ret + TitleCase(bit);
                }
                return ret;
            }

            /// <summary>
            /// To String - object Can Be Null
            /// </summary>
            /// <param name="objectCanBeNull"></param>
            /// <returns></returns>
            public static string ToString(object objectCanBeNull)
            {
                if (objectCanBeNull == null)
                {
                    return "";
                }
                if (objectCanBeNull is byte[])
                {
                    return new UTF7Encoding().GetString((byte[])objectCanBeNull);
                }
                return objectCanBeNull.ToString();
            }

            #endregion

            #endregion
        }

        /// <summary>
        /// Class Credit Card And Money for, format, mask, intrest
        /// </summary>
        public static class CreditCardMoney
        {
            #region CreditCard & Money

            /// <summary>
            ///  FormatMoney
            /// <para>Display two decimal digits (money format)</para>
            /// </summary>
            /// <param name="str"></param>
            /// <returns>Returns two decimal digits (money format)</returns>
            public static string FormatMoney(string str)
            {
                string res = str;
                int pos = str.IndexOf(".");
                if (pos == -1)
                {
                    res += ".00";
                }
                else if (pos == str.Length - 2)
                {
                    res += "0";
                }
                else if (pos < str.Length - 3)
                {
                    res = res.Substring(0, pos + 3);
                }
                return res;
            }

            /// <summary>
            /// Mask Credit Card Number
            /// <para>Masks creditcard number</para>
            /// </summary>
            /// <param name="ccnumber"></param>
            /// <returns>Credicard number as "********" + last 4 numbers";</returns>
            public static string MaskCreditCardNumber(string ccnumber)
            {
                string maskedCCNumber;
                string first4, last4;
                switch (ccnumber.Length)
                {
                    case 16: // VISA and MasterCard
                        maskedCCNumber = ccnumber;
                        first4 = ccnumber.Substring(0, 4);
                        last4 = ccnumber.Substring(ccnumber.Length - 4, 4);
                        maskedCCNumber = first4 + "********" + last4;
                        return maskedCCNumber;
                    case 15: // AMEX 
                        maskedCCNumber = ccnumber;
                        first4 = ccnumber.Substring(0, 4);
                        last4 = ccnumber.Substring(ccnumber.Length - 4, 4);
                        maskedCCNumber = first4 + "*******" + last4;
                        return maskedCCNumber;
                    default: return null;
                }
            }

            /// <summary>
            /// Fix CC Expiry Date
            /// <para>Fix CC Expiry Date</para>
            /// </summary>
            /// <param name="expiryDate"></param>
            /// <returns>CC Expiry Date in 11/11 format</returns>
            public static string FixCCExpiryDate(string expiryDate)
            {
                string yearHalf, monthHalf;
                if (expiryDate.Length == 4)
                {
                    yearHalf = expiryDate.Substring(0, 2);
                    monthHalf = expiryDate.Substring(2, 2);
                    return monthHalf + "/" + yearHalf;
                }
                else
                    return null;
            }

            /// <summary>
            /// Calculate Basic Intrest
            /// </summary>
            /// <param name="principal"></param>
            /// <param name="interest"></param>
            /// <returns>returns decimal</returns>
            public static decimal CalculateBasicIntrest(decimal principal, decimal interest)
            {
                if (principal < 0)
                { // principal cannot be negative
                    principal = 0;
                }

                if (interest < 0)
                { // interest cannot be negative
                    interest = 0;
                }

                decimal interestPaid = principal * (interest / 100);
                decimal totalPI = principal + interestPaid;

                return totalPI;
            }

            /// <summary>
            /// Calculate Basic Intrest trough years
            /// </summary>
            /// <param name="principal"></param>
            /// <param name="interest"></param>
            /// <param name="noYears"></param>
            /// <returns>returns List decimal</returns>
            public static List<decimal> CalculateBasicIntrest(decimal principal, decimal interest, int noYears)
            {
                if (principal < 0)
                {  // principal cannot be negative
                    principal = 0;
                }

                if (interest < 0)
                {  // interest cannot be negative
                    interest = 0;
                }

                List<decimal> totalPI = new List<decimal>();
                // loop through number of years specified 
                int year = 1;
                while (year <= noYears)
                {
                    // calculate interest
                    decimal interestPaid = principal * (interest / 100);

                    // calculate new principal
                    principal += interestPaid;

                    // round to the nearest penny
                    principal = decimal.Round(principal, 2);

                    totalPI.Add(principal);
                    year++;
                }
                return totalPI;
            }

            #endregion
        }

        /// <summary>
        /// Class Image Manipulation for, Tumbnails, Bytearay Conversions
        /// </summary>
        public static class ImageManipulation
        {
            #region Image Manipulation

            /// <summary>
            /// Image To ByteArray
            /// </summary>
            /// <param name="imageIn"></param>
            /// <param name="format"></param>
            /// <returns></returns>
            public static byte[] ImageToByteArray(Image imageIn, ImageFormat format)
            {
                MemoryStream ms = new MemoryStream();
                imageIn.Save(ms, format);
                return ms.ToArray();
            }

            /// <summary>
            /// Byte Array To Image
            /// </summary>
            /// <param name="byteArrayIn"></param>
            /// <returns></returns>
            public static Image ByteArrayToImage(byte[] byteArrayIn)
            {
                MemoryStream ms = new MemoryStream(byteArrayIn);
                Image returnImage = Image.FromStream(ms);
                return returnImage;
            }

            /// <summary>
            /// Create Thumbnail
            /// <para>Creates a thumbnail image from a file spec in the calling URL with Width and Height</para>
            /// </summary>
            /// <param name="ByteArayImage"></param>
            /// <param name="thumbWidth"></param>
            /// <param name="thumbHeight"></param>
            /// <returns></returns>
            public static byte[] CreateThumbnail(byte[] ByteArayImage, int thumbWidth, int thumbHeight)
            {
                MemoryStream ms = new MemoryStream(ByteArayImage);
                Image image = Image.FromStream(ms);
                Image thumbnailImage = image.GetThumbnailImage(thumbWidth, thumbHeight, null, IntPtr.Zero);
                MemoryStream imageStream = new MemoryStream();
                thumbnailImage.Save(imageStream, ImageFormat.Jpeg);
                byte[] imageContent = new Byte[imageStream.Length];
                imageStream.Position = 0;
                imageStream.Read(imageContent, 0, (int)imageStream.Length);

                return imageContent;
            }

            /// <summary>
            /// Create Thumbnail
            /// <para>Creates a thumbnail image from a file spec in the calling URL with Height and proportional Width</para>
            /// </summary>
            /// <param name="ByteArayImage"></param>
            /// <param name="thumbHeight"></param>
            /// <returns></returns>
            public static byte[] CreateThumbnail(byte[] ByteArayImage, int thumbHeight)
            {
                MemoryStream ms = new MemoryStream(ByteArayImage);
                Image image = Image.FromStream(ms);
                decimal Width = (decimal)image.Width / image.Height;
                int thumbWidth = Convert.ToInt32(Width * thumbHeight);

                Image thumbnailImage = image.GetThumbnailImage(thumbWidth, thumbHeight, null, IntPtr.Zero);
                MemoryStream imageStream = new MemoryStream();
                thumbnailImage.Save(imageStream, ImageFormat.Jpeg);
                byte[] imageContent = new Byte[imageStream.Length];
                imageStream.Position = 0;
                imageStream.Read(imageContent, 0, (int)imageStream.Length);

                return imageContent;
            }

            /// <summary>
            /// Merges 4 Images into 1 Image.
            /// </summary>
            /// <param name="image1">The Image you want in the Top-Left Corner.</param>
            /// <param name="image2">The Image you want in the Top-Right Corner.</param>
            /// <param name="image3">The Image you want in the Bottom-Left Corner.</param>
            /// <param name="image4">The Image you want in the Bottom-Right Corner.</param>
            /// <returns>An Image of 4</returns>
            public static Image MergeImages(Image image1, Image image2, Image image3, Image image4)
            {
                //Get the Width of All the Images
                Int32 width = image1.Width + image2.Width + image3.Width + image4.Width;
                //Get the Height of All the Images
                Int32 height = image1.Height + image2.Height + image3.Height + image4.Height;
                //Create a new Bitmap with the Width and Height
                Bitmap bitmap = new Bitmap(width, height);

                //Get All of the x Pixels
                for (int w = 0; w < image1.Width; w++)
                {
                    //Get All of the Y Pixels
                    for (int h = 0; h < image1.Height; h++)
                    {
                        //Create a new Bitmap from image1
                        Bitmap image = new Bitmap(image1);
                        //Set the Cooresponding Pixel
                        bitmap.SetPixel(w, h, image.GetPixel(w, h));
                    }
                }
                //Get All of the x Pixels
                for (int w = 0; w < image2.Width; w++)
                {
                    //Get All of the Y Pixels
                    for (int h = 0; h < image2.Height; h++)
                    {
                        //Create a new Bitmap from image2
                        Bitmap image = new Bitmap(image2);
                        //Set the Cooresponding Pixel
                        bitmap.SetPixel(image.Width + w, h, image.GetPixel(w, h));
                    }
                }
                //Get All of the x Pixels
                for (int w = 0; w < image3.Width; w++)
                {
                    //Get All of the Y Pixels
                    for (int h = 0; h < image3.Height; h++)
                    {
                        //Create a new Bitmap from image3
                        Bitmap image = new Bitmap(image3);
                        //Set the Cooresponding Pixel
                        bitmap.SetPixel(w, image.Height + h, image.GetPixel(w, h));
                    }
                }
                //Get All of the x Pixels
                for (int w = 0; w < image4.Width; w++)
                {
                    //Get All of the Y Pixels
                    for (int h = 0; h < image4.Height; h++)
                    {
                        //Create a new Bitmap from image4
                        Bitmap image = new Bitmap(image4);
                        //Set the Cooresponding Pixel
                        bitmap.SetPixel(image.Width + w, image.Height + h, image.GetPixel(w, h));
                    }
                }

                //Return the new Bitmap
                return bitmap;
            }

            #endregion
        }

        #region Other Useful

        /// <summary>
        /// Encode To 64
        /// </summary>
        /// <param name="toEncode"></param>
        /// <returns>clean string</returns>
        public static string EncodeTo64(string toEncode)
        {
            byte[] toEncodeAsBytes
                  = System.Text.ASCIIEncoding.ASCII.GetBytes(toEncode);
            string returnValue
                  = System.Convert.ToBase64String(toEncodeAsBytes);
            return returnValue;
        }

        /// <summary>
        /// Decode From 64
        /// </summary>
        /// <param name="encodedData"></param>
        /// <returns></returns>
        public static string DecodeFrom64(string encodedData)
        {
            byte[] encodedDataAsBytes
                = System.Convert.FromBase64String(encodedData);
            string returnValue =
               System.Text.ASCIIEncoding.ASCII.GetString(encodedDataAsBytes);
            return returnValue;
        }

        /// <summary>
        /// Hashtable Parse from string
        /// </summary>
        /// <param name="md"></param>
        /// <returns></returns>
        public static Hashtable Parse_md(string md)
        {
            Hashtable keyValueHash = new Hashtable();
            string[] keyAndValuesArr = md.Split("&".ToCharArray());
            foreach (string keyAndValueStr in keyAndValuesArr)
            {
                string[] tmp = keyAndValueStr.Split("=".ToCharArray());
                keyValueHash.Add(tmp[0], tmp[1]);
            }
            return keyValueHash;
        }

        /// <summary>
        /// Convert Array To NVC(Name Value Collection)
        /// </summary>
        /// <param name="array"></param>
        /// <returns>returns NameValueCollection</returns>
        public static NameValueCollection ConvertArrayToNVC(string[,] array)
        {
            NameValueCollection coll = new NameValueCollection();

            for (int i = 0; i < array.GetLength(0); i++)
            {
                // Add the key to the NVC with a null value
                coll.Add(array[i, 0], null);

                for (int j = 0; j < array.GetLength(1); j++)
                {
                    // Update the key with appropriate value
                    coll.Set(array[i, 0], array[i, j]);
                }
            }
            return coll;
        }

        /// <summary>
        /// Get Last N Days
        /// </summary>
        /// <param name="numberOfDays"></param>
        /// <returns>DateTime[]</returns>
        public static List<DateTime> GetLastNDays(int numberOfDays)
        {
            DateTime today = DateTime.Today;

            List<DateTime> days = new List<DateTime>();
            days.Add(today);

            for (int i = 1; i < numberOfDays; i++)
            {
                days.Add(today.AddDays(i * -1));
            }

            return days;
        }

        /// <summary>
        /// ReWrite Phone Number in Form (123) 456 - 7890
        /// </summary>
        /// <param name="PhoneNumber">Phone Number</param>
        /// <returns>Phone Number in Form (123) 456 - 7890</returns>
        public static string ReWritePhoneNumber(String PhoneNumber)
        {
            Regex expressions = new Regex(@"^\(?([1-9]\d{2})\)?\D*?([1-9]\d{2})\D*?(\d{4})$");
            Match regex = Regex.Match(PhoneNumber, expressions.ToString());
            return Convert.ToString("(" + regex.Groups[1]) + ") " + Convert.ToString(regex.Groups[2])+" - "+Convert.ToString(regex.Groups[3]);
        }

        #endregion
    }
}
