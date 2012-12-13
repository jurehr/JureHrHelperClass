using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Net;
using System.Net.Sockets;

namespace JureHR
{
    /// <summary>
    /// Class Validation
    /// </summary>
    public static class Validation
    {
        /// <summary>
        /// Validation For Standard variables
        /// </summary>
        public static class Standard
        {
            /// <summary>
            /// Enum Validation Type
            /// </summary>
            public enum Type
            {
                /// <summary>
                /// Email Validation
                /// </summary>
                EMAIL,
                /// <summary>
                /// Tries to conect to email server 
                /// </summary>
                EMAILDOMAIN,
                /// <summary>
                /// Postal Code Validation
                /// </summary>
                POSTALCODE,
                /// <summary>
                /// Phone Validation
                /// </summary>
                PHONENUMBER,
                /// <summary>
                /// Number Validation
                /// </summary>
                NUMBER,
                /// <summary>
                /// Positive Number Validation
                /// </summary>
                POSITIVENUMBER,
                /// <summary>
                /// Alpha Validation
                /// </summary>
                ALPHA,
                /// <summary>
                /// Alpha numeric Validation
                /// </summary>
                ALPHANUMERIC,
                /// <summary>
                /// Guid Validation
                /// </summary>
                GUID
            };

            #region Validation

            /// <summary>
            /// Validation 
            /// </summary>
            /// <param name="Str"></param>
            /// <param name="ValidationType">Enum ValidationType</param>
            /// <returns>returns bool</returns>
            public static bool IsValid(string Str, Type ValidationType)
            {
                switch (ValidationType)
                {
                    case Type.EMAIL:
                        return IsValidEmailAddress(Str);
                    case Type.EMAILDOMAIN:
                        return IsValidEmailDomain(Str);
                    case Type.POSTALCODE:
                        return IsPostalCode(Str);
                    case Type.PHONENUMBER:
                        return IsValidPhone(Str);
                    case Type.NUMBER:
                        return IsInteger(Str);
                    case Type.POSITIVENUMBER:
                        return IsPositiveWholeNumber(Str);
                    case Type.ALPHA:
                        return IsAlpha(Str);
                    case Type.ALPHANUMERIC:
                        return IsAlphaNumeric(Str);
                    case Type.GUID:
                        return IsGUID(Str);
                }
                return false;
            }

            /// <summary>
            /// <para>Validating Email Address</para>
            /// </summary>
            /// <param name="Email"></param>
            /// <returns>Returns True or False</returns>
            private static bool IsValidEmailAddress(string Email)
            {
                string strRegex = @"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}" +
                    @"\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\" +
                    @".)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$";
                Regex Regex = new Regex(strRegex);
                if (Regex.IsMatch(Email))
                    return (true);
                else
                    return (false);
            }

            /// <summary>
            /// Validates an email's domain
            /// </summary>
            /// <param name="EmailAddress">The email address to validate</param>
            /// <returns>True on valid (can connect to end mail server), False if not valid</returns>
            private static bool IsValidEmailDomain(string EmailAddress)
            {
                bool ReturnVal = false;
                string[] Host = EmailAddress.Split('@');
                string HostName = Host[1];
                IPHostEntry IPHost = Dns.GetHostEntry(HostName);
                IPEndPoint EndPoint = new IPEndPoint(IPHost.AddressList[0], 25);
                Socket s = new Socket(EndPoint.AddressFamily,
                        SocketType.Stream, ProtocolType.Tcp);
                try
                {
                    s.Connect(EndPoint);
                    s.Disconnect(false);
                    ReturnVal = true;
                }
                catch (Exception)
                {
                    ReturnVal = false;
                }
                finally
                {
                    if (s.Connected)
                        s.Disconnect(false);
                }
                return ReturnVal;
            }

            /// <summary>
            /// Validating Postal Code
            /// </summary>
            /// <param name="postalCode"></param>
            /// <returns></returns>
            private static bool IsPostalCode(string postalCode)
            {
                if (postalCode.Length == 7)
                {
                    //Canadian Postal Code in the format of "M3A 1A5"
                    string strRegex = "^[ABCEGHJ-NPRSTVXY]{1}[0-9]{1}[ABCEGHJ-NPRSTV-Z]{1}[ ]?[0-9]{1}[ABCEGHJ-NPRSTV-Z]{1}[0-9]{1}$";

                    Regex Regex = new Regex(strRegex, RegexOptions.IgnoreCase | RegexOptions.Compiled);

                    if (!(Regex.IsMatch(postalCode)))
                        return false;
                    return true;
                }
                if (postalCode.Length == 6)
                {
                    //Canadian Postal Code in the format of "M3A1A5"
                    string strRegex = "^[ABCEGHJ-NPRSTVXY]{1}[0-9]{1}[ABCEGHJ-NPRSTV-Z]{1}?[0-9]{1}[ABCEGHJ-NPRSTV-Z]{1}[0-9]{1}$";

                    Regex Regex = new Regex(strRegex, RegexOptions.IgnoreCase | RegexOptions.Compiled);

                    if (!(Regex.IsMatch(postalCode)))
                        return false;
                    return true;
                }
                return false;
            }

            /// <summary>
            /// Validating Phone
            /// </summary>
            /// <param name="strPhoneInput"></param>
            /// <returns></returns>
            private static bool IsValidPhone(string strPhoneInput)
            {
                // Remove symbols (dash, space and parentheses, etc.)
                string strPhone = Regex.Replace(strPhoneInput, @"[- ()\*\!]", string.Empty);

                // Check for exactly 10 numbers left over
                Regex regTenDigits = new Regex(@"^\d{10}$");
                Match matTenDigits = regTenDigits.Match(strPhone);

                return matTenDigits.Success;
            }

