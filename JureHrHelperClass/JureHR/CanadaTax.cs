using System;
using System.Configuration;
using System.Linq;
using System.Text.RegularExpressions;

namespace JureHR
{
    /// <summary>
    /// Canada specifific calculations for Tax and postalcode checking
    /// </summary>
    public static class CanadaTax
    {
        #region Public Methods

        /// <summary>
        /// Tax Percentages By PostalCode
        /// </summary>
        /// <param name="PostalCode">Canadian Postal Code</param>
        /// <param name="ProvinceCode">out Province Code</param>
        /// <param name="TaxPercentage_GST">out GST Percentage</param>
        /// <param name="TaxPercentage_PST">out PST Percentage</param>
        /// <param name="TaxPercentage_HST">out HST Percentage</param>
        public static void TaxPercentagesByPostalCode(string PostalCode, out string ProvinceCode, out double TaxPercentage_GST, out double TaxPercentage_PST, out double TaxPercentage_HST)
        {
            TaxPercentage_GST = 0.0;
            TaxPercentage_PST = 0.0;
            TaxPercentage_HST = 0.0;
            string text = "";
            ProvinceCode = "";
            GeoData.ProvinceByPostalCode(PostalCode, out text, out ProvinceCode);
            TaxPercentagesByProvince(ProvinceCode, out TaxPercentage_GST, out TaxPercentage_PST, out TaxPercentage_HST);
        }

        /// <summary>
        /// Tax Calculator By PostalCode
        /// </summary>
        /// <param name="PostalCode">Canadian Postal Code</param>
        /// <param name="TaxableAmount"></param>
        /// <param name="ProvinceCode">out Province Code</param>
        /// <param name="TaxedAmount_GST">out GST Amount</param>
        /// <param name="TaxedAmount_PST">out PST Amount</param>
        /// <param name="TaxedAmount_HST">out HST Amount</param>
        /// <param name="TaxedAmount_Total">out Total Tax Amount</param>
        /// <param name="TaxPercentage_Effective">out Effective</param>
        public static void TaxCalculatorByPostalCode(string PostalCode, double TaxableAmount, out string ProvinceCode, out double TaxedAmount_GST, out double TaxedAmount_PST, out double TaxedAmount_HST, out double TaxedAmount_Total, out double TaxPercentage_Effective)
        {
            string text = "";
            ProvinceCode = "";
            GeoData.ProvinceByPostalCode(PostalCode, out text, out ProvinceCode);
            TaxCalculatorByProvince(ProvinceCode, TaxableAmount, out TaxedAmount_GST, out TaxedAmount_PST, out TaxedAmount_HST, out TaxedAmount_Total, out TaxPercentage_Effective);
        }

        /// <summary>
        /// Tax Calculator By Province
        /// </summary>
        /// <param name="ProvinceCode">Province Code</param>
        /// <param name="TaxableAmount"></param>
        /// <param name="TaxedAmount_GST">out GST Amount</param>
        /// <param name="TaxedAmount_PST">out PST Amount</param>
        /// <param name="TaxedAmount_HST">out HST Amount</param>
        /// <param name="TaxedAmount_Total">out Total Tax Amount</param>
        /// <param name="TaxPercentage_Effective">out Effective</param>
        public static void TaxCalculatorByProvince(string ProvinceCode, double TaxableAmount, out double TaxedAmount_GST, out double TaxedAmount_PST, out double TaxedAmount_HST, out double TaxedAmount_Total, out double TaxPercentage_Effective)
        {
            TaxedAmount_GST = 0.0;
            TaxedAmount_PST = 0.0;
            TaxedAmount_HST = 0.0;
            TaxedAmount_Total = 0.0;
            TaxPercentage_Effective = 0.0;
            double GST = 0.0;
            double PST = 0.0;
            double HST = 0.0;
            TaxPercentagesByProvince(ProvinceCode, out GST, out PST, out HST);
            GST /= 100.0;
            PST /= 100.0;
            HST /= 100.0;
            TaxedAmount_GST = TaxableAmount * GST;
            if (ProvinceCode == "QC" || ProvinceCode == "PE")
            {
                TaxedAmount_PST = (TaxableAmount + TaxedAmount_GST) * PST;
                TaxPercentage_Effective = (PST + GST * PST) * 100.0;
            }
            else
            {
                TaxedAmount_PST = TaxableAmount * PST;
                TaxPercentage_Effective = (PST + GST) * 100.0;
            }
            TaxedAmount_HST = TaxableAmount * HST;
            if (HST > 0.0)
            {
                TaxedAmount_GST = 0.0;
                TaxedAmount_PST = 0.0;
            }
            TaxedAmount_Total = TaxedAmount_GST + TaxedAmount_PST + TaxedAmount_HST;
        }

