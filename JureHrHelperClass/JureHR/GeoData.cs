using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JureHR
{
    /// <summary>
    /// This class provides countries, states and teritories
    /// </summary>
    public static class GeoData
    {
        #region ReadOnly Meembers

        private static readonly List<string> states_usa = new List<string>
	    {
		    "","AL","AK", "AS", "AZ","AR","CA","CO","CT","DE","DC","FL", "FM", "GA", "GU","HI","ID","IL","IN","IA","KS","KY","LA","ME","MD","MA", "MH", "MI","MN","MS","MO", "MP","MT","NE","NV","NH","NJ","NM","NY","NC","ND","OH","OK","OR","PA", "PR", "PW", "RI","SC","SD","TN","TX","UT", "VI", "VT","VA","WA","WV","WI","WY"
	    };

        private static readonly List<string> provinces_canada = new List<string>
	    {
		    "","AB", "BC","MB" ,"NB" ,"NF" ,"NT" ,"NS" ,"NU" ,"ON" ,"PE" ,"QC" ,"SK" ,"YT"
	    };

        /// <summary>
        /// all countries in list
        /// </summary>
        public static readonly List<string> Countries = new List<string>
	    {
		    "Select Country...",
		    "Canada",
		    "USA",
		    "Afghanistan",
		    "Albania",
		    "Algeria",
		    "Andorra",
		    "Angola",
		    "Antigua and Barbuda",
		    "Argentina",
		    "Armenia",
		    "Australia",
		    "Austria",
		    "Azerbaijan",
		    "Bahamas, The",
		    "Bahrain",
		    "Bangladesh",
		    "Barbados",
		    "Belarus",
		    "Belgium",
		    "Belize",
		    "Benin",
		    "Bhutan",
		    "Bolivia",
		    "Bosnia and Herzegovina",
		    "Botswana",
		    "Brazil",
		    "Brunei",
		    "Bulgaria",
		    "Burkina",
		    "Burundi",
		    "Cambodia",
		    "Cameroon",
		    "Canada",
		    "Cape Verde",  
		    "Central African Republic",
		    "Chad",
		    "Chile",
		    "China",
		    "Colombia",
		    "Comoros",
		    "Congo, Democratic Republic of",
		    "Congo, Republic of ",
		    "Costa Rica",
		    "Cote d'Ivoire", // ? Ivory Coast 
		    "Croatia",
		    "Cuba",
		    "Cyprus",
		    "Czech Republic",
		    "Denmark",
		    "Djibouti",
		    "Dominica",
		    "Dominican Republic",
		    "Ecuador",
		    "Egypt",
		    "El Salvador", 
		    "Equatorial Guinea",
		    "Eritrea",
		    "Estonia",
		    "Ethiopia",
		    "Fiji",
		    "Finland",
		    "France",
		    "Gabon",
		    "Gambia, The",
		    "Georgia",
		    "Germany",
		    "Ghana",
		    "Greece",
		    "Grenada",
		    "Guatemala",
		    "Guinea",
		    "Guinea-Bissau",
		    "Guyana",
		    "Haiti",
		    "Honduras",
		    "Hungary",
		    "Iceland",
		    "India",
		    "Indonesia",
		    "Iran",
		    "Iraq",
		    "Ireland",
		    "Israel",
		    "Italy",
		    "Jamaica",
		    "Japan",
		    "Jordan",
		    "Kazakhstan",
		    "Kenya",
		    "Kiribati",
		    "Kuwait",
		    "Kyrgyzstan",
		    "Laos",
		    "Latvia",
		    "Lebanon",
		    "Lesotho",
		    "Liberia",
		    "Libya",
		    "Liechtenstein",
		    "Lithuania",
		    "Luxembourg",
		    "Macedonia",
		    "Madagascar",
		    "Malawi",
		    "Malaysia",
		    "Maldives",
		    "Mali",
		    "Malta",
		    "Marshall Islands",
		    "Mauritania",
		    "Mauritius",
		    "Mexico",
		    "Micronesia, Federated States of",
		    "Moldova",
		    "Monaco",
		    "Mongolia",
		    "Morocco",
		    "Mozambique",
		    "Myanmar",
		    "Namibia",
		    "Nauru",
		    "Nepal",
		    "Netherlands",
		    "New Zealand",
		    "Nicaragua",
		    "Niger",
		    "Nigeria",
		    "North Korea",		
		    "Norway",
		    "Oman",
		    "Pakistan",
		    "Palau",
		    "Panama",
		    "Papua New Guinea",
		    "Paraguay",
		    "Peru",
		    "Philippines",
		    "Poland",
		    "Portugal",
		    "Qatar",
		    "Romania",
		    "Russia",
		    "Rwanda",
		    "Saint Kitts and Nevis",
		    "Saint Lucia",
		    "Samoa",
		    "San Marino",  
		    "Sao Tome and Principe", 
		    "Saudi Arabia",
		    "Senegal",
		    "Serbia and Montenegro",
		    "Seychelles",
		    "Sierra Leone",
		    "Singapore",
		    "Slovakia",
		    "Slovenia",
		    "Solomon Islands",
		    "Somalia",
		    "South Africa",
		    "South Korea",		
		    "Spain",
		    "Sri Lanka",
		    "Sudan",
		    "Suriname",
		    "Swaziland",
		    "Sweden",
		    "Switzerland",
		    "Syria",
		    "Taiwan",
		    "Tajikistan",
		    "Tanzania",
		    "Thailand",
		    "Tolo",
		    "Tonga",
		    "Trinidad and Tobago",
		    "Tunisia",
		    "Turkey",
		    "Turkmenistan",
		    "Tuvalu",
		    "Uganda",
		    "Ukraine",
		    "United Arab Emirates",
		    "United Kingdom",
		    "United States",
		    "Uruguay",
		    "Uzbekistan",
		    "Vanuatu",
		    "Vatican City",
		    "Venezuela",
		    "Vietnam",
		    "Yemen",
		    "Zambia",
		    "Zimbabwe",
	    };

        #endregion

        #region Public Metods
        
        /// <summary>
        /// get states or teritories based on country
        /// </summary>
        /// <param name="country"><example>GeoData.GetStates("Canada");</example></param>
        /// <returns></returns>
        public static List<string> GetStatesOrTeritories(string country)
        {
            if (country.Equals("Canada"))
            {
                return provinces_canada;
            }
            if (country.Equals("USA"))
            {
                return states_usa;
            }
            return null;
        }

        /// <summary>
        /// Province By PostalCode
        /// </summary>
        /// <param name="PostalCode"></param>
        /// <param name="CountryCode"></param>
        /// <param name="ProvinceCode"></param>
        public static void ProvinceByPostalCode(string PostalCode, out string CountryCode, out string ProvinceCode)
        {
            CountryCode = "";
            ProvinceCode = "";
            PostalCode = PostalCode.Replace(" ", "");
            PostalCode = PostalCode.Replace("-", "");
            PostalCode = PostalCode.Replace("_", "");
            PostalCode = PostalCode.Trim();
            if (Validation.Standard.IsValid(PostalCode, Validation.Standard.Type.POSTALCODE))
            {
                string text = PostalCode.Substring(0, 1);
                text = text.ToUpper();
                if (text == "A")
                {
                    CountryCode = "CA";
                    ProvinceCode = "NL";
                }
                else
                {
                    if (text == "B")
                    {
                        CountryCode = "CA";
                        ProvinceCode = "NS";
                    }
                    else
                    {
                        if (text == "C")
                        {
                            CountryCode = "CA";
                            ProvinceCode = "PE";
                        }
                        else
                        {
                            if (text == "E")
                            {
                                CountryCode = "CA";
                                ProvinceCode = "NB";
                            }
                            else
                            {
                                if (text == "G" || text == "H" || text == "J")
                                {
                                    CountryCode = "CA";
                                    ProvinceCode = "QC";
                                }
                                else
                                {
                                    if (text == "K" || text == "L" || text == "M" || text == "N" || text == "P")
                                    {
                                        CountryCode = "CA";
                                        ProvinceCode = "ON";
                                    }
                                    else
                                    {
                                        if (text == "R")
                                        {
                                            CountryCode = "CA";
                                            ProvinceCode = "MB";
                                        }
                                        else
                                        {
                                            if (text == "S")
                                            {
                                                CountryCode = "CA";
                                                ProvinceCode = "SK";
                                            }
                                            else
                                            {
                                                if (text == "T")
                                                {
                                                    CountryCode = "CA";
                                                    ProvinceCode = "AB";
                                                }
                                                else
                                                {
                                                    if (text == "V")
                                                    {
                                                        CountryCode = "CA";
                                                        ProvinceCode = "BC";
                                                    }
                                                    else
                                                    {
                                                        if (text == "X")
                                                        {
                                                            CountryCode = "CA";
                                                            string a = PostalCode.Substring(0, 3);
                                                            if (a == "X0A" || a == "X0B" || a == "X0C")
                                                            {
                                                                ProvinceCode = "NU";
                                                            }
                                                            if (a == "X0E" || a == "X0G" || a == "X1A")
                                                            {
                                                                ProvinceCode = "NT";
                                                            }
                                                        }
                                                        else
                                                        {
                                                            if (text == "Y")
                                                            {
                                                                CountryCode = "CA";
                                                                ProvinceCode = "YT";
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Returns state nemes when code provided or code when name is provided
        /// <example>
        ///     string code = GeoData.StateNamesAndCodes("California");
        ///     string name = GeoData.StateNamesAndCodes("CA");
        /// </example>
        /// </summary>
        /// <param name="abbr"></param>
        /// <returns></returns>
        public static string StateNamesAndCodes(string abbr)
        {
            Dictionary<string, string> states = new Dictionary<string, string>();

            states.Add("AL", "Alabama");
            states.Add("AK", "Alaska");
            states.Add("AZ", "Arizona");
            states.Add("AR", "Arkansas");
            states.Add("CA", "California");
            states.Add("CO", "Colorado");
            states.Add("CT", "Connecticut");
            states.Add("DE", "Delaware");
            states.Add("DC", "District of Columbia");
            states.Add("FL", "Florida");
            states.Add("GA", "Georgia");
            states.Add("HI", "Hawaii");
            states.Add("ID", "Idaho");
            states.Add("IL", "Illinois");
            states.Add("IN", "Indiana");
            states.Add("IA", "Iowa");
            states.Add("KS", "Kansas");
            states.Add("KY", "Kentucky");
            states.Add("LA", "Louisiana");
            states.Add("ME", "Maine");
            states.Add("MD", "Maryland");
            states.Add("MA", "Massachusetts");
            states.Add("MI", "Michigan");
            states.Add("MN", "Minnesota");
            states.Add("MS", "Mississippi");
            states.Add("MO", "Missouri");
            states.Add("MT", "Montana");
            states.Add("NE", "Nebraska");
            states.Add("NV", "Nevada");
            states.Add("NH", "New Hampshire");
            states.Add("NJ", "New Jersey");
            states.Add("NM", "New Mexico");
            states.Add("NY", "New York");
            states.Add("NC", "North Carolina");
            states.Add("ND", "North Dakota");
            states.Add("OH", "Ohio");
            states.Add("OK", "Oklahoma");
            states.Add("OR", "Oregon");
            states.Add("PA", "Pennsylvania");
            states.Add("RI", "Rhode Island");
            states.Add("SC", "South Carolina");
            states.Add("SD", "South Dakota");
            states.Add("TN", "Tennessee");
            states.Add("TX", "Texas");
            states.Add("UT", "Utah");
            states.Add("VT", "Vermont");
            states.Add("VA", "Virginia");
            states.Add("WA", "Washington");
            states.Add("WV", "West Virginia");
            states.Add("WI", "Wisconsin");
            states.Add("WY", "Wyoming");
            if (states.ContainsKey(abbr))
            {
                return states.FirstOrDefault(x => x.Key == abbr).Value.ToString();
            }
            if (states.ContainsValue(abbr))
            {
                return states.FirstOrDefault(x => x.Value == abbr).Key.ToString();
            }
            /* error handler is to return an empty string rather than throwing an exception */
            return "";
        }

        /// <summary>
        /// Returns province nemes when code provided or code when name is provided
        /// <example>
        ///     string code = GeoData.ProvinceNamesAndCodes("Ontario");
        ///     string name = GeoData.ProvinceNamesAndCodes("ON");
        /// </example>
        /// </summary>
        /// <param name="abbr"></param>
        /// <returns></returns>
        public static string ProvinceNamesAndCodes(string abbr)
        {
            Dictionary<string, string> provinces = new Dictionary<string, string>();

            provinces.Add("AB", "Alberta");
            provinces.Add("BC", "British Columbia");
            provinces.Add("MB", "Manitoba");
            provinces.Add("NB", "New Brunswick");
            provinces.Add("NL", "Newfoundland and Labrador");
            provinces.Add("NT", "Northwest Territories");
            provinces.Add("NS", "Nova Scotia");
            provinces.Add("NU", "Nunavut");
            provinces.Add("ON", "Ontario");
            provinces.Add("PE", "Prince Edward Island");
            provinces.Add("QC", "Québec");
            provinces.Add("SK", "Saskatchewan");
            provinces.Add("YT", "Yukon");
            if (provinces.ContainsKey(abbr))
            {
                return provinces.FirstOrDefault(x => x.Key == abbr).Value.ToString();
            }
            if (provinces.ContainsValue(abbr))
            {
                return provinces.FirstOrDefault(x => x.Value == abbr).Key.ToString();
            }
            /* error handler is to return an empty string rather than throwing an exception */
            return "";
        }

        /// <summary>
        /// Get Distance Between Two Geo Points
        /// </summary>
        /// <param name="lat1"></param>
        /// <param name="long1"></param>
        /// <param name="lat2"></param>
        /// <param name="long2"></param>
        /// <returns>Distance in m</returns>
        public static double GetDistanceBetweenPoints(double lat1, double long1, double lat2, double long2)
        {
            double distance = 0;
            double dLat = (lat2 - lat1) / 180 * Math.PI;
            double dLong = (long2 - long1) / 180 * Math.PI;
            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
                        + Math.Cos(lat2) * Math.Sin(dLong / 2) * Math.Sin(dLong / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            double radiusE = 6378135; // Equatorial radius
            double radiusP = 6356750; // Polar Radius
            double nr = Math.Pow(radiusE * radiusP * Math.Cos(lat1 / 180 * Math.PI), 2);
            double dr = Math.Pow(radiusE * Math.Cos(lat1 / 180 * Math.PI), 2)
                            + Math.Pow(radiusP * Math.Sin(lat1 / 180 * Math.PI), 2);
            double radius = Math.Sqrt(nr / dr);
            distance = radius * c;
            return distance;
        }

        #endregion

	}
}