            /// <summary>
            ///  This function uses regular expression to match string pattern to validate whole number.
            /// </summary>
            /// <param name="strNumber"></param>
            /// <returns></returns>
            private static bool IsPositiveWholeNumber(string strNumber)
            {
                Regex RegexNotWholePattern = new Regex("[^0-9]");
                return !RegexNotWholePattern.IsMatch(strNumber);
            }

            /// <summary>
            /// Integer function validate integer both positive and negative. Function return TRUE if valid integer found, if not function will return FALSE.
            /// </summary>
            /// <param name="strNumber"></param>
            /// <returns></returns>
            private static bool IsInteger(string strNumber)
            {
                Regex RegexNotIntPattern = new Regex("[^0-9-]");
                Regex RegexIntPattern = new Regex("^-[0-9]+$|^[0-9]+$");
                return !RegexNotIntPattern.IsMatch(strNumber) && RegexIntPattern.IsMatch(strNumber);
            }

            /// <summary>
            /// Function To test for Alphabets.
            /// </summary>
            /// <param name="strToCheck"></param>
            /// <returns></returns>
            private static bool IsAlpha(string strToCheck)
            {
                Regex objAlphaPattern = new Regex("[^a-zA-Z]");
                return !objAlphaPattern.IsMatch(strToCheck);
            }

            /// <summary>
            /// Function to Check for AlphaNumeric.
            /// </summary>
            /// <param name="strToCheck"></param>
            /// <returns></returns>
            private static bool IsAlphaNumeric(string strToCheck)
            {
                Regex objAlphaNumericPattern = new Regex("[^a-zA-Z0-9]");
                return !objAlphaNumericPattern.IsMatch(strToCheck);
            }

            /// <summary>
            /// Function to Check for GUID.
            /// </summary>
            /// <param name="expression"></param>
            /// <returns></returns>
            private static bool IsGUID(string expression)
            {
                if (expression != null)
                {
                    Regex guidRegEx = new Regex(@"^(\{{0,1}([0-9a-fA-F]){8}-([0-9a-fA-F]){4}-([0-9a-fA-F]){4}-([0-9a-fA-F]){4}-([0-9a-fA-F]){12}\}{0,1})$");
                    return guidRegEx.IsMatch(expression);
                }
                return false;
            }

            #endregion
        }

        /// <summary>
        /// Clredit Card Validation By Number
        /// </summary>
        public class CreditCard
        {
            /// <summary>
            /// Enum Type of credit Card
            /// </summary>
            public enum CardType
            {
                /// <summary>
                /// MasterCard Validation By Number
                /// </summary>
                MasterCard,
                /// <summary>
                /// BankCard Validation By Number
                /// </summary>
                BankCard,
                /// <summary>
                /// Visa Validation By Number
                /// </summary>
                Visa,
                /// <summary>
                /// AmericanExpress Validation By Number
                /// </summary>
                AmericanExpress,
                /// <summary>
                /// Discover Validation By Number
                /// </summary>
                Discover,
                /// <summary>
                /// DinersClub Validation By Number
                /// </summary>
                DinersClub,
                /// <summary>
                /// JCB Validation By Number
                /// </summary>
                JCB
            };

            #region Credit Card Validation

            /// <summary>
            /// Validation of Credit Card by lenght, last digits and Luhn algorithm
            /// </summary>
            /// <param name="cardNumber"></param>
            /// <param name="cardType"></param>
            /// <returns></returns>
            public static bool Validate(string cardNumber, CardType cardType)
            {
                byte[] number = new byte[16]; // number to validate

                // Remove non-digits
                int len = 0;
                for (int i = 0; i < cardNumber.Length; i++)
                {
                    if (char.IsDigit(cardNumber, i))
                    {
                        if (len == 16) return false; // number has too many digits
                        number[len++] = byte.Parse(cardNumber[i].ToString());
                    }
                }

                // Validate based on card type, first if tests length, second tests prefix
                switch (cardType)
                {
                    case CardType.MasterCard:
                        if (len != 16)
                            return false;
                        if (number[0] != 5 || number[1] == 0 || number[1] > 5)
                            return false;
                        break;

                    case CardType.BankCard:
                        if (len != 16)
                            return false;
                        if (number[0] != 5 || number[1] != 6 || number[2] > 1)
                            return false;
                        break;

                    case CardType.Visa:
                        if (len != 16 && len != 13)
                            return false;
                        if (number[0] != 4)
                            return false;
                        break;

                    case CardType.AmericanExpress:
                        if (len != 15)
                            return false;
                        if (number[0] != 3 || (number[1] != 4 && number[1] != 7))
                            return false;
                        break;

                    case CardType.Discover:
                        if (len != 16)
                            return false;
                        if (number[0] != 6 || number[1] != 0 || number[2] != 1 || number[3] != 1)
                            return false;
                        break;

                    case CardType.DinersClub:
                        if (len != 14)
                            return false;
                        if (number[0] != 3 || (number[1] != 0 && number[1] != 6 && number[1] != 8)
                           || number[1] == 0 && number[2] > 5)
                            return false;
                        break;

                    case CardType.JCB:
                        if (len != 16 && len != 15)
                            return false;
                        if (number[0] != 3 || number[1] != 5)
                            return false;
                        break;
                }

                // Use Luhn Algorithm to validate
                int sum = 0;
                for (int i = len - 1; i >= 0; i--)
                {
                    if (i % 2 == len % 2)
                    {
                        int n = number[i] * 2;
                        sum += (n / 10) + (n % 10);
                    }
                    else
                        sum += number[i];
                }
                return (sum % 10 == 0);
            }

            #endregion
        }
    }
}