        /// <summary>
        /// Tax Percentages By Province
        /// </summary>
        /// <param name="ProvinceCode">Province Code</param>
        /// <param name="TaxPercentage_GST">out GST Percentage</param>
        /// <param name="TaxPercentage_PST">out PST Percentage</param>
        /// <param name="TaxPercentage_HST">out HST Percentage</param>
        public static void TaxPercentagesByProvince(string ProvinceCode, out double TaxPercentage_GST, out double TaxPercentage_PST, out double TaxPercentage_HST)
        {
            TaxPercentage_GST = 0.0;
            TaxPercentage_PST = 0.0;
            TaxPercentage_HST = 0.0;
            if (ProvinceCode.Length == 2)
            {
                string text = ConfigurationManager.AppSettings["Tax" + ProvinceCode + "PrecentGstPstHst"];
                string[] array = text.Split(new char[]
			{
				'_'
			});
                double.TryParse(array[0], out TaxPercentage_GST);
                double.TryParse(array[1], out TaxPercentage_PST);
                double.TryParse(array[2], out TaxPercentage_HST);
            }
        }

        #endregion

        #region Private Method

        /// <summary>
        /// Fix String To Double
        /// </summary>
        /// <param name="Currency_String"></param>
        /// <returns></returns>
        private static double FixStringToDouble(string Currency_String)
        {
            double num = -1.0;
            if (Currency_String != null && Currency_String != "")
            {
                while (Currency_String.Contains(" "))
                {
                    Currency_String = Currency_String.Replace(" ", "");
                }
                Currency_String = Currency_String.Trim();
                double.TryParse(Currency_String, out num);
                if (num == -1.0 || num == 0.0)
                {
                    try
                    {
                        string text = "";
                        string pattern = "[^\\d\\.\\,]";
                        Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);
                        foreach (Match match in regex.Matches(Currency_String))
                        {
                            if (match != null)
                            {
                                string str = match.ToString();
                                text += str;
                            }
                        }
                        if (text.Length > 0)
                        {
                            string text2 = text;
                            for (int i = 0; i < text2.Length; i++)
                            {
                                Currency_String = Currency_String.Replace(text2[i].ToString(), "");
                            }
                        }
                    }
                    catch
                    {
                    }
                    int num2 = Currency_String.Count((char f) => f == ',');
                    int num3 = Currency_String.Count((char f) => f == '.');
                    if (num2 == 0 && num3 == 0)
                    {
                        double.TryParse(Currency_String, out num);
                    }
                    else
                    {
                        if (num2 == 0 && num3 == 1)
                        {
                            double.TryParse(Currency_String, out num);
                            if (num == -1.0 || num == 0.0)
                            {
                                double.TryParse(Currency_String.Replace(".", ","), out num);
                            }
                        }
                        else
                        {
                            if (num2 == 0 && num3 > 1)
                            {
                                double.TryParse(Currency_String.Replace(".", ""), out num);
                            }
                            else
                            {
                                if (num2 == 1 && num3 == 0)
                                {
                                    double.TryParse(Currency_String, out num);
                                    if (num == -1.0 || num == 0.0)
                                    {
                                        double.TryParse(Currency_String.Replace(",", "."), out num);
                                    }
                                }
                                else
                                {
                                    if (num2 > 1 && num3 == 0)
                                    {
                                        double.TryParse(Currency_String.Replace(",", ""), out num);
                                    }
                                    else
                                    {
                                        int num4 = Currency_String.LastIndexOf(",");
                                        int num5 = Currency_String.LastIndexOf(".");
                                        if (num4 > num5)
                                        {
                                            while (Currency_String.Contains("."))
                                            {
                                                Currency_String = Currency_String.Replace(".", "");
                                            }
                                        }
                                        else
                                        {
                                            if (num5 > num4)
                                            {
                                                while (Currency_String.Contains(","))
                                                {
                                                    Currency_String = Currency_String.Replace(",", "");
                                                }
                                            }
                                        }
                                        double.TryParse(Currency_String, out num);
                                        if (num == -1.0 || num == 0.0)
                                        {
                                            double.TryParse(Currency_String.Replace(",", "."), out num);
                                        }
                                        if (num == -1.0 || num == 0.0)
                                        {
                                            double.TryParse(Currency_String.Replace(".", ","), out num);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                num = 0.0;
            }
            if (num == -1.0)
            {
                num = 0.0;
            }
            return num;
        }
        
        #endregion

    }
}
